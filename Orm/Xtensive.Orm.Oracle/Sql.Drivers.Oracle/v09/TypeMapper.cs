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

    public override void BindBoolean(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : ((bool) value ? 1.0m : 0.0m);
    }

    public override void BindByte(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (byte) value;
    }

    public override void BindSByte(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (sbyte) value;
    }

    public override void BindShort(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (short) value;
    }

    public override void BindUShort(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (ushort) value;
    }

    public override void BindInt(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (int) value;
    }

    public override void BindUInt(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (uint) value;
    }

    public override void BindLong(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (long) value;
    }

    public override void BindULong(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value==null ? (object) DBNull.Value : (decimal) (ulong) value;
    }

    public override void BindFloat(DbParameter parameter, object value)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.Single;
      nativeParameter.Value = value ?? DBNull.Value;
    }

    public override void BindDouble(DbParameter parameter, object value)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.Double;
      nativeParameter.Value = value ?? DBNull.Value;
    }

    public override void BindTimeSpan(DbParameter parameter, object value)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.IntervalDS;
      nativeParameter.Value = value==null ? (object) DBNull.Value : new OracleIntervalDS((TimeSpan) value);
    }

    public override void BindByteArray(DbParameter parameter, object value)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.Blob;
      nativeParameter.Value = value ?? DBNull.Value;
    }

    public override void BindGuid(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value==null ? (object) DBNull.Value : SqlHelper.GuidToString((Guid) value);
    }

    public override void BindString(DbParameter parameter, object value)
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

    public override SqlValueType MapBoolean(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, BooleanPrecision, 0);
    }

    public override SqlValueType MapByte(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, BytePrecision, 0);
    }

    public override SqlValueType MapSByte(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, BytePrecision, 0);
    }

    public override SqlValueType MapShort(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, ShortPrecision, 0);
    }

    public override SqlValueType MapUShort(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, ShortPrecision, 0);
    }

    public override SqlValueType MapInt(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, IntPrecision, 0);
    }

    public override SqlValueType MapUInt(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, IntPrecision, 0);
    }

    public override SqlValueType MapLong(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, LongPrecision, 0);
    }

    public override SqlValueType MapULong(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, LongPrecision, 0);
    }

    public override SqlValueType MapGuid(int? length, int? precision, int? scale)
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