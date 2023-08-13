// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreatePartitionFunction : SqlStatement, ISqlCompileUnit
  {
    private PartitionFunction partitionFunction;

    public PartitionFunction PartitionFunction {
      get {
        return partitionFunction;
      }
    }

    internal override SqlCreatePartitionFunction Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreatePartitionFunction(t.partitionFunction));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreatePartitionFunction(PartitionFunction partitionFunction)
      : base(SqlNodeType.Create)
    {
      this.partitionFunction = partitionFunction;
    }
  }
}
