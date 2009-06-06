// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.23

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Providers.Internals
{
  internal sealed class MergeInheritorsReader : ProviderReader
  {
    private readonly ExecutableProvider[] inheritors;
    private readonly IIndexReader<Tuple, Tuple>[] readers;
    private readonly MapTransform[] transforms;
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
      return new MergeInheritorsReader(Provider, Range, inheritors, transforms);
    }

    #region Private \ internal methods

    private void ResetEnumerator()
    {
      if (enumerator != null)
        enumerator.Dispose();
      enumerator = InheritanceMerger.Merge(
        Provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer,
        inheritors.Select((provider, i) => new Triplet<IEnumerator<Tuple>, Converter<Tuple, Tuple>, MapTransform>(
          readers[i], provider.GetService<IOrderedEnumerable<Tuple,Tuple>>(true).KeyExtractor, transforms[i])).ToList()
        ).GetEnumerator();
    }

    #endregion


    // Constructors

    public MergeInheritorsReader(ExecutableProvider provider, Range<Entire<Tuple>> range, ExecutableProvider[] inheritors, MapTransform[] transforms)
      : base(provider, range)
    {
      this.inheritors = inheritors;
      this.transforms = transforms;
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