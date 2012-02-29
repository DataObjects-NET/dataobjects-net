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
    DbDataReader ISqlExecutor.ExecuteReader(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(statement))
        return driver.ExecuteReader(Session, command);
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
    DbDataReader ISqlExecutor.ExecuteReader(string commandText)
    {
      EnsureConnectionIsOpen();
      using (var command = connection.CreateCommand(commandText))
        return driver.ExecuteReader(Session, command);
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
  }
}