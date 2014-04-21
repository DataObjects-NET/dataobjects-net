﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.10

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal sealed class BoxMapper : PostgreSqlTypeMapper
  {
    private const string BoxTypeName = "NpgsqlTypes.NpgsqlBox, " +
      "Npgsql, Version=2.0.12.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7";

    // Constructors

    public BoxMapper()
      : base(BoxTypeName, "Box", CustomSqlType.Box)
    {
    }
  }
}
