// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Linq;
using Xtensive.Orm.Providers.PostgreSql;
using Xtensive.Sql.Dml;
using SqlCompiler = Xtensive.Sql.Compiler.SqlCompiler;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class Compiler : SqlCompiler
  {
    private static readonly SqlNative OneYearInterval = SqlDml.Native("interval '1 year'");
    private static readonly SqlNative OneMonthInterval = SqlDml.Native("interval '1 month'");
    private static readonly SqlNative OneDayInterval = SqlDml.Native("interval '1 day'");
    private static readonly SqlNative OneSecondInterval = SqlDml.Native("interval '1 second'");

    public override void Visit(SqlDeclareCursor node)
    {
    }

    public override void Visit(SqlOpenCursor node)
    {
      base.Visit(node.Cursor.Declare());
    }

    public override void Visit(SqlBinary node)
    {
      var right = node.Right as SqlArray;
      if (!right.IsNullReference() && (node.NodeType==SqlNodeType.In || node.NodeType==SqlNodeType.NotIn)) {
        var row = SqlDml.Row(right.GetValues().Select(value => SqlDml.Literal(value)).ToArray());
        base.Visit(node.NodeType==SqlNodeType.In ? SqlDml.In(node.Left, row) : SqlDml.NotIn(node.Left, row));
      }
      else
        base.Visit(node);
    }

    public override void Visit(SqlFunctionCall node)
    {
      const double nanosecondsPerSecond = 1000000000.0;

      switch (node.FunctionType) {
      case SqlFunctionType.PadLeft:
      case SqlFunctionType.PadRight:
        SqlHelper.GenericPad(node).AcceptVisitor(this);
        return;
      case SqlFunctionType.Rand:
        SqlDml.FunctionCall(translator.Translate(SqlFunctionType.Rand)).AcceptVisitor(this);
        return;
      case SqlFunctionType.Square:
        SqlDml.Power(node.Arguments[0], 2).AcceptVisitor(this);
        return;
      case SqlFunctionType.IntervalConstruct:
        ((node.Arguments[0] / SqlDml.Literal(nanosecondsPerSecond)) * OneSecondInterval).AcceptVisitor(this);
        return;
      case SqlFunctionType.IntervalToMilliseconds:
        SqlHelper.IntervalToMilliseconds(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.IntervalToNanoseconds:
        SqlHelper.IntervalToNanoseconds(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.IntervalAbs:
        SqlHelper.IntervalAbs(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeConstruct:
        var newNode = (SqlDml.Literal(new DateTime(2001, 1, 1))
          + OneYearInterval * (node.Arguments[0] - 2001)
          + OneMonthInterval * (node.Arguments[1] - 1)
          + OneDayInterval * (node.Arguments[2] - 1));
        newNode.AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeTruncate:
        (SqlDml.FunctionCall("date_trunc", "day", node.Arguments[0])).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeAddMonths:
        (node.Arguments[0] + node.Arguments[1] * OneMonthInterval).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeAddYears:
        (node.Arguments[0] + node.Arguments[1] * OneYearInterval).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeToStringIso:
        DateTimeToStringIso(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      base.Visit(node);
    }

    public override void Visit(SqlCustomFunctionCall node)
    {
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlPointExtractX) {
        NpgsqlPointExtractPart(node.Arguments[0], 0).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlPointExtractY) {
        NpgsqlPointExtractPart(node.Arguments[0], 1).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlTypeExtractPoint) {
        NpgsqlTypeExtractPoint(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlBoxExtractHeight) {
        NpgsqlBoxExtractHeight(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlBoxExtractWidth) {
        NpgsqlBoxExtractWidth(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlCircleExtractCenter) {
        NpgsqlCircleExtractCenter(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlCircleExtractRadius) {
        NpgsqlCircleExtractRadius(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlPathAndPolygonCount) {
        NpgsqlPathAndPolygonCount(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlPathAndPolygonOpen) {
        NpgsqlPathAndPolygonOpen(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlPathAndPolygonContains) {
        NpgsqlPathAndPolygonContains(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlTypeOperatorEquality) {
        NpgsqlTypeOperatorEquality(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlPointConstructor) {
        var newNode = SqlDml.RawConcat(
          NpgsqlTypeConstructor(node.Arguments[0], node.Arguments[1], "point'"),
          SqlDml.Native("'"));
        newNode.AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlBoxConstructor) {
        NpgsqlTypeConstructor(node.Arguments[0], node.Arguments[1], "box").AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlCircleConstructor) {
        NpgsqlTypeConstructor(node.Arguments[0], node.Arguments[1], "circle").AcceptVisitor(this);
        return;
      }
      if (node.FunctionType==PostgresqlSqlFunctionType.NpgsqlLSegConstructor) {
        NpgsqlTypeConstructor(node.Arguments[0], node.Arguments[1], "lseg").AcceptVisitor(this);
        return;
      }
      base.Visit(node);
    }


    private static SqlExpression DateTimeToStringIso(SqlExpression dateTime)
    {
      return SqlDml.FunctionCall("To_Char", dateTime, "YYYY-MM-DD\"T\"HH24:MI:SS");
    }

    protected static SqlExpression NpgsqlPointExtractPart(SqlExpression expression, int part)
    {
      return SqlDml.RawConcat(expression, SqlDml.Native(String.Format("[{0}]", part)));
    }

    protected static SqlExpression NpgsqlTypeExtractPoint(SqlExpression expression, SqlExpression numberPoint)
    {
      var numberPointAsInt = numberPoint as SqlLiteral<int>;
      int valueNumberPoint = numberPointAsInt!=null ? numberPointAsInt.Value : 0;

      return SqlDml.RawConcat(
        SqlDml.Native("("),
        SqlDml.RawConcat(
          expression,
          SqlDml.Native(String.Format("[{0}])", valueNumberPoint))));
    }

    protected static SqlExpression NpgsqlBoxExtractHeight(SqlExpression expression)
    {
      return SqlDml.FunctionCall("HEIGHT", expression);
    }

    protected static SqlExpression NpgsqlBoxExtractWidth(SqlExpression expression)
    {
      return SqlDml.FunctionCall("WIDTH", expression);
    }

    protected static SqlExpression NpgsqlCircleExtractCenter(SqlExpression expression)
    {
      return SqlDml.RawConcat(SqlDml.Native("@@"), expression);
    }

    protected static SqlExpression NpgsqlCircleExtractRadius(SqlExpression expression)
    {
      return SqlDml.FunctionCall("RADIUS", expression);
    }

    protected static SqlExpression NpgsqlPathAndPolygonCount(SqlExpression expression)
    {
      return SqlDml.FunctionCall("NPOINTS", expression);
    }

    protected static SqlExpression NpgsqlPathAndPolygonOpen(SqlExpression expression)
    {
      return SqlDml.FunctionCall("ISOPEN", expression);
    }

    protected static SqlExpression NpgsqlPathAndPolygonContains(SqlExpression expression, SqlExpression point)
    {
      return SqlDml.RawConcat(
        expression,
        SqlDml.RawConcat(
          SqlDml.Native("@>"),
          point));
    }

    protected static SqlExpression NpgsqlTypeOperatorEquality(SqlExpression left, SqlExpression right)
    {
      return SqlDml.RawConcat(left,
        SqlDml.RawConcat(
          SqlDml.Native("~="),
          right));
    }

    private static SqlExpression NpgsqlTypeConstructor(SqlExpression left, SqlExpression right, string type)
    {
      return SqlDml.RawConcat(
        SqlDml.Native(String.Format("{0}(", type)),
        SqlDml.RawConcat(left,
          SqlDml.RawConcat(
            SqlDml.Native(","),
            SqlDml.RawConcat(
              right,
              SqlDml.Native(")")))));
    }

    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}