// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.ComparisonExtraction
{
  internal class ComparisonMethodVisitngState : BaseExtractorState
  {
    public ExtractionInfo Extract(MethodCallExpression exp, ComparisonMethodInfo methodInfo)
    {
      ExtractionInfo objInfo = Visit(exp.Object);
      int keyIndex;
      ExtractionInfo argInfo = VisitMethodCallArguments(exp, out keyIndex);
      if (objInfo == argInfo)
        return null;
      var result = FillValueInfo(objInfo, argInfo, exp.Arguments, keyIndex);

      result.ComparisonMethod = exp;
      result.MethodInfo = methodInfo;
      return result;
    }

    protected override ExtractionInfo VisitUnary(UnaryExpression u)
    {
      ExtractionInfo result = base.VisitUnary(u);
      if (result != null)
        return result;
      if(u.Type != typeof(bool))
        return null;
      result = Visit(u.Operand);
      if (result != null && u.NodeType == ExpressionType.Not)
        result.ReverseRequired = true;
      return result;
    }

    private ExtractionInfo VisitMethodCallArguments(MethodCallExpression exp, out int keyIndex)
    {
      keyIndex = -1;
      if (exp.Arguments.Count > 0) {
        var firstInfo = Visit(exp.Arguments[0]);
        if (exp.Arguments.Count > 1) {
          var secondInfo = Visit(exp.Arguments[1]);
          if (firstInfo != null && secondInfo != null)
            throw new ArgumentException(Resources.Strings.ExCannotParseCallToComparisonMethod);
          if (secondInfo != null) {
            keyIndex = 1;
            return secondInfo;
          }
        }
        if(firstInfo != null) {
          keyIndex = 0;
          return firstInfo;
        }
      }
      return null;
    }

    private static ExtractionInfo FillValueInfo(ExtractionInfo objectInfo, ExtractionInfo argumentInfo,
      ReadOnlyCollection<Expression> arguments, int keyIndex)
    {
      var result = objectInfo != null ? objectInfo : argumentInfo;
      if(result == null)
        throw new ArgumentException(Resources.Strings.ExCannotParseCallToComparisonMethod);
      if (result == objectInfo) {
        if (arguments.Count == 0)
          throw new ArgumentException(Resources.Strings.ExCannotParseCallToComparisonMethod);
        result.Value = arguments[1];
      }
      else {
        if (arguments.Count < 2 || keyIndex < 0)
          throw new ArgumentException(Resources.Strings.ExCannotParseCallToComparisonMethod);
        if (keyIndex > 1)
          result.Value = arguments[0];
        else
          result.Value = arguments[1];
      }
      return result;
    }
  }
}