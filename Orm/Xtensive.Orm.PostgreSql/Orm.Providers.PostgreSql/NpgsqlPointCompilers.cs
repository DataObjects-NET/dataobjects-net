// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.05

using NpgsqlTypes;
using Xtensive.Sql.Dml;
using Operator = Xtensive.Reflection.WellKnown.Operator;

namespace Xtensive.Orm.Providers.PostgreSql
{
  [CompilerContainer(typeof (SqlExpression))]
  internal static class NpgsqlPointCompilers
  {
    #region Extractors

    [Compiler(typeof (NpgsqlPoint), "X", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractX(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlPointExtractX(_this);
    }

    [Compiler(typeof (NpgsqlPoint), "Y", TargetKind.PropertyGet)]
    public static SqlExpression NpgsqlPointExtractY(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlPointExtractY(_this);
    }

    #endregion

    #region Operators

    [Compiler(typeof (NpgsqlPoint), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression NpgsqlPointOperatorEquality(
      [Type(typeof (NpgsqlPoint))] SqlExpression left,
      [Type(typeof (NpgsqlPoint))] SqlExpression right)
    {
      return PostgresqlSqlDml.NpgsqlTypeOperatorEquality(left, right);
    }

    [Compiler(typeof (NpgsqlPoint), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression NpgsqlPointOperatorInequality(
      [Type(typeof (NpgsqlPoint))] SqlExpression left,
      [Type(typeof (NpgsqlPoint))] SqlExpression right)
    {
      return left!=right;
    }

    #endregion

    #region Constructors

    [Compiler(typeof (NpgsqlPoint), null, TargetKind.Constructor)]
    public static SqlExpression NpgsqlPointConstructor(
      [Type(typeof (float))] SqlExpression x,
      [Type(typeof (float))] SqlExpression y)
    {
      return PostgresqlSqlDml.NpgsqlPointConstructor(x, y);
    }

    #endregion
  }
}
