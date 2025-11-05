// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlCreatePartitionFunction : SqlStatement, ISqlCompileUnit
  {
    public PartitionFunction PartitionFunction { get; }

    internal override SqlCreatePartitionFunction Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCreatePartitionFunction(t.PartitionFunction));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCreatePartitionFunction(PartitionFunction partitionFunction)
      : base(SqlNodeType.Create)
    {
      PartitionFunction = partitionFunction;
    }
  }
}
