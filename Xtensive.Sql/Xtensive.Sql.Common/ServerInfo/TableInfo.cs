// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.


using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Describes a table.
  /// </summary>
  public class TableInfo : EntityInfo
  {
    private PartitionMethods partitionMethods;

    /// <summary>
    /// Gets supported partition methods.
    /// </summary>
    public PartitionMethods PartitionMethods
    {
      get { return partitionMethods; }
      set
      {
        this.EnsureNotLocked();
        partitionMethods = value;
      }
    }
  }
}
