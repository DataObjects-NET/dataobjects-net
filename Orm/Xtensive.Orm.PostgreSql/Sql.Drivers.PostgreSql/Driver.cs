// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.IO;
using System.Net.Sockets;
using System.Security;
using Npgsql;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql
{
  internal abstract class Driver : SqlDriver
  {
    [SecuritySafeCritical]
    protected override SqlConnection DoCreateConnection()
    {
      return new Connection(this);
    }

    [SecuritySafeCritical]
    public override SqlExceptionType GetExceptionType(Exception exception)
    {
      // PostgresException is server-side exception.
      // NpgsqlException is might be a client-side exception.
      // for instance, Timeout of connection is client-side now (don't know why:))
      var serverSideException = exception as PostgresException;
      if (serverSideException!=null)
        return ProcessServerSideException(serverSideException);

      var clientSideException = exception as NpgsqlException;
      if (clientSideException!=null)
        return ProcessClientSideException(clientSideException);

      return SqlExceptionType.Unknown;
    }

    private SqlExceptionType ProcessServerSideException(PostgresException serverSideException)
    {
      // There is no guaranteed way to detect a operation timeout.
      // We simply check that error message says something about CommandTimeout connection parameter.
      if (serverSideException.Message.ToUpperInvariant().Contains("COMMANDTIMEOUT"))
        return SqlExceptionType.OperationTimeout;

      if (serverSideException.SqlState.Length!=5)
        return SqlExceptionType.Unknown;

      var errorCode = serverSideException.SqlState.ToUpperInvariant();
      var errorCodeClass = errorCode.Substring(0, 2);

      // Error codes have been taken from
      // http://www.postgresql.org/docs/8.4/static/errcodes-appendix.html

      switch (errorCodeClass) {
        case "08": // connection_exception
          return SqlExceptionType.ConnectionError;
        case "42": // syntax_error_or_access_rule_violation
          if (errorCode == "42501")
            return SqlExceptionType.Unknown;
          return SqlExceptionType.SyntaxError;
      }

      switch (errorCode) {
        case "23502": // not_null_violation
        case "23514": // check_violation
          return SqlExceptionType.CheckConstraintViolation;
        case "23001": // restrict_violation
        case "23503": // foreign_key_violation
          return SqlExceptionType.ReferentialConstraintViolation;
        case "23505": // unique_violation
          return SqlExceptionType.UniqueConstraintViolation;
        case "40P01": // deadlock_detected
          return SqlExceptionType.Deadlock;
        case "40001": // serialization_failure
          return SqlExceptionType.SerializationFailure;
      }

      return SqlExceptionType.Unknown;
    }

    private SqlExceptionType ProcessClientSideException(NpgsqlException clientSideException)
    {
      var innerException = clientSideException.InnerException;
      if (innerException==null)
        return SqlExceptionType.Unknown;
      var ioException = innerException as IOException;
      if (ioException!=null) {
        if (ioException.InnerException!=null) {
          var socetException = ioException.InnerException as SocketException;
          if (socetException!=null && socetException.SocketErrorCode==SocketError.TimedOut)
            return SqlExceptionType.OperationTimeout;
        }
      }
      return SqlExceptionType.Unknown;
    }

    // Constructors

    protected Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}