// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Linq.Normalization
{
  /// <summary>
  /// An operation with many operands.
  /// </summary>
  /// <typeparam name="T">The type of operands.</typeparam>
  [Serializable]
  public abstract class MultioperandOperation<T>
  {
    /// <summary>
    /// Gets the operands.
    /// </summary>
    public HashSet<T> Operands { get; private set; }

    /// <inheritdoc/>
    protected MultioperandOperation()
    {
      Operands = new HashSet<T>();
    }
  }
}