// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Sql.Common;
using Xtensive.Storage.Providers.Sql.Mappings;

namespace Xtensive.Storage.Providers.PgSql
{
  public sealed class SqlValueTypeMapper : Sql.SqlValueTypeMapper
  {
    protected override void BuildTypeSubstitutes()
    {
      base.BuildTypeSubstitutes();

      var @int16 = DomainHandler.SqlDriver.ServerInfo.DataTypes.Int16;

      var @byte = new IntegerDataTypeInfo<byte>(@int16.SqlType, null);
      @byte.Value = new ValueRange<byte>(byte.MinValue, byte.MaxValue);
      BuildDataTypeMapping(@byte);

      var @sbyte = new IntegerDataTypeInfo<sbyte>(@int16.SqlType, null);
      @sbyte.Value = new ValueRange<sbyte>(sbyte.MinValue, sbyte.MaxValue);
      BuildDataTypeMapping(@sbyte);

      var @int32 = DomainHandler.SqlDriver.ServerInfo.DataTypes.Int32;
      var @ushort = new IntegerDataTypeInfo<ushort>(@int32.SqlType, null);
      @ushort.Value = new ValueRange<ushort>(ushort.MinValue, ushort.MaxValue);
      BuildDataTypeMapping(@ushort);

      var @int64 = DomainHandler.SqlDriver.ServerInfo.DataTypes.Int64;
      var @uint = new IntegerDataTypeInfo<uint>(@int64.SqlType, null);
      @uint.Value = new ValueRange<uint>(uint.MinValue, uint.MaxValue);
      BuildDataTypeMapping(@uint);

      var @decimal = DomainHandler.SqlDriver.ServerInfo.DataTypes.Decimal;
      var @ulong = new IntegerDataTypeInfo<ulong>(@decimal.SqlType, null);
      @ulong.Value = new ValueRange<ulong>(ulong.MinValue, ulong.MaxValue);
      BuildDataTypeMapping(@ulong);

      var @binary = DomainHandler.SqlDriver.ServerInfo.DataTypes.VarBinaryMax;
      var @guid = new StreamDataTypeInfo(@binary.SqlType, typeof(Guid), null);
      @guid.Length = new ValueRange<int>(16, 16, 16);
      BuildDataTypeMapping(@guid);

      var @timespan = new RangeDataTypeInfo<TimeSpan>(@int64.SqlType, null);
      @timespan.Value = new ValueRange<TimeSpan>(TimeSpan.FromTicks(@int64.Value.MinValue), TimeSpan.FromTicks(@int64.Value.MaxValue));
      BuildDataTypeMapping(@timespan);

    }

    protected override DataTypeMapping CreateDataTypeMapping(DataTypeInfo dataTypeInfo)
    {
      DataTypeMapping result;
      Type type = dataTypeInfo.Type;
      TypeCode typeCode = Type.GetTypeCode(type);
      var dataReaderAccessor = BuildDataReaderAccessor(dataTypeInfo);
      switch (typeCode) {
      case TypeCode.Object: {
        if (type==typeof(Guid))
          result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Binary, v => ((Guid)v).ToByteArray(), v => new Guid((byte[])v));
        else if (type==typeof (TimeSpan))
          result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Int64, value => ((TimeSpan) value).Ticks, value => TimeSpan.FromTicks((long) value));
        else
          result = base.CreateDataTypeMapping(dataTypeInfo);
        break;
      }
      case TypeCode.Byte:
        result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Int16);
        break;
      case TypeCode.SByte:
        result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Int16);
        break;
      case TypeCode.UInt16:
        result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Int32);
        break;
      case TypeCode.UInt32:
        result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Int64);
        break;
      case TypeCode.UInt64:
        result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Decimal);
        break;
      default:
        result = base.CreateDataTypeMapping(dataTypeInfo);
        break;
      }
      return result;
    }

    /// <inheritdoc/>
    protected override Func<DbDataReader, int, object> BuildDataReaderAccessor(DataTypeInfo dataTypeInfo)
    {
      Type type = dataTypeInfo.Type;
      TypeCode typeCode = Type.GetTypeCode(type);
      switch (typeCode) {
      case TypeCode.Object:
        if (type == typeof(Guid))
          return (reader, fieldIndex) => {
            byte[] result = new byte[16];
            reader.GetBytes(fieldIndex, 0, result, 0, 16);
            return result;
          };
        if (type == typeof(TimeSpan))
          return (reader, fieldIndex) => reader.GetInt64(fieldIndex);
        return base.BuildDataReaderAccessor(dataTypeInfo);
      case TypeCode.Boolean:
        return (reader, fieldIndex) => reader.GetBoolean(fieldIndex);
      case TypeCode.Char:
        return (reader, fieldIndex) => reader.GetChar(fieldIndex);
      case TypeCode.SByte:
        return (reader, fieldIndex) => Convert.ToSByte(reader.GetInt16(fieldIndex));
      case TypeCode.Byte:
        return (reader, fieldIndex) => Convert.ToByte(reader.GetInt16(fieldIndex));
      case TypeCode.UInt16:
        return (reader, fieldIndex) => Convert.ToUInt16(reader.GetInt32(fieldIndex));
      case TypeCode.UInt32:
        return (reader, fieldIndex) => Convert.ToUInt32(reader.GetInt64(fieldIndex));
      case TypeCode.UInt64:
        return (reader, fieldIndex) => Convert.ToUInt64(reader.GetDecimal(fieldIndex));
      default:
        return base.BuildDataReaderAccessor(dataTypeInfo);
      }
    }
  }
}