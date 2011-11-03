// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.12

using System;
using System.Collections.Generic;
using Xtensive.Arithmetic;
using Xtensive.Comparison;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal sealed class AggregateCalculatorProvider
  {
    private readonly Tuple[] accumulators;
    private readonly List<Tuple>[] accumulatorGroups;

    public Action<Tuple, int> GetAggregateCalculator<TSource,TResult>(AggregateType aggregateType, int columnIndex, int sourceIndex)
    {
      switch (aggregateType) {
        case AggregateType.Count:
          return Count(columnIndex);
        case AggregateType.Avg:
          return Avg<TSource,TResult>(columnIndex, sourceIndex);
        case AggregateType.Min:
          return Min<TResult>(columnIndex, sourceIndex);
        case AggregateType.Max:
          return Max<TResult>(columnIndex, sourceIndex);
        case AggregateType.Sum:
          return Sum<TResult>(columnIndex, sourceIndex);
        default:
          throw new ArgumentOutOfRangeException("aggregateType");
      }
    }

    #region Get\StoreAccumulator methods

    private Tuple GetAccumulator(int columnIndex, int groupIndex)
    {
      if (groupIndex<0)
        return accumulators[columnIndex];
      var list = accumulatorGroups[columnIndex];
      var count = list.Count;
      while (count <= groupIndex) {
        list.Add(null);
        count++;
      }
      return list[groupIndex];
    }

    private void StoreAccumulator(int columnIndex, int groupIndex, Tuple value)
    {
      if (groupIndex<0)
        accumulators[columnIndex] = value;
      else
        accumulatorGroups[columnIndex][groupIndex] = value;
    }

    #endregion

    #region Aggregating methods

    private Action<Tuple, int> Count(int columnIndex)
    {
      return delegate(Tuple src, int groupIndex) {
        var acc = GetAccumulator(columnIndex, groupIndex);
        if (acc == null) {
          acc = Tuple.Create(new[] { typeof(long) });
          acc.SetValue(0, (long) 0);
        }
        acc.SetValue(0, acc.GetValue<long>(0) + 1);
        StoreAccumulator(columnIndex, groupIndex, acc);
      };
    }

    private Action<Tuple, int> Max<TResult>(int columnIndex, int sourceIndex)
    {
      var comparer = AdvancedComparer<TResult>.Default.Compare;
      return delegate(Tuple src, int groupIndex) {
        var acc = GetAccumulator(columnIndex, groupIndex);
        if (src.GetValue(sourceIndex) != null)
          if (acc == null) {
            acc = Tuple.Create(src.GetValue<TResult>(sourceIndex));
            StoreAccumulator(columnIndex, groupIndex, acc);
          }
          else 
            if (comparer.Invoke(src.GetValue<TResult>(sourceIndex), acc.GetValue<TResult>(0)) > 0) {
              acc.SetValue(0, src.GetValue<TResult>(sourceIndex));
              StoreAccumulator(columnIndex, groupIndex, acc);
            }
      };
    }

    private Action<Tuple, int> Min<TResult>(int columnIndex, int sourceIndex)
    {
      var comparer = AdvancedComparer<TResult>.Default.Compare;
      return delegate(Tuple src, int groupIndex) {
        var acc = GetAccumulator(columnIndex, groupIndex);
        if (src.GetValue(sourceIndex) != null)
          if (acc == null) {
            acc = Tuple.Create(src.GetValue<TResult>(sourceIndex));
            StoreAccumulator(columnIndex, groupIndex, acc);
          }
          else
            if (comparer.Invoke(src.GetValue<TResult>(sourceIndex), acc.GetValue<TResult>(0)) < 0) {
              acc.SetValue(0, src.GetValue<TResult>(sourceIndex));
              StoreAccumulator(columnIndex, groupIndex, acc);
            }
      };
    }

    private Action<Tuple, int> Sum<TResult>(int columnIndex, int sourceIndex)
    {
      var adder = Arithmetic<TResult>.Default.Add;
      return delegate(Tuple src, int groupIndex) {
        var acc = GetAccumulator(columnIndex, groupIndex);
        if (src.GetValue(sourceIndex) != null)
          if (acc==null)
            acc = Tuple.Create(src.GetValue<TResult>(sourceIndex));
          else
            acc.SetValue(0, adder.Invoke(acc.GetValue<TResult>(0), src.GetValue<TResult>(sourceIndex)));
        StoreAccumulator(columnIndex, groupIndex, acc);
      };
    }

    private Action<Tuple, int> Avg<TSource,TResult>(int columnIndex, int sourceIndex)
    {
      var adder = Arithmetic<TResult>.Default.Add;
      if (typeof(TSource) == typeof(TResult))
        return delegate(Tuple src, int groupIndex) {
          var acc = GetAccumulator(columnIndex, groupIndex);
          if (src.GetValue(sourceIndex) != null)
            if (acc==null)
              acc = Tuple.Create((long)1, (TResult)(object)src.GetValue<TSource>(sourceIndex));
            else {
              acc.SetValue(0, acc.GetValue<long>(0) + 1);
              acc.SetValue(1, adder.Invoke(acc.GetValue<TResult>(1), (TResult)(object)src.GetValue<TSource>(sourceIndex)));
            }
          StoreAccumulator(columnIndex, groupIndex, acc);
        };
      return delegate(Tuple src, int groupIndex) {
        var acc = GetAccumulator(columnIndex, groupIndex);
        if (src.GetValue(sourceIndex) != null)
          if (acc == null)
            acc = Tuple.Create((long)1, Convert.ToDouble(src.GetValue<TSource>(sourceIndex)));
          else {
            acc.SetValue(0, acc.GetValue<long>(0) + 1);
            acc.SetValue(1, adder.Invoke(acc.GetValue<TResult>(1), (TResult)(object)Convert.ToDouble(src.GetValue<TSource>(sourceIndex))));
          }
        StoreAccumulator(columnIndex, groupIndex, acc);
      };
    }

    #endregion

    public Tuple Calculate<T>(AggregateColumn column, int groupIndex, Tuple result)
    {
      var acc = GetAccumulator(column.Index, groupIndex);
      switch (column.AggregateType) {
        case AggregateType.Avg:
          if (acc!=null)
            result.SetValue(column.Index, Arithmetic<T>.Default.Divide(acc.GetValue<T>(1), Convert.ToDouble(acc.GetValue(0))));
          else
            result.SetValue(column.Index, null);
          return result;
        case AggregateType.Count:
          if (acc!=null)
            result.SetValue(column.Index, acc.GetValue(0));
          else {
            result.SetValue(column.Index, (long) 0);
          }
          return result;
        case AggregateType.Sum:
          if (acc != null)
            result.SetValue(column.Index, acc.GetValue(0));
          else
            result.SetValue(column.Index, default(T));
          return result;
        case AggregateType.Max:
        case AggregateType.Min:
          if (acc!=null)
            result.SetValue(column.Index, acc.GetValue(0));
          else
            result.SetValue(column.Index, null);
          return result;
        default:
          throw new ArgumentOutOfRangeException("column.AggregateType");
      }
    }


    // Constructors

    public AggregateCalculatorProvider(RecordSetHeader header, bool isGrouping)
    {
      var count = header.Columns.Count;
      if (!isGrouping)
        accumulators = new Tuple[count];
      else {
        accumulatorGroups = new List<Tuple>[count];
        for (int i = 0; i < count; i++)
          accumulatorGroups[i] = new List<Tuple>();
      }
    }
  }
}