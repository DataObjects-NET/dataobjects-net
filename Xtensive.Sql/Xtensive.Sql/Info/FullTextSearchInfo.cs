// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a full-text capabilities of a database server.
  /// </summary>
  public class FullTextSearchInfo : EntityInfo
  {
    private FullTextSearchFeatures features = FullTextSearchFeatures.None;

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public FullTextSearchFeatures Features {
      get { return features; }
      set {
        this.EnsureNotLocked();
        features = value;
      }
    }
  }
}
