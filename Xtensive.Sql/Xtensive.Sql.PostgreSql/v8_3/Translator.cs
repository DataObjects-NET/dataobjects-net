using System.Text;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.PostgreSql.v8_3
{
  internal class Translator : v8_2.Translator
  {
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

    // Constuctors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}