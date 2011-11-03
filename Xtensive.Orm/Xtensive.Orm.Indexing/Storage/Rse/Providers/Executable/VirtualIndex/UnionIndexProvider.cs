// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.04

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Model;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal;
using Xtensive.Helpers;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex
{
  /// <summary>
  /// General virtual <see cref="IndexAttributes.Union"/> index provider for all indexing storage handlers.
  /// </summary>
  [Serializable]
  public sealed class UnionIndexProvider : ExecutableProvider,
    IOrderedEnumerable<Tuple,Tuple>,
    ICountable
  {
    private readonly ExecutableProvider[] sourceProviders;
    private Converter<Tuple, Tuple> keyExtractor;

    
    /// <inheritdoc/>
    public long Count
    {
      get { return sourceProviders.Sum(provider => provider.GetService<ICountable>(true).Count); }
    }

    /// <inheritdoc/>
    public Func<Entire<Tuple>, Tuple, int> AsymmetricKeyCompare
    {
      get
      {
        var orderedEnumerable = sourceProviders[0].GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return orderedEnumerable.AsymmetricKeyCompare;
      }
    }

    /// <inheritdoc/>
    public AdvancedComparer<Entire<Tuple>> EntireKeyComparer
    {
      get
      {
        var orderedEnumerable = sourceProviders[0].GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return orderedEnumerable.EntireKeyComparer;
      }
    }

    /// <inheritdoc/>
    public AdvancedComparer<Tuple> KeyComparer
    {
      get
      {
        var orderedEnumerable = sourceProviders[0].GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return orderedEnumerable.KeyComparer;
      }
    }

    /// <inheritdoc/>
    public Converter<Tuple, Tuple> KeyExtractor
    {
      get { return keyExtractor; }
    }

    /// <inheritdoc/>
    public IIndexReader<Tuple, Tuple> CreateReader(Range<Entire<Tuple>> range)
    {
      return new UnionIndexReader(this, range, sourceProviders);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Ray<Entire<Tuple>> ray)
    {
      var comparer = KeyComparer;
      var seekResults = new List<SeekResult<Tuple>>();
      for (int i = 0; i < sourceProviders.Length; i++) {
        SeekResult<Tuple> seek = sourceProviders[i].GetService<IOrderedEnumerable<Tuple,Tuple>>().Seek(ray);
        if (seek.ResultType == SeekResultType.Exact)
          return seek;
        if (seek.ResultType==SeekResultType.Nearest)
          seekResults.Add(seek);
      }
      if (seekResults.Count == 0)
        return new SeekResult<Tuple>(SeekResultType.None, null);
      Tuple lowestKey = null;
      int lowestIndex = 0;
      for (int i = 0; i < seekResults.Count; i++) {
        Tuple key = keyExtractor(seekResults[i].Result);
        if (lowestKey == null) {
          lowestKey = key;
          lowestIndex = i;
        }
        int result = comparer.Compare(key, lowestKey);
        if (result < 0) {
          lowestKey = key;
          lowestIndex = i;
        }
      }
      return seekResults[lowestIndex];
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Tuple key)
    {
      var comparer = KeyComparer;
      var seekResults = new List<SeekResult<Tuple>>();
      for (int i = 0; i < sourceProviders.Length; i++) {
        SeekResult<Tuple> seek = sourceProviders[i].GetService<IOrderedEnumerable<Tuple,Tuple>>().Seek(key);
        if (seek.ResultType == SeekResultType.Exact)
          return seek;
        if (seek.ResultType==SeekResultType.Nearest)
          seekResults.Add(seek);
      }
      if (seekResults.Count == 0)
        return new SeekResult<Tuple>(SeekResultType.None, null);
      Tuple lowestKey = null;
      int lowestIndex = 0;
      for (int i = 0; i < seekResults.Count; i++) {
        Tuple keyNext = keyExtractor(seekResults[i].Result);
        if (lowestKey == null) {
          lowestKey = keyNext;
          lowestIndex = i;
        }
        int result = comparer.Compare(keyNext, lowestKey);
        if (result < 0) {
          lowestKey = keyNext;
          lowestIndex = i;
        }
      }
      return seekResults[lowestIndex];
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetKeys(Range<Entire<Tuple>> range)
    {
      foreach (Tuple item in GetItems(range))
        yield return keyExtractor(item);
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(Range<Entire<Tuple>> range)
    {
      return MergeAlgorithm.Merge(
        sourceProviders[0].GetService<IOrderedEnumerable<Tuple,Tuple>>(true).KeyComparer,
        sourceProviders.Select((provider, i) => new Pair<IEnumerable<Tuple>, Converter<Tuple, Tuple>>(
          provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range), provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor)).ToList()
        );
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(RangeSet<Entire<Tuple>> range)
    {
      return MergeAlgorithm.Merge(
        sourceProviders[0].GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer,
        sourceProviders.Select((provider, i) => new Pair<IEnumerable<Tuple>, Converter<Tuple, Tuple>>(
          provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range), provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor)).ToList()
        );
    }

    /// <inheritdoc/>
    public override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return MergeAlgorithm.Merge(
        sourceProviders[0].GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer,
        sourceProviders.Select((provider, i) => new Pair<IEnumerable<Tuple>, Converter<Tuple, Tuple>>(
          provider, provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor)).ToList()
        );
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      var keyTransform = new MapTransform(true, Header.TupleDescriptor, Header.Order.Select(pair => pair.Key).ToArray());
      keyExtractor = (input => keyTransform.Apply(TupleTransformType.TransformedTuple, input));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="sourceProviders">Providers of inheritor indexes.</param>
    public UnionIndexProvider(CompilableProvider origin, params ExecutableProvider[] sourceProviders)
      : base(origin, sourceProviders)
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();

      this.sourceProviders = sourceProviders;
    }
  }
}