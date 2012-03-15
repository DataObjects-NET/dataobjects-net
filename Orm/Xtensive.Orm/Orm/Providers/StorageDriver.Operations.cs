// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  partial class StorageDriver
  {
    public SqlConnection CreateConnection(Session session)
    {
      var connectionInfo = GetConnectionInfo(session);

      if (isLoggingEnabled)
        Log.Info(Strings.LogSessionXCreatingConnectionY, session.ToStringSafely(), connectionInfo);

      try {
        var connection = underlyingDriver.CreateConnection(connectionInfo);
        connection.CommandTimeout = session.CommandTimeout;
        return connection;
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void OpenConnection(Session session, SqlConnection connection)
    {
      if (isLoggingEnabled)
        Log.Info(Strings.LogSessionXOpeningConnectionY, session.ToStringSafely(), GetConnectionInfo(session));

      try {
        connection.Open();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void EnsureConnectionIsOpen(Session session, SqlConnection connection)
    {
      if (connection.State!=ConnectionState.Open)
        OpenConnection(session, connection);
    }

    public void CloseConnection(Session session, SqlConnection connection)
    {
      if (isLoggingEnabled)
        Log.Info(Strings.LogSessionXClosingConnectionY, session.ToStringSafely(), GetConnectionInfo(session));

      try {
        if (connection.State==ConnectionState.Open)
          connection.Close();
        connection.Dispose();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void BeginTransaction(Session session, SqlConnection connection, IsolationLevel isolationLevel)
    {
      if (isLoggingEnabled)
        Log.Info(Strings.LogSessionXBeginningTransactionWithYIsolationLevel, session.ToStringSafely(), isolationLevel);

      try {
        connection.BeginTransaction(isolationLevel);
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void CommitTransaction(Session session, SqlConnection connection)
    {
      if (isLoggingEnabled)
        Log.Info(Strings.LogSessionXCommitTransaction, session.ToStringSafely());

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
        Log.Info(Strings.LogSessionXRollbackTransaction, session.ToStringSafely());

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
        Log.Info(Strings.LogSessionXMakeSavepointY, session.ToStringSafely(), name);

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
        Log.Info(Strings.LogSessionXRollbackToSavepointY, session.ToStringSafely(), name);

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
        Log.Info(Strings.LogSessionXReleaseSavepointY, session.ToStringSafely(), name);

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

    private TResult ExecuteCommand<TResult>(Session session, DbCommand command, Func<DbCommand, TResult> action)
    {
      if (isLoggingEnabled)
        Log.Info(Strings.LogSessionXQueryY, session.ToStringSafely(), command.ToHumanReadableString());

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

    private ConnectionInfo GetConnectionInfo(Session session)
    {
      var sessionConnectionInfo = session==null
        ? configuration.Sessions.System.ConnectionInfo
        : session.Configuration.ConnectionInfo;
      return sessionConnectionInfo ?? configuration.ConnectionInfo;
    }
  }
}