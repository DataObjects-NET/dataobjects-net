// Copyright (C) 2014-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2014.04.10

using System;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;
using Xtensive.Reflection.PostgreSql;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal sealed class PolygonMapper : PostgreSqlTypeMapper
  {
    private static readonly string PolygonTypeName = WellKnownTypes.NpgsqlPolygonType.AssemblyQualifiedName;

    public override void BindValue(DbParameter parameter, object value)
    {
      if (value == null) {
        parameter.Value = DBNull.Value;
        return;
      }

      var npgsqlParameter = (NpgsqlParameter) parameter;
      npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Polygon;

      if (value is NpgsqlPolygon poligon) {
        // we should fix poligons with no points
        npgsqlParameter.Value = (poligon.Count > 0)
          ? value
          : new NpgsqlPolygon(new[] { new NpgsqlPoint() });
      }
      else {
        throw ValueNotOfTypeError(nameof(NpgsqlPolygon));
      }
    }

    // Constructors

    public PolygonMapper()
      : base(PolygonTypeName, NpgsqlDbType.Polygon, CustomSqlType.Polygon)
    {
    }
  }
}
