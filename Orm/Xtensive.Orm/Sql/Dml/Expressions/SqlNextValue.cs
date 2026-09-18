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
  public class SqlNextValue : SqlExpression
  {

    /// <summary>
    /// Gets the increment.
    /// </summary>
    /// <value>The increment.</value>
    public int Increment { get; private set; } = 1;

    /// <summary>
    /// Gets the sequence.
    /// </summary>
    /// <value>The sequence.</value>
    public Sequence Sequence { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlNextValue>(expression);
      Sequence = replacingExpression.Sequence;
      Increment = replacingExpression.Increment;
    }

    internal override SqlNextValue Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlNextValue(t.Sequence, t.Increment));

    internal SqlNextValue(Sequence sequence) : base(SqlNodeType.NextValue)
    {
      Sequence = sequence;
    }

    internal SqlNextValue(Sequence sequence, int increment) : base(SqlNodeType.NextValue)
    {
      Sequence = sequence;
      Increment = increment;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
