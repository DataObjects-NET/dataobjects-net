// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.04

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using System.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Providers.Executable;
using Xtensive.Storage.Rse.Providers.Internals;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Providers.InheritanceSupport
{
  /// <summary>
  /// General virtual <see cref="IndexAttributes.Filtered"/> index provider for all indexing storage handlers.
  /// </summary>
  [Serializable]
  public sealed class FilterInheritorsProvider : UnaryExecutableProvider<Compilable.IndexProvider>,
    IOrderedEnumerable<Tuple,Tuple>,
    ICountable
  {
    private readonly IOrderedEnumerable<Tuple,Tuple> source;
    private readonly Func<Tuple, bool> predicate;
    private MapTransform transform;

    /// <inheritdoc/>
    public long Count {
      get {
        var countable = Source.GetService<ICountable>(true);
        return countable.Count;
      }
    }

    #region Root delegating members

    /// <inheritdoc/>
    public Func<Entire<Tuple>, Tuple, int> AsymmetricKeyCompare
    {
      get { return source.AsymmetricKeyCompare; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<Entire<Tuple>> EntireKeyComparer
    {
      get { return source.EntireKeyComparer; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<Tuple> KeyComparer
    {
      get { return source.KeyComparer; }
    }

    /// <inheritdoc/>
    public Converter<Tuple, Tuple> KeyExtractor
    {
      get { return source.KeyExtractor; }
    }

    #endregion

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(Range<Entire<Tuple>> range)
    {
      IEnumerable<Tuple> items = source.GetItems(range);
      return items.Where(predicate).Select(t => transform.Apply(TupleTransformType.Auto, t));
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(RangeSet<Entire<Tuple>> range)
    {
      IEnumerable<Tuple> items = source.GetItems(range);
      return items.Where(predicate).Select(t => transform.Apply(TupleTransformType.Auto, t));
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetKeys(Range<Entire<Tuple>> range)
    {
      IEnumerable<Tuple> items = source.GetItems(range);
      return items.Where(predicate).Select(tuple => KeyExtractor(tuple));
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Ray<Entire<Tuple>> ray)
    {
      SeekResult<Tuple> seek = source.Seek(ray);
      if (seek.ResultType==SeekResultType.Exact)
        if (!predicate(seek.Result)) {
          var nonExactResult = transform.Apply(TupleTransformType.Auto, seek.Result);
          return new SeekResult<Tuple>(SeekResultType.Nearest, nonExactResult);
        }
      var result = seek.ResultType == SeekResultType.None
        ? default(Tuple)
        : transform.Apply(TupleTransformType.Auto, seek.Result);
      return new SeekResult<Tuple>(seek.ResultType, result);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Tuple key)
    {
      SeekResult<Tuple> seek = source.Seek(key);
      if (seek.ResultType == SeekResultType.Exact)
        if (!predicate(seek.Result)) {
          var nonExactResult = transform.Apply(TupleTransformType.Auto, seek.Result);
          return new SeekResult<Tuple>(SeekResultType.Nearest, nonExactResult);
        }
      var result = seek.ResultType == SeekResultType.None
        ? default(Tuple)
        : transform.Apply(TupleTransformType.Auto, seek.Result);
      return new SeekResult<Tuple>(seek.ResultType, result);
    }

    /// <inheritdoc/>
    public IIndexReader<Tuple, Tuple> CreateReader(Range<Entire<Tuple>> range)
    {
      return new FilteringReader(this, range, Source, predicate, transform);
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return source.Where(predicate).Select(t => transform.Apply(TupleTransformType.Auto, t));
    }

    protected override void Initialize()
    {
      base.Initialize();
      
      var sourceProvider = Source;
      var sourceColumns = sourceProvider.Header.Columns;
      var targetColumns = Header.Columns;
      var map = new int[Header.TupleDescriptor.Count];
      for (int i = 0; i < map.Length; i++) {
        map[i] = MapTransform.NoMapping;
        var columnRef = ((MappedColumn)targetColumns[i]).ColumnInfoRef;
        for (int j = 0; j < sourceColumns.Count; j++) {
          var sourceColumnRef = ((MappedColumn)sourceColumns[j]).ColumnInfoRef;
          if (sourceColumnRef == columnRef) {
            map[i] = j;
            break;
          }
        }
      }
      transform = new MapTransform(true, Header.TupleDescriptor, map);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="provider">Source executable provider.</param>
    /// <param name="typeIdColumn">Index of typeId column.</param>
    /// <param name="typeIds">Identifiers of descendants types.</param>
    public FilterInheritorsProvider(Compilable.IndexProvider origin, ExecutableProvider provider,  int typeIdColumn, IEnumerable<int> typeIds)
      : base(origin, provider)
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();

      source = provider.GetService<IOrderedEnumerable<Tuple,Tuple>>(true);

      predicate = tuple => typeIds.Contains(tuple.GetValueOrDefault<int>(typeIdColumn));
    }
  }
}