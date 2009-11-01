using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.PgSql.v8_0
{
  internal class PgSqlCompiler : SqlCompiler
  {
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

    /*
    public override void Visit(SqlQueryExpression node)
    {
      using(this.context.EnterNode(node))
      {
        this.context.AppendText(this.translator.Translate(this.context, node, NodeSection.Entry));

        this.context.AppendText("(");
        node.Left.AcceptVisitor(this);
        this.context.AppendText(")");

        this.context.AppendText(this.translator.Translate(node.NodeType));

        this.context.AppendText("(");
        node.Right.AcceptVisitor(this);
        this.context.AppendText(")");

        this.context.AppendText(this.translator.Translate(this.context, node, NodeSection.Exit));
      }
    }
    /**/
  }
}