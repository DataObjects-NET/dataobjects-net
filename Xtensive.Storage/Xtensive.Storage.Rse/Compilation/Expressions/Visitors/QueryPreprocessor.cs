// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.26

using System;
using System.Linq.Expressions;

namespace Xtensive.Storage.Rse.Compilation.Expressions.Visitors
{
  public static class QueryPreprocessor
  {
    public static Expression Translate(Expression expression)
    {
      var candidates = EvaluationChecker.GetCandidates(expression);
      expression = FieldAccessTranslator.Translate(expression, candidates);
      expression = ParameterAccessTranslator.Translate(expression, candidates);
      expression = ConstantEvaluator.Evaluate(expression, candidates);
      return expression;
    }
  }
}