// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Sql
{
  internal class SimpleCommandProcessor : CommandProcessor
  {
    public override void ExecuteRequests(bool allowPartialExecution)
    {
      ExecuteAllTasks();
    }
    
    public override IEnumerator<Tuple> ExecuteRequestsWithReader(SqlQueryRequest lastRequest)
    {
      ExecuteAllTasks();
      return RunTupleReader(ExecuteQuery(new SqlQueryTask(lastRequest)), lastRequest.TupleDescriptor);
    }

    #region Private / internal methods

    private void ExecuteAllTasks()
    {
      while (tasks.Count > 0) {
        var task = tasks.Dequeue();
        var queryTask = task as SqlQueryTask;
        if (queryTask!=null)
          ReadTuples(ExecuteQuery(queryTask), queryTask.Request.TupleDescriptor, queryTask.Result);
        var persistTask = task as SqlPersistTask;
        if (persistTask!=null)
          ExecutePersist(persistTask);
      }
    }

    private void ReadTuples(DbDataReader reader, TupleDescriptor descriptor, List<Tuple> output)
    {
      var enumerator = RunTupleReader(reader, descriptor);
      using (enumerator) {
        while (enumerator.MoveNext())
          output.Add(enumerator.Current);
      }
    }

    private void ExecutePersist(SqlPersistTask task)
    {
      AllocateCommand();
      try {
        var part = factory.CreatePersistCommandPart(task, DefaultParameterNamePrefix);
        activeCommand.AddPart(part);
        activeCommand.ExecuteNonQuery();
      }
      finally {
        DisposeCommand();
      }
    }

    private DbDataReader ExecuteQuery(SqlQueryTask task)
    {
      AllocateCommand();
      try {
        var part = factory.CreateQueryCommandPart(task, DefaultParameterNamePrefix);
        activeCommand.AddPart(part);
        return activeCommand.ExecuteReader();
      }
      finally {
        DisposeCommand();
      }
    }

    #endregion


    // Constructors

    public SimpleCommandProcessor(SessionHandler sessionHandler)
      : base(sessionHandler)
    {
    }
  }
}