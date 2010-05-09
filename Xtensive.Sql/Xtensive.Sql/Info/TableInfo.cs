// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a table.
  /// </summary>
  public class TableInfo : EntityInfo
  {
    private PartitionMethods partitionMethods;
    private int maxNumberOfColumns;

    /// <summary>
    /// Gets supported partition methods.
    /// </summary>
    public PartitionMethods PartitionMethods {
      get { return partitionMethods; }
      set {
        this.EnsureNotLocked();
        partitionMethods = value;
      }
    }

    /// <summary>
    /// Gets or sets the maximum number of columns per table.
    /// </summary>
    public int MaxNumberOfColumns {
      get { return maxNumberOfColumns; }
      set {
        this.EnsureNotLocked();
        maxNumberOfColumns = value;
      }
    }
  }
}
