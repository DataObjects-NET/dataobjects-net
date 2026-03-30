// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropPartitionFunction : SqlStatement, ISqlCompileUnit
  {
    public PartitionFunction PartitionFunction { get; }

    internal override SqlDropPartitionFunction Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropPartitionFunction(t.PartitionFunction));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropPartitionFunction(PartitionFunction partitionFunction)
      : base(SqlNodeType.Drop)
    {
      PartitionFunction = partitionFunction;
    }
  }
}
