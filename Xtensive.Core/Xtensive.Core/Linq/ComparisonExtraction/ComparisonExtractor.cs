// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.ComparisonExtraction
{
  public class ComparisonExtractor
  {
    public ComparisonInfo Extract(Expression exp, Func<Expression, bool> keySelector)
    {
      ArgumentValidator.EnsureArgumentNotNull(exp, "exp");
      ArgumentValidator.EnsureArgumentNotNull(keySelector, "keySelector");
      ExtractionInfo extractionInfo = BaseExtractorState.InitialState.Extract(exp, keySelector);
      if (extractionInfo == null)
        return null;
      if (extractionInfo.CanNormalize())
        return new ComparisonInfo(extractionInfo);
      throw new NotImplementedException();
    }

    internal static ExpressionType ReverseOperation(ExpressionType comparisonType)
    {
      switch (comparisonType) {
        case ExpressionType.Equal:
        case ExpressionType.NotEqual:
          return comparisonType;
        case ExpressionType.GreaterThan:
          return ExpressionType.LessThan;
        case ExpressionType.GreaterThanOrEqual:
          return ExpressionType.LessThanOrEqual;
        case ExpressionType.LessThan:
          return ExpressionType.GreaterThan;
        case ExpressionType.LessThanOrEqual:
          return ExpressionType.GreaterThanOrEqual;
        default:
          return comparisonType;
      }
    }
  }
}