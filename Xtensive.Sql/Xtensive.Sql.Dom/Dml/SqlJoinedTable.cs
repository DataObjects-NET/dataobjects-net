// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Dml
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

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlJoinedTable clone = new SqlJoinedTable((SqlJoinExpression)joinExpression.Clone(context));
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
    }
  }
}
