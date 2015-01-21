// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  internal sealed class BatchingCommandProcessor : CommandProcessor, ISqlTaskProcessor
  {
    private readonly int batchSize;
    private readonly Queue<SqlTask> tasks;
    private readonly Dictionary<Guid, CommandInfo> activeCommands;

    void ISqlTaskProcessor.ProcessTask(SqlLoadTask task, Guid uniqueIdentifier)
    {
      var part = Factory.CreateQueryPart(task, GetParameterPrefix(uniqueIdentifier));
      CommandInfo info = activeCommands[uniqueIdentifier];
      info.Command.AddPart(part);
      info.SelectTasks.Add(task);
    }

    void ISqlTaskProcessor.ProcessTask(SqlPersistTask task, Guid uniqueIdentifier)
    {
      if (task.ValidateRowCount) {
        ProcessUnbatchedTask(task, uniqueIdentifier);
        return;
      }
      var sequence = Factory.CreatePersistParts(task, GetParameterPrefix(uniqueIdentifier));
      var commandInfo = activeCommands[uniqueIdentifier];
      foreach (var part in sequence)
        commandInfo.Command.AddPart(part);
    }

    public override void ExecuteTasks(ExecutionBehavior executionBehavior)
    {
      if (executionBehavior==ExecutionBehavior.ExecuteWithNextQuery)
        return;
      
      while (tasks.Count >= batchSize)
        ExecuteBatch(tasks, batchSize, null, Guid.NewGuid());

      if (executionBehavior==ExecutionBehavior.PartialExecutionIsNotAllowed)
        ExecuteBatch(tasks, tasks.Count, null, Guid.NewGuid());
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
      var uniqueIdentifier = Guid.NewGuid();
      var queue = new Queue<SqlTask>(tasks);
      tasks.Clear();

      while (queue.Count >= batchSize)
        ExecuteBatch(queue, batchSize, null, uniqueIdentifier);

      return ExecuteBatch(queue, queue.Count, request, uniqueIdentifier).AsReaderOf(request);
    }

    #region Private / internal methods

    private CommandInfo AllocateCommand(Guid uniqueQueryIndentifier)
    {
      CommandInfo info;
      if (activeCommands.TryGetValue(uniqueQueryIndentifier, out info))
        info.ReentersCount++;
      else {
        info = new CommandInfo {Command = Factory.CreateCommand(),SelectTasks = new List<SqlLoadTask>(), ReentersCount = 0};
        activeCommands.Add(uniqueQueryIndentifier, info);
      }
      return info;
    }

    private void ReleaseCommand(Guid uniqueQueryIdentifier)
    {
      CommandInfo info;
      if (activeCommands.TryGetValue(uniqueQueryIdentifier, out info)) {
        if (info.ReentersCount > 0) {
          info.ReentersCount--;
          info.Command = Factory.CreateCommand();
          info.SelectTasks = new List<SqlLoadTask>();
        }
        else {
          info.Command = null;
          info.SelectTasks = null;
          activeCommands.Remove(uniqueQueryIdentifier);
        }
        return;
      }
      throw new InvalidOperationException();
    }

    private Command ExecuteBatch(Queue<SqlTask> tasks, int numberOfTasks, QueryRequest lastRequest, Guid uniqueIdentifier)
    {
      var shouldReturnReader = lastRequest!=null;

      if (numberOfTasks==0 && !shouldReturnReader)
        return null;

      AllocateCommand(uniqueIdentifier);
      CommandInfo commandInfo = null;
      try {
        while (numberOfTasks > 0 && tasks.Count > 0) {
          numberOfTasks--;
          var task = tasks.Dequeue();
          task.ProcessWith(this, uniqueIdentifier);
        }
        commandInfo = activeCommands[uniqueIdentifier];
        if (shouldReturnReader) {
          var part = Factory.CreateQueryPart(lastRequest);
          commandInfo.Command.AddPart(part);
        }
        if (commandInfo.Command.Count==0)
          return null;
        var hasQueryTasks = commandInfo.SelectTasks.Count > 0;
        if (!hasQueryTasks && !shouldReturnReader) {
          commandInfo.Command.ExecuteNonQuery();
          return null;
        }
        commandInfo.Command.ExecuteReader();
        if (hasQueryTasks) {
          int currentQueryTask = 0;
          while (currentQueryTask < commandInfo.SelectTasks.Count) {
            var queryTask = commandInfo.SelectTasks[currentQueryTask];
            var accessor = queryTask.Request.GetAccessor();
            var result = queryTask.Output;
            while (commandInfo.Command.NextRow())
              result.Add(commandInfo.Command.ReadTupleWith(accessor));
            commandInfo.Command.NextResult();
            currentQueryTask++;
          }
        }
        return shouldReturnReader ? commandInfo.Command : null;
      }
      finally {
        if (!shouldReturnReader)
          commandInfo.Command.Dispose();
        ReleaseCommand(uniqueIdentifier);
      }
    }
    private void ProcessUnbatchedTask(SqlPersistTask task, Guid uniqueIdentifier)
    {
      var commandInfo = activeCommands[uniqueIdentifier];
      if (commandInfo.Command.Count > 0) {
        commandInfo.Command.ExecuteNonQuery();
        ReleaseCommand(uniqueIdentifier);
        AllocateCommand(uniqueIdentifier);
      }
      ExecuteUnbatchedTask(task);
    }

    private void ExecuteUnbatchedTask(SqlPersistTask task)
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

#region Async methods
#if NET45

    /// <inheritdoc />
    public override async Task ExecuteTasksAsync(ExecutionBehavior executionBehavior, CancellationToken token)
    {
      if (executionBehavior==ExecutionBehavior.ExecuteWithNextQuery)
        return;
      var uniqueIdentifier = Guid.NewGuid();
      var queueOfTasks = new Queue<SqlTask>(tasks);
      tasks.Clear();
      while (queueOfTasks.Count >= batchSize)
        await ExecuteBatchAsync(queueOfTasks, batchSize, null, token, uniqueIdentifier);

      if (executionBehavior==ExecutionBehavior.PartialExecutionIsNotAllowed)
        await ExecuteBatchAsync(queueOfTasks, queueOfTasks.Count, null, token,  uniqueIdentifier);
    }

    /// <inheritdoc />
    public override async Task<Command> ExecuteTasksWithReaderAsync(QueryRequest request, CancellationToken token)
    {
      var uniqueIdentifier = Guid.NewGuid();
      var queueOfTasks = new Queue<SqlTask>(tasks);
      tasks.Clear();
      while (queueOfTasks.Count >= batchSize)
        await ExecuteBatchAsync(queueOfTasks, batchSize, null, token, uniqueIdentifier);

      return (await ExecuteBatchAsync(queueOfTasks, queueOfTasks.Count, request, token, uniqueIdentifier));
    }

    private async Task<Command> ExecuteBatchAsync(Queue<SqlTask> tasks, int numberOfTasks, QueryRequest lastRequest, CancellationToken token, Guid uniqueQueryIdentifier)
    {
      var shouldReturnReader = lastRequest != null;

      if (numberOfTasks == 0 && !shouldReturnReader)
        return null;

      var commandInfo = AllocateCommand(uniqueQueryIdentifier);

      try {
        while (numberOfTasks > 0 && tasks.Count > 0) {
          numberOfTasks--;
          var task = tasks.Dequeue();
          task.ProcessWith(this, uniqueQueryIdentifier);
        }
        if (shouldReturnReader) {
          var part = Factory.CreateQueryPart(lastRequest);
          commandInfo.Command.AddPart(part);
        }
        if (commandInfo.Command.Count==0)
          return null;
        var hasQueryTasks = commandInfo.SelectTasks.Count > 0;
        if (!hasQueryTasks && !shouldReturnReader) {
          await commandInfo.Command.ExecuteNonQueryAsync(token);
          return null;
        }
        await commandInfo.Command.ExecuteReaderAsync(token);
        if (hasQueryTasks) {
          int currentQueryTask = 0;
          while (currentQueryTask < commandInfo.SelectTasks.Count) {
            var queryTask = commandInfo.SelectTasks[currentQueryTask];
            var accessor = queryTask.Request.GetAccessor();
            var result = queryTask.Output;
            while (commandInfo.Command.NextRow())
              result.Add(commandInfo.Command.ReadTupleWith(accessor));
            commandInfo.Command.NextResult();
            currentQueryTask++;
          }
        }
        return shouldReturnReader ? commandInfo.Command : null;
      }
      finally {
        if (!shouldReturnReader)
          commandInfo.Command.Dispose();
        ReleaseCommand(uniqueQueryIdentifier);
      }
    }
#endif
#endregion

    private string GetParameterPrefix(Guid uniqueIdentifier)
    {
      var commandInfo = activeCommands[uniqueIdentifier];
      return string.Format("p{0}_", commandInfo.Command.Count + 1);
    }

    #endregion

    // Constructors

    public BatchingCommandProcessor(CommandFactory factory, int batchSize)
      : base(factory)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(batchSize, 1, "batchSize");
      this.batchSize = batchSize;
      tasks = new Queue<SqlTask>();
      activeCommands = new Dictionary<Guid, CommandInfo>();
    }
  }
}