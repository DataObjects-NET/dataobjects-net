// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  internal class BatchingCommandProcessor : CommandProcessor
  {
    private int batchSize;
    
    public override void ExecuteRequests(bool allowPartialExecution)
    {
      while (persistTasks.Count >= batchSize) {
        ExecuteBatch(batchSize, 0, null);
      }
      
      if (queryTasks.Count==0) {
        if (!allowPartialExecution)
          ExecuteBatch(persistTasks.Count, 0, null);
        return;
      }

      int persistAmount = persistTasks.Count;
      int queryAmount = Math.Min(queryTasks.Count, batchSize - persistAmount);

      ExecuteBatch(persistAmount, queryAmount, null);

      while (queryTasks.Count >= batchSize) {
        ExecuteBatch(0, batchSize, null);
      }

      if (!allowPartialExecution)
        ExecuteBatch(0, queryTasks.Count, null);
    }

    public override IEnumerator<Tuple> ExecuteRequestsWithReader(SqlQueryRequest request)
    {
      while (persistTasks.Count >= batchSize) {
        ExecuteBatch(batchSize, 0, null);
      }

      int persistAmount = persistTasks.Count;
      int queryAmount = Math.Min(queryTasks.Count, batchSize - persistAmount);

      if (persistAmount + queryAmount < batchSize) {
        return RunTupleReader(ExecuteBatch(persistAmount, queryAmount, request), request.TupleDescriptor);
      }

      ExecuteBatch(persistAmount, queryAmount, null);

      while (queryTasks.Count >= batchSize) {
        ExecuteBatch(0, batchSize, null);
      }

      return RunTupleReader(ExecuteBatch(0, queryTasks.Count, request), request.TupleDescriptor);
    }

    private DbDataReader ExecuteBatch(int persistAmount, int queryAmount, SqlQueryRequest lastRequest)
    {
      if (persistAmount==0 && queryAmount==0 && lastRequest==null)
        return null;
      AllocateCommand();
      DbDataReader reader = null;
      try {
        if (persistAmount > 0)
          persistAmount = RegisterPersists(persistAmount);
        List<SqlQueryTask> registeredTasks = null;
        if (queryAmount > 0) {
          registeredTasks = RegisterQueries(queryAmount);
          queryAmount = registeredTasks.Count;
        }
        if (lastRequest!=null) {
          var part = factory.CreateQueryCommandPart(new SqlQueryTask(lastRequest), DefaultParameterNamePrefix);
          activeCommand.AddPart(part);
        }
        if (queryAmount==0 && lastRequest==null) {
          activeCommand.ExecuteNonQuery();
          return null;
        }
        reader = activeCommand.ExecuteReader();
        if (registeredTasks!=null) {
          int currentTaskNumber = 0;
          while (currentTaskNumber < registeredTasks.Count) {
            var task = registeredTasks[currentTaskNumber];
            var descriptor = task.Request.TupleDescriptor;
            var accessor = domainHandler.GetDataReaderAccessor(descriptor);
            var output = task.Output;
            while (driver.ReadRow(reader)) {
              var tuple = Tuple.Create(descriptor);
              accessor.Read(reader, tuple);
              output.Add(tuple);
            }
            reader.NextResult();
            currentTaskNumber++;
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

    private List<SqlQueryTask> RegisterQueries(int amount)
    {
      var queries = new List<SqlQueryTask>();
      for (int i = 0; i < amount && queryTasks.Count > 0; i++) {
        var task = queryTasks.Dequeue();
        queries.Add(task);
        activeCommand.AddPart(factory.CreateQueryCommandPart(task, activeCommand.GetParameterPrefix()));
      }
      return queries;
    }

    private int RegisterPersists(int amount)
    {
      int count = 0;
      for (int i = 0; i < amount; i++) {
        var task = persistTasks.Dequeue();
        activeCommand.AddPart(factory.CreatePersistCommandPart(task, activeCommand.GetParameterPrefix()));
        count++;
      }
      return count;
    }

    
    // Constructors

    public BatchingCommandProcessor(DomainHandler domainHandler, SqlConnection connection,int batchSize)
      : base(domainHandler, connection)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(batchSize, 1, "batchSize");
      this.batchSize = batchSize;
    }
  }
}