// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.30

using System;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  internal class StructureFieldValueAdapter<T> : CachingFieldValueAdapter<T>
  {
    public static readonly FieldValueAdapter<T> Instance = new StructureFieldValueAdapter<T>();

    /// <inheritdoc/>
    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      EnsureGenericParameterIsValid(field);
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

    static StructureFieldValueAdapter()
    {
       ctor = (obj, field) => Activator.CreateStructure(field.ValueType, obj, field);
    }
  }
}