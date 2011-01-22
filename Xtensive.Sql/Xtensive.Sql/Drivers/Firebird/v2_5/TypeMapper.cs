// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.10
using System;
using System.Data.Common;

namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
    internal class TypeMapper : Sql.TypeMapper
    {
        public override bool IsParameterCastRequired(Type type)
        {
            return true;
        }

        public override SqlValueType BuildBooleanSqlType(int? length, int? precision, int? scale)
        {
            return new SqlValueType(SqlType.Int16);
        }

        public override SqlValueType BuildTimeSpanSqlType(int? length, int? precision, int? scale)
        {
            return base.BuildStringSqlType(30, null, null);
        }

        public override SqlValueType BuildUShortSqlType(int? length, int? precision, int? scale)
        {
            return base.BuildIntSqlType(length, precision, scale);
        }

        public override SqlValueType BuildUIntSqlType(int? length, int? precision, int? scale)
        {
            return base.BuildLongSqlType(length, precision, scale);
        }

        public override SqlValueType BuildULongSqlType(int? length, int? precision, int? scale)
        {
            return base.BuildStringSqlType(30, null, null); ;
        }

        public override SqlValueType BuildCharSqlType(int? length, int? precision, int? scale)
        {
            return new SqlValueType(SqlType.Char, 1);
        }

        public override object ReadTimeSpan(DbDataReader reader, int index)
        {
            string s = reader.GetString(index);
            if (string.IsNullOrWhiteSpace(s))
                return null;
            int sign = 1;
            if (s[0] == '-')
            {
                s = s.Substring(1);
                sign = -1;
            }
            string[] ss = s.Split(',');
            
            return new TimeSpan(sign * Convert.ToInt32(ss[0]), sign * Convert.ToInt32(ss[1]), sign * Convert.ToInt32(ss[2]), sign * Convert.ToInt32(ss[3]), sign * Convert.ToInt32(ss[4]));
        }

        public override object ReadGuid(DbDataReader reader, int index)
        {
            string s = reader.GetString(index);
            if (string.IsNullOrWhiteSpace(s))
                return null;
            return SqlHelper.GuidFromString(s);
        }

        public override void SetTimeSpanParameterValue(DbParameter parameter, object value)
        {
            if (value == null)
                SetStringParameterValue(parameter, null);
            else
                SetStringParameterValue(parameter, SqlHelper.TimeSpanToString((TimeSpan)value, Constants.TimeSpanFormatString));
        }

        public override object ReadChar(DbDataReader reader, int index)
        {
            char c = (char)base.ReadChar(reader, index);
            if (char.IsControl(c) || char.IsPunctuation(c))
                return c;
            if (char.IsWhiteSpace(c))
                return null;
            return c;
        }

        public override void SetCharParameterValue(DbParameter parameter, object value)
        {
            parameter.DbType = System.Data.DbType.String;
            if (value == null || (default(char).Equals(value)))
            {
                parameter.Value = DBNull.Value;
                return;
            }
            var _char = (char)value;
            parameter.Value = _char == default(char) ? string.Empty : _char.ToString();
        }

        public override object ReadString(DbDataReader reader, int index)
        {
            string s = (string)base.ReadString(reader, index);
            if (s != null)
                s = s.Trim();
            return s;
        }

        public override void SetULongParameterValue(DbParameter parameter, object value)
        {
            parameter.DbType = System.Data.DbType.String;
            parameter.Value = value ?? DBNull.Value;
        }

        public override void SetBooleanParameterValue(DbParameter parameter, object value)
        {
            object val = DBNull.Value;
            if (value != null)
                val = ((bool)value) ? (short)1 : (short)0;
            SetShortParameterValue(parameter, val);
        }

        public override void SetGuidParameterValue(DbParameter parameter, object value)
        {
            SetStringParameterValue(parameter, value==null? DBNull.Value : (object)SqlHelper.GuidToString((Guid)value));
        }

        // Constructors

        public TypeMapper(SqlDriver driver)
            : base(driver)
        {
        }

    }
}
