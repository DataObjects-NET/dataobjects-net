// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.21

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class KeyFieldAccessor<T> : FieldAccessorBase<T> 
  {
    private static readonly FieldAccessorBase<T> instance = new KeyFieldAccessor<T>();
    private static readonly T @default = (T) (object) null;

    public static FieldAccessorBase<T> Instance {
      get { return instance; }
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj, FieldInfo field)
    {
      EnsureTypeIsAssignable(field);
      int fieldIndex = field.MappingInfo.Offset;
      var tuple = obj.Tuple;

      if (!tuple.IsAvailable(fieldIndex))
        return @default;
      return (T) (object) Key.Parse(
        tuple.GetValueOrDefault<string>(fieldIndex));
    }

    /// <inheritdoc/>
    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      EnsureTypeIsAssignable(field);
      var key = (Key) (object) value;
      obj.Tuple.SetValue(field.MappingInfo.Offset, key==null ? null : key.Format());
    }
  }
}