// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2014.04.10

using NpgsqlTypes;
using Xtensive.Reflection.PostgreSql;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal sealed class BoxMapper : PostgreSqlTypeMapper
  {
    private static readonly string BoxTypeName = WellKnownTypes.NpgsqlBoxType.AssemblyQualifiedName;

    // Constructors

    public BoxMapper()
      : base(BoxTypeName, NpgsqlDbType.Box, CustomSqlType.Box)
    {
    }
  }
}
