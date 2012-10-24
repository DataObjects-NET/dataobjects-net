// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.26

using System;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal class DefaultFieldAccessor<T> : FieldAccessor<T>
  {
    private static readonly bool isValueType = (typeof (T).IsValueType);
    private static readonly bool isObject = (typeof (T)==typeof (object));
    private static readonly bool isString = (typeof (T)==typeof (string));
    private static readonly bool isByteArray = (typeof (T)==typeof (byte[]));

    /// <inheritdoc/>
    public override bool AreSameValues(object oldValue, object newValue)
    {
      if (isValueType || isString)
        return object.Equals(oldValue, newValue);
      return false;
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj)
    {
      var field = Field;
      int fieldIndex = field.MappingInfo.Offset;
      var tuple = obj.Tuple;
      var value = tuple.GetValueOrDefault<T>(fieldIndex);
      return value;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid arguments.</exception>
    public override void SetValue(Persistent obj, T value)
    {
      var field = Field;

      if (obj.Session.ValidationContext.IsConsistent) {
        if (!field.IsNullable && value==null)
          throw new InvalidOperationException(string.Format(Strings.ExNotNullableConstraintViolationOnFieldX, field));
        if (value!=null && field.Length > 0) {
          if (isString && field.Length < ((string) (object) value).Length)
            throw new InvalidOperationException(string.Format(Strings.ExLengthConstraintViolationOnFieldX, field));
          if (isByteArray && field.Length < ((byte[]) (object) value).Length)
            throw new InvalidOperationException(string.Format(Strings.ExLengthConstraintViolationOnFieldX, field));
        }
      }

      obj.Tuple.SetValue(field.MappingInfo.Offset, value);
    }
  }
}