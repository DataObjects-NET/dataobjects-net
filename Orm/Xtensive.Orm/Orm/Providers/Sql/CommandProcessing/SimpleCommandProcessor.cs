// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers.Sql
{
  internal sealed class SimpleCommandProcessor : CommandProcessor, ISqlTaskProcessor
  {
    public override void ExecuteTasks(bool allowPartialExecution)
    {
      ExecuteAllTasks();
    }

    public override IEnumerator<Tuple> ExecuteTasksWithReader(QueryRequest lastRequest)
    {
      ExecuteAllTasks();
      var command = CreateCommand();
      var part = Factory.CreateQueryCommandPart(new SqlQueryTask(lastRequest), DefaultParameterNamePrefix);
      command.AddPart(part);
      command.ExecuteReader();
      return RunTupleReader(command, lastRequest.TupleDescriptor);
    }

    void ISqlTaskProcessor.ProcessTask(SqlQueryTask task)
    {
      using (var command = CreateCommand()) {
        var part = Factory.CreateQueryCommandPart(task, DefaultParameterNamePrefix);
        command.AddPart(part);
        command.ExecuteReader();
        var enumerator = RunTupleReader(command, task.Request.TupleDescriptor);
        using (enumerator) {
          while (enumerator.MoveNext())
            task.Output.Add(enumerator.Current);
        }
      }
    }

    void ISqlTaskProcessor.ProcessTask(SqlPersistTask task)
    {
      ExecutePersist(task);
    }

    #region Private / internal methods

    private void ExecuteAllTasks()
    {
      while (Tasks.Count > 0) {
        var task = Tasks.Dequeue();
        task.ProcessWith(this);
      }
    }

    private void ExecutePersist(SqlPersistTask task)
    {
      var sequence = Factory.CreatePersistCommandPart(task, DefaultParameterNamePrefix);
      foreach (var part in sequence) {
        using (var command = CreateCommand()) {
          command.AddPart(part);
          command.ExecuteNonQuery();
        }
      }
    }

    #endregion


    // Constructors

    public SimpleCommandProcessor(Session session, CommandPartFactory factory)
      : base(session, factory)
    {
    }
  }
}