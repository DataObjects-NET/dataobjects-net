// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.27

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Compilation.Expressions;
using Xtensive.Storage.Rse.Compilation.Expressions.Visitors;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation.Linq
{
  public class RseQueryTranslator : ExpressionVisitor
  {
    private readonly QueryProvider provider;
    private RecordSet result;
    private Func<RecordSet, object> shaper;
    private ParameterExpression parameter;
    private readonly MethodInfo nonGenericAccessor;
    private readonly MethodInfo genericAccessor;

    public RecordSet Result
    {
      get { return result; }
    }

    public Func<RecordSet, object> Shaper
    {
      get { return shaper; }
    }


    public void Translate(Expression  expression)
    {
      Visit(expression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression m)
    {
      if (m.Method.DeclaringType == typeof(Queryable) || m.Method.DeclaringType == typeof(Enumerable))
      {
        switch (m.Method.Name) {
          case "Where":
            VisitWhere(m.Arguments[0], (LambdaExpression)m.Arguments[1].StripQuotes());
            break;
        }
        return m;
      }
      return base.VisitMethodCall(m);
    }

    private void VisitWhere(Expression expression, LambdaExpression lambdaExpression)
    {
      Visit(expression);
      var recordSet = result;
      parameter = Expression.Parameter(typeof(Tuple),"t");
      var body = Visit(lambdaExpression.Body);
      var predicate = Expression.Lambda(typeof(Func<Tuple, bool>), body, parameter);
      recordSet = recordSet.Filter((Expression<Func<Tuple, bool>>)predicate);
      result = recordSet;
    }

    protected override Expression VisitUnknown(Expression expression)
    {
      var extendedExpression = (ExtendedExpression)expression; 
      switch(extendedExpression.NodeType) {
        case ExtendedExpressionType.FieldAccess:
          return VisitFieldAccess((ColumnAccessExpression)extendedExpression);
        case ExtendedExpressionType.ParameterAccess:
          break;
        case ExtendedExpressionType.IndexAccess:
          return VisitIndexAccess((IndexAccessExpression) extendedExpression);
        case ExtendedExpressionType.Range:
          break;
        case ExtendedExpressionType.Seek:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      return expression;
    }

    private Expression VisitFieldAccess(ColumnAccessExpression expression)
    {
      var method = expression.Type==typeof (object) ? nonGenericAccessor : genericAccessor.MakeGenericMethod(expression.Type);
      return Expression.Call(parameter, method, Expression.Constant(expression.ColumnIndex));
    }

    private Expression VisitIndexAccess(IndexAccessExpression expression)
    {
      result = IndexProvider.Get(expression.Index).Result;
      return expression;
    }


    // Constructor

    public RseQueryTranslator(QueryProvider provider)
    {
      this.provider = provider;
      foreach (var method in typeof(Tuple).GetMethods()) {
        if (method.Name == "GetValueOrDefault") {
          if (method.IsGenericMethod)
            genericAccessor = method;
          else
            nonGenericAccessor = method;
        }
      }
    }
  }
}