// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using System;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  internal class EntitySetFieldAccessor<T> : FieldAccessorBase<T> 
  {
    private static readonly FieldAccessorBase<T> instance = new EntitySetFieldAccessor<T>();

    public static FieldAccessorBase<T> Instance
    {
      get { return instance; }
    }

    public override T GetValue(Persistent obj, FieldInfo field)
    {
      ValidateType(field);
      IFieldHandler result;
      if (obj.FieldHandlers.TryGetValue(field, out result))
        return (T)result;
      result = EntitySet.Activate(field.ValueType, obj, field);
      obj.FieldHandlers.Add(field, result);
      return (T)result;

    }

    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      // Unable to change EntitySet
      throw new InvalidOperationException(Strings.ExEntitySetCanTBeAssigned);
    }

    private EntitySetFieldAccessor()
    {
    }
  }
}