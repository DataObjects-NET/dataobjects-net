// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.Drivers.SqlServer
{
  internal class Connection : SqlConnection
  {
    private const string DefaultCheckConnectionQuery = "SELECT TOP(0) 0;";

    private readonly bool checkConnectionIsAlive;

    private SqlServerConnection underlyingConnection;
    private SqlTransaction activeTransaction;

    /// <inheritdoc/>
    public override DbConnection UnderlyingConnection { get { return underlyingConnection; } }

    /// <inheritdoc/>
    public override DbTransaction ActiveTransaction { get { return activeTransaction; } }

    /// <inheritdoc/>
    public override DbParameter CreateParameter()
    {
      return new SqlParameter();
    }

    /// <inheritdoc/>
    public override void Open()
    {
      if (!checkConnectionIsAlive) {
        base.Open();
      }
      else {
        var connectionHandlers = Extensions.Get<ConnectionHandlersExtension>();
        if (connectionHandlers == null) {
          OpenWithCheckFast(DefaultCheckConnectionQuery);
        }
        else {
          OpenWithCheckAndNotification(DefaultCheckConnectionQuery, connectionHandlers);
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

      var connectionHandlers = Extensions.Get<ConnectionHandlersExtension>();
      if (connectionHandlers == null) {
        return OpenWithCheckFastAsync(DefaultCheckConnectionQuery, cancellationToken);
      }
      else {
        return OpenWithCheckAndNotificationAsync(DefaultCheckConnectionQuery, connectionHandlers, cancellationToken);
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
      var connectionHandlers = Extensions.Get<ConnectionHandlersExtension>();
      if (connectionHandlers == null) {
        OpenWithCheckFast(script);
      }
      else {
        OpenWithCheckAndNotification(script, connectionHandlers);
      }
    }

    /// <inheritdoc/>
    public override Task OpenAndInitializeAsync(string initializationScript, CancellationToken cancellationToken)
    {
      if (!checkConnectionIsAlive)
        return base.OpenAndInitializeAsync(initializationScript, cancellationToken);

      var script = string.IsNullOrEmpty(initializationScript.Trim())
        ? DefaultCheckConnectionQuery
        : initializationScript;
      var connectionHandlers = Extensions.Get<ConnectionHandlersExtension>();
      return connectionHandlers == null
        ? OpenWithCheckFastAsync(script, cancellationToken)
        : OpenWithCheckAndNotificationAsync(script, connectionHandlers, cancellationToken);
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

    protected override void ClearUnderlyingConnection()
    {
      underlyingConnection = null;
    }

    /// <inheritdoc/>
    protected override void ClearActiveTransaction()
    {
      activeTransaction = null;
    }

    private void OpenWithCheckFast(string checkQueryString)
    {
      var connectionChecked = false;
      var restoreTriggered = false;
      while (!connectionChecked) {
        base.Open();
        try {
          using (var command = underlyingConnection.CreateCommand()) {
            command.CommandText = checkQueryString;
            _ = command.ExecuteNonQuery();
          }
          connectionChecked = true;
        }
        catch (Exception exception) {
          if (InternalHelpers.ShouldRetryOn(exception)) {
            if (restoreTriggered)
              throw;

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

    private void OpenWithCheckAndNotification(string checkQueryString, ConnectionHandlersExtension connectionHandlers)
    {
      var connectionChecked = false;
      var restoreTriggered = false;
      var handlers = connectionHandlers.Handlers;
      while (!connectionChecked) {
        SqlHelper.NotifyConnectionOpening(handlers, UnderlyingConnection, (!connectionChecked && !restoreTriggered));
        underlyingConnection.Open();
        try {
          SqlHelper.NotifyConnectionInitializing(handlers, UnderlyingConnection, checkQueryString, (!connectionChecked && !restoreTriggered));
          using (var command = underlyingConnection.CreateCommand()) {
            command.CommandText = checkQueryString;
            _ = command.ExecuteNonQuery();
          }
          connectionChecked = true;
          SqlHelper.NotifyConnectionOpened(handlers, UnderlyingConnection, (!connectionChecked && !restoreTriggered));
        }
        catch (Exception exception) {
          SqlHelper.NotifyConnectionOpeningFailed(handlers, UnderlyingConnection, exception, (!connectionChecked && !restoreTriggered));
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
        await base.OpenAsync(cancellationToken).ConfigureAwait(false);
        try {
          using (var command = underlyingConnection.CreateCommand()) {
            command.CommandText = checkQueryString;
            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
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
      ConnectionHandlersExtension connectionHandlers, CancellationToken cancellationToken)
    {
      var connectionChecked = false;
      var restoreTriggered = false;
      var handlers = connectionHandlers.Handlers;

      while (!connectionChecked) {
        cancellationToken.ThrowIfCancellationRequested();

        SqlHelper.NotifyConnectionOpening(handlers, UnderlyingConnection, !connectionChecked && !restoreTriggered);

        await underlyingConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
        try {
          SqlHelper.NotifyConnectionInitializing(handlers,
            UnderlyingConnection, checkQueryString, !connectionChecked && !restoreTriggered);

          using (var command = underlyingConnection.CreateCommand()) {
            command.CommandText = checkQueryString;
            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
          }
          connectionChecked = true;
          SqlHelper.NotifyConnectionOpened(handlers, UnderlyingConnection, !connectionChecked && !restoreTriggered);
        }
        catch (Exception exception) {
          SqlHelper.NotifyConnectionOpeningFailed(handlers,
            UnderlyingConnection, exception, (!connectionChecked && !restoreTriggered));

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

    // Constructors

    public Connection(SqlDriver driver, bool checkConnection)
      : base(driver)
    {
      underlyingConnection = new SqlServerConnection();
      checkConnectionIsAlive = checkConnection;
    }
  }
}