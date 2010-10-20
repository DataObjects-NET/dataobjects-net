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
using Xtensive.Storage.Providers.Sql.Resources;

namespace Xtensive.Storage.Providers.Sql
{
  partial class Driver
  {
    public SqlConnection CreateConnection(Session session)
    {
      try {
        var connectionInfo = GetConnectionInfo(session);
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXCreatingConnectionY,
            session.GetFullNameSafely(), connectionInfo);
        var connection = underlyingDriver.CreateConnection(connectionInfo);
        connection.CommandTimeout = session.CommandTimeout;
        return connection;
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    public void OpenConnection(Session session, SqlConnection connection)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXOpeningConnectionY,
            session.GetFullNameSafely(), GetConnectionInfo(session));
        connection.Open();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    public void CloseConnection(Session session, SqlConnection connection)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXClosingConnectionY, 
            session.GetFullNameSafely(), GetConnectionInfo(session));
        if (connection.State==ConnectionState.Open)
          connection.Close();
        connection.Dispose();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }      
    }

    public void BeginTransaction(Session session, SqlConnection connection, IsolationLevel isolationLevel)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXBeginningTransactionWithYIsolationLevel, 
            session.GetFullNameSafely(), isolationLevel);
        connection.BeginTransaction(isolationLevel);
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }      
    }

    public void CommitTransaction(Session session, SqlConnection connection)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXCommitTransaction, session.GetFullNameSafely());
        connection.Commit();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    public void RollbackTransaction(Session session, SqlConnection connection)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXRollbackTransaction, session.GetFullNameSafely());
        connection.Rollback();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }      
    }

    public void MakeSavepoint(Session session, SqlConnection connection, string name)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXMakeSavepointY, session.GetFullNameSafely(), name);
        connection.MakeSavepoint(name);
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    public void RollbackToSavepoint(Session session, SqlConnection connection, string name)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXRollbackToSavepointY, session.GetFullNameSafely(), name);
        connection.RollbackToSavepoint(name);
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }
 
    public void ReleaseSavepoint(Session session, SqlConnection connection, string name)
    {
      try {
        if (isDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXReleaseSavepointY, session.GetFullNameSafely(), name);
        connection.ReleaseSavepoint(name);
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    public int ExecuteNonQuery(Session session, DbCommand command)
    {
      try {
        if (isDebugLoggingEnabled)
          LogCommand(session, command);
        return command.ExecuteNonQuery();
      }
      catch (Exception exception) {
        throw TranslateException(command.ToHumanReadableString(), exception);
      }
    }

    public object ExecuteScalar(Session session, DbCommand command)
    {
      try {
        if (isDebugLoggingEnabled)
          LogCommand(session, command);
        return command.ExecuteScalar();
      }
      catch (Exception exception) {
        throw TranslateException(command.ToHumanReadableString(), exception);
      }
    }

    public DbDataReader ExecuteReader(Session session, DbCommand command)
    {
      try {
        if (isDebugLoggingEnabled)
          LogCommand(session, command);
        return command.ExecuteReader();
      }
      catch (Exception exception) {
        throw TranslateException(command.ToHumanReadableString(), exception);
      }
    }
    
    public bool ReadRow(DbDataReader reader)
    {
      try {
        return reader.Read();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    private StorageException TranslateException(string queryText, Exception exception)
    {
      var exceptionType = underlyingDriver.GetExceptionType(exception);
      var originalMessage = exception.Message;

      var message = string.IsNullOrEmpty(queryText)
        ? string.Format(Strings.ExErrorXOriginalMessageY,
            exceptionType, originalMessage)
        : string.Format(Strings.ExErrorXWhileExecutingQueryYOriginalMessageZ,
            exceptionType, queryText, originalMessage);

      switch (exceptionType) {
      case SqlExceptionType.ConnectionError:
        return new ConnectionErrorException(message, exception);
      case SqlExceptionType.SyntaxError:
        return new SyntaxErrorException(message, exception);
      case SqlExceptionType.CheckConstraintViolation:
        return new CheckConstraintViolationException(message, exception);
      case SqlExceptionType.UniqueConstraintViolation:
        return new UniqueConstraintViolationException(message, exception);
      case SqlExceptionType.ReferentialConstraintViolation:
        return new ReferentialConstraintViolationException(message, exception);
      case SqlExceptionType.Deadlock:
        return new DeadlockException(message, exception);
      case SqlExceptionType.SerializationFailure:
        return new TransactionSerializationFailureException(message, exception);
      case SqlExceptionType.OperationTimeout:
        return new OperationTimeoutException(message, exception);
      default:
        return new StorageException(message, exception);
      }
    }

    private ConnectionInfo GetConnectionInfo(Session session)
    {
      return session.Configuration.ConnectionInfo
        ?? session.Domain.Configuration.ConnectionInfo;
    }

    private void LogCommand(Session session, DbCommand command)
    {
      Log.Debug(Strings.LogSessionXQueryY, session.GetFullNameSafely(), command.ToHumanReadableString());
    }
  }
}