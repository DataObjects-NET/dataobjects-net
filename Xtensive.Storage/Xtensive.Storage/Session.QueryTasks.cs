// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using Xtensive.Storage.Internals;


namespace Xtensive.Storage
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
    
    internal void ExecuteAllDelayedQueries(bool allowPartialExecution)
    {
      if (IsDelayedQueryRunning || queryTasks.Count==0)
        return;
      try {
        IsDelayedQueryRunning = true;
        Handler.ExecuteQueryTasks(queryTasks, allowPartialExecution);
      }
      finally {
        queryTasks.Clear();
        IsDelayedQueryRunning = false;
      }
    }
  }
}