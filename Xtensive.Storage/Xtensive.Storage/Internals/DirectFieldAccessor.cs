// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.26

using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  internal class DirectFieldAccessor<T> : FieldAccessorBase<T>
  {
    private static readonly FieldAccessorBase<T> instance = new DirectFieldAccessor<T>();
    private static readonly bool isObject = (typeof (T)==typeof (object));

    public static FieldAccessorBase<T> Instance
    {
      get { return instance; }
    }

    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      ValidateType(field);
      obj.Tuple.SetValue(field.MappingInfo.Offset, value);
    }

    public override T GetValue(Persistent obj, FieldInfo field)
    {
      ValidateType(field);

      if (isObject)
        return (T) obj.Tuple.GetValueOrDefault(field.MappingInfo.Offset);

      return obj.Tuple.GetValueOrDefault<T>(field.MappingInfo.Offset);
    }

    private DirectFieldAccessor()
    {
    }
  }
}