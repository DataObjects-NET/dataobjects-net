// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.21

using System;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal class KeyFieldAccessor<T> : FieldAccessor<T> 
  {
    private static readonly T @default = default;

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
      var value = tuple.GetValue<string>(fieldIndex, out state);
      if (!state.IsAvailable())
        return @default;
      return (T) (object) Key.Parse(obj.Session.Domain, value);
    }

    /// <inheritdoc/>
    public override void SetValue(Persistent obj, T value)
    {
      var field = Field;
      var key = (Key) (object) value;
      obj.Tuple.SetValue(field.MappingInfo.Offset, key==null ? null : key.Format());
    }
  }
}