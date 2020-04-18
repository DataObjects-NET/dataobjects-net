// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropIndex : SqlStatement, ISqlCompileUnit
  {
    private Index index;
    //private bool? online;
    //private byte? maxDegreeOfParallelism;
    //private IPartitionDescriptor partitioningDescriptor;
    //private string tableSpace;

    public Index Index {
      get {
        return index;
      }
    }

    //public bool? Online {
    //  get {
    //    return online;
    //  }
    //  set {
    //    online = value;
    //  }
    //}

    //public byte? MaxDegreeOfParallelism {
    //  get {
    //    return maxDegreeOfParallelism;
    //  }
    //  set {
    //    maxDegreeOfParallelism = value;
    //  }
    //}

    //public IPartitionDescriptor PartitionDescriptor {
    //  get {
    //    return partitioningDescriptor;
    //  }
    //  set {
    //    partitioningDescriptor = value;
    //  }
    //}

    //public string Filegroup {
    //  get {
    //    return tableSpace;
    //  }
    //  set {
    //    tableSpace = value;
    //  }
    //}

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      
      SqlDropIndex clone = new SqlDropIndex(index);
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropIndex(Index index)
      : base(SqlNodeType.Drop)
    {
      this.index = index;
    }
  }
}
