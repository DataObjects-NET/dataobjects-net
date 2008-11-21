// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.30

using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class StructureFieldAccessor<T> : FieldAccessorBase<T>
  {
    private static readonly FieldAccessorBase<T> instance = new StructureFieldAccessor<T>();

    public static FieldAccessorBase<T> Instance {
      get { return instance; }
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj, FieldInfo field, bool notify)
    {
      ValidateType(field);
      IFieldValueAdapter result;
      if (obj.FieldHandlers.TryGetValue(field, out result))
        return (T) result;
      result = Activator.CreateStructure(field.ValueType, obj, field, notify);
      obj.FieldHandlers.Add(field, result);
      return (T) result;
    }

    /// <inheritdoc/>
    public override void SetValue(Persistent obj, FieldInfo field, T value, bool notify)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ValidateType(field);
      var structure = (Structure) (object) value;
      if (structure.Owner!=null)
        structure.Owner.EnsureIsFetched(structure.Field);
      structure.Tuple.CopyTo(obj.Tuple, 0, field.MappingInfo.Offset, field.MappingInfo.Length);
    }
  }
}