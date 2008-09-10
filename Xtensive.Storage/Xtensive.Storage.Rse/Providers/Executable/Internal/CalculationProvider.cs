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
  internal class CalculationProvider : UnaryExecutableProvider<Compilable.CalculationProvider>
  {
    private MapTransform transform;

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      foreach (var tuple in Source.Enumerate(context)) {
        var resTuple = transform.Apply(TupleTransformType.Tuple, tuple);
        foreach (var col in Origin.CalculatedColumns)
          resTuple.SetValue(col.Index, col.Expression(tuple));
        yield return resTuple;
      }
    }

    protected override void Initialize()
    {
      base.Initialize();
      var columnIndexes = new int[Origin.Header.Columns.Count];
      for (int i = 0; i < columnIndexes.Length; i++)
        columnIndexes[i] = (i < Source.Header.Columns.Count) ? i : MapTransform.NoMapping;
      transform = new MapTransform(false, Origin.Header.TupleDescriptor, columnIndexes);
    }


    // Constructor

    public CalculationProvider(Compilable.CalculationProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}