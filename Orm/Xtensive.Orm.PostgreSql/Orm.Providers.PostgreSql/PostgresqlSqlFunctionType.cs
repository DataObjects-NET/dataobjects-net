// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.PostgreSql
{
  public static class PostgresqlSqlFunctionType
  {
    // Npgsql
    public static readonly SqlCustomFunctionType NpgsqlTypeExtractPoint = new SqlCustomFunctionType("NpgsqlTypeExtractPoint");
    public static readonly SqlCustomFunctionType NpgsqlTypeOperatorEquality = new SqlCustomFunctionType("NpgsqlTypeOperatorEquality");

    // NpgsqlPoint
    public static readonly SqlCustomFunctionType NpgsqlPointConstructor = new SqlCustomFunctionType("NpgsqlPointConstructor");
    public static readonly SqlCustomFunctionType NpgsqlPointExtractX = new SqlCustomFunctionType("NpgsqlPointExtractX");
    public static readonly SqlCustomFunctionType NpgsqlPointExtractY = new SqlCustomFunctionType("NpgsqlPointExtractY");

    // NpgsqlBox
    public static readonly SqlCustomFunctionType NpgsqlBoxConstructor = new SqlCustomFunctionType("NpgsqlBoxConstructor");
    public static readonly SqlCustomFunctionType NpgsqlBoxExtractHeight = new SqlCustomFunctionType("NpgsqlBoxExtractHeight");
    public static readonly SqlCustomFunctionType NpgsqlBoxExtractWidth = new SqlCustomFunctionType("NpgsqlBoxExtractWidth");

    // NpgsqlCircle
    public static readonly SqlCustomFunctionType NpgsqlCircleConstructor = new SqlCustomFunctionType("NpgsqlCircleConstructor");
    public static readonly SqlCustomFunctionType NpgsqlCircleExtractCenter = new SqlCustomFunctionType("NpgsqlCircleExtractCenter");
    public static readonly SqlCustomFunctionType NpgsqlCircleExtractRadius = new SqlCustomFunctionType("NpgsqlCircleExtractRadius");

    // NpgsqlLSeg
    public static readonly SqlCustomFunctionType NpgsqlLSegConstructor = new SqlCustomFunctionType("NpgsqlLSegConstructor");

    // NpgsqlPath and NpgsqlPolygon
    public static readonly SqlCustomFunctionType NpgsqlPathAndPolygonCount = new SqlCustomFunctionType("NpgsqlPathAndPolygonCount");
    public static readonly SqlCustomFunctionType NpgsqlPathAndPolygonOpen = new SqlCustomFunctionType("NpgsqlPathAndPolygonOpen");
    public static readonly SqlCustomFunctionType NpgsqlPathAndPolygonContains = new SqlCustomFunctionType("NpgsqlPathAndPolygonContains");
  }
}
