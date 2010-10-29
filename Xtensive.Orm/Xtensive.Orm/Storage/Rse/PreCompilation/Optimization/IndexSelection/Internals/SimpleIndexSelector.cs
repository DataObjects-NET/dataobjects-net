// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.06

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Storage.Model;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  internal sealed class SimpleIndexSelector : IIndexSelector
  {
    private readonly ICostEvaluator costEvaluator;

    #region Implementation of IIndexSelector

    public Dictionary<IndexInfo, RangeSetInfo> Select(Dictionary<Expression, List<RsExtractionResult>> extractionResults)
    {
      var result = new Dictionary<IndexInfo, RangeSetInfo>(extractionResults.Count);
      foreach (var pair in extractionResults) {
        var cheapestResult = TrySelectCheapestResult(pair.Value);
        if (cheapestResult == null) {
          result.Clear();
          return result;
        }
        if (!result.ContainsKey(cheapestResult.IndexInfo))
          result.Add(cheapestResult.IndexInfo, cheapestResult.RangeSetInfo);
        else
          result[cheapestResult.IndexInfo] = RangeSetExpressionBuilder
            .BuildUnite(result[cheapestResult.IndexInfo], cheapestResult.RangeSetInfo);
      }
      return result;
    }

    #endregion

    #region Private \ internal methods
    private RsExtractionResult TrySelectCheapestResult(IEnumerable<RsExtractionResult> extractionResults)
    {
      RsExtractionResult cheapestResult = null;
      double minimalCost = double.MaxValue;
      double minimalCount = double.MaxValue;
      RsExtractionResult primaryAsCheapest = null;
      foreach (var result in extractionResults) {
        double currentCost;
        if (result.RangeSetInfo.AlwaysFull)
          currentCost = double.MaxValue;
        else {
          var rangeSet = result.RangeSetInfo.GetRangeSet();
          if (rangeSet.IsFull())
            currentCost = double.MaxValue;
          else {
            var costInfo = costEvaluator.Evaluate(result.IndexInfo, rangeSet);
            currentCost = costInfo.TotalCost;
            if (costInfo.RecordCount < minimalCount) {
              primaryAsCheapest = result.IndexInfo.IsPrimary ? result : null;
              minimalCount = costInfo.RecordCount;
            }
          }
        }
        if (currentCost < minimalCost) {
          minimalCost = currentCost;
          cheapestResult = result;
        }
      }
      if (primaryAsCheapest != null)
        return primaryAsCheapest;
      return cheapestResult;
    }
    #endregion


    // Constructors

    public SimpleIndexSelector(ICostEvaluator costEvaluator)
    {
      ArgumentValidator.EnsureArgumentNotNull(costEvaluator, "costEvaluator");
      this.costEvaluator = costEvaluator;
    }
  }
}