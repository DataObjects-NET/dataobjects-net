// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using NpgsqlTypes;
using Xtensive.Sql.Dml;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers.PostgreSql
{
  [CompilerContainer(typeof (SqlExpression))]
  internal class NpgsqlCircleCompilers
  {
    #region Extractors

    [Compiler(typeof (NpgsqlCircle), "Center", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlCircleExtractCenterPoint(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlCircleExtractCenter(_this);
    }

    [Compiler(typeof (NpgsqlCircle), "Radius", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlCircleExtractRadius(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlCircleExtractRadius(_this);
    }

    #endregion

    #region Operators

    [Compiler(typeof (NpgsqlCircle), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression NpgsqlCircleOperatorEquality(
      [Type(typeof (NpgsqlCircle))] SqlExpression left,
      [Type(typeof (NpgsqlCircle))] SqlExpression right)
    {
      return PostgresqlSqlDml.NpgsqlTypeOperatorEquality(left, right);
    }

    [Compiler(typeof (NpgsqlCircle), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression NpgsqlCircleOperatorInequality(
      [Type(typeof (NpgsqlCircle))] SqlExpression left,
      [Type(typeof (NpgsqlCircle))] SqlExpression right)
    {
      return left!=right;
    }

    #endregion

    #region Constructors

    [Compiler(typeof (NpgsqlCircle), null, TargetKind.Constructor)]
    public static SqlExpression NpgsqlCircleConstructor(
      [Type(typeof (NpgsqlPoint))] SqlExpression center,
      [Type(typeof (double))] SqlExpression radius)
    {
      return PostgresqlSqlDml.NpgsqlCircleConstructor(center, radius);
    }

    #endregion
  }
}
