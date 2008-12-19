// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  public class RseQueryTranslator : ExpressionVisitor
  {
    private readonly QueryProvider provider;
    private readonly Expression query;
    private readonly DomainModel model;
    private readonly FieldAccessTranslator fieldAccessTranslator;
    private readonly FieldAccessFlattener fieldAccessFlattener;
    
    public ResultExpression Translate()
    {
      return (ResultExpression) Visit(query);
    }

    protected bool IsRoot(Expression expression)
    {
      return query==expression;
    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value == null)
        return c;
      var rootPoint = c.Value as IQueryable;
      if (rootPoint != null) {
        var type = provider.Model.Types[rootPoint.ElementType];
        var index = type.Indexes.PrimaryIndex;
        return new ResultExpression(
          c.Type,
          IndexProvider.Get(index).Result,
          null,
          true);
      }
      return base.VisitConstant(c);
    }

    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Method.DeclaringType==typeof (Queryable) || mc.Method.DeclaringType==typeof (Enumerable)) {
        switch (mc.Method.Name) {
        // TODO: => Core.Wellknown
        case WellKnown.Queryable.Where:
          return VisitWhere(mc.Arguments[0], mc.Arguments[1].StripQuotes());
        case WellKnown.Queryable.Select:
          return VisitSelect(mc.Type, mc.Arguments[0], mc.Arguments[1].StripQuotes());
        case WellKnown.Queryable.SelectMany:
          if (mc.Arguments.Count==2) {
            return VisitSelectMany(
              mc.Type, mc.Arguments[0],
              mc.Arguments[1].StripQuotes(),
              null);
          }
          if (mc.Arguments.Count==3) {
            return VisitSelectMany(
              mc.Type, mc.Arguments[0],
              mc.Arguments[1].StripQuotes(),
              mc.Arguments[2].StripQuotes());
          }
          break;
        case WellKnown.Queryable.Join:
          return VisitJoin(
            mc.Type, mc.Arguments[0], mc.Arguments[1],
            mc.Arguments[2].StripQuotes(),
            mc.Arguments[3].StripQuotes(),
            mc.Arguments[4].StripQuotes());
        case WellKnown.Queryable.OrderBy:
          return VisitOrderBy(mc.Type, mc.Arguments[0], (mc.Arguments[1].StripQuotes()), Direction.Positive);
        case WellKnown.Queryable.OrderByDescending:
          return VisitOrderBy(mc.Type, mc.Arguments[0], (mc.Arguments[1].StripQuotes()), Direction.Negative);
        case WellKnown.Queryable.ThenBy:
          return VisitThenBy(mc.Arguments[0], (mc.Arguments[1].StripQuotes()), Direction.Positive);
        case WellKnown.Queryable.ThenByDescending:
          return VisitThenBy(mc.Arguments[0], (mc.Arguments[1].StripQuotes()), Direction.Negative);
        case WellKnown.Queryable.GroupBy:
          if (mc.Arguments.Count==2) {
            return VisitGroupBy(
              mc.Arguments[0],
              (mc.Arguments[1].StripQuotes()),
              null,
              null
              );
          }
          if (mc.Arguments.Count==3) {
            LambdaExpression lambda1 = (mc.Arguments[1].StripQuotes());
            LambdaExpression lambda2 = (mc.Arguments[2].StripQuotes());
            if (lambda2.Parameters.Count==1) {
              // second lambda is element selector
              return VisitGroupBy(mc.Arguments[0], lambda1, lambda2, null);
            }
            if (lambda2.Parameters.Count==2) {
              // second lambda is result selector
              return VisitGroupBy(mc.Arguments[0], lambda1, null, lambda2);
            }
          }
          else if (mc.Arguments.Count==4) {
            return VisitGroupBy(
              mc.Arguments[0],
              (mc.Arguments[1].StripQuotes()),
              (mc.Arguments[2].StripQuotes()),
              (mc.Arguments[3].StripQuotes())
              );
          }
          break;
        case WellKnown.Queryable.Count:
        case WellKnown.Queryable.Min:
        case WellKnown.Queryable.Max:
        case WellKnown.Queryable.Sum:
        case WellKnown.Queryable.Average:
          if (mc.Arguments.Count==1) {
            return VisitAggregate(mc.Arguments[0], mc.Method, null, IsRoot(mc));
          }
          if (mc.Arguments.Count==2) {
            LambdaExpression selector = (mc.Arguments[1].StripQuotes());
            return VisitAggregate(mc.Arguments[0], mc.Method, selector, IsRoot(mc));
          }
          break;
        case WellKnown.Queryable.Distinct:
          if (mc.Arguments.Count==1) {
            return VisitDistinct(mc.Arguments[0]);
          }
          break;
        case WellKnown.Queryable.Skip:
          if (mc.Arguments.Count==2) {
            return VisitSkip(mc.Arguments[0], mc.Arguments[1]);
          }
          break;
        case WellKnown.Queryable.Take:
          if (mc.Arguments.Count==2) {
            return VisitTake(mc.Arguments[0], mc.Arguments[1]);
          }
          break;
        case WellKnown.Queryable.First:
        case WellKnown.Queryable.FirstOrDefault:
        case WellKnown.Queryable.Single:
        case WellKnown.Queryable.SingleOrDefault:
          if (mc.Arguments.Count==1) {
            return VisitFirst(mc.Arguments[0], null, mc.Method, IsRoot(mc));
          }
          if (mc.Arguments.Count==2) {
            LambdaExpression predicate = (mc.Arguments[1].StripQuotes());
            return VisitFirst(mc.Arguments[0], predicate, mc.Method, IsRoot(mc));
          }
          break;
        case WellKnown.Queryable.Any:
          if (mc.Arguments.Count==1) {
            return VisitAnyAll(mc.Arguments[0], mc.Method, null, IsRoot(mc));
          }
          if (mc.Arguments.Count==2) {
            LambdaExpression predicate = (mc.Arguments[1].StripQuotes());
            return VisitAnyAll(mc.Arguments[0], mc.Method, predicate, IsRoot(mc));
          }
          break;
        case WellKnown.Queryable.All:
          if (mc.Arguments.Count==2) {
            var predicate = (LambdaExpression) (mc.Arguments[1]);
            return VisitAnyAll(mc.Arguments[0], mc.Method, predicate, IsRoot(mc));
          }
          break;
        case WellKnown.Queryable.Contains:
          if (mc.Arguments.Count==2) {
            return VisitContains(mc.Arguments[0], mc.Arguments[1], IsRoot(mc));
          }
          break;
        }
      }
      return base.VisitMethodCall(mc);
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
      ResultExpression result = predicate!=null ? 
        (ResultExpression) VisitWhere(source, predicate) : 
        (ResultExpression) Visit(source);
      RecordSet recordSet = null;
      switch (method.Name) {
      case WellKnown.Queryable.First:
      case WellKnown.Queryable.FirstOrDefault:
        recordSet = result.RecordSet.Take(1);
        break;
      case WellKnown.Queryable.Single:
      case WellKnown.Queryable.SingleOrDefault:
        recordSet = result.RecordSet.Take(2);
        break;
      }
      var enumerableType = typeof(Enumerable);
      MethodInfo enumerableMethod = enumerableType
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(m => m.Name == method.Name && m.GetParameters().Length == 1)
        .MakeGenericMethod(method.ReturnType);
      MethodInfo castMethod = enumerableType.GetMethod("Cast").MakeGenericMethod(method.ReturnType);
      Expression<Func<RecordSet,object>> materializer = set => set.ToEntities(method.ReturnType);
      var rs = materializer.Parameters[0];
      var body = Expression.Convert(Expression.Call(null, enumerableMethod, Expression.Call(null, castMethod, materializer.Body)), typeof(object));
      var le = Expression.Lambda(body, rs);
      var shaper = (Func<RecordSet, object>) le.Compile();
      return new ResultExpression(method.ReturnType, recordSet, shaper, false);
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
      ResultExpression result;
      int aggregateColumn = 0;
      if (method.Name == WellKnown.Queryable.Count) {
        shaper = set => (int)(set.First().GetValue<long>(0));
        if (argument != null)
          result = (ResultExpression) VisitWhere(source, argument);
        else
          result = (ResultExpression) Visit(source);
      }
      else {
        result = (ResultExpression)Visit(source);
        if (argument==null) 
          throw new NotSupportedException();

//        var column = argument.Body as FieldAccessExpression;
//        if (column==null)
//          throw new NotSupportedException();
//        aggregateColumn = column.Field.MappingInfo.Offset;
        shaper = set => set.First().GetValueOrDefault(0);
        switch (method.Name) {
        case WellKnown.Queryable.Min:
          name = "$Min";
          type = AggregateType.Min;
          break;
        case WellKnown.Queryable.Max:
          name = "$Max";
          type = AggregateType.Max;
          break;
        case WellKnown.Queryable.Sum:
          name = "$Sum";
          type = AggregateType.Sum;
          break;
        case WellKnown.Queryable.Average:
          name = "$Avg";
          type = AggregateType.Avg;
          break;
        }
      }

      var recordSet = result.RecordSet.Aggregate(null, new AggregateColumnDescriptor(name, aggregateColumn, type));
      return new ResultExpression(result.Type, recordSet, shaper, false);
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
      var source = (ResultExpression)Visit(expression);
      source = fieldAccessFlattener.FlattenFieldAccess(source, lambdaExpression);
      var predicate = fieldAccessTranslator.Translate(source, lambdaExpression);
      var recordSet = source.RecordSet.Filter((Expression<Func<Tuple, bool>>)predicate);
      return new ResultExpression(expression.Type, recordSet, null, true);

    }


    // Constructor

    public RseQueryTranslator(QueryProvider provider, Expression query)
    {
      model = provider.Model;
      this.provider = provider;
      this.query = query;
      fieldAccessTranslator = new FieldAccessTranslator(model, query);
      fieldAccessFlattener = new FieldAccessFlattener(model, query);
    }
  }
}