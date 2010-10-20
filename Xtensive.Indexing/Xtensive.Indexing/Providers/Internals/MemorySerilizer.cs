// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.07.16

using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Implementation;

namespace Xtensive.Indexing.Providers.Internals
{
  internal sealed class MemorySerilizer<TKey, TItem> : IndexSerializerBase<TKey, TItem>
  {
    private LeafPage<TKey, TItem> lastLeafPage;
    private Page<TKey, TItem> lastPageRef;

    public override void SerializeLeafPage(LeafPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      page.LeftPageRef = null;
      page.RightPageRef = default(Page<TKey, TItem>);
      page.Identifier = page;
      if (lastLeafPage!=null) {
        page.LeftPageRef = lastLeafPage.Identifier;
        lastLeafPage.RightPageRef = page;
      }
      lastLeafPage = page;
      lastPageRef = page;
    }

    public override void SerializeInnerPage(InnerPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      page.Identifier = page;
      lastPageRef = page;
    }

    public override void SerializeDescriptorPage(DescriptorPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      page.Identifier = page;
      page.RootPageRef = lastPageRef;
      page.RightmostPageRef = lastLeafPage;
    }

    public override void SerializeBloomFilter(DescriptorPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      return;
    }

    public override void SerializeEof(DescriptorPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      return;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The index page provider.</param>
    public MemorySerilizer(IIndexPageProvider<TKey, TItem> provider)
      : base(provider)
    {
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public override void Dispose()
    {      
    }
  }
}