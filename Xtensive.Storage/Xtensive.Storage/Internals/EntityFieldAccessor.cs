// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.26

using System;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

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
      Entity entity = value as Entity;
      if (entity != null) {
        if (obj.Session != entity.Session)
          throw new InvalidOperationException(String.Format("Entity '{0}' is bound to another session.", entity.Key));
      }
      if (ReferenceEquals(value, null))
        for (int i = field.MappingInfo.Offset; i < field.MappingInfo.Offset + field.MappingInfo.Length; i++)
          obj.Tuple.SetValue(i, null);
      else {
        ValidateType(field);
        ((Entity) (object) value).Key.Tuple.CopyTo(obj.Tuple, 0, field.MappingInfo.Offset, field.MappingInfo.Length);
      }
    }

    public override T GetValue(Persistent obj, FieldInfo field)
    {
      ValidateType(field);
      Key key = obj.Session.Domain.KeyManager.Get(field, new SegmentTransform(false, obj.Tuple.Descriptor, new Segment<int>(field.MappingInfo.Offset, field.MappingInfo.Length)).Apply(TupleTransformType.TransformedTuple, obj.Tuple));
      if (key == null)
        return default(T);
      return (T) (object) key.Resolve();
    }


    // Constructors

    private EntityFieldAccessor()
    {
    }
  }
}