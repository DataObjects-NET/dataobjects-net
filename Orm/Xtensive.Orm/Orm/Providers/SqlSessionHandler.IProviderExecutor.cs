// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.09

using System.Collections.Generic;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  public partial class SqlSessionHandler
  {
    // Implementation of IProviderExecutor

    /// <inheritdoc/>
    IEnumerator<Tuple> IProviderExecutor.ExecuteTupleReader(QueryRequest request)
    {
      EnsureConnectionIsOpen();
      var enumerator = commandProcessor.ExecuteTasksWithReader(request);
      using (enumerator) {
        while (enumerator.MoveNext())
          yield return enumerator.Current;
      }
    }

    /// <inheritdoc/>
    void IProviderExecutor.Store(IPersistDescriptor descriptor, IEnumerable<Tuple> tuples)
    {
      EnsureConnectionIsOpen();
      foreach (var tuple in tuples)
        commandProcessor.RegisterTask(new SqlPersistTask(descriptor.StoreRequest, tuple));
      commandProcessor.ExecuteTasks();
    }

    /// <inheritdoc/>
    void IProviderExecutor.Clear(IPersistDescriptor descriptor)
    {
      EnsureConnectionIsOpen();
      commandProcessor.RegisterTask(new SqlPersistTask(descriptor.ClearRequest, null));
      commandProcessor.ExecuteTasks();
    }
  }
}