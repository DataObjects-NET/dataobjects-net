// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.05.26

using System;

using Xtensive.Tuples;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal class EntityFieldAccessor<T> : FieldAccessor<T>
  {
    /// <inheritdoc/>
    public override bool AreSameValues(object oldValue, object newValue)
    {
      return ReferenceEquals(oldValue, newValue);
    }
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid arguments.</exception>
    public override void SetValue(Persistent obj, T value)
    {
      var entity = value as Entity;
      var field = Field;

      if (!ReferenceEquals(value, null) && entity==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExValueShouldBeXDescendant, WellKnownOrmTypes.Entity));

      if (entity!=null && entity.Session!=obj.Session)
        throw new InvalidOperationException(string.Format(
          Strings.ExEntityXIsBoundToAnotherSession, entity.Key)); 

      var mappingInfo = field.MappingInfo;
      int fieldIndex = mappingInfo.Offset;
      if (entity==null) {
        int nextFieldIndex = fieldIndex + mappingInfo.Length;
        for (int i = fieldIndex; i < nextFieldIndex; i++)
          obj.Tuple.SetValue(i, null);
      }
      else {
        entity.Key.Value.CopyTo(obj.Tuple, 0, fieldIndex, mappingInfo.Length);
      }
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj)
    {
      var field = Field;
      Key key = obj.GetReferenceKey(field);
      if (key==null)
        return default(T);
      return (T) (object) obj.Session.Query.SingleOrDefault(key);
    }
  }
}