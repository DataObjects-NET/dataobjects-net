// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Data;
using System.Data.Common;
using System.Security;
using Npgsql;
using NpgsqlTypes;
using Xtensive.Reflection.PostgreSql;


namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class TypeMapper : Sql.TypeMapper
  {
    // 6 fractions instead of .NET's 7
    private const long DateTimeMaxValueAdjustedTicks = 3155378975999999990;

    // 6 fractions instead of .NET's 7
    private const long TimeSpanMinValueAdjustedTicks = -9223372036854775800;
    private const long TimeSpanMaxValueAdjustedTicks = 9223372036854775800;

    protected readonly bool legacyTimestampBehaviorEnabled;
    protected readonly TimeZoneInfo defaultTimeZone;

    public override bool IsParameterCastRequired(Type type)
    {
      switch (Type.GetTypeCode(type)) {
      case TypeCode.Byte:
      case TypeCode.SByte:
      case TypeCode.Int16:
      case TypeCode.UInt16:
      case TypeCode.Single:
      case TypeCode.Double:
      case TypeCode.DateTime:
        return true;
      }
      if (type == WellKnownTypes.DateTimeOffsetType) {
        return true;
      }
      if (type == WellKnownTypes.GuidType) {
        return true;
      }
      if (type == WellKnownTypes.TimeSpanType) {
        return true;
      }
      if (type == WellKnownTypes.ByteArrayType) {
        return true;
      }
      return false;
    }

    public override void BindByte(DbParameter parameter, object value)
    {
      if(value == null) {
        base.BindByte(parameter, value);
      }
      else {
        base.BindByte(parameter, Convert.ToByte(value));
      }
    }

    public override void BindShort(DbParameter parameter, object value)
    {
      if (value == null) {
        base.BindShort(parameter, value);
      }
      else {
        base.BindShort(parameter, Convert.ToInt16(value));
      }
    }

    public override void BindInt(DbParameter parameter, object value)
    {
      if (value == null) {
        base.BindInt(parameter, value);
      }
      else {
        base.BindInt(parameter, Convert.ToInt32(value));
      }
    }

    public override void BindLong(DbParameter parameter, object value)
    {
      if (value == null) {
        base.BindLong(parameter, value);
      }
      else {
        base.BindLong(parameter, Convert.ToInt64(value));
      }
    }

    public override void BindSByte(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int16;
      parameter.Value = value == null ? DBNull.Value : (object) Convert.ToInt16(value);
    }

    public override void BindUShort(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int32;
      parameter.Value = value == null ? DBNull.Value : (object) Convert.ToInt32(value);
    }
    
    public override void BindUInt(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      parameter.Value = value == null ? DBNull.Value : (object) Convert.ToInt64(value);
    }

    public override void BindULong(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value == null ? DBNull.Value : (object) Convert.ToDecimal(value);
    }

    [SecuritySafeCritical]
    public override void BindTimeSpan(DbParameter parameter, object value)
    {
      var nativeParameter = (NpgsqlParameter) parameter;
      nativeParameter.NpgsqlDbType = NpgsqlDbType.Interval;
      nativeParameter.NpgsqlValue = value is null
        ? DBNull.Value
        : value is TimeSpan timeSpanValue
          ? (object) PostgreSqlHelper.CreateNativeIntervalFromTimeSpan(timeSpanValue)
          : throw ValueNotOfTypeError(nameof(WellKnownTypes.TimeSpanType));
    }

    public override void BindGuid(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value == null ? (object) DBNull.Value : SqlHelper.GuidToString((Guid) value);
    }

    [SecuritySafeCritical]
    public override void BindDateOnly(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Date;
      parameter.Value = value != null ? (DateOnly) value : DBNull.Value;
    }

    [SecuritySafeCritical]
    public override void BindDateTime(DbParameter parameter, object value)
    {
      if (legacyTimestampBehaviorEnabled) {
        base.BindDateTime(parameter, value);
      }
      else {
        var nativeParameter = (NpgsqlParameter) parameter;
        // For some reason Npgsql team mapped DbType.DateTime to timestamp WITH timezone
        // (which suppose to be pair to DateTimeOffset) and DbType.DateTime2 to timestamp WITHOUT timezone
        // in Npgsql 6+, though both types have the same range of values and resolution.
        //
        // If no explicit type declared it seems to be identified by DateTime value's Kind,
        // so now we have to unbox-box value to change kind of value.
        nativeParameter.NpgsqlDbType = NpgsqlDbType.Timestamp;
        nativeParameter.Value = value is null
          ? DBNull.Value
          : value is DateTime dtValue
            ? (object) DateTime.SpecifyKind(dtValue, DateTimeKind.Unspecified)
            : throw ValueNotOfTypeError(nameof(WellKnownTypes.DateTimeType));
      }
    }

    [SecuritySafeCritical]
    public override void BindDateTimeOffset(DbParameter parameter, object value)
    {
      var nativeParameter = (NpgsqlParameter) parameter;
      if (legacyTimestampBehaviorEnabled) {
        nativeParameter.NpgsqlDbType = NpgsqlDbType.TimestampTz;
        nativeParameter.NpgsqlValue = value ?? DBNull.Value;
      }
      else {
        nativeParameter.NpgsqlDbType = NpgsqlDbType.TimestampTz;

        // Manual switch to universal time is required by Npgsql from now on,
        nativeParameter.NpgsqlValue = value is null
          ? DBNull.Value
          : value is DateTimeOffset dateTimeOffset
            ? (object) dateTimeOffset.ToUniversalTime()
            : throw ValueNotOfTypeError(nameof(WellKnownTypes.DateTimeOffsetType));
      }
    }

    public override SqlValueType MapByte(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }
 
    public override SqlValueType MapSByte(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }

    public override SqlValueType MapUShort(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int32);
    }

    public override SqlValueType MapUInt(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int64);
    }

    public override SqlValueType MapULong(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 20, 0);
    }

    public override SqlValueType MapGuid(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarChar, 32);
    }

    public override SqlValueType MapTimeSpan(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Interval);
    }

    public override object ReadByte(DbDataReader reader, int index)
    {
      return Convert.ToByte(reader[index]);
    }

    public override object ReadGuid(DbDataReader reader, int index)
    {
      return SqlHelper.GuidFromString(reader.GetString(index));
    }

    [SecuritySafeCritical]
    public override object ReadTimeSpan(DbDataReader reader, int index)
    {
      var nativeReader = (NpgsqlDataReader) reader;
      var nativeInterval = nativeReader.GetFieldValue<NpgsqlInterval>(index);

      // support for full-range of TimeSpans requires us to use raw type
      // and construct timespan from its' values.
      var result = PostgreSqlHelper.ResurrectTimeSpanFromNpgsqlInterval(nativeInterval);

      // for confinience of comparison in .NET we lose 7th fractional point and treat several
      // .Net values as one, Min or Max value.
      if (result == TimeSpan.MinValue || result.Ticks == TimeSpanMinValueAdjustedTicks)
        return TimeSpan.MinValue;
      if (result == TimeSpan.MaxValue || result.Ticks == TimeSpanMaxValueAdjustedTicks)
        return TimeSpan.MaxValue;
      return result;
    }

    [SecuritySafeCritical]
    public override object ReadDecimal(DbDataReader reader, int index)
    {
      var nativeReader = (NpgsqlDataReader) reader;
      return nativeReader.GetDecimal(index);
    }

    public override object ReadDateOnly(DbDataReader reader, int index)
    {
      return reader.GetFieldValue<DateOnly>(index);
    }

    public override object ReadDateTime(DbDataReader reader, int index)
    {
      var value = reader.GetDateTime(index);
      if (value == DateTime.MinValue || value == DateTime.MaxValue)
        return value;
      if (value.Ticks == DateTimeMaxValueAdjustedTicks) {
        // Applied when Infinity aliases are disabled.
        // To not ruin possible comparisons with defined value,
        // it is better to return definded value,
        // not the 6-digit version from PostgreSQL.
        return DateTime.MaxValue;
      }
      return value;
    }

    [SecuritySafeCritical]
    public override object ReadDateTimeOffset(DbDataReader reader, int index)
    {
      var nativeReader = (NpgsqlDataReader) reader;
      var value = nativeReader.GetFieldValue<DateTimeOffset>(index);
      if (value.Ticks == DateTimeMaxValueAdjustedTicks) {
        // Applied when Infinity aliases are disabled.
        // To not ruin possible comparisons with defined values,
        // it is better to return definded value,
        // not the 6-fractions version from PostgreSQL.
        return DateTimeOffset.MaxValue;
      }
      if (value == DateTimeOffset.MaxValue || value == DateTimeOffset.MaxValue)
        return value;

      if (legacyTimestampBehaviorEnabled) {
        // Npgsql 4 or older behavior
        return value;
      }
      else {
        // Here, we try to apply connection time zone (if it was recongized on client-side)
        // to the values we read.
        // If any time zone change happens, we assume that DomainConfiguration.ConnectionInitializationSql
        // is used to make such change and we cache time zone info on first connection in driver factory
        // after initialization has happened.
        // If connection time zone has not been recognized we transform value to local offset
        // on the assumption that database server is usually in the same region.
        return defaultTimeZone is not null
          ?  TimeZoneInfo.ConvertTime(value, defaultTimeZone)
          : (object) value.ToLocalTime();
      }
    }

    internal protected ArgumentException ValueNotOfTypeError(string typeName) =>
      new($"Value is not of '{typeName}' type.");


    // Constructors

    public TypeMapper(PostgreSql.Driver driver)
      : base(driver)
    {
      var postgreServerInfo = driver.PostgreServerInfo;
      legacyTimestampBehaviorEnabled = postgreServerInfo.LegacyTimestampBehavior;
      defaultTimeZone = postgreServerInfo.DefaultTimeZone;
    }
  }
}