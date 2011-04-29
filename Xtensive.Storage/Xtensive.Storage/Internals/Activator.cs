// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.01

using System;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Internals
{
  internal static class Activator
  {
    private static readonly ThreadSafeDictionary<Type, Func<EntityState, Entity>> entityActivators =
      ThreadSafeDictionary<Type, Func<EntityState, Entity>>.Create(new object());

    private static readonly ThreadSafeDictionary<Type, Func<Entity>> newEntityActivators =
      ThreadSafeDictionary<Type, Func<Entity>>.Create(new object());

    private static readonly ThreadSafeDictionary<Type, Func<Tuple, Structure>> structureTupleActivators =
      ThreadSafeDictionary<Type, Func<Tuple, Structure>>.Create(new object());

    private static readonly ThreadSafeDictionary<Type, Func<Persistent, FieldInfo, Structure>> 
      structureActivators = ThreadSafeDictionary<Type, Func<Persistent, FieldInfo, Structure>>
      .Create(new object());

    private static readonly ThreadSafeDictionary<Type, Func<Entity, FieldInfo, EntitySetBase>>
      entitySetActivators = ThreadSafeDictionary<Type, Func<Entity, FieldInfo, EntitySetBase>>
      .Create(new object());

    internal static Entity CreateEntity(Type type, EntityState state)
    {
      var activator = entityActivators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<EntityState, Entity>>);
      Entity result = activator(state);
      // This one is already called from the constructor
      //result.SystemInitialize(true);
      return result;
    }

    internal static Entity CreateNewEntity(Type type)
    {
      var activator = newEntityActivators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<Entity>>);
      Entity result = activator();
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

    internal static Structure CreateStructure(Type type, Tuple tuple)
    {
      var activator = structureTupleActivators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<Tuple, Structure>>);
      Structure result = activator(tuple);
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