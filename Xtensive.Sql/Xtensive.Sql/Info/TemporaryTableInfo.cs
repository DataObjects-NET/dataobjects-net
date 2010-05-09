// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a temporary table.
  /// </summary>
  public class TemporaryTableInfo : EntityInfo
  {
    private TemporaryTableFeatures features;
    private int maxNumberOfColumns;

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public TemporaryTableFeatures Features {
      get { return features; }
      set {
        this.EnsureNotLocked();
        features = value;
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
