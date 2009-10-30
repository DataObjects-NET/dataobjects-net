// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Sql
{
  internal class BatchingCommandProcessor : CommandProcessor
  {
    private int batchSize;
    
    private string GetParameterPrefix()
    {
      return string.Format("p{0}_", activeCommand.Statements.Count + 1);
    }

    public override void ExecuteRequests(bool allowPartialExecution)
    {
      while (tasks.Count >= batchSize)
        ExecuteBatch(batchSize, null);

      if (!allowPartialExecution)
        ExecuteBatch(tasks.Count, null);
    }

    public override IEnumerator<Tuple> ExecuteRequestsWithReader(SqlQueryRequest request)
    {
      while (tasks.Count >= batchSize)
        ExecuteBatch(batchSize, null);

      return RunTupleReader(ExecuteBatch(tasks.Count, request), request.TupleDescriptor);
    }

    private DbDataReader ExecuteBatch(int numberOfTasks, SqlQueryRequest lastRequest)
    {
      if (numberOfTasks==0 && lastRequest==null)
        return null;
      AllocateCommand();
      DbDataReader reader = null;
      try {
        while (numberOfTasks > 0 && tasks.Count > 0) {
          numberOfTasks--;
          var task = tasks.Dequeue();
          var persistTask = task as SqlPersistTask;
          if (persistTask!=null) {
            var part = factory.CreatePersistCommandPart(persistTask, GetParameterPrefix());
            activeCommand.AddPart(part);
          }
          var queryTask = task as SqlQueryTask;
          if (queryTask!=null) {
            var part = factory.CreateQueryCommandPart(queryTask, GetParameterPrefix());
            activeCommand.AddPart(part, queryTask);
          }
        }
        if (lastRequest!=null) {
          var part = factory.CreateQueryCommandPart(new SqlQueryTask(lastRequest), DefaultParameterNamePrefix);
          activeCommand.AddPart(part);
        }
        if (activeCommand.QueryTasks.Count==0 && lastRequest==null) {
          activeCommand.ExecuteNonQuery();
          return null;
        }
        reader = activeCommand.ExecuteReader();
        if (activeCommand.QueryTasks.Count > 0) {
          int currentQueryTask = 0;
          while (currentQueryTask < activeCommand.QueryTasks.Count) {
            var queryTask = activeCommand.QueryTasks[currentQueryTask];
            var descriptor = queryTask.Request.TupleDescriptor;
            var accessor = DomainHandler.GetDataReaderAccessor(descriptor);
            var result = queryTask.Result;
            while (Driver.ReadRow(reader)) {
              var tuple = Tuple.Create(descriptor);
              accessor.Read(reader, tuple);
              result.Add(tuple);
            }
            reader.NextResult();
            currentQueryTask++;
          }
        }
        return lastRequest!=null ? reader : null;
      }
      finally {
        if (reader!=null && lastRequest==null)
          reader.Dispose();
        DisposeCommand();
      }
    }


    // Constructors

    public BatchingCommandProcessor(SessionHandler sessionHandler, int batchSize)
      : base(sessionHandler)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(batchSize, 1, "batchSize");
      this.batchSize = batchSize;
    }
  }
}