// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using NpgsqlTypes;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.PostgreSql
{
  [CompilerContainer(typeof (SqlExpression))]
  internal class NpgsqlPolygonCompilers
  {
    [Compiler(typeof (NpgsqlPolygon), "Count", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPolygonCount(SqlExpression _this)
    {
      return SqlDml.NpgsqlPathAndPolygonCount(_this);
    }

    [Compiler(typeof (NpgsqlPolygon), "Contains", TargetKind.Method)]
    public static SqlExpression NpgsqlPolygonContains(SqlExpression _this,
      [Type(typeof (NpgsqlPoint))] SqlExpression point)
    {
      return SqlDml.NpgsqlPathAndPolygonContains(_this, point);
    }
  }
}
