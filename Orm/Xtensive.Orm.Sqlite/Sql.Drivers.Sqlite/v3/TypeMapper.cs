// Copyright (C) 2011-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.Security.AccessControl;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Sqlite.v3
{
  internal class TypeMapper : Sql.TypeMapper
  {
#if NET6_0_OR_GREATER
    private ValueRange<DateOnly> dateOnlyRange;
    private ValueRange<TimeOnly> timeOnlyRange;
#endif
    private ValueRange<DateTime> dateTimeRange;
    private ValueRange<DateTimeOffset> dateTimeOffsetRange;

    private const string DateTimeOffsetFormat = "yyyy-MM-dd HH:mm:ss.fffK";
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fffffff";
    private const string DateFormat = "yyyy-MM-dd";
    private const string TimeFormat = "HH:mm:ss.fffffff";

    public override object ReadBoolean(DbDataReader reader, int index)
    {
      var value = reader.GetDecimal(index);
      return SQLiteConvert.ToBoolean(value);
    }
#if NET6_0_OR_GREATER

    public override object ReadDateOnly(DbDataReader reader, int index)
    {
      var value = reader.GetString(index);
      return DateOnly.ParseExact(value, DateFormat, CultureInfo.InvariantCulture);
    }

    public override object ReadTimeOnly(DbDataReader reader, int index)
    {
      var value = reader.GetString(index);
      return TimeOnly.ParseExact(value, TimeFormat, CultureInfo.InvariantCulture);
    }
#endif

    public override object ReadDateTimeOffset(DbDataReader reader, int index)
    {
      var value = reader.GetString(index);
      return DateTimeOffset.ParseExact(value, DateTimeOffsetFormat, CultureInfo.InvariantCulture);
    }

    public override void BindSByte(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int16;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void BindUShort(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int32;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void BindUInt(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void BindULong(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void BindDateTime(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      if (value == null) {
        parameter.Value = DBNull.Value;
        return;
      }
      var correctValue = ValueRangeValidator.Correct((DateTime) value, dateTimeRange);
      parameter.Value = correctValue.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
    }
#if NET6_0_OR_GREATER

    public override void BindDateOnly(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      if (value == null) {
        parameter.Value = DBNull.Value;
        return;
      }
      var correctValue = ValueRangeValidator.Correct((DateOnly) value, dateOnlyRange);
      parameter.Value = correctValue.ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    public override void BindTimeOnly(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      if (value == null) {
        parameter.Value = DBNull.Value;
        return;
      }
      var correctValue = ValueRangeValidator.Correct((TimeOnly) value, timeOnlyRange);
      parameter.Value = correctValue.ToString(TimeFormat, CultureInfo.InvariantCulture);
    }
#endif

    public override void BindDateTimeOffset(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      var correctValue = ValueRangeValidator.Correct((DateTimeOffset) value, dateTimeOffsetRange);
      parameter.Value = correctValue.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture);
    }

    public override SqlValueType MapDecimal(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal);
    }

    public override SqlValueType MapChar(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarCharMax);
    }

    public override SqlValueType MapString(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarCharMax);
    }

    public override SqlValueType MapByteArray(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarBinaryMax);
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
      return new SqlValueType(SqlType.Int64);
    }

    public override SqlValueType MapDateTimeOffset(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.DateTimeOffset);
    }

    public override void Initialize()
    {
      base.Initialize();
      dateTimeRange = (ValueRange<DateTime>) Driver.ServerInfo.DataTypes.DateTime.ValueRange;
      dateTimeOffsetRange = (ValueRange<DateTimeOffset>) Driver.ServerInfo.DataTypes.DateTimeOffset.ValueRange;
#if NET6_0_OR_GREATER
      dateOnlyRange = (ValueRange<DateOnly>) Driver.ServerInfo.DataTypes.DateOnly.ValueRange;
      timeOnlyRange = (ValueRange<TimeOnly>) Driver.ServerInfo.DataTypes.TimeOnly.ValueRange;
#endif
    }

    // Constructors

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
