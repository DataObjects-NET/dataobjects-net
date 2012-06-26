// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.06

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Linq
{
  /// <summary>
  /// A wrapper for <see cref="System.Linq.Expressions.Expression"/>.
  /// that can be used for comparing expression trees and calculating their hash codes.
  /// </summary>
  [DebuggerDisplay("{Expression}")]
  public sealed class ExpressionTree
    : IEquatable<ExpressionTree>
  {
    private readonly int hashCode;
    private readonly Expression expression;

    /// <summary>
    /// Gets the underlying <see cref="Expression"/>.
    /// </summary>
    /// <returns></returns>
    public Expression ToExpression()
    {
      return expression;
    }
    
    #region ToString, GetHashCode, Equals, ==, != implementation

    /// <inheritdoc/>
    public override string ToString()
    {
      return expression.ToString(true);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return hashCode;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      var other = obj as ExpressionTree;
      if (ReferenceEquals(other, null))
        return false;
      return Equals(other);
    }

    /// <inheritdoc/>
    public bool Equals(ExpressionTree other)
    {
      return new ExpressionComparer().AreEqual(expression, other.expression);
    }
    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is equal to <paramref name="right"/>.
    /// Otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator == (ExpressionTree left, ExpressionTree right)
    {
      return left.Equals(right);
    }
    
    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/>.
    /// Otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator != (ExpressionTree left, ExpressionTree right)
    {
      return !left.Equals(right);
    }

    #endregion

    /// <summary>
    /// Compares specified <see cref="Expression"/>s by value.
    /// </summary>
    /// <param name="left">First expression to compare.</param>
    /// <param name="right">Second expression to compare.</param>
    /// <returns>true, if <paramref name="left"/> and <paramref name="right"/>
    /// are equal by value, otherwise false.</returns>
    public static bool Equals(Expression left, Expression right)
    {
      return new ExpressionComparer().AreEqual(left, right);
    }

    /// <summary>
    /// Calculates hash code by value for the specified <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">Expression to calculate hash code for.</param>
    /// <returns>Hash code for <paramref name="expression"/>.</returns>
    public static int GetHashCode(Expression expression)
    {
      return new ExpressionHashCodeCalculator().CalculateHashCode(expression);
    }


    // Constructors

    internal ExpressionTree(Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      this.expression = expression;
      hashCode = new ExpressionHashCodeCalculator().CalculateHashCode(expression);
    }
  }
}