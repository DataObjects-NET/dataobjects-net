// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.01

using System;
using System.Collections.Concurrent;
using System.Reflection;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals
{
  internal static class Activator
  {
    private static readonly Assembly OrmAssembly = typeof (Activator).Assembly;

    private const string OrmFactoryMethodName = "CreateObject";
    private const string OtherFactoryMethodName = "~Xtensive.Orm.CreateObject";

    private static readonly ConcurrentDictionary<Type, Func<Session, EntityState, Entity>> EntityActivators
      = new ConcurrentDictionary<Type, Func<Session, EntityState, Entity>>();
    private static readonly ConcurrentDictionary<Type, Func<Session, Tuple, Structure>> DetachedStructureActivators
      = new ConcurrentDictionary<Type, Func<Session, Tuple, Structure>>();
    private static readonly ConcurrentDictionary<Type, Func<Persistent, FieldInfo, Structure>> StructureActivators
      = new ConcurrentDictionary<Type, Func<Persistent, FieldInfo, Structure>>();
    private static readonly ConcurrentDictionary<Type, Func<Entity, FieldInfo, EntitySetBase>> EntitySetActivators
      = new ConcurrentDictionary<Type, Func<Entity, FieldInfo, EntitySetBase>>();

    public static Entity CreateEntity(Session session, Type type, EntityState state)
    {
      var activator = EntityActivators.GetOrAdd(type, GetActivator<Session, EntityState, Entity>);
      return activator.Invoke(session, state);
    }

    public static Structure CreateStructure(Type type, Persistent owner, FieldInfo field)
    {
      var activator = StructureActivators.GetOrAdd(type, GetActivator<Persistent, FieldInfo, Structure>);
      var result = activator.Invoke(owner, field);
      result.SystemInitialize(true);
      return result;
    }

    public static Structure CreateStructure(Session session, Type type, Tuple tuple)
    {
      var activator = DetachedStructureActivators.GetOrAdd(type, GetActivator<Session, Tuple, Structure>);
      return activator.Invoke(session, tuple);
    }

    public static EntitySetBase CreateEntitySet(Entity owner, FieldInfo field)
    {
      var activator = EntitySetActivators.GetOrAdd(field.ValueType, GetActivator<Entity, FieldInfo, EntitySetBase>);
      return activator.Invoke(owner, field);
    }

    private static Func<TArg1, TArg2, TResult> GetActivator<TArg1, TArg2, TResult>(Type type)
      where TArg1 : class
      where TArg2 : class
    {
      var methodName = type.Assembly==OrmAssembly ? OrmFactoryMethodName : OtherFactoryMethodName;
      var parameters = new[] {typeof (TArg1), typeof (TArg2)};
      var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic, null, parameters, null);
      if (method==null)
        throw new InvalidOperationException(string.Format(Strings.ExAssemblyXIsNotProcessedByWeaver, type.Assembly));
      return (Func<TArg1, TArg2, TResult>) Delegate.CreateDelegate(typeof (Func<TArg1, TArg2, TResult>), method);
    }
  }
}