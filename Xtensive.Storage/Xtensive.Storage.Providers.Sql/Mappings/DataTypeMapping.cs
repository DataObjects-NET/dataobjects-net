// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.29

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Sql.Common;

namespace Xtensive.Storage.Providers.Sql.Mappings
{
  [Serializable]
  public sealed class DataTypeMapping
  {
    public Type Type { get; private set; }

    public DataTypeInfo DataTypeInfo { get; private set; }

    public DbType DbType { get; private set; }

    public Func<DbDataReader, int, object> DataReaderAccessor { get; private set; }

    public Func<object, object> ToSqlValue { get; private set; }

    public Func<object, object> FromSqlValue { get; private set; }

    /// <summary>
    /// Translates to SQL value.
    /// </summary>
    /// <param name="value">A value to translate.</param>
    /// <returns></returns>
    public object TranslateToSqlValue(object value)
    {
      return value!=null && value!=DBNull.Value && ToSqlValue!=null
        ? ToSqlValue(value)
        : value;
    }

    // Constructor

    private DataTypeMapping(Type type, DataTypeInfo dataTypeInfo, Func<DbDataReader, int, object> dataReaderAccessor)
    {
      Type = type;
      DataTypeInfo = dataTypeInfo;
      DataReaderAccessor = dataReaderAccessor;
    }

    public DataTypeMapping(DataTypeInfo dataTypeInfo, Func<DbDataReader, int, object> dataReaderAccessor, DbType dbType)
      : this(dataTypeInfo.Type, dataTypeInfo, dataReaderAccessor)
    {
      DbType = dbType;
    }

    public DataTypeMapping(DataTypeInfo dataTypeInfo, Func<DbDataReader, int, object> dataReaderAccessor, DbType dbType, Func<object, object> toSqlValue, Func<object, object> fromSqlValue)
      : this(dataTypeInfo, dataReaderAccessor, dbType)
    {
      ToSqlValue = toSqlValue;
      FromSqlValue = fromSqlValue;
    }

    public DataTypeMapping(Type type, DataTypeInfo dataTypeInfo, Func<DbDataReader, int, object> dataReaderAccessor, DbType dbType, Func<object, object> toSqlValue, Func<object, object> fromSqlValue)
      : this(type, dataTypeInfo, dataReaderAccessor)
    {
      DbType = dbType;
      ToSqlValue = toSqlValue;
      FromSqlValue = fromSqlValue;
    }
  }
}