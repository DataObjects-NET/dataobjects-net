// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql.Info;

namespace Xtensive.Sql
{
  /// <summary>
  /// Creates drivers from the specified connection info.
  /// </summary>
  public abstract class SqlDriverFactory
  {
    /// <summary>
    /// Gets driver for the specified <see cref="ConnectionInfo"/>.
    /// </summary>
    /// <param name="connectionInfo">Connection information to use.</param>
    /// <returns>Driver for <paramref name="connectionInfo"/>.</returns>
    public SqlDriver GetDriver(ConnectionInfo connectionInfo) =>
      GetDriver(connectionInfo, new SqlDriverConfiguration());

    /// <summary>
    /// Asynchronously gets driver for the specified <see cref="ConnectionInfo"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connectionInfo">Connection information to use.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns>Driver for <paramref name="connectionInfo"/>.</returns>
    public Task<SqlDriver> GetDriverAsync(ConnectionInfo connectionInfo, CancellationToken token) =>
      GetDriverAsync(connectionInfo, new SqlDriverConfiguration(), token);

    /// <summary>
    /// Creates driver from the specified <paramref name="connectionInfo"/>.
    /// </summary>
    /// <param name="connectionInfo">The connection info to create driver from.</param>
    /// <param name="forcedVersion">Forced server version.</param>
    /// <returns>Created driver.</returns>
    public SqlDriver GetDriver(ConnectionInfo connectionInfo, string forcedVersion) =>
      GetDriver(connectionInfo, new SqlDriverConfiguration {ForcedServerVersion = forcedVersion});

    /// <summary>
    /// Asynchronously creates driver from the specified <paramref name="connectionInfo"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connectionInfo">The connection info to create driver from.</param>
    /// <param name="forcedVersion">Forced server version.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns>Created driver.</returns>
    public Task<SqlDriver> GetDriverAsync(
      ConnectionInfo connectionInfo, string forcedVersion, CancellationToken token) =>
      GetDriverAsync(connectionInfo, new SqlDriverConfiguration {ForcedServerVersion = forcedVersion}, token);

    /// <summary>
    /// Creates driver for the specified <paramref name="connectionInfo"/>
    /// and <paramref name="configuration"/>.
    /// </summary>
    /// <param name="connectionInfo">The connection info to create driver from.</param>
    /// <param name="configuration">Additional configuration options for the driver.</param>
    /// <returns>Created driver.</returns>
    public SqlDriver GetDriver(ConnectionInfo connectionInfo, SqlDriverConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionInfo, nameof(connectionInfo));
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));

      var connectionString = GetConnectionString(connectionInfo);
      configuration = configuration.Clone();
      var driver = CreateDriver(connectionString, configuration);
      driver.Initialize(this, connectionInfo);
      return driver;
    }

    /// <summary>
    /// Asynchronously creates driver for the specified <paramref name="connectionInfo"/>
    /// and <paramref name="configuration"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connectionInfo">The connection info to create driver from.</param>
    /// <param name="configuration">Additional configuration options for the driver.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns>Created driver.</returns>
    public async Task<SqlDriver> GetDriverAsync(
      ConnectionInfo connectionInfo, SqlDriverConfiguration configuration, CancellationToken token)
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionInfo, nameof(connectionInfo));
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));

      var connectionString = GetConnectionString(connectionInfo);
      configuration = configuration.Clone();
      var driver = await CreateDriverAsync(connectionString, configuration, token).ConfigureAwaitFalse();
      driver.Initialize(this, connectionInfo);
      return driver;
    }

    /// <summary>
    /// Gets connection string for the specified <see cref="ConnectionInfo"/>.
    /// </summary>
    /// <param name="connectionInfo">Connection information to process.</param>
    /// <returns>Connection string for <paramref name="connectionInfo"/>.</returns>
    public string GetConnectionString(ConnectionInfo connectionInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionInfo, nameof(connectionInfo));
      return connectionInfo.ConnectionString
        ?? BuildConnectionString(connectionInfo.ConnectionUrl);
    }

    /// <summary>
    /// Gets <see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.
    /// </summary>
    /// <param name="connection"><see cref="DbConnection"/> to use.</param>
    /// <param name="transaction"><see cref="DbTransaction"/> to use.</param>
    /// <returns><see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.</returns>
    public DefaultSchemaInfo GetDefaultSchema(DbConnection connection, DbTransaction transaction = null)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, nameof(connection));
      return ReadDefaultSchema(connection, transaction);
    }

    /// <summary>
    /// Asynchronously gets <see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connection"><see cref="DbConnection"/> to use.</param>
    /// <param name="transaction"><see cref="DbTransaction"/> to use.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns><see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.</returns>
    public Task<DefaultSchemaInfo> GetDefaultSchemaAsync(
      DbConnection connection, DbTransaction transaction = null, CancellationToken token = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, nameof(connection));
      return ReadDefaultSchemaAsync(connection, transaction, token);
    }

    /// <summary>
    /// Reads <see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.
    /// </summary>
    /// <param name="connection"><see cref="DbConnection"/> to use.</param>
    /// <param name="transaction"><see cref="DbTransaction"/> to use.</param>
    /// <returns><see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.</returns>
    protected abstract DefaultSchemaInfo ReadDefaultSchema(DbConnection connection, DbTransaction transaction);

    /// <summary>
    /// Asynchronously reads <see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connection"><see cref="DbConnection"/> to use.</param>
    /// <param name="transaction"><see cref="DbTransaction"/> to use.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns><see cref="DefaultSchemaInfo"/> for the specified <paramref name="connection"/>.</returns>
    protected abstract Task<DefaultSchemaInfo> ReadDefaultSchemaAsync(
      DbConnection connection, DbTransaction transaction, CancellationToken token);

    /// <summary>
    /// Creates the driver from the specified <paramref name="connectionString"/>.
    /// </summary>
    /// <param name="connectionString">The connection string to create driver from.</param>
    /// <param name="configuration">Additional configuration for the driver.</param>
    /// <returns>Created driver.</returns>
    protected abstract SqlDriver CreateDriver(string connectionString, SqlDriverConfiguration configuration);

    /// <summary>
    /// Asynchronously creates the driver from the specified <paramref name="connectionString"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="connectionString">The connection string to create driver from.</param>
    /// <param name="configuration">Additional configuration for the driver.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns>Created driver.</returns>
    protected abstract Task<SqlDriver> CreateDriverAsync(
      string connectionString, SqlDriverConfiguration configuration, CancellationToken token);

    /// <summary>
    /// Builds the connection string from the specified URL.
    /// </summary>
    /// <param name="connectionUrl">The connection URL.</param>
    /// <returns>Built connection string</returns>
    protected abstract string BuildConnectionString(UrlInfo connectionUrl);
  }
}
