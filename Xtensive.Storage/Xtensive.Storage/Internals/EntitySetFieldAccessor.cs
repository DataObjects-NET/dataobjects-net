// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using System;
using Xtensive.Storage.Model;

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
      throw new InvalidOperationException();
    }

    public override void SetValue(Persistent obj, FieldInfo field, T value)
    {
      ValidateType(field);
      throw new InvalidOperationException();
    }

    private EntitySetFieldAccessor()
    {
    }
  }
}