// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using System.Linq;

namespace Xtensive.Core.Linq.Normalization
{
  /// <summary>
  /// Transforms <see cref="Expression"/> to disjunctive normal form.
  /// </summary>
  [Serializable]
  public sealed class DisjunctiveNormalizer
  {
    private bool isFinished;
    [NonSerialized]
    private DisjunctiveNormalized result;

    /// <summary>
    /// Gets or sets the maximal allowed conjunction operand count.
    /// Default is <see cref="int.MaxValue"/>.
    /// </summary>
    public int MaxConjunctionOperandCount { get; private set; }

    /// <summary>
    /// Transform the specified <paramref name="expression"/> to it disjunctive normal form.
    /// </summary>
    /// <param name="expression">The expression to transform.</param>
    /// <returns>Disjunctive normal representation of <paramref name="expression"/>.</returns>
    public DisjunctiveNormalized Normalize(Expression expression)
    {
      isFinished = false;
      result = null;
      return NormalizeInternal(expression);
    }
    
    #region Private methods

    private DisjunctiveNormalized NormalizeInternal(Expression expression)
    {
      var unary = expression as UnaryExpression;
      if (unary!=null) {
        if (unary.NodeType==ExpressionType.Not &&
          unary.Operand.NodeType==ExpressionType.Not) {
          return NormalizeInternal(((UnaryExpression) unary.Operand).Operand);
        }
        BinaryExpression b;
        if (TryConvertInversionToBinary(unary, out b))
          expression = b;
        else
          expression = unary;
      }

      var binary = expression as BinaryExpression;
      if (binary!=null)
        return NormalizeBinary(binary);

      return new DisjunctiveNormalized(new Conjunction<Expression>(expression));
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
      var left = NormalizeInternal(b.Left);
      if (isFinished) {
        return left;
      }
        
      var right = NormalizeInternal(b.Right);
      if (isFinished) {
        return right;
      }

      var newResult = new DisjunctiveNormalized(
        left.Operands.Concat(right.Operands));
      
      if (newResult.ConjunctionOperandCount > MaxConjunctionOperandCount) {
        isFinished = true;
      }

      return newResult;
    }

    private DisjunctiveNormalized NormalizeCojunction(BinaryExpression b)
    {
      var left = NormalizeInternal(b.Left);
      if (isFinished) {
        return left;
      }

      var right = NormalizeInternal(b.Right);
      if (isFinished) {
        return right;
      }

      var newResult = new DisjunctiveNormalized();
      foreach (var leftConjunction in left.Operands) {
        foreach (var rightConjunction in right.Operands) {
          newResult.Operands.Add(new Conjunction<Expression>(
            leftConjunction.Operands.Concat(rightConjunction.Operands)));
        }
      }

      if (newResult.ConjunctionOperandCount > MaxConjunctionOperandCount) {
        isFinished = true;
      }

      return newResult;
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
        Expression.And(Expression.Not(left), right), 
        Expression.And(left, Expression.Not(right)));
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
      MaxConjunctionOperandCount = maxConjunctionOperandCount;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisjunctiveNormalizer()
    {
      MaxConjunctionOperandCount = int.MaxValue;
    }
  }
}