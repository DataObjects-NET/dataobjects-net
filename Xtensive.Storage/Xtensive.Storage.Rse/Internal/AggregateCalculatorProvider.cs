// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.12

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Arithmetic;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Rse
{
  internal sealed class AggregateCalculatorProvider
  {
    private readonly Tuple[] accumulators;
    private readonly List<Tuple>[] accumulatorGroups;

    public RecordSetHeader Header { get; private set; }

    public Action<Tuple, int> GetAggregateCalculator<T>(AggregateType aggregateType, int columnIndex, int sourceIndex)
    {
      switch (aggregateType) {
      case AggregateType.Count:
        return Count(columnIndex);
      case AggregateType.Avg:
        return Avg<T>(columnIndex, sourceIndex);
      case AggregateType.Min:
        return Min<T>(columnIndex, sourceIndex);
      case AggregateType.Max:
        return Max<T>(columnIndex, sourceIndex);
      case AggregateType.Sum:
        return Sum<T>(columnIndex, sourceIndex);
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

    private Action<Tuple, int> Max<T>(int columnIndex, int sourceIndex)
    {
      var comparer = AdvancedComparer<T>.Default.Compare;
      return delegate(Tuple src, int groupIndex) {
        var acc = GetAccumulator(columnIndex, groupIndex);
        if (src.GetValue(sourceIndex) != null)
          if (acc == null) {
            acc = Tuple.Create(src.GetValue<T>(sourceIndex));
            StoreAccumulator(columnIndex, groupIndex, acc);
          }
          else 
            if (comparer.Invoke(src.GetValue<T>(sourceIndex), acc.GetValue<T>(0)) > 0) {
              acc.SetValue(0, src.GetValue<T>(sourceIndex));
              StoreAccumulator(columnIndex, groupIndex, acc);
            }
      };
    }

    private Action<Tuple, int> Min<T>(int columnIndex, int sourceIndex)
    {
      var comparer = AdvancedComparer<T>.Default.Compare;
      return delegate(Tuple src, int groupIndex) {
        var acc = GetAccumulator(columnIndex, groupIndex);
        if (src.GetValue(sourceIndex) != null)
          if (acc == null) {
            acc = Tuple.Create(src.GetValue<T>(sourceIndex));
            StoreAccumulator(columnIndex, groupIndex, acc);
          }
          else
            if (comparer.Invoke(src.GetValue<T>(sourceIndex), acc.GetValue<T>(0)) < 0) {
              acc.SetValue(0, src.GetValue<T>(sourceIndex));
              StoreAccumulator(columnIndex, groupIndex, acc);
            }
      };
    }

    private Action<Tuple, int> Sum<T>(int columnIndex, int sourceIndex)
    {
      var adder = Arithmetic<T>.Default.Add;
      return delegate(Tuple src, int groupIndex) {
        var acc = GetAccumulator(columnIndex, groupIndex);
        if (src.GetValue(sourceIndex) != null)
          if (acc==null)
            acc = Tuple.Create(src.GetValue<T>(sourceIndex));
          else
            acc.SetValue(0, adder.Invoke(acc.GetValue<T>(0), src.GetValue<T>(sourceIndex)));
          StoreAccumulator(columnIndex, groupIndex, acc);
      };
    }

    private Action<Tuple, int> Avg<T>(int columnIndex, int sourceIndex)
    {
      var adder = Arithmetic<T>.Default.Add;
      return delegate(Tuple src, int groupIndex) {
        var acc = GetAccumulator(columnIndex, groupIndex);
        if (src.GetValue(sourceIndex) != null)
          if (acc==null)
            acc = Tuple.Create((long) 1, src.GetValue<T>(sourceIndex));
          else {
            acc.SetValue(0, acc.GetValue<long>(0) + 1);
            acc.SetValue(1, adder.Invoke(acc.GetValue<T>(1), src.GetValue<T>(sourceIndex)));
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
          result.SetValue(column.Index, Arithmetic<T>.Default.Divide(acc.GetValue<T>(1), (double)acc.GetValue<long>(0)));
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
        if (acc!=null)
          result.SetValue(column.Index, acc.GetValue(0));
        else {
          // TODO: Optimize
          var clone = Tuple.Create(result.Descriptor);
          result.SetValue(column.Index, clone.GetValueOrDefault(column.Index));
        }
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


    // Constructor

    public AggregateCalculatorProvider(RecordSetHeader header, bool isGrouping)
    {
      Header = header;
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