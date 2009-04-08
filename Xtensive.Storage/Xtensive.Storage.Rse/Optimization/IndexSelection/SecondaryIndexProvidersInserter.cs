// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.07

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  internal class SecondaryIndexProvidersInserter : CompilableProviderVisitor
  {
    private readonly DomainModel domainModel;
    private Dictionary<IndexInfo, RangeSetInfo> currentRangeSets;

    public Provider Insert(CompilableProvider source,
      Dictionary<IndexInfo, RangeSetInfo> rangeSets)
    {
      ArgumentValidator.EnsureArgumentNotNull(rangeSets, "rangeSets");
      if (rangeSets.Count == 0)
        throw new ArgumentException(Resources.Strings.ExCollectionMustNotBeEmpty, "rangeSets");
      currentRangeSets = rangeSets;
      return Visit(source);
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      var primaryProvider = provider.Source as IndexProvider;
      if (primaryProvider != null)
        return InsertSecondaryIndexProviders(primaryProvider);
      return base.VisitFilter(provider);
    }

    private Provider InsertSecondaryIndexProviders(IndexProvider source)
    {
      var primaryIndex = source.Index.Resolve(domainModel);
      CompilableProvider concatedIndexes = BuildSecondaryIndexesConcat(primaryIndex);
      return BuildJoin(source, primaryIndex, concatedIndexes);
    }

    private CompilableProvider BuildSecondaryIndexesConcat(IndexInfo primaryIndex)
    {
      if(currentRangeSets.Count == 1)
        return CreateSelectProvider(primaryIndex, currentRangeSets.First());

      CompilableProvider result = null;
      foreach (var pair in currentRangeSets)
        if (result == null)
          result = CreateSelectProvider(primaryIndex, pair);
        else
          result = BuildConcatenation(result, primaryIndex, pair);
      return result;
    }

    private static SelectProvider CreateSelectProvider(IndexInfo primaryIndex,
      KeyValuePair<IndexInfo, RangeSetInfo> pair)
    {
      return new SelectProvider(CreateRangeSetProvider(pair),
        GetIndexesOfPrimaryKeyFields(primaryIndex, pair.Key));
    }

    private static CompilableProvider CreateRangeSetProvider(KeyValuePair<IndexInfo, RangeSetInfo> pair)
    {
      return new RangeSetProvider(IndexProvider.Get(pair.Key), pair.Value.GetSourceAsLambda());
    }

    private static int[] GetIndexesOfPrimaryKeyFields(IndexInfo primaryIndex, IndexInfo secondaryIndex)
    {
      var fieldCount = secondaryIndex.Columns.Count;
      var result = new int[primaryIndex.KeyColumns.Count];
      int resultPosition = 0;
      foreach (var primaryIndexColumn in primaryIndex.KeyColumns)
        for (int i = 0; i < fieldCount; i++)
          if(primaryIndexColumn.Equals(secondaryIndex.Columns[i]))
            result[resultPosition++] = i;
      return result;
    }

    private static Pair<int>[] GetEqualIndexes(int fieldCount)
    {
      var result = new Pair<int>[fieldCount];
      for (int i = 0; i < fieldCount; i++)
        result[i] = new Pair<int>(i, i);
      return result;
    }

    private static CompilableProvider BuildConcatenation(CompilableProvider source, IndexInfo primaryIndex,
      KeyValuePair<IndexInfo, RangeSetInfo> targetPair)
    {
      var concatProvider = new ConcatProvider(source, CreateSelectProvider(primaryIndex, targetPair));
      return new DistinctProvider(concatProvider);
    }

    private static CompilableProvider BuildJoin(CompilableProvider primaryIndexProvider, IndexInfo primaryIndex,
      CompilableProvider secondaryIndexes)
    {
      var alias = new AliasProvider(secondaryIndexes, "secondary");
      return new JoinProvider(alias, primaryIndexProvider, false, JoinType.Hash,
        GetEqualIndexes(primaryIndex.KeyColumns.Count));
    }

    // Constructors

    public SecondaryIndexProvidersInserter(DomainModel domainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      this.domainModel = domainModel;
    }
  }
}