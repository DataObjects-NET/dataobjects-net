// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.09

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
    IEnumerator<Tuple> IProviderExecutor.ExecuteTupleReader(QueryRequest request,
      ParameterContext parameterContext)
    {
      Prepare();
      var context = new CommandProcessorContext(parameterContext);
      var enumerator = commandProcessor.ExecuteTasksWithReader(request, context);
      context.Dispose();
      using (enumerator) {
        while (enumerator.MoveNext())
          yield return enumerator.Current;
      }
    }

    async Task<IEnumerator<Tuple>> IProviderExecutor.ExecuteTupleReaderAsync(QueryRequest request,
      ParameterContext parameterContext, CancellationToken token)
    {
      await PrepareAsync(token);
      using (var context = new CommandProcessorContext(parameterContext))
        return await commandProcessor.ExecuteTasksWithReaderAsync(request, context, token).ConfigureAwait(false);
    }

    async IAsyncEnumerable<Tuple> IProviderExecutor.ExecuteAsyncTupleReaderAsync(QueryRequest request,
      ParameterContext parameterContext, [EnumeratorCancellation] CancellationToken token)
    {
      await PrepareAsync(token);
      using (var context = new CommandProcessorContext(parameterContext)) {
        var enumerable = commandProcessor.ExecuteTasksWithAsyncReaderAsync(request, context, token);
        await foreach (var tuple in enumerable.WithCancellation(token)) {
          yield return tuple;
        }
      }
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