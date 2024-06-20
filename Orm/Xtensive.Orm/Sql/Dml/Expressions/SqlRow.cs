// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlRow: SqlExpressionList
  {
    /// <inheritdoc />
    internal override SqlRow Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) => {
        var expressionsClone = new List<SqlExpression>(t.expressions.Count);
        foreach (var e in t.expressions)
          expressionsClone.Add(e.Clone(c));

        var clone = new SqlRow(expressionsClone);
        return clone;
      });


    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlRow>(expression);
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