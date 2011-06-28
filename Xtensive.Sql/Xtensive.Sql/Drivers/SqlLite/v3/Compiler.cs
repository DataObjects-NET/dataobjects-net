// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.SQLite.v3
{
    internal class Compiler : SqlCompiler
    {
        private static readonly long NanosecondsPerDay = TimeSpan.FromDays(1).Ticks * 100;
        private static readonly long NanosecondsPerSecond = 1000000000;
        private static readonly long NanosecondsPerMillisecond = 1000000;
        private static readonly long MillisecondsPerDay = (long)TimeSpan.FromDays(1).TotalMilliseconds;
        private static readonly long MillisecondsPerSecond = 1000L;

        public override void Visit(SqlSelect node)
        {
            //For hinting limitations see http://www.sqlite.org/lang_indexedby.html
            using (context.EnterScope(node)) {
                context.Output.AppendText(translator.Translate(context, node, SelectSection.Entry));
                VisitSelectColumns(node);
                VisitSelectFrom(node);
                VisitSelectWhere(node);
                VisitSelectGroupBy(node);
                VisitSelectOrderBy(node);
                VisitSelectLimitOffset(node);
                context.Output.AppendText(translator.Translate(context, node, SelectSection.Exit));
            }
        }
        public override void Visit(SqlAggregate node)
        {
            base.Visit(node);
        }

        /// <inheritdoc/>
        public override void Visit(SqlAlterTable node)
        {
            var renameColumnAction = node.Action as SqlRenameColumn;
            if (renameColumnAction != null)
                context.Output.AppendText(((Translator)translator).Translate(context, renameColumnAction));
            else if (node.Action is SqlDropConstraint) {
                using (context.EnterScope(node)) {
                    context.Output.AppendText(translator.Translate(context, node, AlterTableSection.Entry));

                    var action = node.Action as SqlDropConstraint;
                    var constraint = action.Constraint as TableConstraint;
                    context.Output.AppendText(translator.Translate(context, node, AlterTableSection.DropConstraint));
                    if (constraint is ForeignKey)
                        context.Output.AppendText("REFERENCES " + translator.QuoteIdentifier(constraint.DbName));
                    else
                        context.Output.AppendText(translator.Translate(context, constraint, ConstraintSection.Entry));
                    context.Output.AppendText(translator.Translate(context, node, AlterTableSection.DropBehavior));
                    context.Output.AppendText(translator.Translate(context, node, AlterTableSection.Exit));
                }
            }
            else
                base.Visit(node);
        }

        public override void VisitSelectLock(SqlSelect node)
        {
            return;
        }

        /// <inheritdoc/>
        public override void Visit(SqlFunctionCall node)
        {
            switch (node.FunctionType) {
            case SqlFunctionType.CharLength:
                (SqlDml.FunctionCall("LENGTH", node.Arguments) / 2).AcceptVisitor(this);
                return;
            case SqlFunctionType.PadLeft:
            case SqlFunctionType.PadRight:
                return;
            case SqlFunctionType.Concat:
                var nod = node.Arguments[0];
                return;
            case SqlFunctionType.Round:
                // Round should always be called with 2 arguments
                if (node.Arguments.Count == 1) {
                    Visit(SqlDml.FunctionCall(
                      translator.Translate(SqlFunctionType.Round),
                      node.Arguments[0],
                      SqlDml.Literal(0)));
                    return;
                }
                break;
            case SqlFunctionType.Truncate:
                return;
            case SqlFunctionType.Substring:
                if (node.Arguments.Count == 2)
                    Visit(SqlDml.FunctionCall(translator.Translate(SqlFunctionType.Substring), node.Arguments[0], node.Arguments[1]));
                else
                    Visit(SqlDml.FunctionCall(translator.Translate(SqlFunctionType.Substring), node.Arguments[0], node.Arguments[1], node.Arguments[2]));
                return;
            case SqlFunctionType.IntervalToMilliseconds:
                Visit(CastToLong(node.Arguments[0]) / NanosecondsPerMillisecond);
                return;
            case SqlFunctionType.IntervalConstruct:
            case SqlFunctionType.IntervalToNanoseconds:
                Visit(CastToLong(node.Arguments[0]));
                return;
            case SqlFunctionType.DateTimeAddMonths:
                using (context.EnterScope(node)) {
                    context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.Entry, -1));
                    if (node.Arguments.Count > 0) {
                        using (context.EnterCollectionScope()) {
                            int argumentPosition = 0;
                            foreach (SqlExpression item in node.Arguments) {
                                if (!context.IsEmpty)
                                    context.Output.AppendDelimiter(translator.Translate(context, node, FunctionCallSection.ArgumentDelimiter, argumentPosition));
                                context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentEntry, argumentPosition));
                                if (argumentPosition == 1) {
                                    SqlLiteral l = item as SqlLiteral;
                                    if (l != null) {
                                        string value = translator.Translate(context, l.GetValue());
                                        context.Output.AppendText(string.Format("'{0} MONTH'", value));
                                    }
                                }
                                else
                                    item.AcceptVisitor(this);
                                context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentExit, argumentPosition));
                                argumentPosition++;
                            }
                        }
                    }
                    context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.Exit, -1));
                }
                return;
            case SqlFunctionType.DateTimeAddYears:
                using (context.EnterScope(node)) {
                    context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.Entry, -1));
                    if (node.Arguments.Count > 0) {
                        using (context.EnterCollectionScope()) {
                            int argumentPosition = 0;
                            foreach (SqlExpression item in node.Arguments) {
                                if (!context.IsEmpty)
                                    context.Output.AppendDelimiter(translator.Translate(context, node, FunctionCallSection.ArgumentDelimiter, argumentPosition));
                                context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentEntry, argumentPosition));
                                if (argumentPosition == 1) {
                                    SqlLiteral l = item as SqlLiteral;
                                    if (l != null) {
                                        string value = translator.Translate(context, l.GetValue());
                                        context.Output.AppendText(string.Format("'{0} YEARS'", value));
                                    }
                                }
                                else
                                    item.AcceptVisitor(this);
                                context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentExit, argumentPosition));
                                argumentPosition++;
                            }
                        }
                    }
                    context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.Exit, -1));
                }
                return;
            case SqlFunctionType.DateTimeTruncate:
                DateTimeTruncate(node.Arguments[0]).AcceptVisitor(this);
                return;
            case SqlFunctionType.DateTimeConstruct:
                SqlLiteral year = node.Arguments[0] as SqlLiteral;
                SqlLiteral month = node.Arguments[1] as SqlLiteral;
                SqlLiteral day = node.Arguments[2] as SqlLiteral;
                Visit(SqlDml.FunctionCall("DATETIME", string.Format("{0}-{1}-{2}", translator.Translate(context, year.GetValue()), translator.Translate(context, month.GetValue()), translator.Translate(context, day.GetValue()))));
                return;
            }
            base.Visit(node);
        }

        /// <inheritdoc/>
        public override void Visit(SqlFreeTextTable node)
        {
            throw SqlHelper.NotSupported("FreeText");
        }

        /// <inheritdoc/>
        /// //Thanks to Csaba Beer.
        public override void Visit(SqlQueryExpression node)
        {
            using (context.EnterScope(node)) {
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
            using (context.EnterScope(node)) {
                context.Output.AppendText(translator.Translate(context, node, ExtractSection.Entry));
                var part = node.DateTimePart != SqlDateTimePart.Nothing ? translator.Translate(node.DateTimePart) : translator.Translate(node.IntervalPart);
                context.Output.AppendText(translator.Translate(context, node, ExtractSection.From));
                context.Output.AppendText(",");
                node.Operand.AcceptVisitor(this);
                context.Output.AppendText(translator.Translate(context, node, ExtractSection.Exit));
            }
        }

        public override void VisitSelectLimitOffset(SqlSelect node)
        {
            if (!node.Limit.IsNullReference()) {
                context.Output.AppendText(translator.Translate(context, node, SelectSection.Limit));
                context.Output.AppendText(" (");
                node.Limit.AcceptVisitor(this);
                context.Output.AppendText(")");
                return;
            }
            base.VisitSelectLimitOffset(node);
        }

        private SqlExpression DateTimeTruncate(SqlExpression date)
        {
            return SqlDml.FunctionCall("DATE", date);
        }

        private SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
        {
            return SqlDml.Extract(SqlDateTimePart.Second, date1) - SqlDml.Extract(SqlDateTimePart.Second, date2);
        }

        #region Static helpers

        private static SqlCast CastToLong(SqlExpression arg)
        {
            return SqlDml.Cast(arg, SqlType.Int64);
        }

        #endregion

        // Constructors

        /// <param name="driver">The driver.</param>
        public Compiler(SqlDriver driver)
            : base(driver)
        {
        }
    }
}