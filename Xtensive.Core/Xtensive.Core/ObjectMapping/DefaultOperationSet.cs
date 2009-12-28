// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.16

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// Default implementation of <see cref="IOperationSet"/>.
  /// </summary>
  [Serializable]
  public sealed class DefaultOperationSet : LockableBase,
    IOperationSet,
    IEnumerable<OperationInfo>
  {
    private readonly List<OperationInfo> descriptors = new List<OperationInfo>();

    /// <inheritdoc/>
    public bool IsEmpty { get { return descriptors.Count==0; } }

    /// <inheritdoc/>
    public void Apply()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Adds the specified operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    public void Add(OperationInfo operation)
    {
      this.EnsureNotLocked();
      descriptors.Add(operation);
    }

    /// <inheritdoc/>
    public IEnumerator<OperationInfo> GetEnumerator()
    {
      return descriptors.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}