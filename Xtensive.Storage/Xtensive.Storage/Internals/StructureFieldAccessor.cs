// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.30

using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  internal class StructureFieldAccessor<T> : FieldAccessorBase<T>
  {
    private static readonly FieldAccessorBase<T> instance = new StructureFieldAccessor<T>();

    public static FieldAccessorBase<T> Instance
    {
      get { return instance; }
    }

    /// <inheritdoc/>
    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ValidateType(field);
      ((Structure) (object) value).Tuple.Copy(obj.Tuple, 0, field.MappingInfo.Offset, field.MappingInfo.Length);
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj, FieldInfo field)
    {
      ValidateType(field);
      IFieldHandler result;
      if (obj.FieldHandlers.TryGetValue(field, out result))
        return (T) result;
      result = Structure.Activate(field.ValueType, obj, field);
      obj.FieldHandlers.Add(field, result);
      return (T) result;
    }

    private StructureFieldAccessor()
    {
    }
  }
}