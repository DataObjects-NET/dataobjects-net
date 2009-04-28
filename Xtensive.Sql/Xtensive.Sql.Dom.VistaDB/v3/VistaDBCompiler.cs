// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Ddl;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.VistaDB.v3
{
  public class VistaDBCompiler : SqlCompiler
  {
    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      if (node.FunctionType == SqlFunctionType.Substring && node.Arguments.Count == 2) {
        SqlExpression len = Sql.Length(node.Arguments[0]);
        node.Arguments.Add(len);
        base.Visit(node);
        if (node.Arguments.Contains(len))
          node.Arguments.Remove(len);
        return;
      }
      base.Visit(node);
    }

    public override void Visit(SqlAlterTable node)
    {
      if (!(node.Action is SqlRenameAction)) {
        base.Visit(node);
        return;
      }
      SqlRenameAction action = node.Action as SqlRenameAction;
      TableColumn column = action.Node as TableColumn;
      if (column != null)
        context.AppendText(translator.Translate(context, column, action));
      else
        context.AppendText(translator.Translate(context, node.Table, action));
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="driver">The driver.</param>
    public VistaDBCompiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}