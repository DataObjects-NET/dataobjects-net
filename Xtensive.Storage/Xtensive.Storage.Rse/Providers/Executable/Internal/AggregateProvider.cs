// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.11

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal class AggregateProvider : UnaryExecutableProvider<Compilable.AggregateProvider>
  {
    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var result = new List<Tuple> { Tuple.Create(Origin.Header.TupleDescriptor) };
      var calculator = new AggregateCalculatorProvider(Origin.AggregateColumns);
      var actionList = new List<Action<Tuple, Tuple>>();

      foreach (var col in Origin.AggregateColumns)
        actionList.Add((Action<Tuple, Tuple>)typeof(AggregateCalculatorProvider).GetMethod("GetAggregateCalculator")
            .MakeGenericMethod(col.Type).Invoke(calculator, new object[] { col.AggregateType, col.SourceIndex, col.Index }));

      foreach (var tuple in Source.Enumerate(context))
        foreach (var col in Origin.AggregateColumns)
          actionList[col.Index](tuple, calculator.GetAccumulator(col.Index));

      foreach (var col in Origin.AggregateColumns)
        result[0] = (Tuple)typeof(AggregateCalculatorProvider).GetMethod("Calculate")
            .MakeGenericMethod(col.Type).Invoke(calculator, new object[] { col, calculator.GetAccumulator(col.Index), result[0] });
      return result;
    }


    // Constructor

    public AggregateProvider(Compilable.AggregateProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}