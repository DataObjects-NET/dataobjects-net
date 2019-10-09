using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.BulkOperations
{
  internal class ComputedExpressionSqlVisitor : BaseSqlVisitor
  {
    private readonly SqlTable from;
    private readonly SqlTable to;

    public override void Visit(SqlTableColumn node)
    {
      if (node.SqlTable==from)
        node.ReplaceWith(SqlDml.TableColumn(to, node.Name));
    }

    public ComputedExpressionSqlVisitor(SqlTable from, SqlTable to)
    {
      this.from = from;
      this.to = to;
    }
  }
}