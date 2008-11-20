// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.01

using System;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Internals
{
  internal static class Activator
  {
    private static readonly ThreadSafeDictionary<Type, Func<EntityState, bool, Entity>> entityActivators =
      ThreadSafeDictionary<Type, Func<EntityState, bool, Entity>>.Create(new object());

    private static readonly ThreadSafeDictionary<Type, Func<Persistent, FieldInfo, bool, Structure>> structureActivators =
      ThreadSafeDictionary<Type, Func<Persistent, FieldInfo, bool, Structure>>.Create(new object());

    private static readonly ThreadSafeDictionary<Type, Func<Entity, FieldInfo, bool, EntitySetBase>> entitySetActivators =
      ThreadSafeDictionary<Type, Func<Entity, FieldInfo, bool, EntitySetBase>>.Create(new object());

    internal static Entity CreateEntity(Type type, EntityState state, bool notify)
    {
      var activator = entityActivators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<EntityState, bool, Entity>>);
      Entity result = activator(state, notify);
      result.OnInitialize(notify);
      return result;
    }

    internal static Structure CreateStructure(Type type, Persistent owner, FieldInfo field, bool notify)
    {
      var activator = structureActivators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<Persistent, FieldInfo, bool, Structure>>);
      Structure result = activator(owner, field, notify);
      result.OnInitialize(notify);
      return result;
    }

    internal static EntitySetBase CreateEntitySet(Entity owner, FieldInfo field, bool notify)
    {
      var activator = entitySetActivators.GetValue(field.ValueType,
        DelegateHelper.CreateConstructorDelegate<Func<Entity, FieldInfo, bool, EntitySetBase>>);
      EntitySetBase result = activator.Invoke(owner, field, notify);
      result.Initialize(notify);
      return result;
    }
  }
}