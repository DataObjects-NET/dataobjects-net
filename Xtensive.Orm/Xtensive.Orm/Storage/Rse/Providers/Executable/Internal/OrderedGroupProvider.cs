// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.18

using System;
using System.Collections.Generic;
using Xtensive.Comparison;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal class OrderedGroupProvider : UnaryExecutableProvider<Compilable.AggregateProvider>
  {
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      Tuple lastTuple = null;
      int lastGroupIndex = -1;
      var calculator = new AggregateCalculatorProvider(Origin.Header, true);
      var actionList = new List<Action<Tuple, int>>();
      var result = new List<Tuple>();

      // Preparing actions
      foreach (var c in Origin.AggregateColumns)
        actionList.Add((Action<Tuple, int>)
          typeof(AggregateCalculatorProvider)
            .GetMethod("GetAggregateCalculator")
            .MakeGenericMethod(Source.Header.TupleDescriptor[c.SourceIndex], c.Type)
            .Invoke(calculator, new object[] { c.AggregateType, c.Index, c.SourceIndex }));


      // TODO: optimize with yield return

      // Calculating aggregate values
      foreach (var tuple in Source.Enumerate(context)){
        var resultTuple = Origin.Transform.Apply(TupleTransformType.Tuple, tuple);
        if (!AdvancedComparer<Tuple>.Default.Equals(lastTuple,resultTuple)){
          lastGroupIndex++;
          result.Add(Tuple.Create(Origin.Header.TupleDescriptor));
          resultTuple.CopyTo(result[lastGroupIndex]);
          lastTuple = resultTuple;
        }
        foreach (var col in Origin.AggregateColumns)
          actionList[col.Index - Origin.GroupColumnIndexes.Length].Invoke(tuple, lastGroupIndex);
      }

      // TODO: optimize: cache delegates

      // Computing final value for each aggregate
      for (int groupIndex = 0; groupIndex <= lastGroupIndex; groupIndex++) {
        foreach (var c in Origin.AggregateColumns)
          result[groupIndex] = (Tuple) typeof(AggregateCalculatorProvider)
            .GetMethod("Calculate")
            .MakeGenericMethod(c.Type)
            .Invoke(calculator, new object[] { c, groupIndex, result[groupIndex] });
      }

      return result;
    }


    // Constructors

    public OrderedGroupProvider(Compilable.AggregateProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }

  }
}