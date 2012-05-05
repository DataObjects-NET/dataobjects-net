// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.Drivers.Oracle.Resources;
using System.Linq;

namespace Xtensive.Sql.Drivers.Oracle.v09
{
  internal class Compiler : SqlCompiler
  {
    private static readonly SqlExpression SundayNumber = SqlDml.Native(
      "TO_NUMBER(TO_CHAR(TIMESTAMP '2009-07-26 00:00:00.000', 'D'))");

    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
      case SqlFunctionType.PadLeft:
      case SqlFunctionType.PadRight:
        SqlHelper.GenericPad(node).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeAddYears:
        DateTimeAddComponent(node.Arguments[0], node.Arguments[1], true).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeAddMonths:
        DateTimeAddComponent(node.Arguments[0], node.Arguments[1], false).AcceptVisitor(this);
        return;
      case SqlFunctionType.IntervalConstruct:
        IntervalConstruct(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeConstruct:
        DateTimeConstruct(node.Arguments[0], node.Arguments[1], node.Arguments[2]).AcceptVisitor(this);
        return;
      case SqlFunctionType.IntervalAbs:
        SqlHelper.IntervalAbs(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.IntervalToMilliseconds:
        SqlHelper.IntervalToMilliseconds(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.IntervalToNanoseconds:
        SqlHelper.IntervalToNanoseconds(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.Position:
        Position(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
        return;
      case SqlFunctionType.CharLength:
        SqlDml.Coalesce(SqlDml.FunctionCall("LENGTH", node.Arguments[0]), 0).AcceptVisitor(this);
        return;
      default:
        base.Visit(node);
        return;
      }
    }

    public override void Visit(SqlCreateTable node)
    {
      var table = node.Table as TemporaryTable;
      if (table!=null && !table.IsGlobal)
        throw new NotSupportedException(Strings.ExOracleDoesNotSupportLocalTemporaryTables);
      base.Visit(node);
    }

    public override void Visit(SqlTrim node)
    {
      if (node.TrimCharacters!=null && node.TrimCharacters.Length > 1)
        throw new NotSupportedException(Strings.ExOracleDoesNotSupportTrimmingMoreThatOneCharacterAtOnce);
      base.Visit(node);
    }

    public override void Visit(SqlExtract node)
    {
      switch (node.DateTimePart) {
      case SqlDateTimePart.DayOfYear:
        DateTimeExtractDayOfYear(node.Operand).AcceptVisitor(this);
        return;
      case SqlDateTimePart.DayOfWeek:
        DateTimeExtractDayOfWeek(node.Operand).AcceptVisitor(this);
        return;
      default:
        base.Visit(node);
        return;
      }
    }

    public override void VisitSelectFrom(SqlSelect node)
    {
      if (node.From!=null)
        base.VisitSelectFrom(node);
      else
        context.Output.AppendText("FROM DUAL");
    }

    public override void Visit(SqlJoinHint node)
    {
      var method = translator.Translate(node.Method);
      if (string.IsNullOrEmpty(method))
        return;
      context.Output.AppendText(method);
      context.Output.AppendText("(");
      node.Table.AcceptVisitor(this);
      context.Output.AppendText(")");
    }

    public override void Visit(SqlFastFirstRowsHint node)
    {
      context.Output.AppendText(string.Format("FIRST_ROWS({0})", node.Amount));
    }

    public override void Visit(SqlNativeHint node)
    {
      context.Output.AppendText(node.HintText);
    }

    public override void Visit(SqlForceJoinOrderHint node)
    {
      if (node.Tables.IsNullOrEmpty()) 
        context.Output.AppendText("ORDERED");
      else {
        context.Output.AppendText("LEADING(");
        using (context.EnterCollectionScope())
          foreach (var table in node.Tables)
            table.AcceptVisitor(this);
        context.Output.AppendText(")");
      }
    }

    public override void Visit(SqlUpdate node)
    {
      if (node.From!=null)
        throw new NotSupportedException(Strings.ExOracleDoesNotSupportUpdateFromStatements);
      base.Visit(node);
    }

    public override void Visit(SqlUnary node)
    {
      switch (node.NodeType) {
      case SqlNodeType.BitNot:
        BitNot(node.Operand).AcceptVisitor(this);
        return;
      default:
        base.Visit(node);
        return;
      }
    }

    public override void Visit(SqlBinary node)
    {
      switch (node.NodeType) {
      case SqlNodeType.Modulo:
        SqlDml.FunctionCall("MOD", node.Left, node.Right).AcceptVisitor(this);
        return;
      case SqlNodeType.BitAnd:
        BitAnd(node.Left, node.Right).AcceptVisitor(this);
        return;
      case SqlNodeType.BitOr:
        BitOr(node.Left, node.Right).AcceptVisitor(this);
        return;
      case SqlNodeType.BitXor:
        BitXor(node.Left, node.Right).AcceptVisitor(this);
        return;
      default:
        base.Visit(node);
        return;
      }
    }

    private static SqlExpression DateTimeAddComponent(SqlExpression dateTime, SqlExpression units, bool isYear)
    {
      return dateTime + SqlDml.FunctionCall(
        "NumToYmInterval", units, AnsiString(isYear ? "year" : "month"));
    }

    private static SqlExpression IntervalConstruct(SqlExpression nanoseconds)
    {
      const long nanosecondsPerSecond = 1000000000;
      return SqlDml.FunctionCall("NumToDsInterval",
        nanoseconds / SqlDml.Literal(nanosecondsPerSecond), AnsiString("second"));
    }

    private static SqlExpression DateTimeConstruct(SqlExpression years, SqlExpression months, SqlExpression days)
    {
      return SqlDml.FunctionCall("TO_TIMESTAMP",
        SqlDml.FunctionCall("TO_CHAR", ((years * 100) + months) * 100 + days),
        AnsiString("YYYYMMDD"));
    }

    private static SqlExpression DateTimeExtractDayOfWeek(SqlExpression dateTime)
    {
      // TO_CHAR with 'D' returns values depending on NLS_TERRITORY setting,
      // so sunday can be 1 or 7
      // there is no equivalent for sqlserver's @@DATEFIRST function
      // so we need to emulate it with very stupid code
      return (SqlDml.FunctionCall("TO_NUMBER", SqlDml.FunctionCall("TO_CHAR", dateTime, AnsiString("D"))) + 7 - SundayNumber) % 7;
    }

    private static SqlExpression DateTimeExtractDayOfYear(SqlExpression dateTime)
    {
      return SqlDml.FunctionCall("TO_NUMBER", SqlDml.FunctionCall("TO_CHAR", dateTime, AnsiString("DDD")));
    }

    private static SqlExpression BitAnd(SqlExpression left, SqlExpression right)
    {
      return SqlDml.FunctionCall("BITAND", left, right);
    }

    private static SqlExpression BitOr(SqlExpression left, SqlExpression right)
    {
      return left + right - BitAnd(left, right);
    }

    private static SqlExpression BitXor(SqlExpression left, SqlExpression right)
    {
      return BitOr(left, right) - BitAnd(left, right);
    }

    private static SqlExpression BitNot(SqlExpression operand)
    {
      return -1 - operand;
    }

    private static SqlExpression Position(SqlExpression substring, SqlExpression _string) //TODO : look into this (Malisa)
    {
      return SqlDml.FunctionCall("INSTR", _string, substring) - 1;
    }

    private static SqlExpression AnsiString(string value)
    {
      return SqlDml.Native("'" + value + "'");
    }

    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}