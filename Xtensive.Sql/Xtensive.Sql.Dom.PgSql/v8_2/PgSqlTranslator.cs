using System;
using System.Diagnostics;
using System.Text;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.PgSql.v8_2
{
  public class PgSqlTranslator : v8_1.PgSqlTranslator
  {
    public PgSqlTranslator(SqlDriver driver)
      : base(driver)
    {
    }

    [DebuggerStepThrough]
    public override string QuoteString(string str)
    {
      return "E'" + str.Replace("'", "''").Replace(@"\", @"\\") + "'";
    }

    protected override void AppendIndexStorageParameters(StringBuilder builder, Index index)
    {
      if (index.FillFactor!=null)
        builder.AppendFormat("WITH(FILLFACTOR={0})", index.FillFactor);
    }

    public override string Translate(SqlFunctionType type)
    {
      switch (type) {

        //date

      case SqlFunctionType.CurrentDate:
      case SqlFunctionType.CurrentTime:
      case SqlFunctionType.CurrentTimeStamp:
        return "clock_timestamp";

      default:
        return base.Translate(type);
      }
    }

  }
}