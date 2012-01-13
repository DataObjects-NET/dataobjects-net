// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServer
{
  internal abstract class Driver : SqlDriver
  {
    private readonly ErrorMessageParser errorMessageParser;

    protected override SqlConnection CreateConnection(string connectionString)
    {
      return new Connection(this, connectionString);
    }

    public override SqlExceptionType GetExceptionType(Exception exception)
    {
      return GetExceptionInfo(exception).Type;
    }

    public override SqlExceptionInfo GetExceptionInfo(Exception exception)
    {
      var nativeException = exception as SqlException;
      if (nativeException==null)
        return SqlExceptionInfo.Create(SqlExceptionType.Unknown);
      int errorCode = nativeException.Number;
      string errorMessage = nativeException.Message;
      if (errorCode==-2)
        return SqlExceptionInfo.Create(SqlExceptionType.OperationTimeout);
      if (errorCode==1205)
        return SqlExceptionInfo.Create(SqlExceptionType.Deadlock);
      if (errorCode==3958 || errorCode==3960)
        return SqlExceptionInfo.Create(SqlExceptionType.SerializationFailure);
      if (errorCode >= 100 && errorCode <= 499)
        return SqlExceptionInfo.Create(SqlExceptionType.SyntaxError);
      var info = new SqlExceptionInfo();
      if (TryProvideErrorContext(errorCode, errorMessage, info)) {
        info.Lock();
        return info;
      }
      return SqlExceptionInfo.Create(SqlExceptionType.Unknown);
    }

    protected virtual bool TryProvideErrorContext(int errorCode, string errorMessage, SqlExceptionInfo info)
    {
      Dictionary<int, string> parseResult;
      string duplicateValue;
      switch (errorCode) {
        case 2627:
          parseResult = errorMessageParser.Parse(errorCode, errorMessage);
          info.Type = SqlExceptionType.UniqueConstraintViolation;
          info.Constraint = parseResult[2];
          info.Table = ErrorMessageParser.CutSchemaPrefix(parseResult[3]);
          if (parseResult.TryGetValue(4, out duplicateValue))
            info.Value = duplicateValue;
          break;
        case 2601:
          parseResult = errorMessageParser.Parse(errorCode, errorMessage);
          info.Type = SqlExceptionType.UniqueConstraintViolation;
          info.Table = ErrorMessageParser.CutSchemaPrefix(parseResult[1]);
          info.Constraint = parseResult[2];
          if (parseResult.TryGetValue(3, out duplicateValue))
            info.Value = duplicateValue;
          break;
        case 515:
          parseResult = errorMessageParser.Parse(errorCode, errorMessage);
          info.Type = SqlExceptionType.CheckConstraintViolation;
          info.Column = parseResult[1];
          info.Table = ErrorMessageParser.CutDatabaseAndSchemaPrefix(parseResult[2]);
          break;
        case 547:
          parseResult = errorMessageParser.Parse(errorCode, errorMessage);
          switch (parseResult[2]) {
            case "FOREIGN KEY":
            case "REFERENCE":
              info.Type = SqlExceptionType.ReferentialConstraintViolation;
              break;
            case "UNIQUE KEY":
            case "PRIMARY KEY":
              info.Type = SqlExceptionType.UniqueConstraintViolation;
              break;
            case "CHECK":
              info.Type = SqlExceptionType.CheckConstraintViolation;
              break;
            default:
              return false;
          }
          info.Constraint = parseResult[3];
          info.Database = parseResult[4];
          info.Table = ErrorMessageParser.CutSchemaPrefix(parseResult[5]);
          info.Column = ErrorMessageParser.ExtractQuotedText(parseResult[6]);
          break;
        default:
          return false;
      }
      return true;
    }

    // Constructors

    protected Driver(CoreServerInfo coreServerInfo, ErrorMessageParser errorMessageParser)
      : base(coreServerInfo)
    {
      this.errorMessageParser = errorMessageParser;
    }
  }
}