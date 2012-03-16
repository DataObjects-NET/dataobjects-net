// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.22

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Rse.Helpers;
using Xtensive.Orm.Rse.Providers.Compilable;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;


namespace Xtensive.Orm.Rse.PreCompilation.Correction.ApplyProviderCorrection
{
  internal sealed class CalculateProviderCollector
  {
    private readonly ApplyProviderCorrectorRewriter owner;
    private readonly ApplyParameterSearcher parameterSearcher = new ApplyParameterSearcher();
    private readonly CollectorHelper collectorHelper = new CollectorHelper();
    private readonly TupleAccessGatherer tupleGatherer = new TupleAccessGatherer((p, i) => {});

    public bool TryAdd(CalculateProvider provider)
    {
      var applyParameters = FindApplyParameters(provider);
      if (applyParameters.Count == 0)
        return false;
      if (applyParameters.Count > 1)
        ApplyProviderCorrectorRewriter.ThrowInvalidOperationException();
      var applyParameter = applyParameters[0];
      if (owner.State.SelfConvertibleApplyProviders[applyParameter])
        return false;
      var newPair = new Pair<CalculateProvider, ColumnCollection>(provider, provider.Header.Columns);
      if (owner.State.CalculateProviders.ContainsKey(applyParameter))
        owner.State.CalculateProviders[applyParameter].Add(newPair);
      else
        owner.State.CalculateProviders.Add(applyParameter,
          new List<Pair<CalculateProvider, ColumnCollection>> {newPair});
      return true;
    }

    public bool TryAddFilter(FilterProvider filter)
    {
      var tupleAccesses = tupleGatherer.Gather(filter.Predicate);
      if (tupleAccesses.Count == 0)
        return false;
      return TryAddCalculateFilter(filter, tupleAccesses);
    }

    public void ValidateSelectedColumnIndexes(SelectProvider provider)
    {
      if (owner.State.CalculateProviders.Count > 0)
        collectorHelper.ValidateNewColumnIndexes(owner.State.CalculateProviders,
          provider.Header.Columns,
          Strings.ExColumnsUsedByCalculatedColumnExpressionContainingApplyParameterAreRemoved);
      if (owner.State.CalculateFilters.Count > 0)
        collectorHelper.ValidateNewColumnIndexes(owner.State.CalculateFilters,
          provider.Header.Columns, Strings.ExColumnsUsedByPredicateContainingApplyParameterAreRemoved);
    }

    public void AliasColumns(AliasProvider provider)
    {
      if (owner.State.CalculateProviders.Count > 0)
        owner.State.CalculateProviders = collectorHelper.GenericAliasColumns(provider,
          owner.State.CalculateProviders);
      if (owner.State.CalculateFilters.Count > 0)
        owner.State.CalculateFilters = collectorHelper.GenericAliasColumns(provider,
          owner.State.CalculateFilters);
    }

    #region Private \ internal methods

    private List<ApplyParameter> FindApplyParameters(CalculateProvider newProvider)
    {
      return (from column in newProvider.CalculatedColumns
      let parameter = parameterSearcher.Find(column.Expression)
      where parameter!=null
      select parameter).Distinct().ToList();
    }

    private bool TryAddCalculateFilter(FilterProvider filterProvider, List<int> tupleAccesses)
    {
      var result = false;
      foreach (var key in owner.State.Predicates.Keys) {
        if (!owner.State.CalculateProviders.ContainsKey(key))
          continue;
        foreach (var providerPair in owner.State.CalculateProviders[key]) {
          if (ContainsAccessToTupleField(tupleAccesses, providerPair.First, filterProvider)) {
            result = true;
            AddCalculateFilter(providerPair.First, filterProvider);
          }
        }
      }
      return result;
    }

    private static bool ContainsAccessToTupleField(IEnumerable<int> tupleAccesses,
      CalculateProvider calculateProvider, FilterProvider provider)
    {
      return tupleAccesses.Any(i => calculateProvider.Header.Columns.Contains(provider.Header.Columns[i]));
    }

    private void AddCalculateFilter(CalculateProvider calculateProvider, FilterProvider filterProvider)
    {
      var newPair =
        new Pair<Expression<Func<Tuple, bool>>, ColumnCollection>(filterProvider.Predicate,
          filterProvider.Header.Columns);
      if (owner.State.CalculateFilters.ContainsKey(calculateProvider))
        owner.State.CalculateFilters[calculateProvider].Add(newPair);
      else
        owner.State.CalculateFilters.Add(calculateProvider,
          new List<Pair<Expression<Func<Tuple, bool>>, ColumnCollection>> {newPair});
    }

    #endregion


    // Constructors

    public CalculateProviderCollector(ApplyProviderCorrectorRewriter owner)
    {
      ArgumentValidator.EnsureArgumentNotNull(owner, "owner");
      this.owner = owner;
    }
  }
}