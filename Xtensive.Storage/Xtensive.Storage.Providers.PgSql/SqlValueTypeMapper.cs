// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Sql.Common;
using Xtensive.Storage.Providers.Sql.Mappings;

namespace Xtensive.Storage.Providers.PgSql
{
  public sealed class SqlValueTypeMapper : Sql.SqlValueTypeMapper
  {
    /// <inheritdoc/>
    protected override void BuildTypeSubstitutes()
    {
      base.BuildTypeSubstitutes();

      var @int16 = DomainHandler.Driver.ServerInfo.DataTypes.Int16;

      var @byte = new IntegerDataTypeInfo<byte>(@int16.SqlType, null);
      @byte.Value = new ValueRange<byte>(byte.MinValue, byte.MaxValue);
      BuildDataTypeMapping(@byte);

      var @sbyte = new IntegerDataTypeInfo<sbyte>(@int16.SqlType, null);
      @sbyte.Value = new ValueRange<sbyte>(sbyte.MinValue, sbyte.MaxValue);
      BuildDataTypeMapping(@sbyte);

      var @int32 = DomainHandler.Driver.ServerInfo.DataTypes.Int32;
      var @ushort = new IntegerDataTypeInfo<ushort>(@int32.SqlType, null);
      @ushort.Value = new ValueRange<ushort>(ushort.MinValue, ushort.MaxValue);
      BuildDataTypeMapping(@ushort);

      var @int64 = DomainHandler.Driver.ServerInfo.DataTypes.Int64;
      var @uint = new IntegerDataTypeInfo<uint>(@int64.SqlType, null);
      @uint.Value = new ValueRange<uint>(uint.MinValue, uint.MaxValue);
      BuildDataTypeMapping(@uint);

      var @decimal = DomainHandler.Driver.ServerInfo.DataTypes.Decimal;
      var @ulong = new IntegerDataTypeInfo<ulong>(@decimal.SqlType, null);
      @ulong.Value = new ValueRange<ulong>(ulong.MinValue, ulong.MaxValue);
      BuildDataTypeMapping(@ulong);

      var @binary = DomainHandler.Driver.ServerInfo.DataTypes.VarBinaryMax;
      var @guid = new StreamDataTypeInfo(@binary.SqlType, typeof(Guid), null);
      @guid.Length = new ValueRange<int>(16, 16, 16);
      BuildDataTypeMapping(@guid);

      var @timespan = DomainHandler.Driver.ServerInfo.DataTypes.Interval;
      BuildDataTypeMapping(@timespan);
    }

    protected override DataTypeMapping CreateDataTypeMapping(DataTypeInfo dataTypeInfo)
    {
      if (dataTypeInfo.Type==typeof(Guid))
        return new DataTypeMapping(dataTypeInfo, BuildDataReaderAccessor(dataTypeInfo), DbType.Binary,
          v => ((Guid)v).ToByteArray(), v => new Guid((byte[])v));

      if (dataTypeInfo.Type==typeof (TimeSpan))
        return new DataTypeMapping(dataTypeInfo, BuildDataReaderAccessor(dataTypeInfo), DbType.String,
          v => TimeSpanToString((TimeSpan)v), v => ReadTimeSpan(v));

      return base.CreateDataTypeMapping(dataTypeInfo);
    }

    protected override DbType GetDbType(DataTypeInfo dataTypeInfo)
    {
      Type type = dataTypeInfo.Type;
      TypeCode typeCode = Type.GetTypeCode(type);
      switch (typeCode) {
      case TypeCode.Byte:
        return DbType.Int16;
      case TypeCode.SByte:
        return DbType.Int16;
      case TypeCode.UInt16:
        return DbType.Int32;
      case TypeCode.UInt32:
        return DbType.Int64;
      case TypeCode.UInt64:
        return DbType.Decimal;
      default:
        return base.GetDbType(dataTypeInfo);
      }
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
          return (reader, fieldIndex) => ReadTimeSpan(reader[fieldIndex]);
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

    private static TimeSpan ReadTimeSpan(object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");

      switch (Type.GetTypeCode(value.GetType())) {
        case TypeCode.String:
          return StringToTimeSpan((string)value);
        case TypeCode.Byte:
        case TypeCode.SByte:
        case TypeCode.Int16:
        case TypeCode.Int32:
        case TypeCode.Int64:
        case TypeCode.UInt16:
        case TypeCode.UInt32:
        case TypeCode.UInt64:
          return new TimeSpan(Convert.ToInt64(value));
      }

      if (value is TimeSpan)
        return (TimeSpan) value;
      throw new NotSupportedException();
    }

    internal static TimeSpan StringToTimeSpan(string input)
    {
      int days = 0;
      int hours = 0;
      int minutes = 0;
      int seconds = 0;
      int milliseconds = 0;

      //pattern: [[-]DD* day[s]] [[-]H[H]:M[M]:S[S][.ff*]]
      bool mainPartNegative;
      string[] parts = input.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
      switch (parts.Length) {
        case 1:
          // no day part
          mainPartNegative = ParseMainIntervalPart(parts[0], out hours, out minutes, out seconds, out milliseconds);
          break;
        case 2:
          // only day part: "x days"
          mainPartNegative = false;
          days = int.Parse(parts[0]);
          break;
        case 3:
          // both day and HMS parts: "x days y:z:v.ww"
          days = int.Parse(parts[0]);
          mainPartNegative = ParseMainIntervalPart(parts[2], out hours, out minutes, out seconds, out milliseconds);
          break;
        default:
          throw new InvalidOperationException();
      }

      if (mainPartNegative || days < 0) {
        hours = -hours;
        minutes = -minutes;
        seconds = -seconds;
        milliseconds = -milliseconds;
        if (days >= 0)
          days = -days;
      }

      return new TimeSpan(days, hours, minutes, seconds, milliseconds);
    }

    private static bool ParseMainIntervalPart(string value,
      out int hours,
      out int minutes,
      out int seconds,
      out int millisececonds)
    {
      hours = minutes = seconds = millisececonds = 0;
      bool negativeResult;
      value = value.TryCutPrefix("-", out negativeResult);
      string[] parts = value.Split(':', '.');
      if (parts.Length == 0)
        return false;

      hours = int.Parse(parts[0]);
      if (parts.Length > 1) {
        minutes = int.Parse(parts[1]);
        if (parts.Length > 2) {
          seconds = int.Parse(parts[2]);
          if (parts.Length > 3)
            if (parts[3].Length > 4) {
              millisececonds = int.Parse(parts[3].Substring(0, 4));
              if (millisececonds % 10 > 4)
                millisececonds += 10;
              millisececonds /= 10;
            }
            else
              millisececonds = int.Parse(parts[3]);
        }
      }
      
      return negativeResult;
    }

    internal static string TimeSpanToString(TimeSpan value)
    {
      int days = value.Days;
      int hours = value.Hours;
      int minutes = value.Minutes;
      int seconds = value.Seconds;
      int milliseconds = value.Milliseconds;

      bool negative = hours < 0 || minutes < 0 || seconds < 0 || milliseconds < 0;

      if (hours < 0)
        hours = -hours;

      if (minutes < 0)
        minutes = -minutes;

      if (seconds < 0)
        seconds = -seconds;

      if (milliseconds < 0)
        milliseconds = -milliseconds;

      return String.Format("{0} days {1}{2}:{3}:{4}.{5:000}",
          days, negative ? "-" : "", hours, minutes, seconds, milliseconds);
    }
  }
}
