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
    void Visit(SqlAggregate node);
    void Visit(SqlAlterDomain node);
    void Visit(SqlAlterPartitionFunction node);
    void Visit(SqlAlterPartitionScheme node);
    void Visit(SqlAlterTable node);
    void Visit(SqlAlterSequence node);
    void Visit(SqlArray node);
    void Visit(SqlAssignment node);
    void Visit(SqlBatch node);
    void Visit(SqlBetween node);
    void Visit(SqlBinary node);
    void Visit(SqlBreak node);
    void Visit(SqlCase node);
    void Visit(SqlCast node);
    void Visit(SqlCloseCursor node);
    void Visit(SqlCollate node);
    void Visit(SqlColumnRef node);
    void Visit(SqlConcat node);
    void Visit(SqlContinue node);
    void Visit(SqlContainer node);
    void Visit(SqlCommand node);
    void Visit(SqlCreateAssertion node);
    void Visit(SqlCreateCharacterSet node);
    void Visit(SqlCreateCollation node);
    void Visit(SqlCreateDomain node);
    void Visit(SqlCreateIndex node);
    void Visit(SqlCreatePartitionFunction node);
    void Visit(SqlCreatePartitionScheme node);
    void Visit(SqlCreateSchema node);
    void Visit(SqlCreateSequence node);
    void Visit(SqlCreateTable node);
    void Visit(SqlCreateTranslation node);
    void Visit(SqlCreateView node);
    void Visit(SqlCursor node);
    void Visit(SqlDeclareCursor node);
    void Visit(SqlDefaultValue node);
    void Visit(SqlDelete node);
    void Visit(SqlDropAssertion node);
    void Visit(SqlDropCharacterSet node);
    void Visit(SqlDropCollation node);
    void Visit(SqlDropDomain node);
    void Visit(SqlDropIndex node);
    void Visit(SqlDropPartitionFunction node);
    void Visit(SqlDropPartitionScheme node);
    void Visit(SqlDropSchema node);
    void Visit(SqlDropSequence node);
    void Visit(SqlDropTable node);
    void Visit(SqlDropTranslation node);
    void Visit(SqlDropView node);
    void Visit(SqlDynamicFilter node);
    void Visit(SqlPlaceholder node);
    void Visit(SqlExtract node);
    void Visit(SqlFastFirstRowsHint node);
    void Visit(SqlFetch node);
    void Visit(SqlForceJoinOrderHint node);
    void Visit(SqlFreeTextTable node);
    void Visit(SqlFunctionCall node);
    void Visit(SqlIf node);
    void Visit(SqlInsert node);
    void Visit(SqlJoinExpression node);
    void Visit(SqlJoinHint node);
    void Visit(SqlLike node);
    void Visit(SqlLiteral node);
    void Visit(SqlMatch node);
    void Visit(SqlNative node);
    void Visit(SqlNativeHint node);
    void Visit(SqlNextValue value);
    void Visit(SqlNull node);
    void Visit(SqlOpenCursor node);
    void Visit(SqlOrder node);
    void Visit(SqlParameterRef node);
    void Visit(SqlRound node);
    void Visit(SqlQueryExpression node);
    void Visit(SqlQueryRef node);
    void Visit(SqlRow node);
    void Visit(SqlRowNumber node);
    void Visit(SqlRenameTable node);
    void Visit(SqlStatementBlock node);
    void Visit(SqlTableColumn node);
    void Visit(SqlTableRef node);
    void Visit(SqlTrim node);
    void Visit(SqlSelect node);
    void Visit(SqlSubQuery node);
    void Visit(SqlUnary node);
    void Visit(SqlUpdate node);
    void Visit(SqlUserColumn node);
    void Visit(SqlUserFunctionCall node);
    void Visit(SqlDeclareVariable node);
    void Visit(SqlVariable node);
    void Visit(SqlVariant node);
    void Visit(SqlWhile node);
    void Visit(SqlFragment node);
  }
}
