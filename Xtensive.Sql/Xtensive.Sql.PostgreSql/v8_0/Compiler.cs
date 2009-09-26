using System;
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
        SqlHelper.GenericPad(node).AcceptVisitor(this);
        return;
      case SqlFunctionType.Rand:
        SqlDml.FunctionCall(translator.Translate(SqlFunctionType.Rand)).AcceptVisitor(this);
        return;
      case SqlFunctionType.Square:
        SqlDml.Power(node.Arguments[0], 2).AcceptVisitor(this);
        return;
      case SqlFunctionType.IntervalConstruct:
        (node.Arguments[0] * OneMillisecondInterval).AcceptVisitor(this);
        return;
      case SqlFunctionType.IntervalToMilliseconds:
        SqlHelper.IntervalToMilliseconds(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.IntervalAbs:
        SqlHelper.IntervalAbs(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeConstruct:
        var newNode = (SqlDml.Literal(new DateTime(2001, 1, 1))
          + OneYearInterval * (node.Arguments[0] - 2001)
          + OneMonthInterval * (node.Arguments[1] - 1)
          + OneDayInterval * (node.Arguments[2] - 1));
        newNode.AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeTruncate:
        (SqlDml.FunctionCall("date_trunc", "day", node.Arguments[0])).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeAddMonths:
        (node.Arguments[0] + node.Arguments[1] * OneMonthInterval).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeAddYears:
        (node.Arguments[0] + node.Arguments[1] * OneYearInterval).AcceptVisitor(this);
        return;
      }
      base.Visit(node);
    }

    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}