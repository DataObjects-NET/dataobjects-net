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
  public abstract class TransformedTuple<TTupleTransform> : Tuple, ITransformedTuple
    where TTupleTransform : ITupleTransform
  {
    /// <summary>
    /// Gets or sets the transform used to produce this instance.
    /// </summary>
    public TTupleTransform TupleTransform { get; }

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor => TupleTransform.Descriptor;

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.TransformedTupleFormat, base.ToString(), TupleTransform, string.Empty);
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="transform">Tuple transform.</param>
    protected TransformedTuple(TTupleTransform transform)
    {
      TupleTransform = transform;
    }
  }
}