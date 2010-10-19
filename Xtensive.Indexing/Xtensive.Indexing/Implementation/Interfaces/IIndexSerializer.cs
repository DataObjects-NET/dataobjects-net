// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2008.07.16

using System;
using Xtensive.Serialization.Binary;
using Xtensive.Indexing.Implementation;

namespace Xtensive.Indexing.Implementation
{
  /// <summary>
  /// <see cref="Index{TKey,TItem}"/> serializer for page providers.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  public interface IIndexSerializer<TKey, TItem> : IDisposable
  {
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
    /// Serializes the specified descriptor page.
    /// </summary>
    /// <param name="page">The descriptor page.</param>
    void SerializeDescriptorPage(DescriptorPage<TKey, TItem> page);

    /// <summary>
    /// Serializes the bloom filter.
    /// </summary>
    /// <param name="page">The descriptor page.</param>
    void SerializeBloomFilter(DescriptorPage<TKey, TItem> page);

    /// <summary>
    /// Serializes "EOF" mark.
    /// </summary>
    /// <param name="page">The descriptor page.</param>
    void SerializeEof(DescriptorPage<TKey, TItem> page);
  }
}