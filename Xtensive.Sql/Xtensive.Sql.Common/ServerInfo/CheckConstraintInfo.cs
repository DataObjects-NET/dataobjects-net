// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Describes a check constraint.
  /// </summary>
  public class CheckConstraintInfo : ConstraintInfo
  {
    private int maxExpressionLength;

    /// <summary>
    /// Gets or sets the maximal length of the check expression.
    /// </summary>
    /// <value>The maximal length of the check expression.</value>
    public int MaxExpressionLength
    {
      get { return maxExpressionLength; }
      set
      {
        this.EnsureNotLocked();
        maxExpressionLength = value;
      }
    }
  }
}
