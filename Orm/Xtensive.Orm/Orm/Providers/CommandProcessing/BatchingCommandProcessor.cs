// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  internal sealed class BatchingCommandProcessor : CommandProcessor, ISqlTaskProcessor
  {
    private readonly int batchSize;
    private readonly Queue<SqlTask> tasks;

    //private int reenterCount;
    //private Command activeCommand;
    //private List<SqlLoadTask> activeTasks;

    void ISqlTaskProcessor.ProcessTask(SqlLoadTask task, CommandProcessorContext context)
    {
      var part = Factory.CreateQueryPart(task, GetParameterPrefix(context));
      context.ActiveCommand.AddPart(part);
      context.ActiveTasks.Add(task);
    }

    void ISqlTaskProcessor.ProcessTask(SqlPersistTask task, CommandProcessorContext context)
    {
      if (task.ValidateRowCount) {
        ProcessUnbatchedTask(task, context);
        return;
      }
      var sequence = Factory.CreatePersistParts(task, GetParameterPrefix(context));
      foreach (var part in sequence)
        context.ActiveCommand.AddPart(part);
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
      PutTasksForExecution(context);

      while (context.ProcessingTasks.Count >= batchSize)
        ExecuteBatch(batchSize, null, context);

      if (!context.AllowPartialExecution)
        ExecuteBatch(context.ProcessingTasks.Count, null, context);
    }

    public override async Task ExecuteTasksAsync(CommandProcessorContext context, CancellationToken token)
    {
      PutTasksForExecution(context);

      while (context.ProcessingTasks.Count >= batchSize)
        await ExecuteBatchAsync(batchSize, null, context, token).ConfigureAwait(false);

      if (!context.AllowPartialExecution)
        await ExecuteBatchAsync(context.ProcessingTasks.Count, null, context, token).ConfigureAwait(false);
    }

    public override IEnumerator<Tuple> ExecuteTasksWithReader(QueryRequest request, CommandProcessorContext context)
    {
      context.AllowPartialExecution = false;
      PutTasksForExecution(context);

      while (context.ProcessingTasks.Count >= batchSize)
        ExecuteBatch(batchSize, null, context);

      return ExecuteBatch(context.ProcessingTasks.Count, request, context).AsReaderOf(request);
    }

    public override async Task<IEnumerator<Tuple>> ExecuteTasksWithReaderAsync(QueryRequest request, CommandProcessorContext context, CancellationToken token)
    {
      context.ProcessingTasks = new Queue<SqlTask>(tasks);
      tasks.Clear();

      while (context.ProcessingTasks.Count >= batchSize)
        await ExecuteBatchAsync(batchSize, null, context, token).ConfigureAwait(false);

      return (await ExecuteBatchAsync(context.ProcessingTasks.Count, request, context, token).ConfigureAwait(false)).AsReaderOf(request);
    }

    #region Private / internal methods

    private Command ExecuteBatch(int numberOfTasks, QueryRequest lastRequest, CommandProcessorContext context)
    {
      var shouldReturnReader = lastRequest!=null;

      if (numberOfTasks==0 && !shouldReturnReader)
        return null;

      var tasksToProcess = context.ProcessingTasks;

      AllocateCommand(context);

      try {
        while (numberOfTasks > 0 && tasksToProcess.Count > 0) {
          numberOfTasks--;
          var task = tasksToProcess.Dequeue();
          task.ProcessWith(this, context);
        }
        if (shouldReturnReader) {
          var part = Factory.CreateQueryPart(lastRequest);
          context.ActiveCommand.AddPart(part);
        }
        if (context.ActiveCommand.Count==0)
          return null;
        var hasQueryTasks = context.ActiveTasks.Count > 0;
        if (!hasQueryTasks && !shouldReturnReader) {
          context.ActiveCommand.ExecuteNonQuery();
          return null;
        }
        context.ActiveCommand.ExecuteReader();
        if (hasQueryTasks) {
          int currentQueryTask = 0;
          while (currentQueryTask < context.ActiveTasks.Count) {
            var queryTask = context.ActiveTasks[currentQueryTask];
            var accessor = queryTask.Request.GetAccessor();
            var result = queryTask.Output;
            while (context.ActiveCommand.NextRow())
              result.Add(context.ActiveCommand.ReadTupleWith(accessor));
            context.ActiveCommand.NextResult();
            currentQueryTask++;
          }
        }
        return shouldReturnReader ? context.ActiveCommand : null;
      }
      finally {
        if (!shouldReturnReader)
          context.ActiveCommand.Dispose();
        ReleaseCommand(context);
      }
    }

    private async Task<Command> ExecuteBatchAsync(int numberOfTasks, QueryRequest lastRequest, CommandProcessorContext context, CancellationToken token)
    {
      var shouldReturnReader = lastRequest!=null;

      if (numberOfTasks==0 && !shouldReturnReader)
        return null;

      var tasksToProcess = context.ProcessingTasks;

      AllocateCommand(context);

      try {
        while (numberOfTasks > 0 && tasksToProcess.Count > 0) {
          numberOfTasks--;
          var task = tasksToProcess.Dequeue();
          task.ProcessWith(this, context);
        }
        if (shouldReturnReader)
          context.ActiveCommand.AddPart(Factory.CreateQueryPart(lastRequest));
        if (context.ActiveCommand.Count==0)
          return null;
        var hasQueryTasks = context.ActiveTasks.Count > 0;
        if (!hasQueryTasks && !shouldReturnReader) {
          await context.ActiveCommand.ExecuteNonQueryAsync(token).ConfigureAwait(false);
          return null;
        }
        await context.ActiveCommand.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (hasQueryTasks) {
          int currentQueryTask = 0;
          while (currentQueryTask < context.ActiveTasks.Count) {
            var queryTask = context.ActiveTasks[currentQueryTask];
            var accessor = queryTask.Request.GetAccessor();
            var result = queryTask.Output;
            while (context.ActiveCommand.NextRow())
              result.Add(context.ActiveCommand.ReadTupleWith(accessor));
            context.ActiveCommand.NextResult();
            currentQueryTask++;
          }
        }
        return shouldReturnReader ? context.ActiveCommand : null;
      }
      finally {
        if (!shouldReturnReader)
          context.ActiveCommand.Dispose();
        ReleaseCommand(context);
      }
    }

    private void ProcessUnbatchedTask(SqlPersistTask task, CommandProcessorContext context)
    {
      if (context.ActiveCommand.Count > 0) {
        context.ActiveCommand.ExecuteNonQuery();
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
          command.AddPart(part);
          var affectedRowsCount = command.ExecuteNonQuery();
          if (affectedRowsCount==0)
            throw new VersionConflictException(string.Format(
              Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, task.EntityKey));
        }
      }
    }

    private void PutTasksForExecution(CommandProcessorContext context)
    {
      if (context.AllowPartialExecution) {
        context.ProcessingTasks = new Queue<SqlTask>();
        var batchesCount = (int)tasks.Count / batchSize;
        if (batchesCount==0)
          return;
        context.ProcessingTasks = new Queue<SqlTask>();
        while (context.ProcessingTasks.Count < batchesCount * batchSize)
          context.ProcessingTasks.Enqueue(tasks.Dequeue());
      }
      else {
        context.ProcessingTasks = new Queue<SqlTask>(tasks);
        tasks.Clear();
      }
    }

    private string GetParameterPrefix(CommandProcessorContext context)
    {
      return string.Format("p{0}_", context.ActiveCommand.Count + 1);
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