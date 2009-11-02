using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.PostgreSql.v8_1
{
  internal class Translator : v8_0.Translator
  {
    public Translator(SqlDriver driver)
      : base(driver)
    {
    }

    public override string Translate(SqlCompilerContext context, SqlContinue node)
    {
      return "CONTINUE";
    }
  }
}