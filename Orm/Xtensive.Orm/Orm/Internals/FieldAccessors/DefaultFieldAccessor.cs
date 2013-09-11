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
        return Equals(oldValue, newValue);
      return false;
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj)
    {
      int fieldIndex = Field.MappingInfo.Offset;
      var tuple = obj.Tuple;
      var value = tuple.GetValueOrDefault<T>(fieldIndex);
      return value;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid arguments.</exception>
    public override void SetValue(Persistent obj, T value)
    {
      obj.Tuple.SetValue(Field.MappingInfo.Offset, value);
    }
  }
}