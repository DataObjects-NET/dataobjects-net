// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Core.Collections;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Oracle.Resources;
using System.Linq;

namespace Xtensive.Sql.Oracle.v09
{
  internal class Compiler : SqlCompiler
  {
    private static readonly SqlExpression SundayNumber = SqlDml.Native(
      "TO_NUMBER(TO_CHAR(TIMESTAMP '2009-07-26 00:00:00.000', 'D'))");

    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
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
      case SqlFunctionType.Position:
        using (context.EnterScope(node)) {
          context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.Entry, -1));
          using (context.EnterCollectionScope()) {
            context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentEntry, 0));
            node.Arguments[1].AcceptVisitor(this);
            context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentExit, 0));
            context.Output.AppendDelimiter(translator.Translate(context, node, FunctionCallSection.ArgumentDelimiter, 1));
            context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentEntry, 0));
            node.Arguments[0].AcceptVisitor(this);
            context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentExit, 0));
          }
          context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.Exit, -1));
        }
        return;
      default:
        base.Visit(node);
        return;
      }
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

    private static SqlExpression DateTimeAddComponent(SqlExpression dateTime, SqlExpression units, bool isYear)
    {
      return dateTime + SqlDml.FunctionCall(
        "NumToYmInterval", units, AnsiString(isYear ? "year" : "month"));
    }

    private static SqlExpression IntervalConstruct(SqlExpression milliseconds)
    {
      return SqlDml.FunctionCall("NumToDsInterval", milliseconds / 1000, AnsiString("second"));
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