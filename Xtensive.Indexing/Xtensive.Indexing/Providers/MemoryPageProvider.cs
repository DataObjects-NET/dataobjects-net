// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.08.28

using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Implementation.Interfaces;
using Xtensive.Indexing.Providers.Internals;

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
    private MemorySerializationHelper<TKey, TItem> serializeHelper;

    /// <inheritdoc/>
    public override ISerializationHelper<TKey, TItem> SerializationHelper
    {
      get { return serializeHelper; }
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

    #region PageCache access methods. All are empty.

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


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="MemoryPageProvider{TKey,TValue}"/>.
    /// </summary>
    public MemoryPageProvider()
    {
      serializeHelper = new MemorySerializationHelper<TKey, TItem>();
    }
  }
}