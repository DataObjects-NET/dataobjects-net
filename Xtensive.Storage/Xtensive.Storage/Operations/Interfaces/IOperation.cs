// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Declares public contract for operations that could be executed later.
  /// </summary>
  public interface IOperation
  {
    /// <summary>
    /// Prepares the operation using specified operation context.
    /// </summary>
    /// <param name="operationContext">The operation context.</param>
    void Prepare(OperationContext operationContext);

    /// <summary>
    /// Executes the operation using specified operation context.
    /// </summary>
    /// <param name="operationContext">The operation context.</param>
    void Execute(OperationContext operationContext);
  }
}