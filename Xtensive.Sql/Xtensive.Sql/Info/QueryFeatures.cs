// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Defines a list of features which affects
  /// query generation.
  /// </summary>
  [Flags]
  public enum QueryFeatures
  {
    /// <summary>
    /// There are no features affecting query generation.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that it is possible to use named query parameters.
    /// </summary>
    NamedParameters = 0x1,

    /// <summary>
    /// Indicates that parameter prefix should be used for parameter names
    /// (in the case when <see cref="NamedParameters"/> option is active).
    /// </summary>
    UseParameterPrefix = 0x2,

    // SquareBrackets removed - useless

    /// <summary>
    /// Indicates that RDBMS requires multi-table joins to have explicit order.
    /// (like "(a join b) join c")
    /// </summary>
    ExplicitJoinOrder = 0x8,

    /// <summary>
    /// Indicates that RDBMS supports batch query execution.
    /// </summary>
    Batches = 0x10,

    /// <summary>
    /// Indicates that RDBMS supports query optimizer hints.
    /// </summary>
    Hints = 0x20,

    /// <summary>
    /// Indicates that RDBMS supports CROSS APPLY and OUTER APPLY operators.
    /// </summary>
    CrossApply = 0x40,

    /// <summary>
    /// Indicates that RDBMS allows boolean expressions in equality/inequality comparisons,
    /// inside CASE and COALESCE operators.
    /// </summary>
    FullBooleanExpressionSupport = 0x80,

    /// <summary>
    /// Indicates that RDBMS supports paging operators (LIMIT and OFFSET)
    /// </summary>
    Paging = 0x100,
  }
}
