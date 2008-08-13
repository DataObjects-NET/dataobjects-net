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
using Xtensive.Storage.Rse.Providers.Internals;

namespace Xtensive.Storage.Rse.Providers.InheritanceSupport
{
  [Serializable]
  public sealed class JoinInheritorsProvider : ExecutableProvider,
    IOrderedEnumerable<Tuple,Tuple>,
    ICountable
  {
    private readonly int includedColumnsCount;
    private readonly Provider root;
    private readonly IOrderedEnumerable<Tuple, Tuple> rootEnumerable;
    private readonly Provider[] inheritors;
    private MapTransform mapTransform;

    #region Root delegating members

    public long Count
    {
      get
      {
        var countable = root.GetService<ICountable>();
        return countable.Count;
      }
    }

    /// <inheritdoc/>
    public Func<IEntire<Tuple>, Tuple, int> AsymmetricKeyCompare
    {
      get { return rootEnumerable.AsymmetricKeyCompare; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<IEntire<Tuple>> EntireKeyComparer
    {
      get { return rootEnumerable.EntireKeyComparer; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<Tuple> KeyComparer
    {
      get { return rootEnumerable.KeyComparer; }
    }

    /// <inheritdoc/>
    public Converter<Tuple, Tuple> KeyExtractor
    {
      get { return rootEnumerable.KeyExtractor; }
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetKeys(Range<IEntire<Tuple>> range)
    {
      return rootEnumerable.GetKeys(range);
    }

    #endregion

    /// <inheritdoc/>
    public IIndexReader<Tuple, Tuple> CreateReader(Range<IEntire<Tuple>> range)
    {
      return new JoinInheritorsReader(this, range,  root, inheritors, mapTransform);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Ray<IEntire<Tuple>> ray)
    {
      SeekResult<Tuple> seek = rootEnumerable.Seek(ray);
      if (seek.ResultType != SeekResultType.None) {
        EntireValueType[] entireValueTypes = new EntireValueType[ray.Point.Count];
        for (int i = 0; i < ray.Point.Count; i++)
          entireValueTypes[i] = ray.Point.GetValueType(i);
        Tuple[] resultTuples = new Tuple[1+inheritors.Length];
        resultTuples[0] = seek.Result;
        for (int i = 0; i < inheritors.Length; i++) {
          Ray<IEntire<Tuple>> rightRay = new Ray<IEntire<Tuple>>(Entire<Tuple>.Create(KeyExtractor(seek.Result), entireValueTypes));
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

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(Range<IEntire<Tuple>> range)
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

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      var map = new List<Pair<int, int>>();
      for (int i = 0; i < root.Header.Columns.Count; i++)
        map.Add(new Pair<int, int>(0, i));
      for (int i = 0; i < inheritors.Length; i++)
        for (int j = inheritors[i].Header.Order.Count + includedColumnsCount; j < inheritors[i].Header.Columns.Count; j++)
          map.Add(new Pair<int, int>(i + 1, j));
      mapTransform = new MapTransform(true, Header.TupleDescriptor, map.ToArray());
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public JoinInheritorsProvider(CompilableProvider origin, int includedColumnsCount, ExecutableProvider root, ExecutableProvider[] inheritors)
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