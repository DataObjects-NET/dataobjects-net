// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Represents literal value expression.
  /// </summary>
  [Serializable]
  public class SqlLiteral<T> : SqlExpression
  {
    private T value;

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    public T Value {
      get {
        return value;
      }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlLiteral<T>>(expression, "expression");
      SqlLiteral<T> replacingExpression = expression as SqlLiteral<T>;
      value = replacingExpression.Value;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      
      SqlLiteral<T> clone = new SqlLiteral<T>(value);
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal static SqlLiteral<T> Create(T value)
    {
      return new SqlLiteral<T>(value);
    }

    // Constructor

    internal SqlLiteral(T value) : base(SqlNodeType.Literal)
    {
      this.value = value;
    }
  }
}
