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
  /// Leaf page of the <see cref="Index{TKey,TItem}"/>.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">A node value type.</typeparam>
  [Serializable]
  public sealed class LeafPage<TKey, TItem> : DataPage<TKey, TItem>
  {
    private TItem[] items;
    private IPageRef leftPageRef;
    private IPageRef rightPageRef;

    #region Properties: Size, LeftPage(Ref), RightPage(Ref)

    /// <inheritdoc/>
    public override int Size
    {
      [DebuggerStepThrough]
      get { return items.Length; }
    }

    public IPageRef LeftPageRef
    {
      [DebuggerStepThrough]
      get { return leftPageRef; }
      [DebuggerStepThrough]
      set { leftPageRef = value; }
    }

    public LeafPage<TKey, TItem> LeftPage
    {
      [DebuggerStepThrough]
      get { return (LeafPage<TKey, TItem>)Provider.Resolve(leftPageRef); }
      [DebuggerStepThrough]
      set { leftPageRef = value.Identifier; }
    }

    public IPageRef RightPageRef
    {
      [DebuggerStepThrough]
      get { return rightPageRef; }
      [DebuggerStepThrough]
      set { rightPageRef = value; }
    }

    public LeafPage<TKey, TItem> RightPage
    {
      [DebuggerStepThrough]
      get { return (LeafPage<TKey, TItem>)Provider.Resolve(rightPageRef); }
      [DebuggerStepThrough]
      set { rightPageRef = value.Identifier; }
    }

    #endregion

    /// <inheritdoc/>
    public override TKey Key
    {
      [DebuggerStepThrough]
      get { return KeyExtractor(items[0]); }
    }

    /// <inheritdoc/>
    public override TKey GetKey(int index)
    {
      return KeyExtractor(items[index]);
    }

    /// <summary>
    /// Gets the <typeparamref name="TItem"/> at the specified index.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    public TItem this[int index]
    {
      [DebuggerStepThrough]
      get { return items[index]; }
      [DebuggerStepThrough]
      internal set { items[index] = value; }
    }

    /// <summary>
    /// Gets the specified range of items on the page.
    /// </summary>
    /// <param name="offset">The offset of the range to get.</param>
    /// <param name="count">The count of items to get.</param>
    /// <returns>The specified range of items on the page.</returns>
    public IEnumerable<TItem> GetItems(int offset, int count)
    {
      // TODO: Eliminate enumeration, or the whole method
      for (int i = offset; i < offset + count; i++)
        yield return items[i];
    }

    #region Seek methods

    /// <inheritdoc/>
    public override SeekResultPointer<int> Seek(TKey key)
    {
      SeekResultType resultType = SeekResultType.None;
      int minIndex = 0;
      int maxIndex = CurrentSize - 1;
      Func<TKey, TKey, int> compare = Provider.Index.KeyComparer.Compare;
      Converter<TItem, TKey> extractor = KeyExtractor;
      while (minIndex <= maxIndex) {
        int nextIndex = minIndex + ((maxIndex - minIndex) >> 1);
        int comparison = compare(key, extractor(items[nextIndex]));
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
      if (resultType!=SeekResultType.Exact && minIndex < CurrentSize)
        resultType = SeekResultType.Nearest;
      return new SeekResultPointer<int>(resultType, minIndex);
    }

    /// <inheritdoc/>
    public override SeekResultPointer<int> Seek(Ray<Entire<TKey>> ray)
    {
      Func<Entire<TKey>, TKey, int> asymmetricKeyCompare = Provider.Index.AsymmetricKeyCompare;
      SeekResultType resultType = SeekResultType.None;
      int minIndex = 0;
      int maxIndex = CurrentSize - 1;
      while (minIndex <= maxIndex) {
        int nextIndex = minIndex + ((maxIndex - minIndex) >> 1);
        int comparison = asymmetricKeyCompare(ray.Point, KeyExtractor(items[nextIndex]));
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
      if (resultType!=SeekResultType.Exact) {
        if (minIndex < CurrentSize) {
          resultType = SeekResultType.Nearest;
          if (ray.Direction==Direction.Negative) {
            minIndex--;
            if (minIndex < 0)
              resultType = SeekResultType.None;
          }
        }
        else if (ray.Direction==Direction.Negative) {
          minIndex = CurrentSize - 1;
          resultType = SeekResultType.Nearest;
        }
      }
      return new SeekResultPointer<int>(resultType, minIndex);
    }

    #endregion

    #region Insert, Remove, Split, Merge methods

    /// <summary>
    /// Inserts the <paramref name="item"/> at the specified index.
    /// </summary>
    /// <param name="index">The index to insert the item at.</param>
    /// <param name="item">The item to insert.</param>
    public void Insert(int index, TItem item)
    {
      ArgumentValidator.EnsureArgumentIsInRange(index, 0, CurrentSize, "index");
      UpdateVersion();
      Array.Copy(items, index, items, index + 1, CurrentSize - index);
      items[index] = item;
      CurrentSize++;
    }

    /// <inheritdoc/>
    public override void Remove(int index)
    {
      UpdateVersion();
      CurrentSize--;
      Array.Copy(items, index + 1, items, index, CurrentSize - index);  
      items[CurrentSize] = default(TItem);
    }

    /// <inheritdoc/>
    public override DataPage<TKey, TItem> Split()
    {
      UpdateVersion();
      int splitIndex = DescriptorPage.PageSize/2;
      LeafPage<TKey, TItem> newPage = new LeafPage<TKey, TItem>(Provider);
      Array.Copy(items, splitIndex, newPage.items, 0, CurrentSize - splitIndex);
      Array.Clear(items, splitIndex, CurrentSize - splitIndex);
      newPage.CurrentSize = CurrentSize - splitIndex;
      CurrentSize = splitIndex;

      newPage.RightPageRef = RightPageRef;
      RightPageRef = newPage.Identifier;
      newPage.LeftPageRef = Identifier;
      if (newPage.RightPageRef!=null) {
        LeafPage<TKey, TItem> nextToRightPagePage = (LeafPage<TKey, TItem>)Provider.Resolve(newPage.RightPageRef);
        nextToRightPagePage.LeftPageRef = newPage.Identifier;
      }

      RecalculateMeasures();
      newPage.RecalculateMeasures();
      return newPage;
    }

    /// <inheritdoc/>
    public override bool Merge(DataPage<TKey, TItem> page)
    {
      LeafPage<TKey, TItem> secondPage = page.AsLeafPage;
      if ((CurrentSize + secondPage.CurrentSize) < Size) {
        // Merging into a single one
        UpdateVersion();
        secondPage.UpdateVersion();
        Array.Copy(secondPage.items, 0, items, CurrentSize, secondPage.CurrentSize);
        CurrentSize += secondPage.CurrentSize;

        if (secondPage.RightPageRef==null)
          RightPageRef = null;
        else {
          LeafPage<TKey, TItem> rightPage = secondPage.RightPage;
          RightPageRef = rightPage.Identifier;
          rightPage.LeftPageRef = Identifier;
        }

        AddToMeasures(secondPage);
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
        Array.Copy(secondPage.items, 0, items, CurrentSize, currentSizeIncrease);
        Array.Copy(secondPage.items, currentSizeIncrease, secondPage.items, 0, secondPage.CurrentSize - currentSizeIncrease);
        if (MeasureResults!=null) {
          for (int i = 0; i < currentSizeIncrease; i++) {
            TItem node = this[i + CurrentSize];
            AddToMeasures(node);
            secondPage.SubtractFromMeasures(node);
          }
        }
      }
      else {
        // this page is bigger
        // Moving 50% highers items from this page to the secondPage
        Array.Copy(secondPage.items, 0, secondPage.items, -currentSizeIncrease, secondPage.CurrentSize);
        Array.Copy(items, CurrentSize + currentSizeIncrease, secondPage.items, 0, -currentSizeIncrease);
        if (MeasureResults!=null) {
          for (int i = 0; i < -currentSizeIncrease; i++) {
            TItem node = secondPage[i];
            secondPage.AddToMeasures(node);
            SubtractFromMeasures(node);
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
        for (int i = 0; i < CurrentSize; i++)
          MeasureResults.Add(items[i]);
      }
    }

    /// <inheritdoc/>
    public override void AddToMeasures(TItem item)
    {
      if (Provider.Index.HasMeasures) {
        if (!MeasureResults.Add(item)) {
          MeasureUtils<TItem>.BatchRecalculate(MeasureResults, items.Segment(0, CurrentSize));
        }
      }
    }

    /// <inheritdoc/>
    public override void AddToMeasures(IHasMeasureResults<TItem> hasMeasures)
    {
      if (Provider.Index.HasMeasures) {
        if (!MeasureUtils<TItem>.BatchAdd(MeasureResults, hasMeasures.MeasureResults)) {
          MeasureUtils<TItem>.BatchRecalculate(MeasureResults, items.Segment(0, CurrentSize));
        }
      }
    }

    /// <inheritdoc/>
    public override void SubtractFromMeasures(TItem item)
    {
      if (Provider.Index.HasMeasures) {
        if (!MeasureResults.Subtract(item)) {
          MeasureUtils<TItem>.BatchRecalculate(MeasureResults, items.Segment(0, CurrentSize));
        }
      }
    }

    /// <inheritdoc/>
    public override void SubtractFromMeasures(IHasMeasureResults<TItem> hasMeasures)
    {
      if (Provider.Index.HasMeasures) {
        if (!MeasureUtils<TItem>.BatchSubtract(MeasureResults, hasMeasures.MeasureResults)) {
          MeasureUtils<TItem>.BatchRecalculate(MeasureResults, items.Segment(0, CurrentSize));
        }
      }
    }

    #endregion


    // Constructors

    /// <inheritdoc/>
    public LeafPage(IIndexPageProvider<TKey, TItem> provider)
      : base(provider)
    {
      items = new TItem[DescriptorPage.PageSize];
    }
  }
}