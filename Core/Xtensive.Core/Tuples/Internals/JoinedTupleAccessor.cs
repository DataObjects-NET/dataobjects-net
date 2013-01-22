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
    private readonly SetValueDelegate<T> setValue;

    public T GetValueFirst(Tuple tuple, int fieldIndex, out TupleFieldState fieldState)
    {
      var joinedTuple = (JoinedTuple) tuple;
      return getValue(joinedTuple.First, fieldIndex, out fieldState);
    }

    public T GetValueSecond(Tuple tuple, int fieldIndex, out TupleFieldState fieldState)
    {
      var joinedTuple = (JoinedTuple) tuple;
      return getValue(joinedTuple.Second, fieldIndex, out fieldState);
    }

    public void SetValueFirst(Tuple tuple, int fieldIndex, T value)
    {
      var joinedTuple = (JoinedTuple) tuple;
      setValue(joinedTuple.First, fieldIndex, value);
    }

    public void SetValueSecond(Tuple tuple, int fieldIndex, T value)
    {
      var joinedTuple = (JoinedTuple) tuple;
      setValue(joinedTuple.Second, fieldIndex, value);
    }


    // Constructors

    public JoinedTupleAccessor(GetValueDelegate<T> getValue, SetValueDelegate<T> setValue, int fieldIndex)
    {
      this.getValue = getValue;
      this.setValue = setValue;

      GetValueDelegate<T> getValueDelegate;
      SetValueDelegate<T> setValueDelegate;
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