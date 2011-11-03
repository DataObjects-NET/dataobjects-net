// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.15

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing.Optimization;
using Xtensive.Orm.Model;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  /// <summary>
  /// Resolver of a <see cref="IOptimizationInfoProvider{TKey}"/> for a specified <see cref="IndexInfo"/>.
  /// </summary>
  public interface IOptimizationInfoProviderResolver
  {
    /// <summary> 
    /// Resolves the <see cref="IOptimizationInfoProvider{TKey}"/> for <paramref name="indexInfo"/>.
    /// </summary>
    /// <param name="indexInfo">The description of the index.</param>
    IOptimizationInfoProvider<Tuple> Resolve(IndexInfo indexInfo);
  }
}