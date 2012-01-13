// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using System.Data;
using System.Data.Common;
using System.Security;

namespace Xtensive.Sql.Drivers.MySql.v5_0
{
  internal class TypeMapper : Sql.TypeMapper
  {
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
      if (type == typeof (Guid))
        return true;
      if (type == typeof (TimeSpan))
        return true;
      if (type == typeof (byte[]))
        return true;
      return false;
    }

    /// <inheritdoc/>
    public override void SetSByteParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int16;
      parameter.Value = value ?? DBNull.Value;
    }

    /// <inheritdoc/>
    public override void SetUShortParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int32;
      parameter.Value = value ?? DBNull.Value;
    }

    /// <inheritdoc/>
    public override void SetUIntParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      parameter.Value = value ?? DBNull.Value;
    }

    /// <inheritdoc/>
    public override void SetULongParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void SetTimeSpanParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      if (value != null) {
        var timeSpan = (TimeSpan) value;
        parameter.Value = timeSpan.Ticks*100;
      }
      else
        parameter.Value = DBNull.Value;
    }

    /// <inheritdoc/>
    public override void SetGuidParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value == null ? (object) DBNull.Value : SqlHelper.GuidToString((Guid) value);
    }

    /// <inheritdoc/>
    public override SqlValueType BuildByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }

    /// <inheritdoc/>
    public override SqlValueType BuildSByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }

    /// <inheritdoc/>
    public override SqlValueType BuildUShortSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int32);
    }

    /// <inheritdoc/>
    public override SqlValueType BuildUIntSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int64);
    }

    /// <inheritdoc/>
    public override SqlValueType BuildULongSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 20, 0);
    }

    /// <inheritdoc/>
    public override SqlValueType BuildTimeSpanSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int64);
    }

    /// <inheritdoc/>
    public override SqlValueType BuildGuidSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarChar, 32);
    }

    /// <inheritdoc/>
    public override object ReadByte(DbDataReader reader, int index)
    {
      return Convert.ToByte(reader[index]);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override object ReadGuid(DbDataReader reader, int index)
    {
      return SqlHelper.GuidFromString(reader.GetString(index));
    }

#if NET40
    [SecuritySafeCritical]
#endif
    // Constructors
    public TypeMapper(SqlDriver driver)
      : base(driver)
    {}
  }
}