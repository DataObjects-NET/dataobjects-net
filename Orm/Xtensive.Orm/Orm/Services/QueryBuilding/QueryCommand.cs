// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.27

using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Representation of a SQL command similar to <see cref="DbCommand"/>.
  /// Unlike <see cref="DbCommand"/> this type is aware of <see cref="Session.Events"/>
  /// and does all necessary logging of executed SQL.
  /// </summary>
  public sealed class QueryCommand
  {
    private readonly StorageDriver driver;
    private readonly Session session;
    private readonly DbCommand realCommand;

    /// <summary>
    /// Gets SQL query to execute.
    /// </summary>
    public string CommandText => realCommand.CommandText;

    /// <summary>
    /// Executes query and returns <see cref="DbDataReader"/>
    /// for retrieving query results.
    /// </summary>
    /// <returns><see cref="DbDataReader"/> to use.</returns>
    public DbDataReader ExecuteReader() => driver.ExecuteReader(session, realCommand);

    /// <summary>
    /// Executes query and returns <see cref="DbDataReader"/>
    /// for retrieving query results.
    /// </summary>
    /// <param name="token">The token to cancel current operation if needed.</param>
    /// <returns><see cref="DbDataReader"/> to use.</returns>
    public Task<DbDataReader> ExecuteReaderAsync(CancellationToken token = default) =>
      driver.ExecuteReaderAsync(session, realCommand, token);

    /// <summary>
    /// Executes query and returns number of affected rows.
    /// </summary>
    /// <returns>Number of affected rows.</returns>
    public int ExecuteNonQuery() => driver.ExecuteNonQuery(session, realCommand);

    /// <summary>
    /// Asynchronously executes query and returns number of affected rows.
    /// </summary>
    /// <param name="token">The token to cancel current operation if needed.</param>
    /// <returns>Number of affected rows.</returns>
    public Task<int> ExecuteNonQueryAsync(CancellationToken token = default) =>
      driver.ExecuteNonQueryAsync(session, realCommand, token);

    /// <summary>
    /// Executes query and returns scalar result.
    /// </summary>
    /// <returns>Scalar result of query.</returns>
    public object ExecuteScalar() => driver.ExecuteScalar(session, realCommand);

    /// <summary>
    /// Asynchronously executes query and returns scalar result.
    /// </summary>
    /// <param name="token">The token to cancel current operation if needed.</param>
    /// <returns>Scalar result of query.</returns>
    public Task<object> ExecuteScalarAsync(CancellationToken token = default) =>
      driver.ExecuteScalarAsync(session, realCommand, token);

    // Constructors

    internal QueryCommand(StorageDriver driver, Session session, DbCommand realCommand)
    {
      this.driver = driver;
      this.session = session;
      this.realCommand = realCommand;
    }
  }
}