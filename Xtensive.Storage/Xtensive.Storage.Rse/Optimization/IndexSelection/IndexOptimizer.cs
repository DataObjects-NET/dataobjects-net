// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Linq.Normalization;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  /// <summary>
  /// Optimizer which uses ranges of index keys.
  /// </summary>
  [Serializable]
  public sealed class IndexOptimizer : CompilableProviderVisitor, IOptimizer
  {
    private readonly DomainModel domainModel;
    private readonly LruCache<IndexInfo,Pair<IndexInfo, List<IndexInfo>>> seconaryIndexesCache =
      new LruCache<IndexInfo,Pair<IndexInfo, List<IndexInfo>>>(20, pair => pair.First, pair => 1);
    private readonly RangeSetExtractor rsExtractor;
    private readonly IIndexSelector indexSelector;
    private readonly ProviderTreeRewriter treeRewriter;
    private readonly DisjunctiveNormalizer normalizer = new DisjunctiveNormalizer(100);

    /// <inheritdoc/>
    public CompilableProvider Optimize(CompilableProvider rootProvider)
    {
      return (CompilableProvider)Visit(rootProvider);
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      var primaryProvider = provider.Source as IndexProvider;
      if (primaryProvider == null)
        return base.VisitFilter(provider);
      var primaryIndex = primaryProvider.Index.Resolve(domainModel);
      var secondaryIndexes = GetSecondaryIndexes(primaryIndex);
      if (secondaryIndexes.Count == 0)
        return base.VisitFilter(provider);
      var extractionResult = ExtractRangeSets(provider.Predicate, primaryIndex, secondaryIndexes);
      var selectedIndexes = indexSelector.Select(extractionResult);
      return treeRewriter.InsertSecondaryIndexes(provider, selectedIndexes);
    }

    private Dictionary<Expression, List<RSExtractionResult>> ExtractRangeSets(Expression predicate,
      IndexInfo primaryIndex, IEnumerable<IndexInfo> secondaryIndexes)
    {
      DisjunctiveNormalized normalized = null;
      try {
        normalized = normalizer.Normalize(predicate);
      }
      catch (InvalidOperationException){}
      if (normalized != null)
        return rsExtractor.Extract(normalized, secondaryIndexes, primaryIndex.GetRecordSetHeader());
      return rsExtractor.Extract(predicate, secondaryIndexes, primaryIndex.GetRecordSetHeader());
    }

    private List<IndexInfo> GetSecondaryIndexes(IndexInfo primaryIndex)
    {
      Pair<IndexInfo, List<IndexInfo>> cachedPair;
      if (seconaryIndexesCache.TryGetItem(primaryIndex, true, out cachedPair))
        return cachedPair.Second;
      var result = primaryIndex.ReflectedType.Indexes.Where(index => !index.IsPrimary).ToList();
      seconaryIndexesCache.Add(new Pair<IndexInfo, List<IndexInfo>>(primaryIndex, result));
      return result;
    }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="domainModel">The domain model.</param>
    /// <param name="providerResolver">The statistics provider resolver.</param>
    public IndexOptimizer(DomainModel domainModel, IStatisticsProviderResolver providerResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      ArgumentValidator.EnsureArgumentNotNull(providerResolver, "providerResolver");
      this.domainModel = domainModel;
      rsExtractor = new RangeSetExtractor(domainModel);
      indexSelector = new SimpleIndexSelector(new CostEvaluator(providerResolver));
      treeRewriter = new ProviderTreeRewriter(domainModel);
    }
  }
}