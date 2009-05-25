// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.22

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Correction.ApplyProviderCorrection
{
  internal sealed class ApplyPredicateCollector
  {
    private readonly ApplyProviderCorrectorRewriter owner;
    private readonly ApplyParameterSearcher parameterSearcher = new ApplyParameterSearcher();
    private readonly CollectorHelper collectorHelper = new CollectorHelper();

    public bool TryAdd(FilterProvider filter)
    {
      var applyParameter = parameterSearcher.Find(filter.Predicate);
      if(applyParameter != null) {
        if (!owner.State.SelfConvertibleApplyProviders[applyParameter]) {
          SaveApplyPredicate(filter, applyParameter);
          return true;
        }
      }
      return false;
    }

    public void AliasColumns(AliasProvider provider)
    {
      if(owner.State.Predicates.Count > 0)
        owner.State.Predicates = collectorHelper.GenericAliasColumns(provider, owner.State.Predicates);
    }

    public void ValidateAggregatedColumns(AggregateProvider provider)
    {
      if (owner.State.Predicates.Count==0)
        return;
      foreach (var parameterPair in owner.State.Predicates)
        foreach (var predicatePair in parameterPair.Value)
          foreach (var column in predicatePair.Second)
            if (provider.GroupColumnIndexes.Contains(column.Index))
              owner.ThrowInvalidOperationException();
          
    }

    public void ValidateSelectedColumnIndexes(SelectProvider provider)
    {
      if(owner.State.Predicates.Count == 0)
        return;
      ValidateNewPredicateColumnIndexes(provider.Header.Columns);
    }

    #region Private \ internal methods

    private void SaveApplyPredicate(FilterProvider filter, ApplyParameter applyParameter)
    {
      if(owner.State.Predicates.ContainsKey(applyParameter))
        owner.State.Predicates[applyParameter]
          .Add(new Pair<Expression<Func<Tuple, bool>>, ColumnCollection>(filter.Predicate,
            filter.Header.Columns));
      else {
        var newPair = new Pair<Expression<Func<Tuple, bool>>, ColumnCollection>(filter.Predicate,
          filter.Header.Columns);
        owner.State.Predicates.Add(applyParameter,
          new List<Pair<Expression<Func<Tuple, bool>>, ColumnCollection>> {newPair});
      }
    }

    /*private void AliasPredicateColumns(AliasProvider provider)
    {
      var newPredicates =
        new Dictionary<ApplyParameter, List<Pair<Expression<Func<Tuple, bool>>, ColumnCollection>>>(
          owner.State.Predicates.Count);
      foreach (var parameterPair in owner.State.Predicates) {
        var newParameterPairValue = new List<Pair<Expression<Func<Tuple, bool>>, ColumnCollection>>();
        foreach (var predicatePair in parameterPair.Value) {
          var newPredicatePair = new Pair<Expression<Func<Tuple, bool>>, ColumnCollection>(
            predicatePair.First, predicatePair.Second.Alias(provider.Alias));
          newParameterPairValue.Add(newPredicatePair);
        }
        newPredicates.Add(parameterPair.Key, newParameterPairValue);
      }
      owner.State.Predicates = newPredicates;
    }*/

    private void ValidateNewPredicateColumnIndexes(ICollection<Column> mappedColumns)
    {
      foreach (var parameterPair in owner.State.Predicates)
        foreach (var predicatePair in parameterPair.Value)
          if (!collectorHelper.CheckPresenceOfOldColumns(predicatePair.Second, mappedColumns))
            owner.ThrowInvalidOperationException();
    }

    #endregion


    // Constructors

    public ApplyPredicateCollector(ApplyProviderCorrectorRewriter owner)
    {
      ArgumentValidator.EnsureArgumentNotNull(owner, "owner");
      this.owner = owner;
    }
  }
}