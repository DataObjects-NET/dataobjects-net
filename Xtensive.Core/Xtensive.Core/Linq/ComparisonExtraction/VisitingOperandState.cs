// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System.Linq.Expressions;

namespace Xtensive.Core.Linq.ComparisonExtraction
{
  internal class VisitingOperandState : BaseExtractorState
  {
    public ExtractionInfo Extract(Expression exp)
    {
      return Visit(exp);
    }

    protected override ExtractionInfo VisitUnary(UnaryExpression u)
    {
      ExtractionInfo result = base.VisitUnary(u);
      if (result != null)
        return result;
      if (u.Type != typeof(bool))
        return null;
      return Visit(u.Operand);
    }

    protected override ExtractionInfo VisitMethodCall(MethodCallExpression mc)
    {
      ExtractionInfo result = base.VisitMethodCall(mc);
      if (result != null)
        return result;
      ComparisonMethodInfo methodInfo = ComparisonMethodRepository.Get(mc.Method);
      if (methodInfo == null)
        return null;
      return comparisonMethodState.Extract(mc, methodInfo);
    }
  }
}