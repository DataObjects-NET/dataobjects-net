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



namespace Xtensive.Linq
{
  /// <summary>
  /// Abstract <see cref="Expression"/> visitor class.
  /// </summary>
  /// <typeparam name="TResult">Type of the visit result.</typeparam>
  public abstract class ExpressionVisitor<TResult>
  {
    private readonly Dictionary<Expression, TResult> cache = null;

    /// <summary>
    /// Gets a value indicating whether this visitor is caching.
    /// When visitor is caching, visit result 
    /// is cached and resolved by internal cache.
    /// </summary>
    public bool IsCaching
    {
      get { return cache!=null; }
    }
    
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
      if (e==null)
        return default(TResult);

      TResult result;
      if (cache!=null) {
        if (cache.TryGetValue(e, out result))
          return result;
      }

      switch (e.NodeType) {
      case ExpressionType.Negate:
      case ExpressionType.NegateChecked:
      case ExpressionType.Not:
      case ExpressionType.Convert:
      case ExpressionType.ConvertChecked:
      case ExpressionType.ArrayLength:
      case ExpressionType.Quote:
      case ExpressionType.TypeAs:
        result = VisitUnary((UnaryExpression) e);
        break;
      case ExpressionType.Add:
      case ExpressionType.AddChecked:
      case ExpressionType.Subtract:
      case ExpressionType.SubtractChecked:
      case ExpressionType.Multiply:
      case ExpressionType.MultiplyChecked:
      case ExpressionType.Divide:
      case ExpressionType.Modulo:
      case ExpressionType.And:
      case ExpressionType.AndAlso:
      case ExpressionType.Or:
      case ExpressionType.OrElse:
      case ExpressionType.LessThan:
      case ExpressionType.LessThanOrEqual:
      case ExpressionType.GreaterThan:
      case ExpressionType.GreaterThanOrEqual:
      case ExpressionType.Equal:
      case ExpressionType.NotEqual:
      case ExpressionType.Coalesce:
      case ExpressionType.ArrayIndex:
      case ExpressionType.RightShift:
      case ExpressionType.LeftShift:
      case ExpressionType.ExclusiveOr:
        result = VisitBinary((BinaryExpression) e);
        break;
      case ExpressionType.TypeIs:
        result = VisitTypeIs((TypeBinaryExpression) e);
        break;
      case ExpressionType.Conditional:
        result = VisitConditional((ConditionalExpression) e);
        break;
      case ExpressionType.Constant:
        result = VisitConstant((ConstantExpression) e);
        break;
      case ExpressionType.Parameter:
        result = VisitParameter((ParameterExpression) e);
        break;
      case ExpressionType.MemberAccess:
        result = VisitMemberAccess((MemberExpression) e);
        break;
      case ExpressionType.Call:
        result = VisitMethodCall((MethodCallExpression) e);
        break;
      case ExpressionType.Lambda:
        result = VisitLambda((LambdaExpression) e);
        break;
      case ExpressionType.New:
        result = VisitNew((NewExpression) e);
        break;
      case ExpressionType.NewArrayInit:
      case ExpressionType.NewArrayBounds:
        result = VisitNewArray((NewArrayExpression) e);
        break;
      case ExpressionType.Invoke:
        result = VisitInvocation((InvocationExpression) e);
        break;
      case ExpressionType.MemberInit:
        result = VisitMemberInit((MemberInitExpression) e);
        break;
      case ExpressionType.ListInit:
        result = VisitListInit((ListInitExpression) e);
        break;
      default:
        result = VisitUnknown(e);
        break;
      }

      if (cache!=null)
        cache[e] = result;
      return result;
    }

    /// <summary>
    /// Visits the expression list.
    /// </summary>
    /// <param name="expressions">The expression list.</param>
    /// <returns>Visit result.</returns>
    protected virtual ReadOnlyCollection<TResult> VisitExpressionList(ReadOnlyCollection<Expression> expressions)
    {
      var results = new List<TResult>();
      for (int i = 0, n = expressions.Count; i < n; i++) {
        TResult p = Visit(expressions[i]);
        results.Add(p);
      }
      return results.AsReadOnly();
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
    protected ExpressionVisitor()
      : this(false)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="isCaching">Indicates whether visit result 
    /// should be cached and resolved by cache when possible.</param>
    protected ExpressionVisitor(bool isCaching)
    {
      if (isCaching)
        cache = new Dictionary<Expression, TResult>();
    }
  }
}