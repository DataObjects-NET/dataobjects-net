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
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlComment>(expression);
      Text = replacingExpression.Text;
    }

    internal override SqlComment Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlComment(t.Text));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public static SqlComment Join(SqlComment comment1, SqlComment comment2)
    {
      if (ReferenceEquals(comment1, comment2))
        return comment1;

      if (comment1 == null && comment2 == null)
        return null;
      if (comment1 != null) {
        if (comment2 != null)
          comment1.Text += $" {comment2.Text}";
        return comment1;
      }
      else
        return comment2;
    }

    // Constructors

    public SqlComment(string text)
      : base(SqlNodeType.Comment)
    {
      Text = text;
    }
  }
}