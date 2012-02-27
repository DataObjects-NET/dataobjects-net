// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.26

using System;
using System.Linq;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Providers.Sql;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Sql;

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
    /// Translates the specified LINQ query and writes result
    /// to the specified <see cref="QueryRequestBuilder"/>.
    /// </summary>
    /// <typeparam name="TResult">Type of result element.</typeparam>
    /// <param name="query">Query to translate.</param>
    /// <param name="output"><see cref="QueryRequestBuilder"/> to put result to.</param>
    public void TranslateQuery<TResult>(IQueryable<TResult> query, QueryRequestBuilder output)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      ArgumentValidator.EnsureArgumentNotNull(output, "output");

      var configuration = new CompilerConfiguration {PrepareRequest = false};
      var translated = queryProvider.Translate<TResult>(query.Expression, configuration);

      var sqlProvider = translated.DataSource as SqlProvider;
      if (sqlProvider==null)
        throw new InvalidOperationException("Query was not translated to SqlProvider");

      var request = sqlProvider.Request;
      output.Query = request.Statement;
      output.ParameterBindings.Clear();
      foreach (var binding in request.ParameterBindings)
        output.ParameterBindings.Add(new QueryParameterBinding(binding));
    }

    /// <summary>
    /// Creates <see cref="QueryRequestBuilder"/>.
    /// </summary>
    /// <returns>Created instance.</returns>
    public QueryRequestBuilder CreateRequestBuilder()
    {
      return new QueryRequestBuilder(driver);
    }

    /// <summary>
    /// Creates <see cref="QueryRequestBuilder"/> with specified <paramref name="query "/>.
    /// </summary>
    /// <param name="query">Query to use.</param>
    /// <returns>Created instance.</returns>
    public QueryRequestBuilder CreateRequestBuilder(ISqlCompileUnit query)
    {
      return new QueryRequestBuilder(driver) {Query = query};
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

      var command = commandFactory.CreateCommand();
      command.AddPart(commandFactory.CreateQueryPart(request.RealRequest));
      return new QueryCommand(driver, Session, command.Prepare());
    }

    // Constructors

    /// <inheritdoc/>
    public QueryBuilder(Session session)
      : base(session)
    {
      var sqlDomainHandler = (DomainHandler) session.Domain.Handler;
      var sqlSessionHandler = (SessionHandler) session.Handler.GetRealHandler();

      driver = sqlDomainHandler.Driver;
      commandFactory = sqlSessionHandler.CommandFactory;
      queryProvider = session.Query.Provider;
    }
  }
}