// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.13

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Providers
{
  /// <summary>
  /// Default base class for <see cref="Index{TKey,TItem}"/> page providers.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  public abstract class IndexPageProviderBase<TKey, TItem> : IIndexPageProvider<TKey, TItem>
  {
    protected Index<TKey, TItem> index;
    private bool isInitialized;


    /// <inheritdoc/>
    public Index<TKey, TItem> Index
    {
      get { return index; }
      set
      {
        if (index!=null)
          throw Exceptions.AlreadyInitialized("Index");
        index = value;
      }
    }

    /// <inheritdoc/>
    public bool IsInitialized
    {
      get { return isInitialized; }
    }

    /// <inheritdoc/>
    public virtual IndexFeatures Features
    {
      get { return IndexFeatures.Default; }
    }


    // Abstract methods
    /// <inheritdoc/>
    public abstract void AssignIdentifier(Page<TKey, TItem> page);

    /// <inheritdoc/>
    public abstract Page<TKey, TItem> Resolve(IPageRef identifier);

    /// <inheritdoc/>
    public abstract void Flush();

    /// <inheritdoc/>
    public virtual void Clear()
    {
      index.DescriptorPage.Clear();
    }

    /// <inheritdoc/>
    public virtual void Serialize(IEnumerable<TItem> source)
    {
      if (index==null)
        throw new InvalidOperationException(Strings.ExIndexPageProviderIsUnboundToTheIndex);
      if (isInitialized)
        throw new InvalidOperationException(Strings.ExIndexIsAlreadyInitialized);
      if ((Features & IndexFeatures.Serialize)==0)
        throw new InvalidOperationException(Strings.ExIndexPageProviderDoesntSupportSerialize);
      Initialize();

//      AdvancedComparer<TKey> comparer = index.KeyComparer;
//      Converter<TItem, TKey> extractor = index.KeyExtractor;
//      bool bFirstPair = true;
//      TKey previousKey = default(TKey);
//      foreach (TItem item in source) {
//        TKey key = extractor(item);
//        int comparisonResult = bFirstPair ? 1 : comparer.Compare(key, previousKey);
//        if (comparisonResult < 0)
//          throw new InvalidOperationException(Strings.ExWrongKeyOrder);
//        index.Add(item);
//        previousKey = key;
//        bFirstPair = false;
//      }

      foreach (TItem item in source)
        index.Add(item);
    }

    /// <inheritdoc/>
    public virtual void Initialize()
    {
      if (isInitialized)
        throw new InvalidOperationException(Strings.ExIndexIsAlreadyInitialized);
      BaseInitialize();
    }

    protected void BaseInitialize()
    {
      if (isInitialized)
        throw new InvalidOperationException(Strings.ExIndexIsAlreadyInitialized);
      isInitialized = true;
    }

    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    public virtual void Dispose()
    {
    }
  }
}