// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using Xtensive.Storage.Operations;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contract for an operation that could be executed later
  /// after being logged in <see cref="OperationLog"/>.
  /// </summary>
  public interface IOperation
  {
    /// <summary>
    /// Gets the title of the operation.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the description of the operation.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Prepares the operation using specified execution context.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    void Prepare(OperationExecutionContext context);

    /// <summary>
    /// Executes the operation using specified execution context.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    void Execute(OperationExecutionContext context);
  }
}