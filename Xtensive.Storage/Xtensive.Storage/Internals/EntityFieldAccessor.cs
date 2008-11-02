// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.26

using System;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  internal class EntityFieldAccessor<T> : FieldAccessorBase<T>
  {
    private static readonly FieldAccessorBase<T> instance = new EntityFieldAccessor<T>();

    public static FieldAccessorBase<T> Instance
    {
      get { return instance; }
    }

    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      var entity = value as Entity;

      if (!ReferenceEquals(value, null) && entity==null)
        throw new InvalidOperationException(
          string.Format(Strings.ExValueShouldBeXDescendant, typeof (Entity)));

      if (entity!=null && entity.Session!=obj.Session)
        throw new InvalidOperationException(
          string.Format(Strings.EntityXIsBoundToAnotherSession, entity.Key));

      if (entity==null)
        for (int i = field.MappingInfo.Offset; i < field.MappingInfo.Offset + field.MappingInfo.Length; i++)
          obj.Data.SetValue(i, null);
      else {
        ValidateType(field);
        entity.Key.CopyTo(obj.Data, 0, field.MappingInfo.Offset, field.MappingInfo.Length);
      }
    }

    public override T GetValue(Persistent obj, FieldInfo field)
    {
      ValidateType(field);
      Key key = ExtractKey(obj, field);
      if (key==null)
        return default(T);
      return (T) (object) key.Resolve();
    }

    // TODO: Refactor
    internal static Key ExtractKey(Persistent obj, FieldInfo field)
    {
      SegmentTransform transform = obj.Session.Domain.Transforms.GetValue(field, arg => new SegmentTransform(false, obj.Data.Descriptor, new Segment<int>(field.MappingInfo.Offset, field.MappingInfo.Length)));
      TypeInfo type = obj.Session.Domain.Model.Types[field.ValueType];
      var tuple = transform.Apply(TupleTransformType.TransformedTuple, obj.Data);
      if (tuple.ContainsEmptyValues())
        return null;
      return new Key(type, tuple);
    }


    // Constructors

    private EntityFieldAccessor()
    {
    }
  }
}