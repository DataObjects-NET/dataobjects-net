// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.07

using System;
using Xtensive.Core.Conversion;
using Xtensive.Core.Threading;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Internals
{
  internal class EnumFieldAccessor<T> : FieldAccessor<T> 
  {
    public static readonly FieldAccessor<T> Instance = new EnumFieldAccessor<T>();
    private static readonly Type type = typeof(T);
    private static readonly object @default = 
      type.IsNullable() ? null : Enum.GetValues(type).GetValue(0);
    private static ThreadSafeDictionary<Type, Biconverter<T, object>> converters =
      ThreadSafeDictionary<Type, Biconverter<T, object>>.Create(new object());

    /// <inheritdoc/>
    public override T GetValue(Persistent obj, FieldInfo field)
    {
      EnsureGenericParameterIsValid(field);
      int fieldIndex = field.MappingInfo.Offset;
      var tuple = obj.Tuple;

      // Biconverter<object, T> converter = GetConverter(field.ValueType);
      if (!tuple.IsAvailable(fieldIndex) || tuple.IsNull(fieldIndex))
        return (T)@default;
      if (type.IsEnum)
        return (T)Enum.ToObject(type, tuple.GetValueOrDefault(fieldIndex));
      else
        return (T)Enum.ToObject(Nullable.GetUnderlyingType(type), tuple.GetValueOrDefault(fieldIndex));
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