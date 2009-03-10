// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Mssql.v2005
{
  public class MssqlTranslator: v2000.MssqlTranslator
  {
    public override string Translate(SqlCompilerContext context, SqlJoinExpression node, JoinSection section)
    {
      if (section == JoinSection.Specification)
        switch (node.JoinType) {
          case SqlJoinType.CrossApply:
            return "CROSS APPLY";
          case SqlJoinType.LeftOuterApply:
            return "OUTER APPLY";
        }

      return base.Translate(context, node, section);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MssqlTranslator"/> class.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected internal MssqlTranslator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}