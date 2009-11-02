// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.


using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a constraint.
  /// </summary>
  public class ConstraintInfo: EntityInfo
  {
    private ConstraintFeatures features = ConstraintFeatures.None;

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public ConstraintFeatures Features
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