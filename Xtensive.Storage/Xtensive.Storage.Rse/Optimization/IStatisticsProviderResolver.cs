// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.15

using Xtensive.Core.Tuples;
using Xtensive.Indexing.Statistics;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization
{
  /// <summary>
  /// Resolver of a <see cref="IStatisticsProvider{TKey}"/> for a specified <see cref="IndexInfo"/>.
  /// </summary>
  public interface IStatisticsProviderResolver
  {
    /// <summary>
    /// Resolves the <see cref="IStatisticsProvider{TKey}"/> for <paramref name="indexInfo"/>.
    /// </summary>
    /// <param name="indexInfo">The description of the index.</param>
    IStatisticsProvider<Tuple> Resolve(IndexInfo indexInfo);
  }
}