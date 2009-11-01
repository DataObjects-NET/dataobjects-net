// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.28

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing
{
  /// <summary>
  /// A generic collection storing values in order of its <see cref="IndexBase{TKey,TItem}.KeyComparer"/>.
  /// </summary>
  /// <typeparam name="TItem">Type of collection item.</typeparam>
  [Serializable]
  public sealed class SortedCollection<TItem> : OrderedIndexBase<TItem, TItem>,
    ICollection<TItem>,
    IHasVersion<int>
  {
    private readonly static Converter<TItem, TItem> noKeyExtractor = item => item;
    private readonly List<TItem> items = new List<TItem>();
    private int version;

    /// <inheritdoc/>
    public override long Count
    {
      get { return items.Count; }
    }

    /// <inheritdoc/>
    int ICollection<TItem>.Count
    {
      get { return items.Count; }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      get { return false; }
    }

    /// <inheritdoc/>
    public TItem this[int index]
    {
      get { return items[index]; }
    }

    #region Contains, ContainsKey methods

    /// <inheritdoc/>
    public override bool Contains(TItem item)
    {
      int index = items.BinarySearch(item, KeyComparer.Implementation);
      return index >= 0;
    }

    /// <inheritdoc/>
    public override bool ContainsKey(TItem key)
    {
      return Contains(key);
    }

    #endregion

    #region Seek, CreateReader methods

    /// <inheritdoc/>
    public override SeekResult<TItem> Seek(Ray<IEntire<TItem>> ray)
    {
      TItem result;
      var seekResult = InternalSeek(ray);
      if (seekResult.ResultType == SeekResultType.Exact || seekResult.ResultType == SeekResultType.Nearest)
        result = seekResult.Pointer.Current;
      else
        result = default(TItem);
      return new SeekResult<TItem>(seekResult.ResultType, result);
    }

    /// <inheritdoc/>
    public override IIndexReader<TItem,TItem> CreateReader(Range<IEntire<TItem>> range)
    {
      return new SortedCollectionReader<TItem>(this, range);
    }

    #endregion

    #region Modification methods: Add, Remove, etc.

    /// <inheritdoc/>
    public override void Add(TItem item)
    {
      Changed();
      int index = items.BinarySearch(item, KeyComparer.Implementation);
      if (index < 0)
        items.Insert(~index, item);
      else
        items.Insert(index, item);
    }

    /// <inheritdoc/>
    public override bool Remove(TItem item)
    {
      Changed();
      int index = items.BinarySearch(item, KeyComparer.Implementation);
      if (index < 0)
        return false;
      items.RemoveAt(index);
      return true;
    }

    /// <inheritdoc/>
    public override bool RemoveKey(TItem key)
    {
      return Remove(key);
    }

    /// <summary>
    /// Removes the range of items.
    /// </summary>
    /// <param name="index">Index of the first item to remove.</param>
    /// <param name="count">Count of items to remove.</param>
    public void RemoveRange(int index, int count)
    {
      Changed();
      items.RemoveRange(index, count);
    }

    /// <inheritdoc/>
    public override void Replace(TItem item)
    {
      if (!Remove(item))
        throw new ArgumentOutOfRangeException("item");
      Add(item);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      Cleared();
      items.Clear();
    }

    #endregion

    #region Helper methods: CopyTo, BinarySearch

    /// <inheritdoc/>
    public void CopyTo(TItem[] array, int arrayIndex)
    {
      items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Performs binary search for the specified item.
    /// </summary>
    /// <param name="item">The item to locate.</param>
    /// <returns>Standard <see cref="List{T}.BinarySearch(T,IComparer{T})"/> result.</returns>
    public int BinarySearch(TItem item)
    {
      return items.BinarySearch(item, KeyComparer.Implementation);
    }

    #endregion

    #region Measure related methods

    /// <inheritdoc/>
    public override object GetMeasureResult(Range<IEntire<TItem>> range, string name)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(Range<IEntire<TItem>> range, params string[] names)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override object GetMeasureResult(string name)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(params string[] names)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region IHasVersion<int> methods

    public int Version
    {
      get { return version; }
    }

    object IHasVersion.Version
    {
      get { return Version; }
    }

    #endregion

    #region Private \ internal methods

    private void Changed()
    {
      version++;
    }

    private void Cleared()
    {
      version = 0;
    }

    internal SeekResultPointer<SortedCollectionPointer<TItem>> InternalSeek(Ray<IEntire<TItem>> ray)
    {
      Func<IEntire<TItem>, TItem, int> asymmetricKeyCompare = AsymmetricKeyCompare;
      SeekResultType resultType = SeekResultType.None;
      int index = 0;
      int maxIndex = items.Count - 1;
      while (index <= maxIndex) {
        int nextIndex = index + ((maxIndex - index) >> 1);
        int comparison = asymmetricKeyCompare(ray.Point, KeyExtractor(items[nextIndex]));
        if (comparison == 0) {
          index = nextIndex;
          resultType = SeekResultType.Exact;
          break;
        }
        if (comparison > 0)
          index = nextIndex + 1;
        else
          maxIndex = nextIndex - 1;
      }
      if (resultType != SeekResultType.Exact) {
        if (index < items.Count) {
          resultType = SeekResultType.Nearest;
          if (ray.Direction == Direction.Negative) {
            index--;
            if (index < 0)
              resultType = SeekResultType.None;
          }
        }
        else if (ray.Direction == Direction.Negative) {
          index = items.Count - 1;
          resultType = SeekResultType.Nearest;
        }
      }
      else {
        index -= (int)ray.Direction;
        while (index >= 0 && index < items.Count && asymmetricKeyCompare(ray.Point, KeyExtractor(items[index])) == 0)
          index -= (int)ray.Direction;
        index += (int)ray.Direction;
      }
      return new SeekResultPointer<SortedCollectionPointer<TItem>>(resultType, 
        new SortedCollectionPointer<TItem>(this, index));
    }

    #endregion

   
    // Constructors

    /// <inheritdoc/>
    public SortedCollection()
    {
    }

    /// <inheritdoc/>
    public SortedCollection(IndexConfigurationBase<TItem, TItem> configuration)
      : base(configuration)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="configureByDefault"><see langword="True"/>, if collection should be configured by default;
    /// otherwise, <see langword="false"/>.</param>
    public SortedCollection(bool configureByDefault) 
      : this()
    {
      if (configureByDefault)
        Configure(new IndexConfigurationBase<TItem, TItem>(noKeyExtractor, AdvancedComparer<TItem>.System));
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="comparer">Item comparer.</param>
    public SortedCollection(AdvancedComparer<TItem> comparer)
    {
      Configure(new IndexConfigurationBase<TItem, TItem>(noKeyExtractor, comparer));
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="enumerable">The initial collection content.</param>
    /// <param name="comparer">Item comparer.</param>
    public SortedCollection(IEnumerable<TItem> enumerable, AdvancedComparer<TItem> comparer)
      : this(comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(enumerable, "enumerable");
      items.AddRange(enumerable);
      items.Sort(comparer.Implementation);
    }
  }
}
