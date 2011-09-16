// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.02

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Xtensive.Sql.Info;
using Xtensive.Sql;

namespace Xtensive.Sql.SqlServer.v09
{
  public class TypeMapper : Sql.TypeMapper
  {
    private ValueRange<DateTime> dateTimeRange;

    public override bool IsLiteralCastRequired(Type type)
    {
      switch (Type.GetTypeCode(type)) {
        case TypeCode.Byte:
        case TypeCode.SByte:
        case TypeCode.Int16:
        case TypeCode.UInt16:
        case TypeCode.Int64:
        case TypeCode.UInt64:
          return true;
      }
      if (type==typeof(TimeSpan))
        return true;
      if (type==typeof(Guid))
        return true;
      return false;
    }

    public override void SetSByteParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int16;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void SetUShortParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int32;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void SetUIntParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void SetULongParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void SetDateTimeParameterValue(DbParameter parameter, object value)
    {
      if (value!=null)
        value = ValueRangeValidator.Correct((DateTime) value, dateTimeRange);
      base.SetDateTimeParameterValue(parameter, value);
    }

    public override void SetTimeSpanParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      if (value!=null) {
        var timeSpan = (TimeSpan) value;
        parameter.Value = timeSpan.Ticks*100;
      }
      else
        parameter.Value = DBNull.Value;
    }

    public override SqlValueType BuildSByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }

    public override SqlValueType BuildUShortSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int32);
    }

    public override SqlValueType BuildUIntSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int64);
    }

    public override SqlValueType BuildULongSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 20, 0);
    }

    public override SqlValueType BuildTimeSpanSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int64);
    }

    public override object ReadDecimal(DbDataReader reader, int index)
    {
      var nativeReader = (SqlDataReader) reader;
      // TODO: quickfix -- rewrite
      try {
        var value = SqlDecimal.ConvertToPrecScale(
          nativeReader.GetSqlDecimal(index),
          MaxDecimalPrecision.Value - 2, MaxDecimalPrecision.Value / 3);
        return value.Value;
      }
      catch (SqlTruncateException e) {
        return nativeReader.GetSqlDecimal(index).Value;
      }
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