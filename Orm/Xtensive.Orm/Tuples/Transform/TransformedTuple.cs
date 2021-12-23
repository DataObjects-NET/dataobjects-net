// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.07

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;


namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Base class for any transformed tuples.
  /// </summary>
  [Serializable]
  public abstract class TransformedTuple : Tuple
  {
    // /// <summary>
    // /// Gets the transform used to produce this instance.
    // /// </summary>
    // public abstract bool IsReadOnly { get; }

    /// <inheritdoc/>
    public abstract override TupleDescriptor Descriptor { get; }
  }
}