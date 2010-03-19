// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.19

using System;
using System.Diagnostics;

namespace Xtensive.Core.Tuples.Internals
{
  internal class TupleExtenderAccessor
  {
    public Delegate GetValueDelegate;
    public Delegate SetValueDelegate;
  }

  internal sealed class TupleExtenderAccessor<T> : TupleExtenderAccessor
  {
    private readonly GetValueDelegate<T> getValue;
    private readonly Action<Tuple, T> setValue;
    private int fieldIndex;

    public T GetValueFirst(Tuple tuple, out TupleFieldState fieldState)
    {
      var extender = (TupleExtender) tuple;
      return getValue(extender.First, out fieldState);
    }

    public T GetValueSecond(Tuple tuple, out TupleFieldState fieldState)
    {
      var extender = (TupleExtender)tuple;
      return getValue(extender.Second, out fieldState);
    }

    public void SetValueFirst(Tuple tuple, T value)
    {
      var extender = (TupleExtender)tuple;
      setValue(extender.First, value);
    }

    public void SetValueSecond(Tuple tuple, T value)
    {
      var extender = (TupleExtender)tuple;
      setValue(extender.Second, value);
    }


    // Constructors

    public TupleExtenderAccessor(GetValueDelegate<T> getValue, Action<Tuple, T> setValue, int fieldIndex)
    {
      this.getValue = getValue;
      this.setValue = setValue;
      this.fieldIndex = fieldIndex;

      GetValueDelegate<T> getValueDelegate;
      Action<Tuple, T> setValueDelegate;
      if (fieldIndex < TupleExtender.FirstCount) {
        getValueDelegate = GetValueFirst;
        setValueDelegate = SetValueFirst;
      }
      else {
        getValueDelegate = GetValueSecond;
        setValueDelegate = SetValueSecond;
      }
      GetValueDelegate = getValueDelegate;
      SetValueDelegate = setValueDelegate;
    }
  }
}