// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.30

using System;
using Xtensive.Core;

using Xtensive.Tuples;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal class StructureFieldAccessor<T> : CachingFieldAccessor<T>
  {
    /// <inheritdoc/>
    public override bool AreSameValues(object oldValue, object newValue)
    {
      return oldValue.Equals(newValue);
    }
    /// <inheritdoc/>
    public override void SetValue(Persistent obj, T value)
    {
      var field = Field;
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      var valueType = value.GetType();
      if (field.ValueType != valueType)
        throw new InvalidOperationException(String.Format(
          Strings.ExResultTypeIncorrect, valueType.Name, field.ValueType.Name));

      var structure = (Structure) (object) value;
      var adapter = (IFieldValueAdapter)value;
      if (adapter.Owner!=null)
        adapter.Owner.EnsureIsFetched(adapter.Field);
      structure.Tuple.CopyTo(obj.Tuple, 0, field.MappingInfo.Offset, field.MappingInfo.Length);
    }

    // Type initializer

    static StructureFieldAccessor()
    {
       Constructor = (obj, field) => Activator.CreateStructure(field.ValueType, obj, field);
    }
  }
}