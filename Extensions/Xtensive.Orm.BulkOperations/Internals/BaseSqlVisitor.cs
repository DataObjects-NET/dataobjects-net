using System.Collections.Generic;
using Xtensive.Sql;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.BulkOperations
{
  internal abstract class BaseSqlVisitor : ISqlVisitor
  {
    private readonly List<ISqlNode> nodes = new List<ISqlNode>();

    public virtual void Visit(SqlAggregate node)
    {
      VisitInternal(node.Expression);
    }

    public virtual void Visit(SqlAlterDomain node)
    {
      VisitInternal(node.Action);
    }

    public virtual void Visit(SqlAlterPartitionFunction node)
    {
    }

    public virtual void Visit(SqlAlterPartitionScheme node)
    {
    }

    public virtual void Visit(SqlAlterTable node)
    {
      VisitInternal(node.Action);
    }

    public virtual void Visit(SqlAlterSequence node)
    {
    }

    public virtual void Visit(SqlArray node)
    {
    }

    public virtual void Visit(SqlAssignment node)
    {
      VisitInternal(node.Left);
      VisitInternal(node.Right);
    }

    public virtual void Visit(SqlBatch node)
    {
      foreach (SqlStatement statement in node)
        VisitInternal(statement);
    }

    public virtual void Visit(SqlBetween node)
    {
      VisitInternal(node.Expression);
      VisitInternal(node.Left);
      VisitInternal(node.Right);
    }

    public virtual void Visit(SqlBinary node)
    {
      VisitInternal(node.Left);
      VisitInternal(node.Right);
    }

    public virtual void Visit(SqlBreak node)
    {
    }

    public virtual void Visit(SqlCase node)
    {
      VisitInternal(node.Else);
      VisitInternal(node.Value);
      foreach (KeyValuePair<SqlExpression, SqlExpression> pair in node) {
        VisitInternal(pair.Key);
        VisitInternal(pair.Value);
      }
    }

    public virtual void Visit(SqlCast node)
    {
      VisitInternal(node.Operand);
    }

    public virtual void Visit(SqlCloseCursor node)
    {
      VisitInternal(node.Cursor);
    }

    public virtual void Visit(SqlCollate node)
    {
      VisitInternal(node.Operand);
    }

    public virtual void Visit(SqlColumnRef node)
    {
      VisitInternal(node.SqlColumn);
      VisitInternal(node.SqlTable);
    }

    public virtual void Visit(SqlConcat node)
    {
      foreach (SqlExpression expression in node)
        VisitInternal(expression);
    }

    public virtual void Visit(SqlContinue node)
    {
    }

    public virtual void Visit(SqlContainer node)
    {
    }

    public virtual void Visit(SqlCommand node)
    {
    }

    public virtual void Visit(SqlContainsTable node)
    {
      VisitInternal(node.Asterisk);
      foreach (SqlTableColumn column in node.Columns)
        VisitInternal(column);
      VisitInternal(node.SearchCondition);
      foreach (SqlTableColumn column in node.TargetColumns)
        VisitInternal(column);
      VisitInternal(node.TargetTable);
    }

    public virtual void Visit(SqlCreateAssertion node)
    {
    }

    public virtual void Visit(SqlCreateCharacterSet node)
    {
    }

    public virtual void Visit(SqlCreateCollation node)
    {
    }

    public virtual void Visit(SqlCreateDomain node)
    {
    }

    public virtual void Visit(SqlCreateIndex node)
    {
    }

    public virtual void Visit(SqlCreatePartitionFunction node)
    {
    }

    public virtual void Visit(SqlCreatePartitionScheme node)
    {
    }

    public virtual void Visit(SqlCreateSchema node)
    {
    }

    public virtual void Visit(SqlCreateSequence node)
    {
    }

    public virtual void Visit(SqlCreateTable node)
    {
    }

    public virtual void Visit(SqlCreateTranslation node)
    {
    }

    public virtual void Visit(SqlCreateView node)
    {
    }

    public virtual void Visit(SqlCursor node)
    {
      VisitInternal(node.Query);
      foreach (SqlColumn column in node.Columns)
        VisitInternal(column);
    }

    public virtual void Visit(SqlDeclareCursor node)
    {
      VisitInternal(node.Cursor);
    }

    public virtual void Visit(SqlDefaultValue node)
    {
    }

    public virtual void Visit(SqlDelete node)
    {
      VisitInternal(node.From);
      VisitInternal(node.Where);
    }

    public virtual void Visit(SqlDropAssertion node)
    {
    }

    public virtual void Visit(SqlDropCharacterSet node)
    {
    }

    public virtual void Visit(SqlDropCollation node)
    {
    }

    public virtual void Visit(SqlDropDomain node)
    {
    }

    public virtual void Visit(SqlDropIndex node)
    {
    }

    public virtual void Visit(SqlDropPartitionFunction node)
    {
    }

    public virtual void Visit(SqlDropPartitionScheme node)
    {
    }

    public virtual void Visit(SqlDropSchema node)
    {
    }

    public virtual void Visit(SqlDropSequence node)
    {
    }

    public virtual void Visit(SqlDropTable node)
    {
    }

    public virtual void Visit(SqlDropTranslation node)
    {
    }

    public virtual void Visit(SqlDropView node)
    {
    }

    public virtual void Visit(SqlDynamicFilter node)
    {
      foreach (SqlExpression expression in node.Expressions)
        VisitInternal(expression);
    }

    public virtual void Visit(SqlPlaceholder node)
    {
    }

    public virtual void Visit(SqlExtract node)
    {
      VisitInternal(node.Operand);
    }

    public virtual void Visit(SqlFastFirstRowsHint node)
    {
    }

    public virtual void Visit(SqlFetch node)
    {
      VisitInternal(node.Cursor);
      VisitInternal(node.RowCount);
      foreach (ISqlCursorFetchTarget target in node.Targets)
        VisitInternal(target);
    }

    public virtual void Visit(SqlForceJoinOrderHint node)
    {
      foreach (SqlTable table in node.Tables)
        VisitInternal(table);
    }

    public virtual void Visit(SqlFreeTextTable node)
    {
      VisitInternal(node.Asterisk);
      foreach (SqlTableColumn column in node.Columns)
        VisitInternal(column);
      VisitInternal(node.FreeText);
      foreach (SqlTableColumn column in node.TargetColumns)
        VisitInternal(column);
      VisitInternal(node.TargetTable);
    }

    public virtual void Visit(SqlFunctionCall node)
    {
      foreach (SqlExpression expression in node.Arguments)
        VisitInternal(expression);
    }

    public virtual void Visit(SqlCustomFunctionCall node)
    {
      foreach (SqlExpression expression in node.Arguments)
        VisitInternal(expression);
    }

    public virtual void Visit(SqlIf node)
    {
      VisitInternal(node.Condition);
      VisitInternal(node.False);
      VisitInternal(node.True);
    }

    public virtual void Visit(SqlInsert node)
    {
      VisitInternal(node.From);
      VisitInternal(node.Into);
      foreach (var pair in node.Values) {
        VisitInternal(pair.Key);
        VisitInternal(pair.Value);
      }
    }

    public virtual void Visit(SqlJoinExpression node)
    {
      VisitInternal(node.Expression);
      VisitInternal(node.Left);
      VisitInternal(node.Right);
    }

    public virtual void Visit(SqlJoinHint node)
    {
      VisitInternal(node.Table);
    }

    public virtual void Visit(SqlLike node)
    {
      VisitInternal(node.Escape);
      VisitInternal(node.Expression);
      VisitInternal(node.Pattern);
    }

    public virtual void Visit(SqlLiteral node)
    {
    }

    public virtual void Visit(SqlMatch node)
    {
      VisitInternal(node.SubQuery);
      VisitInternal(node.Value);
    }

    public virtual void Visit(SqlNative node)
    {
    }

    public virtual void Visit(SqlNativeHint node)
    {
    }

    public virtual void Visit(SqlNextValue value)
    {
    }

    public virtual void Visit(SqlNull node)
    {
    }

    public virtual void Visit(SqlOpenCursor node)
    {
      VisitInternal(node.Cursor);
    }

    public virtual void Visit(SqlOrder node)
    {
      VisitInternal(node.Expression);
    }

    public virtual void Visit(SqlParameterRef node)
    {
    }

    public virtual void Visit(SqlRound node)
    {
      VisitInternal(node.Argument);
      VisitInternal(node.Length);
    }

    public virtual void Visit(SqlQueryExpression node)
    {
      VisitInternal(node.Left);
      VisitInternal(node.Right);
      foreach (ISqlQueryExpression expression in node)
        VisitInternal(expression);
    }

    public virtual void Visit(SqlQueryRef node)
    {
      VisitInternal(node.Asterisk);
      foreach (SqlTableColumn column in node.Columns)
        VisitInternal(column);
      VisitInternal(node.Query);
      foreach (SqlTable table in node)
        VisitInternal(table);
    }

    public virtual void Visit(SqlRow node)
    {
      foreach (SqlExpression expression in node)
        VisitInternal(expression);
    }

    public virtual void Visit(SqlRowNumber node)
    {
      foreach (SqlOrder order in node.OrderBy)
        VisitInternal(order);
    }

    public virtual void Visit(SqlRenameTable node)
    {
    }

    public virtual void Visit(SqlStatementBlock node)
    {
      foreach (SqlStatement statement in node)
        VisitInternal(statement);
    }

    public virtual void Visit(SqlTableColumn node)
    {
      VisitInternal(node.SqlTable);
    }

    public virtual void Visit(SqlTableRef node)
    {
      VisitInternal(node.Asterisk);
      foreach (SqlTableColumn column in node.Columns)
        VisitInternal(column);
    }

    public virtual void Visit(SqlTrim node)
    {
      VisitInternal(node.Expression);
    }

    public virtual void Visit(SqlSelect node)
    {
      VisitInternal(node.Asterisk);
      foreach (SqlColumn column in node.Columns)
        VisitInternal(column);
      VisitInternal(node.From);
      foreach (SqlColumn column in node.GroupBy)
        VisitInternal(column);
      VisitInternal(node.Having);
      foreach (SqlHint hint in node.Hints)
        VisitInternal(hint);
      VisitInternal(node.Limit);
      VisitInternal(node.Offset);
      foreach (SqlOrder order in node.OrderBy)
        VisitInternal(order);
      VisitInternal(node.Where);
    }

    public virtual void Visit(SqlSubQuery node)
    {
      VisitInternal(node.Query);
    }

    public virtual void Visit(SqlUnary node)
    {
      VisitInternal(node.Operand);
    }

    public virtual void Visit(SqlUpdate node)
    {
      VisitInternal(node.From);
      foreach (SqlHint hint in node.Hints)
        VisitInternal(hint);
      VisitInternal(node.Update);
      foreach (var pair in node.Values) {
        VisitInternal(pair.Key);
        VisitInternal(pair.Value);
      }
      VisitInternal(node.Where);
    }

    public virtual void Visit(SqlUserColumn node)
    {
      VisitInternal(node.Expression);
      VisitInternal(node.SqlTable);
    }

    public virtual void Visit(SqlUserFunctionCall node)
    {
      foreach (SqlExpression argument in node.Arguments)
        VisitInternal(argument);
    }

    public virtual void Visit(SqlDeclareVariable node)
    {
      VisitInternal(node.Variable);
    }

    public virtual void Visit(SqlVariable node)
    {
    }

    public virtual void Visit(SqlVariant node)
    {
      VisitInternal(node.Alternative);
      VisitInternal(node.Main);
    }

    public virtual void Visit(SqlWhile node)
    {
      VisitInternal(node.Condition);
      VisitInternal(node.Statement);
    }


    public virtual void Visit(SqlFragment node)
    {
      VisitInternal(node.Expression);
    }

    public virtual void Visit(SqlComment node)
    {
    }
    
    #region Non-public methods

    private void VisitInternal(ISqlNode node)
    {
      if (nodes.FindIndex(a => ReferenceEquals(a, node)) > -1)
        return;
      nodes.Add(node);
      if (node!=null)
        node.AcceptVisitor(this);
    }

    #endregion
  }
}