// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.29

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  [Service(typeof (ISqlExecutor))]
  internal sealed class SqlExecutor : ISqlExecutor
  {
    private readonly SqlConnection connection;
    private readonly StorageDriver driver;
    private readonly Session session;

    public CommandWithDataReader ExecuteReader(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      return ExecuteReader(connection.CreateCommand(Compile(statement)));
    }

    public int ExecuteNonQuery(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(Compile(statement)))
        return driver.ExecuteNonQuery(session, command);
    }

    public object ExecuteScalar(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(Compile(statement)))
        return driver.ExecuteScalar(session, command);
    }

    public CommandWithDataReader ExecuteReader(string commandText)
    {
      EnsureConnectionIsOpen();
      return ExecuteReader(connection.CreateCommand(commandText));
    }

    public int ExecuteNonQuery(string commandText)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(commandText))
        return driver.ExecuteNonQuery(session, command);
    }

    public object ExecuteScalar(string commandText)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(commandText))
        return driver.ExecuteScalar(session, command);
    }

    public void ExecuteDdl(IEnumerable<string> statements)
    {
      ExecuteMany(statements, ProviderFeatures.DdlBatches);
    }

    public void ExecuteDml(IEnumerable<string> statements)
    {
      ExecuteMany(statements, ProviderFeatures.DmlBatches);
    }

    public SqlExtractionResult Extract(IEnumerable<SqlExtractionTask> tasks)
    {
      EnsureConnectionIsOpen();
      return driver.Extract(connection, tasks);
    }

    #region Private / internal methods

    private void ExecuteMany(IEnumerable<string> statements, ProviderFeatures batchFeatures)
    {
      EnsureConnectionIsOpen();

      if (driver.ProviderInfo.Supports(batchFeatures))
        ExecuteManyBatched(statements);
      else
        ExecuteManyByOne(statements);
    }

    private void ExecuteManyByOne(IEnumerable<string> statements)
    {
      foreach (var statement in statements) {
        if (string.IsNullOrEmpty(statement))
          continue;
        using (var command = connection.CreateCommand(statement))
          driver.ExecuteNonQuery(session, command);
      }
    }

    private void ExecuteManyBatched(IEnumerable<string> statements)
    {
      var groups = SplitOnEmptyEntries(statements);
      foreach (var group in groups) {
        var batch = driver.BuildBatch(group.ToArray());
        if (string.IsNullOrEmpty(batch))
          return;
        using (var command = connection.CreateCommand(batch))
          driver.ExecuteNonQuery(session, command);
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
        reader = driver.ExecuteReader(session, command);
      }
      catch {
        command.Dispose();
        throw;
      }
      return new CommandWithDataReader(command, reader);
    }

    private void EnsureConnectionIsOpen()
    {
      driver.EnsureConnectionIsOpen(session, connection);
    }

    #endregion

    // Constructors

    public SqlExecutor(StorageDriver driver, SqlConnection connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      this.driver = driver;
      this.connection = connection;
    }

    public SqlExecutor(StorageDriver driver, SqlConnection connection, Session session)
      : this(driver, connection)
    {
      this.session = session;
    }
  }
}