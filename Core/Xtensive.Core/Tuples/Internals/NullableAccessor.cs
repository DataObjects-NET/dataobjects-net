// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.19

using System;
using System.Diagnostics;

namespace Xtensive.Tuples.Internals
{
  internal class NullableAccessor
  {
    public Delegate GetValueDelegate;
    public Delegate SetValueDelegate;
  }

  [Serializable]
  internal sealed class NullableAccessor<T> : NullableAccessor
    where T : struct
  {
    private readonly GetValueDelegate<T> getValue;
    private readonly Action<Tuple,T> setValue;

    private int fieldIndex;

    public T? GetValue(Tuple tuple, out TupleFieldState fieldState)
    {
      var value = getValue(tuple, out fieldState);
      if (fieldState == TupleFieldState.Available)
        return value;
      return null;
    }

    public void SetValue(Tuple tuple, T? value)
    {
      if (value.HasValue)
        setValue(tuple, value.Value);
      else
        tuple.SetValue(fieldIndex, null);
    }


    // Constructors

    public NullableAccessor(GetValueDelegate<T> getValue, Action<Tuple, T> setValue, int fieldIndex)
    {
      this.getValue = getValue;
      this.setValue = setValue;
      this.fieldIndex = fieldIndex;
      GetValueDelegate<T?> getValueDelegate = GetValue;
      Action<Tuple, T?> setValueDelegate = SetValue;
      GetValueDelegate = getValueDelegate;
      SetValueDelegate = setValueDelegate;
    }
  }
}