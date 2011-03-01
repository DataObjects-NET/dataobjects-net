﻿using System;
using System.Data;
using System.Data.Common;
using System.Security;
using global::MySql.Data.MySqlClient;

namespace Xtensive.Sql.Drivers.MySql
{
    internal class TypeMapper : Sql.TypeMapper
    {
        public override bool IsLiteralCastRequired(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
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
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.DateTime:
                    return true;
            }
            if (type == typeof(Guid))
                return true;
            if (type == typeof(TimeSpan))
                return true;
            if (type == typeof(byte[]))
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

#if NET40
        [SecuritySafeCritical]
#endif

        public override void SetGuidParameterValue(DbParameter parameter, object value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value == null ? (object)DBNull.Value : SqlHelper.GuidToString((Guid)value);
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

        // Constructors

        public TypeMapper(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
