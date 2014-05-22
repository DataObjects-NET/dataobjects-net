// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.10

using System;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal sealed class PolygonMapper : PostgreSqlTypeMapper
  {
    private static readonly string PolygonTypeName = typeof (NpgsqlPolygon).AssemblyQualifiedName;

    public override void BindValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }

      var npgsqlParameter = (NpgsqlParameter) parameter;
      npgsqlParameter.Value = value;
      npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Polygon;

      // The method Equals(Object, Object), wrapped in a block 'try',
      // is required in order to determine that the value NpgsqlPolygon has been initialized with no parameters.
      try {
        value.Equals(value);
      }
      catch (Exception) {
        // If the value NpgsqlPolygon has been initialized with no parameters, then must set the initial value.
        npgsqlParameter.Value = new NpgsqlPolygon(new[] {new NpgsqlPoint()});
      }
    }

    // Constructors

    public PolygonMapper()
      : base(PolygonTypeName, NpgsqlDbType.Polygon, CustomSqlType.Polygon)
    {
    }
  }
}
