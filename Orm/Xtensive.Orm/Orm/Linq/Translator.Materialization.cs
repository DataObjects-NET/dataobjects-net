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
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Parameters;
using Xtensive.Reflection;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Linq
{
  internal sealed partial class Translator
  {
    public static readonly MethodInfo TranslateMethodInfo;
    public static readonly MethodInfo VisitLocalCollectionSequenceMethodInfo;

    public TranslationResult<TResult> Translate<TResult>()
    {
      var projection = (ProjectionExpression) Visit(context.Query);
      return Translate<TResult>(projection, EnumerableUtils<Parameter<Tuple>>.Empty);
    }

    private TranslationResult<TResult> Translate<TResult>(ProjectionExpression projection, IEnumerable<Parameter<Tuple>> tupleParameterBindings)
    {
      var newItemProjector = projection.ItemProjector.EnsureEntityIsJoined();
      var result = new ProjectionExpression(
        projection.Type,
        newItemProjector,
        projection.TupleParameterBindings,
        projection.ResultType);
      var optimized = Optimize(result);

      // Prepare cached query, if required
      var cachingScope = QueryCachingScope.Current;
      var prepared = cachingScope!=null
        ? PrepareCachedQuery(optimized, cachingScope)
        : optimized;

      // Compilation
      var dataSource = prepared.ItemProjector.DataSource;
      var compiled = context.Domain.Handler.CompilationService.Compile(dataSource.Provider);

      // Build materializer
      var materializer = BuildMaterializer<TResult>(prepared, tupleParameterBindings);
      var translatedQuery = new TranslatedQuery<TResult>(compiled, materializer, projection.TupleParameterBindings, tupleParameterBindings);

      // Providing the result to caching layer, if required
      if (cachingScope != null && translatedQuery.TupleParameters.Count == 0) {
        var parameterizedQuery = new ParameterizedQuery<TResult>(
          translatedQuery,
          cachingScope.QueryParameter);
        cachingScope.ParameterizedQuery = parameterizedQuery;
        return new TranslationResult<TResult>(parameterizedQuery, dataSource);
      }
      return new TranslationResult<TResult>(translatedQuery, dataSource);
    }

    private static ProjectionExpression Optimize(ProjectionExpression origin)
    {
      var originProvider = origin.ItemProjector.DataSource.Provider;

      var usedColumns = origin.ItemProjector
        .GetColumns(ColumnExtractionModes.KeepSegment | ColumnExtractionModes.KeepTypeId | ColumnExtractionModes.OmitLazyLoad)
        .ToList();

      if (usedColumns.Count==0)
        usedColumns.Add(0);
      if (usedColumns.Count < origin.ItemProjector.DataSource.Header.Length) {
        var resultProvider = new SelectProvider(originProvider, usedColumns.ToArray());
        var rs = resultProvider.Result;
        var itemProjector = origin.ItemProjector.Remap(rs, usedColumns.ToArray());
        var result = new ProjectionExpression(
          origin.Type,
          itemProjector,
          origin.TupleParameterBindings,
          origin.ResultType);
        return result;
      }
      return origin;
    }

    private static ProjectionExpression PrepareCachedQuery(ProjectionExpression origin, QueryCachingScope cachingScope)
    {
      if (cachingScope.QueryParameter!=null) {
        var result = cachingScope.QueryParameterReplacer.Replace(origin);
        return (ProjectionExpression) result;
      }
      return origin;
    }

    private Func<IEnumerable<Tuple>, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, TResult> BuildMaterializer<TResult>(ProjectionExpression projection, IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      var rs = Expression.Parameter(typeof(IEnumerable<Tuple>), "rs");
      var session = Expression.Parameter(typeof(Session), "session");
      var tupleParameterBindings = Expression.Parameter(typeof(Dictionary<Parameter<Tuple>, Tuple>), "tupleParameterBindings");
      var parameterContext = Expression.Parameter(typeof(ParameterContext), "parameterContext");
      
      var itemProjector = projection.ItemProjector;
      var materializationInfo = itemProjector.Materialize(context, tupleParameters);
      var elementType = itemProjector.Item.Type;
      var materializeMethod = MaterializationHelper.MaterializeMethodInfo
        .MakeGenericMethod(elementType);
      var compileMaterializerMethod = MaterializationHelper.CompileItemMaterializerMethodInfo
        .MakeGenericMethod(elementType);

      var itemMaterializer = compileMaterializerMethod.Invoke(null, new[] {materializationInfo.Expression});
      Expression<Func<Session, int, MaterializationContext>> materializationContextCtor = (s,n) => new MaterializationContext(s,n);
      var materializationContextExpression = materializationContextCtor
        .BindParameters(
          session,
          Expression.Constant(materializationInfo.EntitiesInRow));

      Expression body = Expression.Call(
        materializeMethod,
        rs,
        materializationContextExpression,
        parameterContext,        
        Expression.Constant(itemMaterializer),
        tupleParameterBindings);

      if (projection.IsScalar) {
        var scalarMethodName = projection.ResultType.ToString();
        var enumerableMethod = typeof (Enumerable)
          .GetMethods(BindingFlags.Static | BindingFlags.Public)
          .First(m => m.Name==scalarMethodName && m.GetParameters().Length==1)
          .MakeGenericMethod(elementType);
        body = Expression.Call(enumerableMethod, body);
      }
      body = body.Type==typeof (TResult)
        ? body
        : Expression.Convert(body, typeof (TResult));

      var projectorExpression = Expression.Lambda<Func<IEnumerable<Tuple>, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, TResult>>(body, rs, session, tupleParameterBindings, parameterContext);
      return projectorExpression.CachingCompile();
    }


    // Constructors

    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    internal Translator(TranslatorContext context)
    {
      this.context = context;
      state = new TranslatorState(this);
    }

    static Translator()
    {
      TranslateMethodInfo = typeof (Translator)
        .GetMethod("Translate", BindingFlags.NonPublic | BindingFlags.Instance, new[] {"TResult"}, new[] {typeof (ProjectionExpression), typeof (IEnumerable<Parameter<Tuple>>)});
      VisitLocalCollectionSequenceMethodInfo = typeof (Translator)
        .GetMethod("VisitLocalCollectionSequence", BindingFlags.NonPublic | BindingFlags.Instance, new[] {"TItem"}, new[] {typeof (Expression)});
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
      var constructorParameters = n.Constructor.GetParameters();
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
        var indexExpression = Expression.Subtract(columnExpression, Expression.Constant(1l));
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
  }
}