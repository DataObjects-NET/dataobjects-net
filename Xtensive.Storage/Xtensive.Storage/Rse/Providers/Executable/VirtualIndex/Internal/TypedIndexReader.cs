// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.26

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Helpers;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal
{
  [Serializable]
  internal class TypedIndexReader: VirtualIndexReader
  {
    private readonly ExecutableProvider toView;
    private readonly Func<Tuple, Tuple> typeInjector;
    private readonly IIndexReader<Tuple, Tuple> reader;

    public override Tuple Current
    {
      get { return typeInjector(reader.Current); }
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
      return new TypedIndexReader(Provider, Range, toView, typeInjector);
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

    public TypedIndexReader(ExecutableProvider provider, Range<Entire<Tuple>> range, ExecutableProvider toView, Func<Tuple,Tuple> typeInjector)
      : base(provider, range)
    {
      this.toView = toView;
      this.typeInjector = typeInjector;
      reader = toView.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).CreateReader(range);
    }
  }
}