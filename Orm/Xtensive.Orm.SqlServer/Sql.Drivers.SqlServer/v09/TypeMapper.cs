// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.02

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServer.v09
{
  internal class TypeMapper : Sql.TypeMapper
  {
    private static readonly SqlDecimal MinDecimal = new SqlDecimal(decimal.MinValue);
    private static readonly SqlDecimal MaxDecimal = new SqlDecimal(decimal.MaxValue);
    private static readonly int NVarCharLength = 4000;
    private static readonly int NVarCharMaxLength = int.MaxValue;

    private ValueRange<DateTime> dateTimeRange;

    public override bool IsParameterCastRequired(Type type)
    {
      switch (Type.GetTypeCode(type)) {
        case TypeCode.Byte:
        case TypeCode.SByte:
        case TypeCode.Int16:
        case TypeCode.UInt16:
          return true;
      }
      if (type==typeof (Guid)) {
        return true;
      }

      return base.IsParameterCastRequired(type);
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
      if (value!=null)
        value = ValueRangeValidator.Correct((DateTime) value, dateTimeRange);
      base.BindDateTime(parameter, value);
    }

    public override void BindString(DbParameter parameter, object value)
    {
      base.BindString(parameter, value);
      var stringValue = value as string;
      if (stringValue==null)
        return;
      parameter.Size = stringValue.Length <= NVarCharLength
        ? NVarCharLength
        : NVarCharMaxLength;
    }
#if NET6_0_OR_GREATER

    public override void BindDateOnly(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Date;
      parameter.Value = value != null ? (DateOnly) value : DBNull.Value;
    }

    public override void BindTimeOnly(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Time;
      parameter.Value = value != null ? (TimeOnly) value : DBNull.Value;
    }
#endif

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

    public override object ReadDecimal(DbDataReader reader, int index)
    {
      var nativeReader = (SqlDataReader) reader;
      var sqlDecimal = nativeReader.GetSqlDecimal(index);
      if (ShouldCompareValues(sqlDecimal)) {
        if (sqlDecimal > MaxDecimal)
          return decimal.MaxValue;
        if (sqlDecimal < MinDecimal)
          return decimal.MinValue;
      }
      return InternalHelpers.TruncateToNetDecimal(sqlDecimal);
    }

    public override object ReadDateTimeOffset(DbDataReader reader, int index)
    {
      return ((SqlDataReader) reader).GetDateTimeOffset(index);
    }
#if NET6_0_OR_GREATER

    public override object ReadDateOnly(DbDataReader reader, int index) =>
      reader.GetFieldValue<DateOnly>(index);

    public override object ReadTimeOnly(DbDataReader reader, int index) =>
      reader.GetFieldValue<TimeOnly>(index);
#endif

    public override void Initialize()
    {
      base.Initialize();
      dateTimeRange = (ValueRange<DateTime>) Driver.ServerInfo.DataTypes.DateTime.ValueRange;
    }

    private bool TryConvert(SqlDecimal sqlDecimal, out decimal result)
    {
      var data = sqlDecimal.Data;
      if (data[3]==0 && sqlDecimal.Scale <= 28) {
        result = new decimal(data[0], data[1], data[2], !sqlDecimal.IsPositive, sqlDecimal.Scale);
        return true;
      }
      result = decimal.Zero;
      return false;
    }

    private SqlDecimal ReducePrecision(SqlDecimal d, int newPrecision)
    {
      var newScale = newPrecision - d.Precision + d.Scale;
      var truncated = SqlDecimal.Truncate(d, newScale);
      return SqlDecimal.ConvertToPrecScale(truncated, newPrecision, newScale);
    }

    private bool ShouldCompareValues(SqlDecimal valueFromDatabase)
    {
      var floorDigitCount = valueFromDatabase.Precision - valueFromDatabase.Scale;
      if (floorDigitCount >= 29) //29 is max count of floor in .net Decimal
        return true;
      return false;
    }

    // Constructors

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}