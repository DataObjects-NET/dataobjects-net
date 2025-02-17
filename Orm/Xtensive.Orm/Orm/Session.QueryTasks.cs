// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Internals;


namespace Xtensive.Orm
{
  partial class Session 
  {
    /// <summary>
    /// Uses only for queries which defined by Future/Delayed api
    /// </summary>
    private readonly IList<QueryTask> userDefinedQueryTasks = new List<QueryTask>();

    /// <summary>
    /// Uses for internal query tasks like fetching, validation of versions and some others
    /// </summary>
    private readonly IList<QueryTask> internalQueryTasks = new List<QueryTask>();

    internal void RegisterUserDefinedDelayedQuery(QueryTask task) => userDefinedQueryTasks.Add(task);

    internal void RegisterInternalDelayedQuery(QueryTask task) => internalQueryTasks.Add(task);

    internal bool ExecuteInternalDelayedQueries(bool skipPersist)
    {
      if (!skipPersist) {
        Persist(PersistReason.Other);
      }

      return ProcessInternalDelayedQueries(false);
    }

    internal async Task<bool> ExecuteInternalDelayedQueriesAsync(bool skipPersist, CancellationToken token = default)
    {
      if (!skipPersist) {
        await PersistAsync(PersistReason.Other, token).ConfigureAwaitFalse();
      }

      return await ProcessInternalDelayedQueriesAsync(false, token).ConfigureAwaitFalse();
    }

    internal bool ExecuteUserDefinedDelayedQueries(bool skipPersist)
    {
      if (!skipPersist) {
        Persist(PersistReason.Query);
      }

      return ProcessUserDefinedDelayedQueries(false);
    }

    internal async Task<bool> ExecuteUserDefinedDelayedQueriesAsync(bool skipPersist, CancellationToken token)
    {
      token.ThrowIfCancellationRequested();
      if (!skipPersist) {
        await PersistAsync(PersistReason.Other, token).ConfigureAwaitFalse();
      }

      token.ThrowIfCancellationRequested();
      return await ProcessUserDefinedDelayedQueriesAsync(false, token).ConfigureAwaitFalse();
    }

    private bool ProcessInternalDelayedQueries(bool allowPartialExecution)
    {
      if (internalQueryTasks.Count==0) {
        return false;
      }

      try {
        Handler.ExecuteQueryTasks(internalQueryTasks.Where(t=>t.LifetimeToken.IsActive), allowPartialExecution);
        return true;
      }
      finally {
        internalQueryTasks.Clear();
      }
    }

    private async Task<bool> ProcessInternalDelayedQueriesAsync(bool allowPartialExecution, CancellationToken token)
    {
      if (internalQueryTasks.Count==0) {
        return false;
      }

      try {
        await Handler.ExecuteQueryTasksAsync(
          internalQueryTasks.Where(t=>t.LifetimeToken.IsActive), allowPartialExecution, token).ConfigureAwaitFalse();
        return true;
      }
      finally {
        internalQueryTasks.Clear();
      }
    }

    private bool ProcessUserDefinedDelayedQueries(bool allowPartialExecution)
    {
      if (userDefinedQueryTasks.Count==0) {
        return false;
      }

      try {
        Handler.ExecuteQueryTasks(userDefinedQueryTasks.Where(t => t.LifetimeToken.IsActive), allowPartialExecution);
        return true;
      }
      finally {
        userDefinedQueryTasks.Clear();
      }
    }

    private async Task<bool> ProcessUserDefinedDelayedQueriesAsync(bool allowPartialExecution, CancellationToken token)
    {
      if (userDefinedQueryTasks.Count==0) {
        return false;
      }

      var aliveTasks = new List<QueryTask>(userDefinedQueryTasks.Count);
      aliveTasks.AddRange(userDefinedQueryTasks.Where(t => t.LifetimeToken.IsActive));
      userDefinedQueryTasks.Clear();
      await Handler.ExecuteQueryTasksAsync(aliveTasks, allowPartialExecution, token).ConfigureAwaitFalse();
      return true;
    }
  }
}
