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
  /// Operation context contract. Operation context manages 
  /// <see cref="IOperation"/> logging in <see cref="Session"/>.
  /// </summary>
  public interface IOperationContext : IEnumerable<IOperation>,
    IDisposable
  {
    /// <summary>
    /// Gets a value indicating whether  <see cref="LogOperation"/> method
    /// is enabled in this context.
    /// Note that operations implementing <see cref="IPrecondition"/> 
    /// are always logged - independently of value of this property.
    /// </summary>
    bool IsLoggingEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether nested contexts must be created
    /// with <see cref="IsLoggingEnabled"/>==<see langword="true" /> option.
    /// </summary>
    bool IsIntermediate { get; }

    /// <summary>
    /// Logs the operation.
    /// When <see cref="IsLoggingEnabled"/> is off, this method logs only
    /// <see cref="IPrecondition"/> operations (e.g. version checks).
    /// </summary>
    /// <param name="operation">The operation to log.</param>
    void LogOperation(IOperation operation);

    /// <summary>
    /// Completes registration of operations that were logged in the context.
    /// </summary>
    void Complete();
  }
}