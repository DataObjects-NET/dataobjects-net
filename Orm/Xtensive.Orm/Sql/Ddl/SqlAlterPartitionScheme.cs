// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

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

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlAlterPartitionScheme(partitionSchema, filegroup);

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
