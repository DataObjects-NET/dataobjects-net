// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2022.02.03

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.MySql.v8_0
{
  internal class Compiler : v5_7.Compiler
  {
    /// <inheritdoc/>
    public override void Visit(SqlQueryExpression node)
    {
      using (context.EnterScope(node)) {
        bool needOpeningParenthesis = true;
        bool needClosingParenthesis = true;

        AppendTranslatedEntry(node);
        _ = context.Output.Append("(");
        node.Left.AcceptVisitor(this);
        _ = context.Output.Append(")");
        AppendTranslated(node.NodeType);
        AppendTranslated(node, QueryExpressionSection.All);
        _ = context.Output.Append("(");
        node.Right.AcceptVisitor(this);
        _ = context.Output.Append(")");
        AppendTranslatedExit(node);
      }
    }


    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
