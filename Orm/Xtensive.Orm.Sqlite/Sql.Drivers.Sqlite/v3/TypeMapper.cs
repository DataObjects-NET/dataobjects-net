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

namespace Xtensive.Sql.Drivers.SQLite.v3
{
  internal class TypeMapper : Sql.TypeMapper
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

//#if NET40
//    [SecuritySafeCritical]
//#endif
//    public override void SetStringParameterValue(DbParameter parameter, object value)
//    {
//      string text = value as string;
//      if (!string.IsNullOrEmpty(text) && text.Length > VarCharMaxLength) {
//        var sqlParameter = (SQLiteParameter) parameter;
//        sqlParameter.InitializeLifetimeService()` = SqlDbType.NText;
//        sqlParameter.Value = text;
//      }
//      else
//        base.SetStringParameterValue(parameter, value);
//    }

//#if NET40
//    [SecuritySafeCritical]
//#endif
//    public override void SetByteArrayParameterValue(DbParameter parameter, object value)
//    {
//      var array = value as byte[];
//      if (array != null && array.Length > VarBinaryMaxLength) {
//        var sqlParameter = (SQLiteParameter) parameter;
//        sqlParameter.SqlDbType = SqlDbType.Image;
//        sqlParameter.Value = array;
//      }
//      else
//        base.SetByteArrayParameterValue(parameter, value);
//    }

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
        parameter.Value = (long) timeSpan.Ticks*100;
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