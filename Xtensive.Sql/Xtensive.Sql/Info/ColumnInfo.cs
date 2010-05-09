// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a table column.
  /// </summary>
  public class ColumnInfo : EntityInfo
  {
    private ColumnFeatures features = ColumnFeatures.None;

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public ColumnFeatures Features {
      get { return features; }
      set {
        this.EnsureNotLocked();
        features = value;
      }
    }
  }
}
