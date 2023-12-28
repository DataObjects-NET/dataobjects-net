// Copyright (C) 2012-2022 Xtensive LLC.
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
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;

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
      await EnsureConnectionIsOpenAsync(token).ConfigureAwaitFalse();
      return await ExecuteReaderAsync(
        connection.CreateCommand(Compile(statement)), commandBehavior, token).ConfigureAwaitFalse();
    }

    public int ExecuteNonQuery(ISqlCompileUnit statement)
    {
      EnsureConnectionIsOpen();
      using var command = connection.CreateCommand(Compile(statement));
      return driver.ExecuteNonQuery(session, command);
    }

    public async Task<int> ExecuteNonQueryAsync(ISqlCompileUnit statement, CancellationToken token = default)
    {
      await EnsureConnectionIsOpenAsync(token).ConfigureAwaitFalse();
      var command = connection.CreateCommand(Compile(statement));
      await using (command.ConfigureAwaitFalse()) {
        return await driver.ExecuteNonQueryAsync(session, command, token).ConfigureAwaitFalse();
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
      await EnsureConnectionIsOpenAsync(token).ConfigureAwaitFalse();
      var command = connection.CreateCommand(Compile(statement));
      await using (command.ConfigureAwaitFalse()) {
        return await driver.ExecuteScalarAsync(session, command, token).ConfigureAwaitFalse();
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
      await EnsureConnectionIsOpenAsync(token).ConfigureAwaitFalse();
      return await ExecuteReaderAsync(
        connection.CreateCommand(commandText), commandBehavior, token).ConfigureAwaitFalse();
    }

    public int ExecuteNonQuery(string commandText)
    {
      EnsureConnectionIsOpen();
      using var command = connection.CreateCommand(commandText);
      return driver.ExecuteNonQuery(session, command);
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText, CancellationToken token = default)
    {
      await EnsureConnectionIsOpenAsync(token).ConfigureAwaitFalse();
      var command = connection.CreateCommand(commandText);
      await using (command.ConfigureAwaitFalse()) {
        return await driver.ExecuteNonQueryAsync(session, command, token).ConfigureAwaitFalse();
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
      await EnsureConnectionIsOpenAsync(token).ConfigureAwaitFalse();
      var command = connection.CreateCommand(commandText);
      await using (command.ConfigureAwaitFalse()) {
        return await driver.ExecuteScalarAsync(session, command, token).ConfigureAwaitFalse();
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
      await EnsureConnectionIsOpenAsync(token).ConfigureAwaitFalse();

      if (driver.ProviderInfo.Supports(ProviderFeatures.Batches)) {
        await ExecuteManyBatchedAsync(statements, token).ConfigureAwaitFalse();
      }
      else {
        await ExecuteManyByOneAsync(statements, token).ConfigureAwaitFalse();
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
      await EnsureConnectionIsOpenAsync(token).ConfigureAwaitFalse();
      return await driver.ExtractAsync(connection, tasks, token).ConfigureAwaitFalse();
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
        await using (command.ConfigureAwaitFalse()) {
          await driver.ExecuteNonQueryAsync(session, command, token).ConfigureAwaitFalse();
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
        await using (command.ConfigureAwaitFalse()) {
          await driver.ExecuteNonQueryAsync(session, command, token).ConfigureAwaitFalse();
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
      var nodeConfiguration = upgradeContext != null ? upgradeContext.NodeConfiguration : session.StorageNode.Configuration;

      return driver.Compile(statement)
        .GetCommandText(
          new SqlPostCompilerConfiguration(nodeConfiguration.GetDatabaseMapping(), nodeConfiguration.GetSchemaMapping()));
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
        reader = await driver.ExecuteReaderAsync(session, command, commandBehavior, token).ConfigureAwaitFalse();
      }
      catch {
        await command.DisposeAsync().ConfigureAwaitFalse();
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