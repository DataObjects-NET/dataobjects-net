// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal class CalculateProvider : UnaryExecutableProvider<Compilable.CalculateProvider>
  {

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      foreach (var tuple in Source.Enumerate(context)) {
        var resTuple = Origin.ResizeTransform.Apply(TupleTransformType.Tuple, tuple);
        foreach (var col in Origin.CalculatedColumns)
          resTuple.SetValue(col.Index, col.Expression.Compile().Invoke(tuple));
        yield return resTuple;
      }
    }


    // Constructor

    public CalculateProvider(Compilable.CalculateProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}