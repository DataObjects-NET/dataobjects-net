// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlNextValue>(expression);
      sequence = replacingExpression.Sequence;
      increment = replacingExpression.Increment;
    }

    internal override SqlNextValue Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlNextValue(t.sequence, t.increment));

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
