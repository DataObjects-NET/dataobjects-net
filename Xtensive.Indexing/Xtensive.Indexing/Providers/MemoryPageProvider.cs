// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.08.28

using Xtensive.Indexing.Implementation;

namespace Xtensive.Indexing.Providers
{
  /// <summary>
  /// Memory <see cref="IIndexPageProvider{TKey,TItem}"/> which provides
  /// <see cref="Index{TKey,TItem}"/> with in-memory stored pages.
  /// </summary>
  /// <typeparam name="TKey">Key type of the page nodes.</typeparam>
  /// <typeparam name="TValue">Node type.</typeparam>
  public sealed class MemoryPageProvider<TKey, TValue> : IndexPageProviderBase<TKey, TValue>
  {
    /// <inheritdoc/>
    public override void AssignIdentifier(Page<TKey, TValue> page)
    {
      page.Identifier = page;
    }

    /// <inheritdoc/>
    public override Page<TKey, TValue> Resolve(IPageRef pageRef)
    {
      return (Page<TKey, TValue>)pageRef;
    }

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
    }
  }
}