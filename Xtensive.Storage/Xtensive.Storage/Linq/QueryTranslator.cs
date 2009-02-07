// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Linq
{
  internal class QueryTranslator : ExpressionVisitor
  {
    private const string AliasPrefix = "alias";

    private int aliasSuffix = 0;
    private readonly QueryProviderBase provider;
    private readonly Expression query;
    private readonly DomainModel model;
    private readonly MemberAccessReplacer memberAccessReplacer;
    private readonly MemberAccessBasedJoiner memberAccessBasedJoiner;
    private readonly ProjectionBuilder projectionBuilder;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private readonly ColumnProjector columnProjector;
    private readonly Dictionary<ParameterExpression, ResultExpression> map;

    public Expression Query
    {
      get { return query; }
    }

    public DomainModel Model
    {
      get { return model; }
    }

    public ExpressionEvaluator Evaluator
    {
      get { return evaluator; }
    }

    public ParameterExtractor ParameterExtractor
    {
      get { return parameterExtractor; }
    }

    public MemberAccessReplacer MemberAccessReplacer
    {
      get { return memberAccessReplacer; }
    }

    public MemberAccessBasedJoiner MemberAccessBasedJoiner
    {
      get { return memberAccessBasedJoiner; }
    }

    public ResultExpression GetProjection(ParameterExpression pe)
    {
      return map[pe];
    }

    public void SetProjection(ParameterExpression pe, ResultExpression value)
    {
      map[pe] = value;
    }

    public ResultExpression Translate()
    {
      return (ResultExpression) Visit(query);
    }

    public Dictionary<string, Segment<int>> BuildFieldMapping(TypeInfo type, int offset)
    {
      var fieldMapping = new Dictionary<string, Segment<int>>();
      foreach (var field in type.Fields) {
        fieldMapping.Add(field.Name, new Segment<int>(offset + field.MappingInfo.Offset, field.MappingInfo.Length)); 
        if (field.IsEntity)
          fieldMapping.Add(field.Name + ".Key", new Segment<int>(offset + field.MappingInfo.Offset, field.MappingInfo.Length));
      }
      var keySegment = new Segment<int>(offset, type.Hierarchy.KeyFields.Sum(pair => pair.Key.MappingInfo.Length));
      fieldMapping.Add("Key", keySegment);

      return fieldMapping;
    }

    protected bool IsRoot(Expression expression)
    {
      return query==expression;
    }

    public string GetNextAlias()
    {
      return AliasPrefix + aliasSuffix++;
    }


    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value == null)
        return c;
      var rootPoint = c.Value as IQueryable;
      if (rootPoint != null) {
        var type = model.Types[rootPoint.ElementType];
        var index = type.Indexes.PrimaryIndex;

        var fieldMapping = BuildFieldMapping(type, 0);
        var mapping = new ResultMapping(fieldMapping, new Dictionary<string, ResultMapping>());
        var recordSet = IndexProvider.Get(index).Result;
        Expression<Func<RecordSet, object>> projector = rs => rs.Parse().Select(r => r.DefaultKey.Resolve());
        Expression<Func<Record, Entity>> ipt = r => r.DefaultKey.Resolve();
        LambdaExpression itemProjector = Expression.Lambda(Expression.Convert(ipt.Body, rootPoint.ElementType), ipt.Parameters[0]);
        
        return new ResultExpression(
          c.Type,
          recordSet,
          mapping,
          projector, 
          itemProjector);
      }
      return base.VisitConstant(c);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Method.DeclaringType==typeof (Queryable) || mc.Method.DeclaringType==typeof (Enumerable)) {
        switch (mc.Method.Name) {
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
          return VisitOrderBy(mc.Arguments[0], (mc.Arguments[1].StripQuotes()), Direction.Positive);
        case WellKnown.Queryable.OrderByDescending:
          return VisitOrderBy(mc.Arguments[0], (mc.Arguments[1].StripQuotes()), Direction.Negative);
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
        case WellKnown.Queryable.LongCount:
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
        default:
          throw new NotSupportedException();
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
      return new ResultExpression(method.ReturnType, recordSet, result.Mapping, (Expression<Func<RecordSet, object>>) le, null);
    }

    private Expression VisitTake(Expression source, Expression take)
    {
      var projection = (ResultExpression)Visit(source);
      var parameter = parameterExtractor.ExtractParameter<int>(take);
      var rs = projection.RecordSet.Take(parameter, true);
      return new ResultExpression(projection.Type, rs, projection.Mapping, projection.Projector, projection.ItemProjector);
    }

    private Expression VisitSkip(Expression source, Expression skip)
    {
      var projection = (ResultExpression)Visit(source);
      var parameter = parameterExtractor.ExtractParameter<int>(skip);
      var rs = projection.RecordSet.Skip(parameter, true);
      return new ResultExpression(projection.Type, rs, projection.Mapping, projection.Projector, projection.ItemProjector);
    }

    private Expression VisitDistinct(Expression expression)
    {
      var result = (ResultExpression)Visit(expression);
      var rs = result.RecordSet.Distinct();
      return new ResultExpression(result.Type, rs, result.Mapping, result.Projector, result.ItemProjector);
    }

    private Expression VisitAggregate(Expression source, MethodInfo method, LambdaExpression argument, bool isRoot)
    {
      if (!isRoot)
        throw new NotImplementedException();
      AggregateType type = AggregateType.Count;
      Expression<Func<RecordSet, object>> shaper;
      ResultExpression result;
      int aggregateColumn = 0;
      if (method.Name == WellKnown.Queryable.Count || method.Name == WellKnown.Queryable.LongCount) {
        if (method.ReturnType == typeof (int))
          shaper = set => (int)(set.First().GetValue<long>(0));
        else
          shaper = set => (set.First().GetValue<long>(0));
        if (argument != null)
          result = (ResultExpression) VisitWhere(source, argument);
        else
          result = (ResultExpression) Visit(source);
      }
      else {
        result = (ResultExpression)Visit(source);
        var columnList = new List<int>();
        if (argument == null) {
          if (result.Mapping.Segment.Length > 1 || result.ItemProjector.Body.Type != result.RecordSet.Header.Columns[0].Type)
            throw new NotSupportedException();
          columnList.Add(0);
        }
        else {
          SetProjection(argument.Parameters[0], result);
          columnList = columnProjector.GetColumns(argument).ToList();
          result = GetProjection(argument.Parameters[0]);
        }
        
        if (columnList.Count != 1)
          throw new NotSupportedException();
        aggregateColumn = columnList[0];
        shaper = set => set.First().GetValueOrDefault(0);
        switch (method.Name) {
        case WellKnown.Queryable.Min:
          type = AggregateType.Min;
          break;
        case WellKnown.Queryable.Max:
          type = AggregateType.Max;
          break;
        case WellKnown.Queryable.Sum:
          type = AggregateType.Sum;
          break;
        case WellKnown.Queryable.Average:
          type = AggregateType.Avg;
          break;
        }
      }

      var recordSet = result.RecordSet.Aggregate(null, new AggregateColumnDescriptor(GetNextAlias(), aggregateColumn, type));
      return new ResultExpression(result.Type, recordSet, null, shaper, null);
    }

    private Expression VisitGroupBy(Expression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
    {
      throw new NotImplementedException();
    }

    private Expression VisitThenBy(Expression expression, LambdaExpression le, Direction direction)
    {
      SetProjection(le.Parameters[0], (ResultExpression) Visit(expression));
      MemberAccessBasedJoiner.Process(le.Body);
      var orderItems = columnProjector.GetColumns(le)
        .Distinct()
        .Select(ci => new KeyValuePair<int, Direction>(ci, direction));
      var result = GetProjection(le.Parameters[0]);
      var dc = ((SortProvider)result.RecordSet.Provider).Order;
      foreach (var item in orderItems) {
        if (!dc.ContainsKey(item.Key))
          dc.Add(item);
      }
      return result;
    }

    private Expression VisitOrderBy(Expression expression, LambdaExpression le, Direction direction)
    {
      SetProjection(le.Parameters[0], (ResultExpression)Visit(expression));
      MemberAccessBasedJoiner.Process(le.Body);
      var orderItems = columnProjector.GetColumns(le)
        .Distinct()
        .Select(ci => new KeyValuePair<int, Direction>(ci, direction));
      var dc = new DirectionCollection<int>(orderItems);
      var result = GetProjection(le.Parameters[0]);
      var rs = result.RecordSet.OrderBy(dc);
      return new ResultExpression(result.Type, rs, result.Mapping, result.Projector, result.ItemProjector);
    }

    private Expression VisitJoin(Type resultType, Expression outerSource, Expression innerSource, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression resultSelector)
    {
      var outerParameter = outerKey.Parameters[0];
      var innerParameter = innerKey.Parameters[0];
      SetProjection(outerParameter, (ResultExpression)Visit(outerSource));
      SetProjection(innerParameter, (ResultExpression)Visit(innerSource));
      memberAccessBasedJoiner.Process(outerKey);
      memberAccessBasedJoiner.Process(innerKey);

      var pairsQuery = 
              from o in columnProjector.GetColumns(outerKey).Select((column, index) => new {column, index})
              join i in columnProjector.GetColumns(innerKey).Select((column, index) => new {column, index}) on o.index equals i.index
              select new Pair<int>(o.column, i.column);
      var keyPairs = pairsQuery.ToArray();

      var outer = GetProjection(outerParameter);
      var inner = GetProjection(innerParameter);

      var innerRecordSet = inner.RecordSet.Alias(GetNextAlias());
      var recordSet = outer.RecordSet.Join(innerRecordSet, keyPairs.ToArray());
      var outerLength = outer.RecordSet.Header.Columns.Count;
      outer = new ResultExpression(outer.Type, recordSet, outer.Mapping, outer.Projector, outer.ItemProjector);
      inner = new ResultExpression(inner.Type, recordSet, inner.Mapping.ShiftOffset(outerLength), inner.Projector, inner.ItemProjector);
      SetProjection(resultSelector.Parameters[0], outer);
      SetProjection(resultSelector.Parameters[1], inner);

      var result = projectionBuilder.Build(resultSelector);
      return result;
    }

    private Expression VisitSelectMany(Type resultType, Expression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
    {
      throw new NotImplementedException();
    }

    private Expression VisitSelect(Type resultType, Expression expression, LambdaExpression le)
    {
      SetProjection(le.Parameters[0], (ResultExpression)Visit(expression));
      var result = projectionBuilder.Build(le);
      return result;
    }

    private Expression VisitWhere(Expression expression, LambdaExpression le)
    {
      var parameter = le.Parameters[0];
      SetProjection(parameter, (ResultExpression) Visit(expression));
      memberAccessBasedJoiner.Process(le);
      var predicate = memberAccessReplacer.ProcessPredicate(le);
      var source = GetProjection(parameter);
      var recordSet = source.RecordSet.Filter((Expression<Func<Tuple, bool>>)predicate);
      return new ResultExpression(expression.Type, recordSet, source.Mapping, source.Projector, source.ItemProjector);
    }


    // Constructor

    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public QueryTranslator(QueryProviderBase provider, Expression query)
    {
      var domain = Domain.Current;
      if (domain==null)
        throw new InvalidOperationException(Strings.ExNoCurrentSession);
      model = domain.Model;
      this.provider = provider;
      this.query = query;
      map = new Dictionary<ParameterExpression, ResultExpression>();
      evaluator = new ExpressionEvaluator(query);
      parameterExtractor = new ParameterExtractor(evaluator);
      memberAccessReplacer = new MemberAccessReplacer(this);
      memberAccessBasedJoiner = new MemberAccessBasedJoiner(this);
      projectionBuilder = new ProjectionBuilder(this);
      columnProjector = new ColumnProjector(this);
    }
  }
}