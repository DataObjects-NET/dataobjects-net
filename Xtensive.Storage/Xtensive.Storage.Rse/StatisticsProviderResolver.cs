// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Indexing.Statistics;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Resolver of a <see cref="IStatisticsProvider{TKey}"/> for a specified <see cref="IndexInfo"/>.
  /// </summary>
  public class StatisticsProviderResolver
  {
    private readonly Func<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>> indexResolver;

    /// <summary>
    /// Resolves the <see cref="IStatisticsProvider{TKey}"/> for <paramref name="indexInfo"/>.
    /// </summary>
    /// <param name="indexInfo">The description of the index.</param>
    public IStatisticsProvider<Tuple> Resolve(IndexInfo indexInfo)
    {
      if (!indexInfo.IsVirtual)
        return indexResolver(indexInfo);
      if ((indexInfo.Attributes & IndexAttributes.Union) != 0) {
        var underlyingProviders = indexInfo.UnderlyingIndexes.Select(index => Resolve(index)).ToArray();
        return new MergedStatisticsProvider(underlyingProviders);
      }
      return Resolve(indexInfo.UnderlyingIndexes.First());
    }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="indexResolver">The index resolver.</param>
    public StatisticsProviderResolver(Func<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>> indexResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(indexResolver, "indexResolver");
      this.indexResolver = indexResolver;
    }
  }
}