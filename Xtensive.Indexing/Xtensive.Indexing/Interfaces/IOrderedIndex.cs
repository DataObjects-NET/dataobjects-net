// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.12

using System.Collections.Generic;
using Xtensive.Indexing.Measures;
using Xtensive.Indexing.Optimization;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Defines index that can return a <see cref="IEnumerator{T}"/> that 
  /// enumerates through the range of index items.
  /// </summary>
  /// <typeparam name="TKey">The type of the index key.</typeparam>
  /// <typeparam name="TItem">The type of the item (should include both key and value).</typeparam>
  public interface IOrderedIndex<TKey, TItem> : 
    IIndex<TKey, TItem>,
    IOrderedEnumerable<TKey, TItem>,
    IRangeMeasurable<TKey, TItem>,
    IOptimizationInfoProvider<TKey>
  {}
}