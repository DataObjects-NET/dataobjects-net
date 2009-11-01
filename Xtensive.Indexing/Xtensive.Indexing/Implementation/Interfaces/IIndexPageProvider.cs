// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.08.28

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Indexing.BloomFilter;

namespace Xtensive.Indexing.Implementation
{
  /// <summary>
  /// <see cref="Index{TKey,TItem}"/> page provider - creates, stores and loads pages.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  public interface IIndexPageProvider<TKey, TItem> : IDisposable,
    IIdentifierResolver<IPageRef, Page<TKey, TItem>>
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
    /// Flushes all cached page changes.
    /// </summary>
    void Flush();

    /// <summary>
    /// Clears the index.
    /// </summary>
    void Clear();

    /// <summary>
    /// Serializes specified <paramref name="source"/> into the index.
    /// </summary>
    /// <param name="source">An enumerable enumerating key-value pairs to serialize. 
    /// To use <see cref="BloomFilter{T}"/>, enumerable must be <see cref="ICountable"/> or <see cref="ICollection"/> 
    /// or <see cref="ICollection{T}"/> in order to provide <see langword="Count"/> property.</param>
    void Serialize(IEnumerable<TItem> source);

    /// <summary>
    /// Initializes the index.
    /// </summary>
    void Initialize();
  }
}