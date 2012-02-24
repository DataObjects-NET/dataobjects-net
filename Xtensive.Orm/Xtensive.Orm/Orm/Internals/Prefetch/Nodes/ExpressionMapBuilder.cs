// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Resources;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal sealed class ExpressionMapBuilder : ExpressionVisitor
  {
    private readonly ExpressionMap result = new ExpressionMap();
    private ParameterExpression currentParameter;

    public static ExpressionMap Build(Expression expression)
    {
      var builder = new ExpressionMapBuilder();
      builder.Visit(expression);
      return builder.result;
    }

    protected override Expression Visit(Expression e)
    {
      ValidateExpressionType(e);
      base.Visit(e);
      return e;
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      ValidateParameter(p);
      return p;
    }

    protected override Expression VisitLambda(LambdaExpression l)
    {
      var oldParameter = currentParameter;
      currentParameter = l.Parameters.First();
      Visit(l.Body);
      currentParameter = oldParameter;
      return l;
    }

    protected override Expression VisitMethodCall(MethodCallExpression call)
    {
      // Unpack nested "target.Prefetch(lambda)" call
      // We will directly map lambda to target to simplify work for NodeBuilder.

      var subprefetches = new List<LambdaExpression>();

      Expression source = call;

      do {
        // Collapse sequental Prefetch() calls.
        // All lambdas will be associated with single parent.
        call = (MethodCallExpression) source;
        ValidateMethodCall(call);
        source = call.Arguments[0];
        var lambda = call.Arguments[1].StripQuotes();
        subprefetches.Add(lambda);
      }
      while (source.NodeType==ExpressionType.Call);

      foreach (var item in subprefetches)
        result.RegisterChild(source, item);

      Visit(source);

      foreach (var item in subprefetches)
        VisitLambda(item);

      return call;
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      ValidateMemberAccess(m);
      result.RegisterChild(m.Expression, m);
      Visit(m.Expression);
      return m;
    }

    private static void ValidateExpressionType(Expression e)
    {
      if (e==null)
        return;

      e = e.StripCasts();

      var isSupported = e.NodeType.In(
        ExpressionType.MemberAccess, ExpressionType.Call, ExpressionType.New,
        ExpressionType.Lambda, ExpressionType.Parameter);

      if (!isSupported)
        throw new NotSupportedException(string.Format(
          Strings.ExOnlyPropertAccessPrefetchOrAnonymousTypeSupportedButFoundX, e));
    }

    private void ValidateParameter(ParameterExpression p)
    {
      if (currentParameter!=p)
        throw new NotSupportedException("Outer parameter should not be accessed from nested Prefetch() call");
    }

    private static void ValidateMemberAccess(MemberExpression memberExpression)
    {
      var isInstanceProperty = memberExpression.Expression!=null
        && (memberExpression.Member is PropertyInfo);

      if (!isInstanceProperty)
        throw new NotSupportedException(string.Format("Only instance properties are supported"));
    }

    private static void ValidateMethodCall(MethodCallExpression e)
    {
      var isPrefetchMethod = e.Method.DeclaringType==typeof (PrefetchExtensions)
        && e.Method.Name=="Prefetch"
        && e.Method.GetParameters().Length==2;

      if (!isPrefetchMethod)
        throw new NotSupportedException(
          string.Format(Strings.ExOnlyPrefetchMethodSupportedButFoundX, e.ToString(true)));
    }

    // Constructors

    private ExpressionMapBuilder()
    {
    }
  }
}