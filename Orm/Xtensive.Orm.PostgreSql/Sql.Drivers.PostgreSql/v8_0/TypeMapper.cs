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
using Xtensive.Orm.PostgreSql;
using Xtensive.Reflection.PostgreSql;


namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class TypeMapper : Sql.TypeMapper
  {
    protected readonly bool legacyTimestampBehaviorEnabled;

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
          ? (object) CreateNativeIntervalFromTimeSpan(timeSpanValue)
          : throw ValueNotOfTypeError(nameof(WellKnownTypes.TimeSpanType));
    }

    public override void BindGuid(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value == null ? (object) DBNull.Value : SqlHelper.GuidToString((Guid) value);
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
        // so now we have to unbox-box value to change kind of value because of this "talanted" person.
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
        // Npgsql team "untaught" the library to do it.
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

      // support for full-range of Timespans required us to use raw type
      // and construct timespan from its' values.
      return ResurrectTimeSpanFromNpgsqlInterval(nativeInterval);
    }

    [SecuritySafeCritical]
    public override object ReadDecimal(DbDataReader reader, int index)
    {
      var nativeReader = (NpgsqlDataReader) reader;
      return nativeReader.GetDecimal(index);
    }

    [SecuritySafeCritical]
    public override object ReadDateTimeOffset(DbDataReader reader, int index)
    {
      var nativeReader = (NpgsqlDataReader) reader;
      var value = nativeReader.GetFieldValue<DateTimeOffset>(index);
      if (legacyTimestampBehaviorEnabled) {
        // Npgsql 4 or older behavior
        return value;
      }
      else {
        // Probably the "mastermind" who made parameter conversion before setting value to parameter be required
        // also forgot about PostgreSQL's built-in "SET TIME ZONE" feature for session, which affects values of TimeStampTz
        // Now applications have to use either local/utc timezone everywhere OR somehow "remember" what they've set in SET TIME ZONE
        // for being able to get values in the timezone they've set. (facapalm)
        //
        // BTW, Npgsql has no API that would provide us current connection timezone so we could apply it to values,
        // there is internal setting in NpgsqlConnection but it is null no matter what is set by
        // 'SET TIME ZONE' statement :-)
        //
        // We'll use local time, that's it! SET TIME ZONE will not work!
        if (value == DateTimeOffset.MinValue || value == DateTimeOffset.MaxValue)
          return value;
        return value.ToLocalTime();
      }
    }

    protected internal static NpgsqlInterval CreateNativeIntervalFromTimeSpan(in TimeSpan timeSpan)
    {
      // Previous Npgsql versions used days and time, no months.
      // Thought we can write everything as time, we keep days and time format

      var ticks = timeSpan.Ticks;

      var days = timeSpan.Days;
      var timeTicks = ticks - (days * TimeSpan.TicksPerDay);
#if NET7_0_OR_GREATER
      var microseconds = timeTicks / TimeSpan.TicksPerMicrosecond;
#else
      var microseconds = timeTicks / 10L; // same as TimeSpan.TicksPerMicrosecond available in .NET7+
#endif
      // no months!
      return new NpgsqlInterval(0, days, microseconds);
    }

    protected internal static TimeSpan ResurrectTimeSpanFromNpgsqlInterval(in NpgsqlInterval npgsqlInterval)
    {
      // We don't write "Months" part of NpgsqlInterval to database
      // because days in months is variable measure in PostgreSQL.
      // We better use exact number of days.
      // But if for some reason, there is Months value > 0 we treat it like each month has 30 days,
      // it seems that Npgsql did the same assumption internally.

      var days = (npgsqlInterval.Months != 0)
        ? npgsqlInterval.Months * WellKnown.IntervalDaysInMonth + npgsqlInterval.Days
        : npgsqlInterval.Days;

      var ticksOfDays = days * TimeSpan.TicksPerDay;
#if NET7_0_OR_GREATER
      var overallTicks = ticksOfDays + (npgsqlInterval.Time * TimeSpan.TicksPerMicrosecond);
#else
      var overallTicks = ticksOfDays + (npgsqlInterval.Time * 10); //same as TimeSpan.TicksPerMicrosecond available in .NET7+
#endif
      return TimeSpan.FromTicks(overallTicks);
    }

    internal protected ArgumentException ValueNotOfTypeError(string typeName)
    {
      return new ArgumentException($"Value is not of '{typeName}' type.");
    }

    // Constructors

    public TypeMapper(PostgreSql.Driver driver)
      : base(driver)
    {
      legacyTimestampBehaviorEnabled = driver.PostgreServerInfo.LegacyTimestampBehavior;
    }
  }
}