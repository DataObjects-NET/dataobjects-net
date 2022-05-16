// Copyright (C) 2010-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.02.09

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  public partial class SqlSessionHandler
  {
    // Implementation of IProviderExecutor

    /// <inheritdoc/>
    DataReader IProviderExecutor.ExecuteTupleReader(QueryRequest request,
      ParameterContext parameterContext)
    {
      Prepare();
      using var context = new CommandProcessorContext(parameterContext);
      return commandProcessor.ExecuteTasksWithReader(request, context);
    }

    /// <inheritdoc/>
    async Task<DataReader> IProviderExecutor.ExecuteTupleReaderAsync(QueryRequest request,
      ParameterContext parameterContext, CancellationToken token)
    {
      await PrepareAsync(token).ConfigureAwait(false);
      var context = new CommandProcessorContext(parameterContext);
      await using (context.ConfigureAwait(false)) {
        return await commandProcessor.ExecuteTasksWithReaderAsync(request, context, token).ConfigureAwait(false);
      }
    }

    /// <inheritdoc/>
    void IProviderExecutor.Store(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples,
      ParameterContext parameterContext)
    {
      Prepare();
      Store(descriptor, tuples);
      Execute(parameterContext);
    }

    /// <inheritdoc/>
    async Task IProviderExecutor.StoreAsync(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples,
      ParameterContext parameterContext, CancellationToken token)
    {
      await PrepareAsync(token).ConfigureAwait(false);

      Store(descriptor, tuples);

      using (var context = new CommandProcessorContext(parameterContext)) {
        await commandProcessor.ExecuteTasksAsync(context, token).ConfigureAwait(false);
      }
    }

    /// <inheritdoc/>
    void IProviderExecutor.Clear(IPersistDescriptor descriptor, ParameterContext parameterContext)
    {
      Prepare();
      commandProcessor.RegisterTask(new SqlPersistTask(descriptor.ClearRequest.Value));
      Execute(parameterContext);
    }

    /// <inheritdoc/>
    void IProviderExecutor.Overwrite(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples)
    {
      Prepare();
      commandProcessor.RegisterTask(new SqlPersistTask(descriptor.ClearRequest.Value));
      Store(descriptor, tuples);
      Execute(new ParameterContext());
    }

    private void Execute(ParameterContext parameterContext)
    {
      using var context = new CommandProcessorContext(parameterContext);
      commandProcessor.ExecuteTasks(context);
    }

    private void Store(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples)
    {
      if (descriptor.LazyLevel1BatchStoreRequest != null && descriptor.LazyLevel2BatchStoreRequest != null) {
        foreach (var level2Chunk in tuples.Chunk(WellKnown.MultiRowInsertLevel2BatchSize)) {
          if (level2Chunk.Length == WellKnown.MultiRowInsertLevel2BatchSize) {
            commandProcessor.RegisterTask(new SqlPersistTask(descriptor.LazyLevel2BatchStoreRequest.Value, level2Chunk));
          }
          else {
            foreach (var level1Chunk in level2Chunk.Chunk(WellKnown.MultiRowInsertLevel1BatchSize)) {
              if (level1Chunk.Length == WellKnown.MultiRowInsertLevel1BatchSize) {
                commandProcessor.RegisterTask(new SqlPersistTask(descriptor.LazyLevel1BatchStoreRequest.Value, level1Chunk));
              }
              else {
                foreach (var tuple in level1Chunk) {
                  commandProcessor.RegisterTask(new SqlPersistTask(descriptor.LazyStoreRequest.Value, tuple));
                }
              }
            }
          }
        }
      }
      else {
        foreach (var tuple in tuples) {
          commandProcessor.RegisterTask(new SqlPersistTask(descriptor.LazyStoreRequest.Value, tuple));
        }
      }
    }
  }
}
