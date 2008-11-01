// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.01

using System;
using System.Reflection;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
using Xtensive.Storage.Resources;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Internals
{
  internal static class Activator
  {
    private static readonly ThreadSafeDictionary<Type, Func<EntityState, Entity>> entityActivators =
      ThreadSafeDictionary<Type, Func<EntityState, Entity>>.Create(new object());

    private static readonly ThreadSafeDictionary<Type, Func<Persistent, FieldInfo, Structure>> structureActivators =
      ThreadSafeDictionary<Type, Func<Persistent, FieldInfo, Structure>>.Create(new object());

    internal static Entity CreateEntity(Type type, EntityState state)
    {
      return entityActivators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<EntityState, Entity>>)
        .Invoke(state);
    }

    internal static Structure CreateStructure(Type type, Persistent owner, FieldInfo field)
    {
      return structureActivators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<Persistent, FieldInfo, Structure>>)
        .Invoke(owner, field);
    }

    internal static IFieldHandler CreateEntitySet(Type type, Persistent owner, FieldInfo field)
    {
      if (field.Association==null)
        throw new InvalidOperationException(String.Format(Strings.ExUnableToActivateEntitySetWithoutAssociation, field.Name));

      Type instanceType;
      if (field.Association.Master.UnderlyingType==null)
        instanceType = typeof (EntitySet<>).MakeGenericType(type);
      else {
        if (field.Association.IsMaster)
          instanceType = typeof (EntitySet<,>).MakeGenericType(type, field.Association.UnderlyingType);
        else
          instanceType = typeof (ReversedEntitySet<,>).MakeGenericType(type, field.Association.Master.UnderlyingType);
      }
      return (IFieldHandler) instanceType.InvokeMember(String.Empty, BindingFlags.CreateInstance, null, null, new object[] {owner, field});
    }
  }
}