// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.26

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  internal class EntityFieldAccessor<T> : FieldAccessorBase<T>
  {
    private static readonly FieldAccessorBase<T> instance = new EntityFieldAccessor<T>();

    public static FieldAccessorBase<T> Instance
    {
      get { return instance; }
    }

    /// <inheritdoc/>
    public override void SetValue(Persistent obj, FieldInfo field, T value, bool notify)
    {
      var entity = value as Entity;

      if (!ReferenceEquals(value, null) && entity==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExValueShouldBeXDescendant, typeof (Entity)));

      if (entity!=null && entity.Session!=obj.Session)
        throw new InvalidOperationException(string.Format(
          Strings.EntityXIsBoundToAnotherSession, entity.Key));

      if (entity==null)
        for (int i = field.MappingInfo.Offset; i < field.MappingInfo.Offset + field.MappingInfo.Length; i++)
          obj.Tuple.SetValue(i, null);
      else {
        ValidateType(field);
        entity.Key.Value.CopyTo(obj.Tuple, 0, field.MappingInfo.Offset, field.MappingInfo.Length);
      }
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj, FieldInfo field, bool notify)
    {
      ValidateType(field);
      Key key = obj.GetKey(field);
      if (key==null)
        return default(T);
      return (T) (object) key.Resolve();
    }
  }
}