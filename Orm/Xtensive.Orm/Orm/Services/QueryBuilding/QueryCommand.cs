// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.27

using System.Data.Common;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Representation of a SQL command similar to <see cref="DbCommand"/>.
  /// Unlike <see cref="DbCommand"/> this type is aware of <see cref="Session.Events"/>
  /// and does all nessesary logging of executed SQL.
  /// </summary>
  public sealed class QueryCommand
  {
    private readonly StorageDriver driver;
    private readonly Session session;
    private readonly DbCommand realCommand;

    /// <summary>
    /// Gets SQL query to execute.
    /// </summary>
    public string CommandText { get { return realCommand.CommandText; } }

    /// <summary>
    /// Executes query and returns <see cref="DbDataReader"/>
    /// for retrieving query results.
    /// </summary>
    /// <returns><see cref="DbDataReader"/> to use.</returns>
    public DbDataReader ExecuteReader()
    {
      return driver.ExecuteReader(session, realCommand, null);
    }

    /// <summary>
    /// Executes query and returns number of affected rows.
    /// </summary>
    /// <returns>Number of affected rows.</returns>
    public int ExecuteNonQuery()
    {
      return driver.ExecuteNonQuery(session, realCommand);
    }

    /// <summary>
    /// Executes query and returns scalar result.
    /// </summary>
    /// <returns>Scalar result of query.</returns>
    public object ExecuteScalar()
    {
      return driver.ExecuteScalar(session, realCommand);
    }

    // Constructors

    internal QueryCommand(StorageDriver driver, Session session, DbCommand realCommand)
    {
      this.driver = driver;
      this.session = session;
      this.realCommand = realCommand;
    }
  }
}