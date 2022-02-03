// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.Firebird.v4_0
{
  internal class Translator : v2_5.Translator
  {
    public override string Translate(SqlCompilerContext context, SqlJoinExpression node, JoinSection section)
    {
      switch (section) {
        case JoinSection.Specification: {
          if (node.Expression == null) {
            switch (node.JoinType) {
              case SqlJoinType.CrossApply:
                return "CROSS JOIN LATERAL";
              case SqlJoinType.LeftOuterApply:
                return "LEFT JOIN LATERAL";
              default:
                return base.Translate(context, node, section);
            }
          }
          return Translate(node.JoinType) + " JOIN";
        }
        case JoinSection.Exit: {
          if (node.JoinType == SqlJoinType.LeftOuterApply) {
            return "ON TRUE";
          }
          return string.Empty;
        }
      }
      return base.Translate(context, node, section);
    }

    // Constructors

    /// <inheritdoc/>
    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
