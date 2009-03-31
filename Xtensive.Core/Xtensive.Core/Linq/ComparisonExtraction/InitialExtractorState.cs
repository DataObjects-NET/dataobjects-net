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
        rightInfo.ReversingRequired = !(rightInfo.ReversingRequired);
      var result = leftInfo != null ? leftInfo : rightInfo;
      if (result.MethodInfo != null)
        return ProcessComparisonMethod(exp.NodeType, leftInfo != null ? exp.Right : exp.Left, result);
      result.ComparisonOperation = exp.NodeType;
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
      var operandInfo = Visit(exp.Operand);
      if (exp.NodeType == ExpressionType.Not && operandInfo != null)
        operandInfo.InversingRequired = !(operandInfo.InversingRequired);
      return operandInfo;
    }

    protected override ExtractionInfo VisitMethodCall(MethodCallExpression exp)
    {
      var extractionInfo = operandState.Extract(exp);
      if (extractionInfo!=null && extractionInfo.MethodInfo!=null
        && extractionInfo.MethodInfo.CorrespondsToLikeOperation)
        return ProcessMethodCorrespondingToLike(exp.NodeType, null, extractionInfo);
      return null;
    }

    private static bool IsComparison(ExpressionType nodeType)
    {
      return nodeType == ExpressionType.GreaterThan || nodeType == ExpressionType.GreaterThanOrEqual ||
             nodeType == ExpressionType.LessThan || nodeType == ExpressionType.LessThanOrEqual ||
             nodeType == ExpressionType.Equal || nodeType == ExpressionType.NotEqual;
    }

    private static ExtractionInfo ProcessComparisonMethod(ExpressionType nodeType, Expression rightPart,
      ExtractionInfo extractionInfo)
    {
      if (extractionInfo.MethodInfo.CorrespondsToLikeOperation)
        return ProcessMethodCorrespondingToLike(nodeType, rightPart, extractionInfo);
      return ProcessCompareToMethod(nodeType, rightPart, extractionInfo);
    }

    private static ExtractionInfo ProcessMethodCorrespondingToLike(ExpressionType nodeType,
      Expression rightPart, ExtractionInfo extractionInfo)
    {
      extractionInfo.Value = MakeValueForLikeOperation(extractionInfo);
      var boolConstant = rightPart as ConstantExpression;
      if (boolConstant != null) {
        if (rightPart.Type != typeof(bool))
          return null;
        var rightValue = (bool) boolConstant.Value;
        extractionInfo.InversingRequired ^= rightValue;
        extractionInfo.InversingRequired ^= nodeType==ExpressionType.NotEqual;
      }
      return extractionInfo;
    }

    private static MethodCallExpression MakeValueForLikeOperation(ExtractionInfo extractionInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(extractionInfo.MethodInfo.LikePattern,
        "extractionInfo.MethodInfo.LikePattern");
      var formatMethod = typeof (string).GetMethod("Format", new[] {typeof (string), typeof (object)});
      return Expression.Call(null, formatMethod, Expression.Constant(extractionInfo.MethodInfo.LikePattern),
        extractionInfo.Value);
    }

    private static ExtractionInfo ProcessCompareToMethod(ExpressionType nodeType, Expression rightPart,
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
      extractionInfo.ComparisonOperation = realComparison;
      return extractionInfo;
    }
  }
}