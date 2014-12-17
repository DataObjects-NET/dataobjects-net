// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Linq;
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
      return ProcessUserDefinedDelayedQueries(false);
    }
#if NET45

    internal async Task<bool> ExecuteDelayedQueriesAsync(bool skipPersist)
    {
      if (queryAsyncTaskStarted)
        return false;
      try {
        lock (lockableObject) {
          queryAsyncTaskStarted = true;
        }
        if (!skipPersist)
          await Task.Factory.StartNew(() => Persist(PersistReason.Query));
        return await ProcessDelayedQueriesAsync(false);
      }
      finally {
        lock (lockableObject) {
          queryAsyncTaskStarted = false;
        }
      }
    }

#endif
    
    internal bool ExecuteInternalDelayedQueries(bool skipPersist)
    {
      if (!skipPersist)
        Persist(PersistReason.Other);
      return ProcessInternalDelayedQueries(false);
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

#if NET45

    private async Task<bool> ProcessDelayedQueriesAsync(bool allowPartalExecution)
    {
      if (isDelayedQueryRunning || userDefinedQueryTasks.Count==0)
        return false;
      var aliveTasks = userDefinedQueryTasks.Where(t => t.LifetimeToken.IsActive).ToList();
      try {
        isDelayedQueryRunning = true;
        foreach (var userDefinedQueryTask in aliveTasks)
          userDefinedQueryTask.TrySetDelayedTaskStarted();
        await Handler.ExecuteQueryTasksAsync(aliveTasks, allowPartalExecution);
        foreach (var userDefinedQueryTask in aliveTasks)
          userDefinedQueryTask.TrySetDelayedTaskFinished(null);
        return true;
      }
      catch (Exception exception) {
        foreach (var userDefinedQueryTask in aliveTasks)
          userDefinedQueryTask.TrySetDelayedTaskFinished(exception);
        return false;
      }
      finally {
        userDefinedQueryTasks.Clear();
        isDelayedQueryRunning = false;
      }
    }

#endif
  }
}