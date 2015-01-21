// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Internals;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  internal sealed class SimpleCommandProcessor : CommandProcessor, ISqlTaskProcessor
  {
    private readonly Queue<SqlTask> tasks;
    private readonly List<SqlLoadTask> loadTasks;

    public override void RegisterTask(SqlTask task)
    {
      tasks.Enqueue(task);
    }

    public override void ClearTasks()
    {
      tasks.Clear();
      loadTasks.Clear();
    }

    /// <inheritdoc />
    public override void ExecuteTasks(ExecutionBehavior executionBehavior)
    {
      while (tasks.Count > 0)
        tasks.Dequeue().ProcessWith(this, Guid.NewGuid());

      if (executionBehavior==ExecutionBehavior.ExecuteWithNextQuery || loadTasks.Count==0)
        return;
      var loads = new Queue<SqlLoadTask>(loadTasks);
      ClearTasks();
      ExecuteTasks(loads);
    }

    public override IEnumerator<Tuple> ExecuteTasksWithReader(QueryRequest lastRequest)
    {
      if (loadTasks.Count!=0) {
        var loads = new Queue<SqlLoadTask>(loadTasks);
        ClearTasks();
        ExecuteTasks(loads);
      }
      var returnedCommand = Factory.CreateCommand();
      var commandPart = Factory.CreateQueryPart(lastRequest);
      returnedCommand.AddPart(commandPart);
      returnedCommand.ExecuteReader();
      return returnedCommand.AsReaderOf(lastRequest);
    }

    void ISqlTaskProcessor.ProcessTask(SqlLoadTask task, Guid uniqueIdentifier)
    {
      loadTasks.Add(task);
    }

    void ISqlTaskProcessor.ProcessTask(SqlPersistTask task, Guid uniqueIdentifier)
    {
      var sequence = Factory.CreatePersistParts(task);
      foreach (var part in sequence) {
        using (var command = Factory.CreateCommand()) {
          command.AddPart(part);
          var affectedRowsCount = command.ExecuteNonQuery();
          if (task.ValidateRowCount && affectedRowsCount==0)
            throw new VersionConflictException(string.Format(
              Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, task.EntityKey));
        }
      }
    }

#if NET45

    /// <inheritdoc />
    public override async Task<Command> ExecuteTasksWithReaderAsync(QueryRequest lastRequest, CancellationToken token)
    {
      if (loadTasks.Count!=0) {
        var loads = new Queue<SqlLoadTask>(loadTasks);
        ClearTasks();
        await ExecuteTasksAsync(loads, token);
      }
      var returnedCommand = Factory.CreateCommand();
      var commandPart = Factory.CreateQueryPart(lastRequest);
      returnedCommand.AddPart(commandPart);
      await returnedCommand.ExecuteReaderAsync(token);
      return returnedCommand;
    }

    /// <inheritdoc />
    public override async Task ExecuteTasksAsync(ExecutionBehavior executionBehavior, CancellationToken token)
    {
      while (tasks.Count > 0)
        tasks.Dequeue().ProcessWith(this, Guid.NewGuid());

      if (executionBehavior==ExecutionBehavior.ExecuteWithNextQuery || loadTasks.Count == 0)
        return;
      var loads = new Queue<SqlLoadTask>(loadTasks);
      ClearTasks();
      await ExecuteTasksAsync(loads, token);
    }

    private async Task ExecuteTasksAsync(Queue<SqlLoadTask> sqlLoadTasks, CancellationToken token)
    {
      while (sqlLoadTasks.Count > 0) {
        var task = sqlLoadTasks.Dequeue();
        using (var command = Factory.CreateCommand()) {
          var part = Factory.CreateQueryPart(task);
          command.AddPart(part);
          await command.ExecuteReaderAsync(token);
          var enumerator = command.AsReaderOf(task.Request);
          using (enumerator) {
            while (enumerator.MoveNext())
              task.Output.Add(enumerator.Current);
          }
        }
      }
    }
#endif

    private void ExecuteTasks(Queue<SqlLoadTask> sqlLoadTasks)
    {
      while (sqlLoadTasks.Count > 0) {
        var task = sqlLoadTasks.Dequeue();
        using (var command = Factory.CreateCommand()) {
          var part = Factory.CreateQueryPart(task);
          command.AddPart(part);
          command.ExecuteReader();
          var enumerator = command.AsReaderOf(task.Request);
          using (enumerator) {
            while (enumerator.MoveNext())
              task.Output.Add(enumerator.Current);
          }
        }
      }
    }

    // Constructors

    public SimpleCommandProcessor(CommandFactory factory)
      : base(factory)
    {
      tasks = new Queue<SqlTask>();
      loadTasks = new List<SqlLoadTask>();
    }
  }
}