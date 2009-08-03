// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Data.Common;
using System.Diagnostics;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Xtensive.Sql.Oracle
{
  internal class DataAccessHandler : ValueTypeMapping.DataAccessHandler
  {
    public override void SetBooleanParameterValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = (bool) value ? 1.0m : 0.0m;
    }

    public override void SetByteParameterValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = (decimal) (byte) value;
    }

    public override void SetSByteParameterValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = (decimal) (sbyte) value;
    }

    public override void SetShortParameterValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = (decimal) (short) value;
    }

    public override void SetUShortParameterValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = (decimal) (ushort) value;
    }

    public override void SetIntParameterValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = (decimal) (int) value;
    }

    public override void SetUIntParameterValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = (decimal) (uint) value;
    }

    public override void SetLongParameterValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = (decimal) (long) value;
    }

    public override void SetULongParameterValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = (decimal) (ulong) value;
    }

    public override void SetTimeSpanParameterValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.IntervalDS;
      nativeParameter.Value = new OracleIntervalDS((TimeSpan) value);
    }

    public override void SetGuidParameterValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = ((Guid) value).ToByteArray();
    }

    public override object ReadBoolean(DbDataReader reader, int index)
    {
      return Math.Abs(reader.GetDecimal(index) - 0.1m) > 0.2m;
    }

    public override object ReadByte(DbDataReader reader, int index)
    {
      return (byte) reader.GetDecimal(index);
    }

    public override object ReadSByte(DbDataReader reader, int index)
    {
      return (sbyte) reader.GetDecimal(index);
    }

    public override object ReadShort(DbDataReader reader, int index)
    {
      return (short) reader.GetDecimal(index);
    }

    public override object ReadUShort(DbDataReader reader, int index)
    {
      return (ushort) reader.GetDecimal(index);
    }

    public override object ReadInt(DbDataReader reader, int index)
    {
      return (int) reader.GetDecimal(index);
    }

    public override object ReadUInt(DbDataReader reader, int index)
    {
      return (uint) reader.GetDecimal(index);
    }

    public override object ReadLong(DbDataReader reader, int index)
    {
      return (long) reader.GetDecimal(index);
    }

    public override object ReadULong(DbDataReader reader, int index)
    {
      return (ulong) reader.GetDecimal(index);
    }

    public override object ReadTimeSpan(DbDataReader reader, int index)
    {
      var nativeReader = (OracleDataReader) reader;
      return (TimeSpan) nativeReader.GetOracleIntervalDS(index);
    }

    public override SqlValueType BuildBooleanSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 1, 0);
    }

    public override SqlValueType BuildByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 3, 0);
    }

    public override SqlValueType BuildSByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 3, 0);
    }

    public override SqlValueType BuildShortSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 5, 0);
    }

    public override SqlValueType BuildUShortSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 5, 0);
    }

    public override SqlValueType BuildIntSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 10, 0);
    }

    public override SqlValueType BuildUIntSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 10, 0);
    }

    public override SqlValueType BuildLongSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 20, 0);
    }

    public override SqlValueType BuildULongSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 20, 0);
    }

    public override SqlValueType BuildGuidSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarBinaryMax);
    }

    public override object ReadGuid(DbDataReader reader, int index)
    {
      var bytes = new byte[16];
      reader.GetBytes(index, 0, bytes, 0, 16);
      return new Guid(bytes);
    }

    // Constructors

    public DataAccessHandler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}