// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Orm;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A command ready for execution.
  /// </summary>
  public sealed class Command : IDisposable
  {
    private readonly Driver driver;
    private readonly Session session;
    private readonly DbCommand underlyingCommand;
    private readonly List<string> statements = new List<string>();
    private readonly List<SqlQueryTask> queryTasks = new List<SqlQueryTask>();

    private DisposableSet disposables;

    /// <summary>
    /// Gets the statements this command is consist of.
    /// </summary>
    public List<string> Statements { get { return statements; } }

    /// <summary>
    /// Gets the query tasks registered in this command.
    /// </summary>
    public List<SqlQueryTask> QueryTasks { get { return queryTasks; } }

    /// <summary>
    /// Adds the part to this command.
    /// </summary>
    /// <param name="part">The part to add.</param>
    public void AddPart(CommandPart part)
    {
      statements.Add(part.Query);
      foreach (var parameter in part.Parameters)
        underlyingCommand.Parameters.Add(parameter);
      if (part.Disposables.Count==0)
        return;
      if (disposables==null)
        disposables = new DisposableSet();
      foreach (var disposable in part.Disposables)
        disposables.Add(disposable);
    }

    /// <summary>
    /// Adds the part to this command.
    /// </summary>
    /// <param name="part">The part to add.</param>
    /// <param name="task">The task.</param>
    public void AddPart(CommandPart part, SqlQueryTask task)
    {
      AddPart(part);
      QueryTasks.Add(task);
    }

    /// <summary>
    /// Executes this command. This method is equivalent of <seealso cref="DbCommand.ExecuteNonQuery"/>.
    /// </summary>
    public void ExecuteNonQuery()
    {
      PrepareCommand();
      driver.ExecuteNonQuery(session, underlyingCommand);
    }

    /// <summary>
    /// Executes this command. This method is equivalent of <seealso cref="DbCommand.ExecuteReader()"/>.
    /// </summary>
    public DbDataReader ExecuteReader()
    {
      PrepareCommand();
      return driver.ExecuteReader(session, underlyingCommand);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      underlyingCommand.DisposeSafely();
      disposables.DisposeSafely();
    }

    #region Private / internal methods

    private void PrepareCommand()
    {
      if (statements.Count==0)
        throw new InvalidOperationException();
      underlyingCommand.CommandText = driver.BuildBatch(statements.ToArray());
    }

    #endregion


    // Constructors

    public Command(Driver driver, Session session, DbCommand underlyingCommand)
    {
      this.driver = driver;
      this.session = session;
      this.underlyingCommand = underlyingCommand;
    }
  }
}