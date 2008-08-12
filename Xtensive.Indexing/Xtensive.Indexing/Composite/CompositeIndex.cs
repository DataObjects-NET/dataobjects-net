// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.03

using System.Diagnostics;
using Xtensive.Core.Helpers;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing.Composite
{
  /// <summary>
  /// Composite index.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public class CompositeIndex<TKey, TItem> : ConfigurableBase<IndexConfiguration<TKey, TItem>>
    where TKey : Tuple
    where TItem : Tuple
  {
    private IUniqueOrderedIndex<TKey, TItem> implementation;

    private readonly IndexSegmentSet<TKey, TItem> segments = new IndexSegmentSet<TKey, TItem>();

    /// <summary>
    /// Gets the index segments.
    /// </summary>
    public IndexSegmentSet<TKey, TItem> Segments
    {
      [DebuggerStepThrough]
      get { return segments; }
    }

    internal IUniqueOrderedIndex<TKey, TItem> Implementation
    {
      [DebuggerStepThrough]
      get { return implementation; }
    }

    /// <inheritdoc/>
    protected override void OnConfigured()
    {
      base.OnConfigured();
      implementation = Configuration.UniqueIndex;

      for (int index = 0, count = (int) Configuration.Segments.Count; index < count; index++) {
        IndexSegmentConfiguration<TKey, TItem> segmentConfiguration = Configuration.Segments[index];
        IndexSegment<TKey, TItem> segment = new IndexSegment<TKey, TItem>(segmentConfiguration, this, index);
        segments.Add(segment);
      }
      segments.Lock(true);
    }
  }
}