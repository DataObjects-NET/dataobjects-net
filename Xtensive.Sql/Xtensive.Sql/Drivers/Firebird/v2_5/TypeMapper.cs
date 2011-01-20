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

        public override SqlValueType BuildBooleanSqlType(int? length, int? precision, int? scale)
        {
            return new SqlValueType(SqlType.Int16);
        }

        public override SqlValueType BuildTimeSpanSqlType(int? length, int? precision, int? scale)
        {
            return base.BuildStringSqlType(30, null, null);
        }

        public override SqlValueType BuildULongSqlType(int? length, int? precision, int? scale)
        {
            return base.BuildStringSqlType(30, null, null); ;
        }

        public override object ReadTimeSpan(DbDataReader reader, int index)
        {
            return new TimeSpan(1, 1, 1, 1, 1);
        }

        public override void SetTimeSpanParameterValue(DbParameter parameter, object value)
        {
            SetStringParameterValue(parameter, "1,1,1,1,1");
        }

        public override void SetULongParameterValue(DbParameter parameter, object value)
        {
            parameter.DbType = System.Data.DbType.String;
            parameter.Value = value ?? DBNull.Value;
        }

        // Constructors

        public TypeMapper(SqlDriver driver)
            : base(driver)
        {
        }

    }
}
