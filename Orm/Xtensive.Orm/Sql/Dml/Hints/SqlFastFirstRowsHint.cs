// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlFastFirstRowsHint : SqlHint
  {
    /// <summary>
    /// Gets the rows amount.
    /// </summary>
    /// <value>The row amount.</value>
    public int Amount { get; private set; }

    internal override SqlFastFirstRowsHint Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlFastFirstRowsHint(t.Amount));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlFastFirstRowsHint(int amount)
    {
      Amount = amount;
    }
  }
}
