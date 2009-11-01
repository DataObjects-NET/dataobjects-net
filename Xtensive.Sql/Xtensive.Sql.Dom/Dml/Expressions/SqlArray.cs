// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Represents generic array expression.
  /// </summary>
  [Serializable]
  public class SqlArray<T> : SqlExpression
  {
    private T[] values;

    /// <summary>
    /// Gets the values.
    /// </summary>
    /// <values>The values.</values>
    public T[] Values {
      get {
        return values;
      }
    }

    public static implicit operator SqlArray<T>(T[] value)
    {
      return new SqlArray<T>(value);
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlArray<T>>(expression, "expression");
      SqlArray<T> replacingExpression = expression as SqlArray<T>;
      values = replacingExpression.Values;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      
      SqlArray<T> clone = new SqlArray<T>((T [])Values.Clone());

      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructor

    internal SqlArray(T[] values) : base(SqlNodeType.Array)
    {
      this.values = values;
    }
  }
}
