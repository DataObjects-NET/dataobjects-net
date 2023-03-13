// Copyright (C) 2019-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.25

using System;
using System.Collections.Generic;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.PostgreSql.v10_0
{
  internal class Compiler : v9_1.Compiler
  {
    protected override SqlUserFunctionCall ConstructDateTime(IReadOnlyList<SqlExpression> arguments) => MakeDateTime(arguments[0], arguments[1], arguments[2]);
#if NET6_0_OR_GREATER

    protected override SqlUserFunctionCall ConstructDate(IReadOnlyList<SqlExpression> arguments) => MakeDate(arguments[0], arguments[1], arguments[2]);

    protected override SqlExpression ConstructTime(IReadOnlyList<SqlExpression> arguments)
    {
      if (arguments.Count == 4) {
        return MakeTime(arguments[0], arguments[1], arguments[2], arguments[3]);
      }
      else {
        return base.ConstructTime(arguments);
      }
    }
#endif

    protected static SqlUserFunctionCall MakeDateTime(SqlExpression year, SqlExpression month, SqlExpression day) =>
      SqlDml.FunctionCall("MAKE_TIMESTAMP", year, month, day, SqlDml.Literal(0), SqlDml.Literal(0), SqlDml.Literal(0.0));
#if NET6_0_OR_GREATER

    protected static SqlUserFunctionCall MakeDate(SqlExpression year, SqlExpression month, SqlExpression day) =>
      SqlDml.FunctionCall("MAKE_DATE", year, month, day);

    protected static SqlUserFunctionCall MakeTime(
       SqlExpression hours, SqlExpression minutes, SqlExpression seconds, SqlExpression milliseconds) =>
     SqlDml.FunctionCall("MAKE_TIME", hours, minutes, seconds + (SqlDml.Cast(milliseconds, SqlType.Double) / 1000));
#endif

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    } 
  }
}