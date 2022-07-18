// Copyright (C) 2012-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.01.29

using System;
using Xtensive.Core;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Providers
{
  public class SqlProviderPreparer : IPostCompiler
  {
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
        ? new SqlProvider(handlers, request, sqlProvider.Origin.MakeVoid(), Array.Empty<ExecutableProvider>())
        : rootProvider;
    }

    public SqlProviderPreparer(HandlerAccessor handlers)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      this.handlers = handlers;
    }
  }
}