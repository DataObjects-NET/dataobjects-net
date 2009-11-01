// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Represents NEXT VALUE FOR expression.
  /// </summary>
  [Serializable]
  public class SqlNextValue : SqlExpression
  {
    private Sequence sequence;

    /// <summary>
    /// Gets the sequence.
    /// </summary>
    /// <value>The sequence.</value>
    public Sequence Sequence {
      get {
        return sequence;
      }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlNextValue>(expression, "expression");
      SqlNextValue replacingExpression = expression as SqlNextValue;
      sequence = replacingExpression.Sequence;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlNextValue clone = new SqlNextValue(sequence);
      context.NodeMapping[this] = clone;
      return clone;
    }

    internal SqlNextValue(Sequence sequence) : base(SqlNodeType.NextValue)
    {
      this.sequence = sequence;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
