// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    private static readonly Type IEnumerableOfKeyType = typeof(IEnumerable<Key>);
    private static readonly Type GenericFuncDefType = typeof(Func<>);
    private static readonly ParameterExpression ParameterContextContextParameter = Expression.Parameter(WellKnownOrmTypes.ParameterContext, "context");

    private readonly TranslatorContext context;
    private readonly bool tagsEnabled;

    internal TranslatorState State { get; private set; } = new(TranslatorState.InitState) {
      NonVisitableExpressions = new()
    };

    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value == null) {
        return c;
      }

      if (c.Value is IQueryable rootPoint) {
        return VisitSequence(rootPoint.Expression);
      }

      return base.VisitConstant(c);
    }

    protected override Expression VisitQueryableMethod(MethodCallExpression mc, QueryableMethodKind methodKind)
    {
      using (CreateScope(new TranslatorState(State))) {
        switch (methodKind) {
          case QueryableMethodKind.Cast:
            return VisitCast(mc.Arguments[0], mc.Method.GetGenericArguments()[0],
              mc.Arguments[0].Type.GetGenericArguments()[0]);
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
            using (CreateScope(new TranslatorState(State) { BuildingProjection = false })) {
              return VisitSetOperations(mc.Arguments[0], mc.Arguments[1], methodKind, mc.Method.GetGenericArguments()[0]);
            }
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
            if (mc.Arguments.Count == 2) {
              return VisitAll(mc.Arguments[0], mc.Arguments[1].StripQuotes(), context.IsRoot(mc));
            }

            break;
          case QueryableMethodKind.OfType:
            var source = mc.Arguments[0];
            var targetType = mc.Method.GetGenericArguments()[0];
            var sourceType = source.Type;
            if (sourceType.IsGenericType) {
              return VisitOfType(source, targetType, sourceType.GetGenericArguments()[0]);
            }
            else {
              var asQueryable = sourceType.GetGenericInterface(WellKnownInterfaces.QueryableOfT);
              if (asQueryable != null) {
                return VisitOfType(source, targetType, asQueryable.GetGenericArguments()[0]);
              }
              throw new NotSupportedException();
            }
          case QueryableMethodKind.Any:
            if (mc.Arguments.Count == 1) {
              return VisitAny(mc.Arguments[0], null, context.IsRoot(mc));
            }

            if (mc.Arguments.Count == 2) {
              return VisitAny(mc.Arguments[0], mc.Arguments[1].StripQuotes(), context.IsRoot(mc));
            }

            break;
          case QueryableMethodKind.Contains:
            if (mc.Arguments.Count == 2) {
              return VisitContains(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc));
            }

            break;
          case QueryableMethodKind.Distinct:
            if (mc.Arguments.Count == 1) {
              return VisitDistinct(mc.Arguments[0]);
            }

            break;
          case QueryableMethodKind.DistinctBy:
            throw new NotSupportedException(Strings.ExUnsupportedDistinctBy);
          case QueryableMethodKind.First:
          case QueryableMethodKind.FirstOrDefault:
          case QueryableMethodKind.Single:
          case QueryableMethodKind.SingleOrDefault:
            if (mc.Arguments.Count == 1) {
              return VisitFirstSingle(mc.Arguments[0], null, mc.Method, context.IsRoot(mc));
            }

            if (mc.Arguments.Count == 2) {
              var predicate = (mc.Arguments[1].StripQuotes());
              return VisitFirstSingle(mc.Arguments[0], predicate, mc.Method, context.IsRoot(mc));
            }

            break;
          case QueryableMethodKind.GroupBy:
            using (CreateScope(new TranslatorState(State) { BuildingProjection = false })) {
              var groupBy = QueryParser.ParseGroupBy(mc);
              return VisitGroupBy(mc.Method.ReturnType,
                groupBy.Source,
                groupBy.KeySelector,
                groupBy.ElementSelector,
                groupBy.ResultSelector);
            }
          case QueryableMethodKind.GroupJoin:
            using (CreateScope(new TranslatorState(State) { BuildingProjection = false })) {
              return VisitGroupJoin(mc.Arguments[0],
                mc.Arguments[1],
                mc.Arguments[2].StripQuotes(),
                mc.Arguments[3].StripQuotes(),
                mc.Arguments[4].StripQuotes(),
                mc.Arguments.Count > 5 ? mc.Arguments[5] : null,
                mc);
            }
          case QueryableMethodKind.Join:
            using (CreateScope(new TranslatorState(State) { BuildingProjection = false })) {
              return VisitJoin(mc.Arguments[0],
                mc.Arguments[1],
                mc.Arguments[2].StripQuotes(),
                mc.Arguments[3].StripQuotes(),
                mc.Arguments[4].StripQuotes(),
                false,
                mc);
            }
          case QueryableMethodKind.OrderBy:
          case QueryableMethodKind.OrderByDescending:
            using (CreateScope(new TranslatorState(State) { BuildingProjection = false })) {
              return VisitSort(mc);
            }
          case QueryableMethodKind.Select:
            return VisitSelect(mc.Arguments[0], mc.Arguments[1].StripQuotes());
          case QueryableMethodKind.SelectMany:
            if (mc.Arguments.Count == 2) {
              return VisitSelectMany(mc.Arguments[0],
                mc.Arguments[1].StripQuotes(),
                null,
                mc);
            }

            if (mc.Arguments.Count == 3) {
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
            if (mc.Arguments.Count == 1) {
              return VisitAggregate(mc.Arguments[0], mc.Method, null, context.IsRoot(mc), mc);
            }

            if (mc.Arguments.Count == 2) {
              return VisitAggregate(mc.Arguments[0], mc.Method, mc.Arguments[1].StripQuotes(), context.IsRoot(mc), mc);
            }

            break;
          case QueryableMethodKind.Skip:
            if (mc.Arguments.Count == 2) {
              return VisitSkip(mc.Arguments[0], mc.Arguments[1]);
            }

            break;
          case QueryableMethodKind.Take:
            if (mc.Arguments.Count == 2) {
              return VisitTake(mc.Arguments[0], mc.Arguments[1]);
            }

            break;
          case QueryableMethodKind.ThenBy:
          case QueryableMethodKind.ThenByDescending:
            using (CreateScope(new TranslatorState(State) { BuildingProjection = false })) {
              return VisitSort(mc);
            }
          case QueryableMethodKind.Where:
            using (CreateScope(new TranslatorState(State) { BuildingProjection = false })) {
              return VisitWhere(mc.Arguments[0], mc.Arguments[1].StripQuotes());
            }
          default:
            throw new ArgumentOutOfRangeException(nameof(methodKind));
        }
      }

      throw new NotSupportedException(string.Format(Strings.ExLinqTranslatorDoesNotSupportMethodX, mc, methodKind));
    }

    private Expression VisitAsQueryable(Expression source) => VisitSequence(source);

    private Expression VisitLeftJoin(MethodCallExpression mc) =>
      VisitJoin(mc.Arguments[0],
        mc.Arguments[1],
        mc.Arguments[2].StripQuotes(),
        mc.Arguments[3].StripQuotes(),
        mc.Arguments[4].StripQuotes(),
        true,
        mc);

    private Expression VisitLock(MethodCallExpression expression)
    {
      var source = expression.Arguments[0];
      var lockMode = (LockMode) ((ConstantExpression) expression.Arguments[1]).Value;
      var lockBehavior = (LockBehavior) ((ConstantExpression) expression.Arguments[2]).Value;
      var visitedSource = (ProjectionExpression) Visit(source);
      var newDataSource = visitedSource.ItemProjector.DataSource.Lock(lockMode, lockBehavior);
      var newItemProjector = new ItemProjectorExpression(
        visitedSource.ItemProjector.Item, newDataSource, visitedSource.ItemProjector.Context);
      var projectionExpression = visitedSource.ApplyItemProjector(newItemProjector);
      return projectionExpression;
    }

    private Expression VisitTag(MethodCallExpression expression)
    {
      var source = expression.Arguments[0];
      var tag = (string) ((ConstantExpression) expression.Arguments[1]).Value;
      var visitedSourceRaw = Visit(source);

      ProjectionExpression visitedSource;
      if (visitedSourceRaw.IsEntitySetExpression()) {
        var entitySetExpression = (EntitySetExpression) visitedSourceRaw;
        var entitySetQuery =
          QueryHelper.CreateEntitySetQuery((Expression) entitySetExpression.Owner, entitySetExpression.Field, context.Domain);
        visitedSource = (ProjectionExpression) Visit(entitySetQuery);
      }
      else {
        visitedSource = (ProjectionExpression) visitedSourceRaw;
      }

      var newDataSource = (tagsEnabled)
        ? visitedSource.ItemProjector.DataSource.Tag(tag)
        : visitedSource.ItemProjector.DataSource;
      var newItemProjector = new ItemProjectorExpression(
        visitedSource.ItemProjector.Item, newDataSource, visitedSource.ItemProjector.Context);
      var projectionExpression = visitedSource.ApplyItemProjector(newItemProjector);
      return projectionExpression;
    }

    /// <exception cref="NotSupportedException">OfType supports only 'Entity' conversion.</exception>
    private ProjectionExpression VisitOfType(Expression source, Type targetType, Type sourceType)
    {
      if (!WellKnownOrmInterfaces.Entity.IsAssignableFrom(sourceType)) {
        throw new NotSupportedException(Strings.ExOfTypeSupportsOnlyEntityConversion);
      }
      if (!WellKnownOrmInterfaces.Entity.IsAssignableFrom(targetType) || !context.Model.Types.Contains(targetType)) {
        throw new NotSupportedException(Strings.ExOfTypeSupportsOnlyEntityConversion);
      }

      var visitedSource = VisitSequence(source);
      if (targetType == sourceType) {
        return visitedSource;
      }

      var targetTypeInfo = context.Model.Types[targetType];

      var currentIndex = 0;
      var indexes = new List<int>(targetTypeInfo.Indexes.PrimaryIndex.Columns.Count);
      foreach (var indexColumn in targetTypeInfo.Indexes.PrimaryIndex.Columns) {
        if (targetTypeInfo.Columns.Contains(indexColumn)) {
          indexes.Add(currentIndex);
        }
        currentIndex++;
      }

      var recordSet = targetTypeInfo.Indexes.PrimaryIndex.GetQuery().Alias(context.GetNextAlias()).Select(indexes);
      var keySegment = visitedSource.ItemProjector.GetColumns(ColumnExtractionModes.TreatEntityAsKey);
      var keyPairs = keySegment
        .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
        .ToArray();

      var dataSource = visitedSource.ItemProjector.DataSource;
      var offset = dataSource.Header.Columns.Count;
      recordSet = recordSet.Alias(context.GetNextAlias());
      recordSet = dataSource.Join(recordSet, keyPairs);

      var entityExpression = EntityExpression.Create(targetTypeInfo, offset, false);
      var itemProjectorExpression = new ItemProjectorExpression(entityExpression, recordSet, context);
      return new ProjectionExpression(sourceType, itemProjectorExpression, visitedSource.TupleParameterBindings);
    }

    /// <exception cref="InvalidCastException">Unable to cast item.</exception>
    private ProjectionExpression VisitCast(Expression source, Type targetType, Type sourceType)
    {
      if (!targetType.IsAssignableFrom(sourceType)) {
        throw new InvalidCastException(string.Format(Strings.ExUnableToCastItemOfTypeXToY, sourceType, targetType));
      }

      var visitedSource = VisitSequence(source);
      var itemProjector = visitedSource.ItemProjector.EnsureEntityIsJoined();
      var projection = visitedSource.ApplyItemProjector(itemProjector);
      if (targetType == sourceType) {
        return projection;
      }

      var sourceEntity = (EntityExpression) projection.ItemProjector.Item.StripMarkers().StripCasts();
      var recordSet = projection.ItemProjector.DataSource;
      var targetTypeInfo = context.Model.Types[targetType];
      var sourceTypeInfo = context.Model.Types[sourceType];
      var map = Enumerable.Repeat(-1, recordSet.Header.Columns.Count).ToArray();
      var targetFieldIndex = 0;
      var targetFields = targetTypeInfo.Fields.Where(f => f.IsPrimitive);
      foreach (var targetField in targetFields) {
        var sourceFieldInfo = targetType.IsInterface && sourceType.IsClass
          ? sourceTypeInfo.FieldMap[targetField]
          : sourceTypeInfo.Fields[targetField.Name];
        var sourceField = sourceEntity.Fields.Single(f => f.Name == sourceFieldInfo.Name);
        var sourceFieldIndex = sourceField.Mapping.Offset;
        var sourceFieldLength = sourceField.Mapping.Length;
        if (map[sourceFieldIndex] != -1) {
          throw new InvalidOperationException(string.Format(Strings.ExUnableToCastXToYAttemptToOverrideExistingFieldMap, sourceType, targetType));
        }
        map[sourceFieldIndex] = targetFieldIndex++;
      }

      var targetEntity = EntityExpression.Create(targetTypeInfo, 0, false);
      Expression expression;
      using (new RemapScope()) {
        expression = targetEntity.Remap(map, new Dictionary<Expression, Expression>());
      }

      var replacer = new ExtendedExpressionReplacer(e => e == sourceEntity ? expression : null);
      var targetItem = replacer.Replace(projection.ItemProjector.Item);
      var targetItemProjector = new ItemProjectorExpression(targetItem, recordSet, context);
      var targetProjectionType = WellKnownInterfaces.QueryableOfT.CachedMakeGenericType(targetType);
      return new ProjectionExpression(targetProjectionType, targetItemProjector, projection.TupleParameterBindings,
        projection.ResultAccessMethod);
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
      var isLocalCollection = source.IsLocalCollection(context);
      if (isLocalCollection) {
        match = Visit(match);
      }

      var matchedElementType = match.Type;
      var sequenceElementType = QueryHelper.GetSequenceElementType(source.Type);
      if (sequenceElementType != matchedElementType) {
        if (sequenceElementType.IsAssignableFrom(matchedElementType)) {
          // Collection<Parent>.Contains(child)
          match = Expression.TypeAs(match, sequenceElementType);
        }
        else {
          // Collection<Child>.Contains(parent)
          if (!isRoot && !isLocalCollection) {
            QueryHelper.TryAddConvarianceCast(ref source, match.Type);
          }
        }
      }

      var p = Expression.Parameter(match.Type, "p");
      var le = FastExpression.Lambda(Expression.Equal(p, match), p);

      if (isRoot) {
        return VisitRootExists(source, le, false);
      }

      if (source.IsQuery() || source.IsLocalCollection(context)) {
        return VisitExists(source, le, false);
      }

      throw new NotSupportedException(Strings.ExContainsMethodIsOnlySupportedForRootExpressionsOrSubqueries);
    }

    private Expression VisitAll(Expression source, LambdaExpression predicate, bool isRoot)
    {
      predicate = FastExpression.Lambda(Expression.Not(predicate.Body), predicate.Parameters[0]);

      if (isRoot) {
        return VisitRootExists(source, predicate, true);
      }

      if (source.IsQuery() || source.IsLocalCollection(context)) {
        return VisitExists(source, predicate, true);
      }

      throw new NotSupportedException(Strings.ExAllMethodIsOnlySupportedForRootExpressionsOrSubqueries);
    }

    private Expression VisitAny(Expression source, LambdaExpression predicate, bool isRoot)
    {
      if (isRoot) {
        return VisitRootExists(source, predicate, false);
      }

      if (source.IsQuery() || source.IsLocalCollection(context)) {
        return VisitExists(source, predicate, false);
      }

      throw new NotSupportedException(Strings.ExAnyMethodIsOnlySupportedForRootExpressionsOrSubqueries);
    }

    private Expression VisitFirstSingle(Expression source, LambdaExpression predicate, MethodInfo method, bool isRoot)
    {
      var markerType = MarkerType.None;
      var applySequenceType = ApplySequenceType.All;

      ProjectionExpression projection;
      using (CreateScope(new TranslatorState(State) {
        RequestCalculateExpressions = State.RequestCalculateExpressions || !isRoot && context.ProviderInfo.SupportedTypes.Contains(method.ReturnType)
      })) {
        projection = predicate != null ? VisitWhere(source, predicate) : VisitSequence(source);
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

      var resultType = (ResultAccessMethod) Enum.Parse(typeof(ResultAccessMethod), method.Name);
      if (isRoot) {
        var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, rightDataSource, context);
        return new ProjectionExpression(
          method.ReturnType,
          itemProjector,
          projection.TupleParameterBindings,
          resultType);
      }

      var lambdaParameter = State.Parameters[0];
      var oldResult = context.Bindings[lambdaParameter];
      var applyParameter = context.GetApplyParameter(oldResult);

      var leftDataSource = oldResult.ItemProjector.DataSource;
      var columnIndex = leftDataSource.Header.Length;
      var dataSource = leftDataSource.Apply(applyParameter, rightDataSource.Alias(context.GetNextAlias()),
        !State.BuildingProjection, applySequenceType, JoinType.LeftOuter);
      var rightItemProjector = projection.ItemProjector.Remap(dataSource, columnIndex);
      var result = new ProjectionExpression(oldResult.Type, oldResult.ItemProjector.Remap(dataSource, 0),
        oldResult.TupleParameterBindings);
      context.Bindings.ReplaceBound(lambdaParameter, result);

      return new MarkerExpression(rightItemProjector.Item, markerType);
    }


    private Expression VisitElementAt(Expression source, Expression index, bool isRoot, Type returnType,
      bool allowDefault)
    {
      if (compiledQueryScope != null
        && index.NodeType == ExpressionType.Constant
        && index.Type == WellKnownTypes.Int32) {
        var errorString = allowDefault
          ? Strings.ExElementAtOrDefaultNotSupportedInCompiledQueries
          : Strings.ExElementAtNotSupportedInCompiledQueries;
        throw new InvalidOperationException(string.Format(errorString, ((ConstantExpression) index).Value));
      }

      var projection = VisitSequence(source);
      Func<ParameterContext, int> compiledParameter;
      if (index.NodeType == ExpressionType.Quote) {
        index = index.StripQuotes();
      }

      CompilableProvider rs;
      if (index.Type == typeof(Func<int>)) {
        Expression<Func<ParameterContext, int>> elementAtIndex;
        ParameterExpression contextParameter;
        if (compiledQueryScope == null) {
          var indexLambda = (Expression<Func<int>>) index;
          contextParameter = ParameterContextContextParameter;
          elementAtIndex = FastExpression.Lambda<Func<ParameterContext, int>>(indexLambda.Body, contextParameter);
        }
        else {
          var replacer = compiledQueryScope.QueryParameterReplacer;
          var newIndex = (Expression<Func<int>>) replacer.Replace(index);
          elementAtIndex = ParameterAccessorFactory.CreateAccessorExpression<int>(newIndex.Body);
          contextParameter = elementAtIndex.Parameters[0];
        }

        compiledParameter = elementAtIndex.CachingCompile();
        var skipComparison = Expression.LessThan(elementAtIndex.Body, Expression.Constant(0));
        var condition = Expression.Condition(skipComparison, Expression.Constant(0), Expression.Constant(1));
        var takeParameter = FastExpression.Lambda<Func<ParameterContext, int>>(condition, contextParameter);
        rs = projection.ItemProjector.DataSource.Skip(compiledParameter).Take(takeParameter.CachingCompile());
      }
      else {
        if ((int) ((ConstantExpression) index).Value < 0) {
          if (allowDefault) {
            rs = projection.ItemProjector.DataSource.Take(0);
          }
          else {
            throw new ArgumentOutOfRangeException(nameof(index), index,
              Strings.ExElementAtIndexMustBeGreaterOrEqualToZero);
          }
        }
        else {
          var parameter = ParameterAccessorFactory.CreateAccessorExpression<int>(index);
          compiledParameter = parameter.CachingCompile();
          rs = projection.ItemProjector.DataSource.Skip(compiledParameter).Take(1);
        }
      }

      var resultType = allowDefault ? ResultAccessMethod.FirstOrDefault : ResultAccessMethod.First;
      if (isRoot) {
        var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, rs, context);
        return new ProjectionExpression(
          returnType,
          itemProjector,
          projection.TupleParameterBindings,
          resultType);
      }

      var lambdaParameter = State.Parameters[0];
      var oldResult = context.Bindings[lambdaParameter];
      var applyParameter = context.GetApplyParameter(oldResult);

      var leftDataSource = oldResult.ItemProjector.DataSource;
      var columnIndex = leftDataSource.Header.Length;
      var dataSource = leftDataSource.Apply(applyParameter, rs.Alias(context.GetNextAlias()), !State.BuildingProjection,
        ApplySequenceType.All, JoinType.LeftOuter);
      var rightItemProjector = projection.ItemProjector.Remap(dataSource, columnIndex);
      var result = new ProjectionExpression(oldResult.Type, oldResult.ItemProjector.Remap(dataSource, 0),
        oldResult.TupleParameterBindings);
      context.Bindings.ReplaceBound(lambdaParameter, result);

      return new MarkerExpression(rightItemProjector.Item, MarkerType.None);
    }


    private ProjectionExpression VisitTake(Expression source, Expression take)
    {
      if (compiledQueryScope != null
        && take.NodeType == ExpressionType.Constant
        && take.Type == WellKnownTypes.Int32) {
        throw new InvalidOperationException(
          string.Format(Strings.ExTakeNotSupportedInCompiledQueries, ((ConstantExpression) take).Value));
      }

      var projection = VisitSequence(source);
      Func<ParameterContext, int> compiledParameter;
      if (take.NodeType == ExpressionType.Quote) {
        take = take.StripQuotes();
      }

      if (take.Type == typeof(Func<int>)) {
        if (compiledQueryScope == null) {
          var takeLambda = (Expression<Func<int>>) take;
          var newTakeLambda = FastExpression.Lambda<Func<ParameterContext, int>>(takeLambda.Body, ParameterContextContextParameter);
          compiledParameter = newTakeLambda.CachingCompile();
        }
        else {
          var replacer = compiledQueryScope.QueryParameterReplacer;
          var newTake = (Expression<Func<int>>) replacer.Replace(take);
          var takeParameterAccessor = ParameterAccessorFactory.CreateAccessorExpression<int>(newTake.Body);
          compiledParameter = takeParameterAccessor.CachingCompile();
        }
      }
      else {
        var parameter = ParameterAccessorFactory.CreateAccessorExpression<int>(take);
        compiledParameter = parameter.CachingCompile();
      }

      var rs = projection.ItemProjector.DataSource.Take(compiledParameter);
      var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, rs, context);
      return new ProjectionExpression(projection.Type, itemProjector, projection.TupleParameterBindings);
    }

    private ProjectionExpression VisitSkip(Expression source, Expression skip)
    {
      if (compiledQueryScope != null
        && skip.NodeType == ExpressionType.Constant
        && skip.Type == WellKnownTypes.Int32) {
        throw new InvalidOperationException(string.Format(Strings.ExSkipNotSupportedInCompiledQueries,
          ((ConstantExpression) skip).Value));
      }

      var projection = VisitSequence(source);
      Func<ParameterContext, int> compiledParameter;
      if (skip.NodeType == ExpressionType.Quote) {
        skip = skip.StripQuotes();
      }

      if (skip.Type == typeof(Func<int>)) {
        if (compiledQueryScope == null) {
          var contextParameter = Expression.Parameter(WellKnownOrmTypes.ParameterContext, "context");
          var skipLambda = (Expression<Func<int>>) skip;
          var newSkipLambda = FastExpression.Lambda<Func<ParameterContext, int>>(skipLambda.Body, contextParameter);
          compiledParameter = newSkipLambda.CachingCompile();
        }
        else {
          var replacer = compiledQueryScope.QueryParameterReplacer;
          var newSkip = (Expression<Func<int>>) replacer.Replace(skip);
          var skipParameterAccessor = ParameterAccessorFactory.CreateAccessorExpression<int>(newSkip.Body);
          compiledParameter = skipParameterAccessor.CachingCompile();
        }
      }
      else {
        var parameter = ParameterAccessorFactory.CreateAccessorExpression<int>(skip);
        compiledParameter = parameter.CachingCompile();
      }

      var rs = projection.ItemProjector.DataSource.Skip(compiledParameter);
      var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, rs, context);
      return new ProjectionExpression(projection.Type, itemProjector, projection.TupleParameterBindings);
    }

    private ProjectionExpression VisitDistinct(Expression expression)
    {
      ProjectionExpression result;
      using (CreateScope(new TranslatorState(State) { RequestCalculateExpressionsOnce = true })) {
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

    private Expression VisitAggregate(Expression source, MethodInfo method, LambdaExpression argument, bool isRoot,
      MethodCallExpression expressionPart)
    {
      var aggregateType = ExtractAggregateType(expressionPart);
      var origin = VisitAggregateSource(source, argument, aggregateType, expressionPart);
      var originProjection = origin.First;
      var originColumnIndex = origin.Second;
      var aggregateDescriptor = new AggregateColumnDescriptor(
        context.GetNextColumnAlias(), originColumnIndex, aggregateType);
      var originDataSource = originProjection.ItemProjector.DataSource;
      var resultDataSource = originDataSource.Aggregate(null, aggregateDescriptor);

      // Some aggregate method change type of the column
      // We should take this into account when translating them
      // Types could be promoted to their nullable equivalent (i.e. double -> double?)
      // or promoted to wider types (i.e. single -> double)

      var resultType = method.ReturnType;
      var columnType = resultDataSource.Header.TupleDescriptor[0];
      var resultIsNullable = resultType.IsNullable();
      var convertResultColumn = resultIsNullable
         ? resultType.StripNullable() != columnType
         : resultType != columnType;
      ;
      if (!convertResultColumn) {
        // Adjust column type so we always use nullable of T instead of T
        columnType = resultType;
      }

      if (isRoot) {
        var projectorBody = (Expression) ColumnExpression.Create(columnType, 0);
        if (convertResultColumn) {
          projectorBody = Expression.Convert(projectorBody, resultType);
        }

        var itemProjector = new ItemProjectorExpression(projectorBody, resultDataSource, context, aggregateType);
        return new ProjectionExpression(
          resultType,
          itemProjector,
          originProjection.TupleParameterBindings,
          resultIsNullable ? ResultAccessMethod.FirstOrDefault : ResultAccessMethod.First);
      }

      // Optimization. Use grouping AggregateProvider.

      if (source is ParameterExpression groupingParameter) {
        var groupingProjection = context.Bindings[groupingParameter];
        if (groupingProjection.ItemProjector.DataSource is AggregateProvider groupingDataSource
          && groupingProjection.ItemProjector.Item.IsGroupingExpression()) {
          var groupingFilterParameter = context.GetApplyParameter(groupingDataSource);
          var commonOriginDataSource = ChooseSourceForAggregate(groupingDataSource.Source,
            SubqueryFilterRemover.Process(originDataSource, groupingFilterParameter),
            ref aggregateDescriptor);
          if (commonOriginDataSource != null) {
            var aggregateDescriptors = groupingDataSource.AggregateColumns
              .Select(c => c.Descriptor)
              .Append(aggregateDescriptor)
              .ToArray(groupingDataSource.AggregateColumns.Length + 1);

            resultDataSource = new AggregateProvider(
              commonOriginDataSource,
              groupingDataSource.GroupColumnIndexes,
              (IReadOnlyList<AggregateColumnDescriptor>) aggregateDescriptors);
            var optimizedItemProjector = groupingProjection.ItemProjector.Remap(resultDataSource, 0);
            groupingProjection = groupingProjection.ApplyItemProjector(optimizedItemProjector);
            context.Bindings.ReplaceBound(groupingParameter, groupingProjection);
            var isSubqueryParameter = State.OuterParameters.Contains(groupingParameter);
            if (isSubqueryParameter) {
              var newApplyParameter = context.GetApplyParameter(resultDataSource);
              foreach (var innerParameter in State.Parameters) {
                var projectionExpression = context.Bindings[innerParameter];
                var newProjectionExpression = projectionExpression.ApplyItemProjector(projectionExpression.ItemProjector.RewriteApplyParameter(groupingFilterParameter, newApplyParameter));
                context.Bindings.ReplaceBound(innerParameter, newProjectionExpression);
              }
            }

            var resultColumn = ColumnExpression.Create(columnType, resultDataSource.Header.Length - 1);
            if (isSubqueryParameter) {
              resultColumn = (ColumnExpression) resultColumn.BindParameter(groupingParameter);
            }
            return convertResultColumn ? Expression.Convert(resultColumn, resultType) : (Expression) resultColumn;
          }
        }
      }

      var result = AddSubqueryColumn(columnType, resultDataSource);
      if (convertResultColumn) {
        return Expression.Convert(result, resultType);
      }
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

      if (left == right) {
        return left;
      }

      if (left.Type == ProviderType.Calculate && left.Sources[0] == right) {
        return left;
      }

      if (right.Type == ProviderType.Calculate && right.Sources[0] == left) {
        return right;
      }

      if (left.Type == ProviderType.Calculate && right.Type == ProviderType.Calculate
        && left.Sources[0] == right.Sources[0]) {
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

    private Pair<ProjectionExpression, int> VisitAggregateSource(Expression source, LambdaExpression aggregateParameter,
      AggregateType aggregateType, Expression visitedExpression)
    {
      // Process any selectors or filters specified via parameter to aggregating method.
      // This effectively substitutes source.Count(filter) -> source.Where(filter).Count()
      // and source.Sum(selector) -> source.Select(selector).Sum()
      // If parameterless method is called this method simply processes source.
      // This method returns project for source expression and index of a column in RSE provider
      // to which aggregate function should be applied.

      ProjectionExpression sourceProjection;
      int aggregatedColumnIndex;

      if (aggregateType == AggregateType.Count) {
        aggregatedColumnIndex = 0;
        sourceProjection = aggregateParameter != null ? VisitWhere(source, aggregateParameter) : VisitSequence(source);
        return new Pair<ProjectionExpression, int>(sourceProjection, aggregatedColumnIndex);
      }

      List<int> columnList = null;
      sourceProjection = VisitSequence(source);
      if (aggregateParameter == null) {
        if (sourceProjection.ItemProjector.IsPrimitive) {
          columnList = sourceProjection.ItemProjector.GetColumns(ColumnExtractionModes.TreatEntityAsKey).ToList();
        }
        else {
          var lambdaType = sourceProjection.ItemProjector.Item.Type;
          EnsureAggregateIsPossible(lambdaType, aggregateType, visitedExpression);
          var paramExpression = Expression.Parameter(lambdaType, "arg");
          aggregateParameter = FastExpression.Lambda(paramExpression, paramExpression);
        }
      }

      if (aggregateParameter != null) {
        using (context.Bindings.Add(aggregateParameter.Parameters[0], sourceProjection))
        using (CreateScope(new TranslatorState(State) { CalculateExpressions = true })) {
          var result = (ItemProjectorExpression) VisitLambda(aggregateParameter);
          if (!result.IsPrimitive) {
            throw new NotSupportedException(
              string.Format(Strings.ExAggregatesForNonPrimitiveTypesAreNotSupported, visitedExpression));
          }

          columnList = result.GetColumns(ColumnExtractionModes.TreatEntityAsKey).ToList();
          sourceProjection = context.Bindings[aggregateParameter.Parameters[0]];
        }
      }

      if (columnList.Count != 1) {
        throw new NotSupportedException(
          string.Format(Strings.ExAggregatesForNonPrimitiveTypesAreNotSupported, visitedExpression));
      }

      aggregatedColumnIndex = columnList[0];
      return new Pair<ProjectionExpression, int>(sourceProjection, aggregatedColumnIndex);
    }

    private static void EnsureAggregateIsPossible(Type type, AggregateType aggregateType, Expression visitedExpression)
    {
      switch (aggregateType) {
        case AggregateType.Count:
          return;
        case AggregateType.Avg:
        case AggregateType.Sum:
          if (!type.IsNumericType()) {
            throw new NotSupportedException(
              string.Format(Strings.ExAggregatesForNonPrimitiveTypesAreNotSupported, visitedExpression));
          }

          return;
        case AggregateType.Min:
        case AggregateType.Max:
          if (type.IsNullable()) {
            type = Nullable.GetUnderlyingType(type);
          }

          if (!WellKnownInterfaces.Comparable.IsAssignableFrom(type)) {
            throw new NotSupportedException(
              string.Format(Strings.ExAggregatesForNonPrimitiveTypesAreNotSupported, visitedExpression));
          }

          return;
      }
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
          throw new NotSupportedException(
            string.Format(Strings.ExAggregateMethodXIsNotSupported, aggregateCall, methodName));
      }
    }

    private ProjectionExpression VisitGroupBy(Type returnType, Expression source, LambdaExpression keySelector,
      LambdaExpression elementSelector, LambdaExpression resultSelector)
    {
      var sequence = VisitSequence(source);

      ProjectionExpression groupingSourceProjection;
      context.Bindings.PermanentAdd(keySelector.Parameters[0], sequence);
      using (CreateScope(new TranslatorState(State) { CalculateExpressions = true, GroupingKey = true })) {
        var itemProjector = (ItemProjectorExpression) VisitLambda(keySelector);
        groupingSourceProjection = new ProjectionExpression(
          WellKnownInterfaces.QueryableOfT.CachedMakeGenericType(keySelector.Body.Type),
          itemProjector,
          sequence.TupleParameterBindings);
      }

      // this is new Object.There is no need to do ToArray
      var keyFieldsRaw = groupingSourceProjection.ItemProjector.GetColumnsAndExpressions(
        ColumnExtractionModes.KeepSegment |
        ColumnExtractionModes.TreatEntityAsKey |
        ColumnExtractionModes.KeepTypeId);

      var nullableKeyColumns = (!State.SkipNullableColumnsDetectionInGroupBy)
        ? GetNullableGroupingExpressions(keyFieldsRaw)
        : Array.Empty<int>();

      var keyColumns = keyFieldsRaw.SelectToArray(pair => pair.First);
      var keyDataSource = groupingSourceProjection.ItemProjector.DataSource.Aggregate(keyColumns);
      var remappedKeyItemProjector =
        groupingSourceProjection.ItemProjector.RemoveOwner().Remap(keyDataSource, keyColumns);

      var groupingProjector = new ItemProjectorExpression(remappedKeyItemProjector.Item, keyDataSource, context);
      var groupingProjection = new ProjectionExpression(groupingSourceProjection.Type, groupingProjector,
        sequence.TupleParameterBindings);

      // subqueryIndex - values of array
      // groupIndex    - indexes of values of array
      var comparisonInfos = keyColumns
        .Select((subqueryIndex, groupIndex) => (
          SubQueryIndex: subqueryIndex,
          GroupIndex: groupIndex,
          Type: keyDataSource.Header.Columns[groupIndex].Type.ToNullable()
        ));
      var applyParameter = context.GetApplyParameter(groupingProjection);
      var tupleParameter = QueryHelper.TupleParameter;

      var filterBody = (nullableKeyColumns.Count == 0)
        ? comparisonInfos.Aggregate(
          (Expression) null,
          (current, comparisonInfo) =>
            MakeBooleanExpression(
              current,
              tupleParameter.MakeTupleAccess(comparisonInfo.Type, comparisonInfo.SubQueryIndex),
              Expression.MakeMemberAccess(Expression.Constant(applyParameter), WellKnownMembers.ApplyParameterValue)
                .MakeTupleAccess(comparisonInfo.Type, comparisonInfo.GroupIndex),
              ExpressionType.Equal,
              ExpressionType.AndAlso))
        : comparisonInfos.Aggregate(
          (Expression) null,
          (current, comparisonInfo) => {
            if (nullableKeyColumns.Contains(comparisonInfo.SubQueryIndex)) {
              var groupingSubqueryConnector = Expression.MakeMemberAccess(Expression.Constant(applyParameter),
                WellKnownMembers.ApplyParameterValue);
              var left = MakeBooleanExpression(
                null,
                tupleParameter.MakeTupleAccess(comparisonInfo.Type, comparisonInfo.SubQueryIndex),
                groupingSubqueryConnector.MakeTupleAccess(comparisonInfo.Type, comparisonInfo.GroupIndex),
                ExpressionType.Equal,
                ExpressionType.AndAlso);

              var right = MakeBooleanExpression(
                null,
                MakeBooleanExpression(
                  null,
                  tupleParameter.MakeTupleAccess(comparisonInfo.Type, comparisonInfo.SubQueryIndex),
                  Expression.Constant(null, comparisonInfo.Type),
                  ExpressionType.Equal,
                  ExpressionType.AndAlso),
                MakeBooleanExpression(
                  null,
                  groupingSubqueryConnector.MakeTupleAccess(comparisonInfo.Type, comparisonInfo.GroupIndex),
                  Expression.Constant(null, comparisonInfo.Type),
                  ExpressionType.Equal,
                  ExpressionType.AndAlso),
                ExpressionType.AndAlso,
                ExpressionType.AndAlso);
              return MakeBooleanExpression(current, left, right, ExpressionType.OrElse, ExpressionType.AndAlso);
            }

            return MakeBooleanExpression(
              current,
              tupleParameter.MakeTupleAccess(comparisonInfo.Type, comparisonInfo.SubQueryIndex),
              Expression.MakeMemberAccess(Expression.Constant(applyParameter), WellKnownMembers.ApplyParameterValue)
                .MakeTupleAccess(comparisonInfo.Type, comparisonInfo.GroupIndex),
              ExpressionType.Equal,
              ExpressionType.AndAlso);
          });

      var filter = FastExpression.Lambda(filterBody, tupleParameter);
      var subqueryProjection = sequence.ApplyItemProjector(new ItemProjectorExpression(
          sequence.ItemProjector.Item,
          groupingSourceProjection.ItemProjector.DataSource.Filter((Expression<Func<Tuple, bool>>) filter),
          context));
      //      var groupingParameter = Expression.Parameter(groupingProjection.ItemProjector.Item.Type, "groupingParameter");
      //      var applyParameter = context.GetApplyParameter(groupingProjection);
      //      using (context.Bindings.Add(groupingParameter, groupingProjection))
      //      using (CreateScope(new TranslatorState(state) { Parameters = state.Parameters.AddOne(groupingParameter).ToArray() })) {
      //        var lambda = FastExpression.Lambda(Expression.Equal(groupingParameter, keySelector.Body), keySelector.Parameters);
      //        subqueryProjection = VisitWhere(VisitSequence(source), lambda);
      //      }

      var keyType = keySelector.Type.GetGenericArguments()[1];
      var elementType = elementSelector == null
        ? keySelector.Parameters[0].Type
        : elementSelector.Type.GetGenericArguments()[1];
      var groupingType = WellKnownInterfaces.GroupingOfTKeyTElement.CachedMakeGenericType(keyType, elementType);

      var realGroupingType =
        resultSelector != null
          ? resultSelector.Parameters[1].Type
          : returnType.GetGenericArguments()[0];

      if (elementSelector != null) {
        subqueryProjection = VisitSelect(subqueryProjection, elementSelector);
      }

      var selectManyInfo = new GroupingExpression.SelectManyGroupingInfo(sequence);
      var groupingParameter = Expression.Parameter(groupingProjection.ItemProjector.Item.Type, "groupingParameter");
      var groupingExpression = new GroupingExpression(realGroupingType, groupingParameter, false, subqueryProjection,
        applyParameter, remappedKeyItemProjector.Item, selectManyInfo);
      var groupingItemProjector =
        new ItemProjectorExpression(groupingExpression, groupingProjector.DataSource, context);
      returnType = resultSelector == null
        ? returnType
        : resultSelector.Parameters[1].Type;
      var resultProjection =
        new ProjectionExpression(returnType, groupingItemProjector, subqueryProjection.TupleParameterBindings);

      if (resultSelector != null) {
        var keyProperty = groupingType.GetProperty(WellKnown.KeyFieldName);
        var convertedParameter = Expression.Convert(resultSelector.Parameters[1], groupingType);
        var keyAccess = Expression.MakeMemberAccess(convertedParameter, keyProperty);
        var rewrittenResultSelectorBody =
          ParameterRewriter.Rewrite(resultSelector.Body, resultSelector.Parameters[0], keyAccess);
        var selectLambda = FastExpression.Lambda(rewrittenResultSelectorBody, resultSelector.Parameters[1]);
        resultProjection = VisitSelect(resultProjection, selectLambda);
      }

      return resultProjection;
    }

    private Expression VisitSort(Expression expression)
    {
      var extractor = new SortExpressionExtractor();
      if (!extractor.Extract(expression)) {
        throw new InvalidOperationException(string.Format(Strings.ExInvalidSortExpressionX, expression));
      }

      ProjectionExpression projection;
      using (CreateScope(new TranslatorState(State) { CalculateExpressions = false })) {
        projection = VisitSequence(extractor.BaseExpression);
      }

      var sortColumns = new DirectionCollection<int>();

      foreach (var item in extractor.SortExpressions) {
        var sortExpression = item.Key;
        var direction = item.Value;
        var sortParameter = sortExpression.Parameters[0];
        using (context.Bindings.Add(sortParameter, projection))
        using (CreateScope(new TranslatorState(State) { ShouldOmitConvertToObject = true, CalculateExpressions = true })) {
          var orderByProjector = (ItemProjectorExpression) VisitLambda(sortExpression);
          var columns = orderByProjector
            .GetColumns(ColumnExtractionModes.TreatEntityAsKey | ColumnExtractionModes.Distinct);
          foreach (var c in columns) {
            if (!sortColumns.ContainsKey(c)) {
              sortColumns.Add(c, direction);
            }
          }

          projection = context.Bindings[sortParameter];
        }
      }

      var dataSource = projection.ItemProjector.DataSource.OrderBy(sortColumns);
      var itemProjector = new ItemProjectorExpression(projection.ItemProjector.Item, dataSource, context);
      return new ProjectionExpression(projection.Type, itemProjector, projection.TupleParameterBindings);
    }

    private ProjectionExpression VisitJoin(Expression outerSource, Expression innerSource, LambdaExpression outerKey,
      LambdaExpression innerKey, LambdaExpression resultSelector, bool isLeftJoin, Expression expressionPart)
    {
      var outerParameter = outerKey.Parameters[0];
      var innerParameter = innerKey.Parameters[0];
      if (innerParameter == outerParameter) {
        throw new NotSupportedException(Strings.ExJoinHasSameInnerAndOuterParameterInstances);
      }

      var outerSequence = VisitSequence(outerSource);
      var innerSequence = VisitSequence(innerSource);
      using (context.Bindings.Add(outerParameter, outerSequence))
      using (context.Bindings.Add(innerParameter, innerSequence)) {
        ItemProjectorExpression outerKeyProjector;
        ItemProjectorExpression innerKeyProjector;
        using (CreateScope(new TranslatorState(State) { CalculateExpressions = true })) {
          outerKeyProjector = (ItemProjectorExpression) VisitLambda(outerKey);
          innerKeyProjector = (ItemProjectorExpression) VisitLambda(innerKey);
        }

        // Default
        var outerColumns =
          ColumnGatherer.GetColumnsAndExpressions(outerKeyProjector.Item, ColumnExtractionModes.TreatEntityAsKey);
        var innerColumns =
          ColumnGatherer.GetColumnsAndExpressions(innerKeyProjector.Item, ColumnExtractionModes.TreatEntityAsKey);

        if (outerColumns.Count != innerColumns.Count) {
          throw new InvalidOperationException(string.Format(Strings.JoinKeysLengthMismatch,
            expressionPart.ToString(true)));
        }

        for (var i = 0; i < outerColumns.Count; i++) {
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
      using (context.Bindings.LinkParameters(resultSelector.Parameters)) {
        return BuildProjection(resultSelector);
      }
    }

    private Expression VisitGroupJoin(Expression outerSource, Expression innerSource, LambdaExpression outerKey,
      LambdaExpression innerKey, LambdaExpression resultSelector, Expression keyComparer, Expression expressionPart)
    {
      if (keyComparer != null) {
        throw new InvalidOperationException(
          string.Format(Strings.ExKeyComparerNotSupportedInGroupJoin, expressionPart));
      }

      var visitedInnerSource = Visit(innerSource);
      var visitedOuterSource = Visit(outerSource);
      var innerItemType = visitedInnerSource.Type.GetGenericArguments()[0];
      var groupingType = WellKnownInterfaces.GroupingOfTKeyTElement.CachedMakeGenericType(innerKey.Type, innerItemType);
      var enumerableType = WellKnownInterfaces.EnumerableOfT.CachedMakeGenericType(innerItemType);
      var groupingResultType = WellKnownInterfaces.QueryableOfT.CachedMakeGenericType(enumerableType);

      ProjectionExpression innerGrouping;
      using (CreateScope(new TranslatorState(State) { SkipNullableColumnsDetectionInGroupBy = true })) {
        innerGrouping = VisitGroupBy(groupingResultType, visitedInnerSource, innerKey, null, null);
      }

      if (innerGrouping.ItemProjector.Item.IsGroupingExpression()
        && visitedInnerSource is ProjectionExpression innerSourceExpression
        && visitedOuterSource is ProjectionExpression outerSourceExpression) {
        var groupingExpression = (GroupingExpression) innerGrouping.ItemProjector.Item;
        var selectManyInfo = new GroupingExpression.SelectManyGroupingInfo(
          outerSourceExpression,
          innerSourceExpression,
          outerKey, innerKey);
        var newGroupingExpression = new GroupingExpression(
          groupingExpression.Type, groupingExpression.OuterParameter,
          groupingExpression.DefaultIfEmpty, groupingExpression.ProjectionExpression,
          groupingExpression.ApplyParameter, groupingExpression.KeyExpression, selectManyInfo);
        var newGroupingItemProjector = new ItemProjectorExpression(
          newGroupingExpression,
          innerGrouping.ItemProjector.DataSource,
          innerGrouping.ItemProjector.Context);
        innerGrouping = innerGrouping.ApplyItemProjector(newGroupingItemProjector);
      }

      var groupingKeyPropertyInfo = groupingType.GetProperty(WellKnown.KeyFieldName);
      var groupingJoinParameter = Expression.Parameter(enumerableType, "groupingJoinParameter");
      var groupingKeyExpression = Expression.MakeMemberAccess(
        Expression.Convert(groupingJoinParameter, groupingType),
        groupingKeyPropertyInfo);
      var lambda = FastExpression.Lambda(groupingKeyExpression, groupingJoinParameter);
      var joinedResult = VisitJoin(visitedOuterSource, innerGrouping, outerKey, lambda, resultSelector, true,
        expressionPart);
      return joinedResult;
    }

    private ProjectionExpression VisitSelectMany(Expression source, LambdaExpression collectionSelector,
      LambdaExpression resultSelector, Expression expressionPart)
    {
      var outerParameter = collectionSelector.Parameters[0];
      var visitedSource = Visit(source);
      var sequence = VisitSequence(visitedSource);

      var indexBinding = BindingCollection<ParameterExpression, ProjectionExpression>.BindingScope.Empty;
      if (collectionSelector.Parameters.Count == 2) {
        var indexProjection = GetIndexBinding(collectionSelector, ref sequence);
        indexBinding = context.Bindings.Add(collectionSelector.Parameters[1], indexProjection);
      }

      using (indexBinding)
      using (context.Bindings.Add(outerParameter, sequence)) {
        var isOuter = false;
        if (collectionSelector.Body.NodeType == ExpressionType.Call) {
          var call = (MethodCallExpression) collectionSelector.Body;
          var method = call.Method;
          isOuter = method.IsGenericMethodSpecificationOf(WellKnownMembers.Queryable.DefaultIfEmpty)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.Enumerable.DefaultIfEmpty);
          if (isOuter) {
            collectionSelector = FastExpression.Lambda(call.Arguments[0], outerParameter);
          }
        }

        ProjectionExpression innerProjection;
        var outerParameters = State.OuterParameters
          .Concat(State.Parameters)
          .Concat(collectionSelector.Parameters)
          .Append(outerParameter)
          .ToArray(State.OuterParameters.Length + State.Parameters.Length + collectionSelector.Parameters.Count + 1);
        using (CreateScope(new TranslatorState(State) {
          OuterParameters = outerParameters,
          Parameters = Array.Empty<ParameterExpression>(),
          RequestCalculateExpressionsOnce = true
        })) {
          var visitedCollectionSelector = Visit(collectionSelector.Body);

          if (visitedCollectionSelector.IsGroupingExpression()) {
            var selectManyInfo = ((GroupingExpression) visitedCollectionSelector).SelectManyInfo;
            if (selectManyInfo.GroupByProjection == null) {
              var rewriteSucceeded = SelectManySelectorRewriter.TryRewrite(
                resultSelector,
                resultSelector.Parameters[0],
                selectManyInfo.GroupJoinOuterKeySelector.Parameters[0],
                out var newResultSelector);

              if (rewriteSucceeded) {
                return VisitJoin(
                  selectManyInfo.GroupJoinOuterProjection,
                  selectManyInfo.GroupJoinInnerProjection,
                  selectManyInfo.GroupJoinOuterKeySelector,
                  selectManyInfo.GroupJoinInnerKeySelector,
                  newResultSelector,
                  isOuter,
                  expressionPart);
              }
            }
            else {
              if (resultSelector == null) {
                return selectManyInfo.GroupByProjection;
              }

              throw new NotImplementedException();
            }
          }

          var projection = VisitSequence(visitedCollectionSelector, collectionSelector);
          var innerItemProjector = projection.ItemProjector;
          if (isOuter) {
            innerItemProjector = innerItemProjector.SetDefaultIfEmpty();
          }

          innerProjection = projection.ApplyItemProjector(innerItemProjector);
        }

        var outerProjection = context.Bindings[outerParameter];
        var applyParameter = context.GetApplyParameter(outerProjection);
        var recordSet = outerProjection.ItemProjector.DataSource.Apply(
          applyParameter,
          innerProjection.ItemProjector.DataSource.Alias(context.GetNextAlias()),
          false,
          ApplySequenceType.All,
          isOuter ? JoinType.LeftOuter : JoinType.Inner);

        if (resultSelector == null) {
          var innerParameter =
            Expression.Parameter(SequenceHelper.GetElementType(collectionSelector.Body.Type), "inner");
          resultSelector = FastExpression.Lambda(innerParameter, outerParameter, innerParameter);
        }

        var resultProjection = CombineProjections(outerProjection, innerProjection, recordSet, resultSelector);
        var resultItemProjector = resultProjection.ItemProjector.RemoveOuterParameter();
        resultProjection = resultProjection.ApplyItemProjector(resultItemProjector);
        return resultProjection;
      }
    }

    private ProjectionExpression VisitSelect(Expression expression, LambdaExpression le)
    {
      var sequence = VisitSequence(expression);
      if (le.Parameters.Count == 2) {
        var indexProjection = GetIndexBinding(le, ref sequence);
        context.Bindings.PermanentAdd(le.Parameters[1], indexProjection);
      }

      context.Bindings.PermanentAdd(le.Parameters[0], sequence);
      var calculateExpressions = State.RequestCalculateExpressions || State.RequestCalculateExpressionsOnce;
      using (CreateScope(new TranslatorState(State) {
        CalculateExpressions = calculateExpressions,
        RequestCalculateExpressionsOnce = false
      })) {
        return BuildProjection(le);
      }
    }

    private ProjectionExpression BuildProjection(LambdaExpression le)
    {
      using (CreateScope(new TranslatorState(State) { BuildingProjection = true })) {
        var itemProjector = (ItemProjectorExpression) VisitLambda(le);
        return new ProjectionExpression(
          WellKnownInterfaces.QueryableOfT.CachedMakeGenericType(le.Body.Type),
          itemProjector,
          TranslatedQuery.EmptyTupleParameterBindings);
      }
    }

    private ProjectionExpression VisitWhere(Expression expression, LambdaExpression le)
    {
      var parameter = le.Parameters[0];
      var visitedSource = VisitSequence(expression);
      var indexBinding = BindingCollection<ParameterExpression, ProjectionExpression>.BindingScope.Empty;
      if (le.Parameters.Count == 2) {
        var indexProjection = GetIndexBinding(le, ref visitedSource);
        indexBinding = context.Bindings.Add(le.Parameters[1], indexProjection);
      }

      using (indexBinding)
      using (context.Bindings.Add(parameter, visitedSource))
      using (CreateScope(new TranslatorState(State) { CalculateExpressions = false, CurrentLambda = le })) {
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
      var result = predicate == null
        ? VisitSequence(source)
        : VisitWhere(source, predicate);

      var existenceColumn = ColumnExpression.Create(WellKnownTypes.Bool, 0);
      var projectorBody = notExists
        ? Expression.Not(existenceColumn)
        : (Expression) existenceColumn;
      var newRecordSet = result.ItemProjector.DataSource.Existence(context.GetNextColumnAlias());
      var itemProjector = new ItemProjectorExpression(projectorBody, newRecordSet, context);
      return new ProjectionExpression(
        WellKnownTypes.Bool,
        itemProjector,
        result.TupleParameterBindings,
        ResultAccessMethod.Single);
    }

    private Expression VisitExists(Expression source, LambdaExpression predicate, bool notExists)
    {
      if (source.IsLocalCollection(context) && predicate != null && predicate.Body.NodeType == ExpressionType.Equal) {
        return VisitExistsAsInclude(source, predicate, notExists);
      }

      ProjectionExpression subquery;
      using (CreateScope(new TranslatorState(State) { CalculateExpressions = false })) {
        subquery = predicate == null
          ? VisitSequence(source)
          : VisitWhere(source, predicate);
      }

      var recordSet = subquery
        .ItemProjector
        .DataSource
        .Existence(context.GetNextColumnAlias());

      var filter = AddSubqueryColumn(WellKnownTypes.Bool, recordSet);
      if (notExists) {
        filter = Expression.Not(filter);
      }

      return filter;
    }

    private Expression VisitExistsAsInclude(Expression source, LambdaExpression predicate, bool notExists)
    {
      // Translate localCollection.Any(item => item==outer) as outer.In(localCollection)

      var parameter = predicate.Parameters[0];
      ProjectionExpression visitedSource;
      using (CreateScope(new TranslatorState(State) {
        TypeOfEntityStoredInKey = source.IsLocalCollection(context) && IsKeyCollection(source.Type)
              ? LocalCollectionKeyTypeExtractor.Extract((BinaryExpression) predicate.Body)
              : State.TypeOfEntityStoredInKey,
        IncludeAlgorithm = IncludeAlgorithm.Auto
      })) {
        visitedSource = VisitSequence(source);
      }

      var outerParameter = State.Parameters[0];
      using (context.Bindings.Add(parameter, visitedSource))
      using (CreateScope(new TranslatorState(State) { CalculateExpressions = false, CurrentLambda = predicate })) {
        ItemProjectorExpression predicateExpression;
        using (CreateScope(new TranslatorState(State) { IncludeAlgorithm = IncludeAlgorithm.Auto })) {
          predicateExpression = (ItemProjectorExpression) VisitLambda(predicate);
        }

        var predicateLambda = predicateExpression.ToLambda(context);

        RawProvider rawProvider;
        if (visitedSource.ItemProjector.DataSource is StoreProvider storeProvider) {
          rawProvider = (RawProvider) storeProvider.Source;
        }
        else {
          var joinProvider = (JoinProvider) visitedSource.ItemProjector.DataSource;
          rawProvider = (RawProvider) ((StoreProvider) joinProvider.Left).Source;
        }

        var filterColumnCount = rawProvider.Header.Length;
        var filteredTuple = context.GetApplyParameter(context.Bindings[outerParameter]);

        // Mapping from filter data column to expression that requires filtering
        var filteredColumnMappings = IncludeFilterMappingGatherer.Gather(
          predicateLambda.Body, predicateLambda.Parameters[0], filteredTuple, filterColumnCount);

        // Mapping from filter data column to filtered column
        var filteredColumns = new int[filterColumnCount];
        for (var i = 0; i < filterColumnCount; i++) {
          var mapping = filteredColumnMappings[i];
          if (mapping.ColumnIndex >= 0) {
            filteredColumns[i] = mapping.ColumnIndex;
          }
          else {
            var descriptor = CreateCalculatedColumnDescriptor(mapping.CalculatedColumn);
            var column = AddCalculatedColumn(outerParameter, descriptor, mapping.CalculatedColumn.Body.Type);
            filteredColumns[i] = column.Mapping.Offset;
          }
        }

        var outerResult = context.Bindings[outerParameter];
        var columnIndex = outerResult.ItemProjector.DataSource.Header.Length;
        var newDataSource = outerResult.ItemProjector.DataSource
          .Include(State.IncludeAlgorithm, true, rawProvider.Source, context.GetNextAlias(), filteredColumns);

        var newItemProjector = outerResult.ItemProjector.Remap(newDataSource, 0);
        var newOuterResult = outerResult.ApplyItemProjector(newItemProjector);
        context.Bindings.ReplaceBound(outerParameter, newOuterResult);
        Expression resultExpression = ColumnExpression.Create(WellKnownTypes.Bool, columnIndex);
        if (notExists) {
          resultExpression = Expression.Not(resultExpression);
        }

        return resultExpression;
      }
    }

    private Expression VisitIn(MethodCallExpression mc)
    {
      var algorithm = IncludeAlgorithm.Auto;
      Expression source = null;
      Expression match = null;
      var arguments = mc.Arguments;
      switch (arguments.Count) {
        case 2:
          source = mc.Arguments[1];
          match = mc.Arguments[0];
          break;
        case 3:
          source = arguments[2];
          match = arguments[0];
          algorithm = (IncludeAlgorithm) ExpressionEvaluator.Evaluate(arguments[1]).Value;
          break;
        default:
          Exceptions.InternalError(string.Format(Strings.ExUnknownInSyntax, mc.ToString(true)), OrmLog.Instance);
          break;
      }

      using (CreateScope(new TranslatorState(State) { IncludeAlgorithm = algorithm })) {
        return VisitContains(source, match, false);
      }
    }

    private Expression VisitSetOperations(Expression outerSource, Expression innerSource,
      QueryableMethodKind methodKind, Type elementType)
    {
      ProjectionExpression outer;
      ProjectionExpression inner;

      QueryHelper.TryAddConvarianceCast(ref outerSource, elementType);
      QueryHelper.TryAddConvarianceCast(ref innerSource, elementType);

      using (CreateScope(new TranslatorState(State) {
        JoinLocalCollectionEntity = true,
        CalculateExpressions = true,
        RequestCalculateExpressions = true
      })) {
        outer = VisitSequence(outerSource);
        inner = VisitSequence(innerSource);
      }

      var outerItemProjector = outer.ItemProjector.RemoveOwner();
      var innerItemProjector = inner.ItemProjector.RemoveOwner();
      var outerColumnList = outerItemProjector.GetColumns(ColumnExtractionModes.Distinct).ToList();
      var innerColumnList = innerItemProjector.GetColumns(ColumnExtractionModes.Distinct).ToList();

      int[] outerColumns, innerColumns;
      if (!outerColumnList.Except(innerColumnList).Any() && outerColumnList.Count == innerColumnList.Count) {
        var outerColumnListCopy = outerColumnList.ToArray();
        Array.Sort(outerColumnListCopy);
        outerColumns = outerColumnListCopy;

        var innerColumnListCopy = innerColumnList.ToArray();
        Array.Sort(innerColumnListCopy);
        innerColumns = innerColumnListCopy;
      }
      else {
        outerColumns = outerColumnList.ToArray();
        innerColumns = innerColumnList.ToArray();
      }

      var outerRecordSet = ShouldWrapDataSourceWithSelect(outerItemProjector, outerColumns)
        ? outerItemProjector.DataSource.Select(outerColumns)
        : outerItemProjector.DataSource;
      var innerRecordSet = ShouldWrapDataSourceWithSelect(innerItemProjector, innerColumns)
        ? innerItemProjector.DataSource.Select(innerColumns)
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

      var tupleParameterBindings = outer.TupleParameterBindings.Union(inner.TupleParameterBindings)
        .ToDictionary(pair => pair.Key, pair => pair.Value);
      var itemProjector = outerItemProjector.Remap(recordSet, outerColumns);
      return new ProjectionExpression(outer.Type, itemProjector, tupleParameterBindings);
    }

    private bool ShouldWrapDataSourceWithSelect(ItemProjectorExpression expression, IReadOnlyList<int> columns) =>
      expression.DataSource.Type != ProviderType.Select
      || expression.DataSource.Header.Length != columns.Count
      || columns.Select((c, i) => (c, i)).Any(x => x.c != x.i);

    private Expression AddSubqueryColumn(Type columnType, CompilableProvider subquery)
    {
      if (subquery.Header.Length != 1) {
        throw Exceptions.InternalError(string.Format(Strings.SubqueryXHeaderMustHaveOnlyOneColumn, subquery),
          OrmLog.Instance);
      }

      var lambdaParameter = State.Parameters[0];
      var oldResult = context.Bindings[lambdaParameter];
      var dataSource = oldResult.ItemProjector.DataSource;
      var applyParameter = context.GetApplyParameter(oldResult.ItemProjector.DataSource);
      var columnIndex = dataSource.Header.Length;
      var newRecordSet = dataSource.Apply(
        applyParameter, subquery, !State.BuildingProjection, ApplySequenceType.Single, JoinType.Inner);
      var newItemProjector = oldResult.ItemProjector.Remap(newRecordSet, 0);
      var newResult = new ProjectionExpression(oldResult.Type, newItemProjector, oldResult.TupleParameterBindings);
      context.Bindings.ReplaceBound(lambdaParameter, newResult);
      return ColumnExpression.Create(columnType, columnIndex);
    }

    private ProjectionExpression VisitSequence(Expression sequenceExpression) =>
      VisitSequence(sequenceExpression, sequenceExpression);

    private ProjectionExpression VisitSequence(Expression sequenceExpression, Expression expressionPart)
    {
      var sequence = sequenceExpression.StripCasts();

      if (compiledQueryScope != null && QueryHelper.IsDirectEntitySetQuery(sequence)) {
        throw new NotSupportedException(
          Strings.ExDirectQueryingForEntitySetInCompiledQueriesIsNotSupportedUseQueryEndpointItemsInstead);
      }

      if (WellKnownOrmTypes.EntitySetBase.IsAssignableFrom(sequence.StripMarkers().Type)) {
        if (sequence.NodeType == ExpressionType.MemberAccess) {
          var memberAccess = (MemberExpression) sequence;
          if (memberAccess.Member is PropertyInfo propertyInfo
            && memberAccess.Expression != null
            && context.Model.Types.TryGetValue(memberAccess.Expression.Type, out var ti)) {
            var field = ti
              .Fields[context.Domain.Handlers.NameBuilder.BuildFieldName(propertyInfo)];
            sequenceExpression = QueryHelper.CreateEntitySetQuery(memberAccess.Expression, field, context.Domain);
          }
        }
      }

      if (sequence.IsLocalCollection(context)) {
        var sequenceType = sequence.Type.IsGenericType && sequence.Type.IsOfGenericType(GenericFuncDefType)
          ? sequence.Type.GetGenericArguments()[0]
          : sequence.Type;

        var itemType = QueryHelper.GetSequenceElementType(sequenceType);
        return (ProjectionExpression) VisitLocalCollectionSequenceMethod
          .CachedMakeGenericMethod(itemType)
          .Invoke(this, new object[] { sequence });
      }

      var visitedExpression = Visit(sequenceExpression).StripCasts();
      ProjectionExpression result = null;

      if (visitedExpression.IsGroupingExpression() || visitedExpression.IsSubqueryExpression()) {
        result = ((SubQueryExpression) visitedExpression).ProjectionExpression;
      }

      if (visitedExpression.IsEntitySetExpression()) {
        var entitySetExpression = (EntitySetExpression) visitedExpression;
        var entitySetQuery =
          QueryHelper.CreateEntitySetQuery((Expression) entitySetExpression.Owner, entitySetExpression.Field, context.Domain);
        result = (ProjectionExpression) Visit(entitySetQuery);
      }

      if (visitedExpression.IsProjection()) {
        result = (ProjectionExpression) visitedExpression;
      }

      if (result != null) {
        var projectorExpression = result.ItemProjector.EnsureEntityIsJoined();
        if (projectorExpression != result.ItemProjector) {
          result = result.ApplyItemProjector(projectorExpression);
        }

        return result;
      }

      throw new InvalidOperationException(
        string.Format(Strings.ExExpressionXIsNotASequence, expressionPart.ToString(true)));
    }

    private ProjectionExpression VisitLocalCollectionSequence<TItem>(Expression sequence)
    {
      Func<ParameterContext, IEnumerable<TItem>> collectionGetter;
      if (compiledQueryScope != null) {
        var replacer = compiledQueryScope.QueryParameterReplacer;
        var replace = replacer.Replace(sequence);
        var parameter = ParameterAccessorFactory.CreateAccessorExpression<IEnumerable<TItem>>(replace);
        collectionGetter = parameter.CachingCompile();
      }
      else {
        var parameter = ParameterAccessorFactory.CreateAccessorExpression<IEnumerable<TItem>>(sequence);
        collectionGetter = parameter.CachingCompile();
      }
      return CreateLocalCollectionProjectionExpression(typeof(TItem), collectionGetter, this, sequence);
    }

    private Expression VisitContainsAny(Expression setA, Expression setB, bool isRoot, Type elementType)
    {
      QueryHelper.TryAddConvarianceCast(ref setA, elementType);
      QueryHelper.TryAddConvarianceCast(ref setB, elementType);

      var setAIsQuery = setA.IsQuery();
      var parameter = Expression.Parameter(elementType, "a");
      var containsMethod = WellKnownMembers.Enumerable.Contains.CachedMakeGenericMethod(elementType);

      if (setAIsQuery) {
        var lambda = FastExpression.Lambda(Expression.Call(containsMethod, setB, parameter), parameter);
        return VisitAny(setA, lambda, isRoot);
      }
      else {
        var lambda = FastExpression.Lambda(Expression.Call(containsMethod, setA, parameter), parameter);
        return VisitAny(setB, lambda, isRoot);
      }
    }

    private Expression VisitContainsAll(Expression setA, Expression setB, bool isRoot, Type elementType)
    {
      QueryHelper.TryAddConvarianceCast(ref setA, elementType);
      QueryHelper.TryAddConvarianceCast(ref setB, elementType);

      var parameter = Expression.Parameter(elementType, "a");
      var containsMethod = WellKnownMembers.Enumerable.Contains.CachedMakeGenericMethod(elementType);

      var lambda = FastExpression.Lambda(Expression.Call(containsMethod, setA, parameter), parameter);
      return VisitAll(setB, lambda, isRoot);
    }

    private Expression VisitContainsNone(Expression setA, Expression setB, bool isRoot, Type elementType)
    {
      QueryHelper.TryAddConvarianceCast(ref setA, elementType);
      QueryHelper.TryAddConvarianceCast(ref setB, elementType);

      var setAIsQuery = setA.IsQuery();
      var parameter = Expression.Parameter(elementType, "a");
      var containsMethod = WellKnownMembers.Enumerable.Contains.CachedMakeGenericMethod(elementType);
      if (setAIsQuery) {
        var lambda = FastExpression.Lambda(Expression.Not(Expression.Call(containsMethod, setB, parameter)), parameter);
        return VisitAll(setA, lambda, isRoot);
      }
      else {
        var lambda = FastExpression.Lambda(Expression.Not(Expression.Call(containsMethod, setA, parameter)), parameter);
        return VisitAll(setB, lambda, isRoot);
      }
    }

    private static ICollection<int> GetNullableGroupingExpressions(List<Pair<int, Expression>> keyFieldsRaw)
    {
      var nullableFields = new HashSet<int>();

      foreach (var pair in keyFieldsRaw) {
        var index = pair.First;
        var expression = pair.Second;

        if (expression is FieldExpression fieldExpression && fieldExpression.Field.IsNullable) {
          _ = nullableFields.Add(index);
        }

        if (expression is EntityExpression entityExpression && entityExpression.IsNullable) {
          _ = nullableFields.Add(index);
        }
      }

      return nullableFields;
    }

    private bool IsKeyCollection(Type localCollectionType)
    {
      return (localCollectionType.IsArray && localCollectionType.GetElementType() == WellKnownOrmTypes.Key)
        || IEnumerableOfKeyType.IsAssignableFrom(localCollectionType);
    }

    internal void RestoreState(in TranslatorState previousState) =>
      State = previousState;

    private TranslatorState.TranslatorScope CreateScope(in TranslatorState newState)
    {
      var scope = new TranslatorState.TranslatorScope(this);
      State = newState;
      return scope;
    }

    private TranslatorState.TranslatorScope CreateLambdaScope(LambdaExpression le, bool allowCalculableColumnCombine)
    {
      var newOuterParameters = new ParameterExpression[State.OuterParameters.Length + State.Parameters.Length];
      State.OuterParameters.CopyTo(newOuterParameters, 0);
      State.Parameters.CopyTo(newOuterParameters, State.OuterParameters.Length);
      return CreateScope(new TranslatorState(State) {
        OuterParameters = newOuterParameters,
        Parameters = le.Parameters.ToArray(le.Parameters.Count),
        CurrentLambda = le,
        AllowCalculableColumnCombine = allowCalculableColumnCombine
      });
    }

    private void ModifyStateAllowCalculableColumnCombine(bool b) =>
      State = new TranslatorState(State) { AllowCalculableColumnCombine = b };
  }
}