// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.07

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  internal sealed class ProviderTreeRewriter : CompilableProviderVisitor
  {
    private readonly DomainModel domainModel;
    private Dictionary<IndexInfo, RangeSetInfo> currentRangeSets;

    public Provider InsertRangeProviders(CompilableProvider source, Dictionary<IndexInfo, RangeSetInfo> rangeSets)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(rangeSets, "rangeSets");
      currentRangeSets = rangeSets;
      return Visit(source);
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      var existingIndexProvider = provider.Source as IndexProvider;
      if (existingIndexProvider != null && currentRangeSets.Count > 0) {
        var existingIndex = existingIndexProvider.Index.Resolve(domainModel);
        var result = InsertRangeSetForExistingIndex(existingIndexProvider, existingIndex);
        if (existingIndex.IsPrimary) {
          var secondaryProvider = UniteSecondaryProviders(existingIndex);
          if (secondaryProvider!=null)
            result = JoinWithPrimaryIndex(secondaryProvider, result, existingIndex);
        }
        return new FilterProvider(result, provider.Predicate);
      }
      return base.VisitFilter(provider);
    }

    #region Private \ internal methods

    private static SelectProvider JoinWithPrimaryIndex(CompilableProvider secondaryIndexProvider,
      CompilableProvider primaryIndexProvider, IndexInfo primaryIndex)
    {
      var alias = new AliasProvider(secondaryIndexProvider, "secondary");
      var join = new JoinProvider(primaryIndexProvider, alias, JoinType.Inner, JoinAlgorithm.Hash,
        GetEqualIndexes(primaryIndex.KeyColumns.Count));
      return new SelectProvider(join, Enumerable.Range(0, primaryIndex.Columns.Count).ToArray());
    }

    private CompilableProvider UniteSecondaryProviders(IndexInfo primaryIndex)
    {
      CompilableProvider result = null;
      foreach (var pair in currentRangeSets.Where(p => p.Key.IsSecondary && p.Key != primaryIndex)) {
        var rangeSetProvider = new RangeSetProvider(IndexProvider.Get(pair.Key),
          pair.Value.GetSourceAsLambda());
        var primaryKeyColumnIndexes = GetIndexesOfPrimaryKeyFields(primaryIndex, pair.Key);
        var selectProvider = new SelectProvider(rangeSetProvider, primaryKeyColumnIndexes);
        if (result == null)
          result = selectProvider;
        else
          result = new UnionProvider(result, selectProvider);
      }
      return result;
    }

    private CompilableProvider InsertRangeSetForExistingIndex(CompilableProvider primaryProvider,
      IndexInfo primaryIndex)
    {
      if (currentRangeSets.ContainsKey(primaryIndex)) {
        var rangeSet = currentRangeSets[primaryIndex];
        primaryProvider = new RangeSetProvider(primaryProvider, rangeSet.GetSourceAsLambda());
      }
      return primaryProvider;
    }

    private static int[] GetIndexesOfPrimaryKeyFields(IndexInfo primaryIndex, IndexInfo secondaryIndex)
    {
      var fieldCount = secondaryIndex.Columns.Count;
      var result = new int[primaryIndex.KeyColumns.Count];
      int resultPosition = 0;
      foreach (KeyValuePair<ColumnInfo, Direction> pair in primaryIndex.KeyColumns)
        for (int i = 0; i < fieldCount; i++)
          if (pair.Key.Equals(secondaryIndex.Columns[i])) {
            result[resultPosition++] = i;
            break;
          }
      return result;
    }

    private static Pair<int>[] GetEqualIndexes(int fieldCount)
    {
      var result = new Pair<int>[fieldCount];
      for (int i = 0; i < fieldCount; i++)
        result[i] = new Pair<int>(i, i);
      return result;
    }
    
    #endregion


    // Constructors

    public ProviderTreeRewriter(DomainModel domainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      this.domainModel = domainModel;
    }
  }
}