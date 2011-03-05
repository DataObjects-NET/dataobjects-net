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
using System.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Providers.Executable;
using Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal;
using Xtensive.Helpers;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex
{
  /// <summary>
  /// General virtual <see cref="IndexAttributes.Filtered"/> index provider for all indexing storage handlers.
  /// </summary>
  public sealed class FilterIndexProvider : UnaryExecutableProvider<Compilable.IndexProvider>,
    IOrderedEnumerable<Tuple,Tuple>,
    ICountable
  {
    private readonly Func<Tuple, bool> predicate;

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

    #endregion

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(Range<Entire<Tuple>> range)
    {
      var items = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range);
      return items.Where(predicate);
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(RangeSet<Entire<Tuple>> range)
    {
      IEnumerable<Tuple> items = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range);
      return items.Where(predicate);
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetKeys(Range<Entire<Tuple>> range)
    {
      IEnumerable<Tuple> items = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range);
      return items.Where(predicate).Select(tuple => KeyExtractor(tuple));
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Ray<Entire<Tuple>> ray)
    {
      SeekResult<Tuple> seek = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).Seek(ray);
      if (seek.ResultType==SeekResultType.Exact)
        if (!predicate(seek.Result)) {
          var nonExactResult = seek.Result;
          return new SeekResult<Tuple>(SeekResultType.Nearest, nonExactResult);
        }
      var result = seek.ResultType == SeekResultType.None
        ? default(Tuple)
        : seek.Result;
      return new SeekResult<Tuple>(seek.ResultType, result);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Tuple key)
    {
      SeekResult<Tuple> seek = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).Seek(key);
      if (seek.ResultType == SeekResultType.Exact)
        if (!predicate(seek.Result)) {
          var nonExactResult = seek.Result;
          return new SeekResult<Tuple>(SeekResultType.Nearest, nonExactResult);
        }
      var result = seek.ResultType == SeekResultType.None
        ? default(Tuple)
        : seek.Result;
      return new SeekResult<Tuple>(seek.ResultType, result);
    }

    /// <inheritdoc/>
    public IIndexReader<Tuple, Tuple> CreateReader(Range<Entire<Tuple>> range)
    {
      return new FilterIndexReader(this, range, Source, predicate);
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).Where(predicate);
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="provider">Source executable provider.</param>
    /// <param name="typeIdColumn">Index of typeId column.</param>
    /// <param name="typeIds">Identifiers of descendants types.</param>
    public FilterIndexProvider(Compilable.IndexProvider origin, ExecutableProvider provider, int typeIdColumn, IEnumerable<int> typeIds)
      : base(origin, provider)
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();
      predicate = tuple => typeIds.Contains(tuple.GetValueOrDefault<int>(typeIdColumn));
    }
  }
}