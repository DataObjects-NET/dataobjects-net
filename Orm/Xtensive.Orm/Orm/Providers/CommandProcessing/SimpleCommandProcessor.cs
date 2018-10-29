// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Linq;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  internal sealed class SimpleCommandProcessor : CommandProcessor, ISqlTaskProcessor
  {
    public override void RegisterTask(SqlTask task)
    {
      task.ProcessWith(this);
    }

    public override void ClearTasks()
    {
    }

    public override void ExecuteTasks(bool allowPartialExecution)
    {
      // All tasks are already executed in RegisterTask method.
    }

    public override IEnumerator<Tuple> ExecuteTasksWithReader(QueryRequest lastRequest)
    {
      var command = Factory.CreateCommand();
      var part = Factory.CreateQueryPart(lastRequest);
      ValidateCommandParameterCount(null, part);
      command.AddPart(part);
      command.ExecuteReader();
      return command.AsReaderOf(lastRequest);
    }

    bool ISqlTaskProcessor.ProcessTask(SqlLoadTask task)
    {
      var part = Factory.CreateQueryPart(task);
      if (!ValidateCommandParameterCount(null, part))
        return false;

      using (var command = Factory.CreateCommand()) {
        command.AddPart(part);
        command.ExecuteReader();
        var enumerator = command.AsReaderOf(task.Request);
        using (enumerator) {
          while (enumerator.MoveNext())
            task.Output.Add(enumerator.Current);
        }

        return true;
      }
    }

    bool ISqlTaskProcessor.ProcessTask(SqlPersistTask task)
    {
      var sequence = Factory.CreatePersistParts(task).ToArray();
      if (!ValidateCommandParameterCount(null, sequence))
        return false;

      foreach (var part in sequence) {
        using (var command = Factory.CreateCommand()) {
          command.AddPart(part);
          var affectedRowsCount = command.ExecuteNonQuery();
          if (task.ValidateRowCount && affectedRowsCount==0)
            throw new VersionConflictException(string.Format(
              Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, task.EntityKey));
        }
      }

      return true;
    }

    // Constructors

    public SimpleCommandProcessor(CommandFactory factory, int maxQueryParameterCount)
      : base(factory, maxQueryParameterCount)
    {
    }
  }
}