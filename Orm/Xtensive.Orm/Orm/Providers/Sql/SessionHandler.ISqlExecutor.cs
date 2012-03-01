// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.29

using System.Data.Common;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers.Sql
{
  public partial class SessionHandler
  {
    // Implementation of ISqlExecutor

    /// <inheritdoc/>
    CommandWithDataReader ISqlExecutor.ExecuteReader(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      return ExecuteReader(connection.CreateCommand(statement));
    }

    /// <inheritdoc/>
    int ISqlExecutor.ExecuteNonQuery(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(statement))
        return driver.ExecuteNonQuery(Session, command);
    }

    /// <inheritdoc/>
    object ISqlExecutor.ExecuteScalar(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(statement))
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