// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.08

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Helpers;
using Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex
{
  /// <summary>
  /// General virtual <see cref="IndexAttributes.View"/> index provider for all indexing storage handlers.
  /// </summary>
  public sealed class ViewIndexProvider : UnaryExecutableProvider<Compilable.IndexProvider>,
    IOrderedEnumerable<Tuple, Tuple>,
    ICountable
  {
    private readonly int[] columnMap;
    private MapTransform transform;

    #region Root delegating members

    /// <inheritdoc/>
    public Func<Entire<Tuple>, Tuple, int> AsymmetricKeyCompare
    {
      get { return Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).AsymmetricKeyCompare; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<Entire<Tuple>> EntireKeyComparer
    {
      get { return Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).EntireKeyComparer; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<Tuple> KeyComparer
    {
      get { return Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer; }
    }

    /// <inheritdoc/>
    public Converter<Tuple, Tuple> KeyExtractor
    {
      get { return Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor; }
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetKeys(Range<Entire<Tuple>> range)
    {
      return Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetKeys(range);
    }

    /// <inheritdoc/>
    public long Count
    {
      get
      {
        var countable = Source.GetService<ICountable>(true);
        return countable.Count;
      }
    }
    
    #endregion
    
    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(Range<Entire<Tuple>> range)
    {
      var items = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range);
      return items.Select(t => transform.Apply(TupleTransformType.Auto, t));
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(RangeSet<Entire<Tuple>> range)
    {
      var items = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range);
      return items.Select(t => transform.Apply(TupleTransformType.Auto, t));
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Ray<Entire<Tuple>> ray)
    {
      var seek = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).Seek(ray);
      var result = seek.ResultType == SeekResultType.None
        ? default(Tuple)
        : transform.Apply(TupleTransformType.Auto, seek.Result);
      return new SeekResult<Tuple>(seek.ResultType, result);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Tuple key)
    {
      var seek = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).Seek(key);
      var result = seek.ResultType == SeekResultType.None
        ? default(Tuple)
        : transform.Apply(TupleTransformType.Auto, seek.Result);
      return new SeekResult<Tuple>(seek.ResultType, result);
    }

    /// <inheritdoc/>
    public IIndexReader<Tuple, Tuple> CreateReader(Range<Entire<Tuple>> range)
    {
      return new ViewIndexReader(this, range, Source, transform);
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context).Select(t => transform.Apply(TupleTransformType.Auto, t));
    }

    protected override void Initialize()
    {
      base.Initialize();
      transform = new MapTransform(true, Header.TupleDescriptor, columnMap);
    }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="provider">The <see cref="UnaryExecutableProvider{TOrigin}.Source"/> property value.</param>
    /// <param name="columnMap">The column map.</param>
    public ViewIndexProvider(IndexProvider origin, ExecutableProvider provider, int[] columnMap)
      : base(origin, provider)
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();

      this.columnMap = columnMap;
    }
  }
}