// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
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
      var result = FillValueInfo(objInfo, argInfo, exp, keyIndex);

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
        result.ReversingRequired = true;
      return result;
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
            throw new ArgumentException(Resources.Strings.ExCannotParseCallToComparisonMethod);
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
            throw new ArgumentException(Resources.Strings.ExCannotParseCallToComparisonMethod);
          argumentInfo = tempInfo;
          keyIndex = i;
        }
      }
      return argumentInfo;
    }

    private static ExtractionInfo FillValueInfo(ExtractionInfo objectInfo, ExtractionInfo argumentInfo,
      MethodCallExpression exp, int keyIndex)
    {
      var result = objectInfo!=null ? objectInfo : argumentInfo;
      if (result==null)
        throw new ArgumentException(Resources.Strings.ExCannotParseCallToComparisonMethod);
      if (result==objectInfo) {
        if (exp.Arguments.Count!=1)
          throw new ArgumentException(Resources.Strings.ExCannotParseCallToComparisonMethod);
        result.Value = exp.Arguments[0];
      }
      else {
        if (keyIndex < 0)
          throw new ArgumentException(Resources.Strings.ExCannotParseCallToComparisonMethod);
        if (exp.Method.IsStatic) {
          if (exp.Arguments.Count < 2 || keyIndex > 1)
            throw new ArgumentException(Resources.Strings.ExCannotParseCallToComparisonMethod);
          if (keyIndex==1) {
            result.Value = exp.Arguments[0];
            result.ReversingRequired = !(result.ReversingRequired);
          }
          else
            result.Value = exp.Arguments[1];
        }
        else {
          if (exp.Arguments.Count < 1)
            throw new ArgumentException(Resources.Strings.ExCannotParseCallToComparisonMethod);
          result.Value = exp.Object;
          result.ReversingRequired = !(result.ReversingRequired);
        }
      }
      return result;
    }
  }
}