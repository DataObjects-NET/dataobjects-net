using System;
using System.Text;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.PostgreSql.v8_0
{
  internal class Compiler : SqlCompiler
  {
    private static readonly SqlNative OneYearInterval = SqlDml.Native("interval '1 year'");
    private static readonly SqlNative OneMonthInterval = SqlDml.Native("interval '1 month'");
    private static readonly SqlNative OneDayInterval = SqlDml.Native("interval '1 day'");
    private static readonly SqlNative OneMillisecondInterval = SqlDml.Native("interval '0.001 second'");

    public override void Visit(SqlDeclareCursor node)
    {

    }

    public override void Visit(SqlOpenCursor node)
    {
      base.Visit(node.Cursor.Declare());
    }

    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
        case SqlFunctionType.Square:
          Visit(SqlDml.Power(node.Arguments[0], 2));
          return;

        case SqlFunctionType.Extract:
          if (Extract(node))
            return;
          break;

        case SqlFunctionType.IntervalExtract:
          if (IntervalExtract(node))
            return;
          break;

        case SqlFunctionType.IntervalConstruct:
          Visit(node.Arguments[0] * OneMillisecondInterval);
          return;

        case SqlFunctionType.IntervalToMilliseconds:
          Visit(IntervalToMilliseconds(node.Arguments[0]));
          return;

        case SqlFunctionType.IntervalDuration:
          Visit(IntervalDuration(node.Arguments[0]));
          return;

        case SqlFunctionType.DateTimeConstruct:
          Visit(SqlDml.Literal(new DateTime(2001, 1, 1))
            + OneYearInterval * (node.Arguments[0] - 2001)
            + OneMonthInterval * (node.Arguments[1] - 1)
            + OneDayInterval * (node.Arguments[2] - 1));
          return;

        case SqlFunctionType.DateTimeTruncate:
          Visit(SqlDml.FunctionCall("date_trunc", "day", node.Arguments[0]));
          return;

        case SqlFunctionType.DateTimeAddInterval:
          Visit(node.Arguments[0] + node.Arguments[1]);
          return;

        case SqlFunctionType.DateTimeSubtractInterval:
        case SqlFunctionType.DateTimeSubtractDateTime:
          Visit(node.Arguments[0] - node.Arguments[1]);
          return;

        case SqlFunctionType.DateTimeAddMonths:
          Visit(node.Arguments[0] + node.Arguments[1] * OneMonthInterval);
          return;

        case SqlFunctionType.DateTimeAddYears:
          Visit(node.Arguments[0] + node.Arguments[1] * OneYearInterval);
          return;
      }

      base.Visit(node);
    }

    private SqlCase IntervalDuration(SqlExpression source)
    {
      var result = SqlDml.Case();
      result.Add(source > SqlDml.Literal(new TimeSpan(0)), source);
      result.Else = -source;
      return result;
    }

    private bool Extract(SqlFunctionCall node)
    {
      var part = ((SqlLiteral<SqlDateTimePart>)node.Arguments[0]).Value;
      var arg = node.Arguments[1];

      if (part == SqlDateTimePart.Second) {
        Visit(CastToLong(RealExtractSeconds(arg)));
        return true;
      }

      if (part == SqlDateTimePart.Millisecond) {
        Visit(CastToLong(RealExtractMilliseconds(arg)) % 1000);
        return true;
      }

      return false;
    }

    public bool IntervalExtract(SqlFunctionCall node)
    {
      var part = ((SqlLiteral<SqlIntervalPart>)node.Arguments[0]).Value;
      var arg = node.Arguments[1]; 
     
      if (part == SqlIntervalPart.Day) {
        Visit(RealExtractDays(arg));
        return true;
      }

      if (part == SqlIntervalPart.Second) {
        Visit(CastToLong(RealExtractSeconds(arg)));
        return true;
      }

      if (part == SqlIntervalPart.Millisecond) {
        Visit(CastToLong(RealExtractMilliseconds(arg)) % 1000);
        return true;
      }

      return false;
    }

    #region Static helpers

    protected static SqlCast CastToLong(SqlExpression arg)
    {
      return SqlDml.Cast(arg, SqlType.Int64);
    }

    protected static SqlUserFunctionCall RealExtractDays(SqlExpression arg)
    {
      return SqlDml.FunctionCall(Translator.RealExtractDays, arg);
    }

    protected static SqlUserFunctionCall RealExtractSeconds(SqlExpression arg)
    {
      return SqlDml.FunctionCall(Translator.RealExtractSeconds, arg);
    }

    protected static SqlUserFunctionCall RealExtractMilliseconds(SqlExpression arg)
    {
      return SqlDml.FunctionCall(Translator.RealExtractMilliseconds, arg);
    }

    protected static SqlBinary IntervalToMilliseconds(SqlExpression interval)
    {
      var days = RealExtractDays(interval);
      var hours = SqlDml.IntervalExtract(SqlIntervalPart.Hour, interval);
      var minutes = SqlDml.IntervalExtract(SqlIntervalPart.Minute, interval);
      var milliseconds = CastToLong(RealExtractMilliseconds(interval));

      return ((days * 24L + hours) * 60L + minutes) * 60L * 1000L + milliseconds;
    }

    #endregion

    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}