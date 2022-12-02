// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.02.29

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  [Service(typeof (ISqlExecutor))]
  internal sealed class SqlExecutor : ISqlExecutor
  {
    private readonly SqlConnection connection;
    private readonly StorageDriver driver;
    private readonly Session session;

    public CommandWithDataReader ExecuteReader(
      ISqlCompileUnit statement, CommandBehavior commandBehavior = CommandBehavior.Default)
    {
      EnsureConnectionIsOpen();
      return ExecuteReader(connection.CreateCommand(Compile(statement)), commandBehavior);
    }

    public Task<CommandWithDataReader> ExecuteReaderAsync(
      ISqlCompileUnit statement, CancellationToken token = default) =>
      ExecuteReaderAsync(statement, CommandBehavior.Default, token);

    public async Task<CommandWithDataReader> ExecuteReaderAsync(
      ISqlCompileUnit statement, CommandBehavior commandBehavior, CancellationToken token = default)
    {
      await EnsureConnectionIsOpenAsync(token).ConfigureAwait(false);
      return await ExecuteReaderAsync(
        connection.CreateCommand(Compile(statement)), commandBehavior, token).ConfigureAwait(false);
    }

    public int ExecuteNonQuery(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using var command = connection.CreateCommand(Compile(statement));
      return driver.ExecuteNonQuery(session, command);
    }

    public async Task<int> ExecuteNonQueryAsync(ISqlCompileUnit statement, CancellationToken token = default)
    {
      await EnsureConnectionIsOpenAsync(token).ConfigureAwait(false);
      var command = connection.CreateCommand(Compile(statement));
      await using (command.ConfigureAwait(false)) {
        return await driver.ExecuteNonQueryAsync(session, command, token).ConfigureAwait(false);
      }
    }

    public object ExecuteScalar(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using var command = connection.CreateCommand(Compile(statement));
      return driver.ExecuteScalar(session, command);
    }

    public async Task<object> ExecuteScalarAsync(ISqlCompileUnit statement, CancellationToken token = default)
    {
      await EnsureConnectionIsOpenAsync(token).ConfigureAwait(false);
      var command = connection.CreateCommand(Compile(statement));
      await using (command.ConfigureAwait(false)) {
        return await driver.ExecuteScalarAsync(session, command, token).ConfigureAwait(false);
      }
    }

    public CommandWithDataReader ExecuteReader(
      string commandText, CommandBehavior commandBehavior = CommandBehavior.Default)
    {
      EnsureConnectionIsOpen();
      return ExecuteReader(connection.CreateCommand(commandText), commandBehavior);
    }

    public Task<CommandWithDataReader> ExecuteReaderAsync(string commandText, CancellationToken token = default) =>
      ExecuteReaderAsync(commandText, CommandBehavior.Default, token);

    public async Task<CommandWithDataReader> ExecuteReaderAsync(
      string commandText, CommandBehavior commandBehavior, CancellationToken token = default)
    {
      await EnsureConnectionIsOpenAsync(token).ConfigureAwait(false);
      return await ExecuteReaderAsync(
        connection.CreateCommand(commandText), commandBehavior, token).ConfigureAwait(false);
    }

    public int ExecuteNonQuery(string commandText)
    {
      EnsureConnectionIsOpen();
      using var command = connection.CreateCommand(commandText);
      return driver.ExecuteNonQuery(session, command);
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText, CancellationToken token = default)
    {
      await EnsureConnectionIsOpenAsync(token).ConfigureAwait(false);
      var command = connection.CreateCommand(commandText);
      await using (command.ConfigureAwait(false)) {
        return await driver.ExecuteNonQueryAsync(session, command, token).ConfigureAwait(false);
      }
    }

    public object ExecuteScalar(string commandText)
    {
      EnsureConnectionIsOpen();
      using var command = connection.CreateCommand(commandText);
      return driver.ExecuteScalar(session, command);
    }

    public async Task<object> ExecuteScalarAsync(string commandText, CancellationToken token = default)
    {
      await EnsureConnectionIsOpenAsync(token).ConfigureAwait(false);
      var command = connection.CreateCommand(commandText);
      await using (command.ConfigureAwait(false)) {
        return await driver.ExecuteScalarAsync(session, command, token).ConfigureAwait(false);
      }
    }

    public void ExecuteMany(IEnumerable<string> statements)
    {
      EnsureConnectionIsOpen();

      if (driver.ProviderInfo.Supports(ProviderFeatures.Batches)) {
        ExecuteManyBatched(statements);
      }
      else {
        ExecuteManyByOne(statements);
      }
    }

    public async Task ExecuteManyAsync(IEnumerable<string> statements, CancellationToken token = default)
    {
      await EnsureConnectionIsOpenAsync(token).ConfigureAwait(false);

      if (driver.ProviderInfo.Supports(ProviderFeatures.Batches)) {
        await ExecuteManyBatchedAsync(statements, token).ConfigureAwait(false);
      }
      else {
        await ExecuteManyByOneAsync(statements, token).ConfigureAwait(false);
      }
    }

    public SqlExtractionResult Extract(IEnumerable<SqlExtractionTask> tasks)
    {
      EnsureConnectionIsOpen();
      return driver.Extract(connection, tasks);
    }

    public async Task<SqlExtractionResult> ExtractAsync(
      IEnumerable<SqlExtractionTask> tasks, CancellationToken token = default)
    {
      await EnsureConnectionIsOpenAsync(token).ConfigureAwait(false);
      return await driver.ExtractAsync(connection, tasks, token).ConfigureAwait(false);
    }

    #region Private / internal methods

    private void ExecuteManyByOne(IEnumerable<string> statements)
    {
      foreach (var statement in statements) {
        if (string.IsNullOrEmpty(statement)) {
          continue;
        }

        using var command = connection.CreateCommand(statement);
        driver.ExecuteNonQuery(session, command);
      }
    }

    private async Task ExecuteManyByOneAsync(IEnumerable<string> statements, CancellationToken token)
    {
      foreach (var statement in statements) {
        if (string.IsNullOrEmpty(statement)) {
          continue;
        }

        var command = connection.CreateCommand(statement);
        await using (command.ConfigureAwait(false)) {
          await driver.ExecuteNonQueryAsync(session, command, token).ConfigureAwait(false);
        }
      }
    }

    private void ExecuteManyBatched(IEnumerable<string> statements)
    {
      var groups = SplitOnEmptyEntries(statements);
      foreach (var group in groups) {
        var batch = driver.BuildBatch(group);
        if (string.IsNullOrEmpty(batch)) {
          return;
        }

        using var command = connection.CreateCommand(batch);
        driver.ExecuteNonQuery(session, command);
      }
    }

    private async Task ExecuteManyBatchedAsync(IEnumerable<string> statements, CancellationToken token)
    {
      var groups = SplitOnEmptyEntries(statements);
      foreach (var group in groups) {
        var batch = driver.BuildBatch(group);
        if (string.IsNullOrEmpty(batch)) {
          return;
        }

        var command = connection.CreateCommand(batch);
        await using (command.ConfigureAwait(false)) {
          await driver.ExecuteNonQueryAsync(session, command, token).ConfigureAwait(false);
        }
      }
    }

    private static IEnumerable<IReadOnlyList<string>> SplitOnEmptyEntries(IEnumerable<string> items)
    {
      var group = new List<string>();
      foreach (var item in items) {
        if (string.IsNullOrEmpty(item)) {
          if (group.Count==0) {
            continue;
          }

          yield return group;
          group = new List<string>();
        }
        else {
          group.Add(item);
        }
      }
      if (group.Count!=0) {
        yield return group;
      }
    }

    private string Compile(ISqlCompileUnit statement)
    {
      if (session==null) {
        return driver.Compile(statement).GetCommandText();
      }

      var upgradeContext = UpgradeContext.GetCurrent(session.Domain.UpgradeContextCookie);
      if (upgradeContext!=null) {
        return driver.Compile(statement, upgradeContext.NodeConfiguration).GetCommandText();
      }

      return driver.Compile(statement, session.StorageNode.Configuration).GetCommandText();
    }

    private CommandWithDataReader ExecuteReader(DbCommand command, CommandBehavior commandBehavior)
    {
      DbDataReader reader;
      try {
        reader = driver.ExecuteReader(session, command, commandBehavior);
      }
      catch {
        command.Dispose();
        throw;
      }
      return new CommandWithDataReader(command, reader);
    }

    private async Task<CommandWithDataReader> ExecuteReaderAsync(DbCommand command, CommandBehavior commandBehavior, CancellationToken token)
    {
      DbDataReader reader;
      try {
        reader = await driver.ExecuteReaderAsync(session, command, commandBehavior, token).ConfigureAwait(false);
      }
      catch {
        await command.DisposeAsync().ConfigureAwait(false);
        throw;
      }
      return new CommandWithDataReader(command, reader);
    }

    private void EnsureConnectionIsOpen() => driver.EnsureConnectionIsOpen(session, connection);

    private Task EnsureConnectionIsOpenAsync(CancellationToken token) =>
      driver.EnsureConnectionIsOpenAsync(session, connection, token);

    #endregion

    // Constructors

    public SqlExecutor(StorageDriver driver, SqlConnection connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(driver, nameof(driver));
      ArgumentValidator.EnsureArgumentNotNull(connection, nameof(connection));
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