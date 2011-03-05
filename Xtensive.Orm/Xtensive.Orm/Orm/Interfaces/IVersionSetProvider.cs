// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.07.21

using System.Collections.Generic;

namespace Xtensive.Orm
{
  /// <summary>
  /// <see cref="VersionSet"/> provider contract.
  /// </summary>
  public interface IVersionSetProvider
  {
    /// <summary>
    /// Creates <see cref="VersionSet"/> set containing versions
    /// for specified <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">The keys to create version set for.</param>
    /// <returns><see cref="VersionSet"/> containing versions
    /// for specified <paramref name="keys"/>.</returns>
    VersionSet CreateVersionSet(IEnumerable<Key> keys);
  }
}