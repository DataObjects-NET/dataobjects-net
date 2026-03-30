// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents order specification.
  /// </summary>
  [Serializable]
  public class SqlOrder : SqlNode
  {
    /// <summary>
    /// Gets the expression to sort by.
    /// </summary>
    /// <value>The expression.</value>
    public SqlExpression Expression { get; private set; }

    /// <summary>
    /// Gets the position of column to sort by.
    /// </summary>
    /// <value>The position.</value>
    public int Position { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="SqlOrder"/> is ascending.
    /// </summary>
    /// <value><see langword="true"/> if ascending; otherwise, <see langword="false"/>.</value>
    public bool Ascending { get; private set; }

    internal override SqlOrder Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        t.Expression is null
            ? new SqlOrder(t.Position, t.Ascending)
            : new SqlOrder(t.Expression.Clone(c), t.Ascending));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlOrder(SqlExpression expression, bool ascending)
      : base(SqlNodeType.Order)
    {
      Expression = expression;
      Ascending = ascending;
    }

    internal SqlOrder(int position, bool ascending) : base(SqlNodeType.Order)
    {
      Position = position;
      Ascending = ascending;
    }
  }
}
