// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Storage.Rse.Compilation.Expressions;
using Xtensive.Storage.Rse.Compilation.Expressions.Visitors;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation.Linq
{
  public sealed class RseQueryCompiler : ExpressionVisitor
  {
    private readonly QueryProvider provider;

    public Expression Translate(Expression expression)
    {
      return Visit(expression);
    }

     protected override Expression VisitMethodCall(MethodCallExpression m)
    {
//      if (m.Method.DeclaringType == typeof(Queryable) || m.Method.DeclaringType == typeof(Enumerable))
//      {
//        switch (m.Method.Name) {
//          case "Where":
//            VisitWhere(m.Arguments[0], (LambdaExpression)m.Arguments[1].StripQuotes());
//            break;
//          case "Select":
//            VisitSelect(m.Type, m.Arguments[0], (LambdaExpression)m.Arguments[1].StripQuotes());
//            break;
//          case "SelectMany":
//            if (m.Arguments.Count == 2) {
//              VisitSelectMany(
//                m.Type, m.Arguments[0],
//                (LambdaExpression)m.Arguments[1].StripQuotes(),
//                null);
//            }
//            else if (m.Arguments.Count == 3) {
//              VisitSelectMany(
//                m.Type, m.Arguments[0],
//                (LambdaExpression)m.Arguments[1].StripQuotes(),
//                (LambdaExpression)m.Arguments[2].StripQuotes());
//            }
//            break;
//          case "Join":
//            VisitJoin(
//              m.Type, m.Arguments[0], m.Arguments[1],
//              (LambdaExpression)m.Arguments[2].StripQuotes(),
//              (LambdaExpression)m.Arguments[3].StripQuotes(),
//              (LambdaExpression)m.Arguments[4].StripQuotes());
//            break;
//          case "Count":
//          case "Min":
//          case "Max":
//          case "Sum":
//          case "Average":
//          if (m.Arguments.Count==1)
//            VisitAggregate(m.Arguments[0], m.Method, null, m==root);
//          else if (m.Arguments.Count==2) {
//            var selector = (LambdaExpression) m.Arguments[1].StripQuotes();
//            VisitAggregate(m.Arguments[0], m.Method, selector, m==root);
//          }
//          break;
//          case "First":
//          case "FirstOrDefault":
//          case "Single":
//          case "SingleOrDefault":
//          if (m.Arguments.Count==1)
//            VisitFirst(m.Arguments[0], null, m.Method, m==root);
//          else if (m.Arguments.Count==2) {
//            var predicate = (LambdaExpression) m.Arguments[1].StripQuotes();
//            VisitFirst(m.Arguments[0], predicate, m.Method, m==root);
//          }
//          break;
//        }
//        return m;
//      }
      return base.VisitMethodCall(m);
    }

    protected override Expression VisitUnknown(Expression expression)
    {
      var extendedExpression = (ExtendedExpression)expression;
      switch (extendedExpression.NodeType) {
        case ExtendedExpressionType.FieldAccess:
          return VisitFieldAccess((ColumnAccessExpression)extendedExpression);
        case ExtendedExpressionType.ParameterAccess:
          return expression;
        case ExtendedExpressionType.IndexAccess:
          return VisitIndexAccess((IndexAccessExpression)extendedExpression);
        case ExtendedExpressionType.Range:
          return VisitRange((RangeExpression)extendedExpression);
        case ExtendedExpressionType.Seek:
          return VisitSeek((SeekExpression)extendedExpression);
      }
      throw new ArgumentOutOfRangeException();
    }

    private Expression VisitFieldAccess(ColumnAccessExpression expression)
    {
      throw new NotImplementedException();
//      var method = expression.Type == typeof(object) ? nonGenericAccessor : genericAccessor.MakeGenericMethod(expression.Type);
//      return Expression.Call(parameter, method, Expression.Constant(expression.ColumnIndex));
    }

    private Expression VisitIndexAccess(IndexAccessExpression expression)
    {
      throw new NotImplementedException();
//      result = IndexProvider.Get(expression.Index).Result;
//      return expression;
    }

    private Expression VisitRange(RangeExpression expression)
    {
      throw new NotImplementedException();
    }

    private Expression VisitSeek(SeekExpression expression)
    {
      throw new NotImplementedException();
    }


    // Constructor

    public RseQueryCompiler(QueryProvider provider)
    {
      this.provider = provider;
    }
  }
}