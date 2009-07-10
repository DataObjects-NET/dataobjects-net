// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Data;
using System.Data.Common;
#if !OLD_NPGSQL
using Npgsql;
using NpgsqlTypes;
#endif

namespace Xtensive.Sql.PostgreSql
{
  internal class DataAccessHandler : ValueTypeMapping.DataAccessHandler
  {
    public override bool IsParameterCastRequired(Type type)
    {
      switch (Type.GetTypeCode(type)) {
      case TypeCode.Int16:
      case TypeCode.Single:
      case TypeCode.Double:
      case TypeCode.DateTime:
        return true;
      }
      if (type==typeof(Guid))
        return true;
      if (type==typeof(TimeSpan))
        return true;
      if (type==typeof(byte[]))
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

    public override void SetTimeSpanParameterValue(DbParameter parameter, object value)
    {
#if OLD_NPGSQL
      parameter.DbType = DbType.String;
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = IntervalHelper.TimeSpanToString((TimeSpan) value);
#else
      var nativeParameter = (NpgsqlParameter) parameter;
      nativeParameter.NpgsqlDbType = NpgsqlDbType.Interval;
      nativeParameter.Value = value!=null
        ? new NpgsqlInterval((TimeSpan) value)
        : DBNull.Value;
#endif
    }

    public override void SetGuidParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Binary;
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      var guid = (Guid) value;
      parameter.Value = guid.ToByteArray();
    }

    public override SqlValueType BuildByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
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

    public override SqlValueType BuildGuidSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarBinaryMax);
    }

    public override object ReadByte(DbDataReader reader, int index)
    {
      return Convert.ToByte(reader[index]);
    }

    public override object ReadGuid(DbDataReader reader, int index)
    {
      var bytes = new byte[16];
      reader.GetBytes(index, 0, bytes, 0, 16);
      return new Guid(bytes);
    }

    public override object ReadTimeSpan(DbDataReader reader, int index)
    {
#if OLD_NPGSQL
      object value = reader.GetValue(index);

      switch (Type.GetTypeCode(value.GetType())) {
      case TypeCode.String:
        return IntervalHelper.StringToTimeSpan((string) value);
      case TypeCode.Byte:
      case TypeCode.SByte:
      case TypeCode.Int16:
      case TypeCode.Int32:
      case TypeCode.Int64:
      case TypeCode.UInt16:
      case TypeCode.UInt32:
      case TypeCode.UInt64:
        return new TimeSpan(Convert.ToInt64(value));
      }

      if (value is TimeSpan)
        return value;
      throw new NotSupportedException();
#else
      var nativeReader = (NpgsqlDataReader) reader;
      return (TimeSpan) nativeReader.GetInterval(index);
#endif
    }

    // Constructors

    public DataAccessHandler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}