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
  public class ExampleTuple<TDescriptor> : GeneratedTuple,
    ITupleFieldAccessor<int>, 
    ITupleFieldAccessor<int?>,
    ITupleFieldAccessor<bool>,
    ITupleFieldAccessor<bool?>,
    ITupleFieldAccessor<string>
    where TDescriptor: GeneratedTupleDescriptor
  {
    private static readonly TDescriptor descriptor;
    private int c0;
    private int f0;
    private string f1;

    #region ITupleFieldAccessor<int> Members

    int ITupleFieldAccessor<int>.GetValueOrDefault(int fieldIndex)
    {
      switch (fieldIndex) {
        case 0:
          return f0;
        case 1:
        case 2:
          throw new InvalidCastException();
      }
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    void ITupleFieldAccessor<int>.SetValue(int fieldIndex, int fieldValue)
    {
      switch (fieldIndex) {
        case 0:
          f0 = fieldValue;
          SetFieldState(fieldIndex, TupleFieldState.IsAvailable);
          return;
        case 1:
        case 2:
          throw new InvalidCastException();
      }
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    #endregion

    #region ITupleFieldAccessor<int?> Members

    int? ITupleFieldAccessor<int?>.GetValueOrDefault(int fieldIndex)
    {
      switch (fieldIndex) {
        case 0:
          if (!HasValue(fieldIndex))
            return new int?();
          return new int?(f0);
        case 1:
        case 2:
          throw new InvalidCastException();
      }
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    void ITupleFieldAccessor<int?>.SetValue(int fieldIndex, int? fieldValue)
    {
      switch (fieldIndex) {
        case 0:
          TupleFieldState state;
          if (fieldValue.HasValue) {
            state = TupleFieldState.IsAvailable;
            f0 = fieldValue.Value;
          }
          else
            state = TupleFieldState.IsAvailable | TupleFieldState.IsNull;
          SetFieldState(fieldIndex, state);
          return;
        case 1:
        case 2:
          throw new InvalidCastException();
      }
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    #endregion

    #region ITupleFieldAccessor<bool> Members

    bool ITupleFieldAccessor<bool>.GetValueOrDefault(int fieldIndex)
    {
      switch (fieldIndex){
        case 1:
          return (bool)(object)((c0 & 4) >> 2);
        case 0:
        case 2:
          throw new InvalidCastException();
      }
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    void ITupleFieldAccessor<bool>.SetValue(int fieldIndex, bool fieldValue)
    {
      switch (fieldIndex) {
        case 1:
          c0 = c0 = (c0 & ~4) | ((int)(object)fieldValue << 2);
          SetFieldState(fieldIndex, TupleFieldState.IsAvailable);
          return;
        case 0:
        case 2:
          throw new InvalidCastException();
      }
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    #endregion

    #region ITupleFieldAccessor<bool?> Members

    bool? ITupleFieldAccessor<bool?>.GetValueOrDefault(int fieldIndex)
    {
      switch (fieldIndex) {
        case 1:
          if (!HasValue(fieldIndex))
            return null;
          return new bool?((bool)(object)((c0 & 4) >> 2));
        case 0:
        case 2:
          throw new InvalidCastException();
      }
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    void ITupleFieldAccessor<bool?>.SetValue(int fieldIndex, bool? fieldValue)
    {
      switch (fieldIndex) {
        case 1:
          TupleFieldState state;
          if (fieldValue.HasValue) {
            state = TupleFieldState.IsAvailable;
            c0 = c0 = (c0 & ~4) | ((int)(object)fieldValue << 2);
          }
          else
            state = TupleFieldState.IsAvailable | TupleFieldState.IsNull;
          SetFieldState(fieldIndex, state);
          return;
        case 0:
        case 2:
          throw new InvalidCastException();
      }
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    #endregion

    #region ITupleFieldAccessor<string> Members

    string ITupleFieldAccessor<string>.GetValueOrDefault(int fieldIndex)
    {
      return f1;
    }

    void ITupleFieldAccessor<string>.SetValue(int fieldIndex, string fieldValue)
    {
      SetFieldState(fieldIndex, TupleFieldState.IsAvailable);
      f1 = fieldValue;
    }

    #endregion

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

    public override T GetValueOrDefault<T>(int fieldIndex)
    {
      return ((ITupleFieldAccessor<T>)this).GetValueOrDefault(fieldIndex);
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
            state = TupleFieldState.IsAvailable;
            f0 = (int)fieldValue;
          }
          else
            state = TupleFieldState.IsAvailable | TupleFieldState.IsNull;
          SetFieldState(fieldIndex, state);
          return;
        case 1:
          if (fieldValue != null) {
            state = TupleFieldState.IsAvailable;
            c0 = (c0 & ~4) | ((int)fieldValue << 2);
          }
          else
            state = TupleFieldState.IsAvailable | TupleFieldState.IsNull;
          SetFieldState(fieldIndex, state);
          return;
        case 2:
          f1 = (string)fieldValue;
          SetFieldState(fieldIndex, TupleFieldState.IsAvailable);
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