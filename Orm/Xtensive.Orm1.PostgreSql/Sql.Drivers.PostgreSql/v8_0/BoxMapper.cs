// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.10

using NpgsqlTypes;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal sealed class BoxMapper : PostgreSqlTypeMapper
  {
    private static readonly string BoxTypeName = typeof (NpgsqlBox).AssemblyQualifiedName;

    // Constructors

    public BoxMapper()
      : base(BoxTypeName, NpgsqlDbType.Box, CustomSqlType.Box)
    {
    }
  }
}
