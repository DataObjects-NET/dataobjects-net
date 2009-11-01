// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.03

using Xtensive.Core.Helpers;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing.Composite
{
  /// <summary>
  /// Composite index wrapper.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public class CompositeIndex<TKey, TItem>: ConfigurableBase<IndexConfiguration<TKey, TItem>> where TKey : Tuple where TItem : Tuple
  {
    internal IUniqueOrderedIndex<TKey, TItem> implementation;

    private IndexSegmentSet<TKey, TItem> segments = new IndexSegmentSet<TKey, TItem>();

    /// <inheritdoc/>
    protected override void OnConfigured()
    {
      base.OnConfigured();
      implementation = Configuration.UniqueIndex;

      for (int i = 0, count = (int) Configuration.Segments.Count; i < count; i++) {
        IndexSegmentConfiguration<TKey, TItem> segmentConfiguration = Configuration.Segments[i];
        IndexSegment<TKey, TItem> segment = new IndexSegment<TKey, TItem>(segmentConfiguration, this, i);
        segments.Add(segment);
      }
      segments.Lock(true);
    }

    /// <summary>
    /// Gets the index segments.
    /// </summary>
    /// <value>The index segments.</value>
    public IndexSegmentSet<TKey, TItem> Segments
    {
      get { return segments; }
    }
  }
}