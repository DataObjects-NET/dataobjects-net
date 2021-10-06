// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using System.Security;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Xtensive.Sql.Drivers.PostgreSql
{
  internal class Connection : SqlConnection
  {
    private NpgsqlConnection underlyingConnection;
    private NpgsqlTransaction activeTransaction;

    /// <inheritdoc/>
    public override DbConnection UnderlyingConnection => underlyingConnection;

    /// <inheritdoc/>
    public override DbTransaction ActiveTransaction => activeTransaction;

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override DbParameter CreateParameter() => new NpgsqlParameter();

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

    public override void Commit()
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();

      try {
        if (!IsTransactionCompleted()) {
          ActiveTransaction.Commit();
        }
      }
      finally {
        ActiveTransaction.Dispose();
        ClearActiveTransaction();
      }
    }

    public override async Task CommitAsync(CancellationToken token = default)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      try {
        if (!IsTransactionCompleted()) {
          await ActiveTransaction.CommitAsync(token).ConfigureAwait(false);
        }
      }
      finally {
        await ActiveTransaction.DisposeAsync().ConfigureAwait(false);
        ClearActiveTransaction();
      }
    }

    public override void Rollback()
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();

      try {
        if (!IsTransactionCompleted()) {
          ActiveTransaction.Rollback();
        }
      }
      finally {
        ActiveTransaction.Dispose();
        ClearActiveTransaction();
      }
    }

    public override async Task RollbackAsync(CancellationToken token = default)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      try {
        if (!IsTransactionCompleted()) {
          await ActiveTransaction.RollbackAsync(token).ConfigureAwait(false);
        }
      }
      finally {
        await ActiveTransaction.DisposeAsync().ConfigureAwait(false);
        ClearActiveTransaction();
      }
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

    private bool IsTransactionCompleted()
    {
      return activeTransaction != null && activeTransaction.IsCompleted;
    }

    // Constructors

    [SecuritySafeCritical]
    public Connection(SqlDriver driver)
      : base(driver)
    {
      underlyingConnection = new NpgsqlConnection();
    }
  }
}
