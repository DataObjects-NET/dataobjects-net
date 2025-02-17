// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Data;
using System.Data.Common;
using System.Security;
using Npgsql;
using NpgsqlTypes;
using Xtensive.Reflection.PostgreSql;


namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class TypeMapper : Sql.TypeMapper
  {
    public override bool IsParameterCastRequired(Type type)
    {
      switch (Type.GetTypeCode(type)) {
      case TypeCode.Byte:
      case TypeCode.SByte:
      case TypeCode.Int16:
      case TypeCode.UInt16:
      case TypeCode.Single:
      case TypeCode.Double:
      case TypeCode.DateTime:
        return true;
      }
      if (type == WellKnownTypes.DateTimeOffsetType) {
        return true;
      }
      if (type == WellKnownTypes.GuidType) {
        return true;
      }
      if (type == WellKnownTypes.TimeSpanType) {
        return true;
      }
      if (type == WellKnownTypes.ByteArrayType) {
        return true;
      }
      return false;
    }

    public override void BindByte(DbParameter parameter, object value)
    {
      if(value == null) {
        base.BindByte(parameter, value);
      }
      else {
        base.BindByte(parameter, Convert.ToByte(value));
      }
    }

    public override void BindShort(DbParameter parameter, object value)
    {
      if (value == null) {
        base.BindShort(parameter, value);
      }
      else {
        base.BindShort(parameter, Convert.ToInt16(value));
      }
    }

    public override void BindInt(DbParameter parameter, object value)
    {
      if (value == null) {
        base.BindInt(parameter, value);
      }
      else {
        base.BindInt(parameter, Convert.ToInt32(value));
      }
    }

    public override void BindLong(DbParameter parameter, object value)
    {
      if (value == null) {
        base.BindLong(parameter, value);
      }
      else {
        base.BindLong(parameter, Convert.ToInt64(value));
      }
    }

    public override void BindSByte(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int16;
      parameter.Value = value == null ? DBNull.Value : (object) Convert.ToInt16(value);
    }

    public override void BindUShort(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int32;
      parameter.Value = value == null ? DBNull.Value : (object) Convert.ToInt32(value);
    }
    
    public override void BindUInt(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      parameter.Value = value == null ? DBNull.Value : (object) Convert.ToInt64(value);
    }

    public override void BindULong(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value == null ? DBNull.Value : (object) Convert.ToDecimal(value);
    }

    [SecuritySafeCritical]
    public override void BindTimeSpan(DbParameter parameter, object value)
    {
      var nativeParameter = (NpgsqlParameter) parameter;
      nativeParameter.NpgsqlDbType = NpgsqlDbType.Interval;
      nativeParameter.Value = value != null
        ? (object) new NpgsqlTimeSpan((TimeSpan) value)
        : DBNull.Value;
    }

    public override void BindGuid(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value == null ? (object) DBNull.Value : SqlHelper.GuidToString((Guid) value);
    }

    [SecuritySafeCritical]
    public override void BindDateTimeOffset(DbParameter parameter, object value)
    {
      var nativeParameter = (NpgsqlParameter) parameter;
      nativeParameter.NpgsqlDbType = NpgsqlDbType.TimestampTz;
      nativeParameter.NpgsqlValue = value ?? DBNull.Value;
    }

    public override SqlValueType MapByte(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
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

    public override SqlValueType MapGuid(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarChar, 32);
    }

    public override SqlValueType MapTimeSpan(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Interval);
    }

    public override object ReadByte(DbDataReader reader, int index)
    {
      return Convert.ToByte(reader[index]);
    }

    public override object ReadGuid(DbDataReader reader, int index)
    {
      return SqlHelper.GuidFromString(reader.GetString(index));
    }

    [SecuritySafeCritical]
    public override object ReadTimeSpan(DbDataReader reader, int index)
    {
      var nativeReader = (NpgsqlDataReader) reader;
      return (TimeSpan) nativeReader.GetInterval(index);
    }

    public override object ReadDecimal(DbDataReader reader, int index)
    {
      var nativeReader = (NpgsqlDataReader) reader;
      return nativeReader.GetDecimal(index);
    }

    [SecuritySafeCritical]
    public override object ReadDateTimeOffset(DbDataReader reader, int index)
    {
      var nativeReader = (NpgsqlDataReader) reader;
      var value = nativeReader.GetFieldValue<DateTimeOffset>(index);
      return value;
    }

    // Constructors

    public TypeMapper(PostgreSql.Driver driver)
      : base(driver)
    {
      legacyTimestampBehaviorEnabled = driver.PostgreServerInfo.LegacyTimestampBehavior;
    }
  }
}