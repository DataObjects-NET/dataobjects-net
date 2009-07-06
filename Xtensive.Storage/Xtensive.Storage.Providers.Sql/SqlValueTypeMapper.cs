// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Xtensive.Core.Reflection;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Providers.Sql.Mappings;
using Xtensive.Storage.Providers.Sql.Resources;
using ColumnInfo = Xtensive.Storage.Model.ColumnInfo;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A SQL specific handler that builds mapping between native .NET types and server specific data types.
  /// </summary>
  public class SqlValueTypeMapper : InitializableHandlerBase
  {
    private const short MaxDecimalPrecision = 60;

    /// <summary>
    /// Gets the domain handler.
    /// </summary>
    protected DomainHandler DomainHandler { get; private set; }

    /// <summary>
    /// Gets the data type mapping schema.
    /// </summary>
    protected DataTypeMappingSchema MappingSchema { get; private set; }

    #region TryGetTypeMapping methods

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns><see cref="DataTypeMapping"/> instance for the specified <paramref name="column"/>.
    /// If no mapping exists returns null.</returns>
    public DataTypeMapping TryGetTypeMapping(ColumnInfo column)
    {
      return TryGetTypeMapping(column.ValueType, column.Length);
    }

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="type">The column type.</param>
    /// <param name="length">The column length.</param>
    /// <returns><see cref="DataTypeMapping"/> instance
    /// for the specified <paramref name="type"/> and <paramref name="length"/>.
    /// If no mapping exists returns null.</returns>
    public DataTypeMapping TryGetTypeMapping(Type type, int? length)
    {
      if (length==null)
        length = 0;

      var exactMapping = MappingSchema.GetExactMapping(type);
      if (exactMapping!=null)
        return exactMapping;

      DataTypeMapping[] ambigiousMappings = MappingSchema.GetAmbigiousMappings(type);
      if (ambigiousMappings!=null) {
        foreach (DataTypeMapping mapping in ambigiousMappings) {
          var sdti = mapping.DataTypeInfo as StreamDataTypeInfo;
          if (sdti==null)
            return mapping;

          if (length==0)
            return mapping;

          if (sdti.Length.MaxValue < length)
            continue;

          return mapping;
        }
        return ambigiousMappings
          .OrderByDescending(mapping => ((StreamDataTypeInfo) mapping.DataTypeInfo).Length.MaxValue)
          .FirstOrDefault();
      }
      return null;
    }

    #endregion

    #region GetTypeMapping methods

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns><see cref="DataTypeMapping"/> instance for the specified <paramref name="column"/>.</returns>
    /// <exception cref="InvalidOperationException">Type of column is not supported.</exception>
    public DataTypeMapping GetTypeMapping(ColumnInfo column)
    {
      return GetTypeMapping(column.ValueType, column.Length);
    }

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="type">The column type.</param>
    /// <param name="length">The column length.</param>
    /// <returns><see cref="DataTypeMapping"/> instance for the specified <paramref name="type"/> and <paramref name="length"/>.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="type"/> is not supported.</exception>
    public DataTypeMapping GetTypeMapping(Type type, int? length)
    {
      var result = TryGetTypeMapping(type, length);
      if (result == null)
        throw new InvalidOperationException(string.Format(Strings.ExTypeXIsNotSupported, type.GetShortName()));
      return result;
    }

    #endregion

    #region BuildSqlValueType methods

    /// <summary>
    /// Builds the type of the SQL value.
    /// </summary>
    /// <param name="columnInfo">The column info.</param>
    /// <returns></returns>
    public SqlValueType BuildSqlValueType(ColumnInfo columnInfo)
    {
      DataTypeMapping dtm = GetTypeMapping(columnInfo);
      return BuildSqlValueType(dtm, columnInfo.Length);
    }

    /// <summary>
    /// Builds the <see cref="SqlValueType"/> from specified <see cref="Type"/> and length.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public SqlValueType BuildSqlValueType(Type type, int? length)
    {
      DataTypeMapping dtm = GetTypeMapping(type, length);
      return BuildSqlValueType(dtm, length);
    }

    /// <summary>
    /// Builds the <see cref="SqlValueType"/> from specified <see cref="DataTypeMapping"/>
    /// and length.
    /// </summary>
    /// <param name="typeMapping">The type mapping.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public SqlValueType BuildSqlValueType(DataTypeMapping typeMapping, int? length)
    {
      var streamInfo = typeMapping.DataTypeInfo as StreamDataTypeInfo;
      if (streamInfo != null) {
        return length==null || length==0
          ? new SqlValueType(streamInfo.SqlType, streamInfo.Length.MaxValue)
          : new SqlValueType(streamInfo.SqlType, Math.Min(length.Value, streamInfo.Length.MaxValue));
      }
      var decimalInfo = typeMapping.DataTypeInfo as FractionalDataTypeInfo<decimal>;
      if (decimalInfo != null)
        return new SqlValueType(decimalInfo.SqlType, decimalInfo.Precision.MaxValue, decimalInfo.Scale.MaxValue);

      var charInfo = typeMapping.DataTypeInfo as RangeDataTypeInfo<char>;
      if (charInfo != null)
        return new SqlValueType(charInfo.SqlType, 1);

      return new SqlValueType(typeMapping.DataTypeInfo.SqlType);
    }

    #endregion

    #region CreateXxxMapping methods

    protected virtual DataTypeMapping CreateByteArrayMapping(DataTypeInfo type)
    {
      return new DataTypeMapping(typeof(byte[]), type, DbType.Binary, (reader, index) => reader.GetValue(index));
    }

    protected virtual DataTypeMapping CreateStringMapping(DataTypeInfo type)
    {
      return new DataTypeMapping(typeof(string), type, DbType.String, (reader, index) => reader.GetString(index));
    }

    protected virtual DataTypeMapping CreateGuidMapping(DataTypeInfo type)
    {
      return new DataTypeMapping(typeof(Guid), type, DbType.Guid, (reader, index) => reader.GetGuid(index));
    }

    protected virtual DataTypeMapping CreateDateTimeMapping(DataTypeInfo type)
    {
      return new DataTypeMapping(typeof(DateTime), type, DbType.DateTime, (reader, index) => reader.GetDateTime(index));
    }

    protected virtual DataTypeMapping CreateTimeSpanMapping(DataTypeInfo type)
    {
      throw new NotSupportedException();
    }

    protected virtual DataTypeMapping CreateDecimalMapping(DataTypeInfo type)
    {
      var oldTypeInfo = DomainHandler.Driver.ServerInfo.DataTypes.Decimal;
      var newPrecisionMinValue = oldTypeInfo.Precision.MinValue;
      var newPrecisionMaxValue = Math.Min(oldTypeInfo.Precision.MaxValue, MaxDecimalPrecision);
      var newPrecision = new ValueRange<short>(newPrecisionMinValue, newPrecisionMaxValue, newPrecisionMaxValue);
      var newScaleMaxValue = (short) (newPrecisionMaxValue / 2);
      var newScale = new ValueRange<short>(oldTypeInfo.Scale.MinValue, newScaleMaxValue, newScaleMaxValue);
      var newTypeInfo = new FractionalDataTypeInfo<decimal>(oldTypeInfo.SqlType, null) {Scale = newScale, Precision = newPrecision};
      
      return new DataTypeMapping(typeof(decimal), newTypeInfo, DbType.Decimal, (reader, index) => reader.GetDecimal(index));
    }

    protected virtual DataTypeMapping CreateDoubleMapping(DataTypeInfo type)
    {
      return new DataTypeMapping(typeof(double), type, DbType.Double, (reader, index) => reader.GetDouble(index));
    }

    protected virtual DataTypeMapping CreateFloatMapping(DataTypeInfo type)
    {
      return new DataTypeMapping(typeof(float), type, DbType.Single, (reader, index) => reader.GetFloat(index));
    }

    protected virtual DataTypeMapping CreateUInt64Mapping(DataTypeInfo type)
    {
      var @decimal = DomainHandler.Driver.ServerInfo.DataTypes.Decimal;
      var @ulong = new FractionalDataTypeInfo<decimal>(@decimal.SqlType, null)
        {
          Value = new ValueRange<decimal>(ulong.MinValue, ulong.MaxValue),
          Precision = new ValueRange<short>(20, 20, 20),
          Scale = new ValueRange<short>(0, 0, 0)
        };

      return new DataTypeMapping(typeof(ulong), @ulong, DbType.Decimal,
        (reader, index) => Convert.ToUInt64(reader.GetValue(index)));
    }

    protected virtual DataTypeMapping CreateInt64Mapping(DataTypeInfo type)
    {
      return new DataTypeMapping(typeof(long), type, DbType.Int64, (reader, index) => reader.GetInt64(index));
    }

    protected virtual DataTypeMapping CreateUInt32Mapping(DataTypeInfo type)
    {
      var @int64 = DomainHandler.Driver.ServerInfo.DataTypes.Int64;
      var @uint = new IntegerDataTypeInfo<uint>(@int64.SqlType, null)
        {
          Value = new ValueRange<uint>(uint.MinValue, uint.MaxValue)
        };

      return new DataTypeMapping(typeof(uint), @uint, DbType.Int64,
        (reader, index) => Convert.ToUInt32(reader.GetValue(index)));
    }

    protected virtual DataTypeMapping CreateInt32Mapping(DataTypeInfo type)
    {
      return new DataTypeMapping(typeof(int), type, DbType.Int32, (reader, index) => reader.GetInt32(index));
    }

    protected virtual DataTypeMapping CreateUInt16Mapping(DataTypeInfo type)
    {
      var @int32 = DomainHandler.Driver.ServerInfo.DataTypes.Int32;
      var @ushort = new IntegerDataTypeInfo<ushort>(@int32.SqlType, null)
        {
          Value = new ValueRange<ushort>(ushort.MinValue, ushort.MaxValue)
        };
      return new DataTypeMapping(typeof(ushort), @ushort, DbType.Int32,
        (reader, index) => Convert.ToUInt16(reader.GetValue(index)));
    }

    protected virtual DataTypeMapping CreateInt16Mapping(DataTypeInfo type)
    {
      return new DataTypeMapping(typeof(short), type, DbType.Int16, (reader, index) => reader.GetInt16(index));
    }

    protected virtual DataTypeMapping CreateSByteMapping(DataTypeInfo type)
    {
      var @int16 = DomainHandler.Driver.ServerInfo.DataTypes.Int16;
      var @sbyte = new IntegerDataTypeInfo<sbyte>(@int16.SqlType, null)
        {
          Value = new ValueRange<sbyte>(sbyte.MinValue, sbyte.MaxValue)
        };
      return new DataTypeMapping(typeof(sbyte), @sbyte, DbType.Int16,
        (reader, index) => Convert.ToSByte(reader.GetValue(index)));
    }

    protected virtual DataTypeMapping CreateByteMapping(DataTypeInfo type)
    {
      return new DataTypeMapping(typeof(byte), type, DbType.Byte, (reader, index) => reader.GetByte(index));
    }

    protected virtual DataTypeMapping CreateBooleanMapping(DataTypeInfo type)
    {
      return new DataTypeMapping(typeof(bool), type, DbType.Boolean, (reader, index) => reader.GetBoolean(index));
    }

    protected virtual DataTypeMapping CreateCharMapping(DataTypeInfo type)
    {
      var @char = new RangeDataTypeInfo<char>(SqlDataType.Char, null)
        {
          Value = new ValueRange<char>(char.MinValue, char.MaxValue, (char) 0)
        };
      return new DataTypeMapping(typeof (char), @char, DbType.String, ReadChar, ToSqlChar);
    }

    #endregion

    /// <inheritdoc/>
    public override void Initialize()
    {
      DomainHandler = Handlers.DomainHandler as DomainHandler;
      MappingSchema = new DataTypeMappingSchema();
      BuildAllMappings();
    }
    
    private void BuildAllMappings()
    {
      DataTypeCollection types = DomainHandler.Driver.ServerInfo.DataTypes;
      MappingSchema.Register(CreateCharMapping(types.Char));
      MappingSchema.Register(CreateBooleanMapping(types.Boolean));
      MappingSchema.Register(CreateByteMapping(types.Byte));
      MappingSchema.Register(CreateSByteMapping(types.SByte));
      MappingSchema.Register(CreateInt16Mapping(types.Int16));
      MappingSchema.Register(CreateUInt16Mapping(types.UInt16));
      MappingSchema.Register(CreateInt32Mapping(types.Int32));
      MappingSchema.Register(CreateUInt32Mapping(types.UInt32));
      MappingSchema.Register(CreateInt64Mapping(types.Int64));
      MappingSchema.Register(CreateUInt64Mapping(types.UInt64));
      MappingSchema.Register(CreateFloatMapping(types.Float));
      MappingSchema.Register(CreateDoubleMapping(types.Double));
      MappingSchema.Register(CreateDecimalMapping(types.Decimal));
      MappingSchema.Register(CreateDateTimeMapping(types.DateTime));
      MappingSchema.Register(CreateTimeSpanMapping(types.Interval));
      MappingSchema.Register(CreateGuidMapping(types.Guid));
      MappingSchema.Register(CreateStringMapping(types.VarChar));
      MappingSchema.Register(CreateStringMapping(types.VarCharMax));
      MappingSchema.Register(CreateByteArrayMapping(types.VarBinary));
      MappingSchema.Register(CreateByteArrayMapping(types.VarBinaryMax));
    }

    private static object ReadChar(DbDataReader reader, int index)
    {
      return reader.GetString(index).SingleOrDefault();
    }

    private static object ToSqlChar(object value)
    {
      var _char = (char) value;
      return _char==default(char) ? string.Empty : _char.ToString();
    }
  }
}