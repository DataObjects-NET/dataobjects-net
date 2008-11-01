// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.01

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Fast read-only <see cref="Tuple"/> implementation.
  /// </summary>
  [Serializable]
  public class FastReadOnlyTuple : Tuple
  {
    private readonly TupleDescriptor descriptor;
    private TupleFieldState[] states;
    private object[] values;

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

    /// <inheritdoc/>
    public override object GetValueOrDefault(int fieldIndex)
    {
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

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The tuple to create the fast read-only tuple from.</param>
    public FastReadOnlyTuple(Tuple source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      descriptor = source.Descriptor;
      int count = descriptor.Count;
      values = new object[count];
      states = new TupleFieldState[count];
      for (int i = 0; i<count; i++) {
        values[i] = source.GetValueOrDefault(i);
        states[i] = source.GetFieldState(i);
      }
    }
  }
}