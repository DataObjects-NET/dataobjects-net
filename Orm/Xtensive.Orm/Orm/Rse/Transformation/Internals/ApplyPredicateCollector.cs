// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.05.22

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;


namespace Xtensive.Orm.Rse.Transformation
{
  internal sealed class ApplyPredicateCollector
  {
    private readonly ApplyProviderCorrectorRewriter owner;
    private readonly ApplyParameterSearcher parameterSearcher = new();
    private readonly CollectorHelper collectorHelper = new();

    public bool TryAdd(FilterProvider filter)
    {
      var applyParameter = parameterSearcher.Find(filter.Predicate);
      if (applyParameter != null) {
        if (!owner.State.CheckIfApplyParameterSeflConvertible(applyParameter)) {
          SaveApplyPredicate(filter, applyParameter);
          return true;
        }
      }
      return false;
    }

    public void AliasColumns(AliasProvider provider)
    {
      if (CheckPredicatesExist()) {
        owner.State.Predicates = collectorHelper.GenericAliasColumns(provider, owner.State.Predicates);
      }
    }

    public void ValidateAggregatedColumns(AggregateProvider provider)
    {
      if (CheckPredicatesExist()) {
        foreach (var parameterPair in owner.State.Predicates)
          foreach (var predicatePair in parameterPair.Value)
            foreach (var column in predicatePair.Item2)
              if (provider.GroupColumnIndexes.Contains(column.Index)) {
                ApplyProviderCorrectorRewriter.ThrowInvalidOperationException(
                  Strings.ExColumnsUsedByPredicateContainingApplyParameterAreRemoved);
              }
      }
    }

    public void ValidateSelectedColumnIndexes(SelectProvider provider)
    {
      if (CheckPredicatesExist()) {
        collectorHelper.ValidateNewColumnIndexes(owner.State.Predicates,
          provider.Header.Columns,
          () => Strings.ExColumnsUsedByPredicateContainingApplyParameterAreRemoved);
      }
    }

    #region Private \ internal methods

    private void SaveApplyPredicate(FilterProvider filter, ApplyParameter applyParameter)
    {
      var providerAndFilterColumns = (filter.Predicate, filter.Header.Columns);
      if (owner.State.Predicates.TryGetValue(applyParameter, out var predicates)) {
        predicates.Add(providerAndFilterColumns);
      }
      else {
        owner.State.Predicates.Add(applyParameter,
          new List<(Expression<Func<Tuple, bool>>, ColumnCollection)> { providerAndFilterColumns });
      }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private bool CheckPredicatesExist() =>
      owner.State.Predicates.Count > 0;

    #endregion


    // Constructors

    public ApplyPredicateCollector(ApplyProviderCorrectorRewriter owner)
    {
      ArgumentValidator.EnsureArgumentNotNull(owner, "owner");
      this.owner = owner;
    }
  }
}