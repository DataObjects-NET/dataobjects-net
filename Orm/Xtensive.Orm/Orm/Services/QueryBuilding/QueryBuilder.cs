// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.26

using System;
using System.Data.Common;
using System.Linq.Expressions;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Providers.Sql;
using Xtensive.Sql;

namespace Xtensive.Orm.Services
{
  [Service(typeof (QueryBuilder), Singleton = true), Infrastructure]
  public sealed class QueryBuilder : SessionBound, ISessionService
  {
    private readonly StorageDriver driver;
    private readonly CommandFactory commandFactory;

    public void TranslateQuery(Expression query, QueryRootBuilder output)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      ArgumentValidator.EnsureArgumentNotNull(output, "output");

      throw new NotImplementedException();
    }

    public QueryRequestBuilder CreateRequestBuilder()
    {
      return new QueryRequestBuilder(driver);
    }

    public QueryRequestBuilder CreateRequestBuilder(ISqlCompileUnit query)
    {
      return new QueryRequestBuilder(driver) {Query = query};
    }

    public DbCommand CreateCommand(QueryRequest request)
    {
      ArgumentValidator.EnsureArgumentNotNull(request, "request");

      var command = commandFactory.CreateCommand();
      command.AddPart(commandFactory.CreateQueryPart(request.RealRequest));
      return command.Prepare();
    }

    // Constructors

    public QueryBuilder(Session session)
      : base(session)
    {
      var sqlDomainHandler = (DomainHandler) session.Domain.Handler;
      var sqlSessionHandler = (SessionHandler) session.Handler.GetRealHandler();

      driver = sqlDomainHandler.Driver;
      commandFactory = sqlSessionHandler.CommandFactory;
    }
  }
}