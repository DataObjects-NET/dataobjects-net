// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2022.02.03

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.MySql.v5_7
{
  internal class Compiler : v5_6.Compiler
  {
    /// <inheritdoc/>
    public override void Visit(SqlQueryExpression node)
    {
      using (context.EnterScope(node)) {
        var wrapLeft = node.Left is SqlSelect sL
           && (sL.OrderBy.Count > 0 || sL.HasLimit || sL.Lock != SqlLockType.Empty);
        var wrapRight = node.Left is SqlSelect sR
          && (sR.OrderBy.Count > 0 || sR.HasLimit || sR.Lock != SqlLockType.Empty);

        context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Entry));
        if (wrapLeft) {
          context.Output.AppendText("(");
          node.Left.AcceptVisitor(this);
          context.Output.AppendText(")");
        }
        else {
          node.Left.AcceptVisitor(this);
        }

        context.Output.AppendText(translator.Translate(node.NodeType));
        context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.All));

        if (wrapLeft) {
          context.Output.AppendText("(");
          node.Right.AcceptVisitor(this);
          context.Output.AppendText(")");
        }
        else {
          node.Right.AcceptVisitor(this);
        }

        context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Exit));
      }
    }

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
