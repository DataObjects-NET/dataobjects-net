// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Collections.Generic;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a node that is capable of being constrained.
  /// </summary>
  public interface IConstrainable
  {
    /// <summary>
    /// Gets the node constraints.
    /// </summary>
    /// <value>The constraints.</value>
    IList<Constraint> Constraints { get; }
  }
}
