// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.02

using System;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples.Transform.Internals;

namespace Xtensive.Core.Tuples.Transform
{
  /// <summary>
  /// Describes read-only tuple transformation.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="SingletonDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public sealed class ReadOnlyTransform : TupleTransformBase
  {
    private static readonly ReadOnlyTransform instance = new ReadOnlyTransform();

    /// <see cref="SingletonDocTemplate.Instance" copy="true"/>
    public static ReadOnlyTransform Instance {
      get { return instance; }
    }

    /// <summary>
    /// <inheritdoc/>
    /// Implementation in this class always returns <see langword="true"/>.
    /// </summary>
    public override bool IsReadOnly {
      get {
        return true;
      }
    }

    /// <inheritdoc/>
    public override Tuple Apply(TupleTransformType transformType, params object[] arguments)
    {
      ArgumentValidator.EnsureArgumentNotNull(arguments, "arguments");
      return Apply(transformType, arguments[0]);
    }

    /// <summary>
    /// Typed version of <see cref="Apply(TupleTransformType,object[])"/>.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="source">Transformation argument.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    public Tuple Apply(TupleTransformType transformType, Tuple source)
    {
      switch (transformType) {
      case TupleTransformType.Auto:
        // TODO: Implement "Auto" for generated read-only tuples, when they'll be ready
      case TupleTransformType.TransformedTuple:
        if (source is ReadOnlyTransformTuple)
          return source;
        return new ReadOnlyTransformTuple(source);
      case TupleTransformType.Tuple:
        // TODO: Return generated read-only tuple copy
        return new ReadOnlyTransformTuple(source.ToRegular());
      default:
        throw new ArgumentOutOfRangeException("transformType");
      }
    }

    
    // Constructors
    
    private ReadOnlyTransform()
    {
    }
  }
}