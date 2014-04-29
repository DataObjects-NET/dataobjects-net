﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.10

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal sealed class CircleMapper : PostgreSqlTypeMapper
  {
    private static readonly string CircleTypeName = typeof (NpgsqlTypes.NpgsqlCircle).AssemblyQualifiedName;

    // Constructors

    public CircleMapper()
      : base(CircleTypeName, "Circle", CustomSqlType.Circle)
    {
    }
  }
}
