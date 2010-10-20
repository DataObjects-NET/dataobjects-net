// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.08

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Helpers;
using Xtensive.Disposing;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal
{
  internal sealed class ViewIndexReader : VirtualIndexReader
  {
    private readonly ExecutableProvider toView;
    private readonly MapTransform transform;
    private readonly IIndexReader<Tuple, Tuple> reader;

    public override Tuple Current
    {
      get { return transform.Apply(TupleTransformType.Auto, reader.Current); }
    }

    public override bool MoveNext()
    {
      return reader.MoveNext();
    }

    public override void MoveTo(Entire<Tuple> key)
    {
      reader.MoveTo(key);
    }

    public override IEnumerator<Tuple> GetEnumerator()
    {
      return new ViewIndexReader(Provider, Range, toView, transform);
    }

    public override void Dispose()
    {
      reader.DisposeSafely();
    }

    public override void Reset()
    {
      reader.Reset();
    }



    // Constructors

    public ViewIndexReader(ExecutableProvider provider, Range<Entire<Tuple>> range, ExecutableProvider toView, MapTransform transform)
      : base(provider, range)
    {
      this.toView = toView;
      this.transform = transform;
      reader = toView.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).CreateReader(range);
    }
  }
}