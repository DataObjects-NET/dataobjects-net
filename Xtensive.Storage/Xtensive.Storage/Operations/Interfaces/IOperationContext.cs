// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.18

using System;
using System.Collections.Generic;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Contract for an operation context that manages <see cref="IOperation"/> registration.
  /// </summary>
  public interface IOperationContext : IEnumerable<IOperation>,
    IDisposable
  {
    /// <summary>
    /// Gets a value indicating whether this context accepts operations with the normal priority.
    /// </summary>
    bool AreNormalOperationAccepted { get; }

    /// <summary>
    /// Gets a value indicating whether nested contexts must be disabled.
    /// </summary>
    bool DisableNested { get; }

    /// <summary>
    /// Adds the operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    void Add(IOperation operation);

    /// <summary>
    /// Adds the operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="highPriority">if set to <see langword="true"/>
    /// the operation is considered as the high priority operation.</param>
    void Add(IOperation operation, bool highPriority);

    /// <summary>
    /// Completes registration of operations in the context.
    /// </summary>
    void Complete();
  }
}