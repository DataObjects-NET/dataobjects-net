// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Security;
using Xtensive.Sql.Info;
using Xtensive.Sql;

namespace Xtensive.Sql.Drivers.Sqlite.v3
{
  internal class TypeMapper : Sql.TypeMapper
  {
    private const int BooleanPrecision = 1;
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

    public override object ReadBoolean(DbDataReader reader, int index)
    {
      var value = reader.GetDecimal(index);
      return SQLiteConvert.ToBoolean(value);
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

    public override SqlValueType BuildCharSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarCharMax);
    }

    public override SqlValueType BuildStringSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarCharMax);
    }

    public override SqlValueType BuildByteArraySqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarBinaryMax);
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
      return new SqlValueType(SqlType.Int64);
    }

    public override SqlValueType BuildTimeSpanSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int64);
    }

    public override object ReadTimeSpan(DbDataReader reader, int index)
    {
      long value;
      try {
        value = reader.GetInt64(index);
      }
      catch (InvalidCastException) {
        value = (long) reader.GetDecimal(index);
      }
      return TimeSpan.FromTicks(value / 100);
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