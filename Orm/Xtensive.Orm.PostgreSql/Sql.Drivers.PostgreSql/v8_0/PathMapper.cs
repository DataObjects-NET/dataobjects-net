// Copyright (C) 2014-2020 Xtensive LLC.
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

      npgsqlParameter.Value = value;
      npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Path;

      // The method Equals(Object, Object), wrapped in a block 'try',
      // is required in order to determine that the value NpgsqlPath has been initialized with no parameters.
      try {
        _ = value.Equals(value);
      }
      catch (Exception) {
        // If the value NpgsqlPath has been initialized with no parameters, then must set the initial value.
        npgsqlParameter.Value = new NpgsqlPath(new[] { new NpgsqlPoint() });
      }
    }

    // Constructors

    public PathMapper()
      : base(PathTypeName, NpgsqlDbType.Path, CustomSqlType.Path)
    {
    }
  }
}
