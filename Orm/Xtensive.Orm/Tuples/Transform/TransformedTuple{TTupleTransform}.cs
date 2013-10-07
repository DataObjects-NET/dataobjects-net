// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.04.30

using System;
using System.Diagnostics;


namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Base class for all transformed tuples.
  /// </summary>
  [Serializable]
  public abstract class TransformedTuple<TTupleTransform> : TransformedTuple
    where TTupleTransform : ITupleTransform
  {
    private TTupleTransform transform;

    /// <inheritdoc/>
    public override ITupleTransform Transform
    {
      [DebuggerStepThrough]
      get { return transform; }
    }

    /// <summary>
    /// Gets or sets the transform used to produce this instance.
    /// </summary>
    public TTupleTransform TypedTransform {
      get { return transform; }
      protected set {
        transform = value;
      }
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="transform">Tuple transform.</param>
    protected TransformedTuple(TTupleTransform transform)
    {
      this.transform = transform;
    }
  }
}