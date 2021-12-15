// Copyright (C) 2018-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2018.09.21

using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.SqlServer.v13
{
  internal class Compiler : v12.Compiler
  {
    protected const string DayPart = "DAY";
    protected const string MillisecondPart = "MS";
    protected const string NanosecondPart = "NS";
    protected const string ZeroTime = "'00:00:00.0000000'";

    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      if (node.FunctionType == SqlFunctionType.DateTimeOffsetTimeOfDay) {
        DateTimeOffsetTimeOfDay(node.Arguments[0]).AcceptVisitor(this);
      }
      else {
        base.Visit(node);
      }
    }

    protected override SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return DateDiffBigNanosecond(date2, date1);
    }

    #region Static Helpers

    protected static SqlUserFunctionCall DateDiffBigNanosecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF_BIG", SqlDml.Native(NanosecondPart), date1, date2);

    private static SqlExpression DateTimeOffsetTimeOfDay(SqlExpression dateTimeOffset) =>
      DateDiffBigNanosecond(
        SqlDml.Native(ZeroTime),
        SqlDml.Cast(dateTimeOffset, new SqlValueType("time")));

    #endregion

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}