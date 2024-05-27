// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlDropIndex : SqlStatement, ISqlCompileUnit
  {
    //private bool? online;
    //private byte? maxDegreeOfParallelism;
    //private IPartitionDescriptor partitioningDescriptor;
    //private string tableSpace;

    public Index Index { get; }

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

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlDropIndex(Index);

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlDropIndex(Index index)
      : base(SqlNodeType.Drop)
    {
      Index = index;
    }
  }
}
