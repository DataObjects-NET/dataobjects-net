// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2014.05.06

using NpgsqlTypes;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Dml.PostgreSql;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers.PostgreSql
{
  [CompilerContainer(typeof(SqlExpression))]
  internal class NpgsqlPathCompilers
  {
    [Compiler(typeof(NpgsqlPath), nameof(NpgsqlPath.Count), TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPathCount(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlPathAndPolygonCount(_this);
    }

    [Compiler(typeof(NpgsqlPath), nameof(NpgsqlPath.Open), TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPathOpen(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlPathAndPolygonOpen(_this);
    }

    [Compiler(typeof(NpgsqlPath), nameof(NpgsqlPath.Contains), TargetKind.Method)]
    public static SqlExpression NpgsqlPathContains(SqlExpression _this,
      [Type(typeof(NpgsqlPoint))] SqlExpression point)
    {
      return PostgresqlSqlDml.NpgsqlPathAndPolygonContains(_this, point);
    }

    #region Operators

    [Compiler(typeof(NpgsqlPath), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression NpgsqlPathOperatorEquality(
      [Type(typeof(NpgsqlPath))] SqlExpression left,
      [Type(typeof(NpgsqlPath))] SqlExpression right)
    {
      return PostgresqlSqlDml.NpgsqlTypeOperatorEquality(left, right);
    }

    [Compiler(typeof(NpgsqlPath), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression NpgsqlPathOperatorInequality(
      [Type(typeof(NpgsqlPath))] SqlExpression left,
      [Type(typeof(NpgsqlPath))] SqlExpression right)
    {
      return !NpgsqlPathOperatorEquality(left, right);
    }

    #endregion
  }
}