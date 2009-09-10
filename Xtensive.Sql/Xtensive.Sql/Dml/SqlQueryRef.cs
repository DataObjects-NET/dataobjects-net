// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlQueryRef clone;
      var ss = query as SqlSelect;
      if (ss != null)
        clone = new SqlQueryRef((SqlSelect)ss.Clone(context), Name);
      else {
        var qe = (SqlQueryExpression) query;
        clone = new SqlQueryRef((SqlQueryExpression)qe.Clone(context), Name);
      }
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlQueryRef(ISqlQueryExpression query) : this(query, string.Empty)
    {
    }

    internal SqlQueryRef(ISqlQueryExpression query, string name) : base(name)
    {
      this.query = query;
      var queryColumns = new List<SqlTableColumn>();
      foreach (var queryExpression in query) {
        var select = queryExpression as SqlSelect;
        if (select == null)
          continue;
        var selectColumns = select.Columns.ToList();
        select.Columns.Clear();
        foreach (var originalColumn in selectColumns) {
          var column = originalColumn;
          var stubColumn = column as ColumnStub;
          if (!ReferenceEquals(null, stubColumn))
            column = stubColumn.Column;
          var columnRef = column as SqlColumnRef;
          if (!ReferenceEquals(null, columnRef)) {
            stubColumn = columnRef.SqlColumn as ColumnStub;
            if (!ReferenceEquals(null, stubColumn))
              column = stubColumn.Column;
          }
          select.Columns.Add(column);
          queryColumns.Add(SqlDml.TableColumn(this, originalColumn.Name));
        }
        break;
      }
      columns = new SqlTableColumnCollection(queryColumns);
    }
  }
}