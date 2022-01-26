// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropPartitionScheme : SqlStatement, ISqlCompileUnit
  {
    private PartitionSchema partitionSchema;

    public PartitionSchema PartitionSchema {
      get {
        return partitionSchema;
      }
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlDropPartitionScheme(partitionSchema);

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropPartitionScheme(PartitionSchema partitionSchema)
      : base(SqlNodeType.Drop)
    {
      this.partitionSchema = partitionSchema;
    }
  }
}
