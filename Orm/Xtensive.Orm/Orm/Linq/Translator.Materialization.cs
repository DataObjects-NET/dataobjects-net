// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.05.28

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq
{
  internal sealed partial class Translator
  {
    private static readonly MethodInfo VisitLocalCollectionSequenceMethod = typeof(Translator).GetMethodEx(nameof(VisitLocalCollectionSequence),
        BindingFlags.NonPublic | BindingFlags.Instance,
        new[] { "TItem" },
        new object[] { WellKnownTypes.Expression });

    private static readonly ParameterExpression ParameterContext = Expression.Parameter(WellKnownOrmTypes.ParameterContext, "parameterContext");
    private static readonly ParameterExpression TupleReader = Expression.Parameter(typeof(RecordSetReader), "tupleReader");
    private static readonly ParameterExpression Session = Expression.Parameter(typeof(Session), "session");

    private readonly CompiledQueryProcessingScope compiledQueryScope;

    public TranslatedQuery Translate()
    {
      var projection = (ProjectionExpression) Visit(context.Query);
      return Translate(projection, Array.Empty<Parameter<Tuple>>());
    }

    internal TranslatedQuery Translate(ProjectionExpression projection,
      IEnumerable<Parameter<Tuple>> tupleParameterBindings)
    {
      var result = projection;
      if (context.SessionTags != null)
        result = ApplySessionTags(result, context.SessionTags);
      var newItemProjector = result.ItemProjector.EnsureEntityIsJoined();
      result = result.ApplyItemProjector(newItemProjector);

      var optimized = Optimize(result);

      // Prepare cached query, if required
      var prepared = compiledQueryScope != null
        ? PrepareCachedQuery(optimized, compiledQueryScope)
        : optimized;

      // Compilation
      var dataSource = prepared.ItemProjector.DataSource;
      var compiled = context.Domain.Handler.CompilationService.Compile(dataSource, context.RseCompilerConfiguration);

      // Build materializer
      var materializer = BuildMaterializer(prepared, tupleParameterBindings);
      var translatedQuery = new TranslatedQuery(
        compiled, materializer, prepared.ResultAccessMethod,
        result.TupleParameterBindings, tupleParameterBindings);

      // Providing the result to caching layer, if required
      if (compiledQueryScope != null && !translatedQuery.TupleParameters.Any()) {
        var parameterizedQuery = new ParameterizedQuery(
          translatedQuery,
          compiledQueryScope.QueryParameter);
        compiledQueryScope.ParameterizedQuery = parameterizedQuery;
        return parameterizedQuery;
      }

      return translatedQuery;
    }

    private static ProjectionExpression Optimize(ProjectionExpression origin)
    {
      var originProvider = origin.ItemProjector.DataSource;

      var usedColumns = origin.ItemProjector
        .GetColumns(ColumnExtractionModes.KeepSegment | ColumnExtractionModes.KeepTypeId | ColumnExtractionModes.OmitLazyLoad)
        .ToList();

      if (usedColumns.Count == 0)
        usedColumns.Add(0);
      if (usedColumns.Count < origin.ItemProjector.DataSource.Header.Length) {
        var usedColumnsArray = usedColumns.ToArray();
        var resultProvider = new SelectProvider(originProvider, usedColumnsArray);
        var itemProjector = origin.ItemProjector.Remap(resultProvider, usedColumnsArray);
        var result = origin.ApplyItemProjector(itemProjector);
        return result;
      }
      return origin;
    }

    private static ProjectionExpression PrepareCachedQuery(
      ProjectionExpression origin, CompiledQueryProcessingScope compiledQueryScope)
    {
      if (compiledQueryScope.QueryParameter != null) {
        var result = compiledQueryScope.QueryParameterReplacer.Replace(origin);
        return (ProjectionExpression) result;
      }
      return origin;
    }

    private static ProjectionExpression ApplySessionTags(ProjectionExpression origin, IReadOnlyList<string> tags)
    {
      var currentProjection = origin;
      foreach (var tag in tags) {
        var projector = currentProjection.ItemProjector;
        var newDataSource = projector.DataSource.Tag(tag);
        var newItemProjector = new ItemProjectorExpression(projector.Item, newDataSource, projector.Context);
        currentProjection = currentProjection.ApplyItemProjector(newItemProjector);
      }
      return currentProjection;
    }

    private Materializer
      BuildMaterializer(ProjectionExpression projection, IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      var itemProjector = projection.ItemProjector;
      var materializationInfo = itemProjector.Materialize(context, tupleParameters);
      var elementType = itemProjector.Item.Type;
      var materializeMethod = MaterializationHelper.MaterializeMethodInfo.CachedMakeGenericMethod(elementType);
      var itemMaterializerFactoryMethod =
        elementType.IsNullable()
          ? MaterializationHelper.CreateNullableItemMaterializerMethodInfo.CachedMakeGenericMethod(
            elementType.GetGenericArguments()[0])
          : MaterializationHelper.CreateItemMaterializerMethodInfo.CachedMakeGenericMethod(elementType);

      var itemMaterializer = itemMaterializerFactoryMethod.Invoke(
        null, new object[] { materializationInfo.Expression, itemProjector.AggregateType });
      Expression<Func<Session, int, MaterializationContext>> materializationContextCtor =
        (s, entityCount) => new MaterializationContext(s, entityCount);
      var materializationContextExpression = materializationContextCtor
        .BindParameters(Session, Expression.Constant(materializationInfo.EntitiesInRow));

      Expression body = Expression.Call(
        materializeMethod,
        TupleReader,
        materializationContextExpression,
        ParameterContext,
        Expression.Constant(itemMaterializer));

      var projectorExpression = FastExpression.Lambda<Func<RecordSetReader, Session, ParameterContext, object>>(
        body, TupleReader, Session, ParameterContext);
      return new Materializer(projectorExpression.CachingCompile());
    }

    private List<Expression> VisitNewExpressionArguments(NewExpression n)
    {
      var arguments = new List<Expression>();
      var origArguments = n.Arguments;
      for (int i = 0, count = origArguments.Count; i < count; i++) {
        var argument = origArguments[i];

        Expression body;
        using (CreateScope(new TranslatorState(State) { CalculateExpressions = false })) {
          body = Visit(argument);
          if (argument.IsQuery()) {
            context.RegisterPossibleQueryReuse(n.Members[i]);
          }
        }
        body = body.IsProjection()
          ? BuildSubqueryResult((ProjectionExpression) body, argument.Type)
          : ProcessProjectionElement(body);
        arguments.Add(body);
      }
      var constructorParameters = n.GetConstructorParameters();
      for (int i = 0; i < arguments.Count; i++) {
        if (arguments[i].Type != constructorParameters[i].ParameterType)
          arguments[i] = Expression.Convert(arguments[i], constructorParameters[i].ParameterType);
      }
      return arguments;
    }

    private void VisitNewExpressionArgumentsSkipResults(NewExpression n)
    {
      var origArguments = n.Arguments;
      for (int i = 0, count = origArguments.Count; i < count; i++) {
        var argument = origArguments[i];

        Expression body;
        using (CreateScope(new TranslatorState(State) { CalculateExpressions = false })) {
          body = Visit(argument);
          if (argument.IsQuery()) {
            context.RegisterPossibleQueryReuse(n.Members[i]);
          }
        }
        body = body.IsProjection()
          ? BuildSubqueryResult((ProjectionExpression) body, argument.Type)
          : ProcessProjectionElement(body);
      }
    }

    private ProjectionExpression GetIndexBinding(LambdaExpression le, ref ProjectionExpression sequence)
    {
      if (le.Parameters.Count == 2) {
        var indexDataSource = sequence.ItemProjector.DataSource.RowNumber(context.GetNextColumnAlias());
        var columnExpression = ColumnExpression.Create(WellKnownTypes.Int64, indexDataSource.Header.Columns.Count - 1);
        var indexExpression = Expression.Subtract(columnExpression, Expression.Constant(1L));
        var itemExpression = Expression.Convert(indexExpression, WellKnownTypes.Int32);
        var indexItemProjector = new ItemProjectorExpression(itemExpression, indexDataSource, context);
        var indexProjectionExpression = new ProjectionExpression(WellKnownTypes.Int64, indexItemProjector, sequence.TupleParameterBindings);
        var sequenceItemProjector = sequence.ItemProjector.Remap(indexDataSource, 0);
        sequence = sequence.ApplyItemProjector(sequenceItemProjector);
        return indexProjectionExpression;
      }
      return null;
    }

    private Expression VisitQuerySingle(MethodCallExpression mc)
    {
      var returnType = mc.Method.ReturnType;
      var argument = mc.Arguments[0];

      var source = ConstructQueryable(returnType);
      var parameter = Expression.Parameter(returnType, "entity");
      var keyAccessor = Expression.MakeMemberAccess(parameter, WellKnownMembers.IEntityKey);
      var equility = Expression.Equal(keyAccessor, argument);
      var lambda = FastExpression.Lambda(equility, parameter);
      return VisitFirstSingle(source, lambda, mc.Method, false);
    }

    // Constructors

    /// <exception cref="InvalidOperationException">There is no current <see cref="Orm.Session"/>.</exception>
    internal Translator(TranslatorContext context, CompiledQueryProcessingScope compiledQueryScope)
    {
      this.compiledQueryScope = compiledQueryScope;
      this.context = context;
      tagsEnabled = context.Domain.TagsEnabled;
    }
  }
}
