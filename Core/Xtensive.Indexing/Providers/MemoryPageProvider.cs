// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.08.28

using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Providers.Internals;
using Xtensive.Indexing.BloomFilter;


namespace Xtensive.Indexing.Providers
{
  /// <summary>
  /// Memory <see cref="IIndexPageProvider{TKey,TItem}"/> which provides
  /// <see cref="Index{TKey,TItem}"/> with in-memory stored pages.
  /// </summary>
  /// <typeparam name="TKey">Key type of the page nodes.</typeparam>
  /// <typeparam name="TItem">Node type.</typeparam>
  public sealed class MemoryPageProvider<TKey, TItem> : IndexPageProviderBase<TKey, TItem>
  {
    /// <inheritdoc/>
    public override IBloomFilter<TKey> GetBloomFilter(IEnumerable<TItem> source)
    {
      return null;
    }

    /// <inheritdoc/>
    public override void AssignIdentifier(Page<TKey, TItem> page)
    {
      page.Identifier = page;
    }

    /// <inheritdoc/>
    public override Page<TKey, TItem> Resolve(IPageRef pageRef)
    {
      return (Page<TKey, TItem>) pageRef;
    }

    #region Caching methods. All are empty.

    /// <inheritdoc/>
    public override void AddToCache(Page<TKey, TItem> page)
    {
    }

    /// <inheritdoc/>
    public override void RemoveFromCache(Page<TKey, TItem> page)
    {
    }

    /// <inheritdoc/>
    public override Page<TKey, TItem> GetFromCache(IPageRef pageRef)
    {
      return null;
    }

    #endregion

    /// <inheritdoc/>
    public override void Flush()
    {
      // Nothing has to be done here
    }

    /// <inheritdoc/>
    public override IIndexSerializer<TKey, TItem> CreateSerializer()
    {
      return new MemorySerilizer<TKey, TItem>(this);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public MemoryPageProvider()
    {
    }
  }
}