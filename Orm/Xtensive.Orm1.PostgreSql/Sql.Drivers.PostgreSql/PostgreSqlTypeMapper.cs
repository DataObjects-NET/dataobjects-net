// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.09

using System;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

namespace Xtensive.Sql.Drivers.PostgreSql
{
  internal abstract class PostgreSqlTypeMapper : CustomTypeMapper
  {
    private readonly Type type;
    private readonly NpgsqlDbType npgsqlDbType;
    private readonly SqlType sqlType;

    public override bool Enabled
    {
      get { return type!=null; }
    }

    public override Type Type
    {
      get { return type; }
    }

    public override void BindValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }

      var npgsqlParameter = (NpgsqlParameter) parameter;
      npgsqlParameter.Value = value;
      npgsqlParameter.NpgsqlDbType = npgsqlDbType;
    }

    public override object ReadValue(DbDataReader reader, int index)
    {
      return reader.GetValue(index);
    }

    public override SqlValueType MapType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(sqlType);
    }

    // Constructors

    protected PostgreSqlTypeMapper(string frameworkType, NpgsqlDbType npgsqlDbType, SqlType sqlType)
    {
      type = Type.GetType(frameworkType);

      this.npgsqlDbType = npgsqlDbType;
      this.sqlType = sqlType;
    }
  }
}
