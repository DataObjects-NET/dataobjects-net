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
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A command ready for execution.
  /// </summary>
  public sealed class Command : IDisposable
  {
    private readonly CommandFactory origin;
    private readonly DbCommand underlyingCommand;

    private readonly List<string> statements = new List<string>();
    private readonly List<SqlQueryTask> queryTasks = new List<SqlQueryTask>();

    private bool prepared;
    private DisposableSet resources;

    /// <summary>
    /// Gets the statements this command is consist of.
    /// </summary>
    public List<string> Statements { get { return statements; } }

    /// <summary>
    /// Gets the query tasks registered in this command.
    /// </summary>
    public List<SqlQueryTask> QueryTasks { get { return queryTasks; } }

    /// <summary>
    /// Gets the <see cref="DbDataReader"/> that was executed in this command.
    /// </summary>
    public DbDataReader Reader { get; private set; }

    /// <summary>
    /// Adds the part to this command.
    /// </summary>
    /// <param name="part">The part to add.</param>
    public void AddPart(CommandPart part)
    {
      prepared = false;
      statements.Add(part.Query);
      foreach (var parameter in part.Parameters)
        underlyingCommand.Parameters.Add(parameter);
      if (part.Resources.Count==0)
        return;
      if (resources==null)
        resources = new DisposableSet();
      foreach (var resource in part.Resources)
        resources.Add(resource);
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
      Prepare();
      origin.Driver.ExecuteNonQuery(origin.Session, underlyingCommand);
    }

    /// <summary>
    /// Executes this command. This method is equivalent of <seealso cref="DbCommand.ExecuteReader()"/>.
    /// </summary>
    public void ExecuteReader()
    {
      Prepare();
      Reader = origin.Driver.ExecuteReader(origin.Session, underlyingCommand);
    }

    /// <summary>
    /// Converts current command
    /// into a <see cref="IEnumerator{Tuple}"/> according to a specified <see cref="Tuples.TupleDescriptor"/>.
    /// </summary>
    /// <param name="descriptor">The descriptor of a result.</param>
    /// <returns>Created <see cref="IEnumerator{Tuple}"/>.</returns>
    public IEnumerator<Tuple> AsReaderOf(TupleDescriptor descriptor)
    {
      var accessor = origin.Driver.GetDataReaderAccessor(descriptor);
      using (this)
        while (origin.Driver.ReadRow(Reader))
          yield return accessor.Read(Reader);
    }

    /// <summary>
    /// Prepares this command for execution.
    /// <returns><see cref="DbCommand"/> ready for execution.</returns>
    /// </summary>
    public DbCommand Prepare()
    {
      if (statements.Count==0)
        throw new InvalidOperationException("Unable to prepare command: no statements registered");

      if (prepared)
        return underlyingCommand;
      prepared = true;
      underlyingCommand.CommandText = origin.Driver.BuildBatch(statements.ToArray());
      return underlyingCommand;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      Reader.DisposeSafely();
      resources.DisposeSafely();
      underlyingCommand.DisposeSafely();
    }

    // Constructors

    internal Command(CommandFactory origin, DbCommand underlyingCommand)
    {
      this.origin = origin;
      this.underlyingCommand = underlyingCommand;
    }
  }
}