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
using Xtensive.Core.Parameters;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Rewriters;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides compilation and caching of queries for reuse.
  /// </summary>
  public sealed class CompiledQuery
  {
    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate and executes them if it's already cached; otherwise executes the <paramref name="query"/> delegate.
    /// </summary>
    /// <typeparam name="TElement">The type of the result element.</typeparam>
    /// <param name="query">Containing query delegate.</param>
    public static IEnumerable<TElement> Execute<TElement>(Func<IQueryable<TElement>> query)
    {
      var domain = Domain.Demand();
      var target = query.Target;
      Pair<MethodInfo, ParameterizedResultExpression> item;
      ParameterizedResultExpression resultExpression = null;
      if (domain.QueryCache.TryGetItem(query.Method, true, out item))
        resultExpression = item.Second;
      if (resultExpression == null) {
        var result = query();
        var queryParameter = new Parameter<object>();
        var compiledResultExpression = QueryProvider.Current.Compile(result.Expression);
        if (target != null) {
          var replacer = new ExtendedExpressionReplacer(e => {
            if (e.NodeType == ExpressionType.Constant && e.Type == target.GetType())
              return Expression.Convert(Expression.MakeMemberAccess(Expression.Constant(queryParameter), WellKnownMembers.ParameterValue), target.GetType());
            return null;
          });
          resultExpression = new ParameterizedResultExpression((ResultExpression)replacer.Replace(compiledResultExpression), queryParameter);
        }
        else
          resultExpression = new ParameterizedResultExpression(compiledResultExpression, queryParameter);
        lock (domain.QueryCache)
          if (!domain.QueryCache.TryGetItem(query.Method, false, out item))
            domain.QueryCache.Add(new Pair<MethodInfo, ParameterizedResultExpression>(query.Method, resultExpression));
        return result;
      }
      return ExecuteSequence<TElement>(resultExpression, target);
    }

    private static IEnumerable<TElement> ExecuteSequence<TElement>(ParameterizedResultExpression resultExpression, object target)
    {
      using (new ParameterScope()) {
        resultExpression.QueryParameter.Value = target;
        foreach (var element in resultExpression.GetResult<IEnumerable<TElement>>())
          yield return element;
      }
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate and executes them if it's already cached; otherwise executes the <paramref name="query"/> delegate.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">Containing query delegate.</param>
    public static TResult Execute<TResult>(Func<TResult> query)
    {
      var domain = Domain.Demand();
      var target = query.Target;
      Pair<MethodInfo, ParameterizedResultExpression> item;
      ParameterizedResultExpression resultExpression = null;
      if (domain.QueryCache.TryGetItem(query.Method, true, out item))
        resultExpression = item.Second;
      if (resultExpression == null) {
        using (var queryScope = new CompiledQueryScope()) {
          var result = query();
          var queryParameter = new Parameter<object>();
          var compiledExpression = queryScope.Context;
          if (target != null) {
            var replacer = new ExtendedExpressionReplacer(e => {
              if (e.NodeType == ExpressionType.Constant && e.Type == target.GetType())
                return Expression.Convert(Expression.MakeMemberAccess(Expression.Constant(queryParameter), WellKnownMembers.ParameterValue), target.GetType());
              return null;
            });
            resultExpression = new ParameterizedResultExpression((ResultExpression)replacer.Replace(compiledExpression), queryParameter);
          }
          else
            resultExpression = new ParameterizedResultExpression(compiledExpression, queryParameter);
          lock (domain.QueryCache)
            if (!domain.QueryCache.TryGetItem(query.Method, false, out item))
              domain.QueryCache.Add(new Pair<MethodInfo, ParameterizedResultExpression>(query.Method, resultExpression));
          return result;
        }
      }
      using (new ParameterScope()) {
        resultExpression.QueryParameter.Value = target;
        var result = resultExpression.GetResult<TResult>();
        return result;
      }
    }
  }
}
