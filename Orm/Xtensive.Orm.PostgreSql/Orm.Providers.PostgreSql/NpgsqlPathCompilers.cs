// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2014.05.06

using NpgsqlTypes;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.PostgreSql
{
  [CompilerContainer(typeof(SqlExpression))]
  internal class NpgsqlPathCompilers
  {
    [Compiler(typeof(NpgsqlPath), "Count", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPathCount(SqlExpression _this)
    {
      return PostgresqlSqlGeometryDml.NpgsqlPathAndPolygonCount(_this);
    }

    [Compiler(typeof(NpgsqlPath), "Open", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPathOpen(SqlExpression _this)
    {
      return PostgresqlSqlGeometryDml.NpgsqlPathAndPolygonOpen(_this);
    }

    [Compiler(typeof(NpgsqlPath), "Contains", TargetKind.Method)]
    public static SqlExpression NpgsqlPathContains(SqlExpression _this,
      [Type(typeof(NpgsqlPoint))] SqlExpression point)
    {
      return PostgresqlSqlGeometryDml.NpgsqlPathAndPolygonContains(_this, point);
    }
  }
}