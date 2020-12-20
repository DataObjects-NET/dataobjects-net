// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using NpgsqlTypes;
using Xtensive.Reflection.PostgreSql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class Driver : PostgreSql.Driver
  {
    protected override Sql.TypeMapper CreateTypeMapper() => new TypeMapper(this);

    protected override SqlCompiler CreateCompiler() => new Compiler(this);

    protected override Model.Extractor CreateExtractor() => new Extractor(this);

    protected override SqlTranslator CreateTranslator() => new Translator(this);

    protected override Info.ServerInfoProvider CreateServerInfoProvider() => new ServerInfoProvider(this);

    protected override void RegisterCustomMappings(TypeMappingRegistryBuilder builder)
    {
      builder.Add(new PointMapper());
      builder.Add(new LSegMapper());
      builder.Add(new BoxMapper());
      builder.Add(new PathMapper());
      builder.Add(new PolygonMapper());
      builder.Add(new CircleMapper());
      builder.Add(WellKnownTypes.DateTimeOffsetType,
        builder.Mapper.ReadDateTimeOffset,
        builder.Mapper.BindDateTimeOffset,
        builder.Mapper.MapDateTimeOffset);
    }

    protected override void RegisterCustomReverseMappings(TypeMappingRegistryBuilder builder)
    {
      builder.AddReverse(CustomSqlType.Point, WellKnownTypes.NpgsqlPointType);
      builder.AddReverse(CustomSqlType.LSeg, WellKnownTypes.NpgsqlLSegType);
      builder.AddReverse(CustomSqlType.Box, WellKnownTypes.NpgsqlBoxType);
      builder.AddReverse(CustomSqlType.Path, WellKnownTypes.NpgsqlPathType);
      builder.AddReverse(CustomSqlType.Polygon, WellKnownTypes.NpgsqlPolygonType);
      builder.AddReverse(CustomSqlType.Circle, WellKnownTypes.NpgsqlCircleType);
      builder.AddReverse(SqlType.DateTimeOffset, WellKnownTypes.DateTimeOffsetType);
    }

    // Constructors

    public Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}