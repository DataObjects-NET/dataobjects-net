// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class Driver : PostgreSql.Driver
  {
    protected override Sql.TypeMapper CreateTypeMapper()
    {
      return new TypeMapper(this);
    }

    protected override SqlCompiler CreateCompiler()
    {
      return new Compiler(this);
    }

    protected override Model.Extractor CreateExtractor()
    {
      return new Extractor(this);
    }

    protected override SqlTranslator CreateTranslator()
    {
      return new Translator(this);
    }
    
    protected override Info.ServerInfoProvider CreateServerInfoProvider()
    {
      return new ServerInfoProvider(this);
    }

    protected override void RegisterCustomMappings(TypeMappingRegistryBuilder builder)
    {
      builder.Add(new PointMapper());
      builder.Add(new LSegMapper());
      builder.Add(new BoxMapper());
      builder.Add(new PathMapper());
      builder.Add(new PolygonMapper());
      builder.Add(new CircleMapper());
    }

    protected override void RegisterCustomSqlTypes(TypeMappingRegistryBuilder builder)
    {
      builder.AddReverseMapping(CustomSqlType.Point, Type.GetType("NpgsqlTypes.NpgsqlPoint, Npgsql, Version=2.0.12.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7"));
      builder.AddReverseMapping(CustomSqlType.LSeg, Type.GetType("NpgsqlTypes.NpgsqlLSeg, Npgsql, Version=2.0.12.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7"));
      builder.AddReverseMapping(CustomSqlType.Box, Type.GetType("NpgsqlTypes.NpgsqlBox, Npgsql, Version=2.0.12.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7"));
      builder.AddReverseMapping(CustomSqlType.Path, Type.GetType("NpgsqlTypes.NpgsqlPath, Npgsql, Version=2.0.12.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7"));
      builder.AddReverseMapping(CustomSqlType.Polygon, Type.GetType("NpgsqlTypes.NpgsqlPolygon, Npgsql, Version=2.0.12.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7"));
      builder.AddReverseMapping(CustomSqlType.Circle, Type.GetType("NpgsqlTypes.NpgsqlCircle, Npgsql, Version=2.0.12.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7"));
    }

    // Constructors

    public Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}