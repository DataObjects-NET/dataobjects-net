// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;

namespace Xtensive.Linq
{
  internal class InitialExtractorState : BaseExtractorState
  {
    private readonly KeySearcher keySearcher = new KeySearcher();

    public ExtractionInfo Extract(Expression exp, Func<Expression, bool> keySelector)
    {
      KeySelector = keySelector;
      var result = Visit(exp);
      if (IsValueInvalidValid(result))
        return null;
      if (IsStandAloneBooleanExpression(result)) {
        result.Value = Expression.Constant(true);
        result.ComparisonOperation = ExpressionType.Equal;
      }
      if (result != null && result.Value == null)
        return null;
      return result;
    }

    private bool IsValueInvalidValid(ExtractionInfo result)
    {
      return result!=null && result.Value!=null
        && keySearcher.ContainsKey(result.Value, KeySelector);
    }

    private static bool IsStandAloneBooleanExpression(ExtractionInfo result)
    {
      return result != null
        && result.Value == null && result.Key.Type == typeof(bool) && result.ComparisonOperation == null
          && (result.MethodInfo == null || result.MethodInfo.Method.ReturnType == typeof(bool)
            && (result.MethodInfo.ComparisonKind == ComparisonKind.Default
              || result.MethodInfo.ComparisonKind == ComparisonKind.Equality));
    }

    protected override ExtractionInfo VisitBinary(BinaryExpression exp)
    {
      var keyInfo = SelectKey(exp);
      if (keyInfo != null)
        return keyInfo;
      if (!IsComparison(exp.NodeType))
        return null;
      var leftInfo = OperandVisitState.Extract(exp.Left, KeySelector);
      var rightInfo = OperandVisitState.Extract(exp.Right, KeySelector);
      if (leftInfo == rightInfo)
        return null;
      if (rightInfo != null)
        rightInfo.ReversingRequired = !(rightInfo.ReversingRequired);
      var result = leftInfo != null ? leftInfo : rightInfo;
      if (result.MethodInfo != null)
        return ProcessComparisonMethodInBinaryExpression(exp.NodeType,
          leftInfo != null ? exp.Right : exp.Left, result);
      result.ComparisonOperation = exp.NodeType;
      result.Value = leftInfo!=null ? exp.Right : exp.Left;
      return result;
    }

    protected override ExtractionInfo VisitUnary(UnaryExpression exp)
    {
      var keyInfo = SelectKey(exp);
      if (keyInfo!=null)
        return keyInfo;
      if (exp.Type!=typeof (bool))
        return null;
      var operandInfo = Visit(exp.Operand);
      if (operandInfo!=null && exp.NodeType==ExpressionType.Not)
        operandInfo.InversingRequired = !(operandInfo.InversingRequired);
      return operandInfo;
    }

    protected override ExtractionInfo VisitMethodCall(MethodCallExpression exp)
    {
      var keyInfo = SelectKey(exp);
      if (keyInfo != null)
        return keyInfo;
      var result = OperandVisitState.Extract(exp, KeySelector);
      return result;
    }

    private static bool IsComparison(ExpressionType nodeType)
    {
      return nodeType==ExpressionType.GreaterThan || nodeType==ExpressionType.GreaterThanOrEqual
        || nodeType==ExpressionType.LessThan || nodeType==ExpressionType.LessThanOrEqual
          || nodeType==ExpressionType.Equal || nodeType==ExpressionType.NotEqual;
    }

    private static ExtractionInfo ProcessComparisonMethodInBinaryExpression(ExpressionType nodeType,
      Expression rightPart, ExtractionInfo extractionInfo)
    {
      if (extractionInfo.MethodInfo.ComparisonKind != ComparisonKind.Default)
        return null;
      return ProcessCompareToMethod(nodeType, rightPart, extractionInfo);
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