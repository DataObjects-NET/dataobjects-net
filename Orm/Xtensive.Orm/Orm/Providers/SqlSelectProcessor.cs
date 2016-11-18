using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using System.Collections.Generic;

namespace Xtensive.Orm.Providers
{
  internal class SqlSelectProcessor : ISqlVisitor
  {
    private readonly SqlSelect rootSelect;
    private readonly ProviderInfo providerInfo;
    private readonly HashSet<SqlExpression> visitedExpressions = new HashSet<SqlExpression>();

    public void Visit(SqlAggregate node)
    {
      if (!node.Expression.IsNullReference())
        Visit(node.Expression);
    }

    public void Visit(SqlAlterDomain node)
    {
    }

    public void Visit(SqlAlterPartitionFunction node)
    {
    }

    public void Visit(SqlAlterPartitionScheme node)
    {
    }

    public void Visit(SqlAlterTable node)
    {
    }

    public void Visit(SqlAlterSequence node)
    {
    }

    public void Visit(SqlArray node)
    {
    }

    public void Visit(SqlAssignment node)
    {
      if (node.Left!=null)
        Visit(node.Left);
      if (!node.Right.IsNullReference())
        Visit(node.Right);
    }

    public void Visit(SqlBatch node)
    {
    }

    public void Visit(SqlBetween node)
    {
      if (!node.Left.IsNullReference())
        Visit(node.Left);
      if (!node.Right.IsNullReference())
        Visit(node.Right);
      if (!node.Expression)
        Visit(node.Expression);
    }

    public void Visit(SqlBinary node)
    {
      if (!node.Left.IsNullReference())
        Visit(node.Left);
      if (!node.Right.IsNullReference())
        Visit(node.Right);
    }

    public void Visit(SqlBreak node)
    {
    }

    public void Visit(SqlCase node)
    {
      if (!node.Value.IsNullReference())
        Visit(node.Value);
      if (!node.Else.IsNullReference())
        Visit(node.Else);
    }

    public void Visit(SqlCast node)
    {
      if (!node.Operand.IsNullReference())
        Visit(node.Operand);
    }

    public void Visit(SqlCloseCursor node)
    {
    }

    public void Visit(SqlCollate node)
    {
      if (!node.Operand.IsNullReference())
        Visit(node.Operand);
    }

    public void Visit(SqlColumnRef node)
    {
      if (!node.SqlColumn.IsNullReference())
        Visit(node.SqlColumn);
    }

    public void Visit(SqlConcat node)
    {
    }

    public void Visit(SqlContainsTable node)
    {
      if (node.TargetTable!=null)
        Visit(node.TargetTable);
      foreach (var column in node.Columns)
        Visit(column);
      foreach (var column in node.TargetColumns)
        Visit(column);
    }

    public void Visit(SqlContinue node)
    {
    }

    public void Visit(SqlContainer node)
    {
    }

    public void Visit(SqlCommand node)
    {
    }

    public void Visit(SqlCreateAssertion node)
    {
    }

    public void Visit(SqlCreateCharacterSet node)
    {
    }

    public void Visit(SqlCreateCollation node)
    {
    }

    public void Visit(SqlCreateDomain node)
    {
    }

    public void Visit(SqlCreateIndex node)
    {
    }

    public void Visit(SqlCreatePartitionFunction node)
    {
    }

    public void Visit(SqlCreatePartitionScheme node)
    {
    }

    public void Visit(SqlCreateSchema node)
    {
    }

    public void Visit(SqlCreateSequence node)
    {
    }

    public void Visit(SqlCreateTable node)
    {
    }

    public void Visit(SqlCreateTranslation node)
    {
    }

    public void Visit(SqlCreateView node)
    {
    }

    public void Visit(SqlCursor node)
    {
    }

    public void Visit(SqlDeclareCursor node)
    {
    }

    public void Visit(SqlDefaultValue node)
    {
    }

    public void Visit(SqlDelete node)
    {
      if (node.Delete!=null)
        Visit(node.Delete);
      if (!node.Where.IsNullReference())
        Visit(node.Where);
    }

    public void Visit(SqlDropAssertion node)
    {
    }

    public void Visit(SqlDropCharacterSet node)
    {
    }

    public void Visit(SqlDropCollation node)
    {
    }

    public void Visit(SqlDropDomain node)
    {
    }

    public void Visit(SqlDropIndex node)
    {
    }

    public void Visit(SqlDropPartitionFunction node)
    {
    }

    public void Visit(SqlDropPartitionScheme node)
    {
    }

    public void Visit(SqlDropSchema node)
    {
    }

    public void Visit(SqlDropSequence node)
    {
    }

    public void Visit(SqlDropTable node)
    {
    }

    public void Visit(SqlDropTranslation node)
    {
    }

    public void Visit(SqlDropView node)
    {
    }

    public void Visit(SqlDynamicFilter node)
    {
    }

    public void Visit(SqlPlaceholder node)
    {
    }

    public void Visit(SqlExtract node)
    {
      if (!node.Operand.IsNullReference())
        Visit(node.Operand);
    }

    public void Visit(SqlFastFirstRowsHint node)
    {
    }

    public void Visit(SqlFetch node)
    {
    }

    public void Visit(SqlForceJoinOrderHint node)
    {
    }

    public void Visit(SqlFreeTextTable node)
    {
      if (node.TargetTable!=null)
        Visit(node.TargetTable);
      foreach (var column in node.Columns)
        Visit(column);
      foreach (var column in node.TargetColumns)
        Visit(column);
    }

    public void Visit(SqlFunctionCall node)
    {
      foreach (var argument in node.Arguments)
        Visit(argument);
    }

    public void Visit(SqlCustomFunctionCall node)
    {
      foreach (var argument in node.Arguments)
        Visit(argument);
    }

    public void Visit(SqlIf node)
    {
      if (node.True!=null)
        Visit(node.True);
      if (node.False!=null)
        Visit(node.False);
      if (!node.Condition.IsNullReference())
        Visit(node.Condition);
    }

    public void Visit(SqlInsert node)
    {
      if (node.From!=null)
        Visit(node.From);
      if (node.Into!=null)
        Visit(node.Into);
      foreach (var value in node.Values.Values)
        Visit(value);
    }

    public void Visit(SqlJoinExpression node)
    {
      if (!node.Expression.IsNullReference())
        Visit(node.Expression);
      if (node.Left!=null)
        Visit(node.Left);
      if (node.Right!=null)
        Visit(node.Right);
    }

    public void Visit(SqlJoinHint node)
    {
    }

    public void Visit(SqlLike node)
    {
      if (!node.Expression.IsNullReference())
        Visit(node.Expression);
      if (!node.Escape.IsNullReference())
        Visit(node.Escape);
      if (!node.Pattern.IsNullReference())
        Visit(node.Pattern);
    }

    public void Visit(SqlLiteral node)
    {
    }

    public void Visit(SqlMatch node)
    {
      if (!node.Value.IsNullReference())
        Visit(node.Value);
      if (!node.SubQuery.IsNullReference())
        Visit(node.SubQuery);
    }

    public void Visit(SqlNative node)
    {
    }

    public void Visit(SqlNativeHint node)
    {
    }

    public void Visit(SqlNextValue value)
    {
    }

    public void Visit(SqlNull node)
    {
    }

    public void Visit(SqlOpenCursor node)
    {
    }

    public void Visit(SqlOrder node)
    {
      if (!node.Expression.IsNullReference())
        Visit(node.Expression);
    }

    public void Visit(SqlParameterRef node)
    {
    }

    public void Visit(SqlRound node)
    {
      if (!node.Argument.IsNullReference())
        Visit(node.Argument);
      if (!node.Length.IsNullReference())
        Visit(node.Length);
    }

    public void Visit(SqlQueryExpression node)
    {
      if (node.Left!=null)
        Visit(node.Left);
      if (node.Right!=null)
        Visit(node.Right);
    }

    public void Visit(SqlQueryRef node)
    {
      foreach (var column in node.Columns)
        Visit(column);
      if (node.Query!=null)
        Visit(node.Query);
    }

    public void Visit(SqlRow node)
    {
    }

    public void Visit(SqlRowNumber node)
    {
      foreach (var order in node.OrderBy)
        Visit(order);
    }

    public void Visit(SqlRenameTable node)
    {
    }

    public void Visit(SqlStatementBlock node)
    {
    }

    public void Visit(SqlTableColumn node)
    {
    }

    public void Visit(SqlTableRef node)
    {
      if (node.DataTable!=null)
        Visit(node.DataTable);
      foreach (var column in node.Columns)
        Visit(column);
    }

    public void Visit(SqlTrim node)
    {
      if (!node.Expression.IsNullReference())
        Visit(node.Expression);
    }

    public void Visit(SqlSelect node)
    {
      foreach (var column in node.Columns)
        Visit(column);
      foreach (var column in node.GroupBy)
        Visit(column);
      foreach (var column in node.OrderBy)
        Visit(column);
      if (node.From!=null)
        Visit(node.From);
      if (!node.Having.IsNullReference())
        Visit(node.Having);
      if (!node.Limit.IsNullReference())
        Visit(node.Limit);
      if (!node.Offset.IsNullReference())
        Visit(node.Offset);
      if (!node.Where.IsNullReference())
        Visit(node.Where);
      foreach (var hint in node.Hints)
        Visit(hint);

      if (node.Columns.Count==0)
        node.Columns.Add(SqlDml.Null, "NULL");

      var hasPaging = node.HasLimit || node.HasOffset;

      var keepOrderBy = ReferenceEquals(node, rootSelect) || hasPaging;
      if (!keepOrderBy)
        node.OrderBy.Clear();

      var addOrderBy = hasPaging
        && node.OrderBy.Count==0
        && providerInfo.Supports(ProviderFeatures.PagingRequiresOrderBy);

      if (addOrderBy)
        node.OrderBy.Add(1);
    }

    public void Visit(SqlSubQuery node)
    {
      if (node.Query!=null)
        Visit(node.Query);
    }

    public void Visit(SqlUnary node)
    {
      if (!node.Operand.IsNullReference())
        Visit(node.Operand);
    }

    public void Visit(SqlUpdate node)
    {
      if (node.From!=null)
        Visit(node.From);
      if (node.Update!=null)
        Visit(node.Update);
      if (!node.Where.IsNullReference())
        Visit(node.Where);
      foreach (var value in node.Values.Values)
        Visit(value);
      foreach (var hint in node.Hints)
        Visit(hint);
    }

    public void Visit(SqlUserColumn node)
    {
      if (!node.Expression.IsNullReference())
        Visit(node.Expression);
    }

    public void Visit(SqlUserFunctionCall node)
    {
      foreach (var argument in node.Arguments)
        Visit(argument);
    }

    public void Visit(SqlDeclareVariable node)
    {
    }

    public void Visit(SqlVariable node)
    {
    }

    public void Visit(SqlVariant node)
    {
    }

    public void Visit(SqlWhile node)
    {
      if (!node.Condition.IsNullReference())
        Visit(node.Condition);
      if (node.Statement!=null)
        Visit(node.Statement);
    }

    public void Visit(SqlFragment node)
    {
    }

    public void Visit(SqlExpression sqlExpression)
    {
      if (visitedExpressions.Contains(sqlExpression))
        return;
      visitedExpressions.Add(sqlExpression);
      sqlExpression.AcceptVisitor(this);
    }

    public void Visit(SqlStatement sqlStatement)
    {
      sqlStatement.AcceptVisitor(this);
    }

    public void Visit(SqlTable sqlTable)
    {
      sqlTable.AcceptVisitor(this);
    }

    private void Visit(ISqlQueryExpression queryExpression)
    {
      queryExpression.AcceptVisitor(this);
    }

    private void Visit(ISqlLValue sqlLValue)
    {
      sqlLValue.AcceptVisitor(this);
    }

    private void Visit(DataTable dataTable)
    {
    }

    private void Visit(SqlHint sqlExpression)
    {
    }

    public static void Process(SqlSelect select, ProviderInfo providerInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(select, "select");
      ArgumentValidator.EnsureArgumentNotNull(providerInfo, "providerInfo");
      new SqlSelectProcessor(select, providerInfo).Visit(select);
    }

    // Constructors

    private SqlSelectProcessor(SqlSelect rootSelect, ProviderInfo providerInfo)
    {
      this.rootSelect = rootSelect;
      this.providerInfo = providerInfo;
    }
  }
}