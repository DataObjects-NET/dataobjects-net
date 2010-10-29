// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.01.02

using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Serialization;
using Xtensive.Indexing.Implementation;

namespace Xtensive.Indexing.Providers.Internals
{
  internal sealed class StreamSerializer<TKey, TItem> : IndexSerializerBase<TKey, TItem>
  {
    public const long OffsetLength = 8;

    private Stream stream;
    private readonly ISerializer serializer;
    private readonly ValueSerializer<long> offsetSerializer;
    private readonly MemoryStream innerPagesStream = new MemoryStream();
    private long reservedLength;

    public ValueSerializer<long> OffsetSerializer
    {
      get { return offsetSerializer; }
    }

    #region Properties: XxxPage, XxxPageRef

    public LeafPage<TKey, TItem> LastLeafPage { get; set; }
    public IPageRef LastPageRef { get; set; }
    private StreamPageRef<TKey, TItem> DescriptorPageRef { get; set; }

    public IPageRef LastLeafPageRef
    {
      [DebuggerStepThrough]
      get { return LastLeafPage==null ? null : LastLeafPage.Identifier; }
    }

    private StreamPageRef<TKey, TItem> NextPageRef
    {
      [DebuggerStepThrough]
      get { return StreamPageRef<TKey, TItem>.Create(stream.Position + reservedLength + innerPagesStream.Position); }
    }

    #endregion

    public override void SerializeLeafPage(LeafPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      page.LeftPageRef = null;
      page.RightPageRef = StreamPageRef<TKey, TItem>.Create(StreamPageRefType.Undefined);

      StreamPageRef<TKey, TItem> pageRef = NextPageRef;
      page.Identifier = pageRef;
      if (LastLeafPage!=null) {
        page.LeftPageRef = LastLeafPage.Identifier;
        LastLeafPage.RightPageRef = pageRef;
        FlushInnerPagesStream(pageRef);
      }
      serializer.Serialize(stream, page);
      reservedLength = OffsetLength;
      LastLeafPage = page;
      LastPageRef = pageRef;
    }

    public override void SerializeInnerPage(InnerPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      StreamPageRef<TKey, TItem> pageRef = NextPageRef;
      page.Identifier = pageRef;
      serializer.Serialize(innerPagesStream, page);
      LastPageRef = pageRef;
    }

    public override void SerializeDescriptorPage(DescriptorPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      FlushInnerPagesStream(null);
      DescriptorPageRef = NextPageRef;
      page.Identifier = DescriptorPageRef;
      page.RootPageRef = LastPageRef;
      page.RightmostPageRef = LastLeafPageRef;
      serializer.Serialize(stream, page);
    }

    public override void SerializeBloomFilter(DescriptorPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      if (page.BloomFilter!=null)
        page.BloomFilter.Serialize(stream);
    }

    public override void SerializeEof(DescriptorPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      OffsetSerializer.Serialize(stream, (long) StreamPageRefType.Null); // "End of cached pages data" mark
      OffsetSerializer.Serialize(stream, ((StreamPageRef<TKey, TItem>) page.Identifier).Offset);
    }

    private void FlushInnerPagesStream(StreamPageRef<TKey, TItem> leafPageRef)
    {
      if (reservedLength!=0) {
        if (leafPageRef!=null) {
          offsetSerializer.Serialize(stream, leafPageRef.Offset);
          LastLeafPage.RightPageRef = leafPageRef;
        }
        else {
          offsetSerializer.Serialize(stream, (long) StreamPageRefType.Null);
          LastLeafPage.RightPageRef = null;
        }
        reservedLength = 0;
      }
      if (innerPagesStream.Length==0)
        return;
      innerPagesStream.WriteTo(stream);
      innerPagesStream.SetLength(0);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The index page provider.</param>
    public StreamSerializer(IIndexPageProvider<TKey, TItem> provider)
      : base(provider)
    {
      var streamPageProvider = (StreamPageProvider<TKey, TItem>) provider;
      serializer = streamPageProvider.Serializer;
      offsetSerializer = streamPageProvider.OffsetSerializer;
      stream = new FileStream(streamPageProvider.StreamProvider.FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 65535, FileOptions.RandomAccess);
      LastPageRef = StreamPageRef<TKey, TItem>.Create(StreamPageRefType.Undefined);
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public override void Dispose()
    {
      stream.DisposeSafely();
      stream = null;
    }
  }
}