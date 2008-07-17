// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2008.07.16

using System;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Indexing.Implementation;

namespace Xtensive.Indexing.Implementation.Interfaces
{
  /// <summary>
  /// Default base class for <see cref="Index{TKey,TItem}"/> page provider helpers.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  public interface ISerializationHelper<TKey, TItem> : IDisposable
  {
    /// <summary>
    /// Gets the serializer.
    /// </summary>
    /// <value>The serializer.</value>
    IValueSerializer Serializer {get;}

    /// <summary>
    /// Gets the offset serializer.
    /// </summary>
    /// <value>The offset serializer.</value>
    ValueSerializer<long> OffsetSerializer {get;}

    /// <summary>
    /// Gets the last leaf page reference.
    /// </summary>
    /// <value>The last leaf page reference.</value>
    IPageRef LastLeafPageRef { get; }

    /// <summary>
    /// Gets the last page reference.
    /// </summary>
    /// <value>The last page reference.</value>
    IPageRef LastPageRef { get; }

    /// <summary>
    /// Gets the descriptor page reference.
    /// </summary>
    /// <value>The descriptor page reference.</value>
    IPageRef DescriptorPageRef { get; }

    /// <summary>
    /// Serializes the specified leaf page.
    /// </summary>
    /// <param name="page">The page.</param>
    void SerializeLeafPage(LeafPage<TKey, TItem> page);

    /// <summary>
    /// Serializes the specified inner page.
    /// </summary>
    /// <param name="page">The inner page.</param>
    void SerializeInnerPage(InnerPage<TKey, TItem> page);

    /// <summary>
    /// Serializes the specified cached inner page.
    /// </summary>
    /// <param name="page">The cached inner page.</param>
    void SerializeCachedInnerPage(InnerPage<TKey, TItem> page);

    /// <summary>
    /// Serializes the specified descriptor page.
    /// </summary>
    /// <param name="page">The descriptor page.</param>
    void SerializeDescriptorPage(DescriptorPage<TKey, TItem> page);

    /// <summary>
    /// Writes "EOF" mark.
    /// </summary>
    /// <param name="descriptorPage">The descriptor page.</param>
    void MarkEOF(DescriptorPage<TKey, TItem> descriptorPage);

    /// <summary>
    /// Creates the serializer for the specified page provider.
    /// </summary>
    /// <param name="provider">The page provider.</param>
    /// <returns></returns>
    IDisposable CreateSerializer(IIndexPageProvider<TKey, TItem> provider);
  }
}