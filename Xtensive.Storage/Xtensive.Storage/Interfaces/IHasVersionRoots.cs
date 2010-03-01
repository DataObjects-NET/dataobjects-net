// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.07

using System.Collections.Generic;

namespace Xtensive.Storage
{
  /// <summary>
  /// Should be implemented by any entities thats has version roots.
  /// </summary>
  public interface IHasVersionRoots
  {
    /// <summary>
    /// Gets the version roots instances.
    /// </summary>
    /// <returns>Version root sequence.</returns>
    IEnumerable<Entity> GetVersionRoots();
  }
}