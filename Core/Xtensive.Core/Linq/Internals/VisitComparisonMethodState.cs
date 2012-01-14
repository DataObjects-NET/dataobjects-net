// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Linq
{
  internal class VisitComparisonMethodState : BaseExtractorState
  {
    public ExtractionInfo Extract(MethodCallExpression exp, ComparisonMethodInfo methodInfo,
      Func<Expression, bool> keySelector)
    {
      KeySelector = keySelector;
      ExtractionInfo objInfo = Visit(exp.Object);
      int keyIndex;
      ExtractionInfo argInfo = VisitMethodCallArguments(exp, out keyIndex);
      if (objInfo == argInfo)
        return null;
      var result = ExtractValueInfo(objInfo, argInfo, exp, keyIndex);

      result.ComparisonMethod = exp;
      result.MethodInfo = methodInfo;
      if(result.MethodInfo.ComparisonKind != ComparisonKind.Default)
        return ProcessNonDefaultComparison(result);
      return result;
    }

    private static ExtractionInfo ProcessNonDefaultComparison(ExtractionInfo extractionInfo)
    {
      switch (extractionInfo.MethodInfo.ComparisonKind) {
      case ComparisonKind.LikeEndsWith:
      case ComparisonKind.LikeStartsWith:
        return extractionInfo;
      case ComparisonKind.Equality:
        return ProcessEqualityComparisonMethod(extractionInfo);
      case ComparisonKind.ForcedGreaterThan:
      case ComparisonKind.ForcedGreaterThanOrEqual:
      case ComparisonKind.ForcedLessThan:
      case ComparisonKind.ForcedLessThanOrEqual:
        return ProcessMethodWithFocedComparisonType(extractionInfo);
      default:
        throw Exceptions.InvalidArgument(extractionInfo.MethodInfo.ComparisonKind,
          "extractionInfo.MethodInfo.ComparisonKind");
      }
    }

    private ExtractionInfo VisitMethodCallArguments(MethodCallExpression exp, out int keyIndex)
    {
      keyIndex = -1;
      if (exp.Method.IsStatic)
        return VisitStaticMethodCallArguments(exp, ref keyIndex);
      return VisitInstanceMethodCallArguments(exp, ref keyIndex);
    }

    private ExtractionInfo VisitStaticMethodCallArguments(MethodCallExpression exp, ref int keyIndex)
    {
      // For a static method, we analyse only two first arguments.
      var argumentCount = exp.Arguments.Count;
      if (argumentCount > 0) {
        var firstInfo = Visit(exp.Arguments[0]);
        if (argumentCount > 1) {
          var secondInfo = Visit(exp.Arguments[1]);
          if (firstInfo != null && secondInfo != null)
            throw Exceptions.InternalError(Resources.Strings.ExCannotParseCallToComparisonMethod,
              Log.Instance);
          if (secondInfo != null) {
            keyIndex = 1;
            return secondInfo;
          }
        }
        if (firstInfo != null) {
          keyIndex = 0;
          return firstInfo;
        }
      }
      return null;
    }

    private ExtractionInfo VisitInstanceMethodCallArguments(MethodCallExpression exp, ref int keyIndex)
    {
      var argumentCount = exp.Arguments.Count;
      ExtractionInfo argumentInfo = null;
      for (int i = 0; i < argumentCount; i++) {
        var tempInfo = Visit(exp.Arguments[i]);
        if (tempInfo != null) {
          if (argumentInfo != null)
            throw Exceptions.InternalError(Resources.Strings.ExCannotParseCallToComparisonMethod,
              Log.Instance);
          argumentInfo = tempInfo;
          keyIndex = i;
        }
      }
      return argumentInfo;
    }

    private static ExtractionInfo ExtractValueInfo(ExtractionInfo objectInfo, ExtractionInfo argumentInfo,
      MethodCallExpression exp, int keyIndex)
    {
      var result = objectInfo!=null ? objectInfo : argumentInfo;
      if (result==null)
        throw Exceptions.InternalError(Resources.Strings.ExCannotParseCallToComparisonMethod,
          Log.Instance);
      if (result==objectInfo)
        return ExtractValueFromInstanceMethodOfKey(result, exp);
      return ExtractValueFromMethod(result, exp, keyIndex);
    }

    private static ExtractionInfo ExtractValueFromMethod(ExtractionInfo result, MethodCallExpression exp,
      int keyIndex)
    {
      if (keyIndex < 0)
        throw Exceptions.InternalError(Resources.Strings.ExCannotParseCallToComparisonMethod,
          Log.Instance);
      if (exp.Method.IsStatic) {
        if (exp.Arguments.Count < 2 || keyIndex > 1)
          throw Exceptions.InternalError(Resources.Strings.ExCannotParseCallToComparisonMethod,
            Log.Instance);
        Expression value;
        bool reversingRequired = false;
        if (keyIndex==1) {
          value = exp.Arguments[0];
          reversingRequired = true;
        }
        else
          value = exp.Arguments[1];
        CheckThatKeyAndValueAreOfCompatibleTypes(result.Key, value);
        if(reversingRequired)
          result.ReversingRequired = !(result.ReversingRequired);
        result.Value = value;
      }
      else {
        if (exp.Arguments.Count < 1)
          throw Exceptions.InternalError(Resources.Strings.ExCannotParseCallToComparisonMethod,
            Log.Instance);
        result.Value = exp.Object;
        result.ReversingRequired = !(result.ReversingRequired);
      }
      return result;
    }

    private static ExtractionInfo ExtractValueFromInstanceMethodOfKey(ExtractionInfo result,
      MethodCallExpression exp)
    {
      if (exp.Arguments.Count == 0)
        throw Exceptions.InternalError(Resources.Strings.ExCannotParseCallToComparisonMethod,
          Log.Instance);
      var value = exp.Arguments[0];
      CheckThatKeyAndValueAreOfCompatibleTypes(result.Key, value);
      result.Value = value;
      return result;
    }

    private static void CheckThatKeyAndValueAreOfCompatibleTypes(Expression key, Expression value)
    {
      if (value.Type != key.Type && !key.Type.IsSubclassOf(value.Type))
        throw Exceptions.InternalError(Resources.Strings.ExCannotParseCallToComparisonMethod,
          Log.Instance);
    }

    private static ExtractionInfo ProcessEqualityComparisonMethod(ExtractionInfo extractionInfo)
    {
      extractionInfo.ComparisonOperation = ExpressionType.Equal;
      return extractionInfo;
    }

    private static ExtractionInfo ProcessMethodWithFocedComparisonType(ExtractionInfo extractionInfo)
    {
      switch (extractionInfo.MethodInfo.ComparisonKind) {
      case ComparisonKind.ForcedGreaterThan:
        extractionInfo.ComparisonOperation = ExpressionType.GreaterThan;
        break;
      case ComparisonKind.ForcedGreaterThanOrEqual:
        extractionInfo.ComparisonOperation = ExpressionType.GreaterThanOrEqual;
        break;
      case ComparisonKind.ForcedLessThan:
        extractionInfo.ComparisonOperation = ExpressionType.LessThan;
        break;
      case ComparisonKind.ForcedLessThanOrEqual:
        extractionInfo.ComparisonOperation = ExpressionType.LessThanOrEqual;
        break;
      default:
        throw Exceptions.InvalidArgument(extractionInfo.MethodInfo.ComparisonKind,
          "extractionInfo.MethodInfo.ComparisonKind");
      }
      return extractionInfo;
    }
  }
}