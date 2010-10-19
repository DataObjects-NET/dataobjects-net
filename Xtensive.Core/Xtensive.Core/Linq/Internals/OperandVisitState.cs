// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;

namespace Xtensive.Linq
{
  internal class OperandVisitState : BaseExtractorState
  {
    public ExtractionInfo Extract(Expression exp, Func<Expression, bool> keySelector)
    {
      KeySelector = keySelector;
      return Visit(exp);
    }

    protected override ExtractionInfo VisitUnary(UnaryExpression u)
    {
      var result = SelectKey(u);
      if (result != null)
        return result;
      if (u.Type != typeof(bool))
        return null;
      result = Visit(u.Operand);
      if (result != null && u.NodeType == ExpressionType.Not)
        result.InversingRequired = !(result.InversingRequired);
      return result;
    }

    protected override ExtractionInfo VisitMethodCall(MethodCallExpression mc)
    {
      var keyInfo = SelectKey(mc);
      if (keyInfo != null)
        return keyInfo;
      ComparisonMethodInfo methodInfo = ComparisonMethodRepository.Get(mc.Method);
      if (methodInfo == null)
        return null;
      return VisitComparisonMethodState.Extract(mc, methodInfo, KeySelector);
    }
  }
}