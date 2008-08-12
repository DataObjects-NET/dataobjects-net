// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal sealed class SelectProvider : UnaryExecutableProvider
  {
    private readonly int[] columnIndexes;
    private MapTransform transform;

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var result = new List<Tuple>();
      foreach (var tuple in Source.Enumerate(context))
        result.Add(transform.Apply(TupleTransformType.Auto, tuple));
      return result;
    }

    protected override void Initialize()
    {
      base.Initialize();
      transform = new MapTransform(true, Header.TupleDescriptor, columnIndexes);
    }


    // Constructors

    public SelectProvider(CompilableProvider origin, ExecutableProvider source, int[] columnIndexes)
      : base(origin, source)
    {
      this.columnIndexes = columnIndexes;
    }
  }
}