// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal class CalculateProvider : UnaryExecutableProvider<Compilable.CalculateProvider>
  {
    private readonly List<Func<Tuple, object>> calculators;

    /// <inheritdoc/>
    public override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      foreach (var tuple in Source.Enumerate(context)) {
        var resTuple = Origin.ResizeTransform.Apply(TupleTransformType.Tuple, tuple);
        for (int i = 0; i < Origin.CalculatedColumns.Length; i++)
          resTuple.SetValue(Origin.CalculatedColumns[i].Index, calculators[i].Invoke(tuple));
        yield return resTuple;
      }
    }


    // Constructors

    public CalculateProvider(Compilable.CalculateProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
      calculators = Origin.CalculatedColumns.Select(c => c.Expression.CachingCompile()).ToList();
    }
  }
}