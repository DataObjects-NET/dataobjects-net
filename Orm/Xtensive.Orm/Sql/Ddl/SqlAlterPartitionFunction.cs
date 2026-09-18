// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  public class SqlAlterPartitionFunction: SqlStatement,
    ISqlCompileUnit
  {
    private SqlAlterPartitionFunctionOption option;

    public PartitionFunction PartitionFunction { get; }

    public string Boundary { get; }

    public SqlAlterPartitionFunctionOption Option { get; }

    internal override SqlAlterPartitionFunction Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAlterPartitionFunction(t.PartitionFunction, t.Boundary, t.option));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterPartitionFunction(
      PartitionFunction partitionFunction, string boundary, SqlAlterPartitionFunctionOption option)
      : base(SqlNodeType.Alter)
    {
      PartitionFunction = partitionFunction;
      Boundary = boundary;
      this.option = option;
    }
  }
}