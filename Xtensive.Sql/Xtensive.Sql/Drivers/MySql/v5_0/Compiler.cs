// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.Drivers.MySql.Resources;

namespace Xtensive.Sql.MySql.v5_0
{
    internal class Compiler : SqlCompiler
    {

        protected static readonly long NanosecondsPerDay = TimeSpan.FromDays(1).Ticks * 100;
        protected static readonly long NanosecondsPerSecond = 1000000000;
        protected static readonly long NanosecondsPerMillisecond = 1000000;
        protected static readonly long NanosecondsPerMicrosecond = 1000;
        protected static readonly long MillisecondsPerDay = (long)TimeSpan.FromDays(1).TotalMilliseconds;
        protected static readonly long MillisecondsPerSecond = 1000L;

        /// <inheritdoc/>
        public override void Visit(SqlSelect node)
        {
            using (context.EnterScope(node))
            {
                context.Output.AppendText(translator.Translate(context, node, SelectSection.Entry));
                VisitSelectColumns(node);
                VisitSelectFrom(node);
                VisitSelectHints(node);
                VisitSelectWhere(node);
                VisitSelectGroupBy(node);
                VisitSelectOrderBy(node);
                VisitSelectLimitOffset(node);
                VisitSelectLock(node);
                context.Output.AppendText(translator.Translate(context, node, SelectSection.Exit));
            }
        }

        /// <inheritdoc/>
        public override void Visit(SqlAlterTable node)
        {
            var renameColumnAction = node.Action as SqlRenameColumn;
            if (renameColumnAction != null)
                context.Output.AppendText(((Translator)translator).Translate(context, renameColumnAction));
            else
                base.Visit(node);
        }

        /// <inheritdoc/>
        public override void Visit(SqlFreeTextTable node)
        {
            //mysql> CREATE TABLE articles (
            //    ->   id INT UNSIGNED AUTO_INCREMENT NOT NULL PRIMARY KEY,
            //    ->   title VARCHAR(200),
            //    ->   body TEXT,
            //    ->   FULLTEXT (title,body)
            //    -> );

            //mysql> SELECT * FROM articles
            //    -> WHERE MATCH (title,body)
            //    -> AGAINST ('database' IN NATURAL LANGUAGE MODE);

            throw SqlHelper.NotSupported("Will Implement");
        }

        /// <inheritdoc/>
        public override void Visit(SqlBinary node)
        {
            switch (node.NodeType)
            {
                case SqlNodeType.DateTimePlusInterval:
                    DateTimeAddInterval(node.Left, node.Right).AcceptVisitor(this);
                    return;
                case SqlNodeType.DateTimeMinusDateTime:
                    DateTimeSubtractDateTime(node.Left, node.Right).AcceptVisitor(this);
                    return;
                case SqlNodeType.DateTimeMinusInterval:
                    DateTimeAddInterval(node.Left, -node.Right).AcceptVisitor(this);
                    return;
                default:
                    base.Visit(node);
                    return;
            }
        }

        protected virtual SqlExpression DateTimeTruncate(SqlExpression date)
        {
            return DateAddMicrosecond(DateAddSecond(DateAddMinute(DateAddHour(date,
              -SqlDml.Extract(SqlDateTimePart.Hour, date)),
              -SqlDml.Extract(SqlDateTimePart.Minute, date)),
              -SqlDml.Extract(SqlDateTimePart.Second, date)),
              -SqlDml.Extract(SqlDateTimePart.Millisecond, date));
        }

        protected virtual SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
        {
            return CastToLong(DateDiffDay(date2, date1)) * NanosecondsPerDay
                + CastToLong(DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1)) * NanosecondsPerMillisecond;
        }

        protected virtual SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
        {
            return DateAddMicrosecond(
              DateAddDay(date, interval / NanosecondsPerDay),
              (interval / NanosecondsPerMillisecond * NanosecondsPerMicrosecond) % (MillisecondsPerDay * NanosecondsPerMicrosecond));
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override void Visit(SqlFunctionCall node)
        {
            switch (node.FunctionType)
            {
                case SqlFunctionType.Concat:
                    SqlExpression[] exprs = new SqlExpression[node.Arguments.Count];
                    node.Arguments.CopyTo(exprs, 0);
                    Visit(SqlDml.Concat(exprs));
                    return;
                case SqlFunctionType.Position:
                    Position(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
                    return;
                case SqlFunctionType.CharLength:
                    SqlDml.CharLength(node.Arguments[0]).AcceptVisitor(this);
                    return;
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
                case SqlFunctionType.Substring:
                    if (node.Arguments.Count == 2)
                    {
                        node = SqlDml.Substring(node.Arguments[0], node.Arguments[1]);
                        SqlExpression len = SqlDml.CharLength(node.Arguments[0]);
                        node.Arguments.Add(len);
                        Visit(node);
                        return;
                    }
                    break;
                case SqlFunctionType.IntervalToMilliseconds:
                    Visit(CastToLong(node.Arguments[0])/NanosecondsPerMillisecond);
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
                case SqlFunctionType.DateTimeTruncate:
                    DateTimeTruncate(node.Arguments[0]).AcceptVisitor(this);
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

        
        #region Static helpers

        private static SqlExpression Position(SqlExpression substring, SqlExpression _string)
        {
            return SqlDml.FunctionCall("LOCATE", _string, substring) - 1;
        }

        private static SqlCast CastToLong(SqlExpression arg)
        {
            return SqlDml.Cast(arg, SqlType.Int64);
        }

        protected static SqlUserFunctionCall DateDiffDay(SqlExpression date1, SqlExpression date2)
        {
            return SqlDml.FunctionCall("DATEDIFF", date1, date2);
        }

        protected static SqlUserFunctionCall DateDiffMillisecond(SqlExpression date1, SqlExpression date2)
        {
            return SqlDml.FunctionCall("MICROSECOND", SqlDml.FunctionCall("DATEDIFF", date1, date2) * NanosecondsPerMillisecond);
        }

        private static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years)
        {
            return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("YEAR"), years, date);
        }

        private static SqlUserFunctionCall DateAddMonth(SqlExpression date, SqlExpression months)
        {
            return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("MONTH"), months, date);
        }

        protected static SqlUserFunctionCall DateAddDay(SqlExpression date, SqlExpression days)
        {
            return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("DAY"), days, date);
        }

        private static SqlUserFunctionCall DateAddHour(SqlExpression date, SqlExpression hours)
        {
            return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("HOUR"), hours, date);
        }

        private static SqlUserFunctionCall DateAddMinute(SqlExpression date, SqlExpression minutes)
        {
            return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("MINUTE"), minutes, date);
        }

        private static SqlUserFunctionCall DateAddSecond(SqlExpression date, SqlExpression seconds)
        {
            return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("SECOND"), seconds, date);
        }

        protected static SqlUserFunctionCall DateAddMicrosecond(SqlExpression date, SqlExpression microseconds)
        {
            return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("MICROSECOND"), microseconds, date);
        }

        #endregion


        // Constructors

        protected internal Compiler(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
