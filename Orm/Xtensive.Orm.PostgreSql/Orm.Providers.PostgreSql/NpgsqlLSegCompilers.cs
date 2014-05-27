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
  internal static class NpgsqlLSegCompilers
  {
    #region Extractors

    [Compiler(typeof (NpgsqlLSeg), "Start", TargetKind.Field)]
    public static SqlExpression NpgsqlLSegExtractStartPoint(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlTypeExtractPoint(_this, 0);
    }

    [Compiler(typeof (NpgsqlLSeg), "End", TargetKind.Field)]
    public static SqlExpression NpgsqlLSegExtractEndPoint(SqlExpression _this)
    {
      return PostgresqlSqlDml.NpgsqlTypeExtractPoint(_this, 1);
    }

    #endregion

    #region Operators

    [Compiler(typeof (NpgsqlLSeg), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression NpgsqlLSegOperatorEquality(
      [Type(typeof (NpgsqlLSeg))] SqlExpression left,
      [Type(typeof (NpgsqlLSeg))] SqlExpression right)
    {
      return left==right;
    }

    [Compiler(typeof (NpgsqlLSeg), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression NpgsqlLSegOperatorInequality(
      [Type(typeof (NpgsqlLSeg))] SqlExpression left,
      [Type(typeof (NpgsqlLSeg))] SqlExpression right)
    {
      return left!=right;
    }

    #endregion

    #region Constructors

    [Compiler(typeof (NpgsqlLSeg), null, TargetKind.Constructor)]
    public static SqlExpression NpgsqlLSegConstructor(
      [Type(typeof (NpgsqlPoint))] SqlExpression start,
      [Type(typeof (NpgsqlPoint))] SqlExpression end)
    {
      return PostgresqlSqlDml.NpgsqlLSegConstructor(start, end);
    }

    #endregion
  }
}
