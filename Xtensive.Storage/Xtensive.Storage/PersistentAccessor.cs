// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.02

using System;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Activator=Xtensive.Storage.Internals.Activator;

namespace Xtensive.Storage
{
  public sealed class PersistentAccessor : SessionBound
  {
    #region Entity/Structure-related methods

    [Infrastructure]
    public Entity CreateEntity(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (!typeof(Entity).IsAssignableFrom(type))
        throw new InvalidOperationException(
          string.Format(Strings.TypeXIsNotAnYDescendant, type, typeof(Entity)));

      var key = Key.Create(type);
      var state = Session.CreateEntityState(key);
      return Activator.CreateEntity(type, state, false);
    }

    [Infrastructure]
    public Entity CreateEntity(Type type, Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");
      if (!typeof(Entity).IsAssignableFrom(type))
        throw new InvalidOperationException(
          string.Format(Strings.TypeXIsNotAnYDescendant, type, typeof(Entity)));

      var key = Key.Create(type, tuple, true);
      var state = Session.CreateEntityState(key);
      return Activator.CreateEntity(type, state, false);
    }

    [Infrastructure]
    public Structure CreateStructure(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (!typeof(Structure).IsAssignableFrom(type))
        throw new InvalidOperationException(
          string.Format(Strings.TypeXIsNotAnYDescendant, type, typeof(Structure)));

      return Activator.CreateStructure(type, null, null, false);
    }

    [Infrastructure]
    public T GetFieldValue<T>(Persistent target, FieldInfo field)
    {
      ValidateArguments(target, field);
      return target.GetFieldValue<T>(field, false);
    }

    [Infrastructure]
    public void SetFieldValue<T>(Persistent target, FieldInfo field, T value)
    {
      ValidateArguments(target, field);
      target.SetFieldValue(field, value, false);
    }

    [Infrastructure]
    public Key GetKey(Persistent target, FieldInfo field)
    {
      ValidateArguments(target, field);
      return target.GetKey(field);
    }

    [Infrastructure]
    public void Remove(Entity target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      target.Remove(false);
    }

    #endregion

    #region Private members

    private static void ValidateArguments(Persistent target, FieldInfo field)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      if (!target.Type.Fields.Contains(field))
        throw new InvalidOperationException(string.Format("Type '{0}' doesn't contain '{1}' field.", target.Type.Name, field.Name));
    }

    #endregion


    // Constructors

    internal PersistentAccessor(Session session)
      : base(session)
    {
    }
  }
}