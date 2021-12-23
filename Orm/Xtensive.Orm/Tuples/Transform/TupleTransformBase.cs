// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.04.30

using System;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Base class for any tuple transform.
  /// </summary>
  [Serializable]
  public abstract class TupleTransformBase
  {
    /// <summary>
    /// Gets <see cref="TupleDescriptor"/> describing the tuples
    /// this transform may produce.
    /// </summary>
    public TupleDescriptor Descriptor { get; }

    /// <summary>
    /// Indicates whether transform always produces read-only tuples or not.
    /// </summary>
    public bool IsReadOnly { get; }

    /// <inheritdoc/>
    public override string ToString() => $"[{GetType().GetShortName()}, {(IsReadOnly ? "readOnly" : string.Empty)}]";

    protected TupleTransformBase(TupleDescriptor descriptor, bool isReadOnly)
    {
      ArgumentValidator.EnsureArgumentNotNull(descriptor, nameof(descriptor));
      Descriptor = descriptor;
      IsReadOnly = isReadOnly;
    }
  }
}