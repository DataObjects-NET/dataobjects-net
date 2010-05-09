// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a reference constraint capabilities.
  /// </summary>
  public class ForeignKeyConstraintInfo : EntityInfo
  {
    private ForeignKeyConstraintFeatures features = ForeignKeyConstraintFeatures.None;
    private ForeignKeyConstraintActions actions = ForeignKeyConstraintActions.None;
    
    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public ForeignKeyConstraintFeatures Features {
      get { return features; }
      set {
        this.EnsureNotLocked();
        features = value;
      }
    }

    /// <summary>
    /// Gets or sets the constraint rules.
    /// </summary>
    /// <value>The rules.</value>
    public ForeignKeyConstraintActions Actions {
      get { return actions; }
      set {
        this.EnsureNotLocked();
        actions = value;
      }
    }
  }
}
