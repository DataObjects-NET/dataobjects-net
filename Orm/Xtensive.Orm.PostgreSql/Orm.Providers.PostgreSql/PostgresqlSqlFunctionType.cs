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
    public static readonly CustomSqlFunctionType NpgsqlTypeExtractPoint = new CustomSqlFunctionType("NpgsqlTypeExtractPoint");
    public static readonly CustomSqlFunctionType NpgsqlTypeOperatorEquality = new CustomSqlFunctionType("NpgsqlTypeOperatorEquality");

    // NpgsqlPoint
    public static readonly CustomSqlFunctionType NpgsqlPointConstructor = new CustomSqlFunctionType("NpgsqlPointConstructor");
    public static readonly CustomSqlFunctionType NpgsqlPointExtractX = new CustomSqlFunctionType("NpgsqlPointExtractX");
    public static readonly CustomSqlFunctionType NpgsqlPointExtractY = new CustomSqlFunctionType("NpgsqlPointExtractY");

    // NpgsqlBox
    public static readonly CustomSqlFunctionType NpgsqlBoxConstructor = new CustomSqlFunctionType("NpgsqlBoxConstructor");
    public static readonly CustomSqlFunctionType NpgsqlBoxExtractHeight = new CustomSqlFunctionType("NpgsqlBoxExtractHeight");
    public static readonly CustomSqlFunctionType NpgsqlBoxExtractWidth = new CustomSqlFunctionType("NpgsqlBoxExtractWidth");

    // NpgsqlCircle
    public static readonly CustomSqlFunctionType NpgsqlCircleConstructor = new CustomSqlFunctionType("NpgsqlCircleConstructor");
    public static readonly CustomSqlFunctionType NpgsqlCircleExtractCenter = new CustomSqlFunctionType("NpgsqlCircleExtractCenter");
    public static readonly CustomSqlFunctionType NpgsqlCircleExtractRadius = new CustomSqlFunctionType("NpgsqlCircleExtractRadius");

    // NpgsqlLSeg
    public static readonly CustomSqlFunctionType NpgsqlLSegConstructor = new CustomSqlFunctionType("NpgsqlLSegConstructor");

    // NpgsqlPath and NpgsqlPolygon
    public static readonly CustomSqlFunctionType NpgsqlPathAndPolygonCount = new CustomSqlFunctionType("NpgsqlPathAndPolygonCount");
    public static readonly CustomSqlFunctionType NpgsqlPathAndPolygonOpen = new CustomSqlFunctionType("NpgsqlPathAndPolygonOpen");
    public static readonly CustomSqlFunctionType NpgsqlPathAndPolygonContains = new CustomSqlFunctionType("NpgsqlPathAndPolygonContains");
  }
}
