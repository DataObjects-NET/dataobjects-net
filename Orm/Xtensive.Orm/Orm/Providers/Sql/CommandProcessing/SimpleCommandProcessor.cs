// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Orm;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A command processor that simply executes all incoming commands immediately.
  /// </summary>
  public sealed class SimpleCommandProcessor : CommandProcessor
  {
    /// <inheritdoc/>
    public override void ExecuteRequests(bool allowPartialExecution)
    {
      ExecuteAllTasks();
    }

    /// <inheritdoc/>
    public override IEnumerator<Tuple> ExecuteRequestsWithReader(QueryRequest lastRequest)
    {
      ExecuteAllTasks();
      var command = CreateCommand();
      var part = factory.CreateQueryCommandPart(new SqlQueryTask(lastRequest), DefaultParameterNamePrefix);
      command.AddPart(part);
      command.ExecuteReader();
      return RunTupleReader(command, lastRequest.TupleDescriptor);
    }

    /// <inheritdoc/>
    public override void ProcessTask(SqlQueryTask task)
    {
      using (var command = CreateCommand()) {
        var part = factory.CreateQueryCommandPart(task, DefaultParameterNamePrefix);
        command.AddPart(part);
        command.ExecuteReader();
        var enumerator = RunTupleReader(command, task.Request.TupleDescriptor);
        using (enumerator) {
          while (enumerator.MoveNext())
            task.Output.Add(enumerator.Current);
        }
      }
    }

    /// <inheritdoc/>
    public override void ProcessTask(SqlPersistTask task)
    {
      ExecutePersist(task);
    }

    #region Private / internal methods

    private void ExecuteAllTasks()
    {
      while (tasks.Count > 0) {
        var task = tasks.Dequeue();
        task.ProcessWith(this);
      }
    }

    private void ExecutePersist(SqlPersistTask task)
    {
      var sequence = factory.CreatePersistCommandPart(task, DefaultParameterNamePrefix);
      foreach (var part in sequence) {
        using (var command = CreateCommand()) {
          command.AddPart(part);
          command.ExecuteNonQuery();
        }
      }
    }

    #endregion


    // Constructors

    public SimpleCommandProcessor(
      DomainHandler domainHandler, Session session,
      SqlConnection connection, CommandPartFactory factory)
      : base(domainHandler, session, connection, factory)
    {
    }
  }
}