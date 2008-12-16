// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  public sealed class EvaluationChecker : ExpressionVisitor
  {
    private readonly Func<Expression, bool> evaluatorPredicate;
    private readonly HashSet<Expression> candidates;
    private bool cannotBeEvaluated;

    public static HashSet<Expression> GetCandidates(Expression expression)
    {
      return GetCandidates(expression, DefaultPredicate);
    }

    public static HashSet<Expression> GetCandidates(Expression expression, Func<Expression, bool> evaluatorPredicate)
    {
      var evaluationChecker = new EvaluationChecker(evaluatorPredicate);
      evaluationChecker.Visit(expression);
      return evaluationChecker.candidates;
    }

    protected override Expression Visit(Expression expression)
    {
      if (expression != null)
      {
        bool saveCannotBeEvaluated = cannotBeEvaluated;
        cannotBeEvaluated = false;
        base.Visit(expression);
        if (!cannotBeEvaluated)
          if (evaluatorPredicate(expression))
            candidates.Add(expression);
          else
            cannotBeEvaluated = true;
        cannotBeEvaluated |= saveCannotBeEvaluated;
      }
      return expression;
    }

    protected override Expression VisitUnknown(Expression expression)
    {
      return expression;
    }

    private static bool DefaultPredicate(Expression expression)
    {
      var cex = expression as ConstantExpression;
      if (cex != null)
      {
        var query = cex.Value as IQueryable;
        if (query != null)
          return false;
      }
      var mc = expression as MethodCallExpression;
      if (mc != null && (mc.Method.DeclaringType == typeof(Enumerable) || mc.Method.DeclaringType == typeof(Queryable)))
        return false;
      if (expression.NodeType == ExpressionType.Convert && expression.Type == typeof(object))
        return true;
      return expression.NodeType != ExpressionType.Parameter &&
        expression.NodeType != ExpressionType.Lambda;
    }

    // Constructors

    private EvaluationChecker(Func<Expression, bool> evaluatorPredicate)
    {
      candidates = new HashSet<Expression>();
      this.evaluatorPredicate = evaluatorPredicate;
    }
  }
}