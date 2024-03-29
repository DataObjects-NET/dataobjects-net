// Copyright (C) 2019-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.25

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.PostgreSql.v10_0
{
  internal class Translator : v9_1.Translator
  {
    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlJoinExpression node, JoinSection section)
    {
      var output = context.Output;
      switch (section) {
        case JoinSection.Specification: {
          if (node.Expression == null) {
            switch (node.JoinType) {
              case SqlJoinType.InnerJoin:
              case SqlJoinType.LeftOuterJoin:
              case SqlJoinType.RightOuterJoin:
              case SqlJoinType.FullOuterJoin:
                throw new NotSupportedException();
              case SqlJoinType.CrossApply:
                _ = output.Append("CROSS JOIN LATERAL");
                return;
              case SqlJoinType.LeftOuterApply:
                _ = output.Append("LEFT JOIN LATERAL");
                return;
            }
          }
          Translate(output, node.JoinType);
          _ = output.Append(" JOIN");
          break;
        }
        case JoinSection.Exit: {
          if (node.JoinType == SqlJoinType.LeftOuterApply) {
            _ = output.Append("ON TRUE");
          }
          break;
        }
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}