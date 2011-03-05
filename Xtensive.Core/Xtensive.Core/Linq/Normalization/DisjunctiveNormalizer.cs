// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Linq.Expressions;
using System.Linq;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Linq.Normalization
{
  /// <summary>
  /// Transforms <see cref="Expression"/> to disjunctive normal form.
  /// </summary>
  [Serializable]
  public sealed class DisjunctiveNormalizer
  {
    /// <summary>
    /// Gets or sets the maximal allowed conjunction operand count.
    /// </summary>
    public int? MaxConjunctionOperandCount { get; private set; }

    /// <summary>
    /// Transform the specified <paramref name="expression"/> to it disjunctive normal form.
    /// </summary>
    /// <param name="expression">The expression to transform.</param>
    /// <returns>Disjunctive normal representation of <paramref name="expression"/>.</returns>
    /// <exception cref="InvalidOperationException">Actual conjunction operand count 
    /// greater than <see cref="MaxConjunctionOperandCount"/>.</exception>
    public DisjunctiveNormalized Normalize(Expression expression)
    {
      Expression unwrappedExp;
      if (expression.NodeType == ExpressionType.Lambda)
        unwrappedExp = Unwrap((LambdaExpression)expression);
      else
        unwrappedExp = expression;
      if (unwrappedExp.Type != typeof(bool))
        throw new ArgumentException(
          String.Format(Resources.Strings.ExExpressionMustReturnValueOfTypeX, typeof(bool)), "expression");
      var unary = unwrappedExp as UnaryExpression;
      if (unary!=null) {
        if (unary.NodeType==ExpressionType.Not &&
          unary.Operand.NodeType==ExpressionType.Not) {
          return Normalize(((UnaryExpression) unary.Operand).Operand);
        }
        BinaryExpression b;
        if (TryConvertInversionToBinary(unary, out b))
          unwrappedExp = b;
        else
          unwrappedExp = unary;
      }

      var binary = unwrappedExp as BinaryExpression;
      var result = binary!=null
        ? NormalizeBinary(binary)
        : new DisjunctiveNormalized(new Conjunction<Expression>(unwrappedExp));

      if (MaxConjunctionOperandCount.HasValue &&
        result.ConjunctionOperandCount > MaxConjunctionOperandCount) {
        throw new InvalidOperationException(
          Resources.Strings.ExActualConjunctionOperandCountGreaterThanExpected);
      }

      return result;
    }

    #region Private methods

    private static Expression Unwrap(LambdaExpression exp)
    {
      var result = exp.Body;
      while (result.NodeType == ExpressionType.Lambda)
        result = ((LambdaExpression)result).Body;
      return result;
    }

    private DisjunctiveNormalized NormalizeBinary(BinaryExpression b)
    {
      if (b.NodeType==ExpressionType.Equal && 
        b.Left.Type==typeof (bool) && 
          b.Right.Type==typeof (bool))
        b = ConvertEqualsToDisjunction(b);
      else if (b.NodeType==ExpressionType.NotEqual && 
        b.Left.Type==typeof (bool) && 
          b.Right.Type==typeof (bool))
        b = ConvertNotEqualsToDisjunction(b);
      else if (b.NodeType==ExpressionType.ExclusiveOr &&
        b.Left.Type==typeof(bool) &&
          b.Right.Type==typeof(bool))
        b = ConvertNotEqualsToDisjunction(b);

      switch (b.NodeType) {
      case ExpressionType.And:
      case ExpressionType.AndAlso:
        return NormalizeCojunction(b);
      case ExpressionType.Or:
      case ExpressionType.OrElse:
        return NormalizeDisjunction(b);
      default:
        return new DisjunctiveNormalized(new Conjunction<Expression>(b));
      }
    }

    private DisjunctiveNormalized NormalizeDisjunction(BinaryExpression b)
    {
      return new DisjunctiveNormalized(
        Normalize(b.Left).Operands.Concat(Normalize(b.Right).Operands));
    }

    private DisjunctiveNormalized NormalizeCojunction(BinaryExpression b)
    {
      var result = new DisjunctiveNormalized();
      foreach (var leftConjunction in Normalize(b.Left).Operands) {
        foreach (var rightConjunction in Normalize(b.Right).Operands) {
          result.Operands.Add(new Conjunction<Expression>(
            leftConjunction.Operands.Concat(rightConjunction.Operands)));
        }
      }
      return result;
    }

    private static BinaryExpression ConvertEqualsToDisjunction(BinaryExpression b)
    {
      var left = b.Left;
      var right = b.Right;
      return Expression.Or(
        Expression.And(left, right), 
        Expression.And(
          Expression.Not(left), 
          Expression.Not(right)));
    }

    private static BinaryExpression ConvertNotEqualsToDisjunction(BinaryExpression b)
    {
      var left = b.Left;
      var right = b.Right;
      return Expression.Or(
        Expression.And(
          Expression.Not(left),
          right),
        Expression.And(
          left,
          Expression.Not(right)));
    }

    private static bool TryConvertInversionToBinary(UnaryExpression exp, out BinaryExpression result)
    {
      var binaryOperand = exp.Operand as BinaryExpression;
      if (binaryOperand==null || 
        (binaryOperand.Left.Type != typeof(bool) &&
        binaryOperand.Right.Type != typeof(bool))) {
        result = null;
        return false;
      }

      switch (binaryOperand.NodeType) {
      case ExpressionType.OrElse:
      case ExpressionType.Or:
        result = Expression.And(
          Expression.Not(binaryOperand.Left), 
          Expression.Not(binaryOperand.Right));
        return true;
      case ExpressionType.And:
      case ExpressionType.AndAlso:
        result = Expression.Or(
          Expression.Not(binaryOperand.Left), 
          Expression.Not(binaryOperand.Right));
        return true;
      case ExpressionType.Equal:
      case ExpressionType.ExclusiveOr:
        result = Expression.NotEqual(binaryOperand.Left, binaryOperand.Right);
        return true;
      case ExpressionType.NotEqual:
        result = Expression.Equal(binaryOperand.Left, binaryOperand.Right);
        return true;
      default:
        result = null;
        return false;
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="maxConjunctionOperandCount">The maximal allowed conjunction operand count.</param>
    public DisjunctiveNormalizer(int maxConjunctionOperandCount)
    {
      ArgumentValidator.EnsureArgumentIsInRange(
        maxConjunctionOperandCount, 1, int.MaxValue, "maxConjunctionOperandCount");

      MaxConjunctionOperandCount = maxConjunctionOperandCount;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisjunctiveNormalizer()
    {
    }
  }
}