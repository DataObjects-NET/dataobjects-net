// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.26

using System;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  internal class DefaultFieldAccessor<T> : FieldAccessorBase<T>
  {
    private static readonly FieldAccessorBase<T> instance = new DefaultFieldAccessor<T>();
    private static readonly bool isObject = (typeof (T)==typeof (object));
    private static readonly bool isString = (typeof (T)==typeof (string));
    private static readonly bool isByteArray = (typeof (T)==typeof (byte[]));

    public static FieldAccessorBase<T> Instance
    {
      get { return instance; }
    }

    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      if (!field.IsNullable && value==null)
        throw new InvalidOperationException(string.Format(Strings.ExNotNullableConstraintViolationOnFieldX, field));

      if (value!=null && field.Length > 0) {
        if (isString && field.Length < ((string) (object) value).Length)
          throw new InvalidOperationException(string.Format(Strings.ExLengthConstraintViolationOnFieldX, field));
        if (isByteArray && field.Length < ((byte[]) (object) value).Length)
          throw new InvalidOperationException(string.Format(Strings.ExLengthConstraintViolationOnFieldX, field));
      }

      ValidateType(field);
      obj.Data.SetValue(field.MappingInfo.Offset, value);
    }

    public override T GetValue(Persistent obj, FieldInfo field)
    {
      ValidateType(field);

      if (isObject)
        return (T) obj.Data.GetValueOrDefault(field.MappingInfo.Offset);

      return obj.Data.GetValueOrDefault<T>(field.MappingInfo.Offset);
    }

    private DefaultFieldAccessor()
    {
    }
  }
}