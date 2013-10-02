// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.04.30

using System;
using System.Diagnostics;
using Xtensive.Reflection;

namespace Xtensive.Tuples.Transform
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
        defaultResult = null;
      }
    }

    /// <inheritdoc/>
    public Tuple DefaultResult {
      get {
        if (defaultResult==null && descriptor!=null)
          defaultResult = Tuple.Create(descriptor).ToReadOnly(TupleTransformType.Tuple);
        return defaultResult;
      }
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