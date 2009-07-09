// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.21

using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class KeyFieldAccessor<T> : FieldAccessor<T> 
  {
    public static readonly FieldAccessor<T> Instance = new KeyFieldAccessor<T>();
    private static readonly T @default = (T) (object) null;

    /// <inheritdoc/>
    public override T GetValue(Persistent obj, FieldInfo field)
    {
      EnsureGenericParameterIsValid(field);
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
      EnsureGenericParameterIsValid(field);
      var key = (Key) (object) value;
      obj.Tuple.SetValue(field.MappingInfo.Offset, key==null ? null : key.Format());
    }
  }
}