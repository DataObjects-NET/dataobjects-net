// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Linq.Rewriters;

namespace Xtensive.Storage.Linq
{
  internal sealed partial class Translator : QueryableVisitor
  {
    private readonly TranslatorContext context;

    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value==null)
        return c;
      var rootPoint = c.Value as IQueryable;
      if (rootPoint!=null)
        return ConstructQueryable(rootPoint);
      return base.VisitConstant(c);
    }

    protected override Expression VisitQueryableMethod(MethodCallExpression mc, QueryableMethodKind methodKind)
    {
      using (state.CreateScope()) {
        state.BuildingProjection = false;
        switch (methodKind) {
        case QueryableMethodKind.Cast:
          break;
        case QueryableMethodKind.AsEnumerable:
          break;
        case QueryableMethodKind.AsQueryable:
          break;
        case QueryableMethodKind.ToArray:
          break;
        case QueryableMethodKind.ToList:
          break;
        case QueryableMethodKind.Aggregate:
          break;
        case QueryableMethodKind.ElementAt:
          break;
        case QueryableMethodKind.ElementAtOrDefault:
          break;
        case QueryableMethodKind.Last:
          break;
        case QueryableMethodKind.LastOrDefault:
          break;
        case QueryableMethodKind.Except:
        case QueryableMethodKind.Intersect:
        case QueryableMethodKind.Concat:
        case QueryableMethodKind.Union:
          return VisitSetOperations(mc.Arguments[0], mc.Arguments[1], methodKind);
        case QueryableMethodKind.Reverse:
          break;
        case QueryableMethodKind.SequenceEqual:
          break;
        case QueryableMethodKind.DefaultIfEmpty:
          break;
        case QueryableMethodKind.SkipWhile:
          break;
        case QueryableMethodKind.TakeWhile:
          break;
        case QueryableMethodKind.All:
          if (mc.Arguments.Count==2)
            return VisitAll(mc.Arguments[0], mc.Arguments[1].StripQuotes(), context.IsRoot(mc));
          break;
        case QueryableMethodKind.OfType:
          return VisitOfType(mc.Arguments[0], mc.Method.GetGenericArguments()[0], mc.Arguments[0].Type.GetGenericArguments()[0]);
        case QueryableMethodKind.Any:
          if (mc.Arguments.Count==1)
            return VisitAny(mc.Arguments[0], null, context.IsRoot(mc));
          if (mc.Arguments.Count==2)
            return VisitAny(mc.Arguments[0], mc.Arguments[1].StripQuotes(), context.IsRoot(mc));
          break;
        case QueryableMethodKind.Contains:
          if (mc.Arguments.Count==2)
            return VisitContains(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc));
          break;
        case QueryableMethodKind.Distinct:
          if (mc.Arguments.Count==1)
            return VisitDistinct(mc.Arguments[0]);
          break;
        case QueryableMethodKind.First:
        case QueryableMethodKind.FirstOrDefault:
        case QueryableMethodKind.Single:
        case QueryableMethodKind.SingleOrDefault:
          if (mc.Arguments.Count==1)
            return VisitFirstSingle(mc.Arguments[0], null, mc.Method, context.IsRoot(mc));
          if (mc.Arguments.Count==2) {
            LambdaExpression predicate = (mc.Arguments[1].StripQuotes());
            return VisitFirstSingle(mc.Arguments[0], predicate, mc.Method, context.IsRoot(mc));
          }
          break;
        case QueryableMethodKind.GroupBy:
          if (mc.Arguments.Count==2) {
            return VisitGroupBy(
              mc.Method.ReturnType,
              mc.Arguments[0],
              mc.Arguments[1].StripQuotes(),
              null,
              null
              );
          }
          if (mc.Arguments.Count==3) {
            LambdaExpression lambda1 = mc.Arguments[1].StripQuotes();
            LambdaExpression lambda2 = mc.Arguments[2].StripQuotes();
            if (lambda2.Parameters.Count==1) {
              // second lambda is element selector
              return VisitGroupBy(
                mc.Method.ReturnType,
                mc.Arguments[0],
                lambda1,
                lambda2,
                null);
            }
            if (lambda2.Parameters.Count==2) {
              // second lambda is result selector
              return VisitGroupBy(
                mc.Method.ReturnType,
                mc.Arguments[0],
                lambda1,
                null,
                lambda2);
            }
          }
          else if (mc.Arguments.Count==4) {
            return VisitGroupBy(
              mc.Method.ReturnType,
              mc.Arguments[0],
              mc.Arguments[1].StripQuotes(),
              mc.Arguments[2].StripQuotes(),
              mc.Arguments[3].StripQuotes()
              );
          }
          break;
        case QueryableMethodKind.GroupJoin:
          return VisitGroupJoin(mc.Arguments[0], mc.Arguments[1],
            mc.Arguments[2].StripQuotes(),
            mc.Arguments[3].StripQuotes(),
            mc.Arguments[4].StripQuotes(),
            mc.Arguments.Count > 5 ? mc.Arguments[5] : null);
        case QueryableMethodKind.Join:
          return VisitJoin(mc.Arguments[0], mc.Arguments[1],
            mc.Arguments[2].StripQuotes(),
            mc.Arguments[3].StripQuotes(),
            mc.Arguments[4].StripQuotes(),
            false);
        case QueryableMethodKind.OrderBy:
          return VisitOrderBy(mc.Arguments[0], mc.Arguments[1].StripQuotes(), Direction.Positive);
        case QueryableMethodKind.OrderByDescending:
          return VisitOrderBy(mc.Arguments[0], mc.Arguments[1].StripQuotes(), Direction.Negative);
        case QueryableMethodKind.Select:
          return VisitSelect(mc.Arguments[0], mc.Arguments[1].StripQuotes());
        case QueryableMethodKind.SelectMany:
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
        case QueryableMethodKind.LongCount:
        case QueryableMethodKind.Count:
        case QueryableMethodKind.Max:
        case QueryableMethodKind.Min:
        case QueryableMethodKind.Sum:
        case QueryableMethodKind.Average:
          if (mc.Arguments.Count==1)
            return VisitAggregate(mc.Arguments[0], mc.Method, null, context.IsRoot(mc));
          if (mc.Arguments.Count==2)
            return VisitAggregate(mc.Arguments[0], mc.Method, mc.Arguments[1].StripQuotes(), context.IsRoot(mc));
          break;
        case QueryableMethodKind.Skip:
          if (mc.Arguments.Count==2)
            return VisitSkip(mc.Arguments[0], mc.Arguments[1]);
          break;
        case QueryableMethodKind.Take:
          if (mc.Arguments.Count==2)
            return VisitTake(mc.Arguments[0], mc.Arguments[1]);
          break;
        case QueryableMethodKind.ThenBy:
          return VisitThenBy(mc.Arguments[0], mc.Arguments[1].StripQuotes(), Direction.Positive);
        case QueryableMethodKind.ThenByDescending:
          return VisitThenBy(mc.Arguments[0], mc.Arguments[1].StripQuotes(), Direction.Negative);
        case QueryableMethodKind.Where:
          return VisitWhere(mc.Arguments[0], mc.Arguments[1].StripQuotes());
        default:
          throw new ArgumentOutOfRangeException("methodKind");
        }
      }
      throw new NotSupportedException();
    }

    private Expression VisitIncludeField(MethodCallExpression expression)
    {
      throw new NotImplementedException("VisitIncludeField not implemented");
    }

    private Expression VisitExcludeField(MethodCallExpression expression)
    {
      throw new NotImplementedException("VisitExcludeField not implemented");
    }

    private Expression VisitExpand(MethodCallExpression expression)
    {
      throw new NotImplementedException("VisitExpand not implemented");
    }

    /// <exception cref="NotSupportedException">OfType supports only 'Entity' conversion.</exception>
    private ProjectionExpression VisitOfType(Expression source, Type targetType, Type sourceType)
    {
      if (!sourceType.IsSubclassOf(typeof (Entity)))
        throw new NotSupportedException(Resources.Strings.ExOfTypeSupportsOnlyEntityConversion);

      var visitedSource = (ProjectionExpression) Visit(source);
      if (targetType==sourceType)
        return visitedSource;

      int offset = 0;
      var recordSet = visitedSource.ItemProjector.DataSource;

      if (targetType.IsSubclassOf(sourceType)) {
        var joinedIndex = context.Model.Types[targetType].Indexes.PrimaryIndex;
        var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
        offset = recordSet.Header.Columns.Count;
        var keySegment = visitedSource.ItemProjector.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
        var keyPairs = keySegment
          .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
          .ToArray();
        recordSet = recordSet.Join(joinedRs, JoinAlgorithm.Default, keyPairs);
      }
      var entityExpression = EntityExpression.Create(context.Model.Types[targetType], offset);
      var itemProjectorExpression = new ItemProjectorExpression(entityExpression, recordSet, context);
      return new ProjectionExpression(sourceType, itemProjectorExpression, visitedSource.TupleParameterBindings);
    }


    private Expression VisitContains(Expression source, Expression match, bool isRoot)
    {
      var p = Expression.Parameter(match.Type, "p");
      var le = FastExpression.Lambda(Expression.Equal(p, match), p);

      if (isRoot)
        return VisitRootExists(source, le, false);

      if (source.IsQuery())
        return VisitExists(source, le, false);

      throw new NotImplementedException();
    }

    private Expression VisitAll(Expression source, LambdaExpression predicate, bool isRoot)
    {
      predicate = FastExpression.Lambda(Expression.Not(predicate.Body), predicate.Parameters[0]);

      if (isRoot)
        return VisitRootExists(source, predicate, true);

      if (source.IsQuery())
        return VisitExists(source, predicate, true);

      throw new NotImplementedException();
    }

    private Expression VisitAny(Expression source, LambdaExpression predicate, bool isRoot)
    {
      if (isRoot)
        return VisitRootExists(source, predicate, false);

      if (source.IsQuery())
        return VisitExists(source, predicate, false);

      throw new NotImplementedException();
    }

    private Expression VisitFirstSingle(Expression source, LambdaExpression predicate, MethodInfo method, bool isRoot)
    {
      var markerType = MarkerType.None;
      var applySequenceType = ApplySequenceType.All;
      var projection = predicate!=null
        ? VisitWhere(source, predicate)
        : VisitSequence(source);
      RecordSet recordSet = null;
      switch (method.Name) {
      case Core.Reflection.WellKnown.Queryable.First:
        applySequenceType = ApplySequenceType.First;
        markerType = MarkerType.First;
        recordSet = projection.ItemProjector.DataSource.Take(1);
        break;
      case Core.Reflection.WellKnown.Queryable.FirstOrDefault:
        applySequenceType = ApplySequenceType.FirstOrDefault;
        markerType = MarkerType.First | MarkerType.Default;
        recordSet = projection.ItemProjector.DataSource.Take(1);
        break;
      case Core.Reflection.WellKnown.Queryable.Single:
        applySequenceType = ApplySequenceType.Single;
        markerType = MarkerType.Single;
        recordSet = projection.ItemProjector.DataSource.Take(2);
        break;
      case Core.Reflection.WellKnown.Queryable.SingleOrDefault:
        applySequenceType = ApplySequenceType.SingleOrDefault;
        markerType = MarkerType.Single | MarkerType.Default;
        recordSet = projection.ItemProjector.DataSource.Take(2);
        break;
      }
      var resultType = (ResultType) Enum.Parse(typeof (ResultType), method.Name);
      if (isRoot) {
        var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, recordSet, context);
        return new ProjectionExpression(method.ReturnType, itemProjector, projection.TupleParameterBindings, resultType);
      }

      var lambdaParameter = state.Parameters[0];
      var oldResult = context.Bindings[lambdaParameter];
      var applyParameter = context.GetApplyParameter(oldResult);

      int columnIndex = oldResult.ItemProjector.DataSource.Header.Length;
      var newRecordSet = oldResult.ItemProjector.DataSource.Apply(applyParameter, recordSet.Alias(context.GetNextAlias()), applySequenceType, JoinType.LeftOuter);
      var newItemProjector = projection.ItemProjector.Remap(newRecordSet, columnIndex);
      var newResult = new ProjectionExpression(oldResult.Type, newItemProjector, oldResult.TupleParameterBindings);
      context.Bindings.ReplaceBound(lambdaParameter, newResult);

      return new MarkerExpression(newItemProjector.Item, markerType);
    }

    private ProjectionExpression VisitTake(Expression source, Expression take)
    {
      var projection = VisitSequence(source);
      var parameter = context.ParameterExtractor.ExtractParameter<int>(take);
      var rs = projection.ItemProjector.DataSource.Take(parameter.CachingCompile());
      var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, rs, context);
      return new ProjectionExpression(projection.Type, itemProjector, projection.TupleParameterBindings);
    }

    private ProjectionExpression VisitSkip(Expression source, Expression skip)
    {
      var projection = VisitSequence(source);
      var parameter = context.ParameterExtractor.ExtractParameter<int>(skip);
      var rs = projection.ItemProjector.DataSource.Skip(parameter.CachingCompile());
      var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, rs, context);
      return new ProjectionExpression(projection.Type, itemProjector, projection.TupleParameterBindings);
    }

    private ProjectionExpression VisitDistinct(Expression expression)
    {
      var result = VisitSequence(expression);
      var itemProjector = result.ItemProjector.RemoveOwner();
      var columnIndexes = itemProjector
        .GetColumns(ColumnExtractionModes.KeepSegment)
        .ToArray();
      var rs = itemProjector.DataSource
        .Select(columnIndexes)
        .Distinct();
      itemProjector = itemProjector.Remap(rs, columnIndexes);
      return new ProjectionExpression(result.Type, itemProjector, result.TupleParameterBindings);
    }

    private Expression VisitAggregate(Expression source, MethodInfo method, LambdaExpression argument, bool isRoot)
    {
      int aggregateColumn;
      AggregateType aggregateType;
      ProjectionExpression innerProjection;

      switch (method.Name) {
      case Core.Reflection.WellKnown.Queryable.Count:
        aggregateType = AggregateType.Count;
        break;
      case Core.Reflection.WellKnown.Queryable.LongCount:
        aggregateType = AggregateType.Count;
        break;
      case Core.Reflection.WellKnown.Queryable.Min:
        aggregateType = AggregateType.Min;
        break;
      case Core.Reflection.WellKnown.Queryable.Max:
        aggregateType = AggregateType.Max;
        break;
      case Core.Reflection.WellKnown.Queryable.Sum:
        aggregateType = AggregateType.Sum;
        break;
      case Core.Reflection.WellKnown.Queryable.Average:
        aggregateType = AggregateType.Avg;
        break;
      default:
        throw new ArgumentOutOfRangeException();
      }

      if (aggregateType==AggregateType.Count) {
        aggregateColumn = 0;
        innerProjection = argument!=null
          ? VisitWhere(source, argument)
          : VisitSequence(source);
      }
      else {
        innerProjection = VisitSequence(source);
        List<int> columnList;

        if (argument==null) {
          if (!innerProjection.ItemProjector.IsPrimitive)
            throw new NotSupportedException("Aggregates for non primitive types are not supported.");
          // Default
          columnList = innerProjection.ItemProjector.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
        }
        else {
          using (context.Bindings.Add(argument.Parameters[0], innerProjection))
          using (state.CreateScope()) {
            state.CalculateExpressions = true;
            var result = (ItemProjectorExpression) VisitLambda(argument);
            // Default
            columnList = result.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
            innerProjection = context.Bindings[argument.Parameters[0]];
          }
        }

        if (columnList.Count!=1)
          throw new NotSupportedException("Aggregates for non primitive types are not supported.");
        aggregateColumn = columnList[0];
      }


      var aggregateColumnDescriptor = new AggregateColumnDescriptor(context.GetNextColumnAlias(), aggregateColumn, aggregateType);
      var dataSource = innerProjection.ItemProjector.DataSource
        .Aggregate(null, aggregateColumnDescriptor);
      var resultType = method.ReturnType;
      var columnType = dataSource.Header.TupleDescriptor[0];
      if (!isRoot) {
        // Optimization. Use grouping AggregateProvider.
        if (source is ParameterExpression) {
          var groupingParameter = (ParameterExpression) source;
          var groupingProjection = context.Bindings[groupingParameter];
          var groupingDataSource = groupingProjection.ItemProjector.DataSource;
          var groupingProvider = groupingDataSource.Provider as AggregateProvider;
          var oldApplyParameter = context.GetApplyParameter(groupingProjection.ItemProjector.DataSource);
          if (groupingProjection.ItemProjector.Item.IsGroupingExpression() && groupingProvider!=null) {
            var newRecordSet = new AggregateProvider(groupingProvider.Source, groupingProvider.GroupColumnIndexes, groupingProvider.AggregateColumns.Select(c => c.Descriptor).AddOne(aggregateColumnDescriptor).ToArray()).Result;
            var newItemProjector = groupingProjection.ItemProjector.Remap(newRecordSet, 0);
            groupingProjection = new ProjectionExpression(groupingProjection.Type, newItemProjector, groupingProjection.TupleParameterBindings, groupingProjection.ResultType);
            context.Bindings.ReplaceBound(groupingParameter, groupingProjection);
            var isSubqueryParameter = state.OuterParameters.Contains(groupingParameter);
            if (isSubqueryParameter) {
              var newApplyParameter = context.GetApplyParameter(newRecordSet);
              foreach (var innerParameter in state.Parameters) {
                var projectionExpression = context.Bindings[innerParameter];
                var newProjectionExpression = new ProjectionExpression(projectionExpression.Type, projectionExpression.ItemProjector.RewriteApplyParameter(oldApplyParameter, newApplyParameter), projectionExpression.TupleParameterBindings, projectionExpression.ResultType);
                context.Bindings.ReplaceBound(innerParameter, newProjectionExpression);
              }
            }
            if (resultType!=columnType && !resultType.IsNullable()) {
              var columnExpression = ColumnExpression.Create(columnType, newRecordSet.Header.Length - 1);
              if (isSubqueryParameter)
                columnExpression = (ColumnExpression) columnExpression.BindParameter(groupingParameter, new Dictionary<Expression, Expression>());
              return Expression.Convert(columnExpression, resultType);
            }
            else {
              var columnExpression = ColumnExpression.Create(resultType, newRecordSet.Header.Length - 1);
              if (isSubqueryParameter)
                columnExpression = (ColumnExpression) columnExpression.BindParameter(groupingParameter, new Dictionary<Expression, Expression>());
              return columnExpression;
            }
          }
        }
        return resultType!=columnType && !resultType.IsNullable()
          ? Expression.Convert(AddSubqueryColumn(columnType, dataSource), resultType)
          : AddSubqueryColumn(method.ReturnType, dataSource);
      }

      var projectorBody = resultType!=columnType && !resultType.IsNullable()
        ? Expression.Convert(ColumnExpression.Create(columnType, 0), resultType)
        : (Expression) ColumnExpression.Create(resultType, 0);

      var itemProjector = new ItemProjectorExpression(projectorBody, dataSource, context);
      return new ProjectionExpression(resultType, itemProjector, innerProjection.TupleParameterBindings, ResultType.First);
    }

    private ProjectionExpression VisitGroupBy(Type returnType, Expression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
    {
      var sequence = VisitSequence(source);

      ProjectionExpression keyProjection;
      using (context.Bindings.PermanentAdd(keySelector.Parameters[0], sequence)) {
        using (state.CreateScope()) {
          state.CalculateExpressions = true;
          var itemProjector = (ItemProjectorExpression) VisitLambda(keySelector);
          keyProjection = new ProjectionExpression(
            typeof (IQueryable<>).MakeGenericType(keySelector.Body.Type),
            itemProjector,
            sequence.TupleParameterBindings);
        }
      }

      var keyColumns = keyProjection.ItemProjector.GetColumns(ColumnExtractionModes.KeepSegment
        | ColumnExtractionModes.TreatEntityAsKey
          | ColumnExtractionModes.KeepTypeId)
        .ToArray();
      var keyDataSource = keyProjection.ItemProjector.DataSource.Aggregate(keyColumns);
      var remappedKeyItemProjector = keyProjection.ItemProjector.RemoveOwner().Remap(keyDataSource, keyColumns);

      var newItemProjector = new ItemProjectorExpression(remappedKeyItemProjector.Item, keyDataSource, context);

      keyProjection = new ProjectionExpression(keyProjection.Type, newItemProjector, sequence.TupleParameterBindings);

      ProjectionExpression subqueryProjection;
      var groupingParameter = Expression.Parameter(keyProjection.ItemProjector.Item.Type, "groupingParameter");

      var applyParameter = context.GetApplyParameter(keyProjection);
      using (context.Bindings.Add(groupingParameter, keyProjection))
      using (state.CreateScope()) {
        state.Parameters = state.Parameters.AddOne(groupingParameter).ToArray();
        var lambda = FastExpression.Lambda(Expression.Equal(groupingParameter, keySelector.Body), keySelector.Parameters);
        subqueryProjection = VisitWhere(VisitSequence(source), lambda);
      }

      var keyType = keySelector.Type.GetGenericArguments()[1];
      var elementType = elementSelector==null
        ? keySelector.Parameters[0].Type
        : elementSelector.Type.GetGenericArguments()[1];
      var groupingType = typeof (IGrouping<,>).MakeGenericType(keyType, elementType);

      Type realGroupingType =
        resultSelector!=null
          ? resultSelector.Parameters[1].Type
          : returnType.GetGenericArguments()[0];

      if (elementSelector!=null)
        subqueryProjection = VisitSelect(subqueryProjection, elementSelector);

      var selectManyInfo = new GroupingExpression.SelectManyGroupingInfo(sequence);
      var groupingExpression = new GroupingExpression(realGroupingType, groupingParameter, false, subqueryProjection, applyParameter, remappedKeyItemProjector.Item, new Segment<int>(0, keyColumns.Length), keySelector, selectManyInfo);
      var groupingItemProjector = new ItemProjectorExpression(groupingExpression, keyDataSource, context);
      returnType = resultSelector==null
        ? returnType
        : resultSelector.Parameters[1].Type;
      var resultProjection = new ProjectionExpression(returnType, groupingItemProjector, subqueryProjection.TupleParameterBindings);

      if (resultSelector!=null) {
        var keyProperty = groupingType.GetProperty(WellKnown.KeyFieldName);
        var convertedParameter = Expression.Convert(resultSelector.Parameters[1], groupingType);
        var keyAccess = Expression.MakeMemberAccess(convertedParameter, keyProperty);
        var rewrittenResultSelectorBody = ParameterRewriter.Rewrite(resultSelector.Body, resultSelector.Parameters[0], keyAccess);
        var selectLambda = FastExpression.Lambda(rewrittenResultSelectorBody, resultSelector.Parameters[1]);
        resultProjection = VisitSelect(resultProjection, selectLambda);
      }

      return resultProjection;
    }

    private Expression VisitOrderBy(Expression expression, LambdaExpression le, Direction direction)
    {
      using (context.Bindings.Add(le.Parameters[0], VisitSequence(expression)))
      using (state.CreateScope()) {
        state.CalculateExpressions = true;
        var orderByProjector = (ItemProjectorExpression) VisitLambda(le);
        var orderItems = orderByProjector
          .GetColumns(ColumnExtractionModes.TreatEntityAsKey | ColumnExtractionModes.Distinct)
          .Select(ci => new KeyValuePair<int, Direction>(ci, direction));
        var dc = new DirectionCollection<int>(orderItems);
        var result = context.Bindings[le.Parameters[0]];
        var dataSource = result.ItemProjector.DataSource.OrderBy(dc);
        var itemProjector = new ItemProjectorExpression(result.ItemProjector.Item, dataSource, context);
        return new ProjectionExpression(result.Type, itemProjector, result.TupleParameterBindings);
      }
    }

    private Expression VisitThenBy(Expression expression, LambdaExpression le, Direction direction)
    {
      using (context.Bindings.Add(le.Parameters[0], VisitSequence(expression)))
      using (state.CreateScope()) {
        state.CalculateExpressions = true;
        var orderByProjector = (ItemProjectorExpression) VisitLambda(le);
        var orderItems = orderByProjector
          .GetColumns(ColumnExtractionModes.TreatEntityAsKey | ColumnExtractionModes.Distinct)
          .Select(ci => new KeyValuePair<int, Direction>(ci, direction));
        var result = context.Bindings[le.Parameters[0]];
        var sortProvider = (SortProvider) result.ItemProjector.DataSource.Provider;
        var sortOrder = new DirectionCollection<int>(sortProvider.Order);
        foreach (var item in orderItems) {
          if (!sortOrder.ContainsKey(item.Key))
            sortOrder.Add(item);
        }
        var recordSet = new SortProvider(sortProvider.Source, sortOrder).Result;
        var itemProjector = new ItemProjectorExpression(result.ItemProjector.Item, recordSet, context);
        return new ProjectionExpression(result.Type, itemProjector, result.TupleParameterBindings);
      }
    }

    private ProjectionExpression VisitJoin(Expression outerSource, Expression innerSource, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression resultSelector, bool isLeftJoin)
    {
      var outerParameter = outerKey.Parameters[0];
      var innerParameter = innerKey.Parameters[0];
      using (context.Bindings.Add(outerParameter, VisitSequence(outerSource)))
      using (context.Bindings.Add(innerParameter, VisitSequence(innerSource))) {
        ItemProjectorExpression outerKeyProjector;
        ItemProjectorExpression innerKeyProjector;
        using (state.CreateScope()) {
          state.CalculateExpressions = true;
          outerKeyProjector = (ItemProjectorExpression) VisitLambda(outerKey);
          innerKeyProjector = (ItemProjectorExpression) VisitLambda(innerKey);
        }

        // Default
        var outerColumns = outerKeyProjector.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
        var innerColumns = innerKeyProjector.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
        var keyPairs = outerColumns.Zip(innerColumns, (o, i) => new Pair<int>(o, i)).ToArray();

        var outer = context.Bindings[outerParameter];
        var inner = context.Bindings[innerParameter];
        var innerAlias = inner.ItemProjector.DataSource.Alias(context.GetNextAlias());
        var recordSet = isLeftJoin
          ? outer.ItemProjector.DataSource.JoinLeft(innerAlias, keyPairs)
          : outer.ItemProjector.DataSource.Join(innerAlias, keyPairs);
        return CombineProjections(outer, inner, recordSet, resultSelector);
      }
    }

    private ProjectionExpression CombineProjections(ProjectionExpression outer, ProjectionExpression inner,
      RecordSet recordSet, LambdaExpression resultSelector)
    {
      var outerDataSource = outer.ItemProjector.DataSource;
      var outerLength = outerDataSource.Header.Length;
      var tupleParameterBindings = outer.TupleParameterBindings.Union(inner.TupleParameterBindings).ToDictionary(pair => pair.Key, pair => pair.Value);
      outer = new ProjectionExpression(outer.Type, outer.ItemProjector.Remap(recordSet, 0), tupleParameterBindings);
      inner = new ProjectionExpression(inner.Type, inner.ItemProjector.Remap(recordSet, outerLength), tupleParameterBindings);

      using (context.Bindings.PermanentAdd(resultSelector.Parameters[0], outer))
      using (context.Bindings.PermanentAdd(resultSelector.Parameters[1], inner))
      using (context.Bindings.LinkParameters(resultSelector.Parameters))
        return BuildProjection(resultSelector);
    }

    private Expression VisitGroupJoin(Expression outerSource, Expression innerSource, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression resultSelector, Expression keyComparer)
    {
      if (keyComparer!=null)
        throw new NotSupportedException(Resources.Strings.ExKeyComparerNotSupportedInGroupJoin);
      var visitedInnerSource = Visit(innerSource);
      var visitedOuterSource = Visit(outerSource);
      var groupingType = typeof (IGrouping<,>).MakeGenericType(innerKey.Type, visitedInnerSource.Type.GetGenericArguments()[0]);
      var enumerableType = typeof (IEnumerable<>).MakeGenericType(visitedInnerSource.Type.GetGenericArguments()[0]);
      var groupingResultType = typeof (IQueryable<>).MakeGenericType(enumerableType);
      var innerGrouping = VisitGroupBy(groupingResultType, visitedInnerSource, innerKey, null, null);

      if (innerGrouping.ItemProjector.Item.IsGroupingExpression()) {
        var groupingExpression = (GroupingExpression) innerGrouping.ItemProjector.Item;
        var selectManyInfo = new GroupingExpression.SelectManyGroupingInfo((ProjectionExpression) visitedOuterSource, (ProjectionExpression) visitedInnerSource, outerKey, innerKey);
        var newGroupingExpression = new GroupingExpression(groupingExpression.Type, groupingExpression.OuterParameter, groupingExpression.DefaultIfEmpty, groupingExpression.ProjectionExpression, groupingExpression.ApplyParameter, groupingExpression.KeyExpression, groupingExpression.Mapping, groupingExpression.OriginalKeySelector, selectManyInfo);
        var newGroupingItemProjector = new ItemProjectorExpression(newGroupingExpression, innerGrouping.ItemProjector.DataSource, innerGrouping.ItemProjector.Context);
        innerGrouping = new ProjectionExpression(innerGrouping.Type, newGroupingItemProjector, innerGrouping.TupleParameterBindings, innerGrouping.ResultType);
      }

      var groupingKeyPropertyInfo = groupingType.GetProperty("Key");
      var groupingJoinParameter = Expression.Parameter(enumerableType, "groupingJoinParameter");
      var groupingKeyExpression = Expression.MakeMemberAccess(
        Expression.Convert(groupingJoinParameter, groupingType)
        , groupingKeyPropertyInfo);
      var lambda = FastExpression.Lambda(groupingKeyExpression, groupingJoinParameter);
      var joinedResult = VisitJoin(visitedOuterSource, innerGrouping, outerKey, lambda, resultSelector, true);
      return joinedResult;
    }

    private ProjectionExpression VisitSelectMany(Type resultType, Expression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
    {
      if (collectionSelector.Parameters.Count > 1)
        throw new NotSupportedException();
      var outerParameter = collectionSelector.Parameters[0];
      var visitedSource = Visit(source);
      var sequence = VisitSequence(visitedSource);
      using (context.Bindings.Add(outerParameter, sequence)) {
        bool isOuter = false;
        if (collectionSelector.Body.NodeType==ExpressionType.Call) {
          var call = (MethodCallExpression) collectionSelector.Body;
          var genericMethodDefinition = call.Method.GetGenericMethodDefinition();
          isOuter = call.Method.IsGenericMethod
            && (genericMethodDefinition==WellKnownMembers.QueryableDefaultIfEmpty
              || genericMethodDefinition==WellKnownMembers.EnumerableDefaultIfEmpty);
          if (isOuter)
            collectionSelector = FastExpression.Lambda(call.Arguments[0], outerParameter);
        }
        ProjectionExpression innerProjection;
        var outerProjection = context.Bindings[outerParameter];
        using (state.CreateScope()) {
          state.OuterParameters = state.OuterParameters
            .Concat(state.Parameters)
            .AddOne(outerParameter)
            .ToArray();
          state.Parameters = ArrayUtils<ParameterExpression>.EmptyArray;
          var visitedCollectionSelector = Visit(collectionSelector.Body);

          if (visitedCollectionSelector.IsGroupingExpression()) {
            var selectManyInfo = ((GroupingExpression) visitedCollectionSelector).SelectManyInfo;
            if (selectManyInfo.GroupByProjection==null)
              return VisitJoin(selectManyInfo.GroupJoinOuterProjection, selectManyInfo.GroupJoinInnerProjection, selectManyInfo.GroupJoinOuterKeySelector, selectManyInfo.GroupJoinInnerKeySelector, resultSelector, true);
            return selectManyInfo.GroupByProjection;
          }

          var projection = VisitSequence(visitedCollectionSelector);
          var innerItemProjector = projection.ItemProjector;
          if (isOuter)
            innerItemProjector = innerItemProjector.SetDefaultIfEmpty();
          innerProjection = new ProjectionExpression(projection.Type, innerItemProjector, projection.TupleParameterBindings, projection.ResultType);
        }
        var applyParameter = context.GetApplyParameter(outerProjection);
        var recordSet = outerProjection.ItemProjector.DataSource.Apply(
          applyParameter,
          innerProjection.ItemProjector.DataSource.Alias(context.GetNextAlias()),
          ApplySequenceType.All,
          isOuter ? JoinType.LeftOuter : JoinType.Inner);

        if (resultSelector==null) {
          var innerParameter = Expression.Parameter(SequenceHelper.GetElementType(collectionSelector.Body.Type), "inner");
          resultSelector = FastExpression.Lambda(innerParameter, outerParameter, innerParameter);
        }
        var resultProjection = CombineProjections(outerProjection, innerProjection, recordSet, resultSelector);
        var resultItemProjector = resultProjection.ItemProjector.RemoveOuterParameter();
        resultProjection = new ProjectionExpression(resultProjection.Type, resultItemProjector, resultProjection.TupleParameterBindings, resultProjection.ResultType);
        return resultProjection;
      }
    }

    private ProjectionExpression VisitSelect(Expression expression, LambdaExpression le)
    {
      var sequence = VisitSequence(expression);
      using (context.Bindings.PermanentAdd(le.Parameters[0], sequence)) {
        var projection = BuildProjection(le);
        return projection;
      }
    }

    private ProjectionExpression BuildProjection(LambdaExpression le)
    {
      using (state.CreateScope()) {
        state.BuildingProjection = true;
        state.CalculateExpressions = true;
        var itemProjector = (ItemProjectorExpression) VisitLambda(le);
        return new ProjectionExpression(
          typeof (IQueryable<>).MakeGenericType(le.Body.Type),
          itemProjector,
          new Dictionary<Parameter<Tuple>, Tuple>());
      }
    }

    private ProjectionExpression VisitWhere(Expression expression, LambdaExpression le)
    {
      var parameter = le.Parameters[0];
      ProjectionExpression visitedSource = VisitSequence(expression);
      using (context.Bindings.Add(parameter, visitedSource))
      using (state.CreateScope()) {
        state.CalculateExpressions = false;
        var predicateExpression = (ItemProjectorExpression) VisitLambda(le);
        var predicate = predicateExpression.ToLambda(context);
        var source = context.Bindings[parameter];
        var recordSet = source.ItemProjector.DataSource.Filter((Expression<Func<Tuple, bool>>) predicate);
        var itemProjector = new ItemProjectorExpression(source.ItemProjector.Item, recordSet, context);
        return new ProjectionExpression(
          expression.Type,
          itemProjector,
          source.TupleParameterBindings);
      }
    }

    private Expression VisitRootExists(Expression source, LambdaExpression predicate, bool notExists)
    {
      var result = predicate==null
        ? VisitSequence(source)
        : VisitWhere(source, predicate);

      var existenceColumn = ColumnExpression.Create(typeof (bool), 0);
      var projectorBody = notExists
        ? Expression.Not(existenceColumn)
        : (Expression) existenceColumn;
      var newRecordSet = result.ItemProjector.DataSource.Existence(context.GetNextColumnAlias());
      var itemProjector = new ItemProjectorExpression(projectorBody, newRecordSet, context);
      return new ProjectionExpression(typeof (bool), itemProjector, result.TupleParameterBindings, ResultType.Single);
    }

    private Expression VisitExists(Expression source, LambdaExpression predicate, bool notExists)
    {
      ProjectionExpression subquery;
      using (state.CreateScope()) {
        state.CalculateExpressions = false;
        subquery = predicate==null
          ? VisitSequence(source)
          : VisitWhere(source, predicate);
      }

      var recordSet = subquery
        .ItemProjector
        .DataSource
        .Existence(context.GetNextColumnAlias());

      var filter = AddSubqueryColumn(typeof (bool), recordSet);
      if (notExists)
        filter = Expression.Not(filter);
      return filter;
    }

    private Expression VisitSetOperations(Expression outerSource, Expression innerSource, QueryableMethodKind methodKind)
    {
      var outer = VisitSequence(outerSource);
      var inner = VisitSequence(innerSource);
      var outerItemProjector = outer.ItemProjector.RemoveOwner();
      var innerItemProjector = inner.ItemProjector.RemoveOwner();
      var outerColumnList = outerItemProjector.GetColumns(ColumnExtractionModes.Default).ToList();
      var innerColumnList = innerItemProjector.GetColumns(ColumnExtractionModes.Default).ToList();
      var outerColumns = outerColumnList.ToArray();
      var outerRecordSet = outerItemProjector.DataSource.Header.Length!=outerColumnList.Count
        ? outerItemProjector.DataSource.Select(outerColumns)
        : outerItemProjector.DataSource;
      var innerRecordSet = innerItemProjector.DataSource.Header.Length!=innerColumnList.Count
        ? innerItemProjector.DataSource.Select(innerColumnList.ToArray())
        : innerItemProjector.DataSource;

      var recordSet = outerItemProjector.DataSource;
      switch (methodKind) {
      case QueryableMethodKind.Concat:
        recordSet = outerRecordSet.Concat(innerRecordSet);
        break;
      case QueryableMethodKind.Except:
        recordSet = outerRecordSet.Except(innerRecordSet);
        break;
      case QueryableMethodKind.Intersect:
        recordSet = outerRecordSet.Intersect(innerRecordSet);
        break;
      case QueryableMethodKind.Union:
        recordSet = outerRecordSet.Union(innerRecordSet);
        break;
      }

      var tupleParameterBindings = outer.TupleParameterBindings.Union(inner.TupleParameterBindings).ToDictionary(pair => pair.Key, pair => pair.Value);

      var itemProjector = outerItemProjector.Remap(recordSet, outerColumns);
      return new ProjectionExpression(outer.Type, itemProjector, tupleParameterBindings);
    }

    private Expression AddSubqueryColumn(Type columnType, RecordSet subquery)
    {
      if (subquery.Header.Length!=1)
        throw new ArgumentException();
      ParameterExpression lambdaParameter = state.Parameters[0];
      var oldResult = context.Bindings[lambdaParameter];

      var applyParameter = context.GetApplyParameter(oldResult);
      int columnIndex = oldResult.ItemProjector.DataSource.Header.Length;
      var newRecordSet = oldResult.ItemProjector.DataSource.Apply(applyParameter, subquery, ApplySequenceType.Single, JoinType.Inner);
      ItemProjectorExpression newItemProjector = oldResult.ItemProjector.Remap(newRecordSet, 0);
      var newResult = new ProjectionExpression(oldResult.Type, newItemProjector, oldResult.TupleParameterBindings);
      context.Bindings.ReplaceBound(lambdaParameter, newResult);

      return ColumnExpression.Create(columnType, columnIndex);
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    private ProjectionExpression VisitSequence(Expression sequenceExpression)
    {
      var sequence = sequenceExpression.StripCasts();
      if (sequence.GetMemberType()==MemberType.EntitySet) {
        if (sequence.NodeType==ExpressionType.MemberAccess) {
          var memberAccess = (MemberExpression) sequence;
          if ((memberAccess.Member is PropertyInfo)
            && memberAccess.Expression!=null
              && memberAccess.Expression.Type.IsSubclassOf(typeof (Entity))) {
            var field = context
              .Model
              .Types[memberAccess.Expression.Type]
              .Fields[memberAccess.Member.Name];
            sequenceExpression = QueryHelper.CreateEntitySetQuery(memberAccess.Expression, field);
          }
        }
      }

      var visitedExpression = Visit(sequenceExpression);
      visitedExpression = visitedExpression.StripCasts();

      if (visitedExpression.IsGroupingExpression()
        || visitedExpression.IsSubqueryExpression())
        return ((SubQueryExpression) visitedExpression).ProjectionExpression;

      if (visitedExpression.IsEntitySetExpression()) {
        var entitySetExpression = (EntitySetExpression) visitedExpression;
        var entitySetQuery = QueryHelper.CreateEntitySetQuery((Expression) entitySetExpression.Owner, entitySetExpression.Field);
        return (ProjectionExpression) Visit(entitySetQuery);
      }

      if (visitedExpression.IsProjection())
        return (ProjectionExpression) visitedExpression;

      throw new NotSupportedException(string.Format(Resources.Strings.ExExpressionOfTypeXIsNotASequence, visitedExpression.Type));
    }


    // Constructors

    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    internal Translator(TranslatorContext context)
    {
      this.context = context;
      state = new State(this);
    }
  }
}