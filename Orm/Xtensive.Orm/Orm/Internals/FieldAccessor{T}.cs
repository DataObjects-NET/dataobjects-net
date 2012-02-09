// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.02

using System;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Internals
{
  internal abstract class FieldAccessor<T> : FieldAccessor
  {
    public T DefaultValue { get; private set; }

    public abstract void SetValue(Persistent obj, T value);

    public abstract T GetValue(Persistent obj);

    public override void SetUntypedValue(Persistent obj, object value)
    {
      SetValue(obj, (T) value);
    }

    public override object GetUntypedValue(Persistent obj)
    {
      return GetValue(obj);
    }


    // Constructors
    
    protected FieldAccessor()
      : base(default(T))
    {
      DefaultValue = default(T);
    }
  }
}