// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlJoinedTable : SqlTable
  {
    private SqlJoinExpression joinExpression;

    /// <summary>
    /// Gets the join expression.
    /// </summary>
    /// <value>The join expression.</value>
    public SqlJoinExpression JoinExpression
    {
      get { return joinExpression; }
    }

    /// <summary>
    /// Gets or sets the aliased columns.
    /// </summary>
    /// <value>Aliased columns.</value>
    public SqlColumnCollection AliasedColumns { get; private set; }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlJoinedTable((SqlJoinExpression)joinExpression.Clone(context));
      clone.AliasedColumns = new SqlColumnCollection(AliasedColumns);
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      joinExpression.AcceptVisitor(visitor);
    }

    /// <inheritdoc/>
    public override IEnumerator<SqlTable> GetEnumerator()
    {
      return joinExpression.GetEnumerator();
    }


    // Constructor

    internal SqlJoinedTable(SqlJoinExpression joinExpression)
    {
      this.joinExpression = joinExpression;
      var joinedColumns = joinExpression.Left.Columns.Concat(joinExpression.Right.Columns).ToList();
      columns = new SqlTableColumnCollection(joinedColumns);
      AliasedColumns = new SqlColumnCollection(columns.Cast<SqlColumn>().ToList());
    }

    internal SqlJoinedTable(SqlJoinExpression joinExpression, IEnumerable<SqlColumn> leftColumns, IEnumerable<SqlColumn> rightColumns)
      : this (joinExpression)
    {
      AliasedColumns = new SqlColumnCollection(leftColumns.Concat(rightColumns).ToList());
    }
  }
}
