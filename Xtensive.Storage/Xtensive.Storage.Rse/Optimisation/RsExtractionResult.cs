// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimisation
{
  /// <summary>
  /// Represents a result of extraction a RangeSet from a predicate.
  /// </summary>
  internal class RsExtractionResult : IEnumerable<RangeSetExpression>
  {
    private readonly SetSlim<RangeSetExpression> partsOfResult = new SetSlim<RangeSetExpression>();
    private LambdaExpression result;
    private bool resultIsStale = true;

    public readonly IndexInfo IndexInfo;

    public void AddPart(RangeSetExpression rsExpression)
    {
      if (rsExpression.Type != typeof(RangeSet<Entire<Tuple>>))
        throw new ArgumentException(String.Format(Resources.Strings.ExExpressionMustReturnValueOfTypeX,
                                                  typeof(RangeSet<Entire<Tuple>>)));
      resultIsStale = true;
      partsOfResult.Add(rsExpression);
    }

    public bool HasResult()
    {
      if(resultIsStale)
        CreateResultExpression();
      return result != null;
    }

    /// <summary>
    /// </summary>
    /// <returns>The complete result consisting of result parts combined by the <c>Unite</c>
    /// method of <see cref="RangeSet{T}"/>.</returns>
    public LambdaExpression GetResult()
    {
      if(resultIsStale)
        CreateResultExpression();
      return result;
    }

    #region Implementation of IEnumerable

    /// <summary>
    /// </summary>
    /// <returns>Enumerator for the collection of result parts.</returns>
    public IEnumerator<RangeSetExpression> GetEnumerator()
    {
      return partsOfResult.GetEnumerator();
    }

    /// <summary>
    /// Returns a enumerator for the collection of result parts.
    /// </summary>
    /// <returns>Enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    private void CreateResultExpression()
    {
      result = null;
      resultIsStale = false;
      if(partsOfResult.Count == 0)
        return;
      RangeSetExpression tempResult = null;
      foreach (var part in partsOfResult) {
        if (tempResult == null)
          tempResult = part;
        else
          tempResult = RangeSetExpressionsBuilder.BuildUnite(tempResult, part);
      }
      if (tempResult != null)
        result = Expression.Lambda(tempResult.Source);
    }

    public RsExtractionResult(IndexInfo indexInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(indexInfo, "indexInfo");
      IndexInfo = indexInfo;
    }
  }
}