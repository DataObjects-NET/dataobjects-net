// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Reflection.PostgreSql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v11_0
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

    public Driver(CoreServerInfo coreServerInfo, PostgreServerInfo pgServerInfo)
      : base(coreServerInfo, pgServerInfo)
    {
    }
  }
}
