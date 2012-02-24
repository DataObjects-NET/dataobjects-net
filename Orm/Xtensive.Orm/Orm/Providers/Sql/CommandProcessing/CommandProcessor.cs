// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A command processor.
  /// </summary>
  public abstract class CommandProcessor
  {
    /// <summary>
    /// Default parameter name prefix.
    /// </summary>
    public const string DefaultParameterNamePrefix = "p0_";

    /// <summary>
    /// Factory of command parts.
    /// </summary>
    public CommandPartFactory Factory { get; private set; }

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

    /// <summary>
    /// Wrapps the specified <see cref="DbDataReader"/>
    /// into a <see cref="IEnumerator{Tuple}"/> according to a specified <see cref="TupleDescriptor"/>.
    /// </summary>
    /// <param name="command">The command to use.</param>
    /// <param name="descriptor">The descriptor of a result.</param>
    /// <returns>Created <see cref="IEnumerator{Tuple}"/>.</returns>
    protected IEnumerator<Tuple> RunTupleReader(Command command, TupleDescriptor descriptor)
    {
      var accessor = Factory.Driver.GetDataReaderAccessor(descriptor);
      using (command)
        while (Factory.Driver.ReadRow(command.Reader)) {
          var tuple = Tuple.Create(descriptor);
          accessor.Read(command.Reader, tuple);
          yield return tuple;
        }
    }

    protected Command CreateCommand()
    {
      var dbCommand = Factory.Connection.CreateCommand();
      if (Session.CommandTimeout!=null)
        dbCommand.CommandTimeout = Session.CommandTimeout.Value;
      return new Command(Factory.Driver, Session, dbCommand);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <param name="session">The session.</param>
    protected CommandProcessor(Session session, CommandPartFactory factory)
    {
      ArgumentValidator.EnsureArgumentNotNull(factory, "factory");
      ArgumentValidator.EnsureArgumentNotNull(session, "session");

      Factory = factory;
      Session = session;

      Tasks = new Queue<SqlTask>();
    }
  }
}