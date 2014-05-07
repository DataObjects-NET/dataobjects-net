// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using NpgsqlTypes;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.PostgreSql
{
  [CompilerContainer(typeof (SqlExpression))]
  internal class NpgsqlBoxCompilers
  {
    #region Extractors

    [Compiler(typeof (NpgsqlBox), "UpperRight", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlBoxExtractUpperRightPoint(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlTypeExtractPoint(_this, 0);
    }

    [Compiler(typeof (NpgsqlBox), "LowerLeft", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlBoxExtractLowerLeftPoint(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlTypeExtractPoint(_this, 1);
    }

    [Compiler(typeof (NpgsqlBox), "Right", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractRight(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlPointExtractX(PostgresqlSqlDml.NpgsqlTypeExtractPoint(_this, 0));
    }

    [Compiler(typeof (NpgsqlBox), "Top", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractTop(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlPointExtractY(PostgresqlSqlDml.NpgsqlTypeExtractPoint(_this, 0));
    }

    [Compiler(typeof (NpgsqlBox), "Left", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractLeft(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlPointExtractX(PostgresqlSqlDml.NpgsqlTypeExtractPoint(_this, 1));
    }

    [Compiler(typeof (NpgsqlBox), "Bottom", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractBottom(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlPointExtractY(PostgresqlSqlDml.NpgsqlTypeExtractPoint(_this, 1));
    }

    [Compiler(typeof (NpgsqlBox), "Height", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractHeight(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlBoxExtractHeight(_this);
    }

    [Compiler(typeof (NpgsqlBox), "Width", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractWidth(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlBoxExtractWidth(_this);
    }

    #endregion
  }
}
