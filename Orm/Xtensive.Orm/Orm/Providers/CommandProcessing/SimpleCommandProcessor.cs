// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;

namespace Xtensive.Orm.Providers
{
  internal sealed class SimpleCommandProcessor : CommandProcessor, ISqlTaskProcessor
  {
    // equals to default batch size from SessionConfiguration
    // hard to choose particular value so let it be some known number :)
    private const int DefaultTaskQueueCapacity = 25;

    private Queue<SqlTask> tasks = new(DefaultTaskQueueCapacity);

    void ISqlTaskProcessor.ProcessTask(SqlLoadTask task, CommandProcessorContext context)
    {
      var part = Factory.CreateQueryPart(task);
      ValidateCommandPartParameters(part);
      context.ActiveCommand.AddPart(part);
      context.ActiveTasks.Add(task);
    }

    void ISqlTaskProcessor.ProcessTask(SqlPersistTask task, CommandProcessorContext context)
    {
      var sequence = Factory.CreatePersistParts(task);
      foreach (var part in sequence) {
        try {
          ValidateCommandPartParameters(part);
          context.ActiveCommand.AddPart(part);
          var affectedRowsCount = context.ActiveCommand.ExecuteNonQuery();
          if (task.ValidateRowCount && affectedRowsCount == 0) {
            throw new VersionConflictException(string.Format(
              Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, task.EntityKey));
          }
        }
        finally {
          context.ActiveCommand.DisposeSafely();
          ReleaseCommand(context);
        }
        AllocateCommand(context);
      }
    }

    public override void RegisterTask(SqlTask task) => tasks.Enqueue(task);

    public override void ClearTasks() => tasks.Clear();

    public override void ExecuteTasks(CommandProcessorContext context)
    {
      context.ProcessingTasks = tasks;
      tasks = new Queue<SqlTask>(DefaultTaskQueueCapacity);

      while (context.ProcessingTasks.Count > 0) {
        AllocateCommand(context);
        try {
          var task = context.ProcessingTasks.Dequeue();
          task.ProcessWith(this, context);
          var loadTask = context.ActiveTasks.FirstOrDefault();
          if (loadTask != null) {
            context.ActiveCommand.ExecuteReader();
            var enumerator = context.ActiveCommand.CreateReader(loadTask.Request.GetAccessor());
            using (enumerator) {
              while (enumerator.MoveNext()) {
                loadTask.Output.Add(enumerator.Current);
              }
            }
          }
        }
        finally {
          context.ActiveCommand.DisposeSafely();
          ReleaseCommand(context);
        }
      }
    }

    public override async Task ExecuteTasksAsync(CommandProcessorContext context, CancellationToken token)
    {
      context.ProcessingTasks = tasks;
      tasks = new Queue<SqlTask>(DefaultTaskQueueCapacity);

      while (context.ProcessingTasks.Count > 0) {
        AllocateCommand(context);
        try {
          var task = context.ProcessingTasks.Dequeue();
          task.ProcessWith(this, context);
          var loadTask = context.ActiveTasks.FirstOrDefault();
          if (loadTask!=null) {
            await context.ActiveCommand.ExecuteReaderAsync(token).ConfigureAwaitFalse();
            var reader = context.ActiveCommand.CreateReader(loadTask.Request.GetAccessor(), token);
            await using (reader.ConfigureAwaitFalse()) {
              while (await reader.MoveNextAsync().ConfigureAwaitFalse()) {
                loadTask.Output.Add(reader.Current);
              }
            }
            context.ActiveTasks.Clear();
          }
        }
        finally {
          await context.ActiveCommand.DisposeSafelyAsync().ConfigureAwaitFalse();
          ReleaseCommand(context);
        }
      }
    }

    public override DataReader ExecuteTasksWithReader(QueryRequest lastRequest, CommandProcessorContext context)
    {
      var oldValue = context.AllowPartialExecution;
      context.AllowPartialExecution = false;
      ExecuteTasks(context);
      context.AllowPartialExecution = oldValue;

      var lastRequestCommand = Factory.CreateCommand();
      var commandPart = Factory.CreateQueryPart(lastRequest, context.ParameterContext);
      ValidateCommandPartParameters(commandPart);
      lastRequestCommand.AddPart(commandPart);
      lastRequestCommand.ExecuteReader();
      return lastRequestCommand.CreateReader(lastRequest.GetAccessor());
    }

    public override async Task<DataReader> ExecuteTasksWithReaderAsync(QueryRequest lastRequest, CommandProcessorContext context, CancellationToken token)
    {
      var oldValue = context.AllowPartialExecution;
      context.AllowPartialExecution = false;

      token.ThrowIfCancellationRequested();

      await ExecuteTasksAsync(context, token).ConfigureAwaitFalse();
      context.AllowPartialExecution = oldValue;

      var lastRequestCommand = Factory.CreateCommand();
      var commandPart = Factory.CreateQueryPart(lastRequest, context.ParameterContext);
      ValidateCommandPartParameters(commandPart);
      lastRequestCommand.AddPart(commandPart);
      token.ThrowIfCancellationRequested();
      await lastRequestCommand.ExecuteReaderAsync(token).ConfigureAwaitFalse();
      return lastRequestCommand.CreateReader(lastRequest.GetAccessor());
    }

    // Constructors

    public SimpleCommandProcessor(CommandFactory factory, int maxQueryParameterCount)
      : base(factory, maxQueryParameterCount)
    {
    }
  }
}
