// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers.Sql
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
    /// Session this command processor is bound to.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// <see cref="SqlTask"/> queue associated with current instance.
    /// </summary>
    public Queue<SqlTask> Tasks { get; set; }

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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="factory">The factory.</param>
    protected CommandProcessor(CommandFactory factory)
    {
      ArgumentValidator.EnsureArgumentNotNull(factory, "factory");
      Factory = factory;
      Tasks = new Queue<SqlTask>();
    }
  }
}