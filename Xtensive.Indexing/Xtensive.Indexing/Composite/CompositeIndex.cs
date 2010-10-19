// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.03

using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Configuration;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing.Optimization;

namespace Xtensive.Indexing.Composite
{
  /// <summary>
  /// Composite index.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public class CompositeIndex<TKey, TItem> : ConfigurableBase<IndexConfiguration<TKey, TItem>>,
    IOptimizationInfoProvider<TKey>
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

    #region Implementation of IOptimizationInfoProvider<TKey>

    /// <inheritdoc/>
    public IStatistics<TKey> GetStatistics()
    {
      return implementation.GetStatistics();
    }

    /// <inheritdoc/>
    public AdvancedComparer<Entire<TKey>> GetEntireKeyComparer()
    {
      return implementation.GetEntireKeyComparer();
    }

    #endregion

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