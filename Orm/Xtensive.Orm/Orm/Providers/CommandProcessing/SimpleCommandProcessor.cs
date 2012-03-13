// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
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
      var command = Factory.CreateCommand();
      var part = Factory.CreateQueryPart(lastRequest);
      command.AddPart(part);
      command.ExecuteReader();
      return command.AsReaderOf(lastRequest);
    }

    void ISqlTaskProcessor.ProcessTask(SqlLoadTask task)
    {
      using (var command = Factory.CreateCommand()) {
        var part = Factory.CreateQueryPart(task);
        command.AddPart(part);
        command.ExecuteReader();
        var enumerator = command.AsReaderOf(task.Request);
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
      var sequence = Factory.CreatePersistParts(task);
      foreach (var part in sequence) {
        using (var command = Factory.CreateCommand()) {
          command.AddPart(part);
          command.ExecuteNonQuery();
        }
      }
    }

    #endregion


    // Constructors

    public SimpleCommandProcessor(CommandFactory factory)
      : base(factory)
    {
    }
  }
}