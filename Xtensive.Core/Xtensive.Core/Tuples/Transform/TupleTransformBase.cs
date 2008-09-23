// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.04.30

using System;
using System.Diagnostics;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Tuples.Transform
{
  /// <summary>
  /// Base class for any tuple transform.
  /// </summary>
  [Serializable]
  public abstract class TupleTransformBase : ITupleTransform
  {
    private TupleDescriptor descriptor;
    private Tuple defaultResult;

    /// <inheritdoc/>
    public TupleDescriptor Descriptor
    {
      get { return descriptor; }
      protected set {
        descriptor = value;
        defaultResult = descriptor==null ? null : Tuple.Create(descriptor).ToReadOnly(TupleTransformType.Tuple);
      }
    }

    /// <inheritdoc/>
    public Tuple DefaultResult {
      get { return defaultResult; }
    }

    /// <inheritdoc/>
    public virtual bool IsReadOnly {
      [DebuggerStepThrough]
      get { return false; }
    }

    /// <inheritdoc/>
    public abstract Tuple Apply(TupleTransformType transformType, params object[] arguments);

    /// <inheritdoc/>
    public override string ToString()
    {
      return GetType().GetShortName();
    }
  }
}