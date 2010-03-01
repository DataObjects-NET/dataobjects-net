// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.08.28

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Indexing.Measures;

namespace Xtensive.Indexing.Implementation
{
  /// <summary>
  /// Inner page of the <see cref="Index{TKey,TItem}"/>.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Node type.</typeparam>
  [Serializable]
  public sealed class InnerPage<TKey, TItem> : DataPage<TKey, TItem>
  {
    private readonly KeyValuePair<TKey, IPageRef>[] items;

    /// <inheritdoc/>
    public override int Size
    {
      [DebuggerStepThrough]
      get { return items.Length - 1; }
    }

    /// <inheritdoc/>
    public override TKey Key
    {
      [DebuggerStepThrough]
      get { return items[0].Key; }
    }

    /// <inheritdoc/>
    public override TKey GetKey(int index)
    {
      return items[++index].Key;
    }

    /// <summary>
    /// Gets the page reference at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index to get the page reference at.</param>
    /// <returns>Page reference at the specified <paramref name="index"/>.</returns>
    [DebuggerStepThrough]
    public IPageRef GetPageRef(int index)
    {
      return items[++index].Value;
    }

    /// <summary>
    /// Gets the <see cref="DataPage{TKey,TItem}"/> at the specified <paramref name="index"/>
    /// by resolving the <see cref="IPageRef"/> at it.
    /// </summary>
    /// <param name="index">The index to get the page at.</param>
    /// <returns><see cref="DataPage{TKey,TItem}"/> at the specified <paramref name="index"/>.</returns>
    public DataPage<TKey, TItem> GetPage(int index)
    {
      return (DataPage<TKey, TItem>)Provider.Resolve(GetPageRef(index));
    }

    /// <summary>
    /// The indexer.
    /// </summary>
    /// <param name="index">The index to get the value for.</param>
    /// <returns>A pair of key-reference at the specified index.</returns>
    public KeyValuePair<TKey, IPageRef> this[int index]
    {
      [DebuggerStepThrough]
      get { return items[++index]; }
      [DebuggerStepThrough]
      internal set { items[++index] = value; }
    }

    #region Seek methods

    /// <inheritdoc/>
    public override SeekResultPointer<int> Seek(TKey key)
    {
      SeekResultType resultType = SeekResultType.None;
      int index = 1;
      int maxIndex = CurrentSize;
      Func<TKey, TKey, int> compare = Provider.Index.KeyComparer.Compare;
      while (index <= maxIndex) {
        int nextIndex = index + ((maxIndex - index) >> 1);
        int comparison = compare(key, items[nextIndex].Key);
        if (comparison==0) {
          index = nextIndex;
          resultType = SeekResultType.Exact;
          break;
        }
        if (comparison > 0)
          index = nextIndex + 1;
        else
          maxIndex = nextIndex - 1;
      }

      index --; // fix up items shift

      if (resultType!=SeekResultType.Exact) {
        index--; // move to previous because by default binary search result points to next greatest item, but we should point to previous lowest.
        if (index < CurrentSize)
          resultType = SeekResultType.Nearest;
      }
      return new SeekResultPointer<int>(resultType, index);
    }

    /// <inheritdoc/>
    public override SeekResultPointer<int> Seek(Ray<Entire<TKey>> ray)
    {
      Func<Entire<TKey>, TKey, int> asymmetricKeyCompare = Provider.Index.AsymmetricKeyCompare;
      SeekResultType resultType = SeekResultType.None;
      int minIndex = 1;
      int maxIndex = CurrentSize;
      while (minIndex <= maxIndex) {
        int nextIndex = minIndex + ((maxIndex - minIndex) >> 1);
        int comparison = asymmetricKeyCompare(ray.Point, items[nextIndex].Key);
        if (comparison==0) {
          minIndex = nextIndex;
          resultType = SeekResultType.Exact;
          break;
        }
        if (comparison > 0)
          minIndex = nextIndex + 1;
        else
          maxIndex = nextIndex - 1;
      }

      minIndex--; // fix items shift
      if (resultType!=SeekResultType.Exact) {
        minIndex--; // move to previous because by default binary search result points to next greatest item, but we should point to previous lowest
        resultType = SeekResultType.Nearest;
      }
      return new SeekResultPointer<int>(resultType, minIndex);
    }

    #endregion

    #region Insert, Remove, Split, Merge methods

    /// <summary>
    /// Inserts the <paramref name="key"/>-<paramref name="pageRef"/> pair at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index to insert at.</param>
    /// <param name="key">The key to insert.</param>
    /// <param name="pageRef">The page reference to insert.</param>
    public void Insert(int index, TKey key, IPageRef pageRef)
    {
      UpdateVersion();
      index++; // To fix -1 offset
      if (index <= CurrentSize)
        Array.Copy(items, index, items, index + 1, CurrentSize - index + 1);
      items[index] = new KeyValuePair<TKey, IPageRef>(key, pageRef);
      if (index!=0)
        CurrentSize++;
    }

    /// <inheritdoc/>
    public override void Remove(int index)
    {
      UpdateVersion();
      index++;
      CurrentSize--;
      Array.Copy(items, index + 1, items, index, CurrentSize - index + 1);
      items[CurrentSize + 1] = default(KeyValuePair<TKey, IPageRef>);
    }

    /// <inheritdoc/>
    public override DataPage<TKey, TItem> Split()
    {
      UpdateVersion();
      int splitIndex = DescriptorPage.PageSize / 2;
      InnerPage<TKey, TItem> newPage = new InnerPage<TKey, TItem>(Provider);
      Array.Copy(items, splitIndex + 1, newPage.items, 0, CurrentSize - splitIndex);
      Array.Clear(items, splitIndex + 1, CurrentSize - splitIndex);
      newPage.CurrentSize = CurrentSize - splitIndex - 1;
      CurrentSize = splitIndex;
      RecalculateMeasures();
      newPage.RecalculateMeasures();
      return newPage;
    }

    /// <inheritdoc/>
    public override bool Merge(DataPage<TKey, TItem> page)
    {
      InnerPage<TKey, TItem> secondPage = page.AsInnerPage;
      if (CurrentSize + secondPage.CurrentSize + 1 < Size) {
        // Merging into a single one
        UpdateVersion();
        secondPage.UpdateVersion();
        Array.Copy(secondPage.items, 0, items, CurrentSize + 1, secondPage.CurrentSize + 1);
        CurrentSize += secondPage.CurrentSize + 1;
        AddToMeasures(page);
        return true;
      }
      int currentSizeIncrease = (secondPage.CurrentSize - CurrentSize) >> 1;
      if (currentSizeIncrease==0)
        return false;
      UpdateVersion();
      secondPage.UpdateVersion();
      if (currentSizeIncrease > 0) {
        // secondPage is bigger
        // Moving 50% lowest items from the secondPage to this one
        Array.Copy(secondPage.items, 0, items, CurrentSize + 1, currentSizeIncrease);
        Array.Copy(secondPage.items, currentSizeIncrease, secondPage.items, 0, secondPage.CurrentSize + 1 - currentSizeIncrease);
        if (MeasureResults!=null) {
          for (int i = 0; i < currentSizeIncrease; i++) {
            DataPage<TKey, TItem> childPage = Provider.Resolve(items[i + CurrentSize + 1].Value).AsDataPage;
            AddToMeasures(childPage);
            secondPage.SubtractFromMeasures(childPage);
          }
        }
      }
      else {
        // this page is bigger
        // Moving 50% highers items from this page to the secondPage
        Array.Copy(secondPage.items, 0, secondPage.items, -currentSizeIncrease, secondPage.CurrentSize + 1);
        Array.Copy(items, CurrentSize + currentSizeIncrease + 1, secondPage.items, 0, -currentSizeIncrease);
        if (MeasureResults!=null) {
          for (int i = 0; i < -currentSizeIncrease; i++) {
            DataPage<TKey, TItem> childPage = Provider.Resolve(secondPage.items[i].Value).AsDataPage;
            secondPage.AddToMeasures(childPage);
            SubtractFromMeasures(childPage);
          }
        }
      }

      CurrentSize += currentSizeIncrease;
      secondPage.CurrentSize -= currentSizeIncrease;
      return false;
    }

    #endregion

    #region Measure related methods

    /// <inheritdoc/>
    public override void RecalculateMeasures()
    {
      if (Provider.Index.HasMeasures) {
        MeasureResults.Reset();
        for (int i = 0; i <= CurrentSize; i++) {
          DataPage<TKey, TItem> dataPage = Provider.Resolve(items[i].Value).AsDataPage;
          MeasureUtils<TItem>.BatchAdd(MeasureResults, dataPage.MeasureResults);
        }
      }
    }

    /// <inheritdoc/>
    public override void AddToMeasures(TItem item)
    {
      if (Provider.Index.HasMeasures) {
        if (!MeasureResults.Add(item)) {
          RecalculateMeasures();
        }
      }
    }

    /// <inheritdoc/>
    public override void SubtractFromMeasures(TItem item)
    {
      if (Provider.Index.HasMeasures) {
        if (!MeasureResults.Subtract(item)) {
          RecalculateMeasures();
        }
      }
    }

    /// <inheritdoc/>
    public override void AddToMeasures(IHasMeasureResults<TItem> hasMeasures)
    {
      if (Provider.Index.HasMeasures) {
        if (!MeasureUtils<TItem>.BatchAdd(MeasureResults, hasMeasures.MeasureResults)) {
          RecalculateMeasures();
        }
      }
    }

    /// <inheritdoc/>
    public override void SubtractFromMeasures(IHasMeasureResults<TItem> hasMeasures)
    {
      if (Provider.Index.HasMeasures) {
        if (!MeasureUtils<TItem>.BatchSubtract(MeasureResults, hasMeasures.MeasureResults)) {
          RecalculateMeasures();
        }
      }
    }

    #endregion


    // Constructors

    /// <inheritdoc/>
    public InnerPage(IIndexPageProvider<TKey, TItem> provider)
      : base(provider)
    {
      items = new KeyValuePair<TKey, IPageRef>[DescriptorPage.PageSize + 1];
    }
  }
}