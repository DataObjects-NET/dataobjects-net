// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Linq;
using Xtensive.Parameters;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Orm.Linq.Rewriters;
using Xtensive.Orm.Resources;
using Xtensive.Orm.Linq.Expressions.Visitors;

namespace Xtensive.Orm.Linq
{
  internal sealed partial class Translator : QueryableVisitor
  {
    public TranslatorState state;
    private readonly TranslatorContext context;

    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value==null)
        return c;
      var rootPoint = c.Value as IQueryable;
      if (rootPoint!=null)
        return VisitSequence(rootPoint.Expression);
      return base.VisitConstant(c);
    }

    protected override Expression VisitQueryableMethod(MethodCallExpression mc, QueryableMethodKind methodKind)
    {
      using (state.CreateScope()) {
        switch (methodKind) {
        case QueryableMethodKind.Cast:
            return VisitCast(mc.Arguments[0], mc.Method.GetGenericArguments()[0], mc.Arguments[0].Type.GetGenericArguments()[0]);
        case QueryableMethodKind.AsQueryable:
          return VisitAsQueryable(mc.Arguments[0]);
        case QueryableMethodKind.AsEnumerable:
          break;
        case QueryableMethodKind.ToArray:
          break;
        case QueryableMethodKind.ToList:
          break;
        case QueryableMethodKind.Aggregate:
          break;
        case QueryableMethodKind.ElementAt:
          return VisitElementAt(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), mc.Method.ReturnType, false);
        case QueryableMethodKind.ElementAtOrDefault:
          return VisitElementAt(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), mc.Method.ReturnType, true);
        case QueryableMethodKind.Last:
          break;
        case QueryableMethodKind.LastOrDefault:
          break;
        case QueryableMethodKind.Except:
        case QueryableMethodKind.Intersect:
        case QueryableMethodKind.Concat:
        case QueryableMethodKind.Union:
          state.BuildingProjection = false;
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
          state.BuildingProjection = false;
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
          state.BuildingProjection = false;
          return VisitGroupJoin(mc.Arguments[0],
            mc.Arguments[1],
            mc.Arguments[2].StripQuotes(),
            mc.Arguments[3].StripQuotes(),
            mc.Arguments[4].StripQuotes(),
            mc.Arguments.Count > 5 ? mc.Arguments[5] : null,
            mc);
        case QueryableMethodKind.Join:
          state.BuildingProjection = false;
          return VisitJoin(mc.Arguments[0],
            mc.Arguments[1],
            mc.Arguments[2].StripQuotes(),
            mc.Arguments[3].StripQuotes(),
            mc.Arguments[4].StripQuotes(),
            false,
            mc);
        case QueryableMethodKind.OrderBy:
          state.BuildingProjection = false;
          return VisitOrderBy(mc.Arguments[0], mc.Arguments[1].StripQuotes(), Direction.Positive);
        case QueryableMethodKind.OrderByDescending:
          state.BuildingProjection = false;
          return VisitOrderBy(mc.Arguments[0], mc.Arguments[1].StripQuotes(), Direction.Negative);
        case QueryableMethodKind.Select:
          return VisitSelect(mc.Arguments[0], mc.Arguments[1].StripQuotes());
        case QueryableMethodKind.SelectMany:
          if (mc.Arguments.Count==2) {
            return VisitSelectMany(mc.Arguments[0],
              mc.Arguments[1].StripQuotes(),
              null,
              mc);
          }
          if (mc.Arguments.Count==3) {
            return VisitSelectMany(mc.Arguments[0],
              mc.Arguments[1].StripQuotes(),
              mc.Arguments[2].StripQuotes(),
              mc);
          }
          break;
        case QueryableMethodKind.LongCount:
        case QueryableMethodKind.Count:
        case QueryableMethodKind.Max:
        case QueryableMethodKind.Min:
        case QueryableMethodKind.Sum:
        case QueryableMethodKind.Average:
          if (mc.Arguments.Count==1)
            return VisitAggregate(mc.Arguments[0], mc.Method, null, context.IsRoot(mc), mc);
          if (mc.Arguments.Count==2)
            return VisitAggregate(mc.Arguments[0], mc.Method, mc.Arguments[1].StripQuotes(), context.IsRoot(mc), mc);
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
          state.BuildingProjection = false;
          return VisitThenBy(mc.Arguments[0], mc.Arguments[1].StripQuotes(), Direction.Positive);
        case QueryableMethodKind.ThenByDescending:
          state.BuildingProjection = false;
          return VisitThenBy(mc.Arguments[0], mc.Arguments[1].StripQuotes(), Direction.Negative);
        case QueryableMethodKind.Where:
          state.BuildingProjection = false;
          return VisitWhere(mc.Arguments[0], mc.Arguments[1].StripQuotes());
        default:
          throw new ArgumentOutOfRangeException("methodKind");
        }
      }
      throw new NotSupportedException(String.Format(Strings.ExLinqTranslatorDoesNotSupportMethodX, mc, methodKind));
    }

    private Expression VisitAsQueryable(Expression source)
    {
      return VisitSequence(source);
    }

    private Expression VisitLeftJoin(MethodCallExpression mc)
    {
      return VisitJoin(mc.Arguments[0],
        mc.Arguments[1],
        mc.Arguments[2].StripQuotes(),
        mc.Arguments[3].StripQuotes(),
        mc.Arguments[4].StripQuotes(),
        true,
        mc);
    }

    private Expression VisitLock(MethodCallExpression expression)
    {
      var source = expression.Arguments[0];
      var lockMode = (LockMode) ((ConstantExpression) expression.Arguments[1]).Value;
      var lockBehavior = (LockBehavior) ((ConstantExpression) expression.Arguments[2]).Value;
      var visitedSource = (ProjectionExpression) Visit(source);
      var newDataSource = visitedSource.ItemProjector.DataSource.Lock(lockMode, lockBehavior);
      var newItemProjector = new ItemProjectorExpression(visitedSource.ItemProjector.Item, newDataSource, visitedSource.ItemProjector.Context);
      var projectionExpression = new ProjectionExpression(
        visitedSource.Type, 
        newItemProjector, 
        visitedSource.TupleParameterBindings, 
        visitedSource.ResultType);
      return projectionExpression;
    }

    /// <exception cref="NotSupportedException">OfType supports only 'Entity' conversion.</exception>
    private ProjectionExpression VisitOfType(Expression source, Type targetType, Type sourceType)
    {
      if (!sourceType.IsSubclassOf(typeof (Entity)))
        throw new NotSupportedException(Resources.Strings.ExOfTypeSupportsOnlyEntityConversion);

      var visitedSource = VisitSequence(source);
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
      var entityExpression = EntityExpression.Create(context.Model.Types[targetType], offset, false);
      var itemProjectorExpression = new ItemProjectorExpression(entityExpression, recordSet, context);
      return new ProjectionExpression(sourceType, itemProjectorExpression, visitedSource.TupleParameterBindings);
    }

    /// <exception cref="InvalidCastException">Unable to cast item.</exception>
    /// <exception cref="NotSupportedException">Cast supports only 'Entity' conversion.</exception>
    private ProjectionExpression VisitCast(Expression source, Type targetType, Type sourceType)
    {
      if (!targetType.IsAssignableFrom(sourceType))
        throw new InvalidCastException(string.Format(Strings.ExUnableToCastItemOfTypeXToY, sourceType, targetType));

      var visitedSource = VisitSequence(source);
      var itemProjector = visitedSource.ItemProjector.EnsureEntityIsJoined();
      var projection = new ProjectionExpression(visitedSource.Type, itemProjector, visitedSource.TupleParameterBindings, visitedSource.ResultType);
      if (targetType==sourceType)
        return projection;
      
      var sourceEntity = (EntityExpression)projection.ItemProjector.Item.StripMarkers().StripCasts();
      var recordSet = projection.ItemProjector.DataSource;
      var targetTypeInfo = context.Model.Types[targetType];
      var sourceTypeInfo = context.Model.Types[sourceType];
      var map = Enumerable.Repeat(-1, recordSet.Header.Columns.Count).ToArray();
      var targetFieldIndex = 0;
      foreach (var targetField in targetTypeInfo.Fields.Where(f => f.Parent == null)) {
        var sourceFieldInfo =  targetType.IsInterface 
          ? sourceTypeInfo.FieldMap[targetField]
          : sourceTypeInfo.Fields[targetField.Name];
        var sourceField = sourceEntity.Fields.Single(f => f.Name == sourceFieldInfo.Name);
        var sourceFieldIndex = sourceField.Mapping.Offset;
        map[sourceFieldIndex] = targetFieldIndex++;
      }
      var targetEntity = EntityExpression.Create(targetTypeInfo, 0, false);
      Expression expression;
      using (new RemapScope())
        expression = targetEntity.Remap(map, new Dictionary<Expression, Expression>());
      var replacer = new ExtendedExpressionReplacer(e => e == sourceEntity ? expression : null);
      var targetItem = replacer.Replace(projection.ItemProjector.Item);
      var targetItemProjector = new ItemProjectorExpression(targetItem, recordSet, context);
      var targetProjectionType = typeof(IQueryable<>).MakeGenericType(targetType);
      return new ProjectionExpression(targetProjectionType, targetItemProjector, projection.TupleParameterBindings, projection.ResultType);
//      if (targetType.IsSubclassOf(sourceType)) {
//        var joinedIndex = context.Model.Types[targetType].Indexes.PrimaryIndex;
//        var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
//        offset = recordSet.Header.Columns.Count;
//        var keySegment = visitedSource.ItemProjector.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
//        var keyPairs = keySegment
//          .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
//          .ToArray();
//        recordSet = recordSet.Join(joinedRs, JoinAlgorithm.Default, keyPairs);
//      }
//      var entityExpression = EntityExpression.Create(context.Model.Types[targetType], offset, false);
//      entityExpression.Remap()
//      var itemProjectorExpression = new ItemProjectorExpression(entityExpression, recordSet, context);
//      return new ProjectionExpression(sourceType, itemProjectorExpression, visitedSource.TupleParameterBindings);
    }


    private Expression VisitContains(Expression source, Expression match, bool isRoot)
    {
      var p = Expression.Parameter(match.Type, "p");
      var le = FastExpression.Lambda(Expression.Equal(p, match), p);

      if (isRoot)
        return VisitRootExists(source, le, false);

      if (source.IsQuery() || source.IsLocalCollection(context))
        return VisitExists(source, le, false);


      throw new NotSupportedException(Strings.ExContainsMethodIsOnlySupportedForRootExpressionsOrSubqueries);
    }

    private Expression VisitAll(Expression source, LambdaExpression predicate, bool isRoot)
    {
      predicate = FastExpression.Lambda(Expression.Not(predicate.Body), predicate.Parameters[0]);

      if (isRoot)
        return VisitRootExists(source, predicate, true);

      if (source.IsQuery() || source.IsLocalCollection(context))
        return VisitExists(source, predicate, true);

      throw new NotSupportedException(Strings.ExAllMethodIsOnlySupportedForRootExpressionsOrSubqueries);
    }

    private Expression VisitAny(Expression source, LambdaExpression predicate, bool isRoot)
    {
      if (isRoot)
        return VisitRootExists(source, predicate, false);

      if (source.IsQuery() || source.IsLocalCollection(context))
        return VisitExists(source, predicate, false);

      throw new NotSupportedException(Strings.ExAnyMethodIsOnlySupportedForRootExpressionsOrSubqueries);
    }

    private Expression VisitFirstSingle(Expression source, LambdaExpression predicate, MethodInfo method, bool isRoot)
    {
      var markerType = MarkerType.None;
      var applySequenceType = ApplySequenceType.All;
      var projection = predicate!=null
        ? VisitWhere(source, predicate)
        : VisitSequence(source);
      RecordQuery rightDataSource = null;
      switch (method.Name) {
      case Xtensive.Reflection.WellKnown.Queryable.First:
        applySequenceType = ApplySequenceType.First;
        markerType = MarkerType.First;
        rightDataSource = projection.ItemProjector.DataSource.Take(1);
        break;
      case Xtensive.Reflection.WellKnown.Queryable.FirstOrDefault:
        applySequenceType = ApplySequenceType.FirstOrDefault;
        markerType = MarkerType.First | MarkerType.Default;
        rightDataSource = projection.ItemProjector.DataSource.Take(1);
        break;
      case Xtensive.Reflection.WellKnown.Queryable.Single:
        applySequenceType = ApplySequenceType.Single;
        markerType = MarkerType.Single;
        rightDataSource = projection.ItemProjector.DataSource.Take(2);
        break;
      case Xtensive.Reflection.WellKnown.Queryable.SingleOrDefault:
        applySequenceType = ApplySequenceType.SingleOrDefault;
        markerType = MarkerType.Single | MarkerType.Default;
        rightDataSource = projection.ItemProjector.DataSource.Take(2);
        break;
      }
      var resultType = (ResultType) Enum.Parse(typeof (ResultType), method.Name);
      if (isRoot) {
        var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, rightDataSource, context);
        return new ProjectionExpression(
          method.ReturnType, 
          itemProjector, 
          projection.TupleParameterBindings, 
          resultType);
      }

      var lambdaParameter = state.Parameters[0];
      var oldResult = context.Bindings[lambdaParameter];
      var applyParameter = context.GetApplyParameter(oldResult);

      var leftDataSource = oldResult.ItemProjector.DataSource;
      var columnIndex = leftDataSource.Header.Length;
      var dataSource = leftDataSource.Apply(applyParameter, rightDataSource.Alias(context.GetNextAlias()), !state.BuildingProjection, applySequenceType, JoinType.LeftOuter);
      var rightItemProjector = projection.ItemProjector.Remap(dataSource, columnIndex);
      var result = new ProjectionExpression(oldResult.Type, oldResult.ItemProjector.Remap(dataSource, 0), oldResult.TupleParameterBindings);
      context.Bindings.ReplaceBound(lambdaParameter, result);

      return new MarkerExpression(rightItemProjector.Item, markerType);
    }


    private Expression VisitElementAt(Expression source, Expression index, bool isRoot, Type returnType, bool allowDefault)
    {
      if (QueryCachingScope.Current!=null
        && index.NodeType==ExpressionType.Constant
          && index.Type==typeof (int)) {
        var errorString = allowDefault
          ? Strings.ExElementAtOrDefaultNotSupportedInCompiledQueries
          : Strings.ExElementAtNotSupportedInCompiledQueries;
        throw new InvalidOperationException(String.Format(errorString, ((ConstantExpression) index).Value));
      }

      var projection = VisitSequence(source);
      Func<int> compiledParameter;
      if (index.NodeType==ExpressionType.Quote)
        index = index.StripQuotes();
      RecordQuery rs;
      if (index.Type==typeof (Func<int>)) {
        Expression<Func<int>> elementAtIndex;
        if (QueryCachingScope.Current==null) {
          elementAtIndex = (Expression<Func<int>>) index;
        }
        else {
          var replacer = QueryCachingScope.Current.QueryParameterReplacer;
          elementAtIndex = (Expression<Func<int>>) replacer.Replace(index);
        }
        compiledParameter = elementAtIndex.CachingCompile();
        var skipComparison = Expression.LessThan(elementAtIndex.Body, Expression.Constant(0));
        var condition = Expression.Condition(skipComparison, Expression.Constant(0), Expression.Constant(1));
        var takeParameter = Expression.Lambda<Func<int>>(condition);
        rs = projection.ItemProjector.DataSource.Skip(compiledParameter).Take(takeParameter.CachingCompile());
      }
      else {
        if ((int) ((ConstantExpression) index).Value < 0) {
          if (allowDefault)
            rs = projection.ItemProjector.DataSource.Take(0);
          else
            throw new ArgumentOutOfRangeException("index", index, Strings.ExElementAtIndexMustBeGreaterOrEqualToZero);
        }
        else {
          Expression<Func<int>> parameter = context.ParameterExtractor.ExtractParameter<int>(index);
          compiledParameter = parameter.CachingCompile();
          rs = projection.ItemProjector.DataSource.Skip(compiledParameter).Take(1);
        }
      }

      var resultType = allowDefault ? ResultType.FirstOrDefault : ResultType.First;
      if (isRoot) {
        var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, rs, context);
        return new ProjectionExpression(
          returnType, 
          itemProjector, 
          projection.TupleParameterBindings, 
          resultType);
      }

      var lambdaParameter = state.Parameters[0];
      var oldResult = context.Bindings[lambdaParameter];
      var applyParameter = context.GetApplyParameter(oldResult);

      var leftDataSource = oldResult.ItemProjector.DataSource;
      var columnIndex = leftDataSource.Header.Length;
      var dataSource = leftDataSource.Apply(applyParameter, rs.Alias(context.GetNextAlias()), !state.BuildingProjection, ApplySequenceType.All, JoinType.LeftOuter);
      var rightItemProjector = projection.ItemProjector.Remap(dataSource, columnIndex);
      var result = new ProjectionExpression(oldResult.Type, oldResult.ItemProjector.Remap(dataSource, 0), oldResult.TupleParameterBindings);
      context.Bindings.ReplaceBound(lambdaParameter, result);

      return new MarkerExpression(rightItemProjector.Item, MarkerType.None);
    }


    private ProjectionExpression VisitTake(Expression source, Expression take)
    {
      if (QueryCachingScope.Current!=null
        && take.NodeType==ExpressionType.Constant
          && take.Type==typeof (int))
        throw new InvalidOperationException(String.Format(Strings.ExTakeNotSupportedInCompiledQueries, ((ConstantExpression) take).Value));
      var projection = VisitSequence(source);
      Func<int> compiledParameter;
      if (take.NodeType==ExpressionType.Quote)
        take = take.StripQuotes();
      if (take.Type==typeof (Func<int>)) {
        if (QueryCachingScope.Current==null)
          compiledParameter = ((Expression<Func<int>>) take).CachingCompile();
        else {
          var replacer = QueryCachingScope.Current.QueryParameterReplacer;
          var newTake = replacer.Replace(take);
          compiledParameter = ((Expression<Func<int>>) newTake).CachingCompile();
        }
      }
      else {
        Expression<Func<int>> parameter = context.ParameterExtractor.ExtractParameter<int>(take);
        compiledParameter = parameter.CachingCompile();
      }
      var rs = projection.ItemProjector.DataSource.Take(compiledParameter);
      var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, rs, context);
      return new ProjectionExpression(projection.Type, itemProjector, projection.TupleParameterBindings);
    }

    private ProjectionExpression VisitSkip(Expression source, Expression skip)
    {
      if (QueryCachingScope.Current!=null
        && skip.NodeType==ExpressionType.Constant
          && skip.Type==typeof (int))
        throw new InvalidOperationException(String.Format(Strings.ExSkipNotSupportedInCompiledQueries, ((ConstantExpression) skip).Value));
      var projection = VisitSequence(source);
      Func<int> compiledParameter;
      if (skip.NodeType==ExpressionType.Quote)
        skip = skip.StripQuotes();
      if (skip.Type==typeof (Func<int>)) {
        if (QueryCachingScope.Current==null)
          compiledParameter = ((Expression<Func<int>>) skip).CachingCompile();
        else {
          var replacer = QueryCachingScope.Current.QueryParameterReplacer;
          var newTake = replacer.Replace(skip);
          compiledParameter = ((Expression<Func<int>>) newTake).CachingCompile();
        }
      }
      else {
        Expression<Func<int>> parameter = context.ParameterExtractor.ExtractParameter<int>(skip);
        compiledParameter = parameter.CachingCompile();
      }
      var rs = projection.ItemProjector.DataSource.Skip(compiledParameter);
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

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    private Expression VisitAggregate(Expression source, MethodInfo method, LambdaExpression argument, bool isRoot, MethodCallExpression expressionPart)
    {
      int aggregateColumn;
      AggregateType aggregateType;
      ProjectionExpression innerProjection;

      switch (method.Name) {
      case Xtensive.Reflection.WellKnown.Queryable.Count:
        aggregateType = AggregateType.Count;
        break;
      case Xtensive.Reflection.WellKnown.Queryable.LongCount:
        aggregateType = AggregateType.Count;
        break;
      case Xtensive.Reflection.WellKnown.Queryable.Min:
        aggregateType = AggregateType.Min;
        break;
      case Xtensive.Reflection.WellKnown.Queryable.Max:
        aggregateType = AggregateType.Max;
        break;
      case Xtensive.Reflection.WellKnown.Queryable.Sum:
        aggregateType = AggregateType.Sum;
        break;
      case Xtensive.Reflection.WellKnown.Queryable.Average:
        aggregateType = AggregateType.Avg;
        break;
      default:
        throw new NotSupportedException(String.Format(Strings.ExAggregateMethodXIsNotSupported, expressionPart, method.Name));
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
            throw new NotSupportedException(String.Format(Strings.ExAggregatesForNonPrimitiveTypesAreNotSupported, expressionPart));
          // Default
          columnList = innerProjection.ItemProjector.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
        }
        else {
          using (context.Bindings.Add(argument.Parameters[0], innerProjection))
          using (state.CreateScope()) {
            state.CalculateExpressions = true;
            var result = (ItemProjectorExpression) VisitLambda(argument);
            if (!result.IsPrimitive)
              throw new NotSupportedException(String.Format(Strings.ExAggregatesForNonPrimitiveTypesAreNotSupported, expressionPart));
            // Default
            columnList = result.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
            innerProjection = context.Bindings[argument.Parameters[0]];
          }
        }

        if (columnList.Count!=1)
          throw new NotSupportedException(String.Format(Strings.ExAggregatesForNonPrimitiveTypesAreNotSupported, expressionPart));
        aggregateColumn = columnList[0];
      }


      var aggregateColumnDescriptor = new AggregateColumnDescriptor(context.GetNextColumnAlias(), aggregateColumn, aggregateType);
      var dataSource = innerProjection.ItemProjector.DataSource
        .Aggregate(null, aggregateColumnDescriptor);
      var resultType = method.ReturnType;
      var columnType = dataSource.Header.TupleDescriptor[0];
      if (!isRoot) {
        // Optimization. Use grouping AggregateProvider.
        // TODO: Fix following code. Should use provider tree rewriting
        if (source is ParameterExpression) {
          var groupingParameter = (ParameterExpression) source;
          var groupingProjection = context.Bindings[groupingParameter];
          var groupingDataSource = groupingProjection.ItemProjector.DataSource;
          var supportedAggregate = !(aggregateType == AggregateType.Count && argument != null);
          var groupingProvider = groupingDataSource.Provider as AggregateProvider;
          var oldApplyParameter = context.GetApplyParameter(groupingProjection.ItemProjector.DataSource);
          if (groupingProjection.ItemProjector.Item.IsGroupingExpression() && groupingProvider != null && supportedAggregate) {
            var filterRemover = new SubqueryFilterRemover(oldApplyParameter);
            var newSourñe = filterRemover.VisitCompilable(innerProjection.ItemProjector.DataSource.Provider);
            var newRecordSet = new AggregateProvider(
              newSourñe, 
              groupingProvider.GroupColumnIndexes, 
              groupingProvider.AggregateColumns.Select(c => c.Descriptor).AddOne(aggregateColumnDescriptor).ToArray()).Result;
            var newItemProjector = groupingProjection.ItemProjector.Remap(newRecordSet, 0);
            groupingProjection = new ProjectionExpression(
              groupingProjection.Type, 
              newItemProjector, 
              groupingProjection.TupleParameterBindings, 
              groupingProjection.ResultType);
            context.Bindings.ReplaceBound(groupingParameter, groupingProjection);
            var isSubqueryParameter = state.OuterParameters.Contains(groupingParameter);
            if (isSubqueryParameter) {
              var newApplyParameter = context.GetApplyParameter(newRecordSet);
              foreach (var innerParameter in state.Parameters) {
                var projectionExpression = context.Bindings[innerParameter];
                var newProjectionExpression = new ProjectionExpression(
                  projectionExpression.Type, 
                  projectionExpression.ItemProjector.RewriteApplyParameter(oldApplyParameter, newApplyParameter), 
                  projectionExpression.TupleParameterBindings, 
                  projectionExpression.ResultType);
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
      return new ProjectionExpression(
        resultType, 
        itemProjector, 
        innerProjection.TupleParameterBindings, 
        ResultType.First);
    }

    private ProjectionExpression VisitGroupBy(Type returnType, Expression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
    {
      var sequence = VisitSequence(source);

      ProjectionExpression groupingSourceProjection;
      using (context.Bindings.PermanentAdd(keySelector.Parameters[0], sequence)) {
        using (state.CreateScope()) {
          state.CalculateExpressions = true;
          var itemProjector = (ItemProjectorExpression) VisitLambda(keySelector);
          groupingSourceProjection = new ProjectionExpression(
            typeof (IQueryable<>).MakeGenericType(keySelector.Body.Type),
            itemProjector,
            sequence.TupleParameterBindings);
        }
      }

      var keyColumns = groupingSourceProjection.ItemProjector.GetColumns(
        ColumnExtractionModes.KeepSegment | 
        ColumnExtractionModes.TreatEntityAsKey | 
        ColumnExtractionModes.KeepTypeId).ToArray();
      var keyDataSource = groupingSourceProjection.ItemProjector.DataSource.Aggregate(keyColumns);
      var remappedKeyItemProjector = groupingSourceProjection.ItemProjector.RemoveOwner().Remap(keyDataSource, keyColumns);

      var groupingProjector = new ItemProjectorExpression(remappedKeyItemProjector.Item, keyDataSource, context);
      var groupingProjection = new ProjectionExpression(groupingSourceProjection.Type, groupingProjector, sequence.TupleParameterBindings);

      var comparisonInfos = keyColumns
        .Select((subqueryIndex, groupIndex) => new {
          SubQueryIndex = subqueryIndex, 
          GroupIndex = groupIndex, 
          Type = keyDataSource.Header.Columns[groupIndex].Type.ToNullable() })
        .ToList();
      var applyParameter = context.GetApplyParameter(groupingProjection);
      var tupleParameter = Expression.Parameter(typeof (Tuple), "tuple");
      var filterBody = comparisonInfos.Aggregate(
        (Expression)null, 
        (current, comparisonInfo) => MakeBinaryExpression(
          current, 
          tupleParameter.MakeTupleAccess(comparisonInfo.Type, comparisonInfo.SubQueryIndex), 
          Expression.MakeMemberAccess(Expression.Constant(applyParameter), WellKnownMembers.ApplyParameterValue)
            .MakeTupleAccess(comparisonInfo.Type, comparisonInfo.GroupIndex), 
          ExpressionType.Equal, 
          ExpressionType.AndAlso));
      var filter = FastExpression.Lambda(filterBody, tupleParameter);
      var subqueryProjection = new ProjectionExpression(
        sequence.Type, 
        new ItemProjectorExpression(
          sequence.ItemProjector.Item,
          groupingSourceProjection.ItemProjector.DataSource.Filter((Expression<Func<Tuple, bool>>)filter),
          context),
        sequence.TupleParameterBindings,
        sequence.ResultType
        );
//      var groupingParameter = Expression.Parameter(groupingProjection.ItemProjector.Item.Type, "groupingParameter");
//      var applyParameter = context.GetApplyParameter(groupingProjection);
//      using (context.Bindings.Add(groupingParameter, groupingProjection))
//      using (state.CreateScope()) {
//        state.Parameters = state.Parameters.AddOne(groupingParameter).ToArray();
//        var lambda = FastExpression.Lambda(Expression.Equal(groupingParameter, keySelector.Body), keySelector.Parameters);
//        subqueryProjection = VisitWhere(VisitSequence(source), lambda);
//      }

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
      var groupingParameter = Expression.Parameter(groupingProjection.ItemProjector.Item.Type, "groupingParameter");
      var groupingExpression = new GroupingExpression(realGroupingType, groupingParameter, false, subqueryProjection, applyParameter, remappedKeyItemProjector.Item, selectManyInfo);
      var groupingItemProjector = new ItemProjectorExpression(groupingExpression, groupingProjector.DataSource, context);
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

    private ProjectionExpression VisitJoin(Expression outerSource, Expression innerSource, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression resultSelector, bool isLeftJoin, Expression expressionPart)
    {
      var outerParameter = outerKey.Parameters[0];
      var innerParameter = innerKey.Parameters[0];
      var outerSequence = VisitSequence(outerSource);
      var innerSequence = VisitSequence(innerSource);
      using (context.Bindings.Add(outerParameter, outerSequence))
      using (context.Bindings.Add(innerParameter, innerSequence)) {
        ItemProjectorExpression outerKeyProjector;
        ItemProjectorExpression innerKeyProjector;
        using (state.CreateScope()) {
          state.CalculateExpressions = true;
          outerKeyProjector = (ItemProjectorExpression) VisitLambda(outerKey);
          innerKeyProjector = (ItemProjectorExpression) VisitLambda(innerKey);
        }

        // Default
        var outerColumns = ColumnGatherer.GetColumnsAndExpressions(outerKeyProjector.Item, ColumnExtractionModes.TreatEntityAsKey);
        var innerColumns = ColumnGatherer.GetColumnsAndExpressions(innerKeyProjector.Item, ColumnExtractionModes.TreatEntityAsKey);

        if (outerColumns.Count!=innerColumns.Count)
          throw new InvalidOperationException(String.Format(Strings.JoinKeysLengthMismatch, expressionPart.ToString(true)));

        for (int i = 0; i < outerColumns.Count; i++) {
          var outerColumnKeyExpression = outerColumns[i].Second as KeyExpression;
          var innerColumnKeyExpression = innerColumns[i].Second as KeyExpression;
          // Check key compatibility
          innerColumnKeyExpression.EnsureKeyExpressionCompatible(outerColumnKeyExpression, expressionPart);
        }

        var keyPairs = outerColumns.Zip(innerColumns, (o, i) => new Pair<int>(o.First, i.First)).ToArray();

        var outer = context.Bindings[outerParameter];
        var inner = context.Bindings[innerParameter];
        var innerAlias = inner.ItemProjector.DataSource.Alias(context.GetNextAlias());
        var recordSet = isLeftJoin
          ? outer.ItemProjector.DataSource.LeftJoin(innerAlias, keyPairs)
          : outer.ItemProjector.DataSource.Join(innerAlias, keyPairs);
        return CombineProjections(outer, inner, recordSet, resultSelector);
      }
    }

    private ProjectionExpression CombineProjections(ProjectionExpression outer, ProjectionExpression inner,
      RecordQuery recordQuery, LambdaExpression resultSelector)
    {
      var outerDataSource = outer.ItemProjector.DataSource;
      var outerLength = outerDataSource.Header.Length;
      var tupleParameterBindings = outer.TupleParameterBindings.Union(inner.TupleParameterBindings).ToDictionary(pair => pair.Key, pair => pair.Value);
      outer = new ProjectionExpression(outer.Type, outer.ItemProjector.Remap(recordQuery, 0), tupleParameterBindings);
      inner = new ProjectionExpression(inner.Type, inner.ItemProjector.Remap(recordQuery, outerLength), tupleParameterBindings);

      using (context.Bindings.PermanentAdd(resultSelector.Parameters[0], outer))
      using (context.Bindings.PermanentAdd(resultSelector.Parameters[1], inner))
      using (context.Bindings.LinkParameters(resultSelector.Parameters))
        return BuildProjection(resultSelector);
    }

    private Expression VisitGroupJoin(Expression outerSource, Expression innerSource, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression resultSelector, Expression keyComparer, Expression expressionPart)
    {
      if (keyComparer!=null)
        throw new InvalidOperationException(String.Format(Resources.Strings.ExKeyComparerNotSupportedInGroupJoin, expressionPart));
      var visitedInnerSource = Visit(innerSource);
      var visitedOuterSource = Visit(outerSource);
      var groupingType = typeof (IGrouping<,>).MakeGenericType(innerKey.Type, visitedInnerSource.Type.GetGenericArguments()[0]);
      var enumerableType = typeof (IEnumerable<>).MakeGenericType(visitedInnerSource.Type.GetGenericArguments()[0]);
      var groupingResultType = typeof (IQueryable<>).MakeGenericType(enumerableType);
      var innerGrouping = VisitGroupBy(groupingResultType, visitedInnerSource, innerKey, null, null);

      if (innerGrouping.ItemProjector.Item.IsGroupingExpression()) {
        var groupingExpression = (GroupingExpression) innerGrouping.ItemProjector.Item;
        var selectManyInfo = new GroupingExpression.SelectManyGroupingInfo((ProjectionExpression) visitedOuterSource, (ProjectionExpression) visitedInnerSource, outerKey, innerKey);
        var newGroupingExpression = new GroupingExpression(groupingExpression.Type, groupingExpression.OuterParameter, groupingExpression.DefaultIfEmpty, groupingExpression.ProjectionExpression, groupingExpression.ApplyParameter, groupingExpression.KeyExpression, selectManyInfo);
        var newGroupingItemProjector = new ItemProjectorExpression(newGroupingExpression, innerGrouping.ItemProjector.DataSource, innerGrouping.ItemProjector.Context);
        innerGrouping = new ProjectionExpression(
          innerGrouping.Type, 
          newGroupingItemProjector, 
          innerGrouping.TupleParameterBindings, 
          innerGrouping.ResultType);
      }

      var groupingKeyPropertyInfo = groupingType.GetProperty("Key");
      var groupingJoinParameter = Expression.Parameter(enumerableType, "groupingJoinParameter");
      var groupingKeyExpression = Expression.MakeMemberAccess(
        Expression.Convert(groupingJoinParameter, groupingType)
        , groupingKeyPropertyInfo);
      var lambda = FastExpression.Lambda(groupingKeyExpression, groupingJoinParameter);
      var joinedResult = VisitJoin(visitedOuterSource, innerGrouping, outerKey, lambda, resultSelector, true, expressionPart);
      return joinedResult;
    }

    private ProjectionExpression VisitSelectMany(Expression source, LambdaExpression collectionSelector, LambdaExpression resultSelector, Expression expressionPart)
    {
      var outerParameter = collectionSelector.Parameters[0];
      var visitedSource = Visit(source);
      var sequence = VisitSequence(visitedSource);

      Disposable indexBinding = null;
      if (collectionSelector.Parameters.Count==2) {
        var indexProjection = GetIndexBinding(collectionSelector, ref sequence);
        indexBinding = context.Bindings.Add(collectionSelector.Parameters[1], indexProjection);
      }

      using (indexBinding)
      using (context.Bindings.Add(outerParameter, sequence)) {
        bool isOuter = false;
        if (collectionSelector.Body.NodeType==ExpressionType.Call) {
          var call = (MethodCallExpression) collectionSelector.Body;
          var genericMethodDefinition = call.Method.GetGenericMethodDefinition();
          isOuter = call.Method.IsGenericMethod
            && (genericMethodDefinition==WellKnownMembers.Queryable.DefaultIfEmpty
              || genericMethodDefinition==WellKnownMembers.Enumerable.DefaultIfEmpty);
          if (isOuter)
            collectionSelector = FastExpression.Lambda(call.Arguments[0], outerParameter);
        }
        ProjectionExpression innerProjection;
        using (state.CreateScope()) {
          state.OuterParameters = state.OuterParameters
            .Concat(state.Parameters)
            .Concat(collectionSelector.Parameters)
            .AddOne(outerParameter).ToArray();
          state.Parameters = ArrayUtils<ParameterExpression>.EmptyArray;
          var visitedCollectionSelector = Visit(collectionSelector.Body);

          if (visitedCollectionSelector.IsGroupingExpression()) {
            var selectManyInfo = ((GroupingExpression) visitedCollectionSelector).SelectManyInfo;
            if (selectManyInfo.GroupByProjection==null) {
              LambdaExpression newResultSelector;
              bool rewriteSucceeded = SelectManySelectorRewriter.TryRewrite(
                resultSelector,
                resultSelector.Parameters[0],
                selectManyInfo.GroupJoinOuterKeySelector.Parameters[0],
                out newResultSelector);

              if (rewriteSucceeded)
                return VisitJoin(
                  selectManyInfo.GroupJoinOuterProjection,
                  selectManyInfo.GroupJoinInnerProjection,
                  selectManyInfo.GroupJoinOuterKeySelector,
                  selectManyInfo.GroupJoinInnerKeySelector,
                  newResultSelector, 
                  isOuter,
                  expressionPart);
            }
            else
              return selectManyInfo.GroupByProjection;
          }

          var projection = VisitSequence(visitedCollectionSelector, collectionSelector);
          var innerItemProjector = projection.ItemProjector;
          if (isOuter)
            innerItemProjector = innerItemProjector.SetDefaultIfEmpty();
          innerProjection = new ProjectionExpression(
            projection.Type, 
            innerItemProjector, 
            projection.TupleParameterBindings, 
            projection.ResultType);
        }
        var outerProjection = context.Bindings[outerParameter];
        var applyParameter = context.GetApplyParameter(outerProjection);
        var recordSet = outerProjection.ItemProjector.DataSource.Apply(
          applyParameter,
          innerProjection.ItemProjector.DataSource.Alias(context.GetNextAlias()),
          false,
          ApplySequenceType.All,
          isOuter ? JoinType.LeftOuter : JoinType.Inner);

        if (resultSelector==null) {
          var innerParameter = Expression.Parameter(SequenceHelper.GetElementType(collectionSelector.Body.Type), "inner");
          resultSelector = FastExpression.Lambda(innerParameter, outerParameter, innerParameter);
        }
        var resultProjection = CombineProjections(outerProjection, innerProjection, recordSet, resultSelector);
        var resultItemProjector = resultProjection.ItemProjector.RemoveOuterParameter();
        resultProjection = new ProjectionExpression(
          resultProjection.Type, 
          resultItemProjector, 
          resultProjection.TupleParameterBindings, 
          resultProjection.ResultType);
        return resultProjection;
      }
    }

    private ProjectionExpression VisitSelect(Expression expression, LambdaExpression le)
    {
      var sequence = VisitSequence(expression);
      Disposable indexBinding = null;
      if (le.Parameters.Count==2) {
        var indexProjection = GetIndexBinding(le, ref sequence);
        indexBinding = context.Bindings.PermanentAdd(le.Parameters[1], indexProjection);
      }
      using (indexBinding)
      using (context.Bindings.PermanentAdd(le.Parameters[0], sequence)) {
        var projection = BuildProjection(le);
        return projection;
      }
    }

    private ProjectionExpression BuildProjection(LambdaExpression le)
    {
      using (state.CreateScope()) {
        state.BuildingProjection = true;
        state.CalculateExpressions = !state.IsTailMethod;
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
      Disposable indexBinding = null;
      if (le.Parameters.Count==2) {
        var indexProjection = GetIndexBinding(le, ref visitedSource);
        indexBinding = context.Bindings.Add(le.Parameters[1], indexProjection);
      }
      using (indexBinding)
      using (context.Bindings.Add(parameter, visitedSource))
      using (state.CreateScope()) {
        state.CalculateExpressions = false;
        state.CurrentLambda = le;
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
      return new ProjectionExpression(
        typeof (bool), 
        itemProjector, 
        result.TupleParameterBindings, 
        ResultType.Single);
    }

    private Expression VisitExists(Expression source, LambdaExpression predicate, bool notExists)
    {
      if (source.IsLocalCollection(context) && predicate!=null && predicate.Body.NodeType==ExpressionType.Equal) {
        var parameter = predicate.Parameters[0];
        ProjectionExpression visitedSource;
        using (state.CreateScope()) {
          state.IncludeAlgorithm = IncludeAlgorithm.Auto;
          visitedSource = VisitSequence(source);
        }

        ParameterExpression outerParameter = state.Parameters[0];
        var outerResult = context.Bindings[outerParameter];


        using (context.Bindings.Add(parameter, visitedSource))
        using (state.CreateScope()) {
          state.CalculateExpressions = false;
          state.CurrentLambda = predicate;
          ItemProjectorExpression predicateExpression;
          using (state.CreateScope()) {
            state.IncludeAlgorithm = IncludeAlgorithm.Auto;
            predicateExpression = (ItemProjectorExpression) VisitLambda(predicate);
          }
          var predicateLambda = predicateExpression.ToLambda(context);

          var parameterSource = context.Bindings[parameter];
          var parameterRecordSet = parameterSource.ItemProjector.DataSource;
          var rawProvider = ((RawProvider) ((StoreProvider) visitedSource.ItemProjector.DataSource.Provider).Source);
          var tuples = rawProvider.Source;
          var mapping = IncludeFilterMappingGatherer.Visit(predicateLambda.Parameters[0], rawProvider.Header.Length, predicateLambda.Body);

          var columnIndex = outerResult.ItemProjector.DataSource.Header.Length;
          var newDataSource = outerResult.ItemProjector.DataSource
            .Include(state.IncludeAlgorithm, true, tuples, context.GetNextAlias(), mapping);

          var newItemProjector = outerResult.ItemProjector.Remap(newDataSource, 0);
          var newOuterResult = new ProjectionExpression(
            outerResult.Type, 
            newItemProjector, 
            outerResult.TupleParameterBindings, 
            outerResult.ResultType);
          context.Bindings.ReplaceBound(outerParameter, newOuterResult);
          Expression columnExpression = ColumnExpression.Create(typeof (bool), columnIndex);
          if (notExists)
            columnExpression = Expression.Not(columnExpression);
          return columnExpression;
        }
      }

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

    private Expression VisitIn(MethodCallExpression mc)
    {
      IncludeAlgorithm algorithm = IncludeAlgorithm.Auto;
      Expression source = null;
      Expression match = null;
      switch (mc.Arguments.Count) {
        case 2:
          source = mc.Arguments[1];
          match = mc.Arguments[0];
          break;
        case 3:
          source = mc.Arguments[2];
          match = mc.Arguments[0];
          algorithm = (IncludeAlgorithm) ExpressionEvaluator.Evaluate(mc.Arguments[1]).Value;
          break;
        default:
          Exceptions.InternalError(String.Format(Strings.ExUnknownInSyntax, mc.ToString(true)), Log.Instance);
          break;
      }
      using (state.CreateScope()) {
        state.IncludeAlgorithm = algorithm;
        return VisitContains(source, match, false);
      }
    }


    private Expression VisitSetOperations(Expression outerSource, Expression innerSource, QueryableMethodKind methodKind)
    {
      ProjectionExpression outer;
      ProjectionExpression inner;
      using (state.CreateScope()) {
        state.JoinLocalCollectionEntity = true;
        state.CalculateExpressions = true;
        state.SetOperationProjection = true;
        outer = VisitSequence(outerSource);
        inner = VisitSequence(innerSource);
      }
      var outerItemProjector = outer.ItemProjector.RemoveOwner();
      var innerItemProjector = inner.ItemProjector.RemoveOwner();
      var outerColumnList = outerItemProjector.GetColumns(ColumnExtractionModes.Default).ToList();
      var innerColumnList = innerItemProjector.GetColumns(ColumnExtractionModes.Default).ToList();
      var outerColumns = outerColumnList.ToArray();
      var outerRecordSet = outerItemProjector.DataSource.Header.Length!=outerColumnList.Count || outerColumnList.Select((c,i) => new {c,i}).Any(x => x.c != x.i)
        ? outerItemProjector.DataSource.Select(outerColumns)
        : outerItemProjector.DataSource;
      var innerRecordSet = innerItemProjector.DataSource.Header.Length != innerColumnList.Count || innerColumnList.Select((c, i) => new { c, i }).Any(x => x.c != x.i)
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

    private Expression AddSubqueryColumn(Type columnType, RecordQuery subquery)
    {
      if (subquery.Header.Length!=1)
        throw Exceptions.InternalError(String.Format(Strings.SubqueryXHeaderMustHaveOnlyOneColumn, subquery), Log.Instance);
      ParameterExpression lambdaParameter = state.Parameters[0];
      var oldResult = context.Bindings[lambdaParameter];
      var dataSource = oldResult.ItemProjector.DataSource;
      var applyParameter = context.GetApplyParameter(oldResult.ItemProjector.DataSource);
      int columnIndex = dataSource.Header.Length;
      var newRecordSet = dataSource.Apply(
        applyParameter,
        subquery,
        !state.BuildingProjection,
        ApplySequenceType.Single,
        JoinType.Inner);
      ItemProjectorExpression newItemProjector = oldResult.ItemProjector.Remap(newRecordSet, 0);
      var newResult = new ProjectionExpression(oldResult.Type, newItemProjector, oldResult.TupleParameterBindings);
      context.Bindings.ReplaceBound(lambdaParameter, newResult);

      return ColumnExpression.Create(columnType, columnIndex);
    }


    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    private ProjectionExpression VisitSequence(Expression sequenceExpression)
    {
      return VisitSequence(sequenceExpression, sequenceExpression);
    }

    private ProjectionExpression VisitSequence(Expression sequenceExpression, Expression expressionPart)
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
              .Fields[context.Domain.NameBuilder.BuildFieldName((PropertyInfo)memberAccess.Member)];
            sequenceExpression = QueryHelper.CreateEntitySetQueryExpression(memberAccess.Expression, field);
          }
        }
      }

      if (sequence.IsLocalCollection(context)) {
        Type sequenceType = (sequence.Type.IsGenericType
          && sequence.Type.GetGenericTypeDefinition()==typeof (Func<>))
          ? sequence.Type.GetGenericArguments()[0]
          : sequence.Type;

        Type itemType = sequenceType
          .GetInterfaces()
          .AddOne(sequenceExpression.Type)
          .Single(type => type.IsGenericType && type.GetGenericTypeDefinition()==typeof (IEnumerable<>))
          .GetGenericArguments()[0];

        return (ProjectionExpression) VisitLocalCollectionSequenceMethodInfo.MakeGenericMethod(itemType).Invoke(this, new object[] {sequence});
      }

      var visitedExpression = Visit(sequenceExpression).StripCasts();
      ProjectionExpression result = null;

      if (visitedExpression.IsGroupingExpression() || visitedExpression.IsSubqueryExpression())
        result = ((SubQueryExpression) visitedExpression).ProjectionExpression;

      if (visitedExpression.IsEntitySetExpression()) {
        var entitySetExpression = (EntitySetExpression) visitedExpression;
        var entitySetQuery = QueryHelper.CreateEntitySetQueryExpression((Expression) entitySetExpression.Owner, entitySetExpression.Field);
        result = (ProjectionExpression) Visit(entitySetQuery);
      }

      if (visitedExpression.IsProjection())
        result = (ProjectionExpression) visitedExpression;
      if (result != null) {
        var projectorExpression = result.ItemProjector.EnsureEntityIsJoined();
        if (projectorExpression != result.ItemProjector)
          result = new ProjectionExpression(
            result.Type,
            projectorExpression,
            result.TupleParameterBindings,
            result.ResultType);
        return result;
      }

      throw new InvalidOperationException(string.Format(Strings.ExExpressionXIsNotASequence, expressionPart.ToString(true)));
    }

// ReSharper disable UnusedMember.Local
    private ProjectionExpression VisitLocalCollectionSequence<TItem>(Expression sequence)
    {
      Func<IEnumerable<TItem>> collectionGetter;
      if (QueryCachingScope.Current!=null) {
        var replacer = QueryCachingScope.Current.QueryParameterReplacer;
        var queryParameter = QueryCachingScope.Current.QueryParameter;
        var replace = replacer.Replace(sequence);
        Expression<Func<IEnumerable<TItem>>> parameter = context.ParameterExtractor.ExtractParameter<IEnumerable<TItem>>(replace);
        collectionGetter = parameter.CachingCompile();
      }
      else {
        Expression<Func<IEnumerable<TItem>>> parameter = context.ParameterExtractor.ExtractParameter<IEnumerable<TItem>>(sequence);
        collectionGetter = parameter.CachingCompile();
      }
      return CreateLocalCollectionProjectionExpression(typeof (TItem), collectionGetter, this, sequence);
    }

// ReSharper restore UnusedMember.Local

    private Expression VisitContainsAny(Expression setA, Expression setB, bool isRoot, Type elementType)
    {
      var setAIsQuery = setA.IsQuery();
      var parameter = Expression.Parameter(elementType, "a");
      var containsMethod = WellKnownMembers.Enumerable.Contains.MakeGenericMethod(elementType);

      if (setAIsQuery) {
        var lambda = Expression.Lambda(Expression.Call(containsMethod, setB, parameter), parameter);
        return VisitAny(setA, lambda, isRoot);
      }
      else {
        var lambda = Expression.Lambda(Expression.Call(containsMethod, setA, parameter), parameter);
        return VisitAny(setB, lambda, isRoot);
      }
    }

    private Expression VisitContainsAll(Expression setA, Expression setB, bool isRoot, Type elementType)
    {
      var parameter = Expression.Parameter(elementType, "a");
      var containsMethod = WellKnownMembers.Enumerable.Contains.MakeGenericMethod(elementType);

      var lambda = Expression.Lambda(Expression.Call(containsMethod, setA, parameter), parameter);
      return VisitAll(setB, lambda, isRoot);
    }

    private Expression VisitContainsNone(Expression setA, Expression setB, bool isRoot, Type elementType)
    {
      var setAIsQuery = setA.IsQuery();
      var parameter = Expression.Parameter(elementType, "a");
      var containsMethod = WellKnownMembers.Enumerable.Contains.MakeGenericMethod(elementType);
      if (setAIsQuery) {
        var lambda = Expression.Lambda(Expression.Not(Expression.Call(containsMethod, setB, parameter)), parameter);
        return VisitAll(setA, lambda, isRoot);
      }
      else {
        var lambda = Expression.Lambda(Expression.Not(Expression.Call(containsMethod, setA, parameter)), parameter);
        return VisitAll(setB, lambda, isRoot);
      }
    }
  }
}