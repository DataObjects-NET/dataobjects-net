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
using Xtensive.Linq;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Transformation
{
  internal sealed class CollectorHelper
  {
    private readonly ParameterRewriter parameterRewriter = new ParameterRewriter();

    public Expression<Func<Tuple, bool>> CreatePredicatesConjunction(
      Expression<Func<Tuple, bool>> newPredicate, Expression<Func<Tuple, bool>> oldPredicate)
    {
      var oldParameter = oldPredicate.Parameters[0];
      var newParameter = newPredicate.Parameters[0];
      var result = (Expression<Func<Tuple, bool>>) parameterRewriter
        .Replace(oldPredicate, oldParameter, newParameter);
      return (Expression<Func<Tuple, bool>>) FastExpression.Lambda(Expression
        .AndAlso(result.Body, newPredicate.Body),newParameter);
    }

    public Expression<Func<Tuple, Tuple, bool>> CreatePredicatesConjunction(
      Expression<Func<Tuple, Tuple, bool>> newPredicate, Expression<Func<Tuple, Tuple, bool>> oldPredicate)
    {
      var oldParameter0 = oldPredicate.Parameters[0];
      var newParameter0 = newPredicate.Parameters[0];
      var result = (Expression<Func<Tuple, Tuple, bool>>) parameterRewriter
        .Replace(oldPredicate, oldParameter0, newParameter0);
      var oldParameter1 = result.Parameters[1];
      var newParameter1 = newPredicate.Parameters[1];
      result = (Expression<Func<Tuple, Tuple, bool>>) parameterRewriter
        .Replace(result, oldParameter1, newParameter1);
      return (Expression<Func<Tuple, Tuple, bool>>) FastExpression.Lambda(Expression
        .AndAlso(result.Body, newPredicate.Body), newParameter0, newParameter1);
    }

    public bool CheckPresenceOfOldColumns(IEnumerable<Column> oldColumns,
      IReadOnlyCollection<Column> mappedColumns)
    {
      foreach (var column in oldColumns)
        if (!mappedColumns.Contains(column))
          return false;
      return true;
    }

    public Dictionary<TDictKey, List<Pair<TPairKey, ColumnCollection>>> 
      GenericAliasColumns<TDictKey, TPairKey>(AliasProvider provider,
      Dictionary<TDictKey, List<Pair<TPairKey, ColumnCollection>>> currentState)
    {
      var newFilters =
        new Dictionary<TDictKey, List<Pair<TPairKey, ColumnCollection>>>(currentState.Count);
      foreach (var providerPair in currentState) {
        var newProviderPairValue = new List<Pair<TPairKey, ColumnCollection>>(providerPair.Value.Count);
        foreach (var predicatePair in providerPair.Value) {
          var newPredicatePair = new Pair<TPairKey, ColumnCollection>(
            predicatePair.First, predicatePair.Second.Alias(provider.Alias));
          newProviderPairValue.Add(newPredicatePair);
        }
        newFilters.Add(providerPair.Key, newProviderPairValue);
      }
      return newFilters;
    }

    public void ValidateNewColumnIndexes<TDictKey, TPairKey>(
      Dictionary<TDictKey, List<Pair<TPairKey, ColumnCollection>>> currentState,
      IReadOnlyCollection<Column> mappedColumns, string description)
    {
      foreach (var providerPair in currentState)
        foreach (var predicatePair in providerPair.Value)
          if (!CheckPresenceOfOldColumns(predicatePair.Second, mappedColumns))
            ApplyProviderCorrectorRewriter.ThrowInvalidOperationException(description);
    }
  }
}