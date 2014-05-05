// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.05

using NpgsqlTypes;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.PostgreSql
{
  [CompilerContainer(typeof (SqlExpression))]
  internal static class NpgsqlPointCompilers
  {
    #region Extractors

    [Compiler(typeof (NpgsqlPoint), "X", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractX(SqlExpression _this)
    {
      return SqlDml.NpgsqlPointExtractX(_this);
    }

    [Compiler(typeof (NpgsqlPoint), "Y", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractY(SqlExpression _this)
    {
      return SqlDml.NpgsqlPointExtractY(_this);
    }

    #endregion
  }
}
