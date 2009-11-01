// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public class SqlFastFirstRowsHint : SqlHint
  {
    private int amount;

    /// <summary>
    /// Gets the rows amount.
    /// </summary>
    /// <value>The row amount.</value>
    public int Amount
    {
      get { return amount; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlFastFirstRowsHint clone = new SqlFastFirstRowsHint(Amount);

      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlFastFirstRowsHint(int amount)
    {
      this.amount = amount;
    }
  }
}
