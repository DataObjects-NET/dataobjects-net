// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Indexing.Measures;
using Xtensive.Storage.Rse;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Internals;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Providers.InheritanceSupport
{
  public sealed class MergeInheritorsProvider : ExecutableProvider,
    IOrderedEnumerable<Tuple,Tuple>,
    ICountable
  {
    private readonly Provider[] sourceProviders;
    private readonly MapTransform keyTransform;
    private readonly Converter<Tuple, Tuple> keyExtractor;
    private readonly MapTransform[] transforms;

    
    /// <inheritdoc/>
    public long Count
    {
      get { return sourceProviders.Sum(provider => provider.GetService<ICountable>(true).Count); }
    }

    /// <inheritdoc/>
    public Func<IEntire<Tuple>, Tuple, int> AsymmetricKeyCompare
    {
      get
      {
        var orderedEnumerable = sourceProviders[0].GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
        return orderedEnumerable.AsymmetricKeyCompare;
      }
    }

    /// <inheritdoc/>
    public AdvancedComparer<IEntire<Tuple>> EntireKeyComparer
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
    public IIndexReader<Tuple, Tuple> CreateReader(Range<IEntire<Tuple>> range)
    {
      return new MergeInheritorsReader(this, range, sourceProviders, transforms);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Ray<IEntire<Tuple>> ray)
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
    public IEnumerable<Tuple> GetKeys(Range<IEntire<Tuple>> range)
    {
      foreach (Tuple item in GetItems(range))
        yield return keyExtractor(item);
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(Range<IEntire<Tuple>> range)
    {
      return InheritanceMerger.Merge(
        sourceProviders[0].GetService<IOrderedEnumerable<Tuple,Tuple>>(true).KeyComparer,
        sourceProviders.Select((provider, i) => new Triplet<IEnumerable<Tuple>, Converter<Tuple, Tuple>, MapTransform>(
          provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range), provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor, transforms[i])).ToList()
        );
    }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return InheritanceMerger.Merge(
        sourceProviders[0].GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer,
        sourceProviders.Select((provider, i) => new Triplet<IEnumerable<Tuple>, Converter<Tuple, Tuple>, MapTransform>(
          provider, provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor, transforms[i])).ToList()
        );
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public MergeInheritorsProvider(CompilableProvider origin, params ExecutableProvider[] sourceProviders)
      : base(origin, sourceProviders)
    {
      this.sourceProviders = sourceProviders;
      keyTransform = new MapTransform(true, Header.TupleDescriptor, Header.OrderInfo.OrderedBy.Select(pair => pair.Key).ToArray());
      keyExtractor = (input => keyTransform.Apply(TupleTransformType.TransformedTuple, input));

      RecordColumnCollection targetColumns = Header.RecordColumnCollection;
      transforms = new MapTransform[sourceProviders.Length];
      for (int sourceIndex = 0; sourceIndex < transforms.Length; sourceIndex++) {
        Provider source = sourceProviders[sourceIndex];
        RecordColumnCollection sourceColumns = source.Header.RecordColumnCollection;
        var map = new int[Header.TupleDescriptor.Count];
        for (int i = 0; i < map.Length; i++) {
          map[i] = -1;
          ColumnInfoRef columnRef = targetColumns[i].ColumnInfoRef;
          for (int j = 0; j < sourceColumns.Count; j++) {
            if (sourceColumns[j].ColumnInfoRef == columnRef) {
              map[i] = j;
              break;
            }
          }
        }
        transforms[sourceIndex] = new MapTransform(true, Header.TupleDescriptor, map);
      }
    }
  }
}