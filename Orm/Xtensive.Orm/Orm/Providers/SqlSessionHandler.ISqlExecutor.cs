// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.29

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  public partial class SqlSessionHandler
  {
    // Implementation of ISqlExecutor

    /// <inheritdoc/>
    CommandWithDataReader ISqlExecutor.ExecuteReader(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      return ExecuteReader(connection.CreateCommand(Compile(statement)));
    }

    /// <inheritdoc/>
    int ISqlExecutor.ExecuteNonQuery(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(Compile(statement)))
        return driver.ExecuteNonQuery(Session, command);
    }

    /// <inheritdoc/>
    object ISqlExecutor.ExecuteScalar(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(Compile(statement)))
        return driver.ExecuteScalar(Session, command);
    }

    /// <inheritdoc/>
    CommandWithDataReader ISqlExecutor.ExecuteReader(string commandText)
    {
      EnsureConnectionIsOpen();
      return ExecuteReader(connection.CreateCommand(commandText));
    }

    /// <inheritdoc/>
    int ISqlExecutor.ExecuteNonQuery(string commandText)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(commandText))
        return driver.ExecuteNonQuery(Session, command);
    }

    /// <inheritdoc/>
    object ISqlExecutor.ExecuteScalar(string commandText)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(commandText))
        return driver.ExecuteScalar(Session, command);
    }

    void ISqlExecutor.ExecuteDdl(IEnumerable<string> statements)
    {
      ExecuteMany(statements, ProviderFeatures.DdlBatches);
    }

    void ISqlExecutor.ExecuteDml(IEnumerable<string> statements)
    {
      ExecuteMany(statements, ProviderFeatures.DmlBatches);
    }

    SqlExtractionResult ISqlExecutor.Extract(IEnumerable<SqlExtractionTask> tasks)
    {
      EnsureConnectionIsOpen();
      return driver.Extract(connection, tasks);
    }

    private void ExecuteMany(IEnumerable<string> statements, ProviderFeatures batchFeatures)
    {
      EnsureConnectionIsOpen();

      if (Handlers.ProviderInfo.Supports(batchFeatures))
        ExecuteManyBatched(statements);
      else
        ExecuteManyByOne(statements);
    }

    private void ExecuteManyByOne(IEnumerable<string> statements)
    {
      foreach (var statement in statements) {
        if (string.IsNullOrEmpty(statement))
          continue;
        using (var command = Connection.CreateCommand(statement))
          driver.ExecuteNonQuery(Session, command);
      }
    }

    private void ExecuteManyBatched(IEnumerable<string> statements)
    {
      var groups = SplitOnEmptyEntries(statements);
      foreach (var group in groups) {
        var batch = driver.BuildBatch(group.ToArray());
        if (string.IsNullOrEmpty(batch))
          return;
        using (var command = Connection.CreateCommand(batch))
          driver.ExecuteNonQuery(Session, command);
      }
    }

    private IEnumerable<IEnumerable<string>> SplitOnEmptyEntries(IEnumerable<string> items)
    {
      var group = new List<string>();
      foreach (var item in items) {
        if (string.IsNullOrEmpty(item)) {
          if (group.Count==0)
            continue;
          yield return group;
          group = new List<string>();
        }
        else
          group.Add(item);
      }
      if (group.Count!=0)
        yield return group;
    }

    private string Compile(ISqlCompileUnit statement)
    {
      return driver.Compile(statement).GetCommandText();
    }

    private CommandWithDataReader ExecuteReader(DbCommand command)
    {
      DbDataReader reader;
      try {
        reader = driver.ExecuteReader(Session, command);
      }
      catch {
        command.Dispose();
        throw;
      }
      return new CommandWithDataReader(command, reader);
    }
  }
}