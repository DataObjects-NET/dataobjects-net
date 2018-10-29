// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

    bool ISqlTaskProcessor.ProcessTask(SqlLoadTask task)
    {
      var part = Factory.CreateQueryPart(task, GetParameterPrefix());
      if (!ValidateCommandParameterCount(activeCommand, part))
        return false;
      activeCommand.AddPart(part);
      activeTasks.Add(task);
      return true;
    }

    bool ISqlTaskProcessor.ProcessTask(SqlPersistTask task)
    {
      if (task.ValidateRowCount)
        return ProcessUnbatchedTask(task);

      var sequence = Factory.CreatePersistParts(task, GetParameterPrefix()).ToArray();
      if (!ValidateCommandParameterCount(activeCommand, sequence))
        return false;

      foreach (var part in sequence)
        activeCommand.AddPart(part);
      return true;
    }

    public override void ExecuteTasks(bool allowPartialExecution)
    {
      while (tasks.Count >= batchSize)
        ExecuteBatch(batchSize, null);

      while (!allowPartialExecution && tasks.Count > 0)
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
      ArgumentValidator.EnsureArgumentNotNull(request, "request");

      while (tasks.Count >= batchSize)
        ExecuteBatch(batchSize, null);

      for (;;) {
        var result = ExecuteBatch(tasks.Count, request);
        if (result!=null && tasks.Count==0)
          return result.AsReaderOf(request);
      }
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
      if (numberOfTasks==0 && lastRequest==null)
        return null;

      AllocateCommand();

      var shouldReturnReader = false;
      try {
        for (; numberOfTasks > 0 && tasks.Count > 0 && tasks.Peek().ProcessWith(this); numberOfTasks--)
          tasks.Dequeue();

        if (lastRequest!=null && tasks.Count==0) {
          var part = Factory.CreateQueryPart(lastRequest);
          if (ValidateCommandParameterCount(activeCommand, part)) {
            shouldReturnReader = true;
            activeCommand.AddPart(part);
          }
        }

        if (activeCommand.Count==0)
          return null;
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

    private bool ProcessUnbatchedTask(SqlPersistTask task)
    {
      if (activeCommand.Count > 0) {
        activeCommand.ExecuteNonQuery();
        ReleaseCommand();
        AllocateCommand();
      }

      return ExecuteUnbatchedTask(task);
    }

    private bool ExecuteUnbatchedTask(SqlPersistTask task)
    {
      var sequence = Factory.CreatePersistParts(task).ToArray();
      if (!ValidateCommandParameterCount(null, sequence))
        return false;

      foreach (var part in sequence) {
        using (var command = Factory.CreateCommand()) {
          command.AddPart(part);
          var affectedRowsCount = command.ExecuteNonQuery();
          if (affectedRowsCount==0)
            throw new VersionConflictException(string.Format(
              Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, task.EntityKey));
        }
      }

      return true;
    }

    private string GetParameterPrefix()
    {
      return string.Format("p{0}_", activeCommand.Count + 1);
    }

    #endregion

    // Constructors

    public BatchingCommandProcessor(CommandFactory factory, int batchSize, int maxQueryParameterCount)
      : base(factory, maxQueryParameterCount)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(batchSize, 1, "batchSize");
      this.batchSize = batchSize;
      tasks = new Queue<SqlTask>();
    }
  }
}