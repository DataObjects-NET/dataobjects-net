// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.09

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  public partial class SqlSessionHandler
  {
    // Implementation of IProviderExecutor

    /// <inheritdoc/>
    IEnumerator<Tuple> IProviderExecutor.ExecuteTupleReader(QueryRequest request)
    {
      Prepare();
      var context = new CommandProcessorContext();
      var enumerator = commandProcessor.ExecuteTasksWithReader(request, context);
      context.Dispose();
      using (enumerator) {
        while (enumerator.MoveNext())
          yield return enumerator.Current;
      }
    }

    async Task<IEnumerator<Tuple>> IProviderExecutor.ExecuteTupleReaderAsync(QueryRequest request, CancellationToken token)
    {
      Prepare();
      using (var context = new CommandProcessorContext())
        return await commandProcessor.ExecuteTasksWithReaderAsync(request, context, token).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    void IProviderExecutor.Store(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples)
    {
      Prepare();
      foreach (var tuple in tuples)
        commandProcessor.RegisterTask(new SqlPersistTask(descriptor.StoreRequest, tuple));
      using (var context = new CommandProcessorContext())
        commandProcessor.ExecuteTasks(context);
    }



    async Task IProviderExecutor.StoreAsync(EnumerationContext enumerationContext,IPersistDescriptor descriptor, IEnumerable<Tuple> tuples, CancellationToken token)
    {
      await PrepareAsync(token);

      if (tuples is ExecutableRawProvider rawProvider) {
        var enumerator = await rawProvider.GetEnumeratorAsync(enumerationContext, token);
        while(enumerator.MoveNext()) {
          commandProcessor.RegisterTask(new SqlPersistTask(descriptor.StoreRequest, enumerator.Current));
        }
      }
      else {
        foreach (var tuple in tuples)
          commandProcessor.RegisterTask(new SqlPersistTask(descriptor.StoreRequest, tuple));
      }

      using (var context = new CommandProcessorContext())
        await commandProcessor.ExecuteTasksAsync(context, token);
    }

    /// <inheritdoc/>
    void IProviderExecutor.Clear(IPersistDescriptor descriptor)
    {
      Prepare();
      commandProcessor.RegisterTask(new SqlPersistTask(descriptor.ClearRequest, null));
      using (var context = new CommandProcessorContext())
        commandProcessor.ExecuteTasks(context);
    }

    /// <inheritdoc/>
    void IProviderExecutor.Overwrite(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples)
    {
      Prepare();
      commandProcessor.RegisterTask(new SqlPersistTask(descriptor.ClearRequest, null));
      foreach (var tuple in tuples)
        commandProcessor.RegisterTask(new SqlPersistTask(descriptor.StoreRequest, tuple));
      using (var context = new CommandProcessorContext())
        commandProcessor.ExecuteTasks(context);
    }
  }
}