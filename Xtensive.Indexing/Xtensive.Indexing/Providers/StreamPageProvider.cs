// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.12.26

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xtensive.Caching;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Internals.DocTemplates;
using Xtensive.IO;
using Xtensive.Serialization;
using Xtensive.Serialization.Binary;
using Xtensive.Threading;
using Xtensive.Indexing.BloomFilter;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Providers.Internals;
using Xtensive.Indexing.Resources;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Indexing.Providers
{
  /// <summary>
  /// Serialize-and-read <see cref="Stream"/> page provider.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  public class StreamPageProvider<TKey, TItem> : IndexPageProviderBase<TKey, TItem>
  {
    private StreamProvider streamProvider;
    private readonly ISerializer serializer;
    private readonly ValueSerializer<long> offsetSerializer;
    private readonly ICache<IPageRef, Page<TKey, TItem>> pageCache;
    private readonly ReaderWriterLockSlim pageCacheLock = new ReaderWriterLockSlim();
    private bool descriptorPageIdentifierAssigned;
    private bool rootPageIdentifierAssigned;

    /// <inheritdoc/>
    public override IndexFeatures Features
    {
      [DebuggerStepThrough]
      get { return IndexFeatures.SerializeAndRead; }
    }

    /// <summary>
    /// Gets the stream provider.
    /// </summary>
    public StreamProvider StreamProvider
    {
      [DebuggerStepThrough]
      get { return streamProvider; }
    }

    /// <summary>
    /// Gets the serializer user by this page provider.
    /// </summary>
    public ISerializer Serializer
    {
      [DebuggerStepThrough]
      get { return serializer; }
    }

    /// <summary>
    /// Gets the offset serializer user by this page provider.
    /// </summary>
    public ValueSerializer<long> OffsetSerializer
    {
      [DebuggerStepThrough]
      get { return offsetSerializer; }
    }

    /// <inheritdoc/>
    public override IBloomFilter<TKey> GetBloomFilter(IEnumerable<TItem> source)
    {
      bool useBloomFilter = index.DescriptorPage.Configuration.UseBloomFilter;
      double bloomFilterBitsPerValue = index.DescriptorPage.Configuration.BloomFilterBitsPerValue;
      if (useBloomFilter) {
        // Try to get item's count
        long count;
        if (source is ICountable) {
          count = ((ICountable) source).Count;
        }
        else if (source is ICollection) {
          count = ((ICollection) source).Count;
        }
        else if (source is ICollection<TItem>) {
          count = ((ICollection<TItem>) source).Count;
        }
        else {
          throw new ArgumentException(Strings.ExUnableToGetCountForBloomFilter, "source");
        }
        if (count > 0) {
          return new MemoryBloomFilter<TKey>(count, BloomFilter<TKey>.GetOptimalHashCount(bloomFilterBitsPerValue));
        }
      }
      return null;
    }

    /// <inheritdoc/>
    public override void AssignIdentifier(Page<TKey, TItem> page)
    {
      if (page is DescriptorPage<TKey, TItem>) {
        if (descriptorPageIdentifierAssigned)
          throw Exceptions.InternalError("Second DescriptorPage has been created.", Log.Instance);
        page.Identifier = StreamPageRef<TKey, TItem>.Create(StreamPageRefType.Descriptor);
        descriptorPageIdentifierAssigned = true;
      }
      else if (!rootPageIdentifierAssigned && (page is LeafPage<TKey, TItem>)) {
        page.Identifier = StreamPageRef<TKey, TItem>.Create((long) 0);
        rootPageIdentifierAssigned = true;
      }
      else
        page.Identifier = StreamPageRef<TKey, TItem>.Create(StreamPageRefType.Undefined);
    }

    /// <inheritdoc/>
    public override Page<TKey, TItem> Resolve(IPageRef identifier)
    {
      if (identifier==null)
        return null;
      Page<TKey, TItem> page = identifier as Page<TKey, TItem>;
      if (page!=null) // Cached page
        return page;
      StreamPageRef<TKey, TItem> streamPageRef = (StreamPageRef<TKey, TItem>) identifier;
      if (!streamPageRef.IsDefined)
        throw Exceptions.InternalError(String.Format("Undefined {0}.", streamPageRef), Log.Instance);
      page = GetFromCache(identifier);
      if (page==null) {
        try {
          page = Deserialize(streamPageRef);
          if (page==null)
            throw Exceptions.InternalError(String.Format("StreamPageRef {0} points to null page.", streamPageRef), Log.Instance);
          page.Provider = this;
          page.Identifier = identifier;
          pageCacheLock.ExecuteWriter(delegate { pageCache.Add(page); });
        }
        catch (Exception e) {
          Log.Error(Strings.ExCantDeserializeIndexPage, streamPageRef, e);
          throw;
        }
      }
      return page;
    }

    #region Page caching methods: AddToCache, RemoveFromCache, GetFromCache

    /// <inheritdoc/>
    public override void AddToCache(Page<TKey, TItem> page)
    {
      if (pageCache!=null) {
        LockCookie? cookie = pageCacheLock.BeginWrite();
        try {
          pageCache.Add(page);
        }
        finally {
          pageCacheLock.EndWrite(cookie);
        }
      }
    }

    /// <inheritdoc/>
    public override void RemoveFromCache(Page<TKey, TItem> page)
    {
      if (pageCache!=null) {
        LockCookie? cookie = pageCacheLock.BeginWrite();
        try {
          pageCache.Remove(page);
        }
        catch (Exception) {
          return;
        }
        finally {
          pageCacheLock.EndWrite(cookie);
        }
      }
    }

    /// <inheritdoc/>
    public override Page<TKey, TItem> GetFromCache(IPageRef pageRef)
    {
      if (pageCache!=null) {
        pageCacheLock.BeginRead();
        try {
          return pageCache[pageRef, true];
        }
        catch (Exception) {
          return null;
        }
        finally {
          pageCacheLock.EndRead();
        }
      }
      else {
        return null;
      }
    }

    #endregion

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override void Flush()
    {
      throw new NotSupportedException(Strings.ExIndexPageProviderDoesntSupportWrite);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      throw new NotSupportedException(Strings.ExIndexPageProviderDoesntSupportWrite);
    }

    /// <inheritdoc/>
    public override IIndexSerializer<TKey, TItem> CreateSerializer()
    {
      return new StreamSerializer<TKey, TItem>(this);
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      if (streamProvider.FileExists) {
        DescriptorPage<TKey, TItem> descriptorPage = DeserializeDescriptorPage();
        descriptorPage.Provider = this;
        index.DescriptorPage = descriptorPage;
      }
    }

    #region Private \ internal methods

    private Page<TKey, TItem> Deserialize(StreamPageRef<TKey, TItem> pageRef)
    {
      if (pageRef==null)
        return null;
      if (!pageRef.IsDefined)
        throw Exceptions.InternalError(String.Format("Undefined {0}.", pageRef), Log.Instance);
      long offset = pageRef.Offset;
      if (offset < 0)
        return null;
      Stream stream = streamProvider.GetStream();
      try {
        stream.Seek(offset, SeekOrigin.Begin);
        Page<TKey, TItem> page = (Page<TKey, TItem>) serializer.Deserialize(stream);
        LeafPage<TKey, TItem> leafPage = page as LeafPage<TKey, TItem>;
        if (leafPage!=null)
          leafPage.RightPageRef = StreamPageRef<TKey, TItem>.Create(offsetSerializer.Deserialize(stream));
        return page;
      }
      finally {
        streamProvider.ReleaseStream(stream);
      }
    }

    private DescriptorPage<TKey, TItem> DeserializeDescriptorPage()
    {
      Stream stream = streamProvider.GetStream();
      try {
        // This is DescriptorPage, so its actual offset is the last serialized number in the stream
        stream.Seek(-StreamSerializer<TKey, TItem>.OffsetLength, SeekOrigin.End);
        long offset = offsetSerializer.Deserialize(stream);
        stream.Seek(offset, SeekOrigin.Begin);
        DescriptorPage<TKey, TItem> descriptorPage = (DescriptorPage<TKey, TItem>) serializer.Deserialize(stream);

        if (descriptorPage.Configuration.UseBloomFilter)
          descriptorPage.BloomFilter = new MemoryBloomFilter<TKey>(stream);

        return descriptorPage;
      }
      finally {
        streamProvider.ReleaseStream(stream);
      }
    }

    #endregion

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <param name="cacheSize">Page cache size (in pages).</param>
    public StreamPageProvider(string fileName, int cacheSize)
    {
      ArgumentValidator.EnsureArgumentNotNull(fileName, "fileName");
      ArgumentValidator.EnsureArgumentIsInRange(cacheSize, 0, int.MaxValue, "cacheSize");
      streamProvider = new StreamProvider(fileName);
      serializer = LegacyBinarySerializer.Instance;
      offsetSerializer = ValueSerializerProvider.Default.GetSerializer<long>();
      if (cacheSize > 0) {
        pageCache =
          new LruCache<IPageRef, Page<TKey, TItem>>(cacheSize, p => p.Identifier, 
            new WeakCache<IPageRef, Page<TKey, TItem>>(false, p => p.Identifier));
      }
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public override void Dispose()
    {
      streamProvider.DisposeSafely();
      streamProvider = null;
    }
  }
}