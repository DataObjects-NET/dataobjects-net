// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public abstract class SqlTableHint : SqlHint
  {
    private readonly SqlTable sqlTable;

    /// <summary>
    /// Gets the SQL table.
    /// </summary>
    /// <value>The SQL table.</value>
    public SqlTable SqlTable
    {
      get { return sqlTable; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlTableHint"/> class.
    /// </summary>
    /// <param name="sqlTable">The SQL table.</param>
    protected SqlTableHint(SqlTable sqlTable)
    {
      this.sqlTable = sqlTable;
    }
  }
}
