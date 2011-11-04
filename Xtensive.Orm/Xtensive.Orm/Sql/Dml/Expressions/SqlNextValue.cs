// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents NEXT VALUE FOR expression.
  /// </summary>
  [Serializable]
  public class SqlNextValue : SqlExpression
  {
    private Sequence sequence;
    private int increment = 1;

    /// <summary>
    /// Gets the increment.
    /// </summary>
    /// <value>The increment.</value>
    public int Increment
    {
      get { return increment; }
    }

    /// <summary>
    /// Gets the sequence.
    /// </summary>
    /// <value>The sequence.</value>
    public Sequence Sequence
    {
      get { return sequence; }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlNextValue>(expression, "expression");
      SqlNextValue replacingExpression = expression as SqlNextValue;
      sequence = replacingExpression.Sequence;
      increment = replacingExpression.Increment;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlNextValue clone = new SqlNextValue(sequence, increment);
      context.NodeMapping[this] = clone;
      return clone;
    }

    internal SqlNextValue(Sequence sequence) : base(SqlNodeType.NextValue)
    {
      this.sequence = sequence;
    }

    internal SqlNextValue(Sequence sequence, int increment) : base(SqlNodeType.NextValue)
    {
      this.sequence = sequence;
      this.increment = increment;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
