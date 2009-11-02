// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.02

using System;
using System.Data;
using System.Data.Common;
using SqlServerCommand = System.Data.SqlClient.SqlCommand;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;
using SqlServerTransaction = System.Data.SqlClient.SqlTransaction;
using SqlServerParameter = System.Data.SqlClient.SqlParameter;

namespace Xtensive.Sql.SqlServer
{
  internal class DataAccessHandler : ValueTypeMapping.DataAccessHandler
  {
    public override void SetSByteParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int16;
      parameter.Value = value;
    }

    public override void SetUShortParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int32;
      parameter.Value = value;
    }

    public override void SetUIntParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      parameter.Value = value;
    }

    public override void SetULongParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value;
    }

    public override void SetTimeSpanParameterValue(DbParameter parameter, object value)
    {
      var timeSpan = (TimeSpan) value;
      parameter.DbType = DbType.Int64;
      parameter.Value = (long) timeSpan.TotalMilliseconds;
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

    public override SqlValueType BuildTimeSpanSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int64);
    }

    public override object ReadTimeSpan(DbDataReader reader, int index)
    {
      return TimeSpan.FromMilliseconds(reader.GetInt64(index));
    }

    // Constructors

    public DataAccessHandler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}