﻿// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.MySql
{
  internal abstract class Driver : SqlDriver
  {
    /// <inheritdoc/>
    protected override SqlConnection DoCreateConnection()
    {
      return new Connection(this);
    }

    /// <inheritdoc/>
    public override SqlExceptionType GetExceptionType(Exception exception)
    {
      var nativeException = exception as MySqlException;
      if (nativeException == null)
        return SqlExceptionType.Unknown;
      int errorCode = nativeException.Number;
      string errorMessage = nativeException.Message;

      //TODO: intepret some of the error codes (Malisa)
      //http://dev.mysql.com/doc/refman/5.0/en/error-handling.html

      switch (errorCode) {
        case 2002:
        case 2003:
          return SqlExceptionType.ConnectionError;
        case 1149:
          return SqlExceptionType.SyntaxError;
        case 1169:
          return SqlExceptionType.UniqueConstraintViolation;
        case 1205:
          return SqlExceptionType.OperationTimeout;
        case 1213:
          return SqlExceptionType.Deadlock;
        case 1216:
        case 1217:
          return SqlExceptionType.ReferentialConstraintViolation;
        case 1613:
          return SqlExceptionType.OperationTimeout;
        default:
          return SqlExceptionType.Unknown;
      }
    }

    // Constructors

    protected Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {}
  }
}