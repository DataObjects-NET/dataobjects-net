// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.ObjectModel;

namespace Xtensive.Sql.Dom.Dml
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
      SqlSelect ss = query as SqlSelect;
      if (ss != null)
        clone = new SqlQueryRef((SqlSelect)ss.Clone(context), Name);
      else {
        SqlQueryExpression qe = (SqlQueryExpression) query;
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
      Collection<SqlTableColumn> queryColumns = new Collection<SqlTableColumn>();
      foreach (ISqlQueryExpression queryExpression in query) {
        SqlSelect select = queryExpression as SqlSelect;
        if (select == null)
          continue;
        foreach (SqlColumn column in select.Columns)
          if (string.IsNullOrEmpty(column.Name))
            queryColumns.Add(Sql.TableColumn(this));
          else
            queryColumns.Add(Sql.TableColumn(this, column.Name));
        break;
      }
      columns = new SqlTableColumnCollection(queryColumns);
    }
  }
}