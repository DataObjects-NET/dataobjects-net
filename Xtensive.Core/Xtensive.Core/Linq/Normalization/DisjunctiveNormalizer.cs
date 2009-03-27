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
      if (isFinished)
        return result;

      var binary = expression as BinaryExpression;
      if (binary!=null)
        return NormalizeBinary(binary);

      var unary = expression as UnaryExpression;
      if (unary != null)
        return NormalizeUnary(unary);
      
      return new DisjunctiveNormalized(new Conjunction<Expression>(expression));
    }

    #region Private methods

    private DisjunctiveNormalized NormalizeUnary(UnaryExpression u)
    {
      if (u.NodeType==ExpressionType.Not) {
        if (u.Operand.NodeType==ExpressionType.Not) {
          return Normalize(((UnaryExpression) u.Operand).Operand);
        }
        
        BinaryExpression binary;
        if (TryConvertInversionToBinary(u, out binary))
          return Normalize(binary);
      }

      return new DisjunctiveNormalized(new Conjunction<Expression>(u));
    }

    private DisjunctiveNormalized NormalizeBinary(BinaryExpression b)
    {
      if (b.NodeType==ExpressionType.Equal && 
        b.Left.Type==typeof (bool) && 
          b.Right.Type==typeof (bool))
        b = NormalizeEquals(b);
      else if (b.NodeType==ExpressionType.NotEqual && 
        b.Left.Type==typeof (bool) && 
          b.Right.Type==typeof (bool))
        b = NormalizeNotEquals(b);

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
      var left = Normalize(b.Left);
      if (isFinished)
        return result;
      var right = Normalize(b.Right);
      if (isFinished)
        return result;
      
      var newResult = new DisjunctiveNormalized(
        left.Operands.Concat(right.Operands));
      
      if (newResult.ConjunctionOperandCount > MaxConjunctionOperandCount) {
        result = 
          left.ConjunctionOperandCount > right.ConjunctionOperandCount ? 
                                                                         left : right;
        isFinished = true;
        return result;
      }

      return newResult;
    }

    private DisjunctiveNormalized NormalizeCojunction(BinaryExpression b)
    {
      var left = Normalize(b.Left);
      if (isFinished)
        return result;
      var right = Normalize(b.Right);
      if (isFinished)
        return result;
      
      var newResult = new DisjunctiveNormalized();
      foreach (var leftConjunction in Normalize(b.Left).Operands) {
        foreach (var rightConjunction in Normalize(b.Right).Operands) {
          newResult.Operands.Add(new Conjunction<Expression>(
            leftConjunction.Operands.Concat(rightConjunction.Operands)));
        }
      }

      if (newResult.ConjunctionOperandCount > MaxConjunctionOperandCount) {
        result = 
          left.ConjunctionOperandCount > right.ConjunctionOperandCount ? 
                                                                         left : right;
        isFinished = true;
        return result;
      }

      return newResult;
    }
    
    private static BinaryExpression NormalizeEquals(BinaryExpression b)
    {
      var left = b.Left;
      var right = b.Right;
      return Expression.Or(
        Expression.And(left, right), 
        Expression.And(
          Expression.Not(left), 
          Expression.Not(right)));
    }

    private static BinaryExpression NormalizeNotEquals(BinaryExpression b)
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
      if (binaryOperand==null) {
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