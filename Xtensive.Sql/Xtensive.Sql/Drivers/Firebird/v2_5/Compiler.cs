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
                bool needOpeningParenthesis = false;
                bool needClosingParenthesis = false;

                //string ltype = node.Left.GetType().FullName;
                //string rtype = node.Right.GetType().FullName;
                //System.Diagnostics.Debug.WriteLine("nodeType:" + node.NodeType + ", " + node.GetType().FullName);
                //System.Diagnostics.Debug.WriteLine("   left:" + node.Left.NodeType + ", " + ltype);
                //System.Diagnostics.Debug.WriteLine("   right:" + node.Right.NodeType + ", " + rtype);
                //System.Diagnostics.Debug.WriteLine("   traversalPath:");
                //foreach (var e in context.GetTraversalPath())
                //    System.Diagnostics.Debug.WriteLine("      " + e.NodeType + ", " + e.GetType().FullName);
                //System.Diagnostics.Debug.Flush();

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



        protected internal Compiler(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
