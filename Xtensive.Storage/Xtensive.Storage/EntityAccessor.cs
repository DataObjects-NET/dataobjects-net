// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.02

using System;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;
using Activator=Xtensive.Storage.Internals.Activator;

namespace Xtensive.Storage
{
  public sealed class EntityAccessor : SessionBound
  {
    #region Entity/Structure-related methods

    [Infrastructure]
    public Persistent CreateInstance(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      TypeInfo typeInfo = Session.Domain.Model.Types[type];
      if (typeInfo.IsEntity) {
        var key = Key.Create(type);
        var state = Session.Cache.Add(key);
        var result = Activator.CreateEntity(type, state);
        return result;
      }
      throw new NotImplementedException();
    }

    [Infrastructure]
    public Persistent CreateInstance(Type type, Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");

      TypeInfo typeInfo = Session.Domain.Model.Types[type];
      if (typeInfo.IsEntity) {
        var key = Key.Create(type, tuple, true);
        var state = Session.Cache.Add(key);
        var result = Activator.CreateEntity(type, state);
        return result;
      }
      throw new NotImplementedException();
    }

    [Infrastructure]
    public T GetField<T>(Persistent target, FieldInfo field)
    {
      ValidateArguments(target, field);
      return target.GetField<T>(field, false);
    }

    [Infrastructure]
    public void SetField<T>(Persistent target, FieldInfo field, T value)
    {
      ValidateArguments(target, field);
      target.SetField(field, value, false);
    }

    [Infrastructure]
    public Key GetKey(Persistent target, FieldInfo field)
    {
      ValidateArguments(target, field);
      // TODO: Refactor
      return target.GetKey(field);
    }

    [Infrastructure]
    public void Remove(Entity target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      target.Remove(false);
    }

    #endregion

    #region EntitySet-related methods

    [Infrastructure]
    public RecordSet GetRecordSet(EntitySet target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      return target.RecordSet;
    }

    [Infrastructure]
    public RecordSet GetRecordSet(Persistent target, FieldInfo field)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      EnsureFieldIsEntitySet(field);

      return GetRecordSet(GetField<EntitySet>(target, field));
    }

    [Infrastructure]
    public bool Add(EntitySet target, Entity item)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      throw new NotImplementedException();
    }

    [Infrastructure]
    public bool Add(Entity target, FieldInfo field, Entity item)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      EnsureFieldIsEntitySet(field);

      return Add(GetField<EntitySet>(target, field), item);
    }

    [Infrastructure]
    public bool Remove(EntitySet target, Entity item)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      throw new NotImplementedException();
    }

    [Infrastructure]
    public bool Remove(Entity target, FieldInfo field, Entity item)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      EnsureFieldIsEntitySet(field);

      return Remove(GetField<EntitySet>(target, field), item);
    }

    [Infrastructure]
    public void Clear(EntitySet target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");

      throw new NotImplementedException();
    }

    [Infrastructure]
    public void Clear(Entity target, FieldInfo field)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      EnsureFieldIsEntitySet(field);

      Clear(GetField<EntitySet>(target, field));
    }

    #endregion

    #region Private members

    private static void EnsureFieldIsEntitySet(FieldInfo field)
    {
      if (!field.IsEntitySet)
        throw new InvalidOperationException(string.Format("Field '{0}' is not an EntitySet field.", field.Name));
    }

    private static void ValidateArguments(Persistent target, FieldInfo field)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      if (!target.Type.Fields.Contains(field))
        throw new InvalidOperationException(string.Format("Type '{0}' doesn't contain '{1}' field.", target.Type.Name, field.Name));
    }

    #endregion

    // Constructor

    internal EntityAccessor(Session session)
      : base(session)
    {
    }
  }
}