// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Edgar Isajanyan
// Created:    2021.09.13

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlComment : SqlExpression
  {
    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Text { get; private set; }
    
    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, nameof(expression));
      ArgumentValidator.EnsureArgumentIs<SqlComment>(expression, nameof(expression));
      var replacingExpression = (SqlComment) expression;
      Text = replacingExpression.Text;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.TryGetValue(this, out var node))
        return node;

      var clone = new SqlComment(Text);
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
    
    // Constructors

    public SqlComment(string text)
      : base(SqlNodeType.Comment)
    {
      Text = text;
    }
  }
}