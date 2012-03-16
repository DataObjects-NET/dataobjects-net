// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.08.28

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Indexing.BloomFilter;

namespace Xtensive.Indexing.Implementation
{
  /// <summary>
  /// <see cref="Xtensive.Indexing.Index{TKey,TItem}"/> page provider - creates, stores and loads pages.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  public interface IIndexPageProvider<TKey, TItem> : IDisposable
  {
    /// <summary>
    /// Gets the <see cref="Index"/> this provider is bound to.
    /// </summary>
    Index<TKey, TItem> Index { get; set; }

    /// <summary>
    /// Indicates if index is already initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets index features supported by the provider.
    /// </summary>
    IndexFeatures Features { get; }

    /// <summary>
    /// Assigns (generates) an identifier to the page.
    /// </summary>
    /// <param name="page">Page to generate the identifier for.</param>
    void AssignIdentifier(Page<TKey, TItem> page);

    /// <summary>
    /// Resolves identifier by providing its owner.
    /// </summary>
    /// <param name="identifier">Identifier to resolve.</param>
    /// <returns>Identifier owner.</returns>
    Page<TKey, TItem> Resolve(IPageRef identifier);

    #region Page caching methods: AddToCache, RemoveFromCache, GetFromCache

    /// <summary>
    /// Adds the specified page to cache.
    /// </summary>
    /// <param name="page">The page.</param>
    void AddToCache(Page<TKey, TItem> page);

    /// <summary>
    /// Removes the specified page from cache.
    /// </summary>
    /// <param name="page">The page.</param>
    void RemoveFromCache(Page<TKey, TItem> page);
    
    /// <summary>
    /// Gets the page from cache by specified <paramref name="pageRef"/>.
    /// </summary>
    /// <param name="pageRef">The page reference.</param>
    /// <returns></returns>
    Page<TKey, TItem> GetFromCache(IPageRef pageRef);

    #endregion
    
    /// <summary>
    /// Flushes all cached page changes.
    /// </summary>
    void Flush();

    /// <summary>
    /// Clears the index.
    /// </summary>
    void Clear();

    /// <summary>
    /// Creates the serializer for the specified page provider.
    /// </summary>
    /// <returns>Newly created serializer.</returns>
    IIndexSerializer<TKey, TItem> CreateSerializer();

    /// <summary>
    /// Gets the bloom filter for specified <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The bloom filter.</returns>
    IBloomFilter<TKey> GetBloomFilter(IEnumerable<TItem> source);

    /// <summary>
    /// Initializes the index.
    /// </summary>
    void Initialize();
  }
}