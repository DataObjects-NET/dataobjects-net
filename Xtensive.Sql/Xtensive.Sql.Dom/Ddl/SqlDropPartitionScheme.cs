// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Ddl
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

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlDropPartitionScheme clone = new SqlDropPartitionScheme(partitionSchema);
      context.NodeMapping[this] = clone;

      return clone;
    }

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
