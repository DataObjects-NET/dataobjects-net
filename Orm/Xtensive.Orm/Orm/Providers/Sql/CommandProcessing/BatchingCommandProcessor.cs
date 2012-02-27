// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers.Sql
{
  internal sealed class BatchingCommandProcessor : CommandProcessor, ISqlTaskProcessor
  {
    private readonly int batchSize;

    private int reenterCount;
    private Command activeCommand;
    private List<SqlLoadTask> activeTasks;

    void ISqlTaskProcessor.ProcessTask(SqlLoadTask task)
    {
      var part = Factory.CreateQueryPart(task, GetParameterPrefix());
      activeCommand.AddPart(part);
      activeTasks.Add(task);
    }

    void ISqlTaskProcessor.ProcessTask(SqlPersistTask task)
    {
      var sequence = Factory.CreatePersistParts(task, GetParameterPrefix());
      foreach (var part in sequence)
        activeCommand.AddPart(part);
    }

    public override void ExecuteTasks(bool allowPartialExecution)
    {
      while (Tasks.Count >= batchSize)
        ExecuteBatch(batchSize, null);

      if (!allowPartialExecution)
        ExecuteBatch(Tasks.Count, null);
    }

    public override IEnumerator<Tuple> ExecuteTasksWithReader(QueryRequest request)
    {
      while (Tasks.Count >= batchSize)
        ExecuteBatch(batchSize, null);

      return ExecuteBatch(Tasks.Count, request).AsReaderOf(request);
    }

    #region Private / internal methods

    private void AllocateCommand()
    {
      if (activeCommand != null)
        reenterCount++;
      else {
        activeTasks = new List<SqlLoadTask>();
        activeCommand = Factory.CreateCommand();
      }
    }

    private void ReleaseCommand()
    {
      if (reenterCount > 0) {
        reenterCount--;
        activeTasks = new List<SqlLoadTask>();
        activeCommand = Factory.CreateCommand();
      }
      else {
        activeCommand = null;
        activeTasks = null;
      }
    }

    private Command ExecuteBatch(int numberOfTasks, QueryRequest lastRequest)
    {
      var shouldReturnReader = lastRequest!=null;

      if (numberOfTasks==0 && !shouldReturnReader)
        return null;

      AllocateCommand();

      try {
        while (numberOfTasks > 0 && Tasks.Count > 0) {
          numberOfTasks--;
          var task = Tasks.Dequeue();
          task.ProcessWith(this);
        }
        if (shouldReturnReader) {
          var part = Factory.CreateQueryPart(lastRequest);
          activeCommand.AddPart(part);
        }
        var hasQueryTasks = activeTasks.Count > 0;
        if (!hasQueryTasks && !shouldReturnReader) {
          activeCommand.ExecuteNonQuery();
          return null;
        }
        activeCommand.ExecuteReader();
        if (hasQueryTasks) {
          int currentQueryTask = 0;
          while (currentQueryTask < activeTasks.Count) {
            var queryTask = activeTasks[currentQueryTask];
            var accessor = queryTask.Request.GetAccessor();
            var result = queryTask.Output;
            while (activeCommand.NextRow())
              result.Add(activeCommand.ReadTupleWith(accessor));
            activeCommand.NextResult();
            currentQueryTask++;
          }
        }
        return shouldReturnReader ? activeCommand : null;
      }
      finally {
        if (!shouldReturnReader)
          activeCommand.Dispose();
        ReleaseCommand();
      }
    }

    private string GetParameterPrefix()
    {
      return string.Format("p{0}_", activeCommand.Count + 1);
    }

    #endregion

    // Constructors

    public BatchingCommandProcessor(CommandFactory factory, int batchSize)
      : base(factory)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(batchSize, 1, "batchSize");
      this.batchSize = batchSize;
    }
  }
}