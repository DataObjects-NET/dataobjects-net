// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.08.28

using System;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  partial class Index<TKey, TItem>
  {
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"><paramref name="item"/> is already added.</exception>
    /// <exception cref="NotSupportedException">Write is not allowed by <see cref="Features"/>.</exception>
    public override void Add(TItem item)
    {
      EnsureConfigured();
      if ((Features & IndexFeatures.Write)==0)
        throw new NotSupportedException(Strings.ExIndexPageProviderDoesntSupportWrite);
      TKey key = KeyExtractor(item);
      DataPage<TKey, TItem> newPage = InternalAdd(RootPage, key, item);
      if (newPage!=null)
        ChangeRootPage(newPage);
    }

    /// <inheritdoc/>
    public override bool Remove(TItem item)
    {
      return RemoveKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Write is not allowed by <see cref="Features"/>.</exception>
    public override bool RemoveKey(TKey key)
    {
      EnsureConfigured();
      if ((Features & IndexFeatures.Write)==0)
        throw new NotSupportedException(Strings.ExIndexPageProviderDoesntSupportWrite);
      TItem item;
      return InternalRemove(RootPage, key, out item);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Write is not allowed by <see cref="Features"/>.</exception>
    public override void Replace(TItem item)
    {
      EnsureConfigured();
      if ((Features & IndexFeatures.Write)==0)
        throw new NotSupportedException(Strings.ExIndexPageProviderDoesntSupportWrite);
      InternalReplace(RootPage, item);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      DescriptorPage.Clear();
      cachedRootPageRef = cachedRightmostPageRef = cachedLeftmostPageRef = null;
      provider.Clear();
    }
  }
}