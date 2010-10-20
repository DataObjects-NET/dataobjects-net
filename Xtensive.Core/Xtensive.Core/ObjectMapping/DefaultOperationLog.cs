// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.16

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.ObjectMapping
{
  /// <summary>
  /// Default implementation of <see cref="IOperationSequence"/> and operation logger.
  /// </summary>
  [Serializable]
  public sealed class DefaultOperationLog : LockableBase,
    IOperationSequence,
    IEnumerable<Operation>
  {
    private readonly List<Operation> operations = new List<Operation>();

    /// <inheritdoc/>
    public long Count {
      get { return operations.Count; }
    }

    /// <inheritdoc/>
    public object Replay(object target)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Adds the specified operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    public void Add(Operation operation)
    {
      this.EnsureNotLocked();
      operations.Add(operation);
    }

    #region IENumerable<...> methods

    /// <inheritdoc/>
    public IEnumerator<Operation> GetEnumerator()
    {
      return operations.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }
}