// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    private readonly CompiledQueryProcessingScope compiledQueryScope;
    public static readonly MethodInfo TranslateMethod;
    private static readonly MethodInfo VisitLocalCollectionSequenceMethod;

    public TranslatedQuery<TResult> Translate<TResult>()
    {
      var projection = (ProjectionExpression) Visit(context.Query);
      return Translate<TResult>(projection, Enumerable.Empty<Parameter<Tuple>>());
    }

    private TranslatedQuery<TResult> Translate<TResult>(ProjectionExpression projection,
      IEnumerable<Parameter<Tuple>> tupleParameterBindings)
    {
      var newItemProjector = projection.ItemProjector.EnsureEntityIsJoined();
      var result = new ProjectionExpression(
        projection.Type,
        newItemProjector,
        projection.TupleParameterBindings,
        projection.ResultType);
      var optimized = Optimize(result);

      // Prepare cached query, if required
      var prepared = compiledQueryScope!=null
        ? PrepareCachedQuery(optimized, compiledQueryScope)
        : optimized;

      // Compilation
      var dataSource = prepared.ItemProjector.DataSource;
      var compiled = context.Domain.Handler.CompilationService.Compile(dataSource, context.RseCompilerConfiguration);

      // Build materializer
      var materializer = BuildMaterializer<TResult>(prepared, tupleParameterBindings);
      var translatedQuery = new TranslatedQuery<TResult>(compiled, materializer, projection.TupleParameterBindings, tupleParameterBindings);

      // Providing the result to caching layer, if required
      if (compiledQueryScope != null && translatedQuery.TupleParameters.Count == 0) {
        var parameterizedQuery = new ParameterizedQuery<TResult>(
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
          origin.ResultType);
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

    private Func<object, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, bool, TResult>
      BuildMaterializer<TResult>(ProjectionExpression projection, IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      var rs = Expression.Parameter(typeof (object), "rs");
      var session = Expression.Parameter(typeof (Session), "session");
      var isAsync = Expression.Parameter(typeof (bool), "isAsync");
      var tupleParameterBindings = Expression.Parameter(typeof (Dictionary<Parameter<Tuple>, Tuple>), "tupleParameterBindings");
      var parameterContext = Expression.Parameter(typeof (ParameterContext), "parameterContext");
      
      var itemProjector = projection.ItemProjector;
      var materializationInfo = itemProjector.Materialize(context, tupleParameters);
      var elementType = itemProjector.Item.Type;
      var materializeMethod = MaterializationHelper.MaterializeMethodInfo.MakeGenericMethod(elementType);
      var compileMaterializerMethod = MaterializationHelper.CompileItemMaterializerMethodInfo
        .MakeGenericMethod(elementType);

      var itemMaterializer = compileMaterializerMethod.Invoke(null, new object[] {materializationInfo.Expression});
      Expression<Func<Session, int, bool, MaterializationContext>> materializationContextCtor =
        (s, entityCount, isAsync) => new MaterializationContext(s, entityCount, isAsync);
      var materializationContextExpression = materializationContextCtor
        .BindParameters(session, Expression.Constant(materializationInfo.EntitiesInRow), isAsync);

      Expression body = Expression.Call(
        materializeMethod,
        rs,
        materializationContextExpression,
        parameterContext,
        Expression.Constant(itemMaterializer),
        tupleParameterBindings);

      if (projection.IsScalar) {
        var materializerResultType = typeof(IEnumerable<>).MakeGenericType(elementType);
        var scalarMethodName = projection.ResultType.ToString();
        var enumerableMethod = typeof (Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name==scalarMethodName && m.GetParameters().Length==1)
          .MakeGenericMethod(elementType);
        body = Expression.Call(enumerableMethod, Expression.Convert(body, materializerResultType));
      }

      var resultType = typeof (TResult);
      body = body.Type == resultType ? body : Expression.Convert(body, resultType);

      var projectorExpression = FastExpression
        .Lambda<Func<object, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, bool, TResult>>(
          body, rs, session, tupleParameterBindings, parameterContext, isAsync);
      return projectorExpression.CachingCompile();
    }

    private List<Expression> VisitNewExpressionArguments(NewExpression n)
    {
      var arguments = new List<Expression>();
      foreach (var argument in n.Arguments) {
        Expression body;
        using (state.CreateScope()) {
          state.CalculateExpressions = false;
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
        var columnExpression = ColumnExpression.Create(typeof (long), indexDataSource.Header.Columns.Count - 1);
        var indexExpression = Expression.Subtract(columnExpression, Expression.Constant(1L));
        var itemExpression = Expression.Convert(indexExpression, typeof (int));
        var indexItemProjector = new ItemProjectorExpression(itemExpression, indexDataSource, context);
        var indexProjectionExpression = new ProjectionExpression(typeof (long), indexItemProjector, sequence.TupleParameterBindings);
        var sequenceItemProjector = sequence.ItemProjector.Remap(indexDataSource, 0);
        sequence = new ProjectionExpression(
          sequence.Type, 
          sequenceItemProjector, 
          sequence.TupleParameterBindings, 
          sequence.ResultType);
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
      state = new TranslatorState(this);
    }

    static Translator()
    {
      TranslateMethod = typeof(Translator).GetMethod(nameof(Translate),
        BindingFlags.NonPublic | BindingFlags.Instance,
        new[] {"TResult"},
        new object[] {typeof(ProjectionExpression), typeof(IEnumerable<Parameter<Tuple>>)});

      VisitLocalCollectionSequenceMethod = typeof(Translator).GetMethod(nameof(VisitLocalCollectionSequence),
        BindingFlags.NonPublic | BindingFlags.Instance,
        new[] {"TItem"},
        new object[] {typeof(Expression)});
    }
  }
}