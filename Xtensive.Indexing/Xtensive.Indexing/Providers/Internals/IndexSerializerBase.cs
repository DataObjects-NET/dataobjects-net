// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2008.07.17

using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Implementation;

namespace Xtensive.Indexing.Providers.Internals
{
  /// <summary>
  /// Default base class for <see cref="Index{TKey,TItem}"/> page provider helpers.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  public abstract class IndexSerializerBase<TKey,TItem> : IIndexSerializer<TKey, TItem>
  {
    /// <summary>
    /// Gets the provider this serializer belongs to.
    /// </summary>
    public IIndexPageProvider<TKey,TItem> Provider { get; private set; }

    /// <inheritdoc/>
    public abstract void SerializeLeafPage(LeafPage<TKey, TItem> page);

    /// <inheritdoc/>
    public abstract void SerializeInnerPage(InnerPage<TKey, TItem> page);

    /// <inheritdoc/>
    public abstract void SerializeDescriptorPage(DescriptorPage<TKey, TItem> page);

    /// <inheritdoc/>
    public abstract void SerializeBloomFilter(DescriptorPage<TKey, TItem> page);

    /// <inheritdoc/>
    public abstract void SerializeEof(DescriptorPage<TKey, TItem> page);


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The <see cref="Provider"/> property value.</param>
    protected IndexSerializerBase(IIndexPageProvider<TKey, TItem> provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      Provider = provider;
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public abstract void Dispose();
  }
}