// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using SqlServerConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.Drivers.SqlServer
{
  internal class Connection : SqlConnection
  {
    private const string DefaultCheckConnectionQuery = "SELECT TOP(0) 0;";

    private readonly bool checkConnectionIsAlive;

    private SqlServerConnection underlyingConnection;
    private SqlTransaction activeTransaction;

    /// <inheritdoc/>
    public override DbConnection UnderlyingConnection => underlyingConnection;

    /// <inheritdoc/>
    public override DbTransaction ActiveTransaction => activeTransaction;

    /// <inheritdoc/>
    public override DbParameter CreateParameter() => new SqlParameter();

    /// <inheritdoc/>
    public override void Open()
    {
      if (!checkConnectionIsAlive) {
        base.Open();
      }
      else {
        var connectionAccessorEx = Extensions.Get<DbConnectionAccessorExtension>();
        if (connectionAccessorEx == null) {
          OpenWithCheckFast(DefaultCheckConnectionQuery);
        }
        else {
          OpenWithCheckAndNotification(DefaultCheckConnectionQuery, connectionAccessorEx);
        }
      }
    }

    /// <inheritdoc/>
    public override Task OpenAsync(CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (!checkConnectionIsAlive) {
        return base.OpenAsync(cancellationToken);
      }

      var connectionAccessorEx = Extensions.Get<DbConnectionAccessorExtension>();
      if (connectionAccessorEx == null) {
        return OpenWithCheckFastAsync(DefaultCheckConnectionQuery, cancellationToken);
      }
      else {
        return OpenWithCheckAndNotificationAsync(DefaultCheckConnectionQuery, connectionAccessorEx, cancellationToken);
      }
    }

    /// <inheritdoc/>
    public override void OpenAndInitialize(string initializationScript)
    {
      if (!checkConnectionIsAlive) {
        base.OpenAndInitialize(initializationScript);
        return;
      }

      var script = string.IsNullOrEmpty(initializationScript.Trim())
        ? DefaultCheckConnectionQuery
        : initializationScript;
      var connectionAccessorEx = Extensions.Get<DbConnectionAccessorExtension>();
      if (connectionAccessorEx == null) {
        OpenWithCheckFast(script);
      }
      else {
        OpenWithCheckAndNotification(script, connectionAccessorEx);
      }
    }

    /// <inheritdoc/>
    public override Task OpenAndInitializeAsync(string initializationScript, CancellationToken token)
    {
      if (!checkConnectionIsAlive) {
        return base.OpenAndInitializeAsync(initializationScript, token);
      }

      var script = string.IsNullOrEmpty(initializationScript.Trim())
        ? DefaultCheckConnectionQuery
        : initializationScript;
      var connectionAccessorEx = Extensions.Get<DbConnectionAccessorExtension>();
      return connectionAccessorEx == null
        ? OpenWithCheckFastAsync(script, token)
        : OpenWithCheckAndNotificationAsync(script, connectionAccessorEx, token);
    }

    /// <inheritdoc/>
    public override void BeginTransaction()
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction();
    }

    /// <inheritdoc/>
    public override void BeginTransaction(IsolationLevel isolationLevel)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction(isolationLevel);
    }

    /// <inheritdoc/>
    public override void Rollback()
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();

      try {
        if (!IsTransactionZombied()) {
          ActiveTransaction.Rollback();
        }
      }
      finally {
        ActiveTransaction.Dispose();
        ClearActiveTransaction();
      }
    }

    /// <inheritdoc/>
    public override async Task RollbackAsync(CancellationToken token = default)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();

      try {
        if (!IsTransactionZombied()) {
          await ActiveTransaction.RollbackAsync(token).ConfigureAwaitFalse();
        }
      }
      finally {
        await ActiveTransaction.DisposeAsync().ConfigureAwaitFalse();
        ClearActiveTransaction();
      }
    }

    /// <inheritdoc/>
    public override void MakeSavepoint(string name)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      activeTransaction.Save(name);
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(string name)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      activeTransaction.Rollback(name);
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(string name)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      // nothing
    }

    protected override void ClearUnderlyingConnection() => underlyingConnection = null;

    /// <inheritdoc/>
    protected override void ClearActiveTransaction() => activeTransaction = null;

    private void OpenWithCheckFast(string checkQueryString)
    {
      var connectionChecked = false;
      var restoreTriggered = false;
      while (!connectionChecked) {
        underlyingConnection.Open();
        try {
          using (var command = underlyingConnection.CreateCommand()) {
            command.CommandText = checkQueryString;
            _ = command.ExecuteNonQuery();
          }
          connectionChecked = true;
        }
        catch (Exception exception) {
          if (InternalHelpers.ShouldRetryOn(exception)) {
            if (restoreTriggered) {
              throw;
            }

            var newConnection = new SqlServerConnection(underlyingConnection.ConnectionString);
            try {
              underlyingConnection.Close();
              underlyingConnection.Dispose();
            }
            catch { }

            underlyingConnection = newConnection;
            restoreTriggered = true;
            continue;
          }

          throw;
        }
      }
    }

    private void OpenWithCheckAndNotification(string checkQueryString, DbConnectionAccessorExtension connectionAccessorEx)
    {
      var connectionChecked = false;
      var restoreTriggered = false;
      var accessors = connectionAccessorEx.Accessors;
      while (!connectionChecked) {
        SqlHelper.NotifyConnectionOpening(accessors, UnderlyingConnection, (!connectionChecked && !restoreTriggered));
        underlyingConnection.Open();
        try {
          SqlHelper.NotifyConnectionInitializing(accessors, UnderlyingConnection, checkQueryString, (!connectionChecked && !restoreTriggered));
          using (var command = underlyingConnection.CreateCommand()) {
            command.CommandText = checkQueryString;
            _ = command.ExecuteNonQuery();
          }
          connectionChecked = true;
          SqlHelper.NotifyConnectionOpened(accessors, UnderlyingConnection, (!connectionChecked && !restoreTriggered));
        }
        catch (Exception exception) {
          SqlHelper.NotifyConnectionOpeningFailed(accessors, UnderlyingConnection, exception, (!connectionChecked && !restoreTriggered));
          if (InternalHelpers.ShouldRetryOn(exception)) {
            if (restoreTriggered) {
              throw;
            }

            var newConnection = new SqlServerConnection(underlyingConnection.ConnectionString);
            try {
              underlyingConnection.Close();
              underlyingConnection.Dispose();
            }
            catch { }

            underlyingConnection = newConnection;
            restoreTriggered = true;
            continue;
          }

          throw;
        }
      }
    }

    private async Task OpenWithCheckFastAsync(string checkQueryString, CancellationToken cancellationToken)
    {
      var connectionChecked = false;
      var restoreTriggered = false;

      while (!connectionChecked) {
        cancellationToken.ThrowIfCancellationRequested();
        await underlyingConnection.OpenAsync(cancellationToken).ConfigureAwaitFalse();
        try {
          var command = underlyingConnection.CreateCommand();
          await using (command.ConfigureAwaitFalse()) {
            command.CommandText = checkQueryString;
            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwaitFalse();
          }
          connectionChecked = true;
        }
        catch (Exception exception) {
          if (InternalHelpers.ShouldRetryOn(exception)) {
            if (restoreTriggered) {
              throw;
            }
            var newConnection = new SqlServerConnection(underlyingConnection.ConnectionString);
            try {
              underlyingConnection.Close();
              underlyingConnection.Dispose();
            }
            catch { }

            underlyingConnection = newConnection;
            restoreTriggered = true;
            continue;
          }

          throw;
        }
      }
    }

    private async Task OpenWithCheckAndNotificationAsync(string checkQueryString,
      DbConnectionAccessorExtension connectionAccessorEx, CancellationToken cancellationToken)
    {
      var connectionChecked = false;
      var restoreTriggered = false;
      var accessors = connectionAccessorEx.Accessors;

      while (!connectionChecked) {
        cancellationToken.ThrowIfCancellationRequested();

        await SqlHelper.NotifyConnectionOpeningAsync(accessors,
            UnderlyingConnection, (!connectionChecked && !restoreTriggered), cancellationToken)
          .ConfigureAwaitFalse();

        await underlyingConnection.OpenAsync(cancellationToken).ConfigureAwaitFalse();
        try {
          await SqlHelper.NotifyConnectionInitializingAsync(accessors,
              UnderlyingConnection, checkQueryString, (!connectionChecked && !restoreTriggered), cancellationToken)
            .ConfigureAwaitFalse();

          var command = underlyingConnection.CreateCommand();
          await using (command.ConfigureAwaitFalse()) {
            command.CommandText = checkQueryString;
            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwaitFalse();
          }
          connectionChecked = true;
          await SqlHelper.NotifyConnectionOpenedAsync(accessors, UnderlyingConnection, (!connectionChecked && !restoreTriggered), cancellationToken)
            .ConfigureAwaitFalse();
        }
        catch (Exception exception) {
          await SqlHelper.NotifyConnectionOpeningFailedAsync(accessors,
              UnderlyingConnection, exception, (!connectionChecked && !restoreTriggered), cancellationToken)
            .ConfigureAwaitFalse();

          if (InternalHelpers.ShouldRetryOn(exception)) {
            if (restoreTriggered) {
              throw;
            }
            var newConnection = new SqlServerConnection(underlyingConnection.ConnectionString);
            try {
              underlyingConnection.Close();
              underlyingConnection.Dispose();
            }
            catch { }

            underlyingConnection = newConnection;
            restoreTriggered = true;
            continue;
          }

          throw;
        }
      }
    }

    private bool IsTransactionZombied()
    {
      return ActiveTransaction != null && ActiveTransaction.Connection == null;
    }

    // Constructors

    public Connection(SqlDriver driver, bool checkConnection)
      : base(driver)
    {
      underlyingConnection = new SqlServerConnection();
      checkConnectionIsAlive = checkConnection;
    }
  }
}
