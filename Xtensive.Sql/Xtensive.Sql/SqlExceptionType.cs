// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

namespace Xtensive.Sql
{
  /// <summary>
  /// Possible exeception types.
  /// </summary>
  public enum SqlExceptionType
  {
    /// <summary>
    /// Reason of exception is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Connection timeout.
    /// </summary>
    ConnectionTimeout = 1,

    /// <summary>
    /// Syntax error in query.
    /// </summary>
    SyntaxError = 2,

    /// <summary>
    /// Constraint violation detected.
    /// </summary>
    ConstraintViolation = 3,

    // NOTE: all recoverable exceptions should have negative values.
    
    /// <summary>
    /// Deadlock detected.
    /// </summary>
    Deadlock = -1,

    /// <summary>
    /// Version conflict detected.
    /// </summary>
    VersionConflict = -2,
  }
}