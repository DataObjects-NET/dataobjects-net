// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System.Collections.Generic;
using System.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal sealed class SelectProvider : UnaryExecutableProvider<Compilable.SelectProvider>
  {
    private readonly int[] columnIndexes;
    private MapTransform transform;

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context).Select(t => transform.Apply(TupleTransformType.Auto, t));
    }

    protected override void Initialize()
    {
      base.Initialize();
      transform = new MapTransform(true, Header.TupleDescriptor, columnIndexes);
    }


    // Constructors

    public SelectProvider(Compilable.SelectProvider origin, ExecutableProvider source, int[] columnIndexes)
      : base(origin, source)
    {
      this.columnIndexes = columnIndexes;
    }
  }
}