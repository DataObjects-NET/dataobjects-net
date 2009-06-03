using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Dml;
using System.Text;

namespace Xtensive.Sql.Dom.PgSql.v8_3
{
  public class PgSqlTranslator : v8_2.PgSqlTranslator
  {
    public PgSqlTranslator(SqlDriver driver)
      : base(driver)
    {
    }

    public override string Translate(SqlCompilerContext context, SqlOrder node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          return (node.Ascending) ? "ASC NULLS FIRST" : "DESC NULLS LAST";
      }
      return string.Empty;
    }

    protected override void AppendIndexColumnSortingOrder(StringBuilder builder, bool ascending)
    {
      builder.Append(ascending ? " ASC" : " DESC");
    }
  }
}