// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropPartitionScheme : SqlStatement, ISqlCompileUnit
  {
    public PartitionSchema PartitionSchema { get; }

    internal override SqlDropPartitionScheme Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlDropPartitionScheme(t.PartitionSchema));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropPartitionScheme(PartitionSchema partitionSchema)
      : base(SqlNodeType.Drop)
    {
      PartitionSchema = partitionSchema;
    }
  }
}
