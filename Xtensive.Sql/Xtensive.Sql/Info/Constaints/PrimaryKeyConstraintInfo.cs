// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.20

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a primary key constaint capabilities.
  /// </summary>
  public class PrimaryKeyConstraintInfo : EntityInfo
  {
    private PrimaryKeyConstraintFeatures features = PrimaryKeyConstraintFeatures.None;
    
    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public PrimaryKeyConstraintFeatures Features {
      get { return features; }
      set {
        this.EnsureNotLocked();
        features = value;
      }
    }
  }
}