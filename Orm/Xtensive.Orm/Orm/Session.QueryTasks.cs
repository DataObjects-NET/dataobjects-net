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
      return ProcessUserDefinedDelayedQueries(false);
    }
    
    internal bool ExecuteInternalDelayedQueries(bool skipPersist)
    {
      if (!skipPersist)
        Persist(PersistReason.Other);
      return ProcessInternalDelayedQueries(false);
    }

    internal async Task<bool> ExecuteDelayedUserQueriesAsync(bool skipPersist, CancellationToken token)
    {
      if (!skipPersist)
        Persist(PersistReason.Query);
      return await ProcessUserDefinedDelayedQueriesAsync(token).ConfigureAwait(false);
    }

    private bool ProcessUserDefinedDelayedQueries(bool allowPartialExecution)
    {
      if (isDelayedQueryRunning || userDefinedQueryTasks.Count==0)
        return false;
      try {
        isDelayedQueryRunning = true;
        Handler.ExecuteQueryTasks(userDefinedQueryTasks.Where(t => t.LifetimeToken.IsActive), allowPartialExecution);
        return true;
      }
      finally {
        userDefinedQueryTasks.Clear();
        isDelayedQueryRunning = false;
      }
    }

    private bool ProcessInternalDelayedQueries(bool allowPartialExecution)
    {
      if (isInternalDelayedQueryRunning || internalQueryTasks.Count == 0)
        return false;
      try {
        isInternalDelayedQueryRunning = true;
        Handler.ExecuteQueryTasks(internalQueryTasks.Where(t=>t.LifetimeToken.IsActive), allowPartialExecution);
        return true;
      }
      finally {
        internalQueryTasks.Clear();
        isInternalDelayedQueryRunning = false;
      }
    }

    private async Task<bool> ProcessUserDefinedDelayedQueriesAsync(CancellationToken token)
    {
      if (userDefinedQueryTasks.Count==0)
        return false;
      var aliveTasks = userDefinedQueryTasks.Where(t => t.LifetimeToken.IsActive).ToList();
      userDefinedQueryTasks.Clear();
      await Handler.ExecuteQueryTasksAsync(aliveTasks, false, token).ConfigureAwait(false);
      return true;
    }
  }
}