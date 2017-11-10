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
  internal class NpgsqlPolygonCompilers
  {
    [Compiler(typeof (NpgsqlPolygon), "Count", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPolygonCount(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlPathAndPolygonCount(_this);
    }

    [Compiler(typeof (NpgsqlPolygon), "Contains", TargetKind.Method)]
    public static SqlExpression NpgsqlPolygonContains(SqlExpression _this,
      [Type(typeof (NpgsqlPoint))] SqlExpression point)
    {
      return PostgresqlSqlDml.NpgsqlPathAndPolygonContains(_this, point);
    }

    #region Operators

    [Compiler(typeof (NpgsqlPolygon), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression NpgsqlPolygonOperatorEquality(
      [Type(typeof (NpgsqlPolygon))] SqlExpression left,
      [Type(typeof (NpgsqlPolygon))] SqlExpression right)
    {
      return PostgresqlSqlDml.NpgsqlTypeOperatorEquality(left, right);
    }

    [Compiler(typeof(NpgsqlPolygon), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression NpgsqlPolygonOperatorInequality(
      [Type(typeof(NpgsqlPolygon))] SqlExpression left,
      [Type(typeof(NpgsqlPolygon))] SqlExpression right)
    {
      return !NpgsqlPolygonOperatorEquality(left, right);
    }

    #endregion
  }
}
