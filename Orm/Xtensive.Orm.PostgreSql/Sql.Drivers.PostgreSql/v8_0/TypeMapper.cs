// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Data;
using System.Data.Common;
using System.Security;
using Npgsql;
using NpgsqlTypes;


namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class TypeMapper : Sql.TypeMapper
  {
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
      return false;
    }

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
      if (type==typeof(Guid))
        return true;
      if (type==typeof(TimeSpan))
        return true;
      if (type==typeof(byte[]))
        return true;
      return false;
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

#if NET40
    [SecuritySafeCritical]
#endif
    public override void BindTimeSpan(DbParameter parameter, object value)
    {
      var nativeParameter = (NpgsqlParameter) parameter;
      nativeParameter.NpgsqlDbType = NpgsqlDbType.Interval;
      nativeParameter.Value = value!=null
        ? (object) new NpgsqlInterval((TimeSpan) value)
        : DBNull.Value;
    }

    public override void BindGuid(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value==null ? (object) DBNull.Value : SqlHelper.GuidToString((Guid) value);
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

    public override object ReadByte(DbDataReader reader, int index)
    {
      return Convert.ToByte(reader[index]);
    }

    public override object ReadGuid(DbDataReader reader, int index)
    {
      return SqlHelper.GuidFromString(reader.GetString(index));
    }

#if NET40
    [SecuritySafeCritical]
#endif
    public override object ReadTimeSpan(DbDataReader reader, int index)
    {
      var nativeReader = (NpgsqlDataReader) reader;
      return (TimeSpan) nativeReader.GetInterval(index);
    }

    // Constructors

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}