// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents sub query expression.
  /// </summary>
  [Serializable]
  public class SqlSubQuery: SqlExpression
  {
    private ISqlQueryExpression query;

    /// <summary>
    /// Gets the query.
    /// </summary>
    /// <value>The query.</value>
    public ISqlQueryExpression Query
    {
      get { return query; }
    }

    /// <inheritdoc/>
    public override void ReplaceWith(SqlExpression expression)
    {
      SqlSubQuery replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlSubQuery>(expression);
      query = replacingExpression.Query;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.TryGetValue(this, out var value)) {
        return value;
      }

      SqlSubQuery clone;
      SqlSelect select = query as SqlSelect;
      SqlQueryExpression expression = query as SqlQueryExpression;
      if (select != null)
        clone = new SqlSubQuery((SqlSelect)select.Clone(context));
      else 
        clone = new SqlSubQuery((SqlQueryExpression)expression.Clone(context));
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlSubQuery(ISqlQueryExpression query)
      : base(SqlNodeType.SubSelect)
    {
      this.query = query;
    }
  }
}