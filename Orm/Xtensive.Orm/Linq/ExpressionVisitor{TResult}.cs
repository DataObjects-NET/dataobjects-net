// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Xtensive.Reflection;
using Xtensive.Core;


namespace Xtensive.Linq
{
  /// <summary>
  /// Abstract <see cref="Expression"/> visitor class.
  /// </summary>
  /// <typeparam name="TResult">Type of the visit result.</typeparam>
  public abstract class ExpressionVisitor<TResult>
  {
    private readonly Dictionary<Expression, TResult> cache;

    /// <summary>
    /// Gets a value indicating whether this visitor is caching.
    /// When visitor is caching, visit result 
    /// is cached and resolved by internal cache.
    /// </summary>
    public bool IsCaching => cache != null;

    /// <summary>
    /// Visits the specified expression.
    /// </summary>
    /// <param name="e">The expression to visit.</param>
    /// <returns>Visit result.</returns>
    /// <remarks>
    /// <see cref="IsCaching"/> policy is enforced by this method.
    /// </remarks>
    protected virtual TResult Visit(Expression e)
    {
      TResult result = default;
      if (e is null || cache?.TryGetValue(e, out result) == true) {
        return result;
      }

      result = e.NodeType switch {
        ExpressionType.Negate
          or ExpressionType.NegateChecked
          or ExpressionType.Not
          or ExpressionType.Convert
          or ExpressionType.ConvertChecked
          or ExpressionType.ArrayLength
          or ExpressionType.Quote
          or ExpressionType.TypeAs => VisitUnary((UnaryExpression) e),
        ExpressionType.Add
          or ExpressionType.AddChecked
          or ExpressionType.Subtract
          or ExpressionType.SubtractChecked
          or ExpressionType.Multiply
          or ExpressionType.MultiplyChecked
          or ExpressionType.Divide
          or ExpressionType.Modulo
          or ExpressionType.And
          or ExpressionType.AndAlso
          or ExpressionType.Or
          or ExpressionType.OrElse
          or ExpressionType.LessThan
          or ExpressionType.LessThanOrEqual
          or ExpressionType.GreaterThan
          or ExpressionType.GreaterThanOrEqual
          or ExpressionType.Equal
          or ExpressionType.NotEqual
          or ExpressionType.Coalesce
          or ExpressionType.ArrayIndex
          or ExpressionType.RightShift
          or ExpressionType.LeftShift
          or ExpressionType.ExclusiveOr => VisitBinary((BinaryExpression) e),
        ExpressionType.TypeIs => VisitTypeIs((TypeBinaryExpression) e),
        ExpressionType.Conditional => VisitConditional((ConditionalExpression) e),
        ExpressionType.Constant => VisitConstant((ConstantExpression) e),
        ExpressionType.Default => VisitDefault((DefaultExpression) e),
        ExpressionType.Parameter => VisitParameter((ParameterExpression) e),
        ExpressionType.MemberAccess => VisitMemberAccess((MemberExpression) e),
        ExpressionType.Call => VisitMethodCall((MethodCallExpression) e),
        ExpressionType.Lambda => VisitLambda((LambdaExpression) e),
        ExpressionType.New => VisitNew((NewExpression) e),
        ExpressionType.NewArrayInit or ExpressionType.NewArrayBounds => VisitNewArray((NewArrayExpression) e),
        ExpressionType.Invoke => VisitInvocation((InvocationExpression) e),
        ExpressionType.MemberInit => VisitMemberInit((MemberInitExpression) e),
        ExpressionType.ListInit => VisitListInit((ListInitExpression) e),
        _ => VisitUnknown(e)
      };

      cache?.Add(e, result);
      return result;
    }

    /// <summary>
    /// Visits the expression list.
    /// </summary>
    /// <param name="expressions">The expression list.</param>
    /// <returns>Visit result.</returns>
    protected virtual IReadOnlyList<TResult> VisitExpressionList(ReadOnlyCollection<Expression> expressions)
    {
      var n = expressions.Count;
      var results = new TResult[n];
      for (int i = 0; i < n; i++) {
        results[i] = Visit(expressions[i]);
      }
      return results.AsSafeWrapper();
    }

    /// <summary>
    /// Visits the unknown expression.
    /// </summary>
    /// <param name="e">The unknown expression.</param>
    /// <returns>Visit result.</returns>
    /// <exception cref="NotSupportedException">Thrown by the base implementation of this method, 
    /// if unknown expression isn't recognized by its overrides.</exception>
    protected virtual TResult VisitUnknown(Expression e)
    {
      throw new NotSupportedException(string.Format(
        Strings.ExUnknownExpressionType, e.GetType().GetShortName(), e.NodeType));
    }

    /// <summary>
    /// Visits the unary expression.
    /// </summary>
    /// <param name="u">The unary expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitUnary(UnaryExpression u);

    /// <summary>
    /// Visits the binary expression.
    /// </summary>
    /// <param name="b">The binary expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitBinary(BinaryExpression b);

    /// <summary>
    /// Visits the "type is" expression.
    /// </summary>
    /// <param name="tb">The "type is" expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitTypeIs(TypeBinaryExpression tb);

    /// <summary>
    /// Visits the constant expression.
    /// </summary>
    /// <param name="c">The constant expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitConstant(ConstantExpression c);

    /// <summary>
    /// Visits the default expression.
    /// </summary>
    /// <param name="d">The default expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitDefault(DefaultExpression d);

    /// <summary>
    /// Visits the conditional expression.
    /// </summary>
    /// <param name="c">The conditional expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitConditional(ConditionalExpression c);

    /// <summary>
    /// Visits the parameter expression.
    /// </summary>
    /// <param name="p">The parameter expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitParameter(ParameterExpression p);

    /// <summary>
    /// Visits the member access expression.
    /// </summary>
    /// <param name="m">The member access expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitMemberAccess(MemberExpression m);

    /// <summary>
    /// Visits the method call expression.
    /// </summary>
    /// <param name="mc">The method call expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitMethodCall(MethodCallExpression mc);

    /// <summary>
    /// Visits the lambda expression.
    /// </summary>
    /// <param name="l">The lambda expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitLambda(LambdaExpression l);

    /// <summary>
    /// Visits the "new" expression.
    /// </summary>
    /// <param name="n">The "new" expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitNew(NewExpression n);

    /// <summary>
    /// Visits the member initialization expression.
    /// </summary>
    /// <param name="mi">The member initialization expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitMemberInit(MemberInitExpression mi);

    /// <summary>
    /// Visits the list initialization expression.
    /// </summary>
    /// <param name="li">The list initialization expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitListInit(ListInitExpression li);

    /// <summary>
    /// Visits the "new array" expression.
    /// </summary>
    /// <param name="na">The "new array" expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitNewArray(NewArrayExpression na);

    /// <summary>
    /// Visits the invocation expression.
    /// </summary>
    /// <param name="i">The invocation expression.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitInvocation(InvocationExpression i);

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="isCaching">Indicates whether visit result 
    /// should be cached and resolved by cache when possible.</param>
    protected ExpressionVisitor(bool isCaching = false)
    {
      cache = isCaching ? new() : null;
    }
  }
}
