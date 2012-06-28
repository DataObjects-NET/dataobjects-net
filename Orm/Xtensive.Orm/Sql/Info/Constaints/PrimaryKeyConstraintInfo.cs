// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.20

using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a primary key constraint capabilities.
  /// </summary>
  public class PrimaryKeyConstraintInfo : EntityInfo
  {
    private PrimaryKeyConstraintFeatures features = PrimaryKeyConstraintFeatures.None;

    private string constantName;

    /// <summary>
    /// Gets or sets the constant name of the primary key constraint.
    /// </summary>
    /// <remarks>This is done for mysql support only. Its' primary keys have always 'PRIMARY' name.</remarks>
    /// <value>The constant name of the primary key constraint.</value>
    public string ConstantName
    {
      get { return constantName; }
      set {
        this.EnsureNotLocked();
        constantName = value;
      }
    }
    
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