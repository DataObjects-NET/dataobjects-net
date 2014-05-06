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
  internal class NpgsqlCircleCompilers
  {
    #region Extractors

    [Compiler(typeof (NpgsqlCircle), "Center", TargetKind.Field)]
    public static SqlExpression NpgsqlCircleExtractCenterPoint(SqlExpression _this)
    {
      return SqlDml.NpgsqlCircleExtractCenter(_this);
    }

    [Compiler(typeof (NpgsqlCircle), "Radius", TargetKind.Field)]
    public static SqlExpression NpgsqlCircleExtractRadius(SqlExpression _this)
    {
      return SqlDml.NpgsqlCircleExtractRadius(_this);
    }

    #endregion
  }
}
