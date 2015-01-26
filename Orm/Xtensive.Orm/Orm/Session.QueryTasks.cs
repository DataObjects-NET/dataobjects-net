// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    private bool isInternalDelayedQueryRunning;
    private bool isDelayedQueryRunning;
    private bool queryAsyncTaskStarted;
    private object lockableObject = new object();

    internal void RegisterUserDefinedDelayedQuery(QueryTask task)
    {
      if (isDelayedQueryRunning)
        throw new InvalidOperationException();
      userDefinedQueryTasks.Add(task);
    }

    internal void RegisterInternalDelayedQuery(QueryTask task)
    {
      if(isInternalDelayedQueryRunning)
        throw new InvalidOperationException();
      internalQueryTasks.Add(task);
    }

    internal bool ExecuteUserDefinedDelayedQueries(bool skipPersist)
    {
      if (!skipPersist)
        Persist(PersistReason.Query);
      return ProcessUserDefinedDelayedQueries(ExecutionBehavior.PartialExecutionIsNotAllowed);
    }
#if NET45

    internal async Task<bool> ExecuteDelayedQueriesAsync(bool skipPersist, CancellationToken token)
    {
      if (!skipPersist)
        Persist(PersistReason.Other);
      return await ProcessUserDefinedDelayedQueriesAsync(token);
    }

#endif
    
    internal bool ExecuteInternalDelayedQueries(bool skipPersist)
    {
      if (!skipPersist)
        Persist(PersistReason.Other);
      return ProcessInternalDelayedQueries(ExecutionBehavior.PartialExecutionIsNotAllowed);
    }

    private bool ProcessUserDefinedDelayedQueries(ExecutionBehavior executionBehavior)
    {
      if (isDelayedQueryRunning || userDefinedQueryTasks.Count == 0)
        return false;
      var aliveTasks = userDefinedQueryTasks.Where(t => t.LifetimeToken.IsActive).ToList();
      userDefinedQueryTasks.Clear();
      try {
        isDelayedQueryRunning = true;
        Handler.ExecuteQueryTasks(aliveTasks, executionBehavior);
        return true;
      }
      finally {
        isDelayedQueryRunning = false;
      }
    }

    private bool ProcessInternalDelayedQueries(ExecutionBehavior executionBehavior)
    {
      if (isInternalDelayedQueryRunning || internalQueryTasks.Count == 0)
        return false;
      var aliveTasks = internalQueryTasks.Where(t => t.LifetimeToken.IsActive).ToList();
      internalQueryTasks.Clear();
      try {
        isInternalDelayedQueryRunning = true;
        Handler.ExecuteQueryTasks(aliveTasks, executionBehavior);
        return true;
      }
      finally {
        isInternalDelayedQueryRunning = false;
      }
    }

#if NET45

    private async Task<bool> ProcessUserDefinedDelayedQueriesAsync(CancellationToken token)
    {
      if (isDelayedQueryRunning || userDefinedQueryTasks.Count==0)
        return false;
      var aliveTasks = userDefinedQueryTasks.Where(t => t.LifetimeToken.IsActive).ToList();
      userDefinedQueryTasks.Clear();
      isDelayedQueryRunning = true;
      try {
        return await Handler.ExecuteQueryTasksAsync(aliveTasks, ExecutionBehavior.PartialExecutionIsNotAllowed, token)
          .ContinueWith(
            task => {
              lock (lockableObject) {
                isDelayedQueryRunning = false;
              }
              if (task.IsFaulted) {
                return false;
              }
              return true;
            });
      }
      finally {
        isDelayedQueryRunning = false;
      }
    }

#endif
  }
}