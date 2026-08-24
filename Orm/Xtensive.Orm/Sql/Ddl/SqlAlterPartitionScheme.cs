// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  public class SqlAlterPartitionScheme : SqlStatement, ISqlCompileUnit
  {
    public PartitionSchema PartitionSchema { get; }

    public string Filegroup { get; }

    internal override SqlAlterPartitionScheme Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAlterPartitionScheme(t.PartitionSchema, t.Filegroup));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterPartitionScheme(PartitionSchema partitionSchema, string filegroup)
      : base(SqlNodeType.Alter)
    {
      PartitionSchema = partitionSchema;
      Filegroup = filegroup;
    }
  }
}
