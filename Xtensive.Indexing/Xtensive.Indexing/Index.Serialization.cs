// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.07.14

using System;
using System.Collections.Generic;
using Xtensive.Core.Comparison;
using Xtensive.Indexing.BloomFilter;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Implementation.Interfaces;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  partial class Index<TKey, TItem>
  {
    private IIndexPageProvider<TKey, TItem> provider;
    private ISerializationHelper<TKey, TItem> serializer;

    /// <summary>
    /// Serializes items from specified <see cref="IEnumerable{T}"/> into the index.
    /// </summary>
    /// <param name="source">Items to serialize into the index.</param>
    public void Serialize(IEnumerable<TItem> source)
    {
      serializer = provider.SerializationHelper;
      using (serializer.CreateSerializer(provider)) {
        if (provider.Index==null)
          throw new InvalidOperationException(Strings.ExIndexPageProviderIsUnboundToTheIndex);
        if ((provider.Features & IndexFeatures.Serialize)==0)
          throw new InvalidOperationException(Strings.ExIndexPageProviderDoesntSupportSerialize);

        DescriptorPage<TKey, TItem> descriptorPage = provider.Index.DescriptorPage;
        int pageSize = provider.Index.DescriptorPage.PageSize;
        AdvancedComparer<TKey> comparer = provider.Index.KeyComparer;
        Converter<TItem, TKey> extractor = provider.Index.KeyExtractor;
        bool bFirstPair = true;
        TKey previousKey = default(TKey);

        LeafPage<TKey, TItem> leafPage = new LeafPage<TKey, TItem>(provider);
        IList<InnerPage<TKey, TItem>> branch = new List<InnerPage<TKey, TItem>>();
        descriptorPage.LeftmostPageRef = StreamPageRef<TKey, TItem>.Create((long) 0);
        IBloomFilter<TKey> bloomFilter = provider.GetBloomFilter(source);

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
        serializer.MarkEOF(descriptorPage);
      }
    }

    private void SerializePages(ISerializationHelper<TKey, TItem> serializationHelper, IList<InnerPage<TKey, TItem>> branch, LeafPage<TKey, TItem> leafPage,
      bool isTailBranch)
    {
      int level = 0;
      DataPage<TKey, TItem> page = leafPage;
      while (true) {
        if (page!=null) {
          if (level==0)
            serializationHelper.SerializeLeafPage(leafPage);
          else {
            InnerPage<TKey, TItem> innerPage = (InnerPage<TKey, TItem>) page;
            serializationHelper.SerializeInnerPage(innerPage);
            provider.AddToCache(innerPage);
          }
        }
        else if (level==0 && branch.Count==0) // Empty index
          serializationHelper.SerializeLeafPage(leafPage);

        if (branch.Count==level) {
          if (isTailBranch)
            break;
          branch.Add(new InnerPage<TKey, TItem>(provider));
        }
        InnerPage<TKey, TItem> abovePage = branch[level];

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