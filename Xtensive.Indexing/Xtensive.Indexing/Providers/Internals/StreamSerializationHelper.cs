// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.01.02

using System;
using System.IO;
using System.Security.AccessControl;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Implementation.Interfaces;

namespace Xtensive.Indexing.Providers.Internals
{
  internal sealed class StreamSerializationHelper<TKey, TItem> : SerializationHelperBase<TKey, TItem>
  {
    public const long OffsetLength = 8;

    private Stream streamToWrite;
    private readonly IValueSerializer serializer;
    private readonly ValueSerializer<long> offsetSerializer;

    private readonly MemoryStream innerPagesStream = new MemoryStream();
    private long reservedLength;
    private LeafPage<TKey, TItem> lastLeafPage;
    private StreamPageRef<TKey, TItem> lastPageRef = StreamPageRef<TKey, TItem>.Create(StreamPageRefType.Undefined);
    private StreamPageRef<TKey, TItem> descriptorPageRef;

    public Stream Stream
    {
      get { return streamToWrite; }
    }

    public IValueSerializer Serializer
    {
      get { return serializer; }
    }

    public ValueSerializer<long> OffsetSerializer
    {
      get { return offsetSerializer; }
    }

    public override IPageRef LastLeafPageRef
    {
      get { return lastLeafPage==null ? null : lastLeafPage.Identifier; }
    }

    public override IPageRef LastPageRef
    {
      get { return lastPageRef; }
    }

    public override IPageRef DescriptorPageRef
    {
      get { return descriptorPageRef; }
    }

    private StreamPageRef<TKey, TItem> NextPageRef
    {
      get { return StreamPageRef<TKey, TItem>.Create(streamToWrite.Position + reservedLength + innerPagesStream.Position); }
    }

    public override void SerializeLeafPage(LeafPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      page.LeftPageRef = null;
      page.RightPageRef = StreamPageRef<TKey, TItem>.Create(StreamPageRefType.Undefined);

      StreamPageRef<TKey, TItem> pageRef = NextPageRef;
      page.Identifier = pageRef;
      if (lastLeafPage!=null) {
        page.LeftPageRef = lastLeafPage.Identifier;
        lastLeafPage.RightPageRef = pageRef;
        FlushInnerPagesStream(pageRef);
      }
      serializer.Serialize(streamToWrite, page);
      reservedLength = OffsetLength;
      lastLeafPage = page;
      lastPageRef = pageRef;
    }

    public override void SerializeInnerPage(InnerPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      StreamPageRef<TKey, TItem> pageRef = NextPageRef;
      page.Identifier = pageRef;
      serializer.Serialize(innerPagesStream, page);
      lastPageRef = pageRef;
    }

    public void SerializeCachedInnerPage(InnerPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      StreamPageRef<TKey, TItem> pageRef = (StreamPageRef<TKey, TItem>) page.Identifier;
      OffsetSerializer.Serialize(streamToWrite, pageRef.Offset);
      StreamPageRef<TKey, TItem> nextPageRef = NextPageRef;
      serializer.Serialize(streamToWrite, page);
      lastPageRef = nextPageRef;
    }

    public override void SerializeDescriptorPage(DescriptorPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      FlushInnerPagesStream(null);
      descriptorPageRef = NextPageRef;
      page.Identifier = descriptorPageRef;
      page.RootPageRef = LastPageRef;
      page.RightmostPageRef = LastLeafPageRef;
      serializer.Serialize(streamToWrite, page);
      if (page.BloomFilter!=null) {
        page.BloomFilter.Serialize(streamToWrite);
      }
    }

    public override void MarkEOF(DescriptorPage<TKey, TItem> descriptorPage)
    {
      // Writing "EOF" mark
      OffsetSerializer.Serialize(streamToWrite, (long) StreamPageRefType.Null); // "End of cached pages data" mark
      OffsetSerializer.Serialize(streamToWrite, ((StreamPageRef<TKey, TItem>) descriptorPage.Identifier).Offset);
    }

    private void FlushInnerPagesStream(StreamPageRef<TKey, TItem> leafPageRef)
    {
      if (reservedLength!=0) {
        if (leafPageRef!=null) {
          offsetSerializer.Serialize(streamToWrite, leafPageRef.Offset);
          lastLeafPage.RightPageRef = leafPageRef;
        }
        else {
          offsetSerializer.Serialize(streamToWrite, (long) StreamPageRefType.Null);
          lastLeafPage.RightPageRef = null;
        }
        reservedLength = 0;
      }
      if (innerPagesStream.Length==0)
        return;
      innerPagesStream.WriteTo(streamToWrite);
      innerPagesStream.SetLength(0);
    }

    public override IDisposable CreateSerializer(IIndexPageProvider<TKey, TItem> provider)
    {
      StreamPageProvider<TKey, TItem> streamPageProvider = (StreamPageProvider<TKey, TItem>) provider;
      streamToWrite = new FileStream(streamPageProvider.StreamProvider.FileName, FileMode.OpenOrCreate, FileSystemRights.Write, FileShare.ReadWrite, 65535, FileOptions.RandomAccess);
      return streamToWrite;
    }

    public override void Dispose()
    {
      streamToWrite.DisposeSafely();
      streamToWrite = null;
    }


    // Constructors

    public StreamSerializationHelper(IValueSerializer serializer, ValueSerializer<long> offsetSerializer)
    {
      ArgumentValidator.EnsureArgumentNotNull(serializer, "serializer");
      ArgumentValidator.EnsureArgumentNotNull(offsetSerializer, "offsetSerializer");
      this.serializer = serializer;
      this.offsetSerializer = offsetSerializer;
    }
  }
}