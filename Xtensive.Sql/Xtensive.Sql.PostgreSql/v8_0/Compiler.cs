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
      case SqlFunctionType.PadLeft:
      case SqlFunctionType.PadRight:
        Visit(GenericPad(node));
        return;
      case SqlFunctionType.Rand:
        Visit(SqlDml.FunctionCall(translator.Translate(SqlFunctionType.Rand)));
        return;
      case SqlFunctionType.Square:
        Visit(SqlDml.Power(node.Arguments[0], 2));
        return;
      case SqlFunctionType.IntervalConstruct:
        Visit(node.Arguments[0] * OneMillisecondInterval);
        return;
      case SqlFunctionType.IntervalToMilliseconds:
        Visit(IntervalToMilliseconds(node.Arguments[0]));
        return;
      case SqlFunctionType.IntervalAbs:
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

    private SqlCase GenericPad(SqlFunctionCall node)
    {
      string paddingFunction;
      switch (node.FunctionType) {
      case SqlFunctionType.PadLeft:
        paddingFunction = "lpad";
        break;
      case SqlFunctionType.PadRight:
        paddingFunction = "rpad";
        break;
      default:
        throw new InvalidOperationException();
      }
      var operand = node.Arguments[0];
      var result = SqlDml.Case();
      result.Add(
        SqlDml.CharLength(operand) < node.Arguments[1],
        SqlDml.FunctionCall(paddingFunction, node.Arguments));
      result.Else = operand;
      return result;
    }

    private SqlBinary IntervalToMilliseconds(SqlExpression interval)
    {
      var days = SqlDml.Extract(SqlIntervalPart.Day, interval);
      var hours = SqlDml.Extract(SqlIntervalPart.Hour, interval);
      var minutes = SqlDml.Extract(SqlIntervalPart.Minute, interval);
      var seconds = SqlDml.Extract(SqlIntervalPart.Second, interval);
      var milliseconds = SqlDml.Extract(SqlIntervalPart.Millisecond, interval);

      return (((days * 24L + hours) * 60L + minutes) * 60L + seconds) * 1000L + milliseconds;
    }

    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}