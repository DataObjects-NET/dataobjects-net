// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Compilation.Expressions;
using Xtensive.Storage.Rse.Compilation.Expressions.Visitors;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation.Linq
{
  public class RseQueryTranslator : ExpressionVisitor
  {
    private readonly QueryProvider provider;
    private static readonly MethodInfo nonGenericAccessor;
    private static readonly MethodInfo genericAccessor;
    private Expression root;
    private ParameterExpression parameter;

    public RseResultExpression Translate(Expression expression)
    {
      root = expression;
      return (RseResultExpression) Visit(expression);
    }

    protected bool IsRoot(Expression expression)
    {
      return root==expression;
    }

    protected override Expression VisitMethodCall(MethodCallExpression m)
    {
      if (m.Method.DeclaringType==typeof (Queryable) || m.Method.DeclaringType==typeof (Enumerable)) {
        switch (m.Method.Name) {
        case "Where":
          return VisitWhere(m.Arguments[0], m.Arguments[1].StripQuotes());
        case "Select":
          return VisitSelect(m.Type, m.Arguments[0], m.Arguments[1].StripQuotes());
        case "SelectMany":
          if (m.Arguments.Count==2) {
            return VisitSelectMany(
              m.Type, m.Arguments[0],
              m.Arguments[1].StripQuotes(),
              null);
          }
          if (m.Arguments.Count==3) {
            return VisitSelectMany(
              m.Type, m.Arguments[0],
              m.Arguments[1].StripQuotes(),
              m.Arguments[2].StripQuotes());
          }
          break;
        case "Join":
          return VisitJoin(
            m.Type, m.Arguments[0], m.Arguments[1],
            m.Arguments[2].StripQuotes(),
            m.Arguments[3].StripQuotes(),
            m.Arguments[4].StripQuotes());
        case "OrderBy":
          return VisitOrderBy(m.Type, m.Arguments[0], (m.Arguments[1].StripQuotes()), Direction.Positive);
        case "OrderByDescending":
          return VisitOrderBy(m.Type, m.Arguments[0], (m.Arguments[1].StripQuotes()), Direction.Negative);
        case "ThenBy":
          return VisitThenBy(m.Arguments[0], (m.Arguments[1].StripQuotes()), Direction.Positive);
        case "ThenByDescending":
          return VisitThenBy(m.Arguments[0], (m.Arguments[1].StripQuotes()), Direction.Negative);
        case "GroupBy":
          if (m.Arguments.Count==2) {
            return VisitGroupBy(
              m.Arguments[0],
              (m.Arguments[1].StripQuotes()),
              null,
              null
              );
          }
          if (m.Arguments.Count==3) {
            LambdaExpression lambda1 = (m.Arguments[1].StripQuotes());
            LambdaExpression lambda2 = (m.Arguments[2].StripQuotes());
            if (lambda2.Parameters.Count==1) {
              // second lambda is element selector
              return VisitGroupBy(m.Arguments[0], lambda1, lambda2, null);
            }
            if (lambda2.Parameters.Count==2) {
              // second lambda is result selector
              return VisitGroupBy(m.Arguments[0], lambda1, null, lambda2);
            }
          }
          else if (m.Arguments.Count==4) {
            return VisitGroupBy(
              m.Arguments[0],
              (m.Arguments[1].StripQuotes()),
              (m.Arguments[2].StripQuotes()),
              (m.Arguments[3].StripQuotes())
              );
          }
          break;
        case "Count":
        case "Min":
        case "Max":
        case "Sum":
        case "Average":
          if (m.Arguments.Count==1) {
            return VisitAggregate(m.Arguments[0], m.Method, null, IsRoot(m));
          }
          if (m.Arguments.Count==2) {
            LambdaExpression selector = (m.Arguments[1].StripQuotes());
            return VisitAggregate(m.Arguments[0], m.Method, selector, IsRoot(m));
          }
          break;
        case "Distinct":
          if (m.Arguments.Count==1) {
            return VisitDistinct(m.Arguments[0]);
          }
          break;
        case "Skip":
          if (m.Arguments.Count==2) {
            return VisitSkip(m.Arguments[0], m.Arguments[1]);
          }
          break;
        case "Take":
          if (m.Arguments.Count==2) {
            return VisitTake(m.Arguments[0], m.Arguments[1]);
          }
          break;
        case "First":
        case "FirstOrDefault":
        case "Single":
        case "SingleOrDefault":
          if (m.Arguments.Count==1) {
            return VisitFirst(m.Arguments[0], null, m.Method, IsRoot(m));
          }
          if (m.Arguments.Count==2) {
            LambdaExpression predicate = (m.Arguments[1].StripQuotes());
            return VisitFirst(m.Arguments[0], predicate, m.Method, IsRoot(m));
          }
          break;
        case "Any":
          if (m.Arguments.Count==1) {
            return VisitAnyAll(m.Arguments[0], m.Method, null, IsRoot(m));
          }
          if (m.Arguments.Count==2) {
            LambdaExpression predicate = (m.Arguments[1].StripQuotes());
            return VisitAnyAll(m.Arguments[0], m.Method, predicate, IsRoot(m));
          }
          break;
        case "All":
          if (m.Arguments.Count==2) {
            var predicate = (LambdaExpression) (m.Arguments[1]);
            return VisitAnyAll(m.Arguments[0], m.Method, predicate, IsRoot(m));
          }
          break;
        case "Contains":
          if (m.Arguments.Count==2) {
            return VisitContains(m.Arguments[0], m.Arguments[1], IsRoot(m));
          }
          break;
        }
      }
      return base.VisitMethodCall(m);
    }

    private Expression VisitContains(Expression source, Expression match, bool isRoot)
    {
      throw new NotImplementedException();
    }

    private Expression VisitAnyAll(Expression source, MethodInfo method, LambdaExpression predicate, bool isRoot)
    {
      throw new NotImplementedException();
    }

    private Expression VisitFirst(Expression source, LambdaExpression predicate, MethodInfo method, bool isRoot)
    {
      if (!isRoot)
        throw new NotImplementedException();
      RseResultExpression result = predicate!=null ? 
        (RseResultExpression) VisitWhere(source, predicate) : 
        (RseResultExpression) Visit(source);
      RecordSet recordSet = null;
      switch (method.Name) {
        case "First":
        case "FirstOrDefault":
          recordSet = result.RecordSet.Take(1);
          break;
        case "Single":
        case "SingleOrDefault":
          recordSet = result.RecordSet.Take(2);
          break;
      }
      var enumerableType = typeof(Enumerable);
      MethodInfo enumerableMethod = enumerableType.GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == method.Name && m.GetParameters().Length == 1);
      enumerableMethod = enumerableMethod.MakeGenericMethod(method.ReturnType);
      MethodInfo castMethod = enumerableType.GetMethod("Cast").MakeGenericMethod(method.ReturnType);
      Func<RecordSet,object> shaper = delegate(RecordSet set) {
        IEnumerable enumerable = provider.EntityMaterializer(set, method.ReturnType);
        object cast = castMethod.Invoke(null, new[] { enumerable });
        object item = enumerableMethod.Invoke(null, new[] { cast });
        return item;
      };

      return new RseResultExpression(method.ReturnType, recordSet, shaper, false);
    }

    private Expression VisitTake(Expression source, Expression take)
    {
      throw new NotImplementedException();
    }

    private Expression VisitSkip(Expression source, Expression skip)
    {
      throw new NotImplementedException();
    }

    private Expression VisitDistinct(Expression expression)
    {
      throw new NotImplementedException();
    }

    private Expression VisitAggregate(Expression source, MethodInfo method, LambdaExpression argument, bool isRoot)
    {
       if (!isRoot)
        throw new NotImplementedException();
      string name = "$Count";
      AggregateType type = AggregateType.Count;
      Func<RecordSet, object> shaper;
      RseResultExpression result;
      int aggregateColumn = 0;
      if (method.Name == "Count") {
        shaper = set => (int)(set.First().GetValue<long>(0));
        if (argument != null)
          result = (RseResultExpression) VisitWhere(source, argument);
        else
          result = (RseResultExpression) Visit(source);
      }
      else {
        result = (RseResultExpression)Visit(source);
        if (argument==null) 
          throw new NotSupportedException();

        var column = argument.Body as ColumnAccessExpression;
        if (column==null)
          throw new NotSupportedException();
        aggregateColumn = column.ColumnIndex;
        shaper = set => set.First().GetValueOrDefault(0);
        switch (method.Name) {
        case "Min":
          name = "$Min";
          type = AggregateType.Min;
          break;
        case "Max":
          name = "$Max";
          type = AggregateType.Max;
          break;
        case "Sum":
          name = "$Sum";
          type = AggregateType.Sum;
          break;
        case "Average":
          name = "$Avg";
          type = AggregateType.Avg;
          break;
        }
      }

      var recordSet = result.RecordSet.Aggregate(null, new AggregateColumnDescriptor(name, aggregateColumn, type));
      return new RseResultExpression(result.Type, recordSet, shaper, false);
    }

    private Expression VisitGroupBy(Expression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
    {
      throw new NotImplementedException();
    }

    private Expression VisitThenBy(Expression expression, LambdaExpression lambdaExpression, Direction direaction)
    {
      throw new NotImplementedException();
    }

    private Expression VisitOrderBy(Type type, Expression expression, LambdaExpression lambdaExpression, Direction direction)
    {
      throw new NotImplementedException();
    }

    private Expression VisitJoin(Type resultType, Expression outerSource, Expression innerSource, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression resultSelector)
    {
      throw new NotImplementedException();
    }

    private Expression VisitSelectMany(Type resultType, Expression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
    {
      throw new NotImplementedException();
    }

    private Expression VisitSelect(Type type, Expression expression, LambdaExpression lambdaExpression)
    {
      throw new NotImplementedException();
    }

    private Expression VisitWhere(Expression expression, LambdaExpression lambdaExpression)
    {
      var source = (RseResultExpression)Visit(expression);
      parameter = Expression.Parameter(typeof(Tuple), "t");
      var body = Visit(lambdaExpression.Body);
      var predicate = Expression.Lambda(typeof(Func<Tuple, bool>), body, parameter);
      var recordSet = source.RecordSet.Filter((Expression<Func<Tuple, bool>>)predicate);
      return new RseResultExpression(expression.Type, recordSet, null, true);

    }

    protected override Expression VisitUnknown(Expression expression)
    {
      var extendedExpression = (ExtendedExpression) expression;
      switch (extendedExpression.NodeType) {
      case ExtendedExpressionType.FieldAccess:
        return VisitFieldAccess((ColumnAccessExpression) extendedExpression);
      case ExtendedExpressionType.ParameterAccess:
        return expression;
      case ExtendedExpressionType.IndexAccess:
        return VisitIndexAccess((IndexAccessExpression) extendedExpression);
      case ExtendedExpressionType.Range:
        return VisitRange((RangeExpression) extendedExpression);
      case ExtendedExpressionType.Seek:
        return VisitSeek((SeekExpression) extendedExpression);
      }
      throw new ArgumentOutOfRangeException();
    }

    private Expression VisitFieldAccess(ColumnAccessExpression expression)
    {
      var method = expression.Type == typeof(object) ? nonGenericAccessor : genericAccessor.MakeGenericMethod(expression.Type);
      return Expression.Call(parameter, method, Expression.Constant(expression.ColumnIndex));
    }

    private Expression VisitIndexAccess(IndexAccessExpression expression)
    {
      return new RseResultExpression(
        expression.Type,
        IndexProvider.Get(expression.Index).Result,
        null,
        true);
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

    public RseQueryTranslator(QueryProvider provider)
    {
      this.provider = provider;
    }

    static RseQueryTranslator()
    {
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