// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.17

using System.Linq.Expressions;

namespace Xtensive.Linq
{
  /// <summary>
  /// Replaces references to one specific instance of an expression node with another node
  /// </summary>
  public class ExpressionReplacer : ExpressionVisitor
  {
    private readonly Expression searchFor;
    private readonly Expression replaceWith;

    /// <summary>
    /// Replaces the specified expression.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="searchFor">The search for.</param>
    /// <param name="replaceWith">The replace with.</param>
    public static Expression Replace(Expression expression, Expression searchFor, Expression replaceWith)
    {
      return new ExpressionReplacer(searchFor, replaceWith).Visit(expression);
    }

    /// <summary>
    /// Replaces all specified expressions.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="searchFor">Expressions search for.</param>
    /// <param name="replaceWith">Expressions replace with.</param>
    /// <returns></returns>
    public static Expression ReplaceAll(Expression expression, Expression[] searchFor, Expression[] replaceWith)
    {
      for (int i = 0, n = searchFor.Length; i < n; i++)
        expression = Replace(expression, searchFor[i], replaceWith[i]);
      return expression;
    }

    /// <inheritdoc/>
    protected override Expression Visit(Expression exp)
    {
      if (exp == searchFor)
        return replaceWith;
      return base.Visit(exp);
    }

    // Constructors

    private ExpressionReplacer(Expression searchFor, Expression replaceWith)
    {
      this.searchFor = searchFor;
      this.replaceWith = replaceWith;
    }
  }
}