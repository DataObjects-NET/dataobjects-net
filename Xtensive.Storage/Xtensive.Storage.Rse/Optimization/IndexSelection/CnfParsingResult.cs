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
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  /// <summary>
  /// Result of extraction a <see cref="RangeSet{T}"/> from a predicate.
  /// </summary>
  [Serializable]
  internal sealed class CnfParsingResult : IEnumerable<RangeSetInfo>
  {
    private readonly HashSet<RangeSetInfo> partsOfResult = new HashSet<RangeSetInfo>();
    private LambdaExpression result;
    private bool resultIsStale = true;

    public readonly IndexInfo IndexInfo;

    public void AddPart(RangeSetInfo rsInfo)
    {
      resultIsStale = true;
      partsOfResult.Add(rsInfo);
    }

    public bool HasResult()
    {
      if(resultIsStale)
        CreateResultExpression();
      return result != null;
    }

    public LambdaExpression GetResult()
    {
      if(resultIsStale)
        CreateResultExpression();
      return result;
    }

    #region Implementation of IEnumerable

    public IEnumerator<RangeSetInfo> GetEnumerator()
    {
      return partsOfResult.GetEnumerator();
    }

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
      RangeSetInfo tempResult = null;
      foreach (var part in partsOfResult) {
        if (tempResult == null)
          tempResult = part;
        else
          tempResult = RangeSetExpressionBuilder.BuildUnite(tempResult, part);
      }
      if (tempResult != null)
        result = Expression.Lambda(tempResult.Source);
    }

    // Constructors

    public CnfParsingResult(IndexInfo indexInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(indexInfo, "indexInfo");
      IndexInfo = indexInfo;
    }
  }
}