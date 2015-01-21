// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    /// <summary>
    /// Factory of command parts.
    /// </summary>
    public CommandFactory Factory { get; private set; }

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
    public abstract IEnumerator<Tuple> ExecuteTasksWithReader(QueryRequest request);

#if NET45
    /// <summary>
    /// Executes all registred requests plus the specified one query asynchronously,
    /// returning <see cref="Task{Command}"/> for the last query.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="token"><see cref="CancellationToken">Cancellation token</see> to cancel task.</param>
    /// <returns>A <see cref="Task{Command}"/> for the specified request.</returns>
    public abstract Task<Command> ExecuteTasksWithReaderAsync(QueryRequest request, CancellationToken token);
#endif

    /// <summary>
    /// Executes all registred requests,
    /// optionally skipping the last requests or all requests according to 
    /// <paramref name="executionBehavior"/> argument.
    /// </summary>
    /// <param name="executionBehavior">
    /// If set to <see cref="ExecutionBehavior.PartialExecutionIsAllowed"/> command processor is allowed to skip last request,
    /// if it decides to.
    /// If set to <see cref="ExecutionBehavior.PartialExecutionIsAllowed"/> command processor leaved excecution until next query.
    /// </param>
    public abstract void ExecuteTasks(ExecutionBehavior executionBehavior);

#if NET45
    /// <summary>
    /// Executes all registred requests asynchronously,
    /// optionally skipping the last requests or all requests according to 
    /// <paramref name="executionBehavior"/> argument.
    /// </summary>
    /// <param name="executionBehavior">
    /// If set to <see cref="ExecutionBehavior.PartialExecutionIsAllowed"/> command processor is allowed to skip last request,
    /// if it decides to.
    /// If set to <see cref="ExecutionBehavior.PartialExecutionIsAllowed"/> command processor leaved excecution until next query.
    /// </param>
    /// <returns></returns>
    public abstract Task ExecuteTasksAsync(ExecutionBehavior executionBehavior, CancellationToken token);
#endif


    /// <summary>
    /// Executes the all registered requests.
    /// Calling this method is equivalent to calling <see cref="ExecuteTasks(ExecutionBehavior)"/> with <see cref="ExecutionBehavior.PartialExecutionIsNotAllowed"/>.
    /// </summary>
    public void ExecuteTasks()
    {
      ExecuteTasks(ExecutionBehavior.PartialExecutionIsNotAllowed);
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="factory">The factory.</param>
    protected CommandProcessor(CommandFactory factory)
    {
      ArgumentValidator.EnsureArgumentNotNull(factory, "factory");
      Factory = factory;
    }
  }
}