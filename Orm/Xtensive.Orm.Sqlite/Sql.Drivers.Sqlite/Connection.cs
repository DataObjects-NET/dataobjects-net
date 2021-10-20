// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Xtensive.Sql.Drivers.Sqlite
{
  internal class Connection : SqlConnection
  {
    private SQLiteConnection underlyingConnection;
    private SQLiteTransaction activeTransaction;

    /// <inheritdoc/>
    public override DbConnection UnderlyingConnection => underlyingConnection;

    /// <inheritdoc/>
    public override DbTransaction ActiveTransaction => activeTransaction;

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override DbParameter CreateParameter() => new SQLiteParameter();

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override void BeginTransaction()
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction();
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override void BeginTransaction(IsolationLevel isolationLevel)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction(SqlHelper.ReduceIsolationLevel(isolationLevel));
    }

    /// <inheritdoc/>
    public override void MakeSavepoint(string name)
    {
      var commandText = GetCreateSavepointCommandText(name);
      ExecuteNonQuery(commandText);
    }

    /// <inheritdoc/>
    public override Task MakeSavepointAsync(string name, CancellationToken token = default)
    {
      var commandText = GetCreateSavepointCommandText(name);
      return ExecuteNonQueryAsync(commandText, token);
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(string name)
    {
      var commandText = GetRollbackToSavepointCommandText(name);
      ExecuteNonQuery(commandText);
    }

    /// <inheritdoc/>
    public override Task RollbackToSavepointAsync(string name, CancellationToken token = default)
    {
      var commandText = GetRollbackToSavepointCommandText(name);
      return ExecuteNonQueryAsync(commandText, token);
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(string name)
    {
      var commandText = GetReleaseSavepointCommandText(name);
      ExecuteNonQuery(commandText);
    }

    /// <inheritdoc/>
    public override Task ReleaseSavepointAsync(string name, CancellationToken token = default)
    {
      var commandText = GetReleaseSavepointCommandText(name);
      return ExecuteNonQueryAsync(commandText, token);
    }

    /// <inheritdoc/>
    protected override void ClearActiveTransaction() => activeTransaction = null;

    /// <inheritdoc/>
    protected override void ClearUnderlyingConnection() => underlyingConnection = null;

    private static string GetCreateSavepointCommandText(string name) => $"SAVEPOINT {name}";

    private static string GetRollbackToSavepointCommandText(string name) =>
      $"ROLLBACK TO SAVEPOINT {name}; RELEASE SAVEPOINT {name};";

    private static string GetReleaseSavepointCommandText(string name) => $"RELEASE SAVEPOINT {name}";

    private void ExecuteNonQuery(string commandText)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();

      using var command = CreateCommand(commandText);
      command.ExecuteNonQuery();
    }

    private async Task ExecuteNonQueryAsync(string commandText, CancellationToken token)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();

      var command = CreateCommand(commandText);
      await using (command.ConfigureAwait(false)) {
        await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
      }
    }

    // Constructors

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public Connection(SqlDriver driver)
      : base(driver)
    {
      underlyingConnection = new SQLiteConnection();
    }
  }
}
