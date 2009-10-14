// Copyright (C) 2009 Xtensive LLC.
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
    public SqlConnection CreateConnection(Session session, UrlInfo urlInfo)
    {
      try {
        if (IsDebugLoggingEnabled) {
          Log.Debug(Strings.LogSessionXCreatingConnectionY, 
            session.GetFullNameSafely(), urlInfo);
        }
        return underlyingDriver.CreateConnection(urlInfo);
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    public void OpenConnection(Session session, SqlConnection connection)
    {
      try {
        if (IsDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXOpeningConnectionY, 
            session.GetFullNameSafely(), connection.Url);
        connection.Open();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    public void CloseConnection(Session session, SqlConnection connection)
    {
      try {
        if (IsDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXClosingConnectionY, 
            session.GetFullNameSafely(), connection.Url);
        if (connection.State==ConnectionState.Open)
          connection.Close();
        connection.Dispose();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }      
    }

    public DbTransaction BeginTransaction(Session session, SqlConnection connection, IsolationLevel isolationLevel)
    {
      try {
        if (IsDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXBeginningTransactionWithYIsolationLevel, 
            session.GetFullNameSafely(), isolationLevel);
        return connection.BeginTransaction(isolationLevel);
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }      
    }

    public void CommitTransaction(Session session, DbTransaction transaction)
    {
      try {
        if (IsDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXCommitTransaction, 
            session.GetFullNameSafely());
        transaction.Commit();
        transaction.Dispose();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    public void RollbackTransaction(Session session, DbTransaction transaction)
    {
      try {
        if (IsDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXRollbackTransaction, 
            session.GetFullNameSafely());
        transaction.Rollback();
        transaction.Dispose();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }      
    }

 
    public int ExecuteNonQuery(Session session, DbCommand command)
    {
      try {
        if (IsDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXQueryY, 
            session.GetFullNameSafely(), command.ToHumanReadableString());
        return command.ExecuteNonQuery();
      }
      catch (Exception exception) {
        throw TranslateException(command.ToHumanReadableString(), exception);
      }
    }

    public object ExecuteScalar(Session session, DbCommand command)
    {
      try {
        if (IsDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXQueryY, 
            session.GetFullNameSafely(), command.ToHumanReadableString());
        return command.ExecuteScalar();
      }
      catch (Exception exception) {
        throw TranslateException(command.ToHumanReadableString(), exception);
      }
    }

    public DbDataReader ExecuteReader(Session session, DbCommand command)
    {
      try {
        if (IsDebugLoggingEnabled)
          Log.Debug(Strings.LogSessionXQueryY, 
            session.GetFullNameSafely(), command.ToHumanReadableString());
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

    private Exception TranslateException(string queryText, Exception exception)
    {
      var exceptionType = underlyingDriver.GetExceptionType(exception);
      var errorText = string.IsNullOrEmpty(queryText)
        ? string.Format(Strings.ExErrorX, exceptionType)
        : string.Format(Strings.ExErrorXWhileExecutingQueryY, exceptionType, queryText);
      return exceptionType.IsReprocessable()
        ? new ReprocessableException(errorText, exception)
        : new StorageException(errorText, exception);
    }
  }
}