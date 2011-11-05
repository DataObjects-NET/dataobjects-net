// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using Xtensive.Orm;

namespace Xtensive.Orm.Providers.Indexing.Memory
{
  /// <summary>
  /// <see cref="Domain"/>-level handler for memory index storage.
  /// </summary>
  public class DomainHandler : Indexing.DomainHandler
  {
    /// <inheritdoc/>
    protected override IndexStorage CreateLocalStorage(string name)
    {
      return new MemoryIndexStorage(name);
    }
  }
}