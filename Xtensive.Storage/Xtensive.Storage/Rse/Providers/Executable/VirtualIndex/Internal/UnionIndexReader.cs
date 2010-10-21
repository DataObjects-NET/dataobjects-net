// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.23

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Helpers;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal
{
  internal sealed class UnionIndexReader : VirtualIndexReader
  {
    private readonly ExecutableProvider[] inheritors;
    private readonly IIndexReader<Tuple, Tuple>[] readers;
    private readonly IDisposable disposables;
    private IEnumerator<Tuple> enumerator;

    public override Tuple Current
    {
      get { return enumerator.Current; }
    }

    public override bool MoveNext()
    {
      return enumerator.MoveNext();
    }

    public override void MoveTo(Entire<Tuple> key)
    {
      foreach (var reader in readers)
        reader.MoveTo(key);
      ResetEnumerator();
    }

    public override void Reset()
    {
      foreach (var reader in readers)
        reader.Reset();
      ResetEnumerator();
    }

    public override IEnumerator<Tuple> GetEnumerator()
    {
      return new UnionIndexReader(Provider, Range, inheritors);
    }

    #region Private \ internal methods

    private void ResetEnumerator()
    {
      if (enumerator != null)
        enumerator.Dispose();
      enumerator = MergeAlgorithm.Merge(
        Provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer,
        inheritors.Select((provider, i) => new Pair<IEnumerator<Tuple>, Converter<Tuple, Tuple>>(
                                             readers[i], provider.GetService<IOrderedEnumerable<Tuple,Tuple>>(true).KeyExtractor)).ToList()
        ).GetEnumerator();
    }

    #endregion


    // Constructors

    public UnionIndexReader(ExecutableProvider provider, Range<Entire<Tuple>> range, ExecutableProvider[] inheritors)
      : base(provider, range)
    {
      this.inheritors = inheritors;
      readers = new IIndexReader<Tuple, Tuple>[inheritors.Length];
      for (int i = 0; i < inheritors.Length; i++)
        readers[i] = inheritors[i].GetService<IOrderedEnumerable<Tuple, Tuple>>(true).CreateReader(range);

      var disposablesSet = new DisposableSet(readers);
      disposables = disposablesSet;

      Reset();
    }

    public override void Dispose()
    {
      disposables.DisposeSafely();
    }
  }
}