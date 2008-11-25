// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.11

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Storage.Rse.Compilation.Expressions.Visitors
{
  /// <summary>
  /// Rewrites an expression tree so that locally isolatable sub-expressions are evaluated and converted into ConstantExpression nodes.
  /// </summary>
  public sealed class ConstantEvaluator : ExpressionVisitor
  {
    #region Nested helper classes

    class EvaluationChecker : ExpressionVisitor
    {
      readonly Func<Expression, bool> evaluatorPredicate;
      readonly HashSet<Expression> candidates;
      bool cannotBeEvaluated;

      private EvaluationChecker(Func<Expression, bool> evaluatorPredicate)
      {
        candidates = new HashSet<Expression>();
        this.evaluatorPredicate = evaluatorPredicate;
      }

      internal static HashSet<Expression> GetCandidates(Func<Expression, bool> evaluatorPredicate, Expression expression)
      {
        var evaluationChecker = new EvaluationChecker(evaluatorPredicate);
        evaluationChecker.Visit(expression);
        return evaluationChecker.candidates;
      }

      protected override Expression Visit(Expression expression)
      {
        if (expression != null) {
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
    }

    #endregion

    private readonly HashSet<Expression> candidates;

    public static Expression Eval(Expression expression)
    {
      return Eval(expression, DefaultPredicate);
    }

    public static Expression Eval(Expression expression, Func<Expression, bool> evaluatorPredicate)
    {
      var candidates = EvaluationChecker.GetCandidates(evaluatorPredicate, expression);
      var ce = new ConstantEvaluator(candidates);
      return ce.Visit(expression);
    }

    protected override Expression Visit(Expression exp)
    {
      if (exp == null)
        return null;
      if (candidates.Contains(exp))
        return Evaluate(exp);
      return base.Visit(exp);
    }

    private Expression Evaluate(Expression e)
    {
      if (e.NodeType==ExpressionType.Constant)
        return e;
      Type type = e.Type;
      if (type.IsValueType)
        e = Expression.Convert(e, typeof (object));
      Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(e);
      Func<object> fn = lambda.Compile();
      return Expression.Constant(fn(), type);
    }

    private static bool DefaultPredicate(Expression expression)
    {
      var cex = expression as ConstantExpression;
      if (cex != null) {
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


    // Constructor

    private ConstantEvaluator(HashSet<Expression> candidates)
    {
      this.candidates = candidates;
    }
  }
}