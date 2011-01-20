// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.17

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
    internal class Compiler : SqlCompiler
    {

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
                context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Entry));
                if (node.Left is SqlQueryExpression) context.Output.AppendText("(");
                node.Left.AcceptVisitor(this);
                if (node.Left is SqlQueryExpression) context.Output.AppendText(")");
                context.Output.AppendText(translator.Translate(node.NodeType));
                context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.All));
                if (node.Right is SqlQueryExpression) context.Output.AppendText("(");
                node.Right.AcceptVisitor(this);
                if (node.Right is SqlQueryExpression) context.Output.AppendText(")");
                context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Exit));
            }
        }



        protected internal Compiler(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
