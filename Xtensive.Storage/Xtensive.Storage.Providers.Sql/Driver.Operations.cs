// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Info;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Resources;
using ColumnInfo = Xtensive.Storage.Model.ColumnInfo;

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
        if ((connection.Driver.ServerInfo.ServerFeatures & ServerFeatures.Savepoints)!=ServerFeatures.Savepoints)
          return; // Driver does not support savepoints, so let's fail later (on rollback)
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
        if ((connection.Driver.ServerInfo.ServerFeatures & ServerFeatures.Savepoints)!=ServerFeatures.Savepoints)
          throw new NotSupportedException(Strings.ExCurrentStorageProviderDoesNotSupportSavepoints);
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
      var sqlExceptionInfo = underlyingDriver.GetExceptionInfo(exception);
      var storageExceptionInfo = GetStorageExceptionInfo(sqlExceptionInfo);
      var builder = new StringBuilder(Strings.SqlErrorOccured);

      var storageErrorDetails = storageExceptionInfo.ToString();
      if (!string.IsNullOrEmpty(storageErrorDetails)) {
        builder.AppendLine();
        builder.AppendFormat(Strings.StorageErrorDetailsX, storageErrorDetails);
      }
      var sqlErrorDetails = sqlExceptionInfo.ToString();
      if (!string.IsNullOrEmpty(sqlErrorDetails)) {
        builder.AppendLine();
        builder.AppendFormat(Strings.SqlErrorDetailsX, sqlErrorDetails);
      }
      if (!string.IsNullOrEmpty(queryText)) {
        builder.AppendLine();
        builder.AppendFormat(Strings.QueryX, queryText);
      }
      var sqlMessage = exception.Message;
      if (!string.IsNullOrEmpty(sqlMessage)) {
        builder.AppendLine();
        builder.AppendFormat(Strings.OriginalMessageX, sqlMessage);
      }

      var storageException = CreateStorageException(sqlExceptionInfo.Type, builder.ToString(), exception);
      storageException.Info = storageExceptionInfo;
      return storageException;
    }

    private StorageExceptionInfo GetStorageExceptionInfo(SqlExceptionInfo info)
    {
      var type = !string.IsNullOrEmpty(info.Table)
        ? domain.Model.Types.FirstOrDefault(t => t.MappingName==info.Table)
        : null;
      ColumnInfo column = null;
      if (type!=null && !string.IsNullOrEmpty(info.Column))
        type.Columns.TryGetValue(info.Column, out column);
      var field = column!=null ? column.Field : null;
      return new StorageExceptionInfo(type, field, info.Value, info.Constraint);
    }

    private static StorageException CreateStorageException(SqlExceptionType type, string message, Exception innerException)
    {
      switch (type) {
      case SqlExceptionType.ConnectionError:
        return new ConnectionErrorException(message, innerException);
      case SqlExceptionType.SyntaxError:
        return new SyntaxErrorException(message, innerException);
      case SqlExceptionType.CheckConstraintViolation:
        return new CheckConstraintViolationException(message, innerException);
      case SqlExceptionType.UniqueConstraintViolation:
        return new UniqueConstraintViolationException(message, innerException);
      case SqlExceptionType.ReferentialConstraintViolation:
        return new ReferentialConstraintViolationException(message, innerException);
      case SqlExceptionType.Deadlock:
        return new DeadlockException(message, innerException);
      case SqlExceptionType.SerializationFailure:
        return new TransactionSerializationFailureException(message, innerException);
      case SqlExceptionType.OperationTimeout:
        return new OperationTimeoutException(message, innerException);
      default:
        return new StorageException(message, innerException);
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