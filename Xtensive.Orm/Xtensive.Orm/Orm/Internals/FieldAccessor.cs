// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.19

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  internal abstract class FieldAccessor
  {
    private FieldInfo field;

    public FieldInfo Field {
      get { return field; }
      set {
        if (field!=null)
          throw Exceptions.AlreadyInitialized("Field");
        field = value;
      }
    }

    public object DefaultUntypedValue { get; private set; }

    public abstract bool AreSameValues(object oldValue, object newValue);

    public abstract void SetUntypedValue(Persistent obj, object value);

    public abstract object GetUntypedValue(Persistent obj);


    // Constructors
    
    protected FieldAccessor(object defaultUntypedValue)
    {
      DefaultUntypedValue = defaultUntypedValue;
    }
  }
}