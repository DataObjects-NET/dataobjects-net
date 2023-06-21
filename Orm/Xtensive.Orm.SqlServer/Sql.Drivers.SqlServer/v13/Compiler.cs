// Copyright (C) 2018-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2018.09.21

using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.SqlServer.v13
{
  internal class Compiler : v12.Compiler
  {
    protected const string MicrosecondPart = "MCS";
    protected const long NanosecondsPerMicrosecond = 1000;

#if NET6_0_OR_GREATER //DO_DATEONLY
    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
        case SqlFunctionType.DateTimeOffsetTimeOfDay:
          DateTimeOffsetTimeOfDay(node.Arguments[0]).AcceptVisitor(this);
          break;
        case SqlFunctionType.IntervalToMilliseconds: {
          if (node.Arguments[0] is SqlBinary binary 
              && (binary.NodeType is SqlNodeType.DateTimeMinusDateTime or SqlNodeType.DateTimeOffsetMinusDateTimeOffset or SqlNodeType.TimeMinusTime)) {
            Visit(DateDiffBigMicrosecond(binary.Right, binary.Left) / CastToLong(1000));
          }
          else {
            base.Visit(node);
          }
          break;
        }
        case SqlFunctionType.IntervalToNanoseconds: {
          if (node.Arguments[0] is SqlBinary binary) {
            if (binary.NodeType is SqlNodeType.DateTimeMinusDateTime or SqlNodeType.DateTimeOffsetMinusDateTimeOffset) {
              // we have to use time consuming algorithm here because
              // DATEDIFF_BIG can throw arithmetic overflow on nanoseconds
              // so we should handle it by this big formula
              Visit(CastToLong(DateTimeSubtractDateTimeExpensive(binary.Left, binary.Right)));
            }
            else if (binary.NodeType is SqlNodeType.TimeMinusTime) {
              //but for time it is OK
              Visit(DateDiffBigMicrosecond(binary.Right, binary.Left));
            }
            else {
              base.Visit(node);
            }
          }
          else {
            base.Visit(node);
          }
          break;
        }
        default:
          base.Visit(node); break;
      }
    }
#else
    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
        case SqlFunctionType.DateTimeOffsetTimeOfDay:
          DateTimeOffsetTimeOfDay(node.Arguments[0]).AcceptVisitor(this);
          break;
        case SqlFunctionType.IntervalToMilliseconds: {
          if (node.Arguments[0] is SqlBinary binary 
              && (binary.NodeType == SqlNodeType.DateTimeMinusDateTime || binary.NodeType == SqlNodeType.DateTimeOffsetMinusDateTimeOffset)) {
            Visit(DateDiffBigMicrosecond(binary.Right, binary.Left) / CastToLong(1000));
          }
          else {
            base.Visit(node);
          }
          break;
        }
        case SqlFunctionType.IntervalToNanoseconds: {
          if (node.Arguments[0] is SqlBinary binary
              && (binary.NodeType == SqlNodeType.DateTimeMinusDateTime || binary.NodeType == SqlNodeType.DateTimeOffsetMinusDateTimeOffset)) {
            // we have to use time consuming algorithm here because
            // DATEDIFF_BIG can throw arithmetic overflow on nanoseconds
            // so we should handle it by this big formula
            Visit(CastToLong(DateTimeSubtractDateTimeExpensive(binary.Left, binary.Right)));
          }
          else {
            base.Visit(node);
          }
          break;
        }
        default:
          base.Visit(node); break;
      }
    }
#endif

    protected override SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return CastToDecimal(DateDiffBigMicrosecond(date2, date1), 18, 0) * CastToLong(1000);
    }

    private SqlExpression DateTimeSubtractDateTimeExpensive(SqlExpression date1, SqlExpression date2)
    {
      return CastToDecimal(DateDiffBigDay(date2, date1), 18, 0) * NanosecondsPerDay
          + CastToDecimal(DateDiffBigMillisecond(DateAddDay(date2, DateDiffBigDay(date2, date1)), date1), 18, 0) * NanosecondsPerMillisecond;
    }

#region Static Helpers

    protected static SqlExpression DateTimeOffsetTimeOfDay(SqlExpression dateTimeOffset) =>
      DateDiffBigNanosecond(
        SqlDml.Native(ZeroTime),
        SqlDml.Cast(dateTimeOffset, new SqlValueType("time")));

    protected static SqlUserFunctionCall DateDiffBigNanosecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF_BIG", SqlDml.Native(NanosecondPart), date1, date2);

    protected static SqlUserFunctionCall DateDiffBigMicrosecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF_BIG", SqlDml.Native(MicrosecondPart), date1, date2);

    protected static SqlUserFunctionCall DateDiffBigMillisecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF_BIG", SqlDml.Native(MillisecondPart), date1, date2);

    protected static SqlUserFunctionCall DateDiffBigDay(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF_BIG", SqlDml.Native(DayPart), date1, date2);

#endregion

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}