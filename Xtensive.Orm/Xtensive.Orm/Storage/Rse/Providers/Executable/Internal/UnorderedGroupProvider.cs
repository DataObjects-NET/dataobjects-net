// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.16

using System;
using System.Collections.Generic;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal class UnorderedGroupProvider : UnaryExecutableProvider<Compilable.AggregateProvider>
  {

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var groupMapping = new Dictionary<Tuple, int>();
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
        var resultTuple = Origin.Transform.Apply(TupleTransformType.Auto, tuple);
        int groupIndex;
        if (!groupMapping.TryGetValue(resultTuple, out groupIndex)){
          groupIndex = groupMapping.Count;
          groupMapping.Add(resultTuple, groupIndex);
          result.Add(Tuple.Create(Origin.Header.TupleDescriptor));
          resultTuple.CopyTo(result[groupIndex]);
        }
        foreach (var col in Origin.AggregateColumns)
          actionList[col.Index - Origin.GroupColumnIndexes.Length].Invoke(tuple, groupIndex);
      }

      // TODO: optimize: cache delegates

      // Computing final value for each aggregate
      foreach (var groupIndex in groupMapping.Values) {
        foreach (var col in Origin.AggregateColumns)
          result[groupIndex] = (Tuple) typeof (AggregateCalculatorProvider)
            .GetMethod("Calculate")
            .MakeGenericMethod(col.Type)
            .Invoke(calculator, new object[] {col, groupIndex, result[groupIndex]});
      }
      return result;
    }


    // Constructors

    public UnorderedGroupProvider(Compilable.AggregateProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}