// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.29

using Xtensive.Core;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Providers.Sql
{
  public class SqlProviderPreparer : IPostCompiler
  {
    private readonly DomainHandler domainHandler;
    private readonly HandlerAccessor handlers;

    public ExecutableProvider Process(ExecutableProvider rootProvider)
    {
      var sqlProvider = rootProvider as SqlProvider;
      if (sqlProvider == null)
        return rootProvider;
      var request = sqlProvider.Request;
      // Nessesary part - prepare request for execution.
      request.Prepare();
      // Optional part - remove all underlying providers to save memory.
      return request.CheckOptions(QueryRequestOptions.AllowOptimization)
        ? new SqlProvider(handlers, request, sqlProvider.Origin.MakeVoid(), new ExecutableProvider[0])
        : rootProvider;
    }

    public SqlProviderPreparer(HandlerAccessor handlers)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      this.handlers = handlers;
      domainHandler = (DomainHandler) handlers.DomainHandler;
    }
  }
}