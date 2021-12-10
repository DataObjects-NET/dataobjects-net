// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2014.05.06

using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.PostgreSql
{
 /// <summary>
  /// Contains additional provider-specific SQL functions.
  /// These functions are not presented in general set of supported SQL
  /// functions and are used only within provider, e.g for operations on
  /// provider-specific types.
  /// </summary>
  public static class PostgresqlSqlFunctionType
  {
    // Npgsql

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlTypeExtractPoint(SqlExpression, SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlTypeExtractPoint = new SqlCustomFunctionType("NpgsqlTypeExtractPoint");

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlTypeOperatorEquality(SqlExpression, SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlTypeOperatorEquality = new SqlCustomFunctionType("NpgsqlTypeOperatorEquality");

    // NpgsqlPoint

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlPointConstructor(SqlExpression, SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlPointConstructor = new SqlCustomFunctionType("NpgsqlPointConstructor");

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlPointExtractX(SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlPointExtractX = new SqlCustomFunctionType("NpgsqlPointExtractX");

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlPointExtractY(SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlPointExtractY = new SqlCustomFunctionType("NpgsqlPointExtractY");

    // NpgsqlBox

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlBoxConstructor(SqlExpression, SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlBoxConstructor = new SqlCustomFunctionType("NpgsqlBoxConstructor");

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlBoxExtractHeight(SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlBoxExtractHeight = new SqlCustomFunctionType("NpgsqlBoxExtractHeight");

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlBoxExtractWidth(SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlBoxExtractWidth = new SqlCustomFunctionType("NpgsqlBoxExtractWidth");

    // NpgsqlCircle

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlCircleConstructor(SqlExpression, SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlCircleConstructor = new SqlCustomFunctionType("NpgsqlCircleConstructor");

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlCircleExtractCenter(SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlCircleExtractCenter = new SqlCustomFunctionType("NpgsqlCircleExtractCenter");

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlCircleExtractRadius(SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlCircleExtractRadius = new SqlCustomFunctionType("NpgsqlCircleExtractRadius");

    // NpgsqlLSeg

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlLSegConstructor(SqlExpression, SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlLSegConstructor = new SqlCustomFunctionType("NpgsqlLSegConstructor");

    // NpgsqlPath and NpgsqlPolygon

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlPathAndPolygonCount(SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlPathAndPolygonCount = new SqlCustomFunctionType("NpgsqlPathAndPolygonCount");

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlPathAndPolygonOpen(SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlPathAndPolygonOpen = new SqlCustomFunctionType("NpgsqlPathAndPolygonOpen");

    /// <summary>
    /// The <see cref="SqlCustomFunctionType"/> declaration for
    /// the <see cref="PostgresqlSqlDml.NpgsqlPathAndPolygonContains(SqlExpression, SqlExpression)"/> function.
    /// </summary>
    public static readonly SqlCustomFunctionType NpgsqlPathAndPolygonContains = new SqlCustomFunctionType("NpgsqlPathAndPolygonContains");
  }
}
