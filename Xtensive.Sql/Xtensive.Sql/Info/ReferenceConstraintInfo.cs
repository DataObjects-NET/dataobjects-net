// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.


using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a reference constraint.
  /// </summary>
  public class ReferenceConstraintInfo : ConstraintInfo
  {
    private ConstraintActions actions = ConstraintActions.None;

    /// <summary>
    /// Gets or sets the constraint rules.
    /// </summary>
    /// <value>The rules.</value>
    public ConstraintActions Actions
    {
      get { return actions; }
      set
      {
        this.EnsureNotLocked();
        actions = value;
      }
    }
  }
}
