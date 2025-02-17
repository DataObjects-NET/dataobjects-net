// Copyright (C) 2008-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.05.26

using System;
using Xtensive.Reflection;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal class DefaultFieldAccessor<T> : FieldAccessor<T>
  {
    private static readonly bool isValueType = typeof(T).IsValueType;
    private static readonly bool isString = typeof(T) == WellKnownTypes.String;

    /// <inheritdoc/>
    public override bool AreSameValues(object oldValue, object newValue)
    {
      if (isValueType || isString) {
        return Equals(oldValue, newValue);
      }

      return false;
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj)
    {
      var fieldIndex = Field.MappingInfo.Offset;
      var tuple = obj.Tuple;
      var value = tuple.GetValueOrDefault<T>(fieldIndex);
      return value;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid arguments.</exception>
    public override void SetValue(Persistent obj, T value) =>
      obj.Tuple.SetValue(Field.MappingInfo.Offset, value);
  }
}
