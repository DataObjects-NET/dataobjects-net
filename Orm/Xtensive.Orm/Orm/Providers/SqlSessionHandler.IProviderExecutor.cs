// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.09

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  public partial class SqlSessionHandler
  {
    // Implementation of IProviderExecutor

    /// <inheritdoc/>
    TupleReader IProviderExecutor.ExecuteTupleReader(QueryRequest request,
      ParameterContext parameterContext)
    {
      Prepare();
      using var context = new CommandProcessorContext(parameterContext);
      return commandProcessor.ExecuteTasksWithReader(request, context);
    }

    async Task<TupleReader> IProviderExecutor.ExecuteTupleReaderAsync(QueryRequest request,
      ParameterContext parameterContext, CancellationToken token)
    {
      await PrepareAsync(token);
      using var context = new CommandProcessorContext(parameterContext);
      return await commandProcessor.ExecuteTasksWithReaderAsync(request, context, token).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    void IProviderExecutor.Store(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples,
      ParameterContext parameterContext)
    {
      Prepare();
      foreach (var tuple in tuples)
        commandProcessor.RegisterTask(new SqlPersistTask(descriptor.StoreRequest, tuple));
      using (var context = new CommandProcessorContext(parameterContext))
        commandProcessor.ExecuteTasks(context);
    }

    /// <inheritdoc/>
    void IProviderExecutor.Clear(IPersistDescriptor descriptor, ParameterContext parameterContext)
    {
      Prepare();
      commandProcessor.RegisterTask(new SqlPersistTask(descriptor.ClearRequest, null));
      using (var context = new CommandProcessorContext(parameterContext))
        commandProcessor.ExecuteTasks(context);
    }

    /// <inheritdoc/>
    void IProviderExecutor.Overwrite(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples)
    {
      Prepare();
      commandProcessor.RegisterTask(new SqlPersistTask(descriptor.ClearRequest, null));
      foreach (var tuple in tuples)
        commandProcessor.RegisterTask(new SqlPersistTask(descriptor.StoreRequest, tuple));
      using (var context = new CommandProcessorContext(new ParameterContext()))
        commandProcessor.ExecuteTasks(context);
    }
  }
}