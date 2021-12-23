// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.04.30

using System;
using Xtensive.Reflection;

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Base class for any tuple transform.
  /// </summary>
  [Serializable]
  public abstract class TupleTransformBase
  {
    private Tuple defaultResult;

    /// <summary>
    /// Gets <see cref="TupleDescriptor"/> describing the tuples
    /// this transform may produce.
    /// <see langword="Null"/> means "any" (i.e. transform definition 
    /// is not descriptor-dependent).
    /// </summary>
    public TupleDescriptor Descriptor { get; }

    /// <summary>
    /// Gets the default result tuple.
    /// Can be used to get default values for the result tuple fields.
    /// Must be a read-only tuple.
    /// </summary>
    public Tuple DefaultResult =>
      Descriptor == null ? null : defaultResult ??= Tuple.Create(Descriptor).ToReadOnly(TupleTransformType.Tuple);

    /// <summary>
    /// Indicates whether transform always produces read-only tuples or not.
    /// </summary>
    public virtual bool IsReadOnly => false;

    /// <inheritdoc/>
    public override string ToString() => GetType().GetShortName();

    protected TupleTransformBase(TupleDescriptor descriptor)
    {
      Descriptor = descriptor;
    }
  }
}