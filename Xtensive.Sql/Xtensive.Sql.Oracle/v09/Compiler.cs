// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using Xtensive.Core.Collections;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Oracle.v09
{
  internal class Compiler : SqlCompiler
  {
    public override void VisitSelectFrom(SqlSelect node)
    {
      if (node.From!=null)
        base.VisitSelectFrom(node);
      else
        context.AppendText("FROM DUAL");
    }

    public override void Visit(SqlJoinHint node)
    {
      var method = translator.Translate(node.Method);
      if (string.IsNullOrEmpty(method))
        return;
      context.AppendText(method);
      context.AppendText("(");
      node.Table.AcceptVisitor(this);
      context.AppendText(")");
    }

    public override void Visit(SqlFastFirstRowsHint node)
    {
      context.AppendText(string.Format("FIRST_ROWS({0})", node.Amount));
    }

    public override void Visit(SqlNativeHint node)
    {
      context.AppendText(node.HintText);
    }

    public override void Visit(SqlForceJoinOrderHint node)
    {
      if (node.Tables.IsNullOrEmpty()) 
        context.AppendText("ORDERED");
      else {
        context.AppendText("LEADING(");
        using (context.EnterCollection())
          foreach (var table in node.Tables)
            table.AcceptVisitor(this);
        context.AppendText(")");
      }
    }

    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}