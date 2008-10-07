// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.27

using System;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposable;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Produces and caches <see cref="Key"/> instances. 
  /// Also acts like an identity map for <see cref="Key"/> instances within single <see cref="Domain"/>.
  /// </summary>
  public sealed class KeyManager : IDisposable
  {
    private readonly Domain domain;
    private readonly object _lock = new object();
    private readonly ICache<Key, Key> cache;
    internal Registry<HierarchyInfo, KeyGenerator> Generators { get; private set; }

    #region Public methods

    /// <summary>
    /// Builds the <see cref="Key"/> according to specified <paramref name="tuple"/>.
    /// </summary>
    /// <param name="type">The type of <see cref="Entity"/> descendant to create a <see cref="Key"/> for.</param>
    /// <param name="tuple"><see cref="Tuple"/> with key values.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="type"/> is not <see cref="Entity"/> descendant.</exception>
    public Key Get(Type type, Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (!type.IsSubclassOf(typeof (Entity)))
        throw new ArgumentException(Strings.ExTypeMustBeEntityDescendant, "type");
      if (tuple.ContainsEmptyValues())
        throw new InvalidOperationException(string.Format("Cannot create Key from tuple: '{0}'", tuple.ToRegular()));

      TypeInfo typeInfo = domain.Model.Types[type];
      Key key = CreateKeyCandidate(typeInfo.Hierarchy, tuple);
      return GetCachedKey(key);
    }

    #endregion

    #region Internal methods

    internal Key Next(TypeInfo type)
    {
      KeyGenerator keyGenerator = Generators[type.Hierarchy];
      return GetCachedKey(CreateKey(type, keyGenerator.Next()));
    }

    internal Key Get(TypeInfo type, Tuple tuple)
    {
      return GetCachedKey(CreateKey(type, tuple));
    }

    internal Key Get(FieldInfo field, Tuple tuple)
    {
      // Tuple with empty values is treated as empty Entity reference
      if (tuple.ContainsEmptyValues())
        return null;

      TypeInfo type = domain.Model.Types[field.ValueType];
      Key result = CreateKeyCandidate(type.Hierarchy, tuple);
      return GetCachedKey(result);
    }

    internal Key GetCachedKey(Key key)
    {
      return LockType.Exclusive.Execute(_lock, () => Cache(key));
    }

    #endregion

    #region Private methods

    private Key CreateKey(TypeInfo type, Tuple tuple)
    {
      Key key = CreateKeyCandidate(type.Hierarchy, tuple);
      key.Type = type;

      if (domain.IsDebugEventLoggingEnabled)
        Log.Debug("Creating key '{0}'", key);

      return key;
    }

    private static Key CreateKeyCandidate(HierarchyInfo hierarchy, Tuple tuple)
    {
      Tuple keyTuple = Tuple.Create(hierarchy.KeyTupleDescriptor);
      tuple.CopyTo(keyTuple, 0, keyTuple.Count);
      return new Key(hierarchy, keyTuple);
    }

    private Key Cache(Key candidate)
    {
      Key cached = cache[candidate, true];
      if (cached==null) {
        cache.Add(candidate);
        return candidate;
      }

      // Updating type property
      if (cached.Type==null && candidate.Type!=null)
        cached.Type = candidate.Type;

      return cached;
    }

    #endregion


    // Constructors

    internal KeyManager(Domain domain)
    {
      this.domain = domain;
      cache = new LruCache<Key, Key>(domain.Configuration.KeyCacheSize, k => k,
        new WeakestCache<Key, Key>(false, false, k => k));
      Generators = new Registry<HierarchyInfo, KeyGenerator>();
    }

    void IDisposable.Dispose()
    {
      if (domain.CheckItemDisposing())
        Generators.Dispose();
    }
  }
}