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
using Xtensive.Indexing;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Executable;
using Xtensive.Storage.Rse.Providers.Internals;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Providers.InheritanceSupport
{
  [Serializable]
  public sealed class FilterInheritorsProvider : UnaryExecutableProvider<Compilable.IndexProvider>,
    IOrderedEnumerable<Tuple,Tuple>,
    ICountable
  {
    private readonly IOrderedEnumerable<Tuple,Tuple> source;
    private readonly Func<Tuple, bool> predicate;
    private readonly bool[] typeIdMatch;

    public long Count {
      get {
        var countable = Source.GetService<ICountable>(true);
        return countable.Count;
      }
    }

    #region Root delegating members

    /// <inheritdoc/>
    public Func<IEntire<Tuple>, Tuple, int> AsymmetricKeyCompare
    {
      get { return source.AsymmetricKeyCompare; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<IEntire<Tuple>> EntireKeyComparer
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
    public IEnumerable<Tuple> GetItems(Range<IEntire<Tuple>> range)
    {
      IEnumerable<Tuple> items = source.GetItems(range);
      return items.Where(predicate);
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetKeys(Range<IEntire<Tuple>> range)
    {
      IEnumerable<Tuple> items = source.GetItems(range);
      return items.Where(predicate).Select(tuple => KeyExtractor(tuple));
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Ray<IEntire<Tuple>> ray)
    {
      SeekResult<Tuple> seek = source.Seek(ray);
      if (seek.ResultType==SeekResultType.Exact)
        if (!predicate(seek.Result))
          return new SeekResult<Tuple>(SeekResultType.Nearest, seek.Result);
      return seek;
    }

    /// <inheritdoc/>
    public IIndexReader<Tuple, Tuple> CreateReader(Range<IEntire<Tuple>> range)
    {
      return new FilteringReader(this, range, Source, predicate);
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return source.Where(predicate);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FilterInheritorsProvider(Compilable.IndexProvider origin, ExecutableProvider provider,  int typeIdColumn, int typesCount,  params int[] typeIds)
      : base(origin, provider)
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();

      source = provider.GetService<IOrderedEnumerable<Tuple,Tuple>>(true);
      typeIdMatch = new bool[typesCount + 1];
      foreach (int typeId in typeIds)
        typeIdMatch[typeId] = true;
      predicate = delegate(Tuple item)
      {
        int typeId = item.GetValueOrDefault<int>(typeIdColumn);
        return typeIdMatch[typeId];
      };
    }
  }
}