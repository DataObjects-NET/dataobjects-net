// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.21

using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class KeyFieldAccessor<T> : FieldAccessor<T> 
  {
    public static readonly FieldAccessor<T> Instance = new KeyFieldAccessor<T>();
    private static readonly T @default = default(T);

    /// <inheritdoc/>
    public override T GetValue(Persistent obj, FieldInfo field)
    {
      EnsureGenericParameterIsValid(field);
      int fieldIndex = field.MappingInfo.Offset;
      var tuple = obj.Tuple;
      TupleFieldState state;
      var value = tuple.GetValue<string>(fieldIndex, out state);
      if (!state.IsAvailable())
        return @default;
      return (T) (object) Key.Parse(value);
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