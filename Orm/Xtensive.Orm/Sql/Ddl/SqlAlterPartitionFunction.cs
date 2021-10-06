// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlAlterPartitionFunction: SqlStatement,
    ISqlCompileUnit
  {
    private PartitionFunction partitionFunction;
    private string boundary;
    private SqlAlterPartitionFunctionOption option;

    public PartitionFunction PartitionFunction
    {
      get { return partitionFunction; }
    }

    public string Boundary
    {
      get { return boundary; }
    }

    public SqlAlterPartitionFunctionOption Option
    {
      get { return option; }
      set { option = value; }
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlAlterPartitionFunction(partitionFunction, boundary, option);

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterPartitionFunction(PartitionFunction partitionFunction, string boundary,
                                       SqlAlterPartitionFunctionOption option)
      : base(SqlNodeType.Alter)
    {
      this.partitionFunction = partitionFunction;
      this.boundary = boundary;
      this.option = option;
    }
  }
}