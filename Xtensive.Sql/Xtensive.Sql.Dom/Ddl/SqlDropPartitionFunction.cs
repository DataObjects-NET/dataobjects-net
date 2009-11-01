// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Ddl
{
  [Serializable]
  public class SqlDropPartitionFunction : SqlStatement, ISqlCompileUnit
  {
    private PartitionFunction partitionFunction;

    public PartitionFunction PartitionFunction {
      get {
        return partitionFunction;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlDropPartitionFunction clone = new SqlDropPartitionFunction(partitionFunction);
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropPartitionFunction(PartitionFunction partitionFunction)
      : base(SqlNodeType.Drop)
    {
      this.partitionFunction = partitionFunction;
    }
  }
}
