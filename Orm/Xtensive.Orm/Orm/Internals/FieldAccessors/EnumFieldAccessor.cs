// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.07

using System;
using Xtensive.Conversion;
using Xtensive.Threading;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Reflection;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal class EnumFieldAccessor<T> : FieldAccessor<T> 
  {
    private static readonly Type type = typeof(T);
    private static readonly object @default = 
      type.IsEnum 
      ? (type.IsNullable() ? null : Enum.GetValues(type).GetValue(0))
      : default(T);
    private static ThreadSafeDictionary<Type, Biconverter<T, object>> converters =
      ThreadSafeDictionary<Type, Biconverter<T, object>>.Create(new object());

    /// <inheritdoc/>
    public override bool AreSameValues(object oldValue, object newValue)
    {
      return object.Equals(oldValue, newValue);
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj)
    {
      var field = Field;
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
    public override void SetValue(Persistent obj, T value)
    {
      var field = Field;
      // Biconverter<object, T> converter = GetConverter(field.ValueType);
      obj.Tuple.SetValue(field.MappingInfo.Offset, value==null ? null : Convert.ChangeType(value, field.Column.ValueType));
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