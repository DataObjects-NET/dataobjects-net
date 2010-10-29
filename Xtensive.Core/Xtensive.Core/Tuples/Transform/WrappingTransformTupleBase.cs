// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.02

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Base class for one-to-one tuple transformations.
  /// </summary>
  [Serializable]
  public abstract class WrappingTransformTupleBase: TransformedTuple
  {
    protected readonly Tuple origin;

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor
    {
      [DebuggerStepThrough]
      get { return origin.Descriptor; }
    }

    /// <inheritdoc />
    public override int Count {
      [DebuggerStepThrough]
      get { return origin.Count; }
    }

    /// <inheritdoc/>
    public override object[] Arguments {
      [DebuggerStepThrough]
      get { return new object[] {origin}; }
    }

    #region GetFieldState, GetValueOrDefault, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return origin.GetFieldState(fieldIndex);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      return origin.GetValue(fieldIndex, out fieldState);
    }

    /// <inheritdoc />
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      origin.SetValue(fieldIndex, fieldValue);
    }

    #endregion

    /// <inheritdoc/>
    public sealed override bool Equals(Tuple other)
    {
      return origin.Equals(other);
    }

    /// <inheritdoc/>
    public sealed override int GetHashCode()
    {
      return origin.GetHashCode();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="tuple">Tuple to provide the wrapper for.</param>
    protected WrappingTransformTupleBase(Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");
      origin = tuple;
    }
  }
}