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
    Unknown,

    /// <summary>
    /// Connection timeout.
    /// </summary>
    ConnectionTimeout,

    /// <summary>
    /// Syntax error in query.
    /// </summary>
    SyntaxError,

    /// <summary>
    /// Unique constraint violation (also denotes unique index key duplication).
    /// </summary>
    UniqueConstraintViolation,

    /// <summary>
    /// Referential constraint (aka foreign key) violation.
    /// </summary>
    ReferentialContraintViolation,

    /// <summary>
    /// Deadlock.
    /// </summary>
    Deadlock,

    /// <summary>
    /// Version conflict.
    /// </summary>
    VersionConflict,
  }
}