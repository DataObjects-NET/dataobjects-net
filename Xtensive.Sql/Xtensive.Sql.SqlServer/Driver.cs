// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Data.SqlClient;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.SqlServer
{
  internal abstract class Driver : SqlDriver
  {
    protected override SqlConnection CreateConnection(string connectionString)
    {
      return new Connection(this, connectionString);
    }

    public override SqlExceptionType GetExceptionType(Exception exception)
    {
      var nativeException = exception as SqlException;
      if (nativeException==null)
        return SqlExceptionType.Unknown;
      int errorCode = nativeException.Number;
      string errorMessage = nativeException.Message;
      if (errorCode==-2)
        return SqlExceptionType.OperationTimeout;
      if (errorCode==1205)
        return SqlExceptionType.Deadlock;
      if (errorCode==3958 || errorCode==3960)
        return SqlExceptionType.SerializationFailure;
      if (errorCode==2627)
        return SqlExceptionType.UniqueConstraintViolation;
      if (errorCode==515) // NOT NULL constraint
        return SqlExceptionType.CheckConstraintViolation;
      if (errorCode==547) {
        // We assume that SQL keywords are not localizable,
        // so it is safe to search them in message.
        if (errorMessage.Contains("CHECK"))
          return SqlExceptionType.CheckConstraintViolation;
        if (errorMessage.Contains("FOREIGN KEY") || errorMessage.Contains("REFERENCE"))
          return SqlExceptionType.ReferentialConstraintViolation;
        if (errorMessage.Contains("UNIQUE KEY") || errorMessage.Contains("PRIMARY KEY"))
          return SqlExceptionType.UniqueConstraintViolation;
        return SqlExceptionType.Unknown;
      }
      if (errorCode >= 100 && errorCode <= 499)
        return SqlExceptionType.SyntaxError;
      return SqlExceptionType.Unknown;
    }

    // Constructors

    protected Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}