using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.PgSql.v8_1
{
  public class PgSqlTranslator : v8_0.PgSqlTranslator
  {
    public PgSqlTranslator(SqlDriver driver)
      : base(driver)
    {
    }

    public override string Translate(SqlCompilerContext context, SqlContinue node)
    {
      return "CONTINUE";
    }
  }
}