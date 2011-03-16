// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.MySql.v5
{
    internal class Compiler : SqlCompiler
    {
        private static readonly SqlNative OneYearInterval = SqlDml.Native("interval '1 year'");
        private static readonly SqlNative OneMonthInterval = SqlDml.Native("interval '1 month'");
        private static readonly SqlNative OneDayInterval = SqlDml.Native("interval '1 day'");
        private static readonly SqlNative OneMillisecondInterval = SqlDml.Native("interval '0.001 second'");


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
            if (!right.IsNullReference() && (node.NodeType == SqlNodeType.In || node.NodeType == SqlNodeType.NotIn))
            {
                var row = SqlDml.Row(right.GetValues().Select(value => SqlDml.Literal(value)).ToArray());
                base.Visit(node.NodeType == SqlNodeType.In ? SqlDml.In(node.Left, row) : SqlDml.NotIn(node.Left, row));
            }
            else
                base.Visit(node);
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
                    SqlDml.Coalesce(SqlDml.FunctionCall("LENGTH", node.Arguments[0]), 0).AcceptVisitor(this);
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
                case SqlFunctionType.IntervalConstruct:
                    (node.Arguments[0] * OneMillisecondInterval).AcceptVisitor(this);
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
                //case SqlFunctionType.DateTimeTruncate:
                //    (SqlDml.FunctionCall("date_trunc", "day", node.Arguments[0])).AcceptVisitor(this);
                //    return;
                case SqlFunctionType.DateTimeAddMonths:
                    (node.Arguments[0] + node.Arguments[1] * OneMonthInterval).AcceptVisitor(this);
                    return;
                case SqlFunctionType.DateTimeAddYears:
                    (node.Arguments[0] + node.Arguments[1] * OneYearInterval).AcceptVisitor(this);
                    return;
            }
            base.Visit(node);
        }


        private static SqlExpression Position(SqlExpression substring, SqlExpression _string)
        {
            return SqlDml.FunctionCall("LOCATE", _string, substring) - 1;
        }

        // Constructors

        protected internal Compiler(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
