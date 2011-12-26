// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Defines a list of features which affects query generation.
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
    ParameterPrefix = 0x2,

    /// <summary>
    /// Indicates that RDBMS requires multi-table joins to have explicit order.
    /// (like "(a join b) join c")
    /// </summary>
    ExplicitJoinOrder = 0x4,

    /// <summary>
    /// Indicates that RDBMS supports batch query execution for DDL statements.
    /// </summary>
    DdlBatches = 0x8,

    /// <summary>
    /// Indicates that RDBMS supports batch query execution for DML statements.
    /// </summary>
    DmlBatches = 0x10,

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
    /// Indicates that RDBMS supports UPDATE ... FROM statements.
    /// </summary>
    UpdateFrom = 0x100,

    /// <summary>
    /// Indicates that RDBMS supports result limiting operator (LIMIT and TOP).
    /// </summary>
    Limit = 0x200,

    /// <summary>
    /// Indicates that RDBMS supports result skipping operator (OFFSET and SKIP).
    /// </summary>
    Offset = 0x400,

    /// <summary>
    /// Indicates that RDBMS supports multicolumn IN operator.
    /// </summary>
    MulticolumnIn = 0x800,
    
    /// <summary>
    /// Indicates whether RDBMS supports INSERT INTO Table DEFAULT VALUES syntax.
    /// </summary>
    DefaultValues = 0x1000,

    /// <summary>
    /// Indicates whether RDBMS supports ROW_NUMBER window function.
    /// </summary>
    RowNumber = 0x2000,

    /// <summary>
    /// Indicates whether RDBMS supports subqueries that return a scalar result.
    /// </summary>
    ScalarSubquery = 0x4000,

    /// <summary>
    /// Indicates that RDBMS supports DELETE ... FROM statements.
    /// </summary>
    DeleteFrom = 0x8000,

    /// <summary>
    /// Indicates that RDBMS supports paging operators (<see cref="Limit"/> and <see cref="Offset"/>).
    /// </summary>
    Paging = Limit | Offset,

    /// <summary>
    /// Indicates that RDBMS supports batches for both DDL and DML statements.
    /// </summary>
    Batches = DdlBatches | DmlBatches,
  }
}
