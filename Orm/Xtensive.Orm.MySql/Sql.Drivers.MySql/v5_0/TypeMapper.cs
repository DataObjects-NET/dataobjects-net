// Copyright (C) 2011-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security;

namespace Xtensive.Sql.Drivers.MySql.v5_0
{
  internal class TypeMapper : Sql.TypeMapper
  {
    private static readonly Type[] CastRequiredTypes = new[] { typeof(Guid), typeof(TimeSpan), typeof(byte[]), typeof (DateOnly), typeof(TimeOnly) };

    /// <inheritdoc/>
    public override bool IsParameterCastRequired(Type type)
    {
      switch (Type.GetTypeCode(type)) {
        case TypeCode.Single:
        case TypeCode.Double:
        case TypeCode.DateTime:
          return true;
      }
      return CastRequiredTypes.Contains(type);
    }

    /// <inheritdoc/>
    public override void BindSByte(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int16;
      parameter.Value = value ?? DBNull.Value;
    }

    /// <inheritdoc/>
    public override void BindUShort(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int32;
      parameter.Value = value ?? DBNull.Value;
    }

    /// <inheritdoc/>
    public override void BindUInt(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      parameter.Value = value ?? DBNull.Value;
    }

    /// <inheritdoc/>
    public override void BindULong(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value ?? DBNull.Value;
    }

    /// <inheritdoc/>
    public override void BindGuid(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value==null ? (object) DBNull.Value : SqlHelper.GuidToString((Guid) value);
    }

    public override void BindDateOnly(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Date;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void BindTimeOnly(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Time;
      parameter.Value = value ?? DBNull.Value;
    }

    /// <inheritdoc/>
    public override SqlValueType MapByte(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }

    /// <inheritdoc/>
    public override SqlValueType MapSByte(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }

    /// <inheritdoc/>
    public override SqlValueType MapUShort(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int32);
    }

    /// <inheritdoc/>
    public override SqlValueType MapUInt(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int64);
    }

    /// <inheritdoc/>
    public override SqlValueType MapULong(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 20, 0);
    }

    /// <inheritdoc/>
    public override SqlValueType MapGuid(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarChar, 32);
    }

    /// <inheritdoc/>
    public override object ReadByte(DbDataReader reader, int index)
    {
      return Convert.ToByte(reader[index]);
    }

    /// <inheritdoc/>
    public override object ReadGuid(DbDataReader reader, int index)
    {
      return SqlHelper.GuidFromString(reader.GetString(index));
    }

    // Constructors

    [SecuritySafeCritical]
    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}