// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Describes an index.
  /// </summary>
  public class IndexInfo : EntityInfo
  {
    private IndexFeatures features = IndexFeatures.None;
    private int maxLength;
    private int maxColumnAmount;
    private PartitionMethods partitionMethods;

    /// <summary>
    /// Gets or sets the maximal index length in bytes.
    /// </summary>
    public int MaxLength
    {
      get { return maxLength; }
      set
      {
        this.EnsureNotLocked();
        maxLength = value;
      }
    }

    /// <summary>
    /// Gets or sets the maximal amount of columns in index.
    /// </summary>
    public int MaxColumnAmount
    {
      get { return maxColumnAmount; }
      set
      {
        this.EnsureNotLocked();
        maxColumnAmount = value;
      }
    }

    /// <summary>
    /// Gets or sets supported partition methods.
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

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public IndexFeatures Features
    {
      get { return features; }
      set
      {
        this.EnsureNotLocked();
        features = value;
      }
    }
  }
}
