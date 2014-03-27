// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.08

using System.Linq;
using Xtensive.Sql.Info;
using FirebirdSql.Data.FirebirdClient;

namespace Xtensive.Sql.Drivers.Firebird
{
  internal abstract class Driver : SqlDriver
  {
    /// <inheritdoc/>
    protected override SqlConnection DoCreateConnection()
    {
      return new Connection(this);
    }

    /// <inheritdoc/>
    public override SqlExceptionType GetExceptionType(System.Exception exception)
    {
      var nativeException = exception as FbException;
      if (nativeException==null)
        return base.GetExceptionType(exception);
      var fbError = nativeException.Errors.LastOrDefault();
      if (fbError==null)
        return base.GetExceptionType(exception);
      switch (fbError.Number) {
        case 335544569:
          return SqlExceptionType.SyntaxError;
        case 335544347: //  exactly: validation error for column
        case 335544558: //  exactly: operation violates check constraint
          return SqlExceptionType.CheckConstraintViolation;
        case 335544665:
          return SqlExceptionType.UniqueConstraintViolation;
        case 335544466:
          return SqlExceptionType.ReferentialConstraintViolation;
        case 335544336: //  exactly: deadlock 
        case 335544345: //  exactly: lock conflict on no wait transaction
          return SqlExceptionType.Deadlock;
        case 335544510: //  exactly: lock_timeout on wait transaction
          return SqlExceptionType.OperationTimeout;
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