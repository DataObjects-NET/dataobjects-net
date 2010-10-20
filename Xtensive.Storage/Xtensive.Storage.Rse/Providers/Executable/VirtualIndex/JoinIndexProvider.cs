// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.03

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex
{
  /// <summary>
  /// General virtual <see cref="IndexAttributes.Join"/> index provider for all indexing storage handlers.
  /// </summary>
  [Serializable]
  public sealed class JoinIndexProvider : ExecutableProvider<IndexProvider>,
    IOrderedEnumerable<Tuple,Tuple>,
    ICountable
  {
    private readonly int keyColumnsCount;
    private readonly List<Pair<int, List<int>>> valueColumnsMap;
    private readonly ExecutableProvider root;
    private readonly ExecutableProvider[] inheritors;
    private MapTransform mapTransform;

    #region Interface implementation

    long ICountable.Count
    {
      get
      {
        var countable = root.GetService<ICountable>();
        return countable.Count;
      }
    }

    Func<Entire<Tuple>, Tuple, int> IHasKeyComparers<Tuple>.AsymmetricKeyCompare
    {
      get { return root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).AsymmetricKeyCompare; }
    }

    AdvancedComparer<Entire<Tuple>> IHasKeyComparers<Tuple>.EntireKeyComparer
    {
      get { return root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).EntireKeyComparer; }
    }

    AdvancedComparer<Tuple> IHasKeyComparers<Tuple>.KeyComparer
    {
      get { return root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer; }
    }

    /// <inheritdoc/>
    public Converter<Tuple, Tuple> KeyExtractor
    {
      get { return root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor; }
    }

    IEnumerable<Tuple> IOrderedEnumerable<Tuple, Tuple>.GetKeys(Range<Entire<Tuple>> range)
    {
      return root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetKeys(range);
    }

    IIndexReader<Tuple, Tuple> IOrderedEnumerable<Tuple, Tuple>.CreateReader(Range<Entire<Tuple>> range)
    {
      return new JoinIndexReader(this, range,  root, inheritors, mapTransform);
    }

    SeekResult<Tuple> IOrderedEnumerable<Tuple, Tuple>.Seek(Ray<Entire<Tuple>> ray)
    {
      SeekResult<Tuple> seek = root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).Seek(ray);
      if (seek.ResultType == SeekResultType.Exact) {
        var resultTuples = new Tuple[1+inheritors.Length];
        resultTuples[0] = seek.Result;
        for (int i = 0; i < inheritors.Length; i++) {
          var rightRay = new Ray<Entire<Tuple>>(new Entire<Tuple>(KeyExtractor(seek.Result), ray.Point.ValueType));
          SeekResult<Tuple> seekRight = inheritors[i].GetService<IOrderedEnumerable<Tuple,Tuple>>().Seek(rightRay);
          if (seekRight.ResultType == SeekResultType.Exact)
            resultTuples[1+i] = seekRight.Result;
        }
        return new SeekResult<Tuple>(
          seek.ResultType,
          mapTransform.Apply(TupleTransformType.Auto, resultTuples));

      }
      return seek;
    }

    SeekResult<Tuple> IOrderedEnumerable<Tuple, Tuple>.Seek(Tuple key)
    {
      SeekResult<Tuple> seek = root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).Seek(key);
      if (seek.ResultType == SeekResultType.Exact) {
        var resultTuples = new Tuple[1+inheritors.Length];
        resultTuples[0] = seek.Result;
        for (int i = 0; i < inheritors.Length; i++) {
          SeekResult<Tuple> seekRight = inheritors[i].GetService<IOrderedEnumerable<Tuple,Tuple>>().Seek(key);
          if (seekRight.ResultType == SeekResultType.Exact)
            resultTuples[1+i] = seekRight.Result;
        }
        return new SeekResult<Tuple>(
          seek.ResultType,
          mapTransform.Apply(TupleTransformType.Auto, resultTuples));

      }
      return seek;
    }

    IEnumerable<Tuple> IOrderedEnumerable<Tuple, Tuple>.GetItems(Range<Entire<Tuple>> range)
    {
      return Internal.JoinAlgorithm.Join(
        root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range),
        root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor,
        root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer, 
        mapTransform,
        inheritors
          .Select(provider => new Pair<Provider,IOrderedEnumerable<Tuple,Tuple>>(provider, provider.GetService<IOrderedEnumerable<Tuple,Tuple>>(true)))
          .Select(pair => new Triplet<IEnumerable<Tuple>, Converter<Tuple, Tuple>, TupleDescriptor>(
            pair.Second.GetItems(range), pair.Second.KeyExtractor, pair.First.Header.TupleDescriptor)).ToList()
          );
    }

    IEnumerable<Tuple> IOrderedEnumerable<Tuple, Tuple>.GetItems(RangeSet<Entire<Tuple>> range)
    {
      return Internal.JoinAlgorithm.Join(
        root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range),
        root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor,
        root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer,
        mapTransform,
        inheritors
          .Select(provider => new Pair<Provider, IOrderedEnumerable<Tuple, Tuple>>(provider, provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true)))
          .Select(pair => new Triplet<IEnumerable<Tuple>, Converter<Tuple, Tuple>, TupleDescriptor>(
            pair.Second.GetItems(range), pair.Second.KeyExtractor, pair.First.Header.TupleDescriptor)).ToList()
          );
    }

    #endregion

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Internal.JoinAlgorithm.Join(
        root,
        root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor,
        root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer,
        mapTransform,
        inheritors
          .Select(provider => new Pair<Provider, IOrderedEnumerable<Tuple, Tuple>>(provider, provider.GetService<IOrderedEnumerable<Tuple, Tuple>>(true)))
          .Select(pair => new Triplet<IEnumerable<Tuple>, Converter<Tuple, Tuple>, TupleDescriptor>(
            pair.Second, pair.Second.KeyExtractor, pair.First.Header.TupleDescriptor)).ToList());
    }

    protected override void Initialize()
    {
      base.Initialize();
      var map = new List<Pair<int, int>>();

      foreach (var pair in valueColumnsMap) {
        if (map.Count == 0)
          map.AddRange(Enumerable.Range(0, keyColumnsCount).Select(i => new Pair<int, int>(pair.First, i)));
        foreach (var columnIndex in pair.Second)
          map.Add(new Pair<int, int>(pair.First, columnIndex + keyColumnsCount));
      }
      if (Header.Length != map.Count)
        throw new InvalidOperationException(Strings.ExUnableToInitializeJoinIndexProviderColumnsCountMismatch);
      mapTransform = new MapTransform(true, Header.TupleDescriptor, map.ToArray());
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="keyColumnsCount">Count of key columns.</param>
    /// <param name="root">Root index provider.</param>
    /// <param name="inheritors">Inheritor index providers.</param>
    public JoinIndexProvider(IndexProvider origin, int keyColumnsCount, List<Pair<int, List<int>>> valueColumnsMap, ExecutableProvider root, ExecutableProvider[] inheritors)
      : base(origin, new[] {root}.Union(inheritors).ToArray())
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();

      this.valueColumnsMap = valueColumnsMap;
      this.keyColumnsCount = keyColumnsCount;
      this.root = root;
      this.inheritors = inheritors;
    }
  }
}