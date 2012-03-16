// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using Xtensive.Orm.Internals;


namespace Xtensive.Orm
{
  partial class Session 
  {
    private readonly List<QueryTask> queryTasks = new List<QueryTask>();

    internal void RegisterDelayedQuery(QueryTask task)
    {
      if (IsDelayedQueryRunning)
        throw new InvalidOperationException();
      queryTasks.Add(task);
    }

    internal bool ExecuteDelayedQueries(bool skipPersist)
    {
      if (!skipPersist)
        Persist(PersistReason.Query);
      return ProcessDelayedQueries(false);
    }

    private bool ProcessDelayedQueries(bool allowPartialExecution)
    {
      if (IsDelayedQueryRunning || queryTasks.Count==0)
        return false;
      EnsureTransactionIsStarted();
      try {
        IsDelayedQueryRunning = true;
        Handler.ExecuteQueryTasks(queryTasks, allowPartialExecution);
        return true;
      }
      finally {
        queryTasks.Clear();
        IsDelayedQueryRunning = false;
      }
    }
  }
}