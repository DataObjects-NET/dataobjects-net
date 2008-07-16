
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.08.28

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Indexing.BloomFilter;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Providers.Internals;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Providers
{
  /// <summary>
  /// Memory <see cref="IIndexPageProvider{TKey,TItem}"/> which provides
  /// <see cref="Index{TKey,TItem}"/> with in-memory stored pages.
  /// </summary>
  /// <typeparam name="TKey">Key type of the page nodes.</typeparam>
  /// <typeparam name="TItem">Node type.</typeparam>
  public sealed class MemoryPageProvider<TKey, TItem> : IndexPageProviderBase<TKey, TItem>
  {
    /// <inheritdoc/>
    public override void AssignIdentifier(Page<TKey, TItem> page)
    {
      page.Identifier = page;
    }

    /// <inheritdoc/>
    public override Page<TKey, TItem> Resolve(IPageRef pageRef)
    {
      return (Page<TKey, TItem>)pageRef;
    }
    /// <inheritdoc/>
    public override void Serialize(IEnumerable<TItem> source)
    {
      if (index == null)
        throw new InvalidOperationException(Strings.ExIndexPageProviderIsUnboundToTheIndex);
      if ((Features & IndexFeatures.Serialize) == 0)
        throw new InvalidOperationException(Strings.ExIndexPageProviderDoesntSupportSerialize);

      DescriptorPage<TKey, TItem> descriptorPage = index.DescriptorPage;
      int pageSize = index.DescriptorPage.PageSize;
      AdvancedComparer<TKey> comparer = index.KeyComparer;
      Converter<TItem, TKey> extractor = index.KeyExtractor;
      bool bFirstPair = true;
      TKey previousKey = default(TKey);
      IBloomFilter<TKey> bloomFilter = GetBloomFilter(source);

      LeafPage<TKey, TItem> leafPage = new LeafPage<TKey, TItem>(this);
      IList<InnerPage<TKey, TItem>> branch = new List<InnerPage<TKey, TItem>>();
      descriptorPage.LeftmostPageRef = default(Page<TKey, TItem>);

      MemorySerializationHelper<TKey, TItem> serializationHelper =
          new MemorySerializationHelper<TKey, TItem>();

      foreach (TItem item in source) {
        TKey key = extractor(item);
        if (bloomFilter!=null) {
          bloomFilter.AddValue(key);
        }
        int comparisonResult = bFirstPair ? 1 : comparer.Compare(key, previousKey);
        if (comparisonResult <= 0)
          throw new InvalidOperationException(Strings.ExIncorrectKeyOrder);
        leafPage.Insert(leafPage.CurrentSize, item);
        leafPage.AddToMeasures(item);
        previousKey = key;
        bFirstPair = false;

        if (pageSize==leafPage.CurrentSize) {
          SerializePages(serializationHelper, branch, leafPage, false);
          leafPage = new LeafPage<TKey, TItem>(this);
        }
      }
      if (leafPage.CurrentSize > 0 || bFirstPair)
        SerializePages(serializationHelper, branch, leafPage, true);
        else
        SerializePages(serializationHelper, branch, null, true);

        descriptorPage.BloomFilter = bloomFilter;
        serializationHelper.SerializeDescriptorPage(descriptorPage);
    }

    private void SerializePages(MemorySerializationHelper<TKey, TItem> serializationHelper,IList<InnerPage<TKey, TItem>> branch, LeafPage<TKey, TItem> leafPage,
  bool isTailBranch)
    {
      int level = 0;
      DataPage<TKey, TItem> page = leafPage;
      while (true)
      { 
        if (page!=null) {
          if (level==0)
            serializationHelper.SerializeLeafPage(leafPage);
          else {
            InnerPage<TKey, TItem> innerPage = (InnerPage<TKey, TItem>)page;
            serializationHelper.SerializeInnerPage(innerPage);
          }
        }
        else if (level==0 && branch.Count==0) // Empty index
          serializationHelper.SerializeLeafPage(leafPage);

        if (branch.Count == level) {
          if (isTailBranch)
            break;
          branch.Add(new InnerPage<TKey, TItem>(this));
        }
        InnerPage<TKey, TItem> abovePage = branch[level];

        if (page != null)
        {
          // Insertion to abovePage
          int insertionIndex = abovePage.CurrentSize;
          if (insertionIndex == 0 && abovePage.GetPageRef(-1) == null)
            insertionIndex = -1;
          abovePage.Insert(insertionIndex, page.Key, page.Identifier);
          abovePage.AddToMeasures(page);
        }

        if (!isTailBranch && abovePage.CurrentSize < index.DescriptorPage.PageSize)
          break;
        // Creating new InnerPage, if not tail; increasing level
        page = abovePage;
        branch[level++] = isTailBranch ? null : new InnerPage<TKey, TItem>(this);
      }
    }

    /// <inheritdoc/>
    public override void Flush()
    {
      // Nothing has to be done here
    }


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="MemoryPageProvider{TKey,TValue}"/>.
    /// </summary>
    public MemoryPageProvider()
    {
    }
  }
}