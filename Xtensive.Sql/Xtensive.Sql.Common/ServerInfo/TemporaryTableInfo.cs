// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.


using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Describes a temporary table.
  /// </summary>
  public class TemporaryTableInfo : EntityInfo
  {
    private TemporaryTableFeatures features;

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public TemporaryTableFeatures Features
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
