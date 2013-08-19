// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.02

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Security;
using Xtensive.Sql.Info;
using Xtensive.Sql;

namespace Xtensive.Sql.Drivers.SqlServerCe.v3_5
{
  internal class TypeMapper : Sql.TypeMapper
  {
    private ValueRange<DateTime> dateTimeRange;

    [SecuritySafeCritical]
    public override void BindString(DbParameter parameter, object value)
    {
      string text = value as string;
      if (!string.IsNullOrEmpty(text) && text.Length > VarCharMaxLength) {
        var sqlParameter = (SqlCeParameter) parameter;
        sqlParameter.SqlDbType = SqlDbType.NText;
        sqlParameter.Value = text;
      }
      else
        base.BindString(parameter, value);
    }

    [SecuritySafeCritical]
    public override void BindByteArray(DbParameter parameter, object value)
    {
      var array = value as byte[];
      if (array != null && array.Length > VarBinaryMaxLength) {
        var sqlParameter = (SqlCeParameter) parameter;
        sqlParameter.SqlDbType = SqlDbType.Image;
        sqlParameter.Value = array;
      }
      else
        base.BindByteArray(parameter, value);
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

    public override void BindTimeSpan(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      if (value!=null) {
        var timeSpan = (TimeSpan) value;
        parameter.Value = (long) timeSpan.Ticks*100;
      }
      else
        parameter.Value = DBNull.Value;
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

    public override SqlValueType MapTimeSpan(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int64);
    }

    public override object ReadTimeSpan(DbDataReader reader, int index)
    {
      long value = 0L;
      try {
        value = reader.GetInt64(index);
      }
      catch (InvalidCastException) {
        value = (long) reader.GetDecimal(index);
      }
      return TimeSpan.FromTicks(value/100);
    }

    public override void Initialize()
    {
      base.Initialize();
      dateTimeRange = (ValueRange<DateTime>) Driver.ServerInfo.DataTypes.DateTime.ValueRange;
    }


    // Constructors

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}