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
        return MakeTime(arguments[0], arguments[1], arguments[2], arguments[3], true);
      }
      else if (arguments.Count == 1) {
        var ticks = arguments[0];
        if (SqlHelper.IsTimeSpanTicks(ticks, out var sourceInterval)) {
          // try to optimize and reduce calculations when TimeSpan.Ticks where used for TimeOnly(ticks) ctor
          return SqlDml.Cast(SqlDml.Cast(sourceInterval, SqlType.VarChar), SqlType.Time);
        }
        else {
          var hour = SqlDml.Cast(ticks / 36000000000, SqlType.Int32);
          var minute = SqlDml.Cast((ticks / 600000000) % 60, SqlType.Int32);
          var second = SqlDml.Cast((ticks / 10000000) % 60, SqlType.Int32);
          var microsecond = SqlDml.Cast((ticks % 10000000) / 10, SqlType.Int32);
          return MakeTime(hour, minute, second, microsecond, false);
        }
      }
      else {
        throw new InvalidOperationException("Unsupported count of parameters");
      }
    }
#endif

    protected static SqlUserFunctionCall MakeDateTime(SqlExpression year, SqlExpression month, SqlExpression day) =>
      SqlDml.FunctionCall("MAKE_TIMESTAMP", year, month, day, SqlDml.Literal(0), SqlDml.Literal(0), SqlDml.Literal(0.0));
#if NET6_0_OR_GREATER

    protected static SqlUserFunctionCall MakeDate(SqlExpression year, SqlExpression month, SqlExpression day) =>
      SqlDml.FunctionCall("MAKE_DATE", year, month, day);

    protected static SqlUserFunctionCall MakeTime(
       SqlExpression hours, SqlExpression minutes, SqlExpression seconds, SqlExpression secondFractions, in bool isMilliseconds) =>
      (isMilliseconds) 
        ? SqlDml.FunctionCall("MAKE_TIME", hours, minutes, seconds + (SqlDml.Cast(secondFractions, SqlType.Double) / 1000))
        : SqlDml.FunctionCall("MAKE_TIME", hours, minutes, seconds + (SqlDml.Cast(secondFractions, SqlType.Double) / 1000000));
#endif

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    } 
  }
}