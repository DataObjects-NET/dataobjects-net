// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A command processor that arranges incoming commands into series of batches
  /// to minimize client-server network roundtrips.
  /// </summary>
  public sealed class BatchingCommandProcessor : CommandProcessor
  {
    private readonly int batchSize;

    /// <inheritdoc/>
    public override void ExecuteRequests(bool allowPartialExecution)
    {
      while (tasks.Count >= batchSize)
        ExecuteBatch(batchSize, null);

      if (!allowPartialExecution)
        ExecuteBatch(tasks.Count, null);
    }

    /// <inheritdoc/>
    public override void ProcessTask(SqlQueryTask task)
    {
      var part = factory.CreateQueryCommandPart(task, GetParameterPrefix());
      activeCommand.AddPart(part, task);
    }

    /// <inheritdoc/>
    public override void ProcessTask(SqlPersistTask task)
    {
      var sequence = factory.CreatePersistCommandPart(task, GetParameterPrefix());
      foreach (var part in sequence)
        activeCommand.AddPart(part);
    }

    /// <inheritdoc/>
    public override IEnumerator<Tuple> ExecuteRequestsWithReader(QueryRequest request)
    {
      while (tasks.Count >= batchSize)
        ExecuteBatch(batchSize, null);

      return RunTupleReader(ExecuteBatch(tasks.Count, request), request.TupleDescriptor);
    }

    #region Private / internal methods

    private DbDataReader ExecuteBatch(int numberOfTasks, QueryRequest lastRequest)
    {
      if (numberOfTasks==0 && lastRequest==null)
        return null;
      AllocateCommand();
      DbDataReader reader = null;
      try {
        while (numberOfTasks > 0 && tasks.Count > 0) {
          numberOfTasks--;
          var task = tasks.Dequeue();
          task.ProcessWith(this);
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
            var accessor = domainHandler.GetDataReaderAccessor(descriptor);
            var result = queryTask.Output;
            while (driver.ReadRow(reader)) {
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
        if (reader!=null && lastRequest==null) {
          reader.Dispose();
          DisposeCommand();
        }
      }
    }

    private string GetParameterPrefix()
    {
      return string.Format("p{0}_", activeCommand.Statements.Count + 1);
    }

    #endregion

    // Constructors

    public BatchingCommandProcessor(
      DomainHandler domainHandler, Session session,
      SqlConnection connection, CommandPartFactory factory, int batchSize)
      : base(domainHandler, session, connection, factory)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(batchSize, 1, "batchSize");
      this.batchSize = batchSize;
    }
  }
}