// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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

    internal override SqlAlterPartitionFunction Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAlterPartitionFunction(t.partitionFunction, t.boundary, t.option));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterPartitionFunction(
      PartitionFunction partitionFunction, string boundary, SqlAlterPartitionFunctionOption option)
      : base(SqlNodeType.Alter)
    {
      this.partitionFunction = partitionFunction;
      this.boundary = boundary;
      this.option = option;
    }
  }
}