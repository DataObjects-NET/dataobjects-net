// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.29

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Common;

namespace Xtensive.Storage.Providers.Sql.Mappings
{
  /// <summary>
  /// Represents a mapping from the native .NET type to the server specific data type.
  /// </summary>
  [Serializable]
  public sealed class DataTypeMapping
  {
    /// <summary>
    /// Gets the native type.
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    /// Gets the data type information.
    /// </summary>
    public DataTypeInfo DataTypeInfo { get; private set; }

    /// <summary>
    /// Gets the <see cref="DbType"/>
    /// </summary>
    public DbType DbType { get; private set; }

    /// <summary>
    /// Gets the data reader accessor.
    /// </summary>
    public Func<DbDataReader, int, object> DataReaderAccessor { get; private set; }

    /// <summary>
    /// Gets the converter from native to SQL value.
    /// </summary>
    public Func<object, object> ToSqlValue { get; private set; }

    /// <summary>
    /// Gets the converter from SQL to native value.
    /// </summary>
    public Func<object, object> FromSqlValue { get; private set; }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="dataTypeInfo">A value for <see cref="DataTypeInfo"/>.</param>
    /// <param name="dbType">A value for <see cref="DbType"/>.</param>
    /// <param name="dataReaderAccessor">A value for <see cref="DataReaderAccessor"/>.</param>/// 
    public DataTypeMapping(Type type, DataTypeInfo dataTypeInfo, DbType dbType,
      Func<DbDataReader, int, object> dataReaderAccessor)
    {
      Type = type;
      DataTypeInfo = dataTypeInfo;
      DbType = dbType;
      DataReaderAccessor = dataReaderAccessor;
    }
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="dataTypeInfo">A value for <see cref="DataTypeInfo"/>.</param>
    /// <param name="dbType">A value for <see cref="DbType"/>.</param>
    /// <param name="dataReaderAccessor">A value for <see cref="DataReaderAccessor"/>.</param>
    /// <param name="toSqlValue">A value for <see cref="ToSqlValue"/>.</param>
    public DataTypeMapping(Type type, DataTypeInfo dataTypeInfo, DbType dbType,
      Func<DbDataReader, int, object> dataReaderAccessor, Func<object, object> toSqlValue)
    {
      Type = type;
      DataTypeInfo = dataTypeInfo;
      DbType = dbType;
      DataReaderAccessor = dataReaderAccessor;
      ToSqlValue = toSqlValue;
    }
  }
}
