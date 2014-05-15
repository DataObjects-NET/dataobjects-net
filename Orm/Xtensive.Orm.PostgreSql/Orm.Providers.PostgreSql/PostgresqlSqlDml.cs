// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06;

using Xtensive.Core;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.PostgreSql
{
  /// <summary>
  /// A factory for SQL DML operations for Postgresql DBMS.
  /// </summary>
  public class PostgresqlSqlDml
  {
    #region Spatial types

    public static SqlExpression NpgsqlTypeExtractPoint(SqlExpression operand, SqlExpression numberPoint)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      ArgumentValidator.EnsureArgumentNotNull(numberPoint, "numberPoint");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlTypeExtractPoint, operand, numberPoint);
    }

    public static SqlExpression NpgsqlTypeOperatorEquality(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlTypeOperatorEquality, left, right);
    }

    #region NpgsqlPoint

    public static SqlExpression NpgsqlPointConstructor(SqlExpression x, SqlExpression y)
    {
      ArgumentValidator.EnsureArgumentNotNull(x, "x");
      ArgumentValidator.EnsureArgumentNotNull(y, "y");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlPointConstructor, x, y);
    }

    public static SqlExpression NpgsqlPointExtractX(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlPointExtractX, operand);
    }

    public static SqlExpression NpgsqlPointExtractY(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlPointExtractY, operand);
    }

    #endregion

    #region NpgsqlBox

    public static SqlExpression NpgsqlBoxConstructor(SqlExpression upperRight, SqlExpression lowerLeft)
    {
      ArgumentValidator.EnsureArgumentNotNull(upperRight, "upperRight");
      ArgumentValidator.EnsureArgumentNotNull(lowerLeft, "lowerLeft");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlBoxConstructor, upperRight, lowerLeft);
    }

    public static SqlExpression NpgsqlBoxExtractHeight(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlBoxExtractHeight, operand);
    }

    public static SqlExpression NpgsqlBoxExtractWidth(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlBoxExtractWidth, operand);
    }

    #endregion

    #region NpgsqlCircle

    public static SqlExpression NpgsqlCircleConstructor(SqlExpression center, SqlExpression radius)
    {
      ArgumentValidator.EnsureArgumentNotNull(center, "center");
      ArgumentValidator.EnsureArgumentNotNull(radius, "radius");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlCircleConstructor, center, radius);
    }

    public static SqlExpression NpgsqlCircleExtractCenter(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlCircleExtractCenter, operand);
    }

    public static SqlExpression NpgsqlCircleExtractRadius(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlCircleExtractRadius, operand);
    }

    #endregion

    #region NpgsqlLSeg

    public static SqlExpression NpgsqlLSegConstructor(SqlExpression start, SqlExpression end)
    {
      ArgumentValidator.EnsureArgumentNotNull(start, "start");
      ArgumentValidator.EnsureArgumentNotNull(end, "end");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlLSegConstructor, start, end);
    }

    #endregion

    #region NpgsqlPath and NpgsqlPolygon

    public static SqlExpression NpgsqlPathAndPolygonCount(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlPathAndPolygonCount, operand);
    }

    public static SqlExpression NpgsqlPathAndPolygonOpen(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlPathAndPolygonOpen, operand);
    }

    public static SqlExpression NpgsqlPathAndPolygonContains(SqlExpression operand, SqlExpression point)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      ArgumentValidator.EnsureArgumentNotNull(point, "point");
      return new CustomSqlFunctionCall(PostgresqlSqlFunctionType.NpgsqlPathAndPolygonContains, operand, point);
    }

    #endregion

    #endregion
  }
}
