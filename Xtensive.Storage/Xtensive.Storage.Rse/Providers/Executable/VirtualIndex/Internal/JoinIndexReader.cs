// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.03

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Internals.DocTemplates;
using Xtensive.Helpers;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal
{
  internal sealed class JoinIndexReader : VirtualIndexReader
  {
    private readonly ExecutableProvider root;
    private readonly ExecutableProvider[] inheritors;
    private readonly MapTransform transform;
    private readonly IIndexReader<Tuple, Tuple> reader;
    private readonly IIndexReader<Tuple, Tuple>[] rightReaders;
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
      reader.MoveTo(key);
      foreach (IIndexReader<Tuple, Tuple> rightReader in rightReaders)
        rightReader.MoveTo(key);
      ResetEnumerator();
    }

    public override void Reset()
    {
      reader.Reset();
      foreach (IIndexReader<Tuple, Tuple> rightReader in rightReaders)
        rightReader.Reset();
      ResetEnumerator();
    }

    public override IEnumerator<Tuple> GetEnumerator()
    {
      return new JoinIndexReader(Provider, Range, root, inheritors, transform);
    }

    #region Private \ internal methods

    private void ResetEnumerator()
    {
      if (enumerator!=null)
        enumerator.Dispose();
      enumerator = JoinAlgorithm.Join(
        reader,
        root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor,
        root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer,
        transform,
        inheritors.Select((provider, i) => new Triplet<IEnumerator<Tuple>, Converter<Tuple, Tuple>, TupleDescriptor>(
                                             rightReaders[i], provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor, provider.Header.TupleDescriptor)).ToList()
        ).GetEnumerator();
    }

    #endregion

    // Constructors

    public JoinIndexReader(ExecutableProvider provider, Range<Entire<Tuple>> range, ExecutableProvider root, ExecutableProvider[] inheritors, MapTransform transform)
      : base(provider, range)
    {
      this.root = root;
      this.inheritors = inheritors;
      this.transform = transform;

      // TODO: AY: .CreateReader(range) везде - возможно чет не то. Но в SetRange было так же - я пока оставил...

      reader = root.GetService<IOrderedEnumerable<Tuple,Tuple>>(true).CreateReader(range);
      rightReaders = new IIndexReader<Tuple, Tuple>[inheritors.Length];
      for (int i = 0; i < inheritors.Length; i++)
        rightReaders[i] = inheritors[i].GetService<IOrderedEnumerable<Tuple, Tuple>>(true).CreateReader(range);

      var disposablesSet = new DisposableSet(rightReaders);
      disposablesSet.Add(reader);
      disposables = disposablesSet;
      Reset();
    }

    public override void Dispose()
    {
      disposables.DisposeSafely();
      enumerator.DisposeSafely();
    }
  }
}