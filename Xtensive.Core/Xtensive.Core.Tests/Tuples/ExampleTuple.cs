// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Internals;

namespace Xtensive.Core.Tests.Tuples
{
  // Contains 3 field : int, bool, string
  public class ExampleTuple<TDescriptor> : GeneratedTuple
    where TDescriptor: GeneratedTupleDescriptor
  {
    private static readonly TDescriptor descriptor;
    private int c0;
    private int f0;
    private string f1;

    public override TupleDescriptor Descriptor
    {
      get { return descriptor; }
    }

    public override Tuple CreateNew()
    {
      return new ExampleTuple<TDescriptor>();
    }

    public override Tuple Clone()
    {
      return new ExampleTuple<TDescriptor>(this);
    }

    public override object GetValueOrDefault(int fieldIndex)
    {
      if (!HasValue(fieldIndex))
        return null;
      switch(fieldIndex) {
        case 0:
          return f0;
        case 1:
          return (c0 & 4) >> 2;
        case 2:
          return f1;
      }
      throw new ArgumentOutOfRangeException();
    }

    public override void SetValue(int fieldIndex, object fieldValue)
    {
      TupleFieldState state;
      switch (fieldIndex) {
        case 0:
          if (fieldValue != null) {
            state = TupleFieldState.Available;
            f0 = (int)fieldValue;
          }
          else
            state = TupleFieldState.Available | TupleFieldState.Null;
          SetFieldState(fieldIndex, state);
          return;
        case 1:
          if (fieldValue != null) {
            state = TupleFieldState.Available;
            c0 = (c0 & ~4) | ((int)fieldValue << 2);
          }
          else
            state = TupleFieldState.Available | TupleFieldState.Null;
          SetFieldState(fieldIndex, state);
          return;
        case 2:
          f1 = (string)fieldValue;
          SetFieldState(fieldIndex, TupleFieldState.Available);
          return;
      }
      throw new ArgumentOutOfRangeException();
    }

    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      switch (fieldIndex) {
        case 0:
          return (TupleFieldState)((c0 & 3) >> 0);
        case 1:
          return (TupleFieldState)((c0 & 24) >> 3);
        case 2:
          return (TupleFieldState)((c0 & 96) >> 5);
      }
      throw new ArgumentOutOfRangeException();
    }

    protected override void SetFieldState(int fieldIndex, TupleFieldState state)
    {
      switch (fieldIndex) {
        case 0:
          c0 = (c0 & ~3) | ((int)state << 0);
          return;
        case 1:
          c0 = (c0 & ~24) | ((int)state << 3);
          return;
        case 2:
          c0 = (c0 & ~96) | ((int)state << 5);
          return;
      }
      throw new ArgumentOutOfRangeException();
    }

    public override bool Equals(Tuple obj)
    {
      if (obj == null)
        return false;
      if (this == obj)
        return true;
      if (Descriptor != obj.Descriptor)
        return false;
      var other = obj as ExampleTuple<TDescriptor>;
      if (other == null) {
        for (int fieldIndex = 0; fieldIndex < Count; fieldIndex++) {
          bool xHasNoValue = !IsAvailable(fieldIndex);
          bool yHasNoValue = !obj.IsAvailable(fieldIndex);
          if (xHasNoValue) {
            if (yHasNoValue)
              continue;
            return false;
          }
          if (yHasNoValue)
            return false;
          if (!Equals(GetValueOrDefault(fieldIndex), obj.GetValueOrDefault(fieldIndex)))
            return false;
        }
        return true;
      }
      return other.c0==c0 && other.f0==f0 && Equals(other.f1, f1);
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as Tuple);
    }

    public override int GetHashCode()
    {
      unchecked {
        int result = 0;
        result = (result * 397) ^ f0;
        result = (result * 397) ^ ((c0 & 4) >> 2);
        result = (result * 397) ^ (f1!=null ? f1.GetHashCode() : 0);
        return result;
      }
    }

    public override int Count
    {
      get { return 5; }
    }


    // Constructors

    public ExampleTuple()
    {
    }

    public ExampleTuple(ExampleTuple<TDescriptor> tuple)
    {
      c0 = tuple.c0;
      f0 = tuple.f0;
      f1 = tuple.f1;
    }

    // Static constructor
    
    static ExampleTuple()
    {
      descriptor = (TDescriptor)TupleDescriptor.Create(typeof(TDescriptor));
    }
  }
}