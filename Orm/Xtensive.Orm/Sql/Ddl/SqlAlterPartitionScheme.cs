// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlAlterPartitionScheme : SqlStatement, ISqlCompileUnit
  {
    private readonly PartitionSchema partitionSchema;
    private readonly string filegroup;

    public PartitionSchema PartitionSchema => partitionSchema;

    public string Filegroup => filegroup;

    internal override SqlAlterPartitionScheme Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAlterPartitionScheme(t.partitionSchema, t.filegroup));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlAlterPartitionScheme(PartitionSchema partitionSchema, string filegroup)
      : base(SqlNodeType.Alter)
    {
      this.partitionSchema = partitionSchema;
      this.filegroup = filegroup;
    }
  }
}
