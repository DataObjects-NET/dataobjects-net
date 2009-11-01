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
using Xtensive.Storage.Rse;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Internals;

namespace Xtensive.Storage.Rse.Providers.InheritanceSupport
{
  public sealed class FilterInheritorsProvider : ProviderImplementation,
    IOrderedEnumerable<Tuple,Tuple>,
    ICountable
  {
    private readonly IOrderedEnumerable<Tuple,Tuple> source;
    private readonly Provider sourceProvider;
    private readonly ProviderOptions options;
    private readonly Func<Tuple, bool> predicate;
    private readonly bool[] typeIDMatch;

    /// <inheritdoc/>
    public override ProviderOptionsStruct Options
    {
      get { return options; }
    }

    public long Count
    {
      get
      {
        var countable = sourceProvider.GetService<ICountable>(true);
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
      return new FilteringReader(this, range, sourceProvider, predicate);
    }

    /// <inheritdoc/>
    public override IEnumerator<Tuple> GetEnumerator()
    {
      return source.Where(predicate).GetEnumerator();
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FilterInheritorsProvider(Provider sourceProvider, int typeIDColumn, int typesCount,  params int[] typeIDs)
      : base(sourceProvider.Header, sourceProvider)
    {
      this.sourceProvider = sourceProvider;
      options = sourceProvider.Options.Internal & ~ProviderOptions.FastCount;
      source = sourceProvider.GetService<IOrderedEnumerable<Tuple,Tuple>>(true);
      typeIDMatch = new bool[typesCount + 1];
      foreach (int typeID in typeIDs)
        typeIDMatch[typeID] = true;
      predicate = delegate(Tuple item)
      {
        int typeID = item.GetValueOrDefault<int>(typeIDColumn);
        return typeIDMatch[typeID];
      };
    }
  }
}