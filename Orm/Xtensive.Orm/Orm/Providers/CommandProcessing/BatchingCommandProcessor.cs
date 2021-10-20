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
  internal sealed class BatchingCommandProcessor : CommandProcessor, ISqlTaskProcessor
  {
    private readonly int batchSize;
    private readonly Queue<SqlTask> tasks;

    void ISqlTaskProcessor.ProcessTask(SqlLoadTask task, CommandProcessorContext context)
    {
      var part = Factory.CreateQueryPart(task, GetParameterPrefix(context));
      if (PendCommandPart(context.ActiveCommand, part)) {
        return;
      }
      context.ActiveCommand.AddPart(part);
      context.ActiveTasks.Add(task);
      context.CurrentTask = null;
    }

    void ISqlTaskProcessor.ProcessTask(SqlPersistTask task, CommandProcessorContext context)
    {
      if (task.ValidateRowCount) {
        ProcessUnbatchedTask(task, context);
        context.CurrentTask = null;
        return;
      }
      var sequence = Factory.CreatePersistParts(task, GetParameterPrefix(context)).ToChainedBuffer();
      if (PendCommandParts(context.ActiveCommand, sequence)) {
        // higly recommended to no tear apart persist actions if they are batchable
        return;
      }
      foreach (var part in sequence) {
        context.ActiveCommand.AddPart(part);
      }

      context.CurrentTask = null;
    }

    public override void RegisterTask(SqlTask task) => tasks.Enqueue(task);

    public override void ClearTasks() => tasks.Clear();

    public override void ExecuteTasks(CommandProcessorContext context)
    {
      PutTasksForExecution(context);

      while (context.ProcessingTasks.Count >= batchSize) {
        _ = ExecuteBatch(batchSize, null, context);
      }

      while (!context.AllowPartialExecution && context.ProcessingTasks.Count > 0) {
        _ = ExecuteBatch(context.ProcessingTasks.Count, null, context);
      }
    }

    public override async Task ExecuteTasksAsync(CommandProcessorContext context, CancellationToken token)
    {
      PutTasksForExecution(context);

      while (context.ProcessingTasks.Count >= batchSize) {
        _ = await ExecuteBatchAsync(batchSize, null, context, token).ConfigureAwait(false);
      }

      while (!context.AllowPartialExecution && context.ProcessingTasks.Count > 0) {
        _ = await ExecuteBatchAsync(context.ProcessingTasks.Count, null, context, token).ConfigureAwait(false);
      }
    }

    public override DataReader ExecuteTasksWithReader(QueryRequest request, CommandProcessorContext context)
    {
      context.AllowPartialExecution = false;
      PutTasksForExecution(context);

      while (context.ProcessingTasks.Count >= batchSize) {
        _ = ExecuteBatch(batchSize, null, context);
      }

      for (;;) {
        var result = ExecuteBatch(context.ProcessingTasks.Count, request, context);
        if (result != null && context.ProcessingTasks.Count == 0) {
          return result.CreateReader(request.GetAccessor());
        }
      }
    }

    public override async Task<DataReader> ExecuteTasksWithReaderAsync(QueryRequest request,
      CommandProcessorContext context, CancellationToken token)
    {
      context.AllowPartialExecution = false;
      PutTasksForExecution(context);

      while (context.ProcessingTasks.Count >= batchSize) {
        _ = await ExecuteBatchAsync(batchSize, null, context, token).ConfigureAwait(false);
      }

      for (; ; ) {
        var result = await ExecuteBatchAsync(context.ProcessingTasks.Count, request, context, token).ConfigureAwait(false);
        if (result != null && context.ProcessingTasks.Count == 0) {
          return result.CreateReader(request.GetAccessor());
        }
      }
    }

    #region Private / internal methods

    private Command ExecuteBatch(int numberOfTasks, QueryRequest lastRequest, CommandProcessorContext context)
    {
      if (numberOfTasks == 0 && lastRequest == null) {
        return null;
      }

      var tasksToProcess = context.ProcessingTasks;

      AllocateCommand(context);

      var shouldReturnReader = false;
      try {
        while (numberOfTasks > 0 && tasksToProcess.Count > 0) {
          var task = tasksToProcess.Peek();
          context.CurrentTask = task;
          task.ProcessWith(this, context);
          if(context.CurrentTask==null) {
            numberOfTasks--;
            _ = tasksToProcess.Dequeue();
          }
          else {
            break;
          }
        }

        var command = context.ActiveCommand;
        if (lastRequest != null && tasksToProcess.Count == 0) {
          var part = Factory.CreateQueryPart(lastRequest, context.ParameterContext);
          if (!PendCommandPart(command, part)) {
            shouldReturnReader = true;
            command.AddPart(part);
          }
        }

        if (command.Count==0) {
          return null;
        }
        var hasQueryTasks = context.ActiveTasks.Count > 0;
        if (!hasQueryTasks && !shouldReturnReader) {
          _ = command.ExecuteNonQuery();
          return null;
        }

        command.ExecuteReader();
        if (hasQueryTasks) {
          var currentQueryTask = 0;
          while (currentQueryTask < context.ActiveTasks.Count) {
            var queryTask = context.ActiveTasks[currentQueryTask];
            var accessor = queryTask.Request.GetAccessor();
            var result = queryTask.Output;
            while (command.NextRow()) {
              result.Add(command.ReadTupleWith(accessor));
            }
            _ = command.NextResult();
            currentQueryTask++;
          }
        }

        return shouldReturnReader ? command : null;
      }
      finally {
        if (!shouldReturnReader) {
          context.ActiveCommand.DisposeSafely();
        }
        ReleaseCommand(context);
      }
    }

    private async Task<Command> ExecuteBatchAsync(int numberOfTasks, QueryRequest lastRequest,
      CommandProcessorContext context, CancellationToken token)
    {
      if (numberOfTasks == 0 && lastRequest == null) {
        return null;
      }

      var tasksToProcess = context.ProcessingTasks;

      AllocateCommand(context);

      var shouldReturnReader = false;
      try {
        while (numberOfTasks > 0 && tasksToProcess.Count > 0) {
          var task = tasksToProcess.Peek();
          context.CurrentTask = task;
          task.ProcessWith(this, context);
          if (context.CurrentTask == null) {
            numberOfTasks--;
            _ = tasksToProcess.Dequeue();
          }
          else {
            break;
          }
        }

        var command = context.ActiveCommand;
        if (lastRequest != null && tasksToProcess.Count == 0) {
          var part = Factory.CreateQueryPart(lastRequest, context.ParameterContext);
          if (!PendCommandPart(command, part)) {
            shouldReturnReader = true;
            command.AddPart(part);
          }
        }

        if (command.Count==0) {
          return null;
        }
        var hasQueryTasks = context.ActiveTasks.Count > 0;
        if (!hasQueryTasks && !shouldReturnReader) {
          _ = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
          return null;
        }

        await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (hasQueryTasks) {
          var currentQueryTask = 0;
          while (currentQueryTask < context.ActiveTasks.Count) {
            var queryTask = context.ActiveTasks[currentQueryTask];
            var accessor = queryTask.Request.GetAccessor();
            var result = queryTask.Output;
            while (command.NextRow()) {
              result.Add(command.ReadTupleWith(accessor));
            }
            _ = command.NextResult();
            currentQueryTask++;
          }
        }

        return shouldReturnReader ? command : null;
      }
      finally {
        if (!shouldReturnReader) {
          await context.ActiveCommand.DisposeSafelyAsync().ConfigureAwait(false);
        }

        ReleaseCommand(context);
      }
    }

    private void ProcessUnbatchedTask(SqlPersistTask task, CommandProcessorContext context)
    {
      if (context.ActiveCommand.Count > 0) {
        _ = context.ActiveCommand.ExecuteNonQuery();
        ReleaseCommand(context);
        AllocateCommand(context);
      }

      ExecuteUnbatchedTask(task);
    }

    private void ExecuteUnbatchedTask(SqlPersistTask task)
    {
      var sequence = Factory.CreatePersistParts(task);
      foreach (var part in sequence) {
        using (var command = Factory.CreateCommand()) {
          ValidateCommandParameters(part);
          command.AddPart(part);
          var affectedRowsCount = command.ExecuteNonQuery();
          if (affectedRowsCount == 0) {
            throw new VersionConflictException(string.Format(
              Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, task.EntityKey));
          }
        }
      }
    }

    private void PutTasksForExecution(CommandProcessorContext context)
    {
      if (context.AllowPartialExecution) {
        context.ProcessingTasks = new Queue<SqlTask>();
        var batchesCount = tasks.Count / batchSize;
        if (batchesCount == 0) {
          return;
        }

        context.ProcessingTasks = new Queue<SqlTask>();
        while (context.ProcessingTasks.Count < batchesCount * batchSize) {
          context.ProcessingTasks.Enqueue(tasks.Dequeue());
        }
      }
      else {
        context.ProcessingTasks = new Queue<SqlTask>(tasks);
        tasks.Clear();
      }
    }

    private bool PendCommandPart(Command currentCommand, CommandPart partToAdd) =>
      PendCommandParts(currentCommand, new[] { partToAdd });

    private bool PendCommandParts(Command currentCommand, ICollection<CommandPart> partsToAdd)
    {
      var currentCount = (currentCommand != null) ? currentCommand.ParametersCount : 0;
      var behavior = GetCommandExecutionBehavior(partsToAdd, currentCount);
      if (behavior == ExecutionBehavior.AsOneCommand) {
        return false;
      }
      return behavior == ExecutionBehavior.TooLargeForAnyCommand
        ? throw new ParametersLimitExceededException(currentCount + partsToAdd.Sum(x => x.Parameters.Count), MaxQueryParameterCount)
        : true;
    }

    private void ValidateCommandParameters(CommandPart commandPart)
    {
      if (GetCommandExecutionBehavior(new[] { commandPart }, 0) == ExecutionBehavior.TooLargeForAnyCommand) {
        throw new ParametersLimitExceededException(commandPart.Parameters.Count, MaxQueryParameterCount);
      }
    }

    private static string GetParameterPrefix(CommandProcessorContext context) =>
      $"p{context.ActiveCommand.Count + 1}_";

    #endregion

    // Constructors

    public BatchingCommandProcessor(CommandFactory factory, int batchSize, int maxQueryParameterCount)
      : base(factory, maxQueryParameterCount)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(batchSize, 1, nameof(batchSize));
      this.batchSize = batchSize;
      tasks = new Queue<SqlTask>();
    }
  }
}