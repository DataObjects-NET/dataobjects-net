// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Linq.Normalization;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  internal sealed class IndexOptimizerVisitor : CompilableProviderVisitor
  {
    private readonly DomainModel domainModel;
    private readonly LruCache<IndexInfo,Pair<IndexInfo, IList<IndexInfo>>> indexesCache =
      new LruCache<IndexInfo,Pair<IndexInfo, IList<IndexInfo>>>(20, pair => pair.First, pair => 1);
    private readonly RangeSetExtractor rsExtractor;
    private readonly IIndexSelector indexSelector;
    private readonly ProviderTreeRewriter treeRewriter;
    private readonly DisjunctiveNormalizer normalizer = new DisjunctiveNormalizer(100);

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
      var indexes = GetIndexes(primaryIndex);
      if (indexes.Count == 0)
        return base.VisitFilter(provider);
      var extractionResult = ExtractRangeSets(provider.Predicate, primaryIndex, indexes);
      Dictionary<IndexInfo, RangeSetInfo> selectedIndexes;
      using (ParameterContext.CreateExpectedValueScope()) {
        selectedIndexes = indexSelector.Select(extractionResult);
      }
      return treeRewriter.InsertSecondaryIndexes(provider, selectedIndexes);
    }

    private Dictionary<Expression, List<RSExtractionResult>> ExtractRangeSets(Expression predicate,
      IndexInfo primaryIndex, IEnumerable<IndexInfo> indexes)
    {
      DisjunctiveNormalized normalized = null;
      try {
        normalized = normalizer.Normalize(predicate);
      }
      catch (InvalidOperationException){}
      if (normalized != null)
        return rsExtractor.Extract(normalized, indexes, primaryIndex.GetRecordSetHeader());
      return rsExtractor.Extract(predicate, indexes, primaryIndex.GetRecordSetHeader());
    }

    private IList<IndexInfo> GetIndexes(IndexInfo primaryIndex)
    {
      Pair<IndexInfo, IList<IndexInfo>> cachedPair;
      if (indexesCache.TryGetItem(primaryIndex, true, out cachedPair))
        return cachedPair.Second;
      var result = primaryIndex.ReflectedType.Indexes.GetIndexesContainingAllData();
      indexesCache.Add(new Pair<IndexInfo, IList<IndexInfo>>(primaryIndex, result));
      return result;
    }


    // Constructors

    public IndexOptimizerVisitor(DomainModel domainModel, IOptimizationInfoProviderResolver providerResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      ArgumentValidator.EnsureArgumentNotNull(providerResolver, "providerResolver");
      this.domainModel = domainModel;
      rsExtractor = new RangeSetExtractor(domainModel, providerResolver);
      indexSelector = new SimpleIndexSelector(new CostEvaluator(providerResolver));
      treeRewriter = new ProviderTreeRewriter(domainModel);}
  }
}