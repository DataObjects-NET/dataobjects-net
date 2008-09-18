// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.12

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Arithmetic;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Rse
{
  internal class AggregateCalculatorProvider
  {
    private readonly Dictionary<int,List<Tuple>> accumulatorSet;

    public AggregateColumn[] Columns { get; set; }

    public Action<Tuple, Tuple, int> GetAggregateCalculator<T>(AggregateType type, int sourceIndex, int resultIndex)
    {
      Action<Tuple, Tuple, int> action = null;

      switch (type) {
        case AggregateType.Avg:
          action = Avg<T>(sourceIndex, resultIndex);
          break;

        case AggregateType.Count:
          action = Count(sourceIndex, resultIndex);
          break;

        case AggregateType.Max:
          action = Max<T>(sourceIndex, resultIndex);
        break;

        case AggregateType.Min:
        action = Min<T>(sourceIndex, resultIndex);
          break;

        case AggregateType.Sum:
          action = Sum<T>(sourceIndex, resultIndex);
          break;
      }

      return action;
    }

    public Tuple GetAccumulator(int resultIndex, int groupIndex)
    {
      int count = groupIndex - accumulatorSet[resultIndex].Count;
      if (count >= 0)
        for (int i=0; i<=count; i++)
          accumulatorSet[resultIndex].Add(null);
      return accumulatorSet[resultIndex][groupIndex];
    }

    public Tuple GetAccumulator(int resultIndex)
    {
      if (accumulatorSet[resultIndex].Count == 0)
        accumulatorSet[resultIndex].Add(null);
      return accumulatorSet[resultIndex][0];
    }

    public Tuple Calculate<T>(AggregateColumn column, Tuple accumulator, Tuple result)
    {
      if (accumulator!=null)
        if (column.AggregateType == AggregateType.Avg)
          result.SetValue(column.Index, Arithmetic<T>.Default.Divide(accumulator.GetValue<T>(1), (double)accumulator.GetValue<long>(0)));
        else
          result.SetValue(column.Index, accumulator.GetValue(0));
      else
        result.SetValue(column.Index, null);
      return result;
    }

    #region Private/Internal methods.

    private Action<Tuple, Tuple, int> Count(int s, int r)
    {
      return delegate(Tuple src, Tuple acc, int g) {
        if (acc == null) {
          acc = Tuple.Create(new[] { typeof(long) });
          acc.SetValue(0, (long)0);
        }
        acc.SetValue(0, acc.GetValue<long>(0) + 1);
        accumulatorSet[r][g] = acc;
      };
    }

    private Action<Tuple, Tuple, int> Max<T>(int s, int r)
    {
      return delegate(Tuple src, Tuple acc, int g) {
        if (src.GetValue(s) != null)
          if (acc == null) {
            acc = Tuple.Create(src.GetValue<T>(s));
            accumulatorSet[r][g] = acc;
          }
          else 
            if (Comparer.Default.Compare(src.GetValue<T>(s), acc.GetValue<T>(0)) > 0) {
              acc.SetValue(0, src.GetValue<T>(s));
              accumulatorSet[r][g] = acc;
            }
      };
    }

    private Action<Tuple, Tuple, int> Min<T>(int s, int r)
    {
      return delegate(Tuple src, Tuple acc, int g) {
        if (src.GetValue(s) != null)
          if (acc == null) {
            acc = Tuple.Create(src.GetValue<T>(s));
            accumulatorSet[r][g] = acc;
          }
          else
            if (Comparer.Default.Compare(src.GetValue<T>(s), acc.GetValue<T>(0)) < 0) {
              acc.SetValue(0, src.GetValue<T>(s));
              accumulatorSet[r][g] = acc;
            }
      };
    }

    private Action<Tuple, Tuple, int> Sum<T>(int s, int r)
    {
      return delegate(Tuple src, Tuple acc, int g) {
        if (src.GetValue(s) != null)
          if (acc == null) {
            acc = Tuple.Create(src.GetValue<T>(s));
            accumulatorSet[r][g] = acc;
          }
          else {
            acc.SetValue(0, Arithmetic<T>.Default.Add(acc.GetValue<T>(0), src.GetValue<T>(s)));
            accumulatorSet[r][g] = acc;
          }
      };
    }

    private Action<Tuple, Tuple, int> Avg<T>(int s, int r)
    {
      return delegate(Tuple src, Tuple acc, int g) {
        if (src.GetValue(s) != null)
          if (acc == null) {
            acc = Tuple.Create((long)1, src.GetValue<T>(s));
            accumulatorSet[r][g] = acc;
          }
          else {
            acc.SetValue(0, acc.GetValue<long>(0) + 1);
            acc.SetValue(1, Arithmetic<T>.Default.Add(acc.GetValue<T>(1), src.GetValue<T>(s)));
            accumulatorSet[r][g] = acc;
          }
      };
    }

    #endregion


    // Constructor

    public AggregateCalculatorProvider(AggregateColumn[] columns)
    {
      Columns = columns;
      accumulatorSet = new Dictionary<int, List<Tuple>>();
      foreach (var column in columns)
        accumulatorSet.Add(column.Index,new List<Tuple>());
    }

  }
}