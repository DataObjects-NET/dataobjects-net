// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
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
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
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
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
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