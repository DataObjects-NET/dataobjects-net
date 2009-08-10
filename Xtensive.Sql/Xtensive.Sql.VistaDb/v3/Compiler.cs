// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.VistaDb.v3
{
  internal class Compiler : SqlCompiler
  {
    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      if (node.FunctionType==SqlFunctionType.Substring && node.Arguments.Count == 2) {
        SqlExpression len = SqlDml.CharLength(node.Arguments[0]);
        node.Arguments.Add(len);
        base.Visit(node);
        if (node.Arguments.Contains(len))
          node.Arguments.Remove(len);
        return;
      }
      base.Visit(node);
    }

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}