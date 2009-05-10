// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Rewriters;
using Activator=System.Activator;

namespace Xtensive.Storage
{
  /// <summary>
  /// Caches the compilation result of provided query.
  /// </summary>
  public static class CachedQuery
  {
    private static object _lock = new object();

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate 
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and executes them if it's already cached; 
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TElement">The type of the result element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static IEnumerable<TElement> Execute<TElement>(Func<IQueryable<TElement>> query)
    {
      var domain = Domain.Demand();
      var target = query.Target;
      var cache = domain.QueryCache;
      Pair<MethodInfo, ParameterizedResultExpression> item;
      ParameterizedResultExpression resultExpression = null;
      lock (cache)
        if (cache.TryGetItem(query.Method, true, out item))
          resultExpression = item.Second;
      if (resultExpression == null) {
        var result = query();
        var compiledResultExpression = QueryProvider.Instance.Compile(result.Expression);
        resultExpression = BuildResultExpression(target, compiledResultExpression);
        lock (cache)
          if (!cache.TryGetItem(query.Method, false, out item))
            cache.Add(new Pair<MethodInfo, ParameterizedResultExpression>(query.Method, resultExpression));
        return result;
      }
      return ExecuteSequence<TElement>(resultExpression, target);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static TResult Execute<TResult>(Func<TResult> query)
    {
      var domain = Domain.Demand();
      var target = query.Target;
      var cache = domain.QueryCache;
      Pair<MethodInfo, ParameterizedResultExpression> item;
      ParameterizedResultExpression resultExpression = null;
      lock (cache)
        if (cache.TryGetItem(query.Method, true, out item))
          resultExpression = item.Second;
      if (resultExpression == null) {
        using (var compilationScope = new QueryCachingScope()) {
          var result = query();
          var compiledExpression = compilationScope.CompilationResult;
          resultExpression = BuildResultExpression(target, compiledExpression);
          lock (cache)
            if (!cache.TryGetItem(query.Method, false, out item))
              cache.Add(new Pair<MethodInfo, ParameterizedResultExpression>(query.Method, resultExpression));
          return result;
        }
      }
      using (new ParameterContext().Activate()) {
        resultExpression.QueryParameter.Value = target;
        var result = resultExpression.GetResult<TResult>();
        return result;
      }
    }

    #region Private methods

    private static ParameterizedResultExpression BuildResultExpression(object target, ResultExpression compiledResultExpression)
    {
      ParameterizedResultExpression resultExpression;
      if (target != null) {
        var closureType = target.GetType();
        var parameterType = typeof (Parameter<>).MakeGenericType(closureType);
        var valueMemberInfo = parameterType.GetProperty("Value", closureType);
        var queryParameter = (Parameter)Activator.CreateInstance(parameterType, "pClosure", target);
        var replacer = new ExtendedExpressionReplacer(e => {
          if (e.NodeType == ExpressionType.Constant && e.Type.IsClosure()) {
            if (e.Type == closureType)
              return Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType), valueMemberInfo);
            throw new NotSupportedException("CachedQuery supports only queries written within its Execute methods.");
          }
          return null;
        });
        resultExpression = new ParameterizedResultExpression((ResultExpression)replacer.Replace(compiledResultExpression), queryParameter);
      }
      else
        resultExpression = new ParameterizedResultExpression(compiledResultExpression, null);
      return resultExpression;
    }

    private static IEnumerable<TElement> ExecuteSequence<TElement>(ParameterizedResultExpression resultExpression, object target)
    {
      using (new ParameterContext().Activate()) {
        if (resultExpression.QueryParameter != null)
          resultExpression.QueryParameter.Value = target;
        foreach (var element in resultExpression.GetResult<IEnumerable<TElement>>())
          yield return element;
      }
    }

    #endregion
  }
}
