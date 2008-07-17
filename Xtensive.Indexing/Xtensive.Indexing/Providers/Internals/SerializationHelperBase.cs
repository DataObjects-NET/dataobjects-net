// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2008.07.17

using System;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Implementation.Interfaces;

namespace Xtensive.Indexing.Providers.Internals
{
  /// <inheritdoc/>
  public abstract class SerializationHelperBase<TKey,TItem> : ISerializationHelper<TKey, TItem>
  {

    /// <inheritdoc/>
    public virtual IValueSerializer Serializer
    {
      get { return null;}
    }

    /// <inheritdoc/>
    public virtual ValueSerializer<long> OffsetSerializer
    {
      get { return null; }
    }

    /// <inheritdoc/>
    public virtual IPageRef DescriptorPageRef
    {
      get { return null; }
    }

    /// <inheritdoc/>
    public virtual void SerializeCachedInnerPage(InnerPage<TKey, TItem> page)
    {
    }

    /// <inheritdoc/>
    public virtual void MarkEOF(DescriptorPage<TKey, TItem> descriptorPage)
    {
    }

    //Abstract methods.

    /// <inheritdoc/>
    public abstract void Dispose();
    
    /// <inheritdoc/>
    public abstract IPageRef LastLeafPageRef { get; }

    /// <inheritdoc/>
    public abstract IPageRef LastPageRef { get; }

    /// <inheritdoc/>
    public abstract void SerializeLeafPage(LeafPage<TKey, TItem> page);

    /// <inheritdoc/>
    public abstract void SerializeInnerPage(InnerPage<TKey, TItem> page);

    /// <inheritdoc/>
    public abstract void SerializeDescriptorPage(DescriptorPage<TKey, TItem> page);

    /// <inheritdoc/>
    public abstract IDisposable CreateSerializer(IIndexPageProvider<TKey, TItem> provider);
  }
}