// Copyright (C) 2012-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.04.02

using System;
using System.Collections.Generic;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.SqlServer.v11
{
  internal class Compiler : v10.Compiler
  {
    /// <inheritdoc/>
    public override void Visit(SqlSelect node) => VisitSelectDefault(node);

    /// <inheritdoc/>
    protected override void VisitSelectLimitOffset(SqlSelect node)
    {
      // FETCH NEXT n ROWS ONLY does not work without OFFSET n ROWS
      // Provide zero offset if no offset was specified by user.

      if (!node.HasOffset && !node.HasLimit) {
        return; // Nothing to process.
      }

      translator.SelectOffset(context, node);

      if (node.HasOffset) {
        node.Offset.AcceptVisitor(this);
      }
      else {
        _ = context.Output.Append("0");
      }

      translator.SelectOffsetEnd(context, node);

      if (node.HasLimit) {
        translator.SelectLimit(context, node);
        node.Limit.AcceptVisitor(this);
        translator.SelectLimitEnd(context, node);
      }
    }

    protected override SqlUserFunctionCall ConstructDateTime(IReadOnlyList<SqlExpression> arguments) =>
      SqlDml.FunctionCall("DATETIME2FROMPARTS", arguments[0], arguments[1], arguments[2], 0, 0, 0, 0, 7);

    protected override SqlUserFunctionCall ConstructDate(IReadOnlyList<SqlExpression> arguments) =>
      SqlDml.FunctionCall("DATEFROMPARTS", arguments[0], arguments[1], arguments[2]);

    protected override SqlExpression ConstructTime(IReadOnlyList<SqlExpression> arguments)
    {
      SqlExpression hour, minute, second, microsecond;
      if (arguments.Count == 4) {
        // argument[3] * 10000 operation is based on statement that millisaconds use 3 digits
        // default precision of time is 7, and if we use raw argument[3] value the result will be .0000xxx,
        // to prevent this and make fractions part valid .xxx0000 we multiply
        hour = arguments[0];
        minute = arguments[1];
        second = arguments[2];
        microsecond = arguments[3] * 10000;
      }
      else if (arguments.Count == 1) {
        var ticks = arguments[0];
        // try to optimize and reduce calculations when TimeSpan.Ticks where used for TimeOnly(ticks) ctor
        ticks = SqlHelper.IsTimeSpanTicks(ticks, out var sourceInterval) ? sourceInterval / 100 : ticks;
        hour = SqlDml.Cast(ticks / 36000000000, SqlType.Int32);
        minute = SqlDml.Cast((ticks / 600000000) % 60, SqlType.Int32);
        second = SqlDml.Cast((ticks / 10000000) % 60, SqlType.Int32);
        microsecond = SqlDml.Cast(ticks % 10000000, SqlType.Int32);
      }
      else {
        throw new InvalidOperationException("Unsupported count of parameters");
      }
      return SqlDml.FunctionCall("TIMEFROMPARTS", hour, minute, second, microsecond, 7);
    }

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}