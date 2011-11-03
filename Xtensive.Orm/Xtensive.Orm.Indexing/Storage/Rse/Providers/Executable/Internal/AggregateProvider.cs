// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.11

using System;
using System.Collections.Generic;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal class AggregateProvider : UnaryExecutableProvider<Compilable.AggregateProvider>
  {
    public override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var result = new List<Tuple> { Tuple.Create(Origin.Header.TupleDescriptor) };
      var calculator = new AggregateCalculatorProvider(Origin.Header, false); 
      var actionList = new List<Action<Tuple, int>>();
      
      // Preparing actions
      foreach (var c in Origin.AggregateColumns)
        actionList.Add((Action<Tuple, int>)
          typeof(AggregateCalculatorProvider)
            .GetMethod("GetAggregateCalculator")
            .MakeGenericMethod(Source.Header.TupleDescriptor[c.SourceIndex], c.Type)
            .Invoke(calculator, new object[] { c.AggregateType, c.Index, c.SourceIndex}));

      // Calculating aggregate values
      foreach (var tuple in Source.Enumerate(context))
        foreach (var c in Origin.AggregateColumns)
          actionList[c.Index].Invoke(tuple, -1);

      // Computing final value for each aggregate
      foreach (var c in Origin.AggregateColumns)
        result[0] = (Tuple) typeof(AggregateCalculatorProvider)
          .GetMethod("Calculate")
          .MakeGenericMethod(c.Type)
          .Invoke(calculator, new object[] { c, -1, result[0] });

      return result;
    }


    // Constructors

    public AggregateProvider(Compilable.AggregateProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}