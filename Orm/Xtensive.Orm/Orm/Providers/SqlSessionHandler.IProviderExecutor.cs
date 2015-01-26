// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.09

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
      var enumerator = commandProcessor.ExecuteTasksWithReader(request);
      using (enumerator) {
        while (enumerator.MoveNext())
          yield return enumerator.Current;
      }
    }

#if NET45
    /// <inheritdoc/>
    async Task<IEnumerator<Tuple>> IProviderExecutor.ExecuteTupleReaderAsync(QueryRequest request, CancellationToken token)
    {
      Prepare();
      var command = await commandProcessor.ExecuteTasksWithReaderAsync(request, token);
      Session.AddNewBlockingCommand(command);
      return command.AsReaderOf(request);
    }
#endif

    /// <inheritdoc/>
    void IProviderExecutor.Store(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples)
    {
      Prepare();
      foreach (var tuple in tuples)
        commandProcessor.RegisterTask(new SqlPersistTask(descriptor.StoreRequest, tuple));
      commandProcessor.ExecuteTasks();
    }

    /// <inheritdoc/>
    void IProviderExecutor.Clear(IPersistDescriptor descriptor)
    {
      Prepare();
      commandProcessor.RegisterTask(new SqlPersistTask(descriptor.ClearRequest, null));
      commandProcessor.ExecuteTasks();
    }

    /// <inheritdoc/>
    void IProviderExecutor.Overwrite(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples)
    {
      Prepare();
      commandProcessor.RegisterTask(new SqlPersistTask(descriptor.ClearRequest, null));
      foreach (var tuple in tuples)
        commandProcessor.RegisterTask(new SqlPersistTask(descriptor.StoreRequest, tuple));
      commandProcessor.ExecuteTasks();
    }
  }
}