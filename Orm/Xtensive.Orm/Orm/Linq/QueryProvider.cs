// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// <see cref="IQueryProvider"/> implementation.
  /// </summary>
  public sealed class QueryProvider : IQueryProvider
  {
    private readonly Session session;

    /// <summary>
    /// Gets <see cref="Session"/> this provider is attached to.
    /// </summary>
    public Session Session { get { return session; } }
    
    /// <inheritdoc/>
    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
      Type elementType = SequenceHelper.GetElementType(expression.Type);
      try {
        var query = (IQueryable) typeof (Queryable<>).Activate(new[] {elementType}, new object[] {this, expression});
        return query;
      }
      catch (TargetInvocationException e) {
        throw e.InnerException;
      }
    }

    /// <inheritdoc/>
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
      return new Queryable<TElement>(this, expression);
    }

    /// <inheritdoc/>
    object IQueryProvider.Execute(Expression expression)
    {
      var resultType = expression.Type.IsOfGenericInterface(typeof (IEnumerable<>))
        ? typeof (IEnumerable<>).MakeGenericType(expression.Type.GetGenericArguments())
        : expression.Type;
      try {
        var executeMethod = WellKnownMembers.QueryProvider.Execute.MakeGenericMethod(resultType);
        var result = executeMethod.Invoke(this, new[] {expression});
        return result;
      }
      catch(TargetInvocationException te) {
        throw te.InnerException;
      }
    }

    /// <inheritdoc/>
    public TResult Execute<TResult>(Expression expression)
    {
      expression = session.Events.NotifyQueryExecuting(expression);
      var query = Translate<TResult>(expression);
      var cachingScope = QueryCachingScope.Current;
      TResult result;
      if (cachingScope != null && !cachingScope.Execute)
        result = default(TResult);
      else
        result = query.Execute(session, new ParameterContext());
      session.Events.NotifyQueryExecuted(expression);
      return result;
    }

    #region Private / internal methods

    internal TranslatedQuery<TResult> Translate<TResult>(Expression expression)
    {
      return Translate<TResult>(expression, session.CompilationService.CreateConfiguration(session));
    }

#if NET45

    internal Task<TranslatedQuery<TResult>> TranslateAsync<TResult>(Expression expression)
    {
      return TranslateAsync<TResult>(expression, session.CompilationService.CreateConfiguration(session));
    }

#endif

    internal TranslatedQuery<TResult> Translate<TResult>(Expression expression, CompilerConfiguration compilerConfiguration)
    {
      try {
        var context = new TranslatorContext(session, compilerConfiguration, expression);
        return context.Translator.Translate<TResult>();
      }
      catch (Exception ex) {
        throw new QueryTranslationException(String.Format(
          Strings.ExUnableToTranslateXExpressionSeeInnerExceptionForDetails, expression.ToString(true)), ex);
      }
    }

#if NET45
    internal Task<TranslatedQuery<TResult>> TranslateAsync<TResult>(Expression expression, CompilerConfiguration compilerConfiguration)
    {
      var cachinngScope = QueryCachingScope.Current;
      var useCachingScope = cachinngScope!=null;
      return Task<TranslatedQuery<TResult>>.Factory.StartNew(
        () => {
          try {
            if (useCachingScope) {
              using (new QueryCachingScope(cachinngScope.QueryParameter, cachinngScope.QueryParameterReplacer)) {
                var context = new TranslatorContext(session, compilerConfiguration, expression);
                return context.Translator.Translate<TResult>();
              }
            }
            else {
              var context = new TranslatorContext(session, compilerConfiguration, expression);
              return context.Translator.Translate<TResult>();
            }
          }
          catch (Exception exception) {
            throw new QueryTranslationException(String.Format(Strings.ExUnableToTranslateXExpressionSeeInnerExceptionForDetails, expression.ToString(true)), exception);
          }
        });
    }

#endif

    #endregion


    // Constructors

    internal QueryProvider(Session session)
    {
      this.session = session;
    }
  }
}