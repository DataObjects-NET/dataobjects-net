// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.04

using Xtensive.Core.Helpers;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing.Composite
{
  public class IndexSegmentSet<TKey, TItem> : ConfigurationSetBase<IndexSegment<TKey, TItem>>
    where TKey : Tuple
    where TItem : Tuple
  {
    /// <inheritdoc/>
    protected override string GetItemName(IndexSegment<TKey, TItem> item)
    {
      return item.SegmentName;
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new IndexSegmentSet<TKey, TItem>();
    }

    /// <inheritdoc/>
    protected override void Clone(ConfigurationBase source)
    {
      base.Clone(source);
      IndexSegmentSet<TKey, TItem> set = (IndexSegmentSet<TKey, TItem>) source;
      foreach (IndexSegment<TKey, TItem> segment in set)
        Add(segment);
    }
  }
}