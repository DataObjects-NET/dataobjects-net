// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.07

using System;
using Xtensive.Core.Conversion;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Internals
{
  internal class EnumFieldAccessor<T> : FieldAccessor<T> 
  {
    public static readonly FieldAccessor<T> Instance = new EnumFieldAccessor<T>();
    private static readonly Type type = typeof(T);
    private static readonly object @default = 
      type.IsEnum 
      ? (type.IsNullable() ? null : Enum.GetValues(type).GetValue(0))
      : default(T);
    private static ThreadSafeDictionary<Type, Biconverter<T, object>> converters =
      ThreadSafeDictionary<Type, Biconverter<T, object>>.Create(new object());

    /// <inheritdoc/>
    public override T GetValue(Persistent obj, FieldInfo field)
    {
      EnsureGenericParameterIsValid(field);
      int fieldIndex = field.MappingInfo.Offset;
      var tuple = obj.Tuple;

      TupleFieldState state;
      var value = tuple.GetValue(fieldIndex, out state);
      if (!state.HasValue())
        return (T)@default;
      if (type.IsEnum)
        return (T)Enum.ToObject(type, value);
      else
        return (T)Enum.ToObject(Nullable.GetUnderlyingType(type), value);
    }

    /// <inheritdoc/>
    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      EnsureGenericParameterIsValid(field);
      // Biconverter<object, T> converter = GetConverter(field.ValueType);
      obj.Tuple.SetValue(field.MappingInfo.Offset, Convert.ChangeType(value, field.Column.ValueType));
    }

    private Biconverter<object, T> GetConverter(Type type)
    {
      throw new NotImplementedException();
//      return converters.GetValue(type, (_type) => 
//        ...
//        , type);
    }
  }
}