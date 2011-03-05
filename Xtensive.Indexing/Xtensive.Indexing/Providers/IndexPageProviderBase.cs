// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.13

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.BloomFilter;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Providers
{
  /// <summary>
  /// Default base class for <see cref="Xtensive.Indexing.Index{TKey,TItem}"/> page providers.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  public abstract class IndexPageProviderBase<TKey, TItem> : IIndexPageProvider<TKey, TItem>
  {
    protected Index<TKey, TItem> index;
    private bool isInitialized;


    /// <inheritdoc/>
    public Index<TKey, TItem> Index
    {
      [DebuggerStepThrough]
      get { return index; }
      set
      {
        if (index!=null)
          throw Exceptions.AlreadyInitialized("Index");
        index = value;
      }
    }

    /// <inheritdoc/>
    public bool IsInitialized
    {
      [DebuggerStepThrough]
      get { return isInitialized; }
    }

    /// <inheritdoc/>
    public virtual IndexFeatures Features
    {
      [DebuggerStepThrough]
      get { return IndexFeatures.Default; }
    }

    // Abstract methods

    /// <inheritdoc/>
    public abstract IBloomFilter<TKey> GetBloomFilter(IEnumerable<TItem> source);

    /// <inheritdoc/>
    public abstract void AssignIdentifier(Page<TKey, TItem> page);

    /// <inheritdoc/>
    public abstract Page<TKey, TItem> Resolve(IPageRef identifier);

    #region Page caching methods: AddToCache, RemoveFromCache, GetFromCache

    /// <inheritdoc/>
    public abstract void AddToCache(Page<TKey, TItem> page);

    /// <inheritdoc/>
    public abstract void RemoveFromCache(Page<TKey, TItem> page);

    /// <inheritdoc/>
    public abstract Page<TKey, TItem> GetFromCache(IPageRef pageRef);

    #endregion

    /// <inheritdoc/>
    public abstract void Flush();

    /// <inheritdoc/>
    public virtual void Clear()
    {
      index.DescriptorPage.Clear();
    }

    /// <inheritdoc/>
    public abstract IIndexSerializer<TKey, TItem> CreateSerializer();

    /// <inheritdoc/>
    public virtual void Initialize()
    {
      if (isInitialized)
        throw new InvalidOperationException(Strings.ExIndexIsAlreadyInitialized);
      isInitialized = true;
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public virtual void Dispose()
    {
      index = null;
    }
  }
}