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
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Internals;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Providers.InheritanceSupport
{
  /// <summary>
  /// General virtual <see cref="IndexAttributes.Union"/> index provider for all indexing storage handlers.
  /// </summary>
  [Serializable]
  public sealed class MergeInheritorsProvider : ExecutableProvider,
    IOrderedEnumerable<Tuple,Tuple>,
    ICountable
  {
    private readonly Provider[] sourceProviders;
    private MapTransform keyTransform;
    private Converter<Tuple, Tuple> keyExtractor;
    private MapTransform[] transforms;

    
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

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return InheritanceMerger.Merge(
        sourceProviders[0].GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer,
        sourceProviders.Select((provider, i) => new Triplet<IEnumerable<Tuple>, Converter<Tuple, Tuple>, MapTransform>(
          provider, provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor, transforms[i])).ToList()
        );
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      keyTransform = new MapTransform(true, Header.TupleDescriptor, Header.Order.Select(pair => pair.Key).ToArray());
      keyExtractor = (input => keyTransform.Apply(TupleTransformType.TransformedTuple, input));

      ColumnCollection targetColumns = Header.Columns;
      transforms = new MapTransform[sourceProviders.Length];
      for (int sourceIndex = 0; sourceIndex < transforms.Length; sourceIndex++) {
        Provider source = sourceProviders[sourceIndex];
        ColumnCollection sourceColumns = source.Header.Columns;
        var map = new int[Header.TupleDescriptor.Count];
        for (int i = 0; i < map.Length; i++) {
          map[i] = -1;
          ColumnInfoRef columnRef = ((MappedColumn)targetColumns[i]).ColumnInfoRef;
          for (int j = 0; j < sourceColumns.Count; j++) {
            if (((MappedColumn)sourceColumns[j]).ColumnInfoRef == columnRef) {
              map[i] = j;
              break;
            }
          }
        }
        transforms[sourceIndex] = new MapTransform(true, Header.TupleDescriptor, map);
      }
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="sourceProviders">Providers of inheritor indexes.</param>
    public MergeInheritorsProvider(CompilableProvider origin, params ExecutableProvider[] sourceProviders)
      : base(origin, sourceProviders)
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();

      this.sourceProviders = sourceProviders;      
    }
  }
}