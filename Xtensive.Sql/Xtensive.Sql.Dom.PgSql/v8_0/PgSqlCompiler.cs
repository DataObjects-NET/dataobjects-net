using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Sql.Common;

namespace Xtensive.Sql.Dom.PgSql.v8_0
{
  internal class PgSqlCompiler : SqlCompiler
  {
    private static readonly SqlUserFunctionCall Timestamp20010101 =
      Sql.FunctionCall(PgSqlTranslator.Timestamp20010101);

    private static readonly SqlUserFunctionCall OneYearInterval =
      Sql.FunctionCall(PgSqlTranslator.OneYearInterval);

    private static readonly SqlUserFunctionCall OneMonthInterval =
      Sql.FunctionCall(PgSqlTranslator.OneMonthInterval);

    private static readonly SqlUserFunctionCall OneDayInterval =
      Sql.FunctionCall(PgSqlTranslator.OneDayInterval);

    private static readonly SqlUserFunctionCall OneMillisecondInterval =
      Sql.FunctionCall(PgSqlTranslator.OneMillisecondInterval);

    protected internal PgSqlCompiler(PgSqlDriver driver)
      : base(driver)
    {
    }

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
          Visit(Sql.Power(node.Arguments[0], 2));
          return;

        case SqlFunctionType.Extract:
          if (VisitExtract(node))
            return;
          break;

        case SqlFunctionType.IntervalExtract:
          if (VisitIntervalExtract(node))
            return;
          break;

        case SqlFunctionType.IntervalConstruct:
          Visit(node.Arguments[0] * OneMillisecondInterval);
          return;

        case SqlFunctionType.IntervalToMilliseconds:
          Visit(IntervalToMilliseconds(node.Arguments[0]));
          return;

        case SqlFunctionType.DateTimeConstruct:
          Visit(Timestamp20010101
            + OneYearInterval * (node.Arguments[0] - 2001)
            + OneMonthInterval * (node.Arguments[1] - 1)
            + OneDayInterval * (node.Arguments[2] - 1));
          return;

        case SqlFunctionType.DateTimeTruncate:
          Visit(Sql.FunctionCall("date_trunc", "day", node.Arguments[0]));
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

    private bool VisitExtract(SqlFunctionCall node)
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

    public bool VisitIntervalExtract(SqlFunctionCall node)
    {
      var part = ((SqlLiteral<SqlIntervalPart>)node.Arguments[0]).Value;
      var arg = node.Arguments[1]; 
     
      if (part == SqlIntervalPart.Day) {
        Visit(RealExtractDays(Sql.FunctionCall("justify_hours", arg)));
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

    private static SqlCast CastToLong(SqlExpression arg)
    {
      return Sql.Cast(arg, SqlDataType.Int64);
    }

    private static SqlUserFunctionCall RealExtractDays(SqlExpression arg)
    {
      return Sql.FunctionCall(PgSqlTranslator.RealExtractDays, arg);
    }

    private static SqlUserFunctionCall RealExtractSeconds(SqlExpression arg)
    {
      return Sql.FunctionCall(PgSqlTranslator.RealExtractSeconds, arg);
    }

    private static SqlUserFunctionCall RealExtractMilliseconds(SqlExpression arg)
    {
      return Sql.FunctionCall(PgSqlTranslator.RealExtractMilliseconds, arg);
    }

    private static SqlBinary IntervalToMilliseconds(SqlExpression interval)
    {
      var days = RealExtractDays(interval);
      var hours = Sql.IntervalExtract(SqlIntervalPart.Hour, interval);
      var minutes = Sql.IntervalExtract(SqlIntervalPart.Minute, interval);
      var milliseconds = Sql.Cast(RealExtractMilliseconds(interval), SqlDataType.Int64);

      return ((days * 24L + hours) * 60L + minutes) * 60L * 1000L + milliseconds;
    }
  }
}