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
  internal sealed class PathMapper : PostgreSqlTypeMapper
  {
    private static readonly string PathTypeName = WellKnownTypes.NpgsqlPathType.AssemblyQualifiedName;

    public override void BindValue(DbParameter parameter, object value)
    {
      if (value == null) {
        parameter.Value = DBNull.Value;
        return;
      }

      var npgsqlParameter = (NpgsqlParameter) parameter;
      npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Path;

      if (value is NpgsqlPath path) {
        // we should fix paths with no points
        npgsqlParameter.Value = (path.Count > 0)
          ? value
          : new NpgsqlPath(new[] { new NpgsqlPoint() });
      }
      else {
        throw ValueNotOfTypeError(nameof(NpgsqlPolygon));
      }
    }

    // Constructors

    public PathMapper()
      : base(PathTypeName, NpgsqlDbType.Path, CustomSqlType.Path)
    {
    }
  }
}
