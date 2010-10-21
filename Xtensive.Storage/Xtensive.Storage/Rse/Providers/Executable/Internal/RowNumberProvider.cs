// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.05

using System;
using System.Collections.Generic;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal class RowNumberProvider : UnaryExecutableProvider<Compilable.RowNumberProvider>
  {

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      long rowNumber = 1;
      foreach (var tuple in Source.Enumerate(context))
      {
        var resTuple = Origin.ResizeTransform.Apply(TupleTransformType.Tuple, tuple);
        resTuple.SetValue(Origin.SystemColumn.Index, rowNumber++);
        yield return resTuple;
      }
    }


    // Constructors

    public RowNumberProvider(Compilable.RowNumberProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}