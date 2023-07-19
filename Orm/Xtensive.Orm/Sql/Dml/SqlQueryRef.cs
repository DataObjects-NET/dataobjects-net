// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlQueryRef : SqlTable
  {
    private readonly ISqlQueryExpression query;

    /// <summary>
    /// Gets the query statement.
    /// </summary>
    /// <value>The query statement.</value>
    public ISqlQueryExpression Query
    {
      get { return query; }
    }

    internal override SqlQueryRef Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        t.query is SqlSelect ss
          ? new SqlQueryRef(ss.Clone(c), t.Name)
          : new SqlQueryRef(((SqlQueryExpression) t.query).Clone(c), t.Name));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlQueryRef(ISqlQueryExpression query)
      : this(query, string.Empty)
    {
    }

    internal SqlQueryRef(ISqlQueryExpression query, string name)
      : base(name)
    {
      this.query = query;
      var queryColumns = new List<SqlTableColumn>();
      foreach (var queryExpression in query) {
        if (queryExpression is SqlSelect sqlSelect) {
          var selectColumns = sqlSelect.Columns;
          for (int index = 0, count = selectColumns.Count; index < count; index++) {
            var originalColumn = selectColumns[index];
            var column = originalColumn;
            if (column is SqlColumnStub stubColumn) {
              column = stubColumn.Column;
            }

            if (column is SqlColumnRef columnRef) {
              stubColumn = columnRef.SqlColumn as SqlColumnStub;
              if (stubColumn is not null) {
                column = stubColumn.Column;
              }
            }

            selectColumns[index] = column;
            queryColumns.Add(SqlDml.TableColumn(this, originalColumn.Name));
          }
        }

        if (queryExpression is SqlFreeTextTable freeTextTable) {
          queryColumns.AddRange(
            freeTextTable.Columns.Select(originalColumn => SqlDml.TableColumn(this, originalColumn.Name)));
        }

        if (queryExpression is SqlContainsTable containsTable) {
          queryColumns.AddRange(
            containsTable.Columns.Select(originalColumn => SqlDml.TableColumn(this, originalColumn.Name)));
        }

        break;
      }
      columns = new SqlTableColumnCollection(queryColumns);
    }
  }
}