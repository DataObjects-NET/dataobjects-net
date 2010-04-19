// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.26

using System;
using Xtensive.Storage.Resources;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Internals.FieldAccessors
{
  internal class EntityFieldAccessor<T> : FieldAccessor<T>
  {
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid arguments.</exception>
    public override void SetValue(Persistent obj, T value)
    {
      var entity = value as Entity;
      var field = Field;

      if (!ReferenceEquals(value, null) && entity==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExValueShouldBeXDescendant, typeof (Entity)));

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
      return (T) (object) Query.SingleOrDefault(key);
    }
  }
}