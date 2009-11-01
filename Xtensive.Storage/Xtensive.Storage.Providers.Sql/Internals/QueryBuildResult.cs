// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.30

using Xtensive.Sql.Dom.Dml;
using System.Collections.Generic;

namespace Xtensive.Storage.Providers.Sql
{
  internal struct QueryBuildResult
  {
    private readonly SqlTable table;
    private readonly SqlExpression expression;
    private readonly SqlTable primaryTable;
    private readonly List<SqlColumn> columns;

    public List<SqlColumn> Columns
    {
      get { return columns; }
    }

    public SqlTable Table
    {
      get { return table; }
    }

    public SqlExpression Expression
    {
      get { return expression; }
    }

    public SqlTable PrimaryTable
    {
      get { return primaryTable; }
    }

    public QueryBuildResult(SqlTable table, SqlExpression expression, SqlTable primaryTable, IEnumerable<SqlColumn> columns)
    {
      this.table = table;
      this.primaryTable = primaryTable;
      this.expression = expression;
      this.columns = new List<SqlColumn>(columns);
    }
  }
}