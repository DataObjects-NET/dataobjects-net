// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.16

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal class UnOrderedGroupProvider : UnaryExecutableProvider<Compilable.UnOrderedGroupProvider>
  {

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var groupMapping = new Dictionary<Tuple, int>();
      var calculator = new AggregateCalculatorProvider(Origin.AggregateColumns);
      var actionList = new List<Action<Tuple, Tuple, int>>();
      var result = new List<Tuple>();

      foreach (var col in Origin.AggregateColumns)
        actionList.Add((Action<Tuple, Tuple, int>)typeof(AggregateCalculatorProvider).GetMethod("GetAggregateCalculator")
            .MakeGenericMethod(col.Type).Invoke(calculator, new object[] { col.AggregateType, col.SourceIndex, col.Index}));

      foreach (var tuple in Source.Enumerate(context)){
        var resultTuple = Origin.ResizeTransform.Apply(TupleTransformType.Tuple, tuple);
        int groupIndex;
        if (!groupMapping.TryGetValue(resultTuple, out groupIndex)){
          groupIndex = groupMapping.Count;
          groupMapping.Add(resultTuple, groupIndex);
          result.Add(Tuple.Create(Origin.Header.TupleDescriptor));
          resultTuple.CopyTo(result[groupIndex]);
        }
        foreach (var col in Origin.AggregateColumns) {
          actionList[col.Index - Origin.GroupColumnIndexes.Length](tuple, calculator.GetAccumulator(col.Index, groupIndex), groupIndex);
        }
      }

      foreach (var group in groupMapping.Values) {
        foreach (var col in Origin.AggregateColumns)
          result[group] = (Tuple) typeof (AggregateCalculatorProvider).GetMethod("Calculate")
            .MakeGenericMethod(col.Type).Invoke(calculator, new object[] {col, calculator.GetAccumulator(col.Index, group), result[group]});
      }
      return result;
    }


    // Constructor

    public UnOrderedGroupProvider(Compilable.UnOrderedGroupProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }

  }
}