// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.05

using System;
using System.Diagnostics;

namespace Xtensive.Orm
{
  /// <summary>
  /// Defines possible operation log types.
  /// </summary>
  public enum OperationLogType
  {
    /// <summary>
    /// Log stores system operations.
    /// The default option.
    /// </summary>
    SystemOperationLog,
    /// <summary>
    /// Log stores outermost operations.
    /// </summary>
    OutermostOperationLog,
    /// <summary>
    /// Log stores undo operations.
    /// </summary>
    UndoOperationLog,
  }
}