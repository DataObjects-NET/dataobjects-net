// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Linq.Normalization
{
  /// <summary>
  /// Provides methods for transform <see cref="Expression"/> to disjunctive normal form.
  /// </summary>
  [Serializable]
  public sealed class DisjunctiveNormalizer
  {
    private bool finish;
    private DisjunctiveNormalized normalizedResult;

    public int MaxTermsCount { get; private set; }

    /// <summary>
    /// Transform the specified <see cref="Expression"/> to it disjunctive normal form.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public DisjunctiveNormalized Normalize(Expression expression)
    {
      if (finish)
        return normalizedResult;

      var binary = expression as BinaryExpression;
      if (binary != null) {
        return NormalizeBinary(binary);
      }

      var unary = expression as UnaryExpression;
      if (unary != null)
        return NormalizeUnary(unary);
      
      return new DisjunctiveNormalized(new Conjunction<Expression>(expression));
    }

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
      if (b.NodeType==ExpressionType.Equal && b.Left.Type==typeof (bool) && b.Right.Type==typeof (bool))
        b = ConvertEqualsToDisjunction(b);
      else if (b.NodeType==ExpressionType.NotEqual && b.Left.Type==typeof (bool) && b.Right.Type==typeof (bool))
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
      var normalizedLeft = Normalize(b.Left);
      if (finish)
        return normalizedResult;
      var normalizedRight = Normalize(b.Right);
      if (finish)
        return normalizedResult;
      
      var normalized = new DisjunctiveNormalized(normalizedLeft.Operands, normalizedRight.Operands);
      
      if (MaxTermsCount > 0 && normalized.TermsCount > MaxTermsCount) {
        normalizedResult = normalizedLeft.TermsCount > normalizedRight.TermsCount ? normalizedLeft : normalizedRight;
        finish = true;
        return normalizedResult;
      }

      return normalized;
    }

    private DisjunctiveNormalized NormalizeCojunction(BinaryExpression b)
    {
      var normalizedLeft = Normalize(b.Left);
      if (finish)
        return normalizedResult;
      var normalizedRight = Normalize(b.Right);
      if (finish)
        return normalizedResult;
      
      var normalized = new DisjunctiveNormalized();
      foreach (var leftConjunction in Normalize(b.Left).Operands) {
        foreach (var rightConjunction in Normalize(b.Right).Operands) {
          normalized.Operands.Add(new Conjunction<Expression>(leftConjunction.Operands, rightConjunction.Operands));
        }
      }

      if (MaxTermsCount > 0 && normalized.TermsCount > MaxTermsCount)
      {
        normalizedResult = normalizedLeft.TermsCount > normalizedRight.TermsCount ? normalizedLeft : normalizedRight;
        finish = true;
        return normalizedResult;
      }

      return normalized;
    }
    
    private static BinaryExpression ConvertEqualsToDisjunction(BinaryExpression b)
    {
      var left = b.Left;
      var right = b.Right;
      return Expression.Or(Expression.And(left, right), Expression.And(Expression.Not(left), Expression.Not(right)));
    }

    private static BinaryExpression ConvertNotEqualsToDisjunction(BinaryExpression b)
    {
      var left = b.Left;
      var right = b.Right;
      return Expression.Or(Expression.And(Expression.Not(left), right), Expression.And(left, Expression.Not(right)));
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
        result = Expression.And(Expression.Not(binaryOperand.Left), Expression.Not(binaryOperand.Right));
        return true;
      case ExpressionType.And:
      case ExpressionType.AndAlso:
        result = Expression.Or(Expression.Not(binaryOperand.Left), Expression.Not(binaryOperand.Right));
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


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="maxTermsCount">The maximum count of result terms.</param>
    public DisjunctiveNormalizer(int maxTermsCount)
    {
      MaxTermsCount = maxTermsCount;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisjunctiveNormalizer()
    {
      MaxTermsCount = -1;
    }

  }
}