// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.04

using Xtensive.Core.Configuration;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;

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
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);
      IndexSegmentSet<TKey, TItem> set = (IndexSegmentSet<TKey, TItem>) source;
      foreach (IndexSegment<TKey, TItem> segment in set)
        Add(segment);
    }
  }
}