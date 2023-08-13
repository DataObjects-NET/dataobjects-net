// Copyright (C) 2013-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2013.12.30

using System;
using System.Collections.Generic;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.MySql.v5_6
{
  internal class Compiler : v5_5.Compiler
  {
#if NET6_0_OR_GREATER
    public override void Visit(SqlBinary node)
    {
      if (node.NodeType == SqlNodeType.TimePlusInterval) {
        SqlDml.FunctionCall("TIME", TimeAddInterval(SqlDml.Cast(node.Left, SqlType.DateTime), node.Right)).AcceptVisitor(this);
      }
      else {
        base.Visit(node);
      }
    }

    public override void Visit(SqlFunctionCall node)
    {
      var arguments = node.Arguments;
      switch (node.FunctionType) {
        case SqlFunctionType.TimeAddHours:
          Visit(
            SqlDml.FunctionCall("TIME",
              SqlDml.FunctionCall("ADDTIME",
                SqlDml.Cast(arguments[0], SqlType.DateTime),
                arguments[1] * 10000))); // 10000 = 1:00:00
          return;
        case SqlFunctionType.TimeAddMinutes:
          Visit(
            SqlDml.FunctionCall("TIME",
              SqlDml.FunctionCall("ADDTIME",
                SqlDml.Cast(arguments[0], SqlType.DateTime),
                arguments[1] * 100))); // 100 = 0:01:00
          return;
        case SqlFunctionType.TimeToDateTime:
          Visit(SqlDml.Cast(arguments[0], SqlType.DateTime));
          return;
        default:
          base.Visit(node);
          return;
      }
    }

    protected override SqlUserFunctionCall TimeAddInterval(SqlExpression time, SqlExpression interval)
    {
      var timeAsDate = SqlDml.Cast(time, SqlType.DateTime);
      return DateTimeAddMicrosecond(timeAsDate,
        (interval / NanosecondsPerMillisecond * NanosecondsPerMicrosecond) % (MillisecondsPerDay * NanosecondsPerMicrosecond));
    }

    protected override SqlBinary TimeSubtractTime(SqlExpression time1, SqlExpression time2) =>
      SqlDml.Modulo(
        NanosecondsPerDay + CastToDecimal(DateTimeDiffMicrosecond(time2, time1), 18, 0) * NanosecondsPerMicrosecond,
        NanosecondsPerDay);
#endif

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
