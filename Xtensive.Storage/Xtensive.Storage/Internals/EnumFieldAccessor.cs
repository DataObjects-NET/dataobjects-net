// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.07

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class EnumFieldAccessor<T> : FieldAccessorBase<T> 
  {
    private static readonly FieldAccessorBase<T> instance = new EnumFieldAccessor<T>();
    private static Type type = typeof(T);
    private static readonly object @default = (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) ? null : Enum.GetValues(type).GetValue(0);

    public static FieldAccessorBase<T> Instance
    {
      get { return instance; }
    }

    public override T GetValue(Persistent obj, FieldInfo field)
    {
      ValidateType(field);
      if (!obj.Tuple.IsAvailable(field.MappingInfo.Offset) || obj.Tuple.IsNull(field.MappingInfo.Offset))
        return (T)@default;
      if (type.IsEnum)
        return (T)Enum.ToObject(type, obj.Tuple.GetValueOrDefault(field.MappingInfo.Offset));
      else
        return (T)Enum.ToObject(Nullable.GetUnderlyingType(type), obj.Tuple.GetValueOrDefault(field.MappingInfo.Offset));
    }

    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      ValidateType(field);
      obj.Tuple.SetValue(field.MappingInfo.Offset, Convert.ChangeType(value, field.Column.ValueType));
    }

    private EnumFieldAccessor()
    {
    }
  }
}