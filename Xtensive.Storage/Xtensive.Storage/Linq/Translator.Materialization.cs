// Copyright (C) 2009 Xtensive LLC.
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
using Xtensive.Core.Collections;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Materialization;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Linq
{
  internal sealed partial class Translator
  {
    public static readonly MethodInfo TranslateMethodInfo;

    public TranslatedQuery<TResult> Translate<TResult>()
    {
      var projection = (ProjectionExpression) Visit(context.Query);
      return Translate<TResult>(projection, EnumerableUtils<Parameter<Tuple>>.Empty);
    }

    private TranslatedQuery<TResult> Translate<TResult>(ProjectionExpression projection, IEnumerable<Parameter<Tuple>> tupleParameterBindings)
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

      var dataSource = prepared.ItemProjector.DataSource;
      var materializer = BuildMaterializer<TResult>(prepared, tupleParameterBindings);
      var translatedQuery = new TranslatedQuery<TResult>(dataSource, materializer, projection.TupleParameterBindings, tupleParameterBindings);

      // Providing the result to caching layer, if required
      if (cachingScope!=null) {
        var parameterizedQuery = new ParameterizedQuery<TResult>(
          translatedQuery,
          cachingScope.QueryParameter);
        cachingScope.ParameterizedQuery = parameterizedQuery;
        return parameterizedQuery;
      }
      return translatedQuery;
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
        ItemProjectorExpression itemProjector = origin.ItemProjector.Remap(rs, usedColumns.ToArray());
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

    private Func<IEnumerable<Tuple>, Dictionary<Parameter<Tuple>, Tuple>, TResult> BuildMaterializer<TResult>(ProjectionExpression projection, IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      var itemProjector = projection.ItemProjector;
      var materializationInfo = itemProjector.Materialize(context, tupleParameters);
      var rs = Expression.Parameter(typeof(IEnumerable<Tuple>), "rs");
      var tupleParameterBindings = Expression.Parameter(typeof (Dictionary<Parameter<Tuple>, Tuple>), "tupleParameterBindings");
      var elementType = itemProjector.Item.Type;
      var materializeMethod = MaterializationHelper.MaterializeMethodInfo
        .MakeGenericMethod(elementType);
      var compileMaterializerMethod = MaterializationHelper.CompileItemMaterializerMethodInfo
        .MakeGenericMethod(elementType);
      var itemMaterializer = compileMaterializerMethod.Invoke(null, new[] {materializationInfo.Expression});
      var materializationContext = new MaterializationContext(materializationInfo.EntitiesInRow);

      Expression body = Expression.Call(
        materializeMethod,
        rs,
        Expression.Constant(materializationContext),
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
      else
        body = body.Type==typeof (TResult)
          ? body
          : Expression.Convert(body, typeof (TResult));

      var projectorExpression = Expression.Lambda<Func<IEnumerable<Tuple>, Dictionary<Parameter<Tuple>, Tuple>, TResult>>(body, rs, tupleParameterBindings);
      return projectorExpression.CachingCompile();
    }


    // Constructors

    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    internal Translator(TranslatorContext context)
    {
      this.context = context;
      state = new State(this);
    }

    static Translator()
    {
      TranslateMethodInfo = typeof (Translator)
        .GetMethod("Translate", BindingFlags.NonPublic | BindingFlags.Instance, new[] {"TResult"}, new[] {typeof (ProjectionExpression), typeof (IEnumerable<Parameter<Tuple>>)});
      VisitLocalCollectionSequenceMethodInfo = typeof (Translator)
        .GetMethod("VisitLocalCollectionSequence", BindingFlags.NonPublic | BindingFlags.Instance, new[] {"TItem"}, new[] {"IEnumerable`1"});
    }

    private bool TypeIsStorageMappable(Type type)
    {
      // TODO: AG: Take info from storage!
      return type.IsPrimitive
        || type==typeof (decimal)
          || type==typeof (string)
            || type==typeof (DateTime)
              || type==typeof (TimeSpan);
    }
  }
}