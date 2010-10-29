// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.18

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Xtensive.Tuples.Internals
{
  /// <summary>
  /// Describes empty tuple.
  /// </summary>
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

    /// <inheritdoc/>
    public override int Count
    {
      get { return 0; }
    }

    /// <inheritdoc/>
    public override Tuple CreateNew()
    {
      return instance;
    }

    /// <inheritdoc/>
    public override Tuple Clone()
    {
      return instance;
    }

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    /// <inheritdoc/>
    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    /// <inheritdoc/>
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