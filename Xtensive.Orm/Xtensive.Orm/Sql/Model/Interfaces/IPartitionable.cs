// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Defines an object that supports partitioning.
  /// </summary>
  public interface IPartitionable
  {
    /// <summary>
    /// Gets or sets the partition descriptor.
    /// </summary>
    /// <value>The partition descriptor.</value>
    PartitionDescriptor PartitionDescriptor { get; set; }
  }
}