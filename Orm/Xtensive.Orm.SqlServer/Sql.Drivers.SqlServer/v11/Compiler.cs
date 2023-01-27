// Copyright (C) 2012-2022 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.02

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

      AppendTranslated(node, SelectSection.Offset);

      if (node.HasOffset) {
        node.Offset.AcceptVisitor(this);
      }
      else {
        _ = context.Output.Append("0");
      }

      AppendSpaceIfNecessary();
      translator.Translate(context, node, SelectSection.OffsetEnd);

      if (node.HasLimit) {
        AppendTranslated(node, SelectSection.Limit);
        node.Limit.AcceptVisitor(this);
        AppendSpaceIfNecessary();
        translator.Translate(context, node, SelectSection.LimitEnd);
      }
    }

    public override void Visit(SqlFunctionCall node)
    {
      var arguments = node.Arguments;
      switch (node.FunctionType) {
        case SqlFunctionType.DateTimeConstruct:
          Visit(SqlDml.FunctionCall("DATETIME2FROMPARTS", arguments[0], arguments[1], arguments[2], 0, 0, 0, 0, 7));
          //Visit(DateAddDay(DateAddMonth(DateAddYear(SqlDml.Literal(new DateTime(2001, 1, 1)),
          //  arguments[0] - 2001),
          //  arguments[1] - 1),
          //  arguments[2] - 1));
          return;
#if NET6_0_OR_GREATER
        case SqlFunctionType.DateConstruct: {
          Visit(SqlDml.FunctionCall("DATEFROMPARTS", arguments[0], arguments[1], arguments[2]));

          //Visit(SqlDml.Cast(DateAddDay(DateAddMonth(DateAddYear(SqlDml.Literal(new DateOnly(2001, 1, 1)),
          //  arguments[0] - 2001),
          //  arguments[1] - 1),
          //  arguments[2] - 1), SqlType.Date));
          return;
        }
        case SqlFunctionType.TimeConstruct: {
          // argument[3] * 10000 operation is based on statement that millisaconds use 3 digits
          // default precision of time is 7, and if we use raw argument[3] value the result will be .0000xxx,
          // to prevent this and make fractions part valid .xxx0000 we multiply
          Visit(SqlDml.FunctionCall("TIMEFROMPARTS", arguments[0], arguments[1], arguments[2], arguments[3] * 10000, 7));
          //Visit(SqlDml.Cast(DateAddMillisecond(DateAddSecond(DateAddMinute(DateAddHour(SqlDml.Literal(new TimeOnly(0, 0, 0)),
          //  arguments[0]),
          //  arguments[1]),
          //  arguments[2]),
          //  arguments[3]), SqlType.Time));
          return;
        }
#endif
      }

      base.Visit(node);
    }

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}