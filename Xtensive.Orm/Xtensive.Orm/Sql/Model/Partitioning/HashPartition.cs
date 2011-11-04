// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a hash partition.
  /// </summary>
  [Serializable]
  public class HashPartition : Partition
  {
    #region Constructors

    internal HashPartition(PartitionDescriptor partitionDescriptor, string filegroup) : base(partitionDescriptor, filegroup)
    {
    }

    #endregion
  }
}
