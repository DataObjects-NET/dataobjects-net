// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes an identity source.
  /// </summary>
  public class IdentityInfo : LockableBase
  {
    private IdentityFeatures features = IdentityFeatures.None;

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public IdentityFeatures Features {
      get { return features; }
      set {
        this.EnsureNotLocked();
        features = value;
      }
    }
  }
}
