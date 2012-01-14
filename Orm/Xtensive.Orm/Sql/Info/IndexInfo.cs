// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes an index.
  /// </summary>
  public class IndexInfo : EntityInfo
  {
    private IndexFeatures features = IndexFeatures.None;
    private int maxLength;
    private int maxNumberOfColumns;
    private PartitionMethods partitionMethods;

    /// <summary>
    /// Gets or sets the maximal index length in bytes.
    /// </summary>
    public int MaxLength {
      get { return maxLength; }
      set {
        this.EnsureNotLocked();
        maxLength = value;
      }
    }

    /// <summary>
    /// Gets or sets the maximum number of columns per index.
    /// </summary>
    public int MaxNumberOfColumns {
      get { return maxNumberOfColumns; }
      set {
        this.EnsureNotLocked();
        maxNumberOfColumns = value;
      }
    }

    /// <summary>
    /// Gets or sets supported partition methods.
    /// </summary>
    public PartitionMethods PartitionMethods {
      get { return partitionMethods; }
      set {
        this.EnsureNotLocked();
        partitionMethods = value;
      }
    }

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public IndexFeatures Features {
      get { return features; }
      set {
        this.EnsureNotLocked();
        features = value;
      }
    }
  }
}
