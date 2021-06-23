// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A command processor.
  /// </summary>
  public abstract class CommandProcessor
  {
    protected enum ExecutionBehavior
    {
      AsOneCommand,
      AsTwoCommands,
      AsSeveralCommands,
      TooLargeForAnyCommand,
    }

    /// <summary>
    /// Factory of command parts.
    /// </summary>
    public CommandFactory Factory { get; private set; }

    /// <summary>
    /// Gets max query parameter count.
    /// </summary>
    public int MaxQueryParameterCount { get; private set; }

    /// <summary>
    /// Registers tasks for execution.
    /// </summary>
    /// <param name="task">Task to register.</param>
    public abstract void RegisterTask(SqlTask task);

    /// <summary>
    /// Clears all registered tasks
    /// </summary>
    public abstract void ClearTasks();

    /// <summary>
    /// Executes all registred requests plus the specified one query,
    /// returning <see cref="IEnumerator{Tuple}"/> for the last query.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <returns>A <see cref="IEnumerator{Tuple}"/> for the specified request.</returns>
    public abstract IEnumerator<Tuple> ExecuteTasksWithReader(QueryRequest request, CommandProcessorContext context);

    /// <summary>
    /// Asynchronously executes all registred requests plus the specified one query.
    /// Default implementation is synchronous and returns complete task.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="context">The context in which the requests are executed.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>A task performing this operation.</returns>
    public virtual Task<IEnumerator<Tuple>> ExecuteTasksWithReaderAsync(QueryRequest request,
      CommandProcessorContext context, CancellationToken token)
    {
      token.ThrowIfCancellationRequested();
      return Task.FromResult(ExecuteTasksWithReader(request, context));
    }

    /// <summary>
    /// Executes all registred requests,
    /// optionally skipping the last requests according to 
    /// <paramref name="context.AllowPartialExecution"/> argument.
    /// </summary>
    /// <param name="context">The context in which the requests are executed.</param>
    public abstract void ExecuteTasks(CommandProcessorContext context);

    /// <summary>
    /// Asyncronously executes all registered requests,
    /// optionally skipping the last requests according to
    /// <paramref name="context.AllowPartialExecution"/> argument.
    /// Default implementation executes requests synchronously and returns completed task.
    /// </summary>
    /// <param name="context">The context in which the requests are executed.</param>
    /// <param name="token">Token to cancel this operation</param>
    /// <returns>A task preforming this operation.</returns>
    public virtual async Task ExecuteTasksAsync(CommandProcessorContext context, CancellationToken token)
    {
      token.ThrowIfCancellationRequested();
      ExecuteTasks(context);
      await Task.Yield();
    }

    protected void AllocateCommand(CommandProcessorContext context)
    {
      if (context.ActiveCommand != null) {
        context.ReenterCount++;
      }
      else {
        context.ActiveTasks = new List<SqlLoadTask>();
        context.ActiveCommand = Factory.CreateCommand();
      }
    }

    protected void ReleaseCommand(CommandProcessorContext context)
    {
      if (context.ReenterCount > 0) {
        context.ReenterCount--;
        context.ActiveTasks = new List<SqlLoadTask>();
        context.ActiveCommand = Factory.CreateCommand();
      }
      else {
        context.ActiveCommand = null;
        context.ActiveTasks = null;
      }
    }

    protected ExecutionBehavior GetCommandExecutionBehavior(ICollection<CommandPart> commandParts, int currentParametersCount)
    {
      if (MaxQueryParameterCount == int.MaxValue) {
        return ExecutionBehavior.AsOneCommand;
      }

      var sum = 0;
      foreach (var commandPart in commandParts) {
        var count = commandPart.Parameters.Count;
        if (count > MaxQueryParameterCount) {
          return ExecutionBehavior.TooLargeForAnyCommand;
        }
        sum += count;
      }
      if (sum + currentParametersCount <= MaxQueryParameterCount) {
        return ExecutionBehavior.AsOneCommand;
      }
      return sum < MaxQueryParameterCount
        ? ExecutionBehavior.AsTwoCommands
        : ExecutionBehavior.AsSeveralCommands;
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <param name="maxQueryParameterCount">The maximum parameter count per query.</param>
    protected CommandProcessor(CommandFactory factory, int maxQueryParameterCount)
    {
      ArgumentValidator.EnsureArgumentNotNull(factory, "factory");
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(maxQueryParameterCount, 0, "maxQueryParameterCount");
      Factory = factory;
      MaxQueryParameterCount = maxQueryParameterCount;
    }
  }
}