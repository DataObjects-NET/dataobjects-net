// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.05.22

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Transformation
{
  internal sealed class CalculateProviderCollector
  {
    private readonly ApplyProviderCorrectorRewriter owner;
    private readonly ApplyParameterSearcher parameterSearcher = new();
    private readonly CollectorHelper collectorHelper = new();
    private readonly TupleAccessGatherer tupleGatherer = new((p, i) => {});

    public bool TryAdd(CalculateProvider provider)
    {
      var applyParameters = FindApplyParameters(provider);
      if (applyParameters.Count == 0)
        return false;
      if (applyParameters.Count > 1)
        ApplyProviderCorrectorRewriter.ThrowInvalidOperationException();
      var applyParameter = applyParameters[0];
      if (owner.State.CheckIfApplyParameterSeflConvertible(applyParameter))
        return false;
      var newPair = (provider, provider.Header.Columns);

      if (owner.State.CalculateProviders.TryGetValue(applyParameter, out var existingList)) {
        existingList.Add(newPair);
      }
      else {
        owner.State.CalculateProviders.Add(applyParameter,
          new List<(CalculateProvider, ColumnCollection)> {newPair});
      }
      return true;
    }

    public bool TryAddFilter(FilterProvider filter)
    {
      var tupleAccesses = tupleGatherer.Gather(filter.Predicate);
      return tupleAccesses.Count != 0 && TryAddCalculateFilter(filter, tupleAccesses);
    }

    public void ValidateSelectedColumnIndexes(SelectProvider provider)
    {
      if (CheckCalculateProvidersExist()) {
        collectorHelper.ValidateNewColumnIndexes(owner.State.CalculateProviders,
          provider.Header.Columns,
          () => Strings.ExColumnsUsedByCalculatedColumnExpressionContainingApplyParameterAreRemoved);
      }

      if (CheckCalculateFiltersExist()) {
        collectorHelper.ValidateNewColumnIndexes(owner.State.CalculateFilters,
          provider.Header.Columns,
          () => Strings.ExColumnsUsedByPredicateContainingApplyParameterAreRemoved);
      }
    }

    public void AliasColumns(AliasProvider provider)
    {
      if (CheckCalculateProvidersExist()) {
        owner.State.CalculateProviders = collectorHelper.GenericAliasColumns(provider,
          owner.State.CalculateProviders);
      }
      if (CheckCalculateFiltersExist()) {
        owner.State.CalculateFilters = collectorHelper.GenericAliasColumns(provider,
          owner.State.CalculateFilters);
      }
    }

    #region Private \ internal methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckCalculateProvidersExist() => owner.State.CalculateProviders.Count > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckCalculateFiltersExist() => owner.State.CalculateFilters.Count > 0;


    private List<ApplyParameter> FindApplyParameters(CalculateProvider newProvider)
    {
      return (from column in newProvider.CalculatedColumns
          let parameter = parameterSearcher.Find(column.Expression)
          where parameter!=null
          select parameter)
        .Distinct()
        .ToList();
    }

    private bool TryAddCalculateFilter(FilterProvider filterProvider, List<int> tupleAccesses)
    {
      var result = false;
      foreach (var key in owner.State.Predicates.Keys) {
        if (owner.State.CalculateProviders.TryGetValue(key, out var providerPairs)) {
          foreach (var providerPair in providerPairs) {
            if (ContainsAccessToTupleField(tupleAccesses, providerPair.Item1, filterProvider)) {
              result = true;
              AddCalculateFilter(providerPair.Item1, filterProvider);
            }
          }
        }
      }
      return result;
    }

    private static bool ContainsAccessToTupleField(IEnumerable<int> tupleAccesses,
        CalculateProvider calculateProvider, FilterProvider provider) =>
      tupleAccesses.Any(i => calculateProvider.Header.Columns.Contains(provider.Header.Columns[i]));

    private void AddCalculateFilter(CalculateProvider calculateProvider, FilterProvider filterProvider)
    {
      var newPair = (filterProvider.Predicate, filterProvider.Header.Columns);
      if (owner.State.CalculateFilters.TryGetValue(calculateProvider, out var filters)) {
        filters.Add(newPair);
      }
      else {
        owner.State.CalculateFilters.Add(calculateProvider,
          new List<(Expression<Func<Tuple, bool>>, ColumnCollection)> {newPair});
      }
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