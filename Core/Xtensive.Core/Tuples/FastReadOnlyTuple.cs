// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.01

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Tuples
{
  /// <summary>
  /// Fast read-only <see cref="Tuple"/> implementation.
  /// </summary>
  [Serializable]
  public sealed class FastReadOnlyTuple : Tuple
  {
    private readonly TupleDescriptor descriptor;
    private readonly TupleFieldState[] states;
    private readonly object[] values;
    private int? cachedHash;

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor
    {
      get { return descriptor; }
    }

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return states[fieldIndex];
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      throw Exceptions.ObjectIsReadOnly(null);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      fieldState = states[fieldIndex];
      return values[fieldIndex];
    }

    /// <inheritdoc/>
    /// <summary>
    /// <inherited/>
    /// Always throws <see cref="NotSupportedException"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      throw Exceptions.ObjectIsReadOnly(null);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      if (!cachedHash.HasValue)
        cachedHash = base.GetHashCode();
      return cachedHash.GetValueOrDefault();
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The tuple to create the fast read-only tuple from.</param>
    public FastReadOnlyTuple(Tuple source)
    {
      descriptor = source.Descriptor;
      int count = descriptor.Count;
      values = new object[count];
      states = new TupleFieldState[count];
      for (int i = 0; i<count; i++) {
        TupleFieldState state;
        values[i] = source.GetValue(i, out state);
        states[i] = state;
      }
    }
  }
}