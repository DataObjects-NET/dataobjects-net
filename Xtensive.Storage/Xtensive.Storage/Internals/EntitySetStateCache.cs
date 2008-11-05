// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.05

using Xtensive.Core.Caching;

namespace Xtensive.Storage.Internals
{
  public sealed class EntitySetStateCache
  {
    public long? Count;
    public ICache<Key, Key> ExistingKeys;


    // Constructors

    public EntitySetStateCache(long cacheSize)
    {
      ExistingKeys = new LruCache<Key, Key>(cacheSize, cachedKey => cachedKey);
    }
  }
}