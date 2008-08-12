// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.08.28

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Measures;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  partial class Index<TKey, TItem>
  {
    /// <inheritdoc/>
    public override IMeasureSet<TItem> Measures
    {
      [DebuggerStepThrough]
      get { return DescriptorPage.Measures; }
    }


    // IMeasureable<...> methods

    /// <inheritdoc/>
    public override object GetMeasureResult(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      EnsureConfigured();

      if (!HasMeasures)
        throw new NotSupportedException(String.Format(
          Strings.ExMeasureIsNotDefined, name));
      var measure = Measures[name];
      if (measure!=null)
        return RootPage.MeasureResults[Measures.IndexOf(measure)].Result;
      throw new NotSupportedException(String.Format(Strings.ExMeasureIsNotDefined, name));
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(params string[] names)
    {
      object[] result = new object[names.Length];
      for (int i = 0; i < names.Length; i++)
        result[i] = GetMeasureResult(names[i]);
      return result;
    }


    // IRangeMeasureable<...> methods

    /// <inheritdoc/>
    public override object GetMeasureResult(Range<IEntire<TKey>> range, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(range, "range");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      EnsureConfigured();

      IMeasure<TItem> measure = Measures.GetItem<IMeasure<TItem>>(name);
      if (measure==null)
        throw new InvalidOperationException(String.Format(Strings.ExMeasureIsNotDefined, name));
      if (range.IsEmpty)
        return measure.CreateNew().Result;

      int index = Measures.IndexOf(measure);
      Range<IEntire<TKey>> positiveRange = range.Redirect(Direction.Positive, Configuration.EntireKeyComparer);
      var rangeLeftPtr  = InternalSeek(RootPage, new Ray<IEntire<TKey>>(positiveRange.EndPoints.First));
      var rangeRightPtr = InternalSeek(RootPage, new Ray<IEntire<TKey>>(positiveRange.EndPoints.Second, Direction.Negative));
      if (rangeLeftPtr.ResultType==SeekResultType.None || rangeRightPtr.ResultType==SeekResultType.None) {
        return measure.Result; // Empty result.
      }

      LeafPage<TKey, TItem> firstPage = rangeLeftPtr.Pointer.Page;
      int firstPageIndex = rangeLeftPtr.Pointer.Index;
      LeafPage<TKey, TItem> lastPage = rangeRightPtr.Pointer.Page;
      int lastPageIndex = rangeRightPtr.Pointer.Index;
      if (firstPage==lastPage || firstPage.RightPage==lastPage)
        return MeasureUtils<TItem>.BatchCalculate(measure, GetItems(positiveRange)).Result; // Results within one similar page or witihin neighbour pages
      
      // Add intermediate measurements to result
      IMeasure<TItem> result = measure.CreateNew();
      foreach (TItem item in firstPage.GetItems(firstPageIndex, firstPage.CurrentSize - firstPageIndex))
        result.Add(item);
      foreach (TItem item in lastPage.GetItems(0, lastPageIndex + 1))
        result.Add(item);
      LeafPage<TKey, TItem> intermediatePage = rangeLeftPtr.Pointer.Page.RightPage;
      while (intermediatePage!=rangeRightPtr.Pointer.Page) {
        result.AddWith(intermediatePage.MeasureResults[index]);
        intermediatePage = intermediatePage.RightPage;
      }
      return result.Result;
    }

    /// <inheritdoc/>
    public override object[] GetMeasureResults(Range<IEntire<TKey>> range, params string[] names)
    {
      object[] result = new object[names.Length];
      for (int i = 0; i < names.Length; i++)
        result[i] = GetMeasureResult(range, names[i]);
      return result;
    }
  }
}