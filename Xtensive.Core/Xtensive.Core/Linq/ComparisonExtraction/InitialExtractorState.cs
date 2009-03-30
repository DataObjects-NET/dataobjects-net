// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.ComparisonExtraction
{
  internal class InitialExtractorState : BaseExtractorState
  {
    public ExtractionInfo Extract(Expression exp, Func<Expression, bool> keySelector)
    {
      KeySelector = keySelector;
      return Visit(exp);
    }

    protected override ExtractionInfo VisitBinary(BinaryExpression exp)
    {
      var keyInfo = SelectKey(exp);
      if (keyInfo != null)
        return keyInfo;
      if (!IsComparison(exp.NodeType))
        return null;
      var leftInfo = operandState.Extract(exp.Left);
      var rightInfo = operandState.Extract(exp.Right);
      if (leftInfo == rightInfo)
        return null;
      if (rightInfo != null)
        rightInfo.ReverseRequired = !(rightInfo.ReverseRequired);
      var result = leftInfo != null ? leftInfo : rightInfo;
      if (result.MethodInfo != null)
        return ProcessComparisonMethod(exp.NodeType, leftInfo != null ? exp.Right : exp.Left, result);
      result.ComparisonType = exp.NodeType;
      result.Value = leftInfo!=null ? exp.Right : exp.Left;
      return result;
    }

    protected override ExtractionInfo VisitUnary(UnaryExpression exp)
    {
      var keyInfo = SelectKey(exp);
      if (keyInfo != null)
        return keyInfo;
      if (exp.Type != typeof(bool))
        return null;
      var operandInfo = operandState.Extract(exp.Operand);
      if (exp.NodeType == ExpressionType.Not && operandInfo != null)
        operandInfo.ReverseRequired = !(operandInfo.ReverseRequired);
      return operandInfo;
    }

    private static bool IsComparison(ExpressionType nodeType)
    {
      return nodeType == ExpressionType.GreaterThan || nodeType == ExpressionType.GreaterThanOrEqual ||
             nodeType == ExpressionType.LessThan || nodeType == ExpressionType.LessThanOrEqual ||
             nodeType == ExpressionType.Equal || nodeType == ExpressionType.NotEqual;
    }

    private ExtractionInfo ProcessComparisonMethod(ExpressionType nodeType, Expression rightPart,
      ExtractionInfo extractionInfo)
    {
      if (extractionInfo.MethodInfo.CorrespondsToLikeOperation)
        return ProcessMethodCorrespondingToLike(nodeType, rightPart, extractionInfo);
      return ProcessCompareToMethod(nodeType, rightPart, extractionInfo);
    }

    private ExtractionInfo ProcessMethodCorrespondingToLike(ExpressionType nodeType, Expression rightPart,
      ExtractionInfo extractionInfo)
    {
      var boolConstant = rightPart as ConstantExpression;
      if (boolConstant == null || rightPart.Type != typeof(bool))
        return null;
      var rightValue = (bool)boolConstant.Value;
      extractionInfo.ReverseRequired ^= rightValue;
      extractionInfo.ReverseRequired ^= nodeType == ExpressionType.NotEqual;
      return extractionInfo;
    }

    private ExtractionInfo ProcessCompareToMethod(ExpressionType nodeType, Expression rightPart,
      ExtractionInfo extractionInfo)
    {
      var compareToResult = rightPart as ConstantExpression;
      if (compareToResult == null)
        return null;
      var comparisonResult = (int) compareToResult.Value;
      ExpressionType realComparison;
      if (comparisonResult < 0) {
        if (nodeType == ExpressionType.LessThan || nodeType == ExpressionType.LessThanOrEqual ||
            nodeType == ExpressionType.Equal)
          realComparison = ExpressionType.LessThan;
        else
          realComparison = ExpressionType.GreaterThanOrEqual;
      }
      else if (comparisonResult == 0) {
        realComparison = nodeType;
      }
      else {
        if (nodeType == ExpressionType.LessThan || nodeType == ExpressionType.LessThanOrEqual ||
            nodeType == ExpressionType.NotEqual)
          realComparison = ExpressionType.LessThanOrEqual;
        else
          realComparison = ExpressionType.GreaterThan;
      }
      extractionInfo.ComparisonType = realComparison;
      return extractionInfo;
    }
  }
}