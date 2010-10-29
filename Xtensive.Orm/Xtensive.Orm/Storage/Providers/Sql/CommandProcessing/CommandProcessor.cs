// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A command processor.
  /// </summary>
  public abstract class CommandProcessor : IDisposable
  {
    /// <summary>
    /// Default parameter name prefix.
    /// </summary>
    protected const string DefaultParameterNamePrefix = "p0_";

    /// <summary>
    /// Factory of command parts.
    /// </summary>
    protected readonly CommandPartFactory factory;

    /// <summary>
    /// Currently registered tasks.
    /// </summary>
    protected readonly Queue<SqlTask> tasks = new Queue<SqlTask>();

    /// <summary>
    /// A SQL handler of current domain.
    /// </summary>
    protected readonly DomainHandler domainHandler;

    /// <summary>
    /// A SQL driver.
    /// </summary>
    protected readonly Driver driver;

    /// <summary>
    /// Session this command processor is bound to.
    /// </summary>
    protected readonly Session session;

    /// <summary>
    /// Connection this command processor is bound to.
    /// </summary>
    protected readonly SqlConnection connection;

    /// <summary>
    /// Number of recursive enters in query execution methods.
    /// </summary>
    protected int reenterCount;

    /// <summary>
    /// Active command.
    /// </summary>
    protected Command activeCommand;

    /// <summary>
    /// Processes the specified task.
    /// </summary>
    /// <param name="task">The task to process.</param>
    public abstract void ProcessTask(SqlQueryTask task);

    /// <summary>
    /// Processes the specified task.
    /// </summary>
    /// <param name="task">The task to process.</param>
    public abstract void ProcessTask(SqlPersistTask task);

    /// <summary>
    /// Executes all registred requests plus the specified one query,
    /// returning <see cref="IEnumerator{Tuple}"/> for the last query.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <returns>A <see cref="IEnumerator{Tuple}"/> for the specified request.</returns>
    public abstract IEnumerator<Tuple> ExecuteRequestsWithReader(QueryRequest request);

    /// <summary>
    /// Executes all registred requests,
    /// optionally skipping the last requests according to 
    /// <paramref name="allowPartialExecution"/> argument.
    /// </summary>
    /// <param name="allowPartialExecution">
    /// if set to <see langword="true"/> command processor is allowed to skip last request,
    /// if it decides to.</param>
    public abstract void ExecuteRequests(bool allowPartialExecution);

    /// <summary>
    /// Executes the all registered requests.
    /// Calling this method is equivalent to calling <see cref="ExecuteRequests(bool)"/> with <see langword="false"/>.
    /// </summary>
    public void ExecuteRequests()
    {
      ExecuteRequests(false);
    }

    /// <summary>
    /// Registers the specified task for execution.
    /// </summary>
    /// <param name="task">The task to register.</param>
    public void RegisterTask(SqlTask task)
    {
      tasks.Enqueue(task);
    }

    /// <summary>
    /// Clears all registered tasks.
    /// </summary>
    public void ClearTasks()
    {
      tasks.Clear();
    }

    /// <summary>
    /// Wrapps the specified <see cref="DbDataReader"/>
    /// into a <see cref="IEnumerator{Tuple}"/> according to a specified <see cref="TupleDescriptor"/>.
    /// </summary>
    /// <param name="reader">The reader to wrap.</param>
    /// <param name="descriptor">The descriptor of a result.</param>
    /// <returns>Created <see cref="IEnumerator{Tuple}"/>.</returns>
    protected IEnumerator<Tuple> RunTupleReader(DbDataReader reader, TupleDescriptor descriptor)
    {
      var accessor = domainHandler.GetDataReaderAccessor(descriptor);
      using (reader) {
        while (driver.ReadRow(reader)) {
          var tuple = Tuple.Create(descriptor);
          accessor.Read(reader, tuple);
          yield return tuple;
        }
      }
    }

    /// <summary>
    /// Allocates the active command.
    /// </summary>
    protected void AllocateCommand()
    {
      if (activeCommand!=null)
        reenterCount++;
      else 
        activeCommand = CreateCommand();
    }

    /// <summary>
    /// Disposes the active command.
    /// </summary>
    protected void DisposeCommand()
    {
      activeCommand.DisposeSafely();
      if (reenterCount > 0) {
        reenterCount--;
        activeCommand = CreateCommand();
      }
      else
        activeCommand = null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      DisposeCommand();
    }

    #region Private / internal methods

    private Command CreateCommand()
    {
      var dbCommand = connection.CreateCommand();
      if (session.CommandTimeout!=null)
        dbCommand.CommandTimeout = session.CommandTimeout.Value;
      return new Command(driver, session, dbCommand);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="domainHandler">The domain handler.</param>
    /// <param name="session">The session.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="factory">The factory.</param>
    protected CommandProcessor(
      DomainHandler domainHandler, Session session,
      SqlConnection connection, CommandPartFactory factory)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainHandler, "domainHandler");
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      ArgumentValidator.EnsureArgumentNotNull(factory, "factory");

      this.domainHandler = domainHandler;
      this.session = session;
      this.connection = connection;
      this.factory = factory;

      driver = domainHandler.Driver;
    }
  }
}