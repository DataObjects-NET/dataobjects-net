// Copyright (C) 2009-2020 Xtensive LLC.
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
    private static readonly ParameterExpression parameterContext = Expression.Parameter(WellKnownOrmTypes.ParameterContext, "parameterContext");
    private static readonly ParameterExpression tupleReader = Expression.Parameter(typeof(RecordSetReader), "tupleReader");
    private static readonly ParameterExpression session = Expression.Parameter(typeof(Session), "session");

    private readonly CompiledQueryProcessingScope compiledQueryScope;
    public static readonly MethodInfo TranslateMethod;
    private static readonly MethodInfo VisitLocalCollectionSequenceMethod;

    public TranslatedQuery Translate()
    {
      var projection = (ProjectionExpression) Visit(context.Query);
      return Translate(projection, Enumerable.Empty<Parameter<Tuple>>());
    }

    private TranslatedQuery Translate(ProjectionExpression projection,
      IEnumerable<Parameter<Tuple>> tupleParameterBindings)
    {
      var newItemProjector = projection.ItemProjector.EnsureEntityIsJoined();
      var result = new ProjectionExpression(
        projection.Type,
        newItemProjector,
        projection.TupleParameterBindings,
        projection.ResultAccessMethod);
      var optimized = Optimize(result);

      // Prepare cached query, if required
      var prepared = compiledQueryScope!=null
        ? PrepareCachedQuery(optimized, compiledQueryScope)
        : optimized;

      // Compilation
      var dataSource = prepared.ItemProjector.DataSource;
      var compiled = context.Domain.Handler.CompilationService.Compile(dataSource, context.RseCompilerConfiguration);

      // Build materializer
      var materializer = BuildMaterializer(prepared, tupleParameterBindings);
      var translatedQuery = new TranslatedQuery(
        compiled, materializer, prepared.ResultAccessMethod,
        projection.TupleParameterBindings, tupleParameterBindings);

      // Providing the result to caching layer, if required
      if (compiledQueryScope != null && translatedQuery.TupleParameters.Count == 0) {
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

      if (usedColumns.Count==0)
        usedColumns.Add(0);
      if (usedColumns.Count < origin.ItemProjector.DataSource.Header.Length) {
        var resultProvider = new SelectProvider(originProvider, usedColumns.ToArray());
        var itemProjector = origin.ItemProjector.Remap(resultProvider, usedColumns.ToArray());
        var result = new ProjectionExpression(
          origin.Type,
          itemProjector,
          origin.TupleParameterBindings,
          origin.ResultAccessMethod);
        return result;
      }
      return origin;
    }

    private static ProjectionExpression PrepareCachedQuery(
      ProjectionExpression origin, CompiledQueryProcessingScope compiledQueryScope)
    {
      if (compiledQueryScope.QueryParameter!=null) {
        var result = compiledQueryScope.QueryParameterReplacer.Replace(origin);
        return (ProjectionExpression) result;
      }
      return origin;
    }

    private Materializer
      BuildMaterializer(ProjectionExpression projection, IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      var itemProjector = projection.ItemProjector;
      var materializationInfo = itemProjector.Materialize(context, tupleParameters);
      var elementType = itemProjector.Item.Type;
      var materializeMethod = MaterializationHelper.MaterializeMethodInfo.MakeGenericMethod(elementType);
      var itemMaterializerFactoryMethod =
        elementType.IsNullable()
          ? MaterializationHelper.CreateNullableItemMaterializerMethodInfo.MakeGenericMethod(
            elementType.GetGenericArguments()[0])
          : MaterializationHelper.CreateItemMaterializerMethodInfo.MakeGenericMethod(elementType);

      var itemMaterializer = itemMaterializerFactoryMethod.Invoke(
        null, new object[] {materializationInfo.Expression, itemProjector.AggregateType});
      Expression<Func<Session, int, MaterializationContext>> materializationContextCtor =
        (s, entityCount) => new MaterializationContext(s, entityCount);
      var materializationContextExpression = materializationContextCtor
        .BindParameters(session, Expression.Constant(materializationInfo.EntitiesInRow));

      Expression body = Expression.Call(
        materializeMethod,
        tupleReader,
        materializationContextExpression,
        parameterContext,
        Expression.Constant(itemMaterializer));

      var projectorExpression = FastExpression.Lambda<Func<RecordSetReader, Session, ParameterContext, object>>(
        body, tupleReader, session, parameterContext);
      return new Materializer(projectorExpression.CachingCompile());
    }

    private List<Expression> VisitNewExpressionArguments(NewExpression n)
    {
      var arguments = new List<Expression>();
      foreach (var argument in n.Arguments) {
        Expression body;
        using (CreateScope(new TranslatorState(state) { CalculateExpressions = false })) {
          body = Visit(argument);
        }
        body = body.IsProjection()
          ? BuildSubqueryResult((ProjectionExpression) body, argument.Type)
          : ProcessProjectionElement(body);
        arguments.Add(body);
      }
      var constructorParameters = n.GetConstructorParameters();
      for (int i = 0; i < arguments.Count; i++) {
        if (arguments[i].Type!=constructorParameters[i].ParameterType)
          arguments[i] = Expression.Convert(arguments[i], constructorParameters[i].ParameterType);
      }
      return arguments;
    }

    private ProjectionExpression GetIndexBinding(LambdaExpression le, ref ProjectionExpression sequence)
    {
      if (le.Parameters.Count==2) {
        var indexDataSource = sequence.ItemProjector.DataSource.RowNumber(context.GetNextColumnAlias());
        var columnExpression = ColumnExpression.Create(WellKnownTypes.Int64, indexDataSource.Header.Columns.Count - 1);
        var indexExpression = Expression.Subtract(columnExpression, Expression.Constant(1L));
        var itemExpression = Expression.Convert(indexExpression, WellKnownTypes.Int32);
        var indexItemProjector = new ItemProjectorExpression(itemExpression, indexDataSource, context);
        var indexProjectionExpression = new ProjectionExpression(WellKnownTypes.Int64, indexItemProjector, sequence.TupleParameterBindings);
        var sequenceItemProjector = sequence.ItemProjector.Remap(indexDataSource, 0);
        sequence = new ProjectionExpression(
          sequence.Type,
          sequenceItemProjector,
          sequence.TupleParameterBindings,
          sequence.ResultAccessMethod);
        return indexProjectionExpression;
      }
      return null;
    }

    private Expression VisitQuerySingle(MethodCallExpression mc)
    {
      var returnType = mc.Method.ReturnType;

      var argument = mc.Arguments[0];
      var queryAll = Expression.Call(null, WellKnownMembers.Query.All.MakeGenericMethod(returnType));
      var source = ConstructQueryable(queryAll);
      var parameter = Expression.Parameter(returnType, "entity");
      var keyAccessor = Expression.MakeMemberAccess(parameter, WellKnownMembers.IEntityKey);
      var equility = Expression.Equal(keyAccessor, argument);
      var lambda = FastExpression.Lambda(equility, parameter);
      return VisitFirstSingle(source, lambda, mc.Method, false);
    }

    // Constructors

    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    internal Translator(TranslatorContext context, CompiledQueryProcessingScope compiledQueryScope)
    {
      this.compiledQueryScope = compiledQueryScope;
      this.context = context;
    }

    static Translator()
    {
      TranslateMethod = typeof(Translator).GetMethod(nameof(Translate),
        BindingFlags.NonPublic | BindingFlags.Instance,
        Array.Empty<string>(),
        new object[] {WellKnownOrmTypes.ProjectionExpression, typeof(IEnumerable<Parameter<Tuple>>)});

      VisitLocalCollectionSequenceMethod = typeof(Translator).GetMethod(nameof(VisitLocalCollectionSequence),
        BindingFlags.NonPublic | BindingFlags.Instance,
        new[] {"TItem"},
        new object[] {WellKnownTypes.Expression});
    }
  }
}