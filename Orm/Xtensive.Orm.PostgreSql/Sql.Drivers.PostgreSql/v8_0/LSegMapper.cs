// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.10

using NpgsqlTypes;
using Xtensive.Reflection.PostgreSql;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal sealed class LSegMapper : PostgreSqlTypeMapper
  {
    private static readonly string LSegTypeName = WellKnownTypes.NpgsqlLSegType.AssemblyQualifiedName;

    // Constructors

    public LSegMapper()
      : base(LSegTypeName, NpgsqlDbType.LSeg, CustomSqlType.LSeg)
    {
    }
  }
}
