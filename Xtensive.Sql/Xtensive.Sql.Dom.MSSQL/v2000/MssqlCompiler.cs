// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Mssql.v2000
{
  public class MssqlCompiler : SqlCompiler
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

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="driver">The driver.</param>
    public MssqlCompiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}