// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Configuration;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  partial class StorageDriver
  {
    private sealed class InitializationSqlExtension
    {
      public string Script;
    }

    public void ApplyNodeConfiguration(SqlConnection connection, NodeConfiguration nodeConfiguration)
    {
      if (nodeConfiguration.ConnectionInfo!=null)
        connection.ConnectionInfo = nodeConfiguration.ConnectionInfo;

      if (!string.IsNullOrEmpty(nodeConfiguration.ConnectionInitializationSql))
        SetInitializationSql(connection, nodeConfiguration.ConnectionInitializationSql);
    }

    public SqlConnection CreateConnection(Session session)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXCreatingConnection, session.ToStringSafely());

      SqlConnection connection;
      try {
        connection = underlyingDriver.CreateConnection();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }

      var sessionConfiguration = GetConfiguration(session);
      if (sessionConfiguration.ConnectionInfo!=null)
        connection.ConnectionInfo = sessionConfiguration.ConnectionInfo;
      connection.CommandTimeout = sessionConfiguration.DefaultCommandTimeout;

      if (!string.IsNullOrEmpty(configuration.ConnectionInitializationSql))
        SetInitializationSql(connection, configuration.ConnectionInitializationSql);

      return connection;
    }

    public void OpenConnection(Session session, SqlConnection connection)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXOpeningConnectionY, session.ToStringSafely(), connection.ConnectionInfo);

      try {
        connection.Open();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }

      var extension = connection.Extensions.Get<InitializationSqlExtension>();
      if (extension!=null && !string.IsNullOrEmpty(extension.Script))
        using (var command = connection.CreateCommand(extension.Script))
          ExecuteNonQuery(session, command);
    }

    public void EnsureConnectionIsOpen(Session session, SqlConnection connection)
    {
      if (connection.State!=ConnectionState.Open)
        OpenConnection(session, connection);
    }

    public void CloseConnection(Session session, SqlConnection connection)
    {
      if (connection.State!=ConnectionState.Open)
        return;

      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXClosingConnectionY, session.ToStringSafely(), connection.ConnectionInfo);

      try {
        connection.Close();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void DisposeConnection(Session session, SqlConnection connection)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXDisposingConnection, session.ToStringSafely());

      try {
        connection.Dispose();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void BeginTransaction(Session session, SqlConnection connection, IsolationLevel? isolationLevel)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXBeginningTransactionWithYIsolationLevel, session.ToStringSafely(), isolationLevel);

      if (isolationLevel==null)
        isolationLevel = IsolationLevelConverter.Convert(GetConfiguration(session).DefaultIsolationLevel);

      try {
        connection.BeginTransaction(isolationLevel.Value);
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void CommitTransaction(Session session, SqlConnection connection)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXCommitTransaction, session.ToStringSafely());

      try {
        connection.Commit();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void RollbackTransaction(Session session, SqlConnection connection)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXRollbackTransaction, session.ToStringSafely());

      try {
        connection.Rollback();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void MakeSavepoint(Session session, SqlConnection connection, string name)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXMakeSavepointY, session.ToStringSafely(), name);

      if (!hasSavepoints)
        return; // Driver does not support savepoints, so let's fail later (on rollback)

      try {
        connection.MakeSavepoint(name);
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void RollbackToSavepoint(Session session, SqlConnection connection, string name)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXRollbackToSavepointY, session.ToStringSafely(), name);

      if (!hasSavepoints)
        throw new NotSupportedException(Strings.ExCurrentStorageProviderDoesNotSupportSavepoints);

      try {
        connection.RollbackToSavepoint(name);
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }
 
    public void ReleaseSavepoint(Session session, SqlConnection connection, string name)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXReleaseSavepointY, session.ToStringSafely(), name);

      try {
        connection.ReleaseSavepoint(name);
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public int ExecuteNonQuery(Session session, DbCommand command)
    {
      return ExecuteCommand(session, command, c => c.ExecuteNonQuery());
    }

    public object ExecuteScalar(Session session, DbCommand command)
    {
      return ExecuteCommand(session, command, c => c.ExecuteScalar());
    }

    public DbDataReader ExecuteReader(Session session, DbCommand command)
    {
      return ExecuteCommand(session, command, c => c.ExecuteReader());
    }

#if NET45

    public Task<int> ExecuteNonQueryAsync(Session session, DbCommand command, CancellationToken token)
    {
      return ExecuteCommand(session, command, c => c.ExecuteNonQueryAsync(token));
    }

    public Task<object> ExecuteScalarAsync(Session session, DbCommand command, CancellationToken token)
    {
      return ExecuteCommand(session, command, c => c.ExecuteScalarAsync(token));
    }

    public Task<DbDataReader> ExecuteReaderAsync(Session session, DbCommand command, CancellationToken token)
    {
      return ExecuteCommand(session, command, c => command.ExecuteReaderAsync(token));
    }

#endif

    private TResult ExecuteCommand<TResult>(Session session, DbCommand command, Func<DbCommand, TResult> action)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXQueryY, session.ToStringSafely(), command.ToHumanReadableString());

      if (session!=null)
        session.Events.NotifyDbCommandExecuting(command);

      TResult result;
      try {
        result = action.Invoke(command);
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception, command.ToHumanReadableString());
      }

      if (session!=null)
        session.Events.NotifyDbCommandExecuted(command);

      return result;
    }

    private void SetInitializationSql(SqlConnection connection, string script)
    {
      var extension = connection.Extensions.Get<InitializationSqlExtension>();
      if (extension==null) {
        extension = new InitializationSqlExtension();
        connection.Extensions.Set(extension);
      }
      extension.Script = script;
    }

    private SessionConfiguration GetConfiguration(Session session)
    {
      return session!=null ? session.Configuration : configuration.Sessions.System;
    }
  }
}