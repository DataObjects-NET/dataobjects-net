// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public class SqlUsePlanHint : SqlHint
  {
    private string plan;

    /// <summary>
    /// Gets the execution plan.
    /// </summary>
    /// <value>The execution plan.</value>
    public string Plan
    {
      get { return plan; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlUsePlanHint clone = new SqlUsePlanHint(plan);

      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlUsePlanHint(string plan)
    {
      this.plan = plan;
    }
  }
}
