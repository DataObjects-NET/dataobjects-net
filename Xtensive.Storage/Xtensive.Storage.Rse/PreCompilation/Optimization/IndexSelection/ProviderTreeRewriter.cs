// Copyright (C) 2009 Xtensive LLC.
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
      var primaryProvider = provider.Source;
      if (primaryProvider != null && currentRangeSets.Count > 0) {
        var primaryIndex = ((IndexProvider)primaryProvider).Index.Resolve(domainModel);
        if (currentRangeSets.ContainsKey(primaryIndex)) {
          var rangeSet = currentRangeSets[primaryIndex];
          primaryProvider = new RangeSetProvider(primaryProvider, rangeSet.GetSourceAsLambda());
        }
        CompilableProvider secondaryProvider = null;
        foreach (var pair in currentRangeSets.Where(p => p.Key.IsSecondary)) {
          var rangeSetProvider = new RangeSetProvider(IndexProvider.Get(pair.Key), pair.Value.GetSourceAsLambda());
          var primaryKeyColumnIndexes = GetIndexesOfPrimaryKeyFields(primaryIndex, pair.Key);
          var selectProvider = new SelectProvider(rangeSetProvider, primaryKeyColumnIndexes);
          if (secondaryProvider == null)
            secondaryProvider = selectProvider;
          else
            secondaryProvider = new UnionProvider(secondaryProvider, selectProvider);
        }
        if (secondaryProvider != null) {
          var alias = new AliasProvider(secondaryProvider, "secondary");
          var join = new JoinProvider(primaryProvider, alias, false, JoinType.Hash, GetEqualIndexes(primaryIndex.KeyColumns.Count));
          var resultSelectProvider = new SelectProvider(join, Enumerable.Range(0, primaryIndex.Columns.Count).ToArray());
          return new FilterProvider(resultSelectProvider, provider.Predicate);
        }
        return new FilterProvider(primaryProvider, provider.Predicate);
      }
      return base.VisitFilter(provider);
    }

    #region Private \ internal methods

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