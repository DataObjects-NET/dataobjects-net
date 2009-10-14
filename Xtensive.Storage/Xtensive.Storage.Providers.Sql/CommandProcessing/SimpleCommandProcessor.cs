// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Tuples;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  internal class SimpleCommandProcessor : CommandProcessor
  {
    public override void ExecuteRequests(bool allowPartialExecution)
    {
      ExecuteAllTasks();
    }

    private void ReadTuples(DbDataReader reader, TupleDescriptor descriptor, List<Tuple> output)
    {
      var enumerator = RunTupleReader(reader, descriptor);
      using (enumerator) {
        while (enumerator.MoveNext())
          output.Add(enumerator.Current);
      }
    }
    
    public override IEnumerator<Tuple> ExecuteRequestsWithReader(SqlQueryRequest lastRequest)
    {
      ExecuteAllTasks();
      return RunTupleReader(ExecuteQueryQuickly(new SqlQueryTask(lastRequest)), lastRequest.TupleDescriptor);
    }

    private void ExecuteAllTasks()
    {
      while (persistTasks.Count > 0) {
        var task = persistTasks.Dequeue();
        ExecutePersistQuickly(task);
      }
      while (queryTasks.Count > 0) {
        var task = queryTasks.Dequeue();
        ReadTuples(ExecuteQueryQuickly(task), task.Request.TupleDescriptor, task.Output);
      }
    }


    // Constructors

    public SimpleCommandProcessor(SessionHandler sessionHandler)
      : base(sessionHandler)
    {
    }
  }
}