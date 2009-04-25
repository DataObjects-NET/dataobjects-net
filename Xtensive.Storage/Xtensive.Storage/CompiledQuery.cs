// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Linq.Expressions;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides compilation and caching of queries for reuse.
  /// </summary>
  public sealed class CompiledQuery
  {
    /// <summary>
    /// Compiles the specified query.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query expression to be compiled.</param>
    /// <returns>A generic delegate of type <see cref="Func{TResult}"/> that represents the compiled query.</returns>
    public static Func<TResult> Compile<TResult>(Expression<Func<TResult>> query)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Compiles the specified query.
    /// </summary>
    /// <typeparam name="T0">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query expression to be compiled.</param>
    /// <returns>A generic delegate of type <see cref="Func{T0,TResult}"/> that represents the compiled query.</returns>
    public static Func<T0,TResult> Compile<T0,TResult>(Expression<Func<T0,TResult>> query)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Compiles the specified query.
    /// </summary>
    /// <typeparam name="T0">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="T1">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query expression to be compiled.</param>
    /// <returns>A generic delegate of type <see cref="Func{T0,TResult}"/> that represents the compiled query.</returns>
    public static Func<T0,T1, TResult> Compile<T0, T1, TResult>(Expression<Func<T0, T1, TResult>> query)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Compiles the specified query.
    /// </summary>
    /// <typeparam name="T0">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="T1">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="T2">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query expression to be compiled.</param>
    /// <returns>A generic delegate of type <see cref="Func{T0,TResult}"/> that represents the compiled query.</returns>
    public static Func<T0, T1, T2, TResult> Compile<T0, T1, T2, TResult>(Expression<Func<T0, T1, T2, TResult>> query)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Compiles the specified query.
    /// </summary>
    /// <typeparam name="T0">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="T1">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="T2">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="T3">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query expression to be compiled.</param>
    /// <returns>A generic delegate of type <see cref="Func{T0,TResult}"/> that represents the compiled query.</returns>
    public static Func<T0, T1, T2, T3, TResult> Compile<T0, T1, T2, T3, TResult>(Expression<Func<T0, T1, T2, T3, TResult>> query)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Compiles the specified query.
    /// </summary>
    /// <typeparam name="T0">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="T1">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="T2">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="T3">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="T4">The type of the <paramref name="query"/> parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query expression to be compiled.</param>
    /// <returns>A generic delegate of type <see cref="Func{T0,TResult}"/> that represents the compiled query.</returns>
    public static Func<T0, T1, T2, T3, T4, TResult> Compile<T0, T1, T2, T3, T4, TResult>(Expression<Func<T0, T1, T2, T3, T4, TResult>> query)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate and executes them if it's already cached; otherwise executes the <paramref name="query"/> delegate.
    /// </summary>
    /// <typeparam name="TElement">The type of the result element.</typeparam>
    /// <param name="query">Containing query delegate.</param>
    public static IEnumerable<TElement> Execute<TElement>(Func<IEnumerable<TElement>> query)
    {
      return Execute<IEnumerable<TElement>>(query);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate and executes them if it's already cached; otherwise executes the <paramref name="query"/> delegate.
    /// </summary>
    /// <typeparam name="TElement">The type of the result element.</typeparam>
    /// <param name="query">Containing query delegate.</param>
    public static IEnumerable<TElement> Execute<TElement>(Func<IQueryable<TElement>> query)
    {
      var domain = Domain.Current;
      if (domain != null) {
        Pair<MethodInfo, ResultExpression> item;
        ResultExpression resultExpression = null;
        if (domain.QueryCache.TryGetItem(query.Method, true, out item))
          resultExpression = item.Second;
        if (resultExpression == null) {
          var result = query();
          resultExpression = QueryProvider.LatestCompiledResult;
          lock (domain.QueryCache)
            if (!domain.QueryCache.TryGetItem(query.Method, false, out item))
              domain.QueryCache.Add(new Pair<MethodInfo, ResultExpression>(query.Method, resultExpression));
          return result;
        }
        return resultExpression.GetResult<IEnumerable<TElement>>();
      }
      //TODO: write error message
      throw new InvalidOperationException();
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate and executes them if it's already cached; otherwise executes the <paramref name="query"/> delegate.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">Containing query delegate.</param>
    public static TResult Execute<TResult>(Func<TResult> query)
    {
      var domain = Domain.Current;
      if (domain != null) {
        Pair<MethodInfo, ResultExpression> item;
        ResultExpression resultExpression = null;
        if (domain.QueryCache.TryGetItem(query.Method, true, out item))
          resultExpression = item.Second;
        if (resultExpression == null) {
          var result = query();
          resultExpression = QueryProvider.LatestCompiledResult;
          lock (domain.QueryCache)
            if (!domain.QueryCache.TryGetItem(query.Method, false, out item))
              domain.QueryCache.Add(new Pair<MethodInfo, ResultExpression>(query.Method, resultExpression));
          return result;
        }
        return resultExpression.GetResult<TResult>();
      }
      //TODO: write error message
      throw new InvalidOperationException();
    }
  }
}