// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.25

using System;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  public sealed class StorageExceptionBuilder
  {
    private readonly SqlDriver driver;
    private readonly Func<DomainModel> modelProvider;
    private readonly bool includeSqlInExceptions;

    public StorageException BuildException(Exception origin)
    {
      return BuildException(origin, null);
    }

    public StorageException BuildException(Exception origin, string queryText)
    {
      var sqlExceptionInfo = driver.GetExceptionInfo(origin);
      var storageExceptionInfo = GetStorageExceptionInfo(sqlExceptionInfo);
      var builder = new StringBuilder(Strings.SqlErrorOccured);

      if (storageExceptionInfo!=null) {
        var storageErrorDetails = storageExceptionInfo.ToString();
        if (!string.IsNullOrEmpty(storageErrorDetails)) {
          _ = builder.AppendLine()
            .AppendFormat(Strings.StorageErrorDetailsX, storageErrorDetails);
        }
      }
      var sqlErrorDetails = sqlExceptionInfo.ToString();
      if (!string.IsNullOrEmpty(sqlErrorDetails)) {
        _ = builder.AppendLine()
          .AppendFormat(Strings.SqlErrorDetailsX, sqlErrorDetails);
      }
      if (!string.IsNullOrEmpty(queryText) && includeSqlInExceptions) {
        _ = builder.AppendLine()
          .AppendFormat(Strings.QueryX, queryText);
      }
      var sqlMessage = origin.Message;
      if (!string.IsNullOrEmpty(sqlMessage)) {
        _ = builder.AppendLine()
          .AppendFormat(Strings.OriginalMessageX, sqlMessage);
      }

      var storageException = CreateStorageException(sqlExceptionInfo.Type, builder.ToString(), origin, storageExceptionInfo);
      return storageException;
    }

    private StorageExceptionInfo GetStorageExceptionInfo(SqlExceptionInfo info)
    {
      var model = modelProvider.Invoke();
      if (model==null)
        return null;
      var type = !string.IsNullOrEmpty(info.Table)
        ? model.Types.FirstOrDefault(t => t.MappingName==info.Table)
        : null;
      ColumnInfo column = null;
      if (type!=null && !string.IsNullOrEmpty(info.Column))
        type.Columns.TryGetValue(info.Column, out column);
      var field = column!=null ? column.Field : null;
      return new StorageExceptionInfo(type, field, info.Value, info.Constraint);
    }

    private static StorageException CreateStorageException(SqlExceptionType type, string message,
      Exception innerException, StorageExceptionInfo storageExceptionInfo)
    {
      switch (type) {
      case SqlExceptionType.ConnectionError:
        return new ConnectionErrorException(message, innerException) { Info = storageExceptionInfo};
      case SqlExceptionType.SyntaxError:
        return new SyntaxErrorException(message, innerException) { Info = storageExceptionInfo };
      case SqlExceptionType.CheckConstraintViolation:
        return new CheckConstraintViolationException(message, innerException) { Info = storageExceptionInfo };
      case SqlExceptionType.UniqueConstraintViolation:
        return new UniqueConstraintViolationException(message, innerException) { Info = storageExceptionInfo };
      case SqlExceptionType.ReferentialConstraintViolation:
        return new ReferentialConstraintViolationException(message, innerException) { Info = storageExceptionInfo };
      case SqlExceptionType.Deadlock:
        return new DeadlockException(message, innerException) { Info = storageExceptionInfo };
      case SqlExceptionType.SerializationFailure:
        return new TransactionSerializationFailureException(message, innerException) { Info = storageExceptionInfo };
      case SqlExceptionType.OperationTimeout:
        return new OperationTimeoutException(message, innerException) { Info = storageExceptionInfo };
      default:
        return new StorageException(message, innerException) { Info = storageExceptionInfo };
      }
    }

    // Constructors

    internal StorageExceptionBuilder(SqlDriver driver, DomainConfiguration configuration, Func<DomainModel> modelProvider)
    {
      this.driver = driver;
      this.modelProvider = modelProvider;

      includeSqlInExceptions = configuration.IncludeSqlInExceptions;
    }
  }
}