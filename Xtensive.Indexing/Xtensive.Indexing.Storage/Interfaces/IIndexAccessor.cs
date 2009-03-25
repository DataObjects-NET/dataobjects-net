// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System.Collections.Generic;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing.Storage
{
  /// <summary>
  /// Index accessor API (DML API).
  /// Provides read-write access to any index 
  /// in the <see cref="IStorage"/>, allows to 
  /// execute queries on them.
  /// </summary>
  public interface IIndexAccessor
  {
    /// <summary>
    /// Gets the index by its name.
    /// </summary>
    /// <param name="name">The name of the index to get.</param>
    /// <returns>The index with specified name.</returns>
    IIndex<Tuple, Tuple> GetIndex(string name);

    /// <summary>
    /// Queries the storage.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>Query result.</returns>
    object Query(object query);
  }
}