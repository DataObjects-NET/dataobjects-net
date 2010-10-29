// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.07

using System.Collections.Generic;

namespace Xtensive.Orm
{
  /// <summary>
  /// Should be implemented by entities that have version roots.
  /// </summary>
  public interface IHasVersionRoots
  {
    /// <summary>
    /// Gets the sequence of entity's version roots.
    /// </summary>
    IEnumerable<Entity> GetVersionRoots();
  }
}