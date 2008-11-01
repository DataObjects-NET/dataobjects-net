// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.27

using System;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Disposable;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Produces and caches <see cref="Key"/> instances. 
  /// Also acts like an identity map for <see cref="Key"/> instances within single <see cref="Domain"/>.
  /// </summary>
  public sealed class KeyManager : IDisposable
  {
    private readonly Domain domain;
    private readonly ICache<Key, Pair<Key, TypeInfo>> cache;
    internal Registry<HierarchyInfo, KeyGenerator> Generators { get; private set; }

    #region Private methods

    internal TypeInfo ResolveType(Key key)
    {
      if (key.IsResolved())
        return key.Type;

      Pair<Key, TypeInfo> result;
      if (cache.TryGetItem(key, true, out result))
        return result.Second;

      var session = Session.Current;
      if (session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is unknown. Fetch is required.", session, key);

      FieldInfo field = key.Hierarchy.Root.Fields[session.Domain.NameBuilder.TypeIdFieldName];
      Fetcher.Fetch(key, field);

      //cache.Add(new Pair<Key, TypeInfo>(key, candidate.Type));
      throw new InvalidOperationException();
    }

    #endregion


    // Constructors

    internal KeyManager(Domain domain)
    {
      this.domain = domain;
      cache = new LruCache<Key, Pair<Key, TypeInfo>>(domain.Configuration.KeyCacheSize, k => k.First);
      Generators = new Registry<HierarchyInfo, KeyGenerator>();
    }

    void IDisposable.Dispose()
    {
      if (domain.CheckItemDisposing())
        Generators.Dispose();
    }
  }
}