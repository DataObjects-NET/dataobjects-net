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
using Xtensive.Linq;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Linq.Model;
using Xtensive.Orm.Linq.Rewriters;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection;
using Tuple = Xtensive.Tuples.Tuple;

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
          return VisitSetOperations(mc.Arguments[0], mc.Arguments[1], methodKind, mc.Method.GetGenericArguments()[0]);
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
          var groupBy = QueryParser.ParseGroupBy(mc);
          return VisitGroupBy(mc.Method.ReturnType,
            groupBy.Source,
            groupBy.KeySelector,
            groupBy.ElementSelector,
            groupBy.ResultSelector);
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
        case QueryableMethodKind.OrderByDescending:
          return VisitSort(mc);
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
        case QueryableMethodKind.ThenByDescending:
          return VisitSort(mc);
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
      if (!typeof(IEntity).IsAssignableFrom(sourceType))
        throw new NotSupportedException(Strings.ExOfTypeSupportsOnlyEntityConversion);

      var visitedSource = VisitSequence(source);
      if (targetType==sourceType)
        return visitedSource;

      int offset = 0;
      var recordSet = visitedSource.ItemProjector.DataSource;

      if (sourceType.IsAssignableFrom(targetType)) {
        var joinedIndex = context.Model.Types[targetType].Indexes.PrimaryIndex;
        var joinedRs = joinedIndex.GetQuery().Alias(context.GetNextAlias());
        offset = recordSet.Header.Columns.Count;
        var keySegment = visitedSource.ItemProjector.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
        var keyPairs = keySegment
          .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
          .ToArray();
        recordSet = recordSet.Join(joinedRs, keyPairs);
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
        var sourceFieldInfo =  targetType.IsInterface && sourceType.IsClass
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
      var matchedElementType = match.Type;
      var sequenceElementType = QueryHelper.GetSequenceElementType(source.Type);
      if (sequenceElementType != matchedElementType) {
        if (sequenceElementType.IsAssignableFrom(matchedElementType)) {
          // Collection<Parent>.Contains(child)
          match = Expression.TypeAs(match, sequenceElementType);
        }
        else {
          // Collection<Child>.Contains(parent)
          if (!isRoot && !source.IsLocalCollection(context))
            QueryHelper.TryAddConvarianceCast(ref source, match.Type);
        }
      }

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

      ProjectionExpression projection;
      using (state.CreateScope()) {
        var isPrimitiveType = WellKnown.SupportedPrimitiveAndNullableTypes.Contains(method.ReturnType);
        state.RequestCalculateExpressions = state.RequestCalculateExpressions
          || !isRoot && isPrimitiveType;
        projection = predicate!=null ? VisitWhere(source, predicate) : VisitSequence(source);
      }

      CompilableProvider rightDataSource = null;
      switch (method.Name) {
      case Reflection.WellKnown.Queryable.First:
        applySequenceType = ApplySequenceType.First;
        markerType = MarkerType.First;
        rightDataSource = projection.ItemProjector.DataSource.Take(1);
        break;
      case Reflection.WellKnown.Queryable.FirstOrDefault:
        applySequenceType = ApplySequenceType.FirstOrDefault;
        markerType = MarkerType.First | MarkerType.Default;
        rightDataSource = projection.ItemProjector.DataSource.Take(1);
        break;
      case Reflection.WellKnown.Queryable.Single:
        applySequenceType = ApplySequenceType.Single;
        markerType = MarkerType.Single;
        rightDataSource = projection.ItemProjector.DataSource.Take(2);
        break;
      case Reflection.WellKnown.Queryable.SingleOrDefault:
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
      CompilableProvider rs;
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
      ProjectionExpression result;
      using (state.CreateScope()) {
        state.RequestCalculateExpressionsOnce = true;
        result = VisitSequence(expression);
      }
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

    private Expression VisitAggregate(Expression source, MethodInfo method, LambdaExpression argument, bool isRoot, MethodCallExpression expressionPart)
    {
      var aggregateType = ExtractAggregateType(expressionPart);
      var origin = VisitAggregateSource(source, argument, aggregateType, expressionPart);
      var originProjection = origin.First;
      var originColumnIndex = origin.Second;
      var aggregateDescriptor = new AggregateColumnDescriptor(context.GetNextColumnAlias(), originColumnIndex, aggregateType);
      var originDataSource = originProjection.ItemProjector.DataSource;
      var resultDataSource = originDataSource.Aggregate(null, aggregateDescriptor);

      // Some aggregate method change type of the column
      // We should take this into account when translating them
      // Types could be promoted to their nullable equivalent (i.e. double -> double?)
      // or promoted to wider types (i.e. single -> double)

      var resultType = method.ReturnType;
      var columnType = resultDataSource.Header.TupleDescriptor[0];
      var convertResultColumn = resultType!=columnType && !resultType.IsNullable();
      if (!convertResultColumn) // Adjust column type so we always use nullable of T instead of T
        columnType = resultType;

      if (isRoot) {
        var projectorBody = (Expression) ColumnExpression.Create(columnType, 0);
        if (convertResultColumn)
          projectorBody = Expression.Convert(projectorBody, resultType);
        var itemProjector = new ItemProjectorExpression(projectorBody, resultDataSource, context);
        return new ProjectionExpression(resultType, itemProjector, originProjection.TupleParameterBindings, ResultType.First);
      }

      // Optimization. Use grouping AggregateProvider.

      var groupingParameter = source as ParameterExpression;
      if (groupingParameter!=null) {
        var groupingProjection = context.Bindings[groupingParameter];
        var groupingDataSource = groupingProjection.ItemProjector.DataSource as AggregateProvider;
        if (groupingDataSource!=null && groupingProjection.ItemProjector.Item.IsGroupingExpression()) {
          var groupingFilterParameter = context.GetApplyParameter(groupingDataSource);
          var commonOriginDataSource = ChooseSourceForAggregate(groupingDataSource.Source,
            SubqueryFilterRemover.Process(originDataSource, groupingFilterParameter),
            ref aggregateDescriptor);
          if (commonOriginDataSource!=null) {
            resultDataSource = new AggregateProvider(
              commonOriginDataSource, groupingDataSource.GroupColumnIndexes,
              groupingDataSource.AggregateColumns.Select(c => c.Descriptor).AddOne(aggregateDescriptor).ToArray());
            var optimizedItemProjector = groupingProjection.ItemProjector.Remap(resultDataSource, 0);
            groupingProjection = new ProjectionExpression(
              groupingProjection.Type, optimizedItemProjector,
              groupingProjection.TupleParameterBindings, groupingProjection.ResultType);
            context.Bindings.ReplaceBound(groupingParameter, groupingProjection);
            var isSubqueryParameter = state.OuterParameters.Contains(groupingParameter);
            if (isSubqueryParameter) {
              var newApplyParameter = context.GetApplyParameter(resultDataSource);
              foreach (var innerParameter in state.Parameters) {
                var projectionExpression = context.Bindings[innerParameter];
                var newProjectionExpression = new ProjectionExpression(
                  projectionExpression.Type,
                  projectionExpression.ItemProjector.RewriteApplyParameter(groupingFilterParameter, newApplyParameter),
                  projectionExpression.TupleParameterBindings,
                  projectionExpression.ResultType);
                context.Bindings.ReplaceBound(innerParameter, newProjectionExpression);
              }
            }
            var resultColumn = ColumnExpression.Create(columnType, resultDataSource.Header.Length - 1);
            if (isSubqueryParameter)
              resultColumn = (ColumnExpression) resultColumn.BindParameter(groupingParameter);
            if (convertResultColumn)
              return Expression.Convert(resultColumn, resultType);
            return resultColumn;
          }
        }
      }

      var result = AddSubqueryColumn(columnType, resultDataSource);
      if (convertResultColumn)
        return Expression.Convert(result, resultType);
      return result;
    }

    private CompilableProvider ChooseSourceForAggregate(CompilableProvider left, CompilableProvider right,
      ref AggregateColumnDescriptor aggregateDescriptor)
    {
      // Choose best available RSE provider when folding aggregate subqueries.
      // Currently we support 3 scenarios:
      // 1) Both origins (for main part and for subquery) are the same provider -> that provider is used.
      // 2) One of the providers is Calculate upon other provider -> Calculate provider is used.
      // 3) Both providers are Calculate and they share a common source -> New combining calculate provider is created

      if (left==right)
        return left;

      if (left.Type==ProviderType.Calculate && left.Sources[0]==right)
        return left;

      if (right.Type==ProviderType.Calculate && right.Sources[0]==left)
        return right;

      if (left.Type==ProviderType.Calculate && right.Type==ProviderType.Calculate
        && left.Sources[0]==right.Sources[0]) {
        var source = (CompilableProvider) left.Sources[0];
        var leftCalculateProvider = (CalculateProvider) left;
        var rightCalculateProvider = (CalculateProvider) right;
        var calculatedColumns = leftCalculateProvider.CalculatedColumns
          .Concat(rightCalculateProvider.CalculatedColumns)
          .Select(c => new CalculatedColumnDescriptor(c.Name, c.Type, c.Expression))
          .ToArray();
        if (aggregateDescriptor.SourceIndex >= source.Header.Length) {
          aggregateDescriptor = new AggregateColumnDescriptor(
            aggregateDescriptor.Name,
            aggregateDescriptor.SourceIndex + leftCalculateProvider.CalculatedColumns.Length,
            aggregateDescriptor.AggregateType);
        }
        return source.Calculate(true, calculatedColumns);
      }

      // No provider matches our criteria -> don't fold aggregate providers.
      return null;
    }

    private Pair<ProjectionExpression, int> VisitAggregateSource(
      Expression source, LambdaExpression aggregateParameter, AggregateType aggregateType, Expression visitedExpression)
    {
      // Process any selectors or filters specified via parameter to aggregating method.
      // This effectively substitutes source.Count(filter) -> source.Where(filter).Count()
      // and source.Sum(selector) -> source.Select(selector).Sum()
      // If parameterless method is called this method simply processes source.
      // This method returns project for source expression and index of a column in RSE provider
      // to which aggregate function should be applied.

      ProjectionExpression sourceProjection;
      int aggregatedColumnIndex;

      if (aggregateType==AggregateType.Count) {
        aggregatedColumnIndex = 0;
        sourceProjection = aggregateParameter!=null ? VisitWhere(source, aggregateParameter) : VisitSequence(source);
        return new Pair<ProjectionExpression, int>(sourceProjection, aggregatedColumnIndex);
      }

      List<int> columnList;
      sourceProjection = VisitSequence(source);
      if (aggregateParameter==null) {
        if (!sourceProjection.ItemProjector.IsPrimitive)
          throw new NotSupportedException(String.Format(Strings.ExAggregatesForNonPrimitiveTypesAreNotSupported, visitedExpression));
        columnList = sourceProjection.ItemProjector.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
      }
      else {
        using (context.Bindings.Add(aggregateParameter.Parameters[0], sourceProjection))
        using (state.CreateScope()) {
          state.CalculateExpressions = true;
          var result = (ItemProjectorExpression) VisitLambda(aggregateParameter);
          if (!result.IsPrimitive)
            throw new NotSupportedException(String.Format(Strings.ExAggregatesForNonPrimitiveTypesAreNotSupported, visitedExpression));
          columnList = result.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
          sourceProjection = context.Bindings[aggregateParameter.Parameters[0]];
        }
      }

      if (columnList.Count!=1)
        throw new NotSupportedException(String.Format(Strings.ExAggregatesForNonPrimitiveTypesAreNotSupported, visitedExpression));

      aggregatedColumnIndex = columnList[0];
      return new Pair<ProjectionExpression, int>(sourceProjection, aggregatedColumnIndex);
    }

    private static AggregateType ExtractAggregateType(MethodCallExpression aggregateCall)
    {
      var methodName = aggregateCall.Method.Name;
      switch (methodName) {
      case Reflection.WellKnown.Queryable.Count:
        return AggregateType.Count;
      case Reflection.WellKnown.Queryable.LongCount:
        return AggregateType.Count;
      case Reflection.WellKnown.Queryable.Min:
        return AggregateType.Min;
      case Reflection.WellKnown.Queryable.Max:
        return AggregateType.Max;
      case Reflection.WellKnown.Queryable.Sum:
        return AggregateType.Sum;
      case Reflection.WellKnown.Queryable.Average:
        return AggregateType.Avg;
      default:
        throw new NotSupportedException(string.Format(Strings.ExAggregateMethodXIsNotSupported, aggregateCall, methodName));
      }
    }

    private ProjectionExpression VisitGroupBy(Type returnType, Expression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
    {
      var sequence = VisitSequence(source);

      ProjectionExpression groupingSourceProjection;
      context.Bindings.PermanentAdd(keySelector.Parameters[0], sequence);
      using (state.CreateScope()) {
        state.CalculateExpressions = true;
        state.GroupingKey = true;
        var itemProjector = (ItemProjectorExpression) VisitLambda(keySelector);
        groupingSourceProjection = new ProjectionExpression(
          typeof (IQueryable<>).MakeGenericType(keySelector.Body.Type),
          itemProjector,
          sequence.TupleParameterBindings);
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

    private Expression VisitSort(Expression expression)
    {
      var extractor = new SortExpressionExtractor();
      if (!extractor.Extract(expression))
        throw new InvalidOperationException(string.Format(Strings.ExInvalidSortExpressionX, expression));

      state.BuildingProjection = false;
      ProjectionExpression projection;
      using (state.CreateScope()) {
        state.CalculateExpressions = false;
        projection = VisitSequence(extractor.BaseExpression);
      }

      var sortColumns = new DirectionCollection<int>();

      foreach (var item in extractor.SortExpressions) {
        var sortExpression = item.Key;
        var direction = item.Value;
        var sortParameter = sortExpression.Parameters[0];
        using (context.Bindings.Add(sortParameter, projection))
        using (state.CreateScope()) {
          state.CalculateExpressions = true;
          var orderByProjector = (ItemProjectorExpression) VisitLambda(sortExpression);
          var columns = orderByProjector
            .GetColumns(ColumnExtractionModes.TreatEntityAsKey | ColumnExtractionModes.Distinct);
          foreach (var c in columns)
            if (!sortColumns.ContainsKey(c))
              sortColumns.Add(c, direction);
          projection = context.Bindings[sortParameter];
        }
      }

      var dataSource = projection.ItemProjector.DataSource.OrderBy(sortColumns);
      var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, dataSource, context);
      return new ProjectionExpression(projection.Type, itemProjector, projection.TupleParameterBindings);
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
      CompilableProvider recordQuery, LambdaExpression resultSelector)
    {
      var outerDataSource = outer.ItemProjector.DataSource;
      var outerLength = outerDataSource.Header.Length;
      var tupleParameterBindings = outer.TupleParameterBindings.Union(inner.TupleParameterBindings).ToDictionary(pair => pair.Key, pair => pair.Value);
      outer = new ProjectionExpression(outer.Type, outer.ItemProjector.Remap(recordQuery, 0), tupleParameterBindings);
      inner = new ProjectionExpression(inner.Type, inner.ItemProjector.Remap(recordQuery, outerLength), tupleParameterBindings);

      context.Bindings.PermanentAdd(resultSelector.Parameters[0], outer);
      context.Bindings.PermanentAdd(resultSelector.Parameters[1], inner);
      using (context.Bindings.LinkParameters(resultSelector.Parameters))
        return BuildProjection(resultSelector);
    }

    private Expression VisitGroupJoin(Expression outerSource, Expression innerSource, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression resultSelector, Expression keyComparer, Expression expressionPart)
    {
      if (keyComparer!=null)
        throw new InvalidOperationException(String.Format(Strings.ExKeyComparerNotSupportedInGroupJoin, expressionPart));

      var visitedInnerSource = Visit(innerSource);
      var visitedOuterSource = Visit(outerSource);
      var innerItemType = visitedInnerSource.Type.GetGenericArguments()[0];
      var groupingType = typeof (IGrouping<,>).MakeGenericType(innerKey.Type, innerItemType);
      var enumerableType = typeof (IEnumerable<>).MakeGenericType(innerItemType);
      var groupingResultType = typeof (IQueryable<>).MakeGenericType(enumerableType);
      var innerGrouping = VisitGroupBy(groupingResultType, visitedInnerSource, innerKey, null, null);

      if (innerGrouping.ItemProjector.Item.IsGroupingExpression()
        && visitedInnerSource is ProjectionExpression
        && visitedOuterSource is ProjectionExpression) {
        var groupingExpression = (GroupingExpression) innerGrouping.ItemProjector.Item;
        var selectManyInfo = new GroupingExpression.SelectManyGroupingInfo(
          (ProjectionExpression) visitedOuterSource,
          (ProjectionExpression) visitedInnerSource,
          outerKey, innerKey);
        var newGroupingExpression = new GroupingExpression(
          groupingExpression.Type, groupingExpression.OuterParameter,
          groupingExpression.DefaultIfEmpty, groupingExpression.ProjectionExpression,
          groupingExpression.ApplyParameter, groupingExpression.KeyExpression, selectManyInfo);
        var newGroupingItemProjector = new ItemProjectorExpression(
          newGroupingExpression,
          innerGrouping.ItemProjector.DataSource,
          innerGrouping.ItemProjector.Context);
        innerGrouping = new ProjectionExpression(
          innerGrouping.Type, 
          newGroupingItemProjector, 
          innerGrouping.TupleParameterBindings, 
          innerGrouping.ResultType);
      }

      var groupingKeyPropertyInfo = groupingType.GetProperty("Key");
      var groupingJoinParameter = Expression.Parameter(enumerableType, "groupingJoinParameter");
      var groupingKeyExpression = Expression.MakeMemberAccess(
        Expression.Convert(groupingJoinParameter, groupingType),
        groupingKeyPropertyInfo);
      var lambda = FastExpression.Lambda(groupingKeyExpression, groupingJoinParameter);
      var joinedResult = VisitJoin(visitedOuterSource, innerGrouping, outerKey, lambda, resultSelector, true, expressionPart);
      return joinedResult;
    }

    private ProjectionExpression VisitSelectMany(Expression source, LambdaExpression collectionSelector, LambdaExpression resultSelector, Expression expressionPart)
    {
      var outerParameter = collectionSelector.Parameters[0];
      var visitedSource = Visit(source);
      var sequence = VisitSequence(visitedSource);

      IDisposable indexBinding = null;
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
          state.RequestCalculateExpressionsOnce = true;
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
      if (le.Parameters.Count==2) {
        var indexProjection = GetIndexBinding(le, ref sequence);
        context.Bindings.PermanentAdd(le.Parameters[1], indexProjection);
      }
      context.Bindings.PermanentAdd(le.Parameters[0], sequence);
      using (state.CreateScope()) {
          state.CalculateExpressions =
            state.RequestCalculateExpressions || state.RequestCalculateExpressionsOnce;
          state.RequestCalculateExpressionsOnce = false;

        return BuildProjection(le);
      }
    }

    private ProjectionExpression BuildProjection(LambdaExpression le)
    {
      using (state.CreateScope()) {
        state.BuildingProjection = true;
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
      IDisposable indexBinding = null;
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
      if (source.IsLocalCollection(context) && predicate!=null && predicate.Body.NodeType==ExpressionType.Equal)
        return VisitExistsAsInclude(source, predicate, notExists);

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

    private Expression VisitExistsAsInclude(Expression source, LambdaExpression predicate, bool notExists)
    {
      // Translate localCollection.Any(item => item==outer) as outer.In(localCollection)

      var parameter = predicate.Parameters[0];
      ProjectionExpression visitedSource;
      using (state.CreateScope()) {
        state.IncludeAlgorithm = IncludeAlgorithm.Auto;
        visitedSource = VisitSequence(source);
      }

      var outerParameter = state.Parameters[0];
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

        RawProvider rawProvider;
        var storeProvider = visitedSource.ItemProjector.DataSource as StoreProvider;
        if (storeProvider!=null)
          rawProvider = (RawProvider) storeProvider.Source;
        else {
          var joinProvider = (JoinProvider) visitedSource.ItemProjector.DataSource;
          rawProvider = (RawProvider) ((StoreProvider)joinProvider.Left).Source;
        }
        var filterColumnCount = rawProvider.Header.Length;
        var filteredTuple = context.GetApplyParameter(context.Bindings[outerParameter]);

        // Mapping from filter data column to expression that requires filtering
        var filteredColumnMappings = IncludeFilterMappingGatherer.Gather(
          predicateLambda.Body, predicateLambda.Parameters[0], filteredTuple, filterColumnCount);

        // Mapping from filter data column to filtered column
        var filteredColumns = Enumerable.Repeat(-1, filterColumnCount).ToArray();
        for (int i = 0; i < filterColumnCount; i++) {
          var mapping = filteredColumnMappings[i];
          if (mapping.ColumnIndex >= 0)
            filteredColumns[i] = mapping.ColumnIndex;
          else {
            var descriptor = CreateCalculatedColumnDescriptor(mapping.CalculatedColumn);
            var column = AddCalculatedColumn(outerParameter, descriptor, mapping.CalculatedColumn.Body.Type);
            filteredColumns[i] = column.Mapping.Offset;
          }
        }

        var outerResult = context.Bindings[outerParameter];
        var columnIndex = outerResult.ItemProjector.DataSource.Header.Length;
        var newDataSource = outerResult.ItemProjector.DataSource
          .Include(state.IncludeAlgorithm, true, rawProvider.Source, context.GetNextAlias(), filteredColumns);

        var newItemProjector = outerResult.ItemProjector.Remap(newDataSource, 0);
        var newOuterResult = new ProjectionExpression(
          outerResult.Type,
          newItemProjector,
          outerResult.TupleParameterBindings,
          outerResult.ResultType);
        context.Bindings.ReplaceBound(outerParameter, newOuterResult);
        Expression resultExpression = ColumnExpression.Create(typeof (bool), columnIndex);
        if (notExists)
          resultExpression = Expression.Not(resultExpression);
        return resultExpression;
      }
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
          Exceptions.InternalError(String.Format(Strings.ExUnknownInSyntax, mc.ToString(true)), OrmLog.Instance);
          break;
      }
      using (state.CreateScope()) {
        state.IncludeAlgorithm = algorithm;
        return VisitContains(source, match, false);
      }
    }


    private Expression VisitSetOperations(Expression outerSource, Expression innerSource, QueryableMethodKind methodKind, Type elementType)
    {
      ProjectionExpression outer;
      ProjectionExpression inner;

      QueryHelper.TryAddConvarianceCast(ref outerSource, elementType);
      QueryHelper.TryAddConvarianceCast(ref innerSource, elementType);

      using (state.CreateScope()) {
        state.JoinLocalCollectionEntity = true;
        state.CalculateExpressions = true;
        state.RequestCalculateExpressions = true;
        outer = VisitSequence(outerSource);
        inner = VisitSequence(innerSource);
      }
      var outerItemProjector = outer.ItemProjector.RemoveOwner();
      var innerItemProjector = inner.ItemProjector.RemoveOwner();
      var outerColumnList = outerItemProjector.GetColumns(ColumnExtractionModes.Distinct).ToList();
      var innerColumnList = innerItemProjector.GetColumns(ColumnExtractionModes.Distinct).ToList();
      if (!outerColumnList.Except(innerColumnList).Any() && outerColumnList.Count==innerColumnList.Count) {
        outerColumnList = outerColumnList.OrderBy(i => i).ToList();
        innerColumnList = innerColumnList.OrderBy(i => i).ToList();
      }
      var outerColumns = outerColumnList.ToArray();
      var outerRecordSet = ShouldWrapDataSourceWithSelect(outerItemProjector, outerColumnList)
        ? outerItemProjector.DataSource.Select(outerColumns)
        : outerItemProjector.DataSource;
      var innerRecordSet = ShouldWrapDataSourceWithSelect(innerItemProjector, innerColumnList)
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

    private bool ShouldWrapDataSourceWithSelect(ItemProjectorExpression expression, ICollection<int> columns)
    {
      return expression.DataSource.Type != ProviderType.Select
        || expression.DataSource.Header.Length != columns.Count
        || columns.Select((c, i) => new { c, i }).Any(x => x.c != x.i);
    }

    private Expression AddSubqueryColumn(Type columnType, CompilableProvider subquery)
    {
      if (subquery.Header.Length!=1)
        throw Exceptions.InternalError(string.Format(Strings.SubqueryXHeaderMustHaveOnlyOneColumn, subquery), OrmLog.Instance);
      var lambdaParameter = state.Parameters[0];
      var oldResult = context.Bindings[lambdaParameter];
      var dataSource = oldResult.ItemProjector.DataSource;
      var applyParameter = context.GetApplyParameter(oldResult.ItemProjector.DataSource);
      var columnIndex = dataSource.Header.Length;
      var newRecordSet = dataSource.Apply(
        applyParameter, subquery, !state.BuildingProjection, ApplySequenceType.Single, JoinType.Inner);
      var newItemProjector = oldResult.ItemProjector.Remap(newRecordSet, 0);
      var newResult = new ProjectionExpression(oldResult.Type, newItemProjector, oldResult.TupleParameterBindings);
      context.Bindings.ReplaceBound(lambdaParameter, newResult);
      return ColumnExpression.Create(columnType, columnIndex);
    }

    private ProjectionExpression VisitSequence(Expression sequenceExpression)
    {
      return VisitSequence(sequenceExpression, sequenceExpression);
    }

    private ProjectionExpression VisitSequence(Expression sequenceExpression, Expression expressionPart)
    {
      var sequence = sequenceExpression.StripCasts();

      if (QueryCachingScope.Current!=null && QueryHelper.IsDirectEntitySetQuery(sequence))
        throw new NotSupportedException(Strings.ExDirectQueryingForEntitySetInCompiledQueriesIsNotSupportedUseQueryEndpointItemsInstead);

      if (sequence.GetMemberType()==MemberType.EntitySet) {
        if (sequence.NodeType==ExpressionType.MemberAccess) {
          var memberAccess = (MemberExpression) sequence;
          if ((memberAccess.Member is PropertyInfo)
            && memberAccess.Expression!=null
              && context.Model.Types.Contains(memberAccess.Expression.Type)) {
            var field = context
              .Model
              .Types[memberAccess.Expression.Type]
              .Fields[context.Domain.Handlers.NameBuilder.BuildFieldName((PropertyInfo)memberAccess.Member)];
            sequenceExpression = QueryHelper.CreateEntitySetQuery(memberAccess.Expression, field);
          }
        }
      }

      if (sequence.IsLocalCollection(context)) {
        Type sequenceType = (sequence.Type.IsGenericType
          && sequence.Type.GetGenericTypeDefinition()==typeof (Func<>))
          ? sequence.Type.GetGenericArguments()[0]
          : sequence.Type;

        var itemType = QueryHelper.GetSequenceElementType(sequenceType);
        return (ProjectionExpression) VisitLocalCollectionSequenceMethod
          .MakeGenericMethod(itemType)
          .Invoke(this, new object[] {sequence});
      }

      var visitedExpression = Visit(sequenceExpression).StripCasts();
      ProjectionExpression result = null;

      if (visitedExpression.IsGroupingExpression() || visitedExpression.IsSubqueryExpression())
        result = ((SubQueryExpression) visitedExpression).ProjectionExpression;

      if (visitedExpression.IsEntitySetExpression()) {
        var entitySetExpression = (EntitySetExpression) visitedExpression;
        var entitySetQuery = QueryHelper.CreateEntitySetQuery((Expression) entitySetExpression.Owner, entitySetExpression.Field);
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
      QueryHelper.TryAddConvarianceCast(ref setA, elementType);
      QueryHelper.TryAddConvarianceCast(ref setB, elementType);

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
      QueryHelper.TryAddConvarianceCast(ref setA, elementType);
      QueryHelper.TryAddConvarianceCast(ref setB, elementType);

      var parameter = Expression.Parameter(elementType, "a");
      var containsMethod = WellKnownMembers.Enumerable.Contains.MakeGenericMethod(elementType);

      var lambda = Expression.Lambda(Expression.Call(containsMethod, setA, parameter), parameter);
      return VisitAll(setB, lambda, isRoot);
    }

    private Expression VisitContainsNone(Expression setA, Expression setB, bool isRoot, Type elementType)
    {
      QueryHelper.TryAddConvarianceCast(ref setA, elementType);
      QueryHelper.TryAddConvarianceCast(ref setB, elementType);

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