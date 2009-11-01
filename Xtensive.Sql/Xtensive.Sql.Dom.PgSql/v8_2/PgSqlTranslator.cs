using System.Diagnostics;
using System.Text;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.PgSql.v8_2
{
  public class PgSqlTranslator : v8_1.PgSqlTranslator
  {
    public PgSqlTranslator(SqlDriver driver)
      : base(driver)
    {
    }

    [DebuggerHidden]
    public override string QuoteString(string str)
    {
      return "E'" + str.Replace("'", "''").Replace(@"\", @"\\") + "'";
    }

    protected override void AppendIndexStorageParameters(StringBuilder builder, Index index)
    {
      if (index.FillFactor!=null)
        builder.AppendFormat("WITH(FILLFACTOR={0})", index.FillFactor);
    }
  }
}