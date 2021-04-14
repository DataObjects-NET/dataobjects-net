// Copyright (C) 2019-2021 Xtensive LLC.
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
    public override string Translate(SqlCompilerContext context, SqlJoinExpression node, JoinSection section)
    {
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
                return "CROSS JOIN LATERAL";
              case SqlJoinType.LeftOuterApply:
                return "LEFT JOIN LATERAL";
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

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}