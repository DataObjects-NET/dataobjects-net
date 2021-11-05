// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlRow: SqlExpressionList
  {
    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.TryGetValue(this, out var value)) {
        return value;
      }

      var expressionsClone = new Collection<SqlExpression>();
      foreach (var e in expressions)
        expressionsClone.Add((SqlExpression) e.Clone(context));

      var clone = new SqlRow(expressionsClone);
      return clone;
    }


    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlRow>(expression, "expression");
      var replacingExpression = (SqlRow) expression;
      expressions.Clear();
      foreach (SqlExpression e in replacingExpression)
        expressions.Add(e);
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }


    // Constructors

    internal SqlRow(IList<SqlExpression> expressions)
      : base(SqlNodeType.Row, expressions)
    {
      this.expressions = expressions;
    }
  }
}