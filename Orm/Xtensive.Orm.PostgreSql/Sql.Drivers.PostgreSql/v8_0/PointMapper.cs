// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.09

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal sealed class PointMapper : PostgreSqlTypeMapper
  {
    private const string PointTypeName = "NpgsqlTypes.NpgsqlPoint, " +
      "Npgsql, Version=2.0.12.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7";

    // Constructors

    public PointMapper()
      : base(PointTypeName, "Point", CustomSqlType.Point)
    {
    }
  }
}
