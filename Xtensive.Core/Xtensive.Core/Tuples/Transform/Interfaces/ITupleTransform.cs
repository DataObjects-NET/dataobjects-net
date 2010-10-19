// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.04.30

using Xtensive.Tuples.Transform;

namespace Xtensive.Tuples
{
  /// <summary>
  /// Tuple transformation definition.
  /// </summary>
  public interface ITupleTransform
  {
    /// <summary>
    /// Gets <see cref="TupleDescriptor"/> describing the tuples
    /// this transform may produce.
    /// <see langword="Null"/> means "any" (i.e. transform definition 
    /// is not descriptor-dependent).
    /// </summary>
    TupleDescriptor Descriptor { get; }

    /// <summary>
    /// Gets the default result tuple.
    /// Can be used to get default values for the result tuple fields.
    /// Must be a read-only tuple.
    /// </summary>
    Tuple DefaultResult { get; }

    /// <summary>
    /// Indicates whether transform always produces read-only tuples or not.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="arguments">Transformation arguments.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    Tuple Apply(TupleTransformType transformType, params object[] arguments);
  }
}