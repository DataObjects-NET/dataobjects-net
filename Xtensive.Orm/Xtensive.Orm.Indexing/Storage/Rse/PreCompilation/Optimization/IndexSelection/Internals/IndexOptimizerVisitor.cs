// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Caching;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Linq.Normalization;
using Xtensive.Orm.Model;
using Xtensive.Parameters;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;

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
      var index = primaryProvider.Index.Resolve(domainModel);
      var indexes = index.IsPrimary ? GetIndexes(index) : new List<IndexInfo> {index};
      if (indexes.Count == 0)
        return base.VisitFilter(provider);
      var extractionResult = ExtractRangeSets(provider.Predicate, index, indexes);
      Dictionary<IndexInfo, RangeSetInfo> selectedIndexes;
      using (ParameterContext.ExpectedValues.Activate()) {
        selectedIndexes = indexSelector.Select(extractionResult);
      }
      return treeRewriter.InsertRangeProviders(provider, selectedIndexes);
    }

    #region Private \ internal methods
    private Dictionary<Expression, List<RsExtractionResult>> ExtractRangeSets(Expression predicate,
      IndexInfo index, IEnumerable<IndexInfo> indexes)
    {
      DisjunctiveNormalized normalized = null;
      try {
        normalized = normalizer.Normalize(predicate);
      }
      catch (InvalidOperationException){}
      if (normalized != null)
        return rsExtractor.Extract(normalized, indexes, index.GetRecordSetHeader());
      return rsExtractor.Extract(predicate, indexes, index.GetRecordSetHeader());
    }

    private IList<IndexInfo> GetIndexes(IndexInfo index)
    {
      Pair<IndexInfo, IList<IndexInfo>> cachedPair;
      if (indexesCache.TryGetItem(index, true, out cachedPair))
        return cachedPair.Second;
      var result = index.ReflectedType.Indexes.GetIndexesContainingAllData();
      indexesCache.Add(new Pair<IndexInfo, IList<IndexInfo>>(index, result));
      return result;
    }
    #endregion


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