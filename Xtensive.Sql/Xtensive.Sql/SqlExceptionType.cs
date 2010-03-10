// Copyright (C) 2003-2010 Xtensive LLC.
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
    /// Connection error (such as timeout).
    /// </summary>
    ConnectionError,

    /// <summary>
    /// Syntax error in query.
    /// </summary>
    SyntaxError,

    /// <summary>
    /// Check constraint violation (including NOT NULL constraints)
    /// </summary>
    CheckConstraintViolation,

    /// <summary>
    /// Unique constraint violation (also denotes unique and primary index key duplication).
    /// </summary>
    UniqueConstraintViolation,

    /// <summary>
    /// Referential constraint (aka foreign key) violation.
    /// </summary>
    ReferentialConstraintViolation,

    /// <summary>
    /// Deadlock.
    /// </summary>
    Deadlock,

    /// <summary>
    /// Concurrent access serialization failure.
    /// </summary>
    SerializationFailure,

    /// <summary>
    /// Operation timed out.
    /// </summary>
    OperationTimeout,
  }
}