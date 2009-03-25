// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.03

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Helpers;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using System.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Providers.Internals;

namespace Xtensive.Storage.Rse.Providers.InheritanceSupport
{
  /// <summary>
  /// General virtual <see cref="IndexAttributes.Join"/> index provider for all indexing storage handlers.
  /// </summary>
  [Serializable]
  public sealed class JoinInheritorsProvider : ExecutableProvider<IndexProvider>,
    IOrderedEnumerable<Tuple,Tuple>,
    ICountable
  {
    private readonly int includedColumnsCount;
    private readonly Provider root;
    private readonly IOrderedEnumerable<Tuple, Tuple> rootEnumerable;
    private readonly Provider[] inheritors;
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
      get { return rootEnumerable.AsymmetricKeyCompare; }
    }

    AdvancedComparer<Entire<Tuple>> IHasKeyComparers<Tuple>.EntireKeyComparer
    {
      get { return rootEnumerable.EntireKeyComparer; }
    }

    AdvancedComparer<Tuple> IHasKeyComparers<Tuple>.KeyComparer
    {
      get { return rootEnumerable.KeyComparer; }
    }

    /// <inheritdoc/>
    public Converter<Tuple, Tuple> KeyExtractor
    {
      get { return rootEnumerable.KeyExtractor; }
    }

    IEnumerable<Tuple> IOrderedEnumerable<Tuple, Tuple>.GetKeys(Range<Entire<Tuple>> range)
    {
      return rootEnumerable.GetKeys(range);
    }

    IIndexReader<Tuple, Tuple> IOrderedEnumerable<Tuple, Tuple>.CreateReader(Range<Entire<Tuple>> range)
    {
      return new JoinInheritorsReader(this, range,  root, inheritors, mapTransform);
    }

    SeekResult<Tuple> IOrderedEnumerable<Tuple, Tuple>.Seek(Ray<Entire<Tuple>> ray)
    {
      SeekResult<Tuple> seek = rootEnumerable.Seek(ray);
      if (seek.ResultType != SeekResultType.None) {
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
      SeekResult<Tuple> seek = rootEnumerable.Seek(key);
      if (seek.ResultType != SeekResultType.None) {
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
      return InheritanceJoiner.Join(
        rootEnumerable.GetItems(range),
        rootEnumerable.KeyExtractor,
        rootEnumerable.KeyComparer, 
        mapTransform,
        inheritors
          .Select(provider => new Pair<Provider,IOrderedEnumerable<Tuple,Tuple>>(provider, provider.GetService<IOrderedEnumerable<Tuple,Tuple>>(true)))
          .Select(pair => new Triplet<IEnumerable<Tuple>, Converter<Tuple, Tuple>, TupleDescriptor>(
            pair.Second.GetItems(range), pair.Second.KeyExtractor, pair.First.Header.TupleDescriptor)).ToList()
          );
    }

    IEnumerable<Tuple> IOrderedEnumerable<Tuple, Tuple>.GetItems(RangeSet<Entire<Tuple>> range)
    {
      return InheritanceJoiner.Join(
        rootEnumerable.GetItems(range),
        rootEnumerable.KeyExtractor,
        rootEnumerable.KeyComparer,
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
      return InheritanceJoiner.Join(
        root,
        rootEnumerable.KeyExtractor,
        rootEnumerable.KeyComparer,
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
      for (int i = 0; i < root.Header.Length; i++)
        map.Add(new Pair<int, int>(0, i));
      for (int i = 0; i < inheritors.Length; i++)
        for (int j = inheritors[i].Header.Order.Count + includedColumnsCount; j < inheritors[i].Header.Length; j++)
          map.Add(new Pair<int, int>(i + 1, j));
      mapTransform = new MapTransform(true, Header.TupleDescriptor, map.ToArray());
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="includedColumnsCount">Amount of included columns.</param>
    /// <param name="root">Root index provider.</param>
    /// <param name="inheritors">Inheritor index providers.</param>
    public JoinInheritorsProvider(IndexProvider origin, int includedColumnsCount, ExecutableProvider root, ExecutableProvider[] inheritors)
      : base(origin, new[]{root}.Union(inheritors).ToArray())
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();

      this.includedColumnsCount = includedColumnsCount;
      this.root = root;
      rootEnumerable = root.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      this.inheritors = inheritors;      
    }
  }
}