// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Sql
{
  internal class BatchingCommandProcessor : CommandProcessor
  {
    private readonly List<string> registeredCommands = new List<string>();
    private readonly List<SqlQueryTask> registeredQueryTasks = new List<SqlQueryTask>();

    private int batchSize;
    
    public override void ExecuteRequests(bool dirty)
    {
      while (persistTasks.Count >= batchSize) {
        ExecuteBatch(batchSize, 0, null);
      }
      
      if (queryTasks.Count==0) {
        if (!dirty)
          ExecuteBatch(persistTasks.Count, 0, null);
        return;
      }

      int persistAmount = persistTasks.Count;
      int queryAmount = Math.Min(queryTasks.Count, batchSize - persistAmount);

      ExecuteBatch(persistAmount, queryAmount, null);

      while (queryTasks.Count >= batchSize) {
        ExecuteBatch(0, batchSize, null);
      }

      if (!dirty)
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
      bool hasPersists = persistAmount > 0;
      bool hasQueries = queryAmount > 0;
      bool hasLastQuery = lastRequest!=null;
      if (!hasPersists && !hasQueries && !hasLastQuery)
        return null;
      CreateCommand();
      DbDataReader reader = null;
      try {
        if (hasPersists)
          RegisterPersists(persistAmount);
        if (hasQueries)
          RegisterQueries(queryAmount);
        if (hasLastQuery)
          registeredCommands.Add(CreateQueryCommandPart(new SqlQueryTask(lastRequest), DefaultParameterNamePrefix));
        command.CommandText = driver.BuildBatch(registeredCommands.ToArray());
        if (!hasQueries && !hasLastQuery) {
          driver.ExecuteNonQuery(command);
          return null;
        }
        reader = driver.ExecuteReader(command);
        int currentTaskNumber = 0;
        while (currentTaskNumber < registeredQueryTasks.Count) {
          var task = registeredQueryTasks[currentTaskNumber];
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
        return hasLastQuery ? reader : null;
      }
      finally {
        if (reader!=null && !hasLastQuery)
          reader.Dispose();
        DisposeCommand();
      }
    }

    private new void DisposeCommand()
    {
      base.DisposeCommand();
      registeredCommands.Clear();
      registeredQueryTasks.Clear();
    }

    private void RegisterQueries(int amount)
    {
      for (int i = 0; i < amount; i++) {
        var task = queryTasks.First.Value;
        registeredQueryTasks.Add(task);
        registeredCommands.Add(CreateQueryCommandPart(task, GetParameterPrefix()));
        queryTasks.RemoveFirst();
      }
    }

    private void RegisterPersists(int amount)
    {
      for (int i = 0; i < amount; i++) {
        var task = persistTasks.First.Value;
        registeredCommands.Add(CreatePersistCommandPart(task, GetParameterPrefix()));
        persistTasks.RemoveFirst();
      }
    }

    private string GetParameterPrefix()
    {
      return string.Format("p{0}_", registeredCommands.Count);
    }
    
    // Constructors

    public BatchingCommandProcessor(DomainHandler domainHandler, int batchSize)
      : base(domainHandler)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(batchSize, 1, "batchSize");
      this.batchSize = batchSize;
    }
  }
}