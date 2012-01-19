// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.18

using System;
using Xtensive.Notifications;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Creates indexes for collections.
  /// </summary>
  public interface ICollectionIndexProvider
  {
    /// <summary>
    /// Creates either unique or non-unique index for the <paramref name="owner"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the value.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <param name="name">The name of the index.</param>
    /// <param name="owner">The collection to be indexed.</param>
    /// <param name="unique"><see langword="True"/> if created index should be unique;
    /// otherwise, <see langword="false"/>.</param>
    /// <param name="extractKey">Key extractor.</param>
    /// <returns>
    ///   <see cref="IIndex"/> instance.
    /// </returns>
    /// <remarks>This method always creates <see cref="CollectionIndex{TKey,TItem}"/> instance
    /// wrapping the implementation of specified type (<typeparamref name="TImplementation"/>).</remarks>
    IIndex<TKey, TItem> CreateIndex<TKey, TItem, TImplementation>(
      string name, ICollectionChangeNotifier<TItem> owner, bool unique, Converter<TItem, TKey> extractKey)
      where TImplementation : IIndex<TKey, TItem>;

    /// <summary>
    /// Creates unique index for the <paramref name="owner"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the value.</typeparam>
    /// <param name="name">The name of the index.</param>
    /// <param name="owner">The collection to be indexed.</param>
    /// <param name="extractKey">Key extractor.</param>
    /// <returns>
    ///   <see cref="IIndex{TKey,TItem}"/> instance.
    /// </returns>
    /// <remarks>This method always creates <see cref="CollectionIndex{TKey,TItem}"/> instance
    /// wrapping the implementation of specified type (<typeparamref name="TImplementation"/>).</remarks>
    IUniqueIndex<TKey, TItem> CreateUniqueIndex<TKey, TItem, TImplementation>(
      string name, ICollectionChangeNotifier<TItem> owner, Converter<TItem, TKey> extractKey)
      where TImplementation : IUniqueIndex<TKey, TItem>, IIndex<TKey, TItem>;

    /// <summary>
    /// Creates non-unique index for the <paramref name="owner"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the value.</typeparam>
    /// <param name="name">The name of the index.</param>
    /// <param name="owner">The collection to be indexed.</param>
    /// <param name="extractKey">Key extractor.</param>
    /// <returns>
    ///   <see cref="IIndex{TKey,TItem}"/> instance.
    /// </returns>
    /// <remarks>This method always creates <see cref="CollectionIndex{TKey,TItem}"/> instance
    /// wrapping the implementation of specified type (<typeparamref name="TImplementation"/>).</remarks>
    INonUniqueIndex<TKey, TItem> CreateNonUniqueIndex<TKey, TItem, TImplementation>(
      string name, ICollectionChangeNotifier<TItem> owner, Converter<TItem, TKey> extractKey)
      where TImplementation : INonUniqueIndex<TKey, TItem>, IIndex<TKey, TItem>;
  }
}