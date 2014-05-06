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
  internal class NpgsqlPathCompilers
  {
    [Compiler(typeof (NpgsqlPath), "Count", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPathCount(SqlExpression _this)
    {
      return SqlDml.NpgsqlPathAndPolygonCount(_this);
    }

    [Compiler(typeof (NpgsqlPath), "Open", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPathOpen(SqlExpression _this)
    {
      return SqlDml.NpgsqlPathAndPolygonOpen(_this);
    }

    [Compiler(typeof (NpgsqlPath), "Contains", TargetKind.Method)]
    public static SqlExpression NpgsqlPathContains(SqlExpression _this,
      [Type(typeof (NpgsqlPoint))] SqlExpression point)
    {
      return SqlDml.NpgsqlPathAndPolygonContains(_this, point);
    }
  }
}
