// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2014.05.06;

using Xtensive.Core;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.PostgreSql
{
  /// <summary>
  /// A factory for SQL DML operations for Postgresql DBMS.
  /// Similarly to <see cref="Xtensive.Sql.SqlDml"/> it contains
  /// factory methods for provider-specific <see cref="SqlExpression"/>s.
  /// </summary>
  public class PostgresqlSqlDml
  {
    #region Spatial types

    /// <summary>
    /// Constructs an <see cref="SqlExpression"/> instance extracting a "point'(x,y)'" with the
    /// specified <see paramref="pointIndex"/> from the value of some geometry type like the 
    /// "box'(x1,y1),(x2,y2)'" or the "lseg'[(x1,y1),(x2,y2)]'" represented by sequence of points.
    /// </summary>
    public static SqlExpression NpgsqlTypeExtractPoint(SqlExpression operand, SqlExpression pointIndex)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, nameof(operand));
      ArgumentValidator.EnsureArgumentNotNull(pointIndex, nameof(pointIndex));
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlTypeExtractPoint, operand, pointIndex);
    }

    /// <summary>
    /// Creates an <see cref="SqlExpression"/> instance comparing the <see paramref="left"/> and <see paramref="right"/>
    /// operands for equality.
    /// </summary>
    public static SqlExpression NpgsqlTypeOperatorEquality(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlTypeOperatorEquality, left, right);
    }

    #region NpgsqlPoint

    /// <summary>
    /// Creates an <see cref="SqlExpression"/> instance representing the Postgre SQL's "point'(x,y)'" data type.
    /// </summary>
    public static SqlExpression NpgsqlPointConstructor(SqlExpression x, SqlExpression y)
    {
      ArgumentValidator.EnsureArgumentNotNull(x, "x");
      ArgumentValidator.EnsureArgumentNotNull(y, "y");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlPointConstructor, x, y);
    }

    /// <summary>
    /// Assuming the <see paramref="operand"/> is the "point'(x,y)'" datatype builds an expressiong extracting its 'x' component value.
    /// </summary>
    public static SqlExpression NpgsqlPointExtractX(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlPointExtractX, operand);
    }

    /// <summary>
    /// Assuming the <see paramref="operand"/> is the "point'(x,y)'" datatype builds an expressiong extracting its 'y' component value.
    /// </summary>
    public static SqlExpression NpgsqlPointExtractY(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlPointExtractY, operand);
    }

    #endregion

    #region NpgsqlBox

    /// <summary>
    /// Creates an <see cref="SqlExpression"/> instance representing the Postgre SQL's "box'(x1,y1),(x2,y2)'" data type.
    /// </summary>
    /// <param name="upperRight">An <see cref="SqlExpression"/> for the "point'(x1,y1)'" representing the upper right corner of the box.</param>
    /// <param name="lowerLeft">An <see cref="SqlExpression"/> for the "point'(x2,y2)'" representing the lower left corner of the box.</param>
    public static SqlExpression NpgsqlBoxConstructor(SqlExpression upperRight, SqlExpression lowerLeft)
    {
      ArgumentValidator.EnsureArgumentNotNull(upperRight, "upperRight");
      ArgumentValidator.EnsureArgumentNotNull(lowerLeft, "lowerLeft");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlBoxConstructor, upperRight, lowerLeft);
    }

    /// <summary>
    /// Assuming the <see paramref="operand"/> is the "box'(x1,y1),(x2,y2)'" datatype builds an expressiong extracting its height.
    /// </summary>
    public static SqlExpression NpgsqlBoxExtractHeight(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlBoxExtractHeight, operand);
    }

    /// <summary>
    /// Assuming the <see paramref="operand"/> is the "box'(x1,y1),(x2,y2)'" datatype builds an expressiong extracting its width.
    /// </summary>
    public static SqlExpression NpgsqlBoxExtractWidth(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlBoxExtractWidth, operand);
    }

    #endregion

    #region NpgsqlCircle

    /// <summary>
    /// Creates an <see cref="SqlExpression"/> instance representing the Postgre SQL's "circle'&lt;(x,y),r&gt;'" data type.
    /// </summary>
    /// <param name="center">An <see cref="SqlExpression"/> for the "point'(x,y)'" representing the center of the circle.</param>
    /// <param name="radius">An <see cref="SqlExpression"/> for the floating point value representing a radius of the circle.</param>
    /// <returns></returns>
    public static SqlExpression NpgsqlCircleConstructor(SqlExpression center, SqlExpression radius)
    {
      ArgumentValidator.EnsureArgumentNotNull(center, "center");
      ArgumentValidator.EnsureArgumentNotNull(radius, "radius");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlCircleConstructor, center, radius);
    }

    /// <summary>
    /// Assuming the <see paramref="operand"/> is the "circle'&lt;(x,y),r&gt;'" datatype builds an expressiong extracting
    /// the center point of the circle.
    /// </summary>
    public static SqlExpression NpgsqlCircleExtractCenter(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlCircleExtractCenter, operand);
    }

    /// <summary>
    /// Assuming the <see paramref="operand"/> is the "circle'&lt;(x,y),r&gt;'" datatype builds an expressiong extracting
    /// the circle's radius.
    /// </summary>
    public static SqlExpression NpgsqlCircleExtractRadius(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlCircleExtractRadius, operand);
    }

    #endregion

    #region NpgsqlLSeg

    /// <summary>
    /// Creates an <see cref="SqlExpression"/> instance representing the Postgre SQL's "lseg'[(x1,y1),(x2,y2)]'" data type.
    /// </summary>
    /// <param name="start">An <see cref="SqlExpression"/> for the "point'(x1,y1)'" representing starting point of the segment.</param>
    /// <param name="end">An <see cref="SqlExpression"/> for the "point'(x2,y2)'" representing ending point of the segment.</param>
    public static SqlExpression NpgsqlLSegConstructor(SqlExpression start, SqlExpression end)
    {
      ArgumentValidator.EnsureArgumentNotNull(start, "start");
      ArgumentValidator.EnsureArgumentNotNull(end, "end");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlLSegConstructor, start, end);
    }

    #endregion

    #region NpgsqlPath and NpgsqlPolygon

    /// <summary>
    /// Creates an <see cref="SqlExpression"/> computing the count of points
    /// for the "path'[(x1,y1),...]'" and "polygon'((x1,y1),...)'" data types.
    /// </summary>
    public static SqlExpression NpgsqlPathAndPolygonCount(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlPathAndPolygonCount, operand);
    }

    /// <summary>
    /// Creates an <see cref="SqlExpression"/> determining if the specified <see paramref="operand"/> represents
    /// an open "path'[(x1,y1),...]'" or "polygon'((x1,y1),...)'".
    /// </summary>
    public static SqlExpression NpgsqlPathAndPolygonOpen(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlPathAndPolygonOpen, operand);
    }

    /// <summary>
    /// Assuming the specified <see paramref="operand"/> is a "path'[(x1,y1),...]'" or "polygon'((x1,y1),...)'" object
    /// creates an <see cref="SqlExpression"/> determining if it contain the specified <see paramref="point"/>. 
    /// </summary>
    public static SqlExpression NpgsqlPathAndPolygonContains(SqlExpression operand, SqlExpression point)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      ArgumentValidator.EnsureArgumentNotNull(point, "point");
      return new SqlCustomFunctionCall(PostgresqlSqlFunctionType.NpgsqlPathAndPolygonContains, operand, point);
    }

    #endregion

    #endregion
  }
}
