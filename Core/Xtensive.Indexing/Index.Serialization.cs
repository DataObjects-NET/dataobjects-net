// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.07.14

using System;
using System.Collections.Generic;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  partial class Index<TKey, TItem>
  {
    private IIndexPageProvider<TKey, TItem> provider;
    private IIndexSerializer<TKey, TItem> serializer;

    /// <summary>
    /// Serializes items from specified <see cref="IEnumerable{T}"/> into the index.
    /// </summary>
    /// <param name="source">Items to serialize into the index.</param>
    public void Serialize(IEnumerable<TItem> source)
    {
      using (serializer = provider.CreateSerializer()) {
        if (provider.Index==null)
          throw new InvalidOperationException(Strings.ExIndexPageProviderIsUnboundToTheIndex);
        if ((provider.Features & IndexFeatures.Serialize)==0)
          throw new InvalidOperationException(Strings.ExIndexPageProviderDoesntSupportSerialize);

        var descriptorPage = provider.Index.DescriptorPage;
        int pageSize = provider.Index.DescriptorPage.PageSize;
        var comparer = provider.Index.KeyComparer;
        var extractor = provider.Index.KeyExtractor;
        bool bFirstPair = true;
        TKey previousKey = default(TKey);

        var leafPage = new LeafPage<TKey, TItem>(provider);
        var branch = new List<InnerPage<TKey, TItem>>();
        descriptorPage.LeftmostPageRef = StreamPageRef<TKey, TItem>.Create((long) 0);
        var bloomFilter = provider.GetBloomFilter(source);

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
            SerializePages(serializer, branch, leafPage, false);
            leafPage = new LeafPage<TKey, TItem>(provider);
          }
        }

        if (leafPage.CurrentSize > 0 || bFirstPair)
          SerializePages(serializer, branch, leafPage, true);
        else
          SerializePages(serializer, branch, null, true);

        descriptorPage.BloomFilter = bloomFilter;
        serializer.SerializeDescriptorPage(descriptorPage);
        serializer.SerializeBloomFilter(descriptorPage);
        serializer.SerializeEof(descriptorPage);
      }
    }

    private void SerializePages(IIndexSerializer<TKey, TItem> indexSerializer, IList<InnerPage<TKey, TItem>> branch, LeafPage<TKey, TItem> leafPage,
      bool isTailBranch)
    {
      int level = 0;
      DataPage<TKey, TItem> page = leafPage;
      while (true) {
        if (page!=null) {
          if (level==0)
            indexSerializer.SerializeLeafPage(leafPage);
          else {
            var innerPage = (InnerPage<TKey, TItem>) page;
            indexSerializer.SerializeInnerPage(innerPage);
            provider.AddToCache(innerPage);
          }
        }
        else if (level==0 && branch.Count==0) // Empty index
          indexSerializer.SerializeLeafPage(leafPage);

        if (branch.Count==level) {
          if (isTailBranch)
            break;
          branch.Add(new InnerPage<TKey, TItem>(provider));
        }
        var abovePage = branch[level];

        if (page!=null) {
          // Insertion to abovePage
          int insertionIndex = abovePage.CurrentSize;
          if (insertionIndex==0 && abovePage.GetPageRef(-1)==null)
            insertionIndex = -1;
          abovePage.Insert(insertionIndex, page.Key, page.Identifier);
          abovePage.AddToMeasures(page);
        }

        if (!isTailBranch && abovePage.CurrentSize < provider.Index.DescriptorPage.PageSize)
          break;
        // Creating new InnerPage, if not tail; increasing level
        page = abovePage;
        branch[level++] = isTailBranch ? null : new InnerPage<TKey, TItem>(provider);
      }
    }
  }
}