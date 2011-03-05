// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Reflection;
using Xtensive.Storage.Rse;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// Expression visitor that checks ability to evaluate expression to <see cref="ConstantExpression"/>.
  /// </summary>
  public sealed class ExpressionEvaluator : ExpressionVisitor
  {
    private readonly HashSet<Expression> candidates = new HashSet<Expression>();
    private bool couldBeEvaluated;


    /// <summary>
    /// Determines whether specified <paramref name="e"/> can be evaluated.
    /// </summary>
    /// <param name="e">The expression.</param>
    /// <returns>
    ///   <see langword="true" /> if <paramref name="e"/> can be evaluated; otherwise, <see langword="false" />.
    /// </returns>
    public bool CanBeEvaluated(Expression e)
    {
      return candidates.Contains(e);
    }

    /// <summary>
    /// Evaluates the specified <paramref name="e"/> into <see cref="ConstantExpression"/>.
    /// </summary>
    /// <param name="e">The expression.</param>
    public static ConstantExpression Evaluate(Expression e)
    {
      if (e==null)
        return null;
      if (e.NodeType==ExpressionType.Constant)
        return (ConstantExpression) e;
      Type type = e.Type;
      if (type.IsValueType)
        e = Expression.Convert(e, typeof (object));
      var lambda = Expression.Lambda<Func<object>>(e);
      var func = lambda.CachingCompile();
      return Expression.Constant(func(), type);
    }

    /// <inheritdoc/>
    protected override Expression Visit(Expression e)
    {
      if (e!=null) {
        bool saved = couldBeEvaluated;
        couldBeEvaluated = true;
        base.Visit(e);
        if (couldBeEvaluated)
          if (CanEvaluateExpression(e))
            candidates.Add(e);
          else
            couldBeEvaluated = false;
        couldBeEvaluated &= saved;
      }
      return e;
    }

    /// <inheritdoc/>
    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    // Private methods

    private static bool CanEvaluateExpression(Expression expression)
    {
      if (expression.Type==typeof (ApplyParameter))
        return false;
      var cex = expression as ConstantExpression;
      if (cex!=null) {
        var query = cex.Value as IQueryable;
        return query==null;
      }
      if (expression.NodeType==ExpressionType.MemberAccess) {
        var ma = (MemberExpression) expression;
        if (ma.Expression==null)
          return !typeof (IQueryable).IsAssignableFrom(ma.Type);
        if (ma.Expression.Type.IsNullable() && ma.Member.Name == "Value")
          return false;
        if (ma.Expression.NodeType==ExpressionType.Constant) {
          var rfi = ma.Member as FieldInfo;
          if (rfi!=null && (rfi.FieldType.IsGenericType && typeof (IQueryable).IsAssignableFrom(rfi.FieldType)))
            return false;
        }
      }
      var mc = expression as MethodCallExpression;
#pragma warning disable 612,618
      if (mc != null) {
        var methodInfo = mc.Method;
        if (methodInfo.DeclaringType == typeof (Enumerable) ||
            methodInfo.DeclaringType == typeof (Queryable) || 
            (methodInfo.DeclaringType == typeof (Query)) && methodInfo.IsGenericMethod)
          return false;
      }
#pragma warning restore 612,618
      if (expression.NodeType==ExpressionType.Convert && expression.Type==typeof (object))
        return true;
      return expression.NodeType!=ExpressionType.Parameter &&
        expression.NodeType!=ExpressionType.Lambda;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ExpressionEvaluator(Expression e)
    {
      couldBeEvaluated = true;
      Visit(e);
    }
  }
}