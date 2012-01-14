// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.19

using System;
using System.Diagnostics;

namespace Xtensive.Tuples.Internals
{
  internal class JoinedTupleAccessor
  {
    public Delegate GetValueDelegate;
    public Delegate SetValueDelegate;
  }

  internal sealed class JoinedTupleAccessor<T> : JoinedTupleAccessor
  {
    private readonly GetValueDelegate<T> getValue;
    private readonly Action<Tuple, T> setValue;
    private int fieldIndex;

    public T GetValueFirst(Tuple tuple, out TupleFieldState fieldState)
    {
      var joinedTuple = (JoinedTuple) tuple;
      return getValue(joinedTuple.First, out fieldState);
    }

    public T GetValueSecond(Tuple tuple, out TupleFieldState fieldState)
    {
      var joinedTuple = (JoinedTuple) tuple;
      return getValue(joinedTuple.Second, out fieldState);
    }

    public void SetValueFirst(Tuple tuple, T value)
    {
      var joinedTuple = (JoinedTuple)tuple;
      setValue(joinedTuple.First, value);
    }

    public void SetValueSecond(Tuple tuple, T value)
    {
      var joinedTuple = (JoinedTuple)tuple;
      setValue(joinedTuple.Second, value);
    }


    // Constructors

    public JoinedTupleAccessor(GetValueDelegate<T> getValue, Action<Tuple, T> setValue, int fieldIndex)
    {
      this.getValue = getValue;
      this.setValue = setValue;
      this.fieldIndex = fieldIndex;

      GetValueDelegate<T> getValueDelegate;
      Action<Tuple, T> setValueDelegate;
      if (fieldIndex < JoinedTuple.FirstCount) {
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