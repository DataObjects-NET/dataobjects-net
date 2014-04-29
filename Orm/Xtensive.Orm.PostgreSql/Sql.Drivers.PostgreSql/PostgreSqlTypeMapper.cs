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
    private readonly string udtType;
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

      NpgsqlDbType npgsqlDbType;
      Enum.TryParse(udtType, true, out npgsqlDbType);

      npgsqlParameter.Value = value;
      npgsqlParameter.NpgsqlDbType = npgsqlDbType;

      // The method Equals(Object, Object) was added in order to keep track of data types NpgsqlPath and  NpgsqlPolygon, 
      // which are defined without an initial value.
      try {
        value.Equals(value);
      }
      catch (Exception) {
        // For data types NpgsqlPath and NpgsqlPolygon, which defined without an initial values must specify to default values. 
        if (value is NpgsqlPath)
          npgsqlParameter.Value = new NpgsqlPath(new[] {new NpgsqlPoint()});
        if (value is NpgsqlPolygon)
          npgsqlParameter.Value = new NpgsqlPolygon(new[] {new NpgsqlPoint()});
      }
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

    protected PostgreSqlTypeMapper(string frameworkType, string udtType, SqlType sqlType)
    {
      type = Type.GetType(frameworkType);

      this.udtType = udtType;
      this.sqlType = sqlType;
    }
  }
}
