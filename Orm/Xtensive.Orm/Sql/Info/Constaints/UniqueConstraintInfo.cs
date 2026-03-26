// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.20

using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a unique constaint capabilities
  /// </summary>
  public class UniqueConstraintInfo : EntityInfo
  {
    private UniqueConstraintFeatures features = UniqueConstraintFeatures.None;
    
    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public UniqueConstraintFeatures Features {
      get { return features; }
      set {
        EnsureNotLocked();
        features = value;
      }
    }
  }
}