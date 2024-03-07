// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    public static Expression ReplaceAll(
      Expression expression, IReadOnlyList<Expression> searchFor, IReadOnlyList<Expression> replaceWith)
    {
      for (int i = 0, n = searchFor.Count; i < n; i++) {
        expression = Replace(expression, searchFor[i], replaceWith[i]);
      }

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