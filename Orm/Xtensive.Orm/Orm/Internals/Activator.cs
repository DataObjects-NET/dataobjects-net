// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.01

using System;
using Xtensive.Collections;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using FieldInfo=Xtensive.Orm.Model.FieldInfo;

namespace Xtensive.Orm.Internals
{
  internal static class Activator
  {
    private static readonly ThreadSafeDictionary<Type, Func<Session, EntityState, Entity>> entityActivators =
      ThreadSafeDictionary<Type, Func<Session, EntityState, Entity>>.Create(new object());

    private static readonly ThreadSafeDictionary<Type, Func<Session,Entity>> newEntityActivators =
      ThreadSafeDictionary<Type, Func<Session,Entity>>.Create(new object());

    private static readonly ThreadSafeDictionary<Type, Func<Session, Tuple, Structure>> structureTupleActivators =
      ThreadSafeDictionary<Type, Func<Session, Tuple, Structure>>.Create(new object());

    private static readonly ThreadSafeDictionary<Type, Func<Persistent, FieldInfo, Structure>> 
      structureActivators = ThreadSafeDictionary<Type, Func<Persistent, FieldInfo, Structure>>
      .Create(new object());

    private static readonly ThreadSafeDictionary<Type, Func<Entity, FieldInfo, EntitySetBase>>
      entitySetActivators = ThreadSafeDictionary<Type, Func<Entity, FieldInfo, EntitySetBase>>
      .Create(new object());

    internal static Entity CreateEntity(Session session, Type type, EntityState state)
    {
      var activator = entityActivators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<Session, EntityState, Entity>>);
      Entity result = activator(session, state);
      // This one is already called from the constructor
      //result.SystemInitialize(true);
      return result;
    }

    internal static Entity CreateNewEntity(Session session, Type type)
    {
      var activator = newEntityActivators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<Session,Entity>>);
      Entity result = activator(session);
      return result;
    }

    internal static Structure CreateStructure(Type type, Persistent owner, FieldInfo field)
    {
      var activator = structureActivators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<Persistent, FieldInfo,Structure>>);
      Structure result = activator(owner, field);
      result.SystemInitialize(true);
      return result;
    }

    internal static Structure CreateStructure(Session session, Type type, Tuple tuple)
    {
      var activator = structureTupleActivators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<Session, Tuple, Structure>>);
      Structure result = activator(session, tuple);
      return result;
    }

    internal static EntitySetBase CreateEntitySet(Entity owner, FieldInfo field)
    {
      var activator = entitySetActivators.GetValue(field.ValueType,
        DelegateHelper.CreateConstructorDelegate<Func<Entity, FieldInfo, EntitySetBase>>);
      EntitySetBase result = activator.Invoke(owner, field);
      return result;
    }
  }
}