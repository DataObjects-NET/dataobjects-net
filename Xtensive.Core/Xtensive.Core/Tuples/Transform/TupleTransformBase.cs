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
    /// <inheritdoc/>
    public TupleDescriptor Descriptor { get; protected set; }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public virtual bool IsReadOnly {
      get {
        return false;
      }
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