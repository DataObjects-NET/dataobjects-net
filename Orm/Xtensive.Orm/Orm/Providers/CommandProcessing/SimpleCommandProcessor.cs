// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  internal sealed class SimpleCommandProcessor : CommandProcessor, ISqlTaskProcessor
  {
    private readonly Queue<SqlTask> tasks;

    void ISqlTaskProcessor.ProcessTask(SqlLoadTask task, CommandProcessorContext context)
    {
      var part = Factory.CreateQueryPart(task);
      context.ActiveCommand.AddPart(part);
      context.ActiveTasks.Add(task);
    }

    void ISqlTaskProcessor.ProcessTask(SqlPersistTask task, CommandProcessorContext context)
    {
      var sequence = Factory.CreatePersistParts(task);
      foreach (var part in sequence) {
        try {
          context.ActiveCommand.AddPart(part);
          var affectedRowsCount = context.ActiveCommand.ExecuteNonQuery();
          if (task.ValidateRowCount && affectedRowsCount==0)
            throw new VersionConflictException(string.Format(
              Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, task.EntityKey));
        }
        finally {
          context.ActiveCommand.DisposeSafely();
          ReleaseCommand(context);
        }
        AllocateCommand(context);
      }
    }

    public override void RegisterTask(SqlTask task)
    {
      tasks.Enqueue(task);
    }

    public override void ClearTasks()
    {
      tasks.Clear();
    }

    public override void ExecuteTasks(CommandProcessorContext context)
    {
      context.ProcessingTasks = new Queue<SqlTask>(tasks);
      tasks.Clear();

      while (context.ProcessingTasks.Count > 0) {
        AllocateCommand(context);
        try {
          context.ProcessingTasks.Dequeue().ProcessWith(this, context);
        }
        finally {
          context.ActiveCommand.Dispose();
          ReleaseCommand(context);
        }
      }
    }

    public override async Task ExecuteTasksAsync(CommandProcessorContext context, CancellationToken token)
    {
      context.ProcessingTasks = new Queue<SqlTask>(tasks);
      tasks.Clear();

      while (context.ProcessingTasks.Count > 0) {
        AllocateCommand(context);
        try {
          var task = context.ProcessingTasks.Dequeue();
          task.ProcessWith(this, context);
          var loadTask = context.ActiveTasks.FirstOrDefault();
          if (loadTask != null) {
            await context.ActiveCommand.ExecuteReaderAsync(token).ConfigureAwait(false);
            var enumerator = context.ActiveCommand.AsReaderOf(context.ActiveTasks.First().Request);
            using (enumerator) {
              while (enumerator.MoveNext())
                loadTask.Output.Add(enumerator.Current);
            }
            context.ActiveTasks.Clear();
          }
        }
        finally {
          context.ActiveCommand.Dispose();
          ReleaseCommand(context);
        }
      }
    }

    public override IEnumerator<Tuple> ExecuteTasksWithReader(QueryRequest lastRequest, CommandProcessorContext context)
    {
      var oldValue = context.AllowPartialExecution;
      context.AllowPartialExecution = false;
      ExecuteTasks(context);
      context.AllowPartialExecution = oldValue;

      var lastRequestCommand = Factory.CreateCommand();
      var commandPart = Factory.CreateQueryPart(lastRequest);
      lastRequestCommand.AddPart(commandPart);
      lastRequestCommand.ExecuteReader();
      return lastRequestCommand.AsReaderOf(lastRequest);
    }

    public override async Task<IEnumerator<Tuple>> ExecuteTasksWithReaderAsync(QueryRequest lastRequest, CommandProcessorContext context, CancellationToken token)
    {
      var oldValue = context.AllowPartialExecution;
      context.AllowPartialExecution = false;

      token.ThrowIfCancellationRequested();

      await ExecuteTasksAsync(context, token);
      context.AllowPartialExecution = oldValue;

      var lastRequestCommand = Factory.CreateCommand();
      var commandPart = Factory.CreateQueryPart(lastRequest);
      lastRequestCommand.AddPart(commandPart);
      token.ThrowIfCancellationRequested();
      await lastRequestCommand.ExecuteReaderAsync(token);
      return lastRequestCommand.AsReaderOf(lastRequest);
    }

    // Constructors

      public SimpleCommandProcessor(CommandFactory factory)
      : base(factory)
    {
      tasks = new Queue<SqlTask>();
    }
  }
}