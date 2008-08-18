// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Tuples.Internals
{
  /// <summary>
  /// Base class for any generated tuple.
  /// </summary>
  [Serializable]
  public abstract class GeneratedTuple: RegularTuple, ITupleFieldAccessor<object>
  {
    /// <inheritdoc />
    public override sealed void SetValue<T>(int fieldIndex, T fieldValue)
    {
      ((ITupleFieldAccessor<T>)this).SetValue(fieldIndex, fieldValue);
    }

    /// <inheritdoc/>
    public override sealed T GetValueOrDefault<T>(int fieldIndex)
    {
      return ((ITupleFieldAccessor<T>)this).GetValueOrDefault(fieldIndex);
    }

    /// <summary>
    /// Sets the field state associated with the field.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to set the state for.</param>
    /// <param name="state">The state to set.</param>
    protected abstract void SetFieldState(int fieldIndex, TupleFieldState state);


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected GeneratedTuple()
    {
    }
  }
}