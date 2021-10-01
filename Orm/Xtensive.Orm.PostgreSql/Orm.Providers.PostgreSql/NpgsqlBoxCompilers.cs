// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2014.05.06

using NpgsqlTypes;
using Xtensive.Sql.Dml;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers.PostgreSql
{
  [CompilerContainer(typeof(SqlExpression))]
  internal class NpgsqlBoxCompilers
  {
    #region Extractors

    [Compiler(typeof(NpgsqlBox), "UpperRight", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlBoxExtractUpperRightPoint(SqlExpression _this)
    {
      return PostgresqlSqlGeometryDml.NpgsqlTypeExtractPoint(_this, 0);
    }

    [Compiler(typeof(NpgsqlBox), "LowerLeft", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlBoxExtractLowerLeftPoint(SqlExpression _this)
    {
      return PostgresqlSqlGeometryDml.NpgsqlTypeExtractPoint(_this, 1);
    }

    [Compiler(typeof(NpgsqlBox), "Right", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractRight(SqlExpression _this)
    {
      return PostgresqlSqlGeometryDml.NpgsqlPointExtractX(PostgresqlSqlGeometryDml.NpgsqlTypeExtractPoint(_this, 0));
    }

    [Compiler(typeof(NpgsqlBox), "Top", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractTop(SqlExpression _this)
    {
      return PostgresqlSqlGeometryDml.NpgsqlPointExtractY(PostgresqlSqlGeometryDml.NpgsqlTypeExtractPoint(_this, 0));
    }

    [Compiler(typeof(NpgsqlBox), "Left", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractLeft(SqlExpression _this)
    {
      return PostgresqlSqlGeometryDml.NpgsqlPointExtractX(PostgresqlSqlGeometryDml.NpgsqlTypeExtractPoint(_this, 1));
    }

    [Compiler(typeof(NpgsqlBox), "Bottom", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractBottom(SqlExpression _this)
    {
      return PostgresqlSqlGeometryDml.NpgsqlPointExtractY(PostgresqlSqlGeometryDml.NpgsqlTypeExtractPoint(_this, 1));
    }

    [Compiler(typeof(NpgsqlBox), "Height", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractHeight(SqlExpression _this)
    {
      return PostgresqlSqlGeometryDml.NpgsqlBoxExtractHeight(_this);
    }

    [Compiler(typeof(NpgsqlBox), "Width", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractWidth(SqlExpression _this)
    {
      return PostgresqlSqlGeometryDml.NpgsqlBoxExtractWidth(_this);
    }

    #endregion

    #region Operators

    [Compiler(typeof(NpgsqlBox), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression NpgsqlBoxOperatorEquality(
      [Type(typeof(NpgsqlBox))] SqlExpression left,
      [Type(typeof(NpgsqlBox))] SqlExpression right)
    {
      return PostgresqlSqlGeometryDml.NpgsqlTypeOperatorEquality(left, right);
    }

    [Compiler(typeof(NpgsqlBox), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression NpgsqlBoxOperatorInequality(
      [Type(typeof(NpgsqlBox))] SqlExpression left,
      [Type(typeof(NpgsqlBox))] SqlExpression right)
    {
      return !NpgsqlBoxOperatorEquality(left, right);
    }

    #endregion

    #region Constructors

    [Compiler(typeof(NpgsqlBox), null, TargetKind.Constructor)]
    public static SqlExpression NpgsqlBoxConstructor(
      [Type(typeof(float))] SqlExpression top,
      [Type(typeof(float))] SqlExpression right,
      [Type(typeof(float))] SqlExpression bottom,
      [Type(typeof(float))] SqlExpression left)
    {
      return PostgresqlSqlGeometryDml.NpgsqlBoxConstructor(
        PostgresqlSqlGeometryDml.NpgsqlPointConstructor(right, top),
        PostgresqlSqlGeometryDml.NpgsqlPointConstructor(left, bottom));
    }

    [Compiler(typeof(NpgsqlBox), null, TargetKind.Constructor)]
    public static SqlExpression NpgsqlBoxConstructor(
      [Type(typeof(NpgsqlPoint))] SqlExpression upperRight,
      [Type(typeof(NpgsqlPoint))] SqlExpression lowerLeft)
    {
      return PostgresqlSqlGeometryDml.NpgsqlBoxConstructor(upperRight, lowerLeft);
    }

    #endregion
  }
}