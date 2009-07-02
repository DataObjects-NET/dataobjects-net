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
  internal class StructureFieldAccessor<T> : FieldAccessorBase<T>
  {
    private static readonly FieldAccessorBase<T> instance = new StructureFieldAccessor<T>();

    public static FieldAccessorBase<T> Instance {
      get { return instance; }
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj, FieldInfo field)
    {
      EnsureTypeIsAssignable(field);
      IFieldValueAdapter result;
      if (obj.FieldHandlers.TryGetValue(field, out result))
        return (T) result;
      result = Activator.CreateStructure(field.ValueType, obj, field);
      obj.FieldHandlers.Add(field, result);
      return (T) result;
    }

    /// <inheritdoc/>
    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      EnsureTypeIsAssignable(field);
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
  }
}