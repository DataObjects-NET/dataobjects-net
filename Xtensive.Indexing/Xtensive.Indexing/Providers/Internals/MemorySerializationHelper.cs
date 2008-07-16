// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.07.16

using Xtensive.Core;
using Xtensive.Indexing.Implementation;


namespace Xtensive.Indexing.Providers.Internals
{
  internal sealed class MemorySerializationHelper<TKey, TItem>
  {
    private LeafPage<TKey, TItem> lastLeafPage;
    private Page<TKey, TItem> lastPageRef = default(Page<TKey,TItem>);

    public Page<TKey, TItem> LastLeafPageRef
    {
      get { return lastLeafPage==null ? null : (Page<TKey, TItem>)lastLeafPage.Identifier; }
    }

    public Page<TKey, TItem> LastPageRef
    {
      get { return lastPageRef; }
    }

    public void SerializeLeafPage(LeafPage<TKey, TItem> page)
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

    public void SerializeInnerPage(InnerPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      page.Identifier = page;
      lastPageRef = page;
    }

    public void SerializeDescriptorPage(DescriptorPage<TKey, TItem> page)
    {
      ArgumentValidator.EnsureArgumentNotNull(page, "page");
      page.Identifier = page;
      page.RootPageRef = LastPageRef;
      page.RightmostPageRef = LastLeafPageRef;
    }


    // Constructors

    public MemorySerializationHelper()
  {
  }

  }
}