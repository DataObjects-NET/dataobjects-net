// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  internal sealed class BatchingCommandProcessor : CommandProcessor, ISqlTaskProcessor
  {
    private readonly int batchSize;
    private readonly Queue<SqlTask> tasks;

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
      if (task.ValidateRowCount) {
        ExecuteCurrentBatch();
        ProcessUnbatchedTask(task);
        ResetCommand();
        return;
      }
      var sequence = Factory.CreatePersistParts(task, GetParameterPrefix());
      foreach (var part in sequence)
        activeCommand.AddPart(part);
    }

    public override void ExecuteTasks(bool allowPartialExecution)
    {
      while (tasks.Count >= batchSize)
        ExecuteBatch(batchSize, null);

      if (!allowPartialExecution)
        ExecuteBatch(tasks.Count, null);
    }

    public override void RegisterTask(SqlTask task)
    {
      tasks.Enqueue(task);
    }

    public override void ClearTasks()
    {
      tasks.Clear();
    }

    public override IEnumerator<Tuple> ExecuteTasksWithReader(QueryRequest request)
    {
      while (tasks.Count >= batchSize)
        ExecuteBatch(batchSize, null);

      return ExecuteBatch(tasks.Count, request).AsReaderOf(request);
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

    private void ResetCommand()
    {
      ReleaseCommand();
      AllocateCommand();
    }

    private Command ExecuteBatch(int numberOfTasks, QueryRequest lastRequest)
    {
      var shouldReturnReader = lastRequest!=null;

      if (numberOfTasks==0 && !shouldReturnReader)
        return null;

      AllocateCommand();

      try {
        while (numberOfTasks > 0 && tasks.Count > 0) {
          numberOfTasks--;
          var task = tasks.Dequeue();
          task.ProcessWith(this);
        }
        if (shouldReturnReader) {
          var part = Factory.CreateQueryPart(lastRequest);
          activeCommand.AddPart(part);
        }
        var hasQueryTasks = activeTasks.Count > 0;
        if (!hasQueryTasks && !shouldReturnReader && activeCommand.Count>0) {
          activeCommand.ExecuteNonQuery();
          return null;
        }
        if(activeCommand.Count>0)
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

    private void ProcessUnbatchedTask(SqlPersistTask task)
    {
      var sequence = Factory.CreatePersistParts(task);
      foreach (var part in sequence) {
        using (var command = Factory.CreateCommand()) {
          command.AddPart(part);
          var affectedRowsCount = command.ExecuteNonQuery();
          if (affectedRowsCount==0)
            throw new VersionConflictException(string.Format(
              Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, task.EntityKey));
        }
      }
    }

    private void ExecuteCurrentBatch()
    {
      if (activeCommand.Count>0)
        activeCommand.ExecuteNonQuery();
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
      tasks = new Queue<SqlTask>();
    }
  }
}