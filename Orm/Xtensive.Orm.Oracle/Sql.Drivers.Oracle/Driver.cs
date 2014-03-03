// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Oracle.DataAccess.Client;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Oracle
{
  internal abstract class Driver : SqlDriver
  {
    protected override SqlConnection DoCreateConnection()
    {
      return new Connection(this);
    }

    public override SqlExceptionType GetExceptionType(System.Exception exception)
    {
      var nativeException = exception as OracleException;
      if (nativeException==null)
        return SqlExceptionType.Unknown;
      switch (nativeException.Number) {
      case 1:
        return SqlExceptionType.UniqueConstraintViolation;
      case 2291:
      case 2292:
        return SqlExceptionType.ReferentialConstraintViolation;
      case 1400:
      case 2290:
        return SqlExceptionType.CheckConstraintViolation;
      case 60:
        return SqlExceptionType.Deadlock;
      case 1555:
      case 8177:
        return SqlExceptionType.SerializationFailure;
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