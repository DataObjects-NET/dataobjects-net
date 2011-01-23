// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.17

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;
using System;

namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
    internal class Compiler : SqlCompiler
    {

        protected static readonly long NanosecondsPerDay = TimeSpan.FromDays(1).Ticks * 100;
        protected static readonly long NanosecondsPerSecond = 1000000000;
        protected static readonly long NanosecondsPerMillisecond = 1000000;
        protected static readonly long MillisecondsPerDay = (long)TimeSpan.FromDays(1).TotalMilliseconds;
        protected static readonly long MillisecondsPerSecond = 1000L;


        public override void Visit(SqlSelect node)
        {
            base.Visit(node);
        }

        public override void VisitSelectFrom(SqlSelect node)
        {
            if (node.From != null)
                base.VisitSelectFrom(node);
            else
                context.Output.AppendText("FROM RDB$DATABASE");
        }

        public override void Visit(SqlQueryExpression node)
        {
            using (context.EnterScope(node))
            {
                bool needOpeningParenthesis = false;
                bool needClosingParenthesis = false;
                context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Entry));
                if (needOpeningParenthesis) context.Output.AppendText("(");
                node.Left.AcceptVisitor(this);
                if (needClosingParenthesis) context.Output.AppendText(")");
                context.Output.AppendText(translator.Translate(node.NodeType));
                context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.All));
                if (needOpeningParenthesis) context.Output.AppendText("(");
                node.Right.AcceptVisitor(this);
                if (needClosingParenthesis) context.Output.AppendText(")");
                context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Exit));
            }
        }

        public override void Visit(SqlExtract node)
        {
            switch (node.IntervalPart)
            {
                case SqlIntervalPart.Day:
                    Visit(CastToLong(node.Operand / NanosecondsPerDay));
                    return;
                case SqlIntervalPart.Hour:
                    Visit(CastToLong(node.Operand / (60 * 60 * NanosecondsPerSecond)) % 24);
                    return;
                case SqlIntervalPart.Minute:
                    Visit(CastToLong(node.Operand / (60 * NanosecondsPerSecond)) % 60);
                    return;
                case SqlIntervalPart.Second:
                    Visit(CastToLong(node.Operand / NanosecondsPerSecond) % 60);
                    return;
                case SqlIntervalPart.Millisecond:
                    Visit(CastToLong(node.Operand / NanosecondsPerMillisecond) % MillisecondsPerSecond);
                    return;
                case SqlIntervalPart.Nanosecond:
                    Visit(CastToLong(node.Operand));
                    return;
            }
            base.Visit(node);
        }

        public override void Visit(SqlUnary node)
        {
            if (node.NodeType == SqlNodeType.BitNot)
            {
                Visit(SqlDml.BitXor(node.Operand, SqlDml.Literal(Int64.MaxValue)));
                return;
            }
            base.Visit(node);
            using (context.EnterScope(node))
            {
                context.Output.AppendText(translator.Translate(context, node, NodeSection.Entry));
                node.Operand.AcceptVisitor(this);
                context.Output.AppendText(translator.Translate(context, node, NodeSection.Exit));
            }
        }

        public override void Visit(SqlBinary node)
        {
            switch (node.NodeType)
            {
                case SqlNodeType.DateTimePlusInterval:
                    DateTimeAddInterval(node.Left, node.Right).AcceptVisitor(this);
                    return;
                case SqlNodeType.DateTimeMinusInterval:
                    DateTimeAddInterval(node.Left, -node.Right).AcceptVisitor(this);
                    return;
                default:
                    base.Visit(node);
                    return;
            }
        }

        /// <inheritdoc/>
        public override void Visit(SqlFunctionCall node)
        {
            switch (node.FunctionType)
            {
                case SqlFunctionType.Concat:
                    SqlExpression[] exprs = new SqlExpression[node.Arguments.Count];
                    node.Arguments.CopyTo(exprs, 0);
                    Visit(SqlDml.Concat (exprs));
                    return;
                case SqlFunctionType.CharLength:
                    (SqlDml.FunctionCall("DATALENGTH", node.Arguments) / 2).AcceptVisitor(this);
                    return;
                case SqlFunctionType.DateTimeTruncate:
                    Visit(SqlDml.Cast(node.Arguments[0], new SqlValueType("Date")));
                    return;
                case SqlFunctionType.IntervalToMilliseconds:
                    Visit(CastToLong(node.Arguments[0]) / NanosecondsPerMillisecond);
                    return;
                case SqlFunctionType.IntervalConstruct:
                case SqlFunctionType.IntervalToNanoseconds:
                    Visit(CastToLong(node.Arguments[0]));
                    return;
                case SqlFunctionType.DateTimeAddMonths:
                    Visit(DateAddMonth(node.Arguments[0], node.Arguments[1]));
                    return;
                case SqlFunctionType.DateTimeAddYears:
                    Visit(DateAddYear(node.Arguments[0], node.Arguments[1]));
                    return;
                case SqlFunctionType.DateTimeConstruct:
                    Visit(DateAddDay(DateAddMonth(DateAddYear(SqlDml.Literal(new DateTime(2001, 1, 1)),
                      node.Arguments[0] - 2001),
                      node.Arguments[1] - 1),
                      node.Arguments[2] - 1));
                    return;
            }

            base.Visit(node);
        }

        protected virtual SqlExpression DateTimeTruncate(SqlExpression date)
        {
            return DateAddMillisecond(DateAddSecond(DateAddMinute(DateAddHour(date,
              -SqlDml.Extract(SqlDateTimePart.Hour, date)),
              -SqlDml.Extract(SqlDateTimePart.Minute, date)),
              -SqlDml.Extract(SqlDateTimePart.Second, date)),
              -SqlDml.Extract(SqlDateTimePart.Millisecond, date));
        }

        protected virtual SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
        {
            return DateAddMillisecond(
              DateAddDay(date, interval / NanosecondsPerDay),
              (interval / NanosecondsPerMillisecond) % (MillisecondsPerDay));
        }

        #region Static helpers

        private static SqlCast CastToLong(SqlExpression arg)
        {
            return SqlDml.Cast(arg, SqlType.Int64);
        }

        protected static SqlUserFunctionCall DateDiffDay(SqlExpression date1, SqlExpression date2)
        {
            return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("DAY"), date1, date2);
        }

        protected static SqlUserFunctionCall DateDiffMillisecond(SqlExpression date1, SqlExpression date2)
        {
            return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("MILLISECOND"), date1, date2);
        }

        private static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years)
        {
            return SqlDml.FunctionCall("DATEADD", SqlDml.Native("YEAR"), years, date);
        }

        private static SqlUserFunctionCall DateAddMonth(SqlExpression date, SqlExpression months)
        {
            return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MONTH"), months, date);
        }

        protected static SqlUserFunctionCall DateAddDay(SqlExpression date, SqlExpression days)
        {
            return SqlDml.FunctionCall("DATEADD", SqlDml.Native("DAY"), days, date);
        }

        private static SqlUserFunctionCall DateAddHour(SqlExpression date, SqlExpression hours)
        {
            return SqlDml.FunctionCall("DATEADD", SqlDml.Native("HOUR"), hours, date);
        }

        private static SqlUserFunctionCall DateAddMinute(SqlExpression date, SqlExpression minutes)
        {
            return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MINUTE"), minutes, date);
        }

        private static SqlUserFunctionCall DateAddSecond(SqlExpression date, SqlExpression seconds)
        {
            return SqlDml.FunctionCall("DATEADD", SqlDml.Native("SECOND"), seconds, date);
        }

        protected static SqlUserFunctionCall DateAddMillisecond(SqlExpression date, SqlExpression milliseconds)
        {
            return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MILLISECOND"), milliseconds, date);
        }

        #endregion

        protected internal Compiler(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
