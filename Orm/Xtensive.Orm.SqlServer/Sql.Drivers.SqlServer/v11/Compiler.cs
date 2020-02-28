// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.02

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.SqlServer.v11
{
  internal class Compiler : v10.Compiler
  {
    public override void Visit(SqlSelect node)
    {
      VisitSelectDefault(node);
    }

    public override void VisitSelectLimitOffset(SqlSelect node)
    {
      // FETCH NEXT n ROWS ONLY does not work without OFFSET n ROWS
      // Provide zero offset if no offset was specified by user.

      if (!node.HasOffset && !node.HasLimit)
        return; // Nothing to process.

      context.Output.AppendText(translator.Translate(context, node, SelectSection.Offset));
      if (node.HasOffset)
        node.Offset.AcceptVisitor(this);
      else
        context.Output.AppendText("0");
      context.Output.AppendText(translator.Translate(context, node, SelectSection.OffsetEnd));

      if (node.HasLimit) {
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Limit));
        node.Limit.AcceptVisitor(this);
        context.Output.AppendText(translator.Translate(context, node, SelectSection.LimitEnd));
      }
    }

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}