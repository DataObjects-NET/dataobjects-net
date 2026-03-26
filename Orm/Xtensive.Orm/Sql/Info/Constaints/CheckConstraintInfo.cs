// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a check constraint capabilities.
  /// </summary>
  public class CheckConstraintInfo : EntityInfo
  {
    private int maxExpressionLength;
    private CheckConstraintFeatures features = CheckConstraintFeatures.None;

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public CheckConstraintFeatures Features {
      get { return features; }
      set {
        EnsureNotLocked();
        features = value;
      }
    }

    /// <summary>
    /// Gets or sets the maximal length of the check expression.
    /// </summary>
    /// <value>The maximal length of the check expression.</value>
    public int MaxExpressionLength {
      get { return maxExpressionLength; }
      set {
        EnsureNotLocked();
        maxExpressionLength = value;
      }
    }
  }
}
