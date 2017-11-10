// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using NpgsqlTypes;
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
      builder.Add(typeof (DateTimeOffset),
        builder.Mapper.ReadDateTimeOffset,
        builder.Mapper.BindDateTimeOffset,
        builder.Mapper.MapDateTimeOffset);
    }

    protected override void RegisterCustomReverseMappings(TypeMappingRegistryBuilder builder)
    {
      builder.AddReverse(CustomSqlType.Point, typeof (NpgsqlPoint));
      builder.AddReverse(CustomSqlType.LSeg, typeof (NpgsqlLSeg));
      builder.AddReverse(CustomSqlType.Box, typeof (NpgsqlBox));
      builder.AddReverse(CustomSqlType.Path, typeof (NpgsqlPath));
      builder.AddReverse(CustomSqlType.Polygon, typeof (NpgsqlPolygon));
      builder.AddReverse(CustomSqlType.Circle, typeof (NpgsqlCircle));
      builder.AddReverse(SqlType.DateTimeOffset, typeof (DateTimeOffset));
    }

    // Constructors

    public Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}