// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.26

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Provides API for dealing with query pipeline.
  /// </summary>
  [Service(typeof (QueryBuilder), Singleton = true), Infrastructure]
  public sealed class QueryBuilder : SessionBound, ISessionService
  {
    private readonly StorageDriver driver;
    private readonly CommandFactory commandFactory;
    private readonly QueryProvider queryProvider;

    /// <summary>
    /// Translates the specified LINQ query into SQL DOM query.
    /// </summary>
    /// <typeparam name="TResult">Type of result element.</typeparam>
    /// <param name="query">Query to translate.</param>
    /// <returns>Translated query.</returns>
    public QueryTranslationResult TranslateQuery<TResult>(IQueryable<TResult> query)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");

      var configuration = new CompilerConfiguration {PrepareRequest = false};
      var translated = queryProvider.Translate<TResult>(query.Expression, configuration);

      var sqlProvider = translated.DataSource as SqlProvider;
      if (sqlProvider==null)
        throw new InvalidOperationException("Query was not translated to SqlProvider");

      var request = sqlProvider.Request;

      return new QueryTranslationResult(
        request.Statement,
        request.ParameterBindings.Select(b => new QueryParameterBinding(b)));
    }

    /// <summary>
    /// Compiles the specified SQL DOM query.
    /// </summary>
    /// <param name="query">Query to compile.</param>
    /// <returns>Compiled query.</returns>
    public SqlCompilationResult CompileQuery(ISqlCompileUnit query)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      return driver.Compile(query);
    }

    /// <summary>
    /// Creates new <see cref="QueryParameterBinding"/> with specified
    /// <paramref name="valueType"/> and <paramref name="valueAccessor"/>.
    /// </summary>
    /// <param name="valueType">Value type to use.</param>
    /// <param name="valueAccessor">Value accessor to use.</param>
    /// <returns>Created binding.</returns>
    public QueryParameterBinding CreateParameterBinding(Type valueType, Func<object> valueAccessor)
    {
      ArgumentValidator.EnsureArgumentNotNull(valueType, "valueType");
      ArgumentValidator.EnsureArgumentNotNull(valueAccessor, "valueAccessor");

      var mapping = driver.GetTypeMapping(valueType);
      return new QueryParameterBinding(
        new Providers.QueryParameterBinding(mapping, valueAccessor));
    }

    /// <summary>
    /// Builds request using specified <paramref name="compiledQuery"/> and <paramref name="bindings"/>.
    /// </summary>
    /// <returns>Built request.</returns>
    public QueryRequest CreateRequest(SqlCompilationResult compiledQuery, IEnumerable<QueryParameterBinding> bindings)
    {
      ArgumentValidator.EnsureArgumentNotNull(compiledQuery, "compiledQuery");
      ArgumentValidator.EnsureArgumentNotNull(bindings, "bindings");

      return new QueryRequest(new UserQueryRequest(
        compiledQuery,
        bindings.Select(b => b.RealBinding)));
    }

    /// <summary>
    /// Creates <see cref="QueryCommand"/> that is ready for execution
    /// by preparing the specified <paramref name="request"/> in current session.
    /// </summary>
    /// <param name="request">Request to use.</param>
    /// <returns>Created command.</returns>
    public QueryCommand CreateCommand(QueryRequest request)
    {
      ArgumentValidator.EnsureArgumentNotNull(request, "request");

      if (Session.Transaction!=null)
        Session.EnsureTransactionIsStarted();

      var command = commandFactory.CreateCommand();
      command.AddPart(commandFactory.CreateQueryPart(request.RealRequest));
      return new QueryCommand(driver, Session, command.Prepare());
    }

    // Constructors

    /// <inheritdoc/>
    [ServiceConstructor]
    public QueryBuilder(Session session)
      : base(session)
    {
      var sqlSessionHandler = (SqlSessionHandler) session.Handler.GetRealHandler();

      driver = session.Domain.Handlers.StorageDriver;
      commandFactory = sqlSessionHandler.CommandFactory;
      queryProvider = session.Query.Provider;
    }
  }
}