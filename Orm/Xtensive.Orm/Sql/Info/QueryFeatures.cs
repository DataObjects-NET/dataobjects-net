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
    None = 0,

    /// <summary>
    /// Indicates that it is possible to use named query parameters.
    /// </summary>
    NamedParameters = 1 << 0,

    /// <summary>
    /// Indicates that parameter prefix should be used for parameter names
    /// (in the case when <see cref="NamedParameters"/> option is active).
    /// </summary>
    ParameterPrefix = 1 << 2,

    /// <summary>
    /// Indicates that RDBMS requires multi-table joins to have explicit order.
    /// (like "(a join b) join c")
    /// </summary>
    ExplicitJoinOrder = 1 << 3,

    /// <summary>
    /// Indicates that RDBMS supports batch query execution for DDL statements.
    /// </summary>
    DdlBatches = 1 << 4,

    /// <summary>
    /// Indicates that RDBMS supports batch query execution for DML statements.
    /// </summary>
    DmlBatches = 1 << 5,

    /// <summary>
    /// Indicates that RDBMS supports query optimizer hints.
    /// </summary>
    Hints = 1 << 6,

    /// <summary>
    /// Indicates that RDBMS supports CROSS APPLY and OUTER APPLY operators.
    /// </summary>
    CrossApply = 1 << 7,

    /// <summary>
    /// Indicates that RDBMS allows boolean expressions in equality/inequality comparisons,
    /// inside CASE and COALESCE operators.
    /// </summary>
    FullBooleanExpressionSupport = 1 << 8,

    /// <summary>
    /// Indicates that RDBMS supports UPDATE ... FROM statements.
    /// </summary>
    UpdateFrom = 1 << 9,

    /// <summary>
    /// Indicates that RDBMS supports DELETE ... FROM statements.
    /// </summary>
    DeleteFrom = 1 << 10,

    /// <summary>
    /// Indicates that RDBMS supports result limiting operator (LIMIT and TOP).
    /// </summary>
    Limit = 1 << 11,

    /// <summary>
    /// Indicates that RDBMS supports result skipping operator (OFFSET and SKIP).
    /// </summary>
    Offset = 1 << 12,

    /// <summary>
    /// Indicates that RDBMS supports multicolumn IN operator.
    /// </summary>
    MulticolumnIn = 1 << 13,

    /// <summary>
    /// Indicates whether RDBMS supports ROW_NUMBER window function.
    /// </summary>
    RowNumber = 1 << 14,

    /// <summary>
    /// Indicates whether RDBMS supports subqueries that return a scalar result.
    /// </summary>
    ScalarSubquery = 1 << 15,

    /// <summary>
    /// Indicates whether RDBMS supports INSERT INTO Table DEFAULT VALUES syntax.
    /// </summary>
    InsertDefaultValues = 1 << 16,

    /// <summary>
    /// Indicates whether RDBMS supports UPDATE Table SET COLUMN = DEFAULT syntax.
    /// </summary>
    UpdateDefaultValues = 1 << 17,

    /// <summary>
    /// Indicates whether RDBMS supports accessing multiple schemasin single statement.
    /// </summary>
    MultischemaQueries = 1 << 18,

    /// <summary>
    /// Indicates whether RDBMS supports accessing multiple databases in single statement.
    /// </summary>
    MultidatabaseQueries = 1 << 19,

    /// <summary>
    /// Indicates whether RDBMS requires "order by" clause when "limit" or "offset" is used.
    /// </summary>
    PagingRequiresOrderBy = 1 << 20,

    /// <summary>
    /// Indicates whether RDBMS raises error when "limit 0" clause is provided.
    /// </summary>
    ZeroLimitIsError = 1 << 21,

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
