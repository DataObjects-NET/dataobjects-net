// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.06

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Linq.Internals;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// A wrapper for <see cref="Expression"/>
  /// that can be used for comparing expression trees and calculating thier hash codes.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Expression}")]
  public sealed class ExpressionTree
    : IEquatable<ExpressionTree>
  {
    private readonly int hashCode;

    /// <summary>
    /// Gets the expression wrapped by this <see cref="ExpressionTree"/>.
    /// </summary>
    /// <value>The expression.</value>
    public Expression Expression { get; private set; }

    #region ToString, GetHashCode, Equals, ==, != implementation

    /// <inheritdoc/>
    public override string ToString()
    {
      return Expression.ToString(true);
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
    public override int GetHashCode()
    {
      return hashCode;
    }

    /// <inheritdoc/>
    public bool Equals(ExpressionTree other)
    {
      return new ExpressionComparer().AreEqual(Expression, other.Expression);
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
    /// Otherwise, <see langword="false>.
    /// </returns>
    public static bool operator != (ExpressionTree left, ExpressionTree right)
    {
      return !left.Equals(right);
    }

    #endregion

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="expression">The expression.</param>
    public ExpressionTree(Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      Expression = expression;
      hashCode = new ExpressionHashCodeCalculator().CalculateHashCode(expression);
    }
  }
}