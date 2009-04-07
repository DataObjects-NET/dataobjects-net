// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Core.Reflection;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Providers.Sql.Mappings;
using ColumnInfo = Xtensive.Storage.Model.ColumnInfo;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlValueTypeMapper : InitializableHandlerBase
  {
    protected DomainHandler DomainHandler { get; private set; }

    /// <summary>
    /// Gets the data type mapping schema.
    /// </summary>
    protected DataTypeMappingSchema MappingSchema { get; private set; }

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns><see cref="DataTypeMapping"/> instance for the specified <paramref name="column"/>.
    /// If no mapping exists returns null.</returns>
    public DataTypeMapping TryGetTypeMapping(ColumnInfo column)
    {
      int length = column.Length.HasValue ? column.Length.Value : 0;
      Type type = column.ValueType;

      return TryGetTypeMapping(type, length);
    }

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns><see cref="DataTypeMapping"/> instance for the specified <paramref name="column"/>.</returns>
    /// <exception cref="InvalidOperationException">Type of column is not supported.</exception>
    public DataTypeMapping GetTypeMapping(ColumnInfo column)
    {
      int length = column.Length.HasValue ? column.Length.Value : 0;
      Type type = column.ValueType;

      return GetTypeMapping(type, length);
    }

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="type">The column type.</param>
    /// <returns><see cref="DataTypeMapping"/> instance for the specified <paramref name="type"/>.
    /// If no mapping exists returns null.</returns>
    public DataTypeMapping TryGetTypeMapping(Type type)
    {
      return TryGetTypeMapping(type, 0);
    }

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="type">The column type.</param>
    /// <returns><see cref="DataTypeMapping"/> instance for the specified <paramref name="type"/>.</returns>
    /// <exception cref="InvalidOperationException"><param name="type">Type</param> is not supported.</exception>
    public DataTypeMapping GetTypeMapping(Type type)
    {
      return GetTypeMapping(type, 0);
    }

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="type">The column type.</param>
    /// <param name="length">The column length.</param>
    /// <returns><see cref="DataTypeMapping"/> instance
    /// for the specified <paramref name="type"/> and <paramref name="length"/>.
    /// If no mapping exists returns null.</returns>
    public DataTypeMapping TryGetTypeMapping(Type type, int length)
    {
      {
        DataTypeMapping mapping = MappingSchema.GetExactMapping(type);
        if (mapping != null)
          return mapping;
      }

      DataTypeMapping[] ambigiousMappings = MappingSchema.GetAmbigiousMappings(type);
      if (ambigiousMappings != null)
      {
        foreach (DataTypeMapping mapping in ambigiousMappings)
        {
          var sdti = mapping.DataTypeInfo as StreamDataTypeInfo;
          if (sdti == null)
            return mapping;

          if (length == 0)
            return mapping;

          if (sdti.Length.MaxValue < length)
            continue;

          return mapping;
        }
      }
      return null;
    }

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="type">The column type.</param>
    /// <param name="length">The column length.</param>
    /// <returns><see cref="DataTypeMapping"/> instance for the specified <paramref name="type"/> and <paramref name="length"/>.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="type"/> is not supported.</exception>
    public DataTypeMapping GetTypeMapping(Type type, int length)
    {
      var result = TryGetTypeMapping(type, length);
      if (result == null)
        throw new InvalidOperationException(string.Format("Type '{0}' is not supported.", type.GetShortName()));
      return result;
    }

    public SqlValueType BuildSqlValueType(ColumnInfo columnInfo)
    {
      DataTypeMapping dtm = GetTypeMapping(columnInfo);
      return BuildSqlValueType(columnInfo, dtm);
    }

    public SqlValueType BuildSqlValueType(Type type, int length)
    {
      DataTypeMapping dtm = GetTypeMapping(type);
      return BuildSqlValueType(length, dtm);
    }

    private SqlValueType BuildSqlValueType(ColumnInfo column, DataTypeMapping typeMapping)
    {
      int length = column.Length.HasValue ? column.Length.Value : 0;
      Type type = column.ValueType;

      return BuildSqlValueType(length, typeMapping);
    }

    private SqlValueType BuildSqlValueType(int length, DataTypeMapping typeMapping)
    {
      var streamInfo = typeMapping.DataTypeInfo as StreamDataTypeInfo;
      if (streamInfo!=null) {
        if (length==0)
          return new SqlValueType(streamInfo.SqlType, streamInfo.Length.MaxValue);
        return new SqlValueType(streamInfo.SqlType, length);
      }
      var decimalInfo = typeMapping.DataTypeInfo as FractionalDataTypeInfo<decimal>;
      if (decimalInfo != null)
        return new SqlValueType(decimalInfo.SqlType, decimalInfo.Precision.MaxValue, decimalInfo.Scale.DefaultValue.Value);
      
      return new SqlValueType(typeMapping.DataTypeInfo.SqlType);
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      DomainHandler = Handlers.DomainHandler as DomainHandler;
      MappingSchema = new DataTypeMappingSchema();
      BuildNativeTypes();
      BuildTypeSubstitutes();
    }

    private void BuildNativeTypes()
    {
      DataTypeCollection types = DomainHandler.SqlDriver.ServerInfo.DataTypes;
      BuildDataTypeMapping(types.Boolean);
      BuildDataTypeMapping(types.Byte);
      BuildDataTypeMapping(types.DateTime);
      BuildDataTypeMapping(types.Decimal);
      BuildDataTypeMapping(types.Double); 
      BuildDataTypeMapping(types.Float);
      BuildDataTypeMapping(types.Guid);
      BuildDataTypeMapping(types.Int16);
      BuildDataTypeMapping(types.Int32);
      BuildDataTypeMapping(types.Int64);
      BuildDataTypeMapping(types.SByte);
      BuildDataTypeMapping(types.UInt16);
      BuildDataTypeMapping(types.UInt32);
      BuildDataTypeMapping(types.UInt64);
      BuildDataTypeMapping(types.VarBinary);
      BuildDataTypeMapping(types.VarBinaryMax);
      BuildDataTypeMapping(types.VarChar);
      BuildDataTypeMapping(types.VarCharMax);
    }

    protected virtual void BuildTypeSubstitutes()
    {
    }

    protected void BuildDataTypeMapping(DataTypeInfo dataTypeInfo)
    {
      if (dataTypeInfo==null)
        return;

      DataTypeMapping mapping = CreateDataTypeMapping(dataTypeInfo);
      MappingSchema.Register(mapping);
    }

    protected virtual DataTypeMapping CreateDataTypeMapping(DataTypeInfo dataTypeInfo)
    {
      return new DataTypeMapping(dataTypeInfo, BuildDataReaderAccessor(dataTypeInfo), GetDbType(dataTypeInfo));
    }

    protected virtual DbType GetDbType(DataTypeInfo dataTypeInfo)
    {
      TypeCode typeCode = Type.GetTypeCode(dataTypeInfo.Type);
      switch (typeCode) {
      case TypeCode.Object:
        if (dataTypeInfo.Type==typeof (byte[]))
          return DbType.Binary;
        if (dataTypeInfo.Type == typeof(Guid))
          return DbType.Guid;
        throw new ArgumentOutOfRangeException();
      case TypeCode.Boolean:
        return DbType.Boolean;
      case TypeCode.Char:
        return DbType.StringFixedLength;
      case TypeCode.SByte:
        return DbType.SByte;
      case TypeCode.Byte:
        return DbType.Byte;
      case TypeCode.Int16:
        return DbType.Int16;
      case TypeCode.UInt16:
        return DbType.UInt16;
      case TypeCode.Int32:
        return DbType.Int32;
      case TypeCode.UInt32:
        return DbType.UInt32;
      case TypeCode.Int64:
        return DbType.Int64;
      case TypeCode.UInt64:
        return DbType.UInt64;
      case TypeCode.Single:
        return DbType.Single;
      case TypeCode.Double:
        return DbType.Double;
      case TypeCode.Decimal:
        return DbType.Decimal;
      case TypeCode.DateTime:
        return DbType.DateTime;
      case TypeCode.String:
        return DbType.String;
      default:
        throw new ArgumentOutOfRangeException();
      }
    }

    protected virtual Func<DbDataReader, int, object> BuildDataReaderAccessor(DataTypeInfo dataTypeInfo)
    {
      Type type = dataTypeInfo.Type;
      TypeCode typeCode = Type.GetTypeCode(type);
      switch (typeCode) {
      case TypeCode.Object:
        if (type==typeof (byte[]))
          return (reader, fieldIndex) => reader.GetValue(fieldIndex);
        if (type==typeof (Guid))
          return (reader, fieldIndex) => reader.GetGuid(fieldIndex);
        else
          throw new ArgumentOutOfRangeException();
      case TypeCode.Boolean:
        return (reader, fieldIndex) => reader.GetBoolean(fieldIndex);
      case TypeCode.Char:
        return (reader, fieldIndex) => reader.GetChar(fieldIndex);
      case TypeCode.SByte:
        return (reader, fieldIndex) => Convert.ToSByte(reader.GetDecimal(fieldIndex));
      case TypeCode.Byte:
        return (reader, fieldIndex) => reader.GetByte(fieldIndex);
      case TypeCode.Int16:
        return (reader, fieldIndex) => reader.GetInt16(fieldIndex);
      case TypeCode.UInt16:
        return (reader, fieldIndex) => Convert.ToUInt16(reader.GetDecimal(fieldIndex));
      case TypeCode.Int32:
        return (reader, fieldIndex) => reader.GetInt32(fieldIndex);
      case TypeCode.UInt32:
        return (reader, fieldIndex) => Convert.ToUInt32(reader.GetDecimal(fieldIndex));
      case TypeCode.Int64:
        return (reader, fieldIndex) => reader.GetInt64(fieldIndex);
      case TypeCode.UInt64:
        return (reader, fieldIndex) => Convert.ToUInt64(reader.GetDecimal(fieldIndex));
      case TypeCode.Single:
        return (reader, fieldIndex) => reader.GetFloat(fieldIndex);
      case TypeCode.Double:
        return (reader, fieldIndex) => reader.GetDouble(fieldIndex);
      case TypeCode.Decimal:
        return (reader, fieldIndex) => reader.GetDecimal(fieldIndex);
      case TypeCode.DateTime:
        return (reader, fieldIndex) => reader.GetDateTime(fieldIndex);
      case TypeCode.String:
        return (reader, fieldIndex) => reader.GetString(fieldIndex);
      default:
        throw new ArgumentOutOfRangeException();
      }
    }
  }
}