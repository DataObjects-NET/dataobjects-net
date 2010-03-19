// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.18

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Xtensive.Core.Tuples.Internals
{
  [Serializable]
  public sealed class EmptyTuple : RegularTuple
  {
    private readonly static EmptyTuple instance = new EmptyTuple();

    /// <summary>
    /// Provides the only instance of this class.
    /// </summary>
    public static EmptyTuple Instance
    {
      [DebuggerStepThrough]
      get { return instance; }
    }

    public override int Count
    {
      get { return 0; }
    }

    public override Tuple CreateNew()
    {
      return instance;
    }

    public override Tuple Clone()
    {
      return instance;
    }

    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    public override void SetValue(int fieldIndex, object fieldValue)
    {
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = EmptyTupleDescriptor.Instance;
    }


    // Constructors

    private EmptyTuple()
      : base(EmptyTupleDescriptor.Instance)
    {
    }
  }
}