// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.Firebird.v4_0
{
  internal class Translator : v2_5.Translator
  {
    public override void Translate(SqlCompilerContext context, SqlJoinExpression node, JoinSection section)
    {
      var output = context.Output;
      switch (section) {
        case JoinSection.Specification: {
          if (node.Expression == null) {
            switch (node.JoinType) {
              case SqlJoinType.CrossApply:
                output.Append("CROSS JOIN LATERAL");
                break;
              case SqlJoinType.LeftOuterApply:
                output.Append("LEFT JOIN LATERAL");
                break;
              default:
                base.Translate(context, node, section);
                break;
            }
            return;
          }
          output.Append(Translate(node.JoinType) + " JOIN");
          return;
        }
        case JoinSection.Exit: {
          if (node.JoinType == SqlJoinType.LeftOuterApply) {
            output.Append("ON TRUE");
          }
          return;
        }
      }
      base.Translate(context, node, section);
    }

    // Constructors

    /// <inheritdoc/>
    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
