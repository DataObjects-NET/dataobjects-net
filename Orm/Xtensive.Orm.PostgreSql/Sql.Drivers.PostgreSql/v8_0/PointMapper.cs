// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.09

using NpgsqlTypes;
using Xtensive.Reflection.PostgreSql;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal sealed class PointMapper : PostgreSqlTypeMapper
  {
    private static readonly string PointTypeName = WellKnownTypes.NpgsqlPointType.AssemblyQualifiedName;

    // Constructors

    public PointMapper()
      : base(PointTypeName, NpgsqlDbType.Point, CustomSqlType.Point)
    {
    }
  }
}
