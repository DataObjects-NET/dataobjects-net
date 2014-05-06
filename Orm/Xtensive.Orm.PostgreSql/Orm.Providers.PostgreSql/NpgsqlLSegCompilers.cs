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
  internal static class NpgsqlLSegCompilers
  {
    #region Extractors

    [Compiler(typeof (NpgsqlLSeg), "Start", TargetKind.Field)]
    public static SqlExpression NpgsqlLSegExtractStartPoint(SqlExpression _this)
    {
      return SqlDml.NpgsqlTypeExtractPoint(_this, 0);
    }

    [Compiler(typeof (NpgsqlLSeg), "End", TargetKind.Field)]
    public static SqlExpression NpgsqlLSegExtractEndPoint(SqlExpression _this)
    {
      return SqlDml.NpgsqlTypeExtractPoint(_this, 1);
    }

    #endregion
  }
}
