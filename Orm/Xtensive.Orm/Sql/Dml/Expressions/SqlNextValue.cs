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
      SqlNextValue replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlNextValue>(expression);
      sequence = replacingExpression.Sequence;
      increment = replacingExpression.Increment;
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlNextValue(sequence, increment);

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
