// Copyright (C) 2003-2021 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    public string Value { get; private set; }
    
    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlComment>(expression, "expression");
      var replacingExpression = (SqlComment) expression;
      Value = replacingExpression.Value;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.TryGetValue(this, out var node))
        return node;

      var clone = new SqlComment(Value);
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
    
    // Constructors

    public SqlComment(string comment)
      : base(SqlNodeType.Comment)
    {
      this.Value = comment;
    }
  }
}