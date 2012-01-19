// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System.Collections.Generic;

namespace Xtensive.Orm
{
  /// <summary>
  /// <see cref="IOperation"/> logger contract.
  /// </summary>
  public interface IOperationLogger
  {
    /// <summary>
    /// Gets operation log type.
    /// </summary>
    OperationLogType LogType { get; }
    
    /// <summary>
    /// Logs the specified operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    void Log(IOperation operation);

    /// <summary>
    /// Logs the specified sequence of operations.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    void Log(IEnumerable<IOperation> source);
  }
}