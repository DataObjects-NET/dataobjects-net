// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.07

namespace Xtensive.Sql.Drivers.SqlServer.v10
{
  internal sealed class GeometryMapper : SqlServerTypeMapper
  {
    private const string GeometryTypeName =
      "Microsoft.SqlServer.Types.SqlGeometry, " +
      "Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

    // Constructors

    public GeometryMapper()
      : base(GeometryTypeName, "geometry", CustomSqlType.Geometry)
    {
    }
  }
}