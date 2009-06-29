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

    public static FieldAccessorBase<T> Instance {
      get { return instance; }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid arguments.</exception>
    public override void SetValue(Persistent obj, FieldInfo field, T value, bool notify)
    {
      var entity = value as Entity;

      if (!ReferenceEquals(value, null) && entity==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExValueShouldBeXDescendant, typeof (Entity)));

      if (entity!=null && entity.Session!=obj.Session)
        throw new InvalidOperationException(string.Format(
          Strings.ExEntityXIsBoundToAnotherSession, entity.Key)); 

      var mappingInfo = field.MappingInfo;
      int fieldIndex = mappingInfo.Offset;
      if (entity==null) {
        int nextFieldIndex = fieldIndex + mappingInfo.Length;
        for (int i = fieldIndex; i < nextFieldIndex; i++)
          obj.Tuple.SetValue(i, null);
      }
      else {
        ValidateType(field);
        entity.Key.Value.CopyTo(obj.Tuple, 0, fieldIndex, mappingInfo.Length);
      }
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj, FieldInfo field, bool notify)
    {
      ValidateType(field);
      Key key = obj.GetReferenceKey(field);
      if (key==null)
        return default(T);
      return (T) (object) key.Resolve();
    }
  }
}