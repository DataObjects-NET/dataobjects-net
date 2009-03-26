// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.Normalization
{
  /// <summary>
  /// Provides methods for transform <see cref="Expression"/> to disjunctive normal form.
  /// </summary>
  [Serializable]
  public class DisjunctiveNormalizer : ExpressionVisitor
  {
    /// <inheritdoc/>
    protected override Expression VisitUnary(UnaryExpression u)
    {
      if (u.NodeType==ExpressionType.Not) {
        if (u.Operand.NodeType==ExpressionType.Not) {
          return Visit(((UnaryExpression) u.Operand).Operand);
        }
        else if (IsConjunctionOrDisjunction(u.Operand))
          return VisitBinary(MoveInversionDown(u));
      }

      return base.VisitUnary(u);
    }

    /// <inheritdoc/>
    protected override Expression VisitBinary(BinaryExpression b)
    {
      if (IsConjunction(b))
        return VisitConjunction(b);

      var left = Visit(b.Left);
      var right = Visit(b.Right);

      if (b.NodeType == ExpressionType.Equal && (!IsTerm(left) || !IsTerm(right))) {
        var binary = Expression.Or(Expression.And(left, right), Expression.And(Expression.Not(left), Expression.Not(right)));
        return VisitBinary(binary);
      }

      if (b.NodeType == ExpressionType.NotEqual && (!IsTerm(left) || !IsTerm(right))) {
        var binary = Expression.Or(Expression.And(Expression.Not(left), right), Expression.And(left, Expression.Not(right)));
        return VisitBinary(binary);
      }

      return base.VisitBinary(b);
    }

    protected virtual Expression VisitConjunction(BinaryExpression c)
    {
      var left = Visit(c.Left);
      var right = Visit(c.Right);
      
      var binaryLeft = left as BinaryExpression;
      if (binaryLeft != null && IsDisjunction(binaryLeft)) {
        return Expression.Or(Expression.And(binaryLeft.Left, right), Expression.And(binaryLeft.Right, right));
      }

      var binaryRight = right as BinaryExpression;
      if(binaryRight != null && IsDisjunction(binaryRight)) {
        return Expression.Or(Expression.And(left, binaryRight.Left), Expression.And(left, binaryRight.Right));
      }

      return Expression.MakeBinary(c.NodeType, left, right, c.IsLiftedToNull, c.Method);
    }

    protected virtual BinaryExpression MoveInversionDown(UnaryExpression exp)
    {
      var binaryChild = exp.Operand as BinaryExpression;
      BinaryExpression newExp;

      if (IsDisjunction(binaryChild))
        newExp = Expression.And(Expression.Not(binaryChild.Left), Expression.Not(binaryChild.Right));
      else
        newExp = Expression.Or(Expression.Not(binaryChild.Left), Expression.Not(binaryChild.Right));

      return newExp;
    }

    /// <summary>
    /// Transform the specified <see cref="Expression"/> to it disjunctive normal form.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public Expression Normalize(Expression expression)
    {
      return Visit(expression);
    }

    protected static bool IsTerm(Expression e)
    {
      var binary = e as BinaryExpression;
      if (binary != null)
        return !IsConjunctionOrDisjunction(binary);

      //var unary = e as UnaryExpression;
      //if (unary != null)
      //  return unary.NodeType==ExpressionType.Not && IsTerm(unary.Operand);

      return true;
    }

    protected static bool IsConjunctionOrDisjunction(Expression exp)
    {
      var binary = exp as BinaryExpression;
      return IsConjunctionOrDisjunction(binary);
    }

    protected static bool IsConjunctionOrDisjunction(BinaryExpression exp)
    {
      return IsConjunction(exp) || IsDisjunction(exp);
    }

    protected static bool IsConjunction(BinaryExpression exp)
    {
      return exp.Type == typeof(bool)
          && (exp.NodeType == ExpressionType.And
          || exp.NodeType == ExpressionType.AndAlso);
    }

    protected static bool IsDisjunction(BinaryExpression exp)
    {
      return exp.Type == typeof(bool)
          && (exp.NodeType == ExpressionType.Or
          || exp.NodeType == ExpressionType.OrElse);
    }

  }
}