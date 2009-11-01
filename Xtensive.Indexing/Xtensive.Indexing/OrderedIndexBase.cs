// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.15

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Base class for all unique ordered indexes.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public abstract class OrderedIndexBase<TKey, TItem>: IndexBase<TKey, TItem>,
    IOrderedIndex<TKey, TItem>
  {
    /// <inheritdoc/>
    public IEnumerable<TKey> GetKeys(Range<IEntire<TKey>> range)
    {
      foreach (TItem item in GetItems(range))
        yield return KeyExtractor(item);
    }

    /// <inheritdoc/>
    public IEnumerable<TItem> GetItems(Range<IEntire<TKey>> range)
    {
      return CreateReader(range);
    }
    
    /// <inheritdoc/>
    public abstract SeekResult<TItem> Seek(Ray<IEntire<TKey>> ray);

    /// <inheritdoc/>
    public abstract IIndexReader<TKey, TItem> CreateReader(Range<IEntire<TKey>> range);

    /// <inheritdoc/>
    public abstract object GetMeasureResult(Range<IEntire<TKey>> range, string name);

    /// <inheritdoc/>
    public abstract object[] GetMeasureResults(Range<IEntire<TKey>> range, params string[] names);

    /// <inheritdoc/>
    public override IEnumerator<TItem> GetEnumerator()
    {
      return CreateReader(this.GetFullRange());
    }
 

    // Constructors

    /// <inheritdoc/>
    protected OrderedIndexBase()
    {
    }

    /// <inheritdoc/>
    protected OrderedIndexBase(IndexConfigurationBase<TKey, TItem> configuration)
      : base(configuration)
    {
    }
  }
}