// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql
{
  /// <summary>
  /// A contract for visitor of SQL DOM query model.
  /// </summary>
  public interface ISqlVisitor
  {
    /// <summary>
    /// Visits <see cref="SqlAggregate"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlAggregate node);

    /// <summary>
    /// Visits <see cref="SqlAlterDomain"/> statements.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlAlterDomain node);

    /// <summary>
    /// Visits <see cref="SqlAlterPartitionFunction"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlAlterPartitionFunction node);

    /// <summary>
    /// Visits <see cref="SqlAlterPartitionScheme"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlAlterPartitionScheme node);

    /// <summary>
    /// Visits <see cref="SqlAlterTable"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlAlterTable node);

    /// <summary>
    /// Visits <see cref="SqlAlterSequence"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlAlterSequence node);

    /// <summary>
    /// Visits <see cref="SqlArray"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlArray node);

    /// <summary>
    /// Visits <see cref="SqlAssignment"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlAssignment node);

    /// <summary>
    /// Visits <see cref="SqlBatch"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlBatch node);

    /// <summary>
    /// Visits <see cref="SqlBetween"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlBetween node);

    /// <summary>
    /// Visits <see cref="SqlBinary"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlBinary node);

    /// <summary>
    /// Visits <see cref="SqlBreak"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlBreak node);

    /// <summary>
    /// Visits <see cref="SqlCase"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlCase node);

    /// <summary>
    /// Visits <see cref="SqlCast"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlCast node);

    /// <summary>
    /// Visits <see cref="SqlCloseCursor"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCloseCursor node);

    /// <summary>
    /// Visits <see cref="SqlCollate"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlCollate node);

    /// <summary>
    /// Visits <see cref="SqlColumnRef"/>.
    /// </summary>
    /// <param name="node">Column reference to visit.</param>
    void Visit(SqlColumnRef node);

    /// <summary>
    /// Visits <see cref="SqlConcat"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlConcat node);

    /// <summary>
    /// Visits <see cref="SqlContainsTable"/> table.
    /// </summary>
    /// <param name="node">Table to visit.</param>
    void Visit(SqlContainsTable node);

    /// <summary>
    /// Visits <see cref="SqlContinue"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlContinue node);

    /// <summary>
    /// Visits <see cref="SqlContainer"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlContainer node);

    /// <summary>
    /// Visits <see cref="SqlCommand"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCommand node);

    /// <summary>
    /// Visits <see cref="SqlComment"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlComment node);

    /// <summary>
    /// Visits <see cref="SqlCreateAssertion"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreateAssertion node);

    /// <summary>
    /// Visits <see cref="SqlCreateCharacterSet"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreateCharacterSet node);

    /// <summary>
    /// Visits <see cref="SqlCreateCollation"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreateCollation node);

    /// <summary>
    /// Visits <see cref="SqlCreateDomain"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreateDomain node);

    /// <summary>
    /// Visits <see cref="SqlCreateIndex"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreateIndex node);

    /// <summary>
    /// Visits <see cref="SqlCreatePartitionFunction"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreatePartitionFunction node);

    /// <summary>
    /// Visits <see cref="SqlCreatePartitionScheme"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreatePartitionScheme node);

    /// <summary>
    /// Visits <see cref="SqlCreateSchema"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreateSchema node);

    /// <summary>
    /// Visits <see cref="SqlCreateSequence"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreateSequence node);

    /// <summary>
    /// Visits <see cref="SqlCreateTable"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreateTable node);
    /// <summary>
    /// Visits <see cref="SqlCreateTranslation"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreateTranslation node);

    /// <summary>
    /// Visits <see cref="SqlCreateView"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlCreateView node);

    /// <summary>
    /// Visits <see cref="SqlCursor"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlCursor node);

    /// <summary>
    /// Visits <see cref="SqlDeclareCursor"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDeclareCursor node);

    /// <summary>
    /// Visits <see cref="SqlDeclareVariable"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDeclareVariable node);

    /// <summary>
    /// Visits <see cref="SqlDefaultValue"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlDefaultValue node);

    /// <summary>
    /// Visits <see cref="SqlDelete"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDelete node);

    /// <summary>
    /// Visits <see cref="SqlDropAssertion"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropAssertion node);

    /// <summary>
    /// Visits <see cref="SqlDropCharacterSet"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropCharacterSet node);

    /// <summary>
    /// Visits <see cref="SqlDropCollation"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropCollation node);

    /// <summary>
    /// Visits <see cref="SqlDropDomain"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropDomain node);

    /// <summary>
    /// Visits <see cref="SqlDropIndex"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropIndex node);

    /// <summary>
    /// Visits <see cref="SqlDropPartitionFunction"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropPartitionFunction node);

    /// <summary>
    /// Visits <see cref="SqlDropPartitionScheme"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropPartitionScheme node);

    /// <summary>
    /// Visits <see cref="SqlDropSchema"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropSchema node);

    /// <summary>
    /// Visits <see cref="SqlDropSequence"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropSequence node);

    /// <summary>
    /// Visits <see cref="SqlDropTable"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropTable node);

    /// <summary>
    /// Visits <see cref="SqlDropTranslation"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropTranslation node);

    /// <summary>
    /// Visits <see cref="SqlDropView"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlDropView node);

    /// <summary>
    /// Visits <see cref="SqlTruncateTable"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlTruncateTable node);

    /// <summary>
    /// Visits <see cref="SqlDynamicFilter"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlDynamicFilter node);

    /// <summary>
    /// Visits <see cref="SqlPlaceholder"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlPlaceholder node);

    /// <summary>
    /// Visits <see cref="SqlExtract"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlExtract node);

    /// <summary>
    /// Visits <see cref="SqlFastFirstRowsHint"/> node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    void Visit(SqlFastFirstRowsHint node);

    /// <summary>
    /// Visits <see cref="SqlFetch"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlFetch node);

    /// <summary>
    /// Visits <see cref="SqlForceJoinOrderHint"/> node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    void Visit(SqlForceJoinOrderHint node);

    /// <summary>
    /// Visits <see cref="SqlFragment"/> node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    void Visit(SqlFragment node);

    /// <summary>
    /// Visits <see cref="SqlFreeTextTable"/> table.
    /// </summary>
    /// <param name="node">Table to visit.</param>
    void Visit(SqlFreeTextTable node);

    /// <summary>
    /// Visits <see cref="SqlFunctionCall"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlFunctionCall node);

    /// <summary>
    /// Visits <see cref="SqlCustomFunctionCall"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlCustomFunctionCall node);

    /// <summary>
    /// Visits <see cref="SqlIf"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlIf node);

    /// <summary>
    /// Visits <see cref="SqlInsert"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlInsert node);

    /// <summary>
    /// Visits <see cref="SqlJoinExpression"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlJoinExpression node);

    /// <summary>
    /// Visits <see cref="SqlJoinHint"/> node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    void Visit(SqlJoinHint node);

    /// <summary>
    /// Visits <see cref="SqlLike"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlLike node);

    /// <summary>
    /// Visits <see cref="SqlLiteral"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlLiteral node);

    /// <summary>
    /// Visits <see cref="SqlMatch"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlMatch node);

    /// <summary>
    /// Visits <see cref="SqlMetadata"/> expression.
    /// </summary>
    /// <param name="node">Expression ot visit.</param>
    void Visit(SqlMetadata node);

    /// <summary>
    /// Visits <see cref="SqlNative"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlNative node);

    /// <summary>
    /// Visits <see cref="SqlNativeHint"/> node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    void Visit(SqlNativeHint node);

    /// <summary>
    /// Visits <see cref="SqlNextValue"/> expression.
    /// </summary>
    /// <param name="value">Expression to visit.</param>
    void Visit(SqlNextValue value);

    /// <summary>
    /// Visits <see cref="SqlNull"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlNull node);

    /// <summary>
    /// Visits <see cref="SqlOpenCursor"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlOpenCursor node);

    /// <summary>
    /// Visits <see cref="SqlOrder"/> node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    void Visit(SqlOrder node);

    /// <summary>
    /// Visits <see cref="SqlParameterRef"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlParameterRef node);

    /// <summary>
    /// Visits <see cref="SqlRound"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlRound node);

    /// <summary>
    /// Visits <see cref="SqlQueryExpression"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlQueryExpression node);

    /// <summary>
    /// Visits <see cref="SqlQueryRef"/> node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    void Visit(SqlQueryRef node);

    /// <summary>
    /// Visits <see cref="SqlRow"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlRow node);

    /// <summary>
    /// Visits <see cref="SqlRowNumber"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlRowNumber node);

    /// <summary>
    /// Visits <see cref="SqlRenameTable"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlRenameTable node);

    /// <summary>
    /// Visits <see cref="SqlSelect"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlSelect node);

    /// <summary>
    /// Visits <see cref="SqlStatementBlock"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlStatementBlock node);

    /// <summary>
    /// Visits <see cref="SqlSubQuery"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlSubQuery node);

    /// <summary>
    /// Visits <see cref="SqlTableColumn"/> node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    void Visit(SqlTableColumn node);

    /// <summary>
    /// Visits <see cref="SqlTableRef"/> node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    void Visit(SqlTableRef node);

    /// <summary>
    /// Visits <see cref="SqlTrim"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlTrim node);

    /// <summary>
    /// Visits <see cref="SqlUnary"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlUnary node);

    /// <summary>
    /// Visits <see cref="SqlUpdate"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlUpdate node);

    /// <summary>
    /// Visits <see cref="SqlUserColumn"/> node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    void Visit(SqlUserColumn node);

    /// <summary>
    /// Visits <see cref="SqlUserFunctionCall"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlUserFunctionCall node);

    /// <summary>
    /// Visits <see cref="SqlVariable"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlVariable node);

    /// <summary>
    /// Visits <see cref="SqlVariant"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    void Visit(SqlVariant node);

    /// <summary>
    /// Visits <see cref="SqlWhile"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    void Visit(SqlWhile node);
  }
}
