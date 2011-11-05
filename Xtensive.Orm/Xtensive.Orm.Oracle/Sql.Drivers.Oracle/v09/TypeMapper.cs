// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Data;
using System.Data.Common;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Xtensive.Sql.Drivers.Oracle.v09
{
  internal class TypeMapper : Sql.TypeMapper
  {
    private const int BooleanPrecision = 1;
    private const int BytePrecision = 3;
    private const int ShortPrecision = 5;
    private const int IntPrecision = 10;
    private const int LongPrecision = 20;

    public override void SetBooleanParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : ((bool) value ? 1.0m : 0.0m);
    }

    public override void SetByteParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (byte) value;
    }

    public override void SetSByteParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (sbyte) value;
    }

    public override void SetShortParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (short) value;
    }

    public override void SetUShortParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (ushort) value;
    }

    public override void SetIntParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (int) value;
    }

    public override void SetUIntParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (uint) value;
    }

    public override void SetLongParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (long) value;
    }

    public override void SetULongParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (ulong) value;
    }

    public override void SetFloatParameterValue(DbParameter parameter, object value)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.Single;
      nativeParameter.Value = value ?? DBNull.Value;
    }

    public override void SetDoubleParameterValue(DbParameter parameter, object value)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.Double;
      nativeParameter.Value = value ?? DBNull.Value;
    }

    public override void SetTimeSpanParameterValue(DbParameter parameter, object value)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.IntervalDS;
      nativeParameter.Value = value==null ? (object) DBNull.Value : new OracleIntervalDS((TimeSpan) value);
    }

    public override void SetByteArrayParameterValue(DbParameter parameter, object value)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.Blob;
      nativeParameter.Value = value ?? DBNull.Value;
    }

    public override void SetGuidParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value==null ? (object) DBNull.Value : SqlHelper.GuidToString((Guid) value);
    }

    public override void SetStringParameterValue(DbParameter parameter, object value)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.NVarchar2;
      nativeParameter.Value = value ?? DBNull.Value;
    }

    public override object ReadBoolean(DbDataReader reader, int index)
    {
      //return reader.GetDecimal(index)!=0.0m;
      return Math.Abs(ReadDecimalSafely(reader, index, BooleanPrecision, 0)) > 0.3m;
    }

    public override object ReadByte(DbDataReader reader, int index)
    {
      //return (byte) reader.GetDecimal(index);
      return (byte) ReadDecimalSafely(reader, index, BytePrecision, 0);
    }

    public override object ReadSByte(DbDataReader reader, int index)
    {
      //return (sbyte) reader.GetDecimal(index);
      return (sbyte) ReadDecimalSafely(reader, index, BytePrecision, 0);
    }

    public override object ReadShort(DbDataReader reader, int index)
    {
      //return (short) reader.GetDecimal(index);
      return (short) ReadDecimalSafely(reader, index, ShortPrecision, 0);
    }

    public override object ReadUShort(DbDataReader reader, int index)
    {
      //return (ushort) reader.GetDecimal(index);
      return (ushort) ReadDecimalSafely(reader, index, ShortPrecision, 0);
    }

    public override object ReadInt(DbDataReader reader, int index)
    {
      //return (int) reader.GetDecimal(index);
      return (int) ReadDecimalSafely(reader, index, IntPrecision, 0);
    }

    public override object ReadUInt(DbDataReader reader, int index)
    {
      //return (uint) reader.GetDecimal(index);
      return (uint) ReadDecimalSafely(reader, index, IntPrecision, 0);
    }

    public override object ReadLong(DbDataReader reader, int index)
    {
      //return (long) reader.GetDecimal(index);
      return (long) ReadDecimalSafely(reader, index, LongPrecision, 0);
    }

    public override object ReadULong(DbDataReader reader, int index)
    {
      //return (ulong) reader.GetDecimal(index);
      return (ulong) ReadDecimalSafely(reader, index, LongPrecision, 0);
    }

    public override object ReadDecimal(DbDataReader reader, int index)
    {
      return ReadDecimalSafely(reader, index, MaxDecimalPrecision.Value, MaxDecimalPrecision.Value / 2);
    }

    public override object ReadFloat(DbDataReader reader, int index)
    {
      return Convert.ToSingle(reader[index]);
    }

    public override object ReadDouble(DbDataReader reader, int index)
    {
      return Convert.ToDouble(reader[index]);
    }

    public override object ReadTimeSpan(DbDataReader reader, int index)
    {
      var nativeReader = (OracleDataReader) reader;
      return (TimeSpan) nativeReader.GetOracleIntervalDS(index);
    }
    
    public override object ReadGuid(DbDataReader reader, int index)
    {
      return SqlHelper.GuidFromString(reader.GetString(index));
    }

    private static decimal ReadDecimalSafely(DbDataReader reader, int index, int newPrecision, int newScale)
    {
      var nativeReader = (OracleDataReader) reader;
      var result = OracleDecimal.ConvertToPrecScale(nativeReader.GetOracleDecimal(index), newPrecision, newScale);
      return result.Value;
    }

    public override SqlValueType BuildBooleanSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, BooleanPrecision, 0);
    }

    public override SqlValueType BuildByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, BytePrecision, 0);
    }

    public override SqlValueType BuildSByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, BytePrecision, 0);
    }

    public override SqlValueType BuildShortSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, ShortPrecision, 0);
    }

    public override SqlValueType BuildUShortSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, ShortPrecision, 0);
    }

    public override SqlValueType BuildIntSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, IntPrecision, 0);
    }

    public override SqlValueType BuildUIntSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, IntPrecision, 0);
    }

    public override SqlValueType BuildLongSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, LongPrecision, 0);
    }

    public override SqlValueType BuildULongSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, LongPrecision, 0);
    }

    public override SqlValueType BuildGuidSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarChar, 32);
    }

    // Constructors

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}