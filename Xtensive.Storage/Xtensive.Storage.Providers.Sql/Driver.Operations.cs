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
    public SqlConnection CreateConnection(UrlInfo urlInfo)
    {
      try {
        return underlyingDriver.CreateConnection(urlInfo);
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    public void OpenConnection(SqlConnection connection)
    {
      try {
        connection.Open();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    public void CloseConnection(SqlConnection connection)
    {
      try {
        if (connection.State==ConnectionState.Open)
          connection.Close();
        connection.Dispose();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }      
    }

    public DbTransaction BeginTransaction(SqlConnection connection, IsolationLevel isolationLevel)
    {
      try {
        return connection.BeginTransaction(isolationLevel);
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }      
    }

    public void CommitTransaction(DbTransaction transaction)
    {
      try {
        transaction.Commit();
        transaction.Dispose();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }
    }

    public void RollbackTransaction(DbTransaction transaction)
    {
      try {
        transaction.Rollback();
        transaction.Dispose();
      }
      catch (Exception exception) {
        throw TranslateException(null, exception);
      }      
    }

 
    public int ExecuteNonQuery(DbCommand command)
    {
      try {
        return command.ExecuteNonQuery();
      }
      catch (Exception exception) {
        throw TranslateException(command.CommandText, exception);
      }
    }

    public object ExecuteScalar(DbCommand command)
    {
      try {
        return command.ExecuteScalar();
      }
      catch (Exception exception) {
        throw TranslateException(command.CommandText, exception);
      }
    }

    public DbDataReader ExecuteReader(DbCommand command)
    {
      try {
        return command.ExecuteReader();
      }
      catch (Exception exception) {
        throw TranslateException(command.CommandText, exception);
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