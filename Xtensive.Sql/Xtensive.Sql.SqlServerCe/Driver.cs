// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Data.SqlServerCe;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.SqlServerCe
{
  internal abstract class Driver : SqlDriver
  {
    public override SqlConnection CreateConnection()
    {
      return new Connection(this);
    }

    public override SqlExceptionType GetExceptionType(Exception exception)
    {
      var nativeException = exception as SqlCeException;
      if (nativeException==null)
        return SqlExceptionType.Unknown;
      int errorCode = nativeException.NativeError;
      if (errorCode >= 25500 && errorCode <= 26499)
        return SqlExceptionType.SyntaxError;
      switch (nativeException.NativeError) {
      case 25090:
        return SqlExceptionType.OperationTimeout;
      case 25016:
        return SqlExceptionType.UniqueConstraintViolation;
      case 25025:
      case 25026:
        return SqlExceptionType.ReferentialConstraintViolation;
      default:
        return SqlExceptionType.Unknown;
      }
    }

    // Constructors

    protected Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}