// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using MySql.Data.MySqlClient;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.MySql
{
  internal abstract class Driver : SqlDriver
  {
    /// <inheritdoc/>
    protected override SqlConnection CreateConnection(string connectionString)
    {
      return new Connection(this, connectionString);
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

      return SqlExceptionType.Unknown;
    }

    // Constructors

    protected Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {}
  }
}