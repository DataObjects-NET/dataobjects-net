// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Undead
// Created:    2008.06.04

using System;
using System.Collections.Generic;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal
{
  internal sealed class FilterIndexReader : VirtualIndexReader
  {
    private readonly ExecutableProvider toFilter;
    private readonly Func<Tuple, bool> predicate;
    private readonly IIndexReader<Tuple, Tuple> reader;

    public override Tuple Current
    {
      get { return reader.Current; }
    }

    public override bool MoveNext()
    {
      while (reader.MoveNext())
        if (predicate(reader.Current))
          return true;
      return false;
    }

    public override void MoveTo(Entire<Tuple> key)
    {
      reader.MoveTo(key);
    }

    public override void Reset()
    {
      reader.Reset();
    }

    public override IEnumerator<Tuple> GetEnumerator()
    {
      return new FilterIndexReader(Provider, Range, toFilter, predicate);
    }


    // Constructors

    public FilterIndexReader(ExecutableProvider provider, Range<Entire<Tuple>> range, ExecutableProvider toFilter, Func<Tuple,bool> predicate)
      : base(provider, range)
    {
      this.toFilter = toFilter;
      this.predicate = predicate;
      var orderedEnumerable = toFilter.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      reader = orderedEnumerable.CreateReader(range);
    }

    public override void Dispose()
    {
      reader.DisposeSafely();
    }
  }
}