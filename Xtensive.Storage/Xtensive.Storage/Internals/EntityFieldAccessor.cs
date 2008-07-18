// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.26

using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
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
      if (ReferenceEquals(value, null)) {
        for (int i = field.MappingInfo.Offset; i < field.MappingInfo.Offset + field.MappingInfo.Length; i++) {
          obj.Tuple.SetValue(i, null);
        }
      }
      else {
        ValidateType(field);
        ((Entity) (object) value).Key.Tuple.Copy(obj.Tuple, 0, field.MappingInfo.Offset, field.MappingInfo.Length);
      }
    }

    public override T GetValue(Persistent obj, FieldInfo field)
    {
      ValidateType(field);
      Key key = obj.Session.Domain.KeyManager.BuildForeignKey(obj, field);
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