// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
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

    /// <summary>
    /// Executes all registred requests,
    /// optionally skipping the last requests according to 
    /// <paramref name="allowPartialExecution"/> argument.
    /// </summary>
    /// <param name="allowPartialExecution">
    /// if set to <see langword="true"/> command processor is allowed to skip last request,
    /// if it decides to.</param>
    public abstract void ExecuteTasks(bool allowPartialExecution);

    /// <summary>
    /// Executes the all registered requests.
    /// Calling this method is equivalent to calling <see cref="ExecuteTasks(bool)"/> with <see langword="false"/>.
    /// </summary>
    public void ExecuteTasks()
    {
      ExecuteTasks(false);
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