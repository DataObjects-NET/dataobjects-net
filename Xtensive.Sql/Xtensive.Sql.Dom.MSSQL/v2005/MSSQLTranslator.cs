// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Ddl;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Mssql.v2005
{
  public class MssqlTranslator: SqlTranslator
  {
    internal const string DateDiffDay = "date_diff_day";
    internal const string DateDiffMillisecond = "date_diff_ms";
    internal const string DateAddYear = "date_add_year";
    internal const string DateAddMonth = "date_add_month";
    internal const string DateAddDay = "date_add_day";
    internal const string DateAddHour = "date_add_hour";
    internal const string DateAddMinute = "date_add_minute";
    internal const string DateAddSecond = "date_add_second";
    internal const string DateAddMillisecond = "date_add_ms";
    internal const string DatePartWeekDay = "date_part_weekday";
    internal const string DateFirst = "date_first";

    public override void Initialize()
    {
      base.Initialize();
      numberFormat.NumberDecimalSeparator = ".";
      dateTimeFormat.ShortDatePattern = "\\'yyyy'-'MM'-'dd";
      dateTimeFormat.LongTimePattern = "HH':'mm':'ss'.'fff\\'";
    }

    public override string Translate(SqlCompilerContext context, SqlAggregate node, NodeSection section)
    {
      switch (section)
      {
        case NodeSection.Entry:
          string result = Translate(node.NodeType) + "(" + ((node.Distinct) ? "DISTINCT" : string.Empty);
          if (node.NodeType == SqlNodeType.Count)
            return "CAST(" + result;
          return result;
        case NodeSection.Exit:
          if (node.NodeType == SqlNodeType.Count)
            return ") AS bigint)";
          return ")";
      }
      return string.Empty;
    }

    public override string Translate(SqlCompilerContext context, bool cascade, AlterTableSection section)
    {
      return String.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
    {
      switch (section)
      {
        case FunctionCallSection.ArgumentEntry:
          return String.Empty;
        case FunctionCallSection.ArgumentDelimiter:
          return ArgumentDelimiter;
        default:
          return base.Translate(context, node, section, position);
      }
    }

    public override string Translate(SqlFunctionType functionType)
    {
      switch (functionType)
      {
        case SqlFunctionType.CurrentDate:
          return "GETDATE";
        case SqlFunctionType.Extract:
          return "DATEPART";
        case SqlFunctionType.Length:
          return "LEN";
        case SqlFunctionType.Position:
          return "PATINDEX";
        case SqlFunctionType.Atan2:
          return "ATN2";
      }
      return base.Translate(functionType);
    }

    public override string Translate(SqlNodeType type)
    {
      switch (type)
      {
        case SqlNodeType.Concat:
          return "+";
        case SqlNodeType.Overlaps:
          throw new NotSupportedException(String.Format("'{0}' is not supported.", type));
      }
      return base.Translate(type);
    }

    public override string Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      switch (section)
      {
        case TableColumnSection.Type:
          StringBuilder sb = new StringBuilder();
          if (column.Domain == null)
            sb.Append(Translate(column.DataType));
          else
            sb.Append(QuoteIdentifier(column.Domain.Schema.DbName, column.Domain.DbName));
          if (column.Collation != null)
            sb.Append(" COLLATE " + column.Collation.DbName);
          return sb.ToString();
        case TableColumnSection.GenerationExpressionEntry:
          return "AS (";
        case TableColumnSection.GeneratedEntry:
        case TableColumnSection.GeneratedExit:
        case TableColumnSection.SetIdentityInfoElement:
        case TableColumnSection.Exit:
          return String.Empty;
        default:
          return base.Translate(context, column, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SequenceDescriptor descriptor,
                                     SequenceDescriptorSection section)
    {
      switch (section)
      {
        case SequenceDescriptorSection.StartValue:
        case SequenceDescriptorSection.RestartValue:
          if (descriptor.StartValue.HasValue)
            return "IDENTITY (" + descriptor.StartValue.Value + RowItemDelimiter;
          return String.Empty;
        case SequenceDescriptorSection.Increment:
          if (descriptor.Increment.HasValue)
            return descriptor.Increment.Value + ")";
          return String.Empty;
        default:
          return String.Empty;
      }
    }

    public override string Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      switch (section)
      {
        case ConstraintSection.Exit:
          ForeignKey fk = constraint as ForeignKey;
          if (fk != null)
          {
            if (fk.OnUpdate == ReferentialAction.Cascade)
              return ") ON UPDATE CASCADE";
            if (fk.OnDelete == ReferentialAction.Cascade)
              return ") ON DELETE CASCADE";
          }
          return ")";
        default:
          return base.Translate(context, constraint, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      switch (section)
      {
        case CreateTableSection.Entry:
          {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE ");
            TemporaryTable tmpTable = node.Table as TemporaryTable;
            if (tmpTable != null)
            {
              if (tmpTable.IsGlobal)
                tmpTable.DbName = "##" + tmpTable.Name;
              else
                tmpTable.DbName = "#" + tmpTable.Name;
            }
            sb.Append("TABLE " + Translate(node.Table));
            return sb.ToString();
          }
        case CreateTableSection.Exit:
          {
            string result = string.IsNullOrEmpty(node.Table.Filegroup)
                              ? string.Empty
                              : " ON " + QuoteIdentifier(node.Table.Filegroup);
            return result;
          }
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlCreateView node, NodeSection section)
    {
      switch (section)
      {
        case NodeSection.Exit:
          if (node.View.CheckOptions == CheckOptions.Cascaded)
            return "WITH CHECK OPTION";
          else
            return string.Empty;
        default:
          return base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlDeclareCursor node, DeclareCursorSection section)
    {
      if (section == DeclareCursorSection.Holdability || section == DeclareCursorSection.Returnability)
        return string.Empty;
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlDelete node, DeleteSection section)
    {
      switch (section)
      {
        case DeleteSection.Entry:
          return (node.Top > 0) ? "DELETE TOP " + node.Top + " FROM" : base.Translate(context, node, section);
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlInsert node, InsertSection section)
    {
      switch (section)
      {
        case InsertSection.Entry:
          return (node.Top > 0) ? "INSERT TOP " + node.Top + " INTO" : base.Translate(context, node, section);
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlJoinExpression node, JoinSection section)
    {
      switch (section) {
        case JoinSection.Specification:
          if (node.Expression == null)
            switch (node.JoinType) {
              case SqlJoinType.InnerJoin:
              case SqlJoinType.LeftOuterJoin:
              case SqlJoinType.RightOuterJoin:
              case SqlJoinType.FullOuterJoin:
                throw new NotSupportedException();
              case SqlJoinType.CrossApply:
                return "CROSS APPLY";
              case SqlJoinType.LeftOuterApply:
                return "OUTER APPLY";
             }
          SqlJoinHint joinHint = TryFindJoinHint(context, node);
          return Translate(node.JoinType)
            + (joinHint != null ? " " + Translate(joinHint.JoinMethod) : string.Empty) + " JOIN";
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlQueryExpression node, QueryExpressionSection section)
    {
      if (node.All && section == QueryExpressionSection.All && (node.NodeType == SqlNodeType.Except || node.NodeType == SqlNodeType.Intersect))
        return string.Empty;
      return base.Translate(context, node, section);
    }

    private static SqlJoinHint TryFindJoinHint(SqlCompilerContext context, SqlJoinExpression node)
    {
      SqlQueryStatement statement = null;
      for (int i = 0, count = context.GetTraversalPath().Length; i < count; i++)
      {
        if (context.GetTraversalPath()[i] is SqlQueryStatement)
          statement = context.GetTraversalPath()[i] as SqlQueryStatement;
      }
      if (statement == null || statement.Hints.Count == 0)
        return null;

      SqlJoinHint joinHintCandidate = null;
      foreach (SqlHint sqlHint in statement.Hints)
      {
        if (!(sqlHint is SqlJoinHint))
          continue;
        SqlJoinHint joinHint = sqlHint as SqlJoinHint;
        // Exact match (MSSQL approach).
        if (joinHint.SqlJoin != null && joinHint.SqlJoin == node)
          return joinHint;
        // Semi-exact match (combined MSSQl & Oracle approaches).
        else if (joinHint.SqlTables != null && joinHint.SqlTables.Length == 2 && joinHint.SqlTables[0] == node.Left &&
                 joinHint.SqlTables[1] == node.Right)
          joinHintCandidate = joinHint;
        // Semi-exact match (Oracle approach). See SqlJoinMethod enum comments for details.
        else if (joinHintCandidate == null && joinHint.SqlTables != null && joinHint.SqlTables.Length == 1 &&
                 joinHint.SqlTables[0] == node.Right)
          joinHintCandidate = joinHint;
      }
      return joinHintCandidate;
    }

    public override string Translate(SqlJoinMethod method)
    {
      switch (method)
      {
        case SqlJoinMethod.Hash:
          return "HASH";
        case SqlJoinMethod.Merge:
          return "MERGE";
        case SqlJoinMethod.Loop:
          return "LOOP";
        case SqlJoinMethod.Remote:
          return "REMOTE";
        default:
          return string.Empty;
      }
    }

    public override string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section)
      {
        case SelectSection.Entry:
          if (node.Distinct)
            return (node.Top > 0) ? "SELECT DISTINCT TOP " + node.Top : "SELECT DISTINCT TOP 100 PERCENT";
          else
            return (node.Top > 0) ? "SELECT TOP " + node.Top : "SELECT TOP 100 PERCENT";
        case SelectSection.Exit:
          if (node.Hints.Count == 0)
            return string.Empty;
          else
          {
            List<string> hints = new List<string>(node.Hints.Count);
            foreach (SqlHint hint in node.Hints)
            {
              if (hint is SqlTableHint)
                continue;
              SqlJoinHint joinHint = hint as SqlJoinHint;
              if (joinHint != null && joinHint.SqlTables == null && joinHint.SqlJoin == null)
                hints.Add(Translate(joinHint.JoinMethod) + " JOIN");
              else if (hint is SqlForceJoinOrderHint)
                hints.Add("FORCE ORDER");
              else if (hint is SqlFastFirstRowsHint)
              {
                SqlFastFirstRowsHint ffrHint = hint as SqlFastFirstRowsHint;
                hints.Add("FAST " + ffrHint.Amount);
              }
              else if (hint is SqlUsePlanHint)
              {
                SqlUsePlanHint upHint = hint as SqlUsePlanHint;
                hints.Add("USE PLAN N'" + upHint.Plan);
              }
              else if (hint is SqlNativeHint)
              {
                SqlNativeHint nHint = hint as SqlNativeHint;
                hints.Add(nHint.HintText);
              }
            }
            return hints.Count > 0 ? "OPTION (" + string.Join(", ", hints.ToArray()) + ")" : string.Empty;
          }
      }

      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlTableRef node, TableSection section)
    {
      switch (section)
      {
        case TableSection.AliasDeclaration:

          SqlQueryStatement statement = null;
          for (int i = 0, count = context.GetTraversalPath().Length; i < count; i++)
          {
            if (context.GetTraversalPath()[i] is SqlQueryStatement)
              statement = context.GetTraversalPath()[i] as SqlQueryStatement;
          }
          if (statement == null || statement.Hints.Count == 0)
            return base.Translate(context, node, section);

          List<string> tableHints = new List<string>();
          foreach (SqlHint hint in statement.Hints)
          {
            SqlTableHint tableHint = hint as SqlTableHint;
            if (tableHint == null || tableHint.SqlTable != node)
              continue;

            SqlTableLockHint lockHint = hint as SqlTableLockHint;
            if (lockHint != null)
            {
              if (lockHint.IsolationLevel != SqlTableIsolationLevel.Default)
                tableHints.Add(Translate(lockHint.IsolationLevel));
              if (lockHint.LockType != SqlTableLockType.Default)
                tableHints.Add(Translate(lockHint.LockType));
            }
            else
            {
              SqlTableScanHint scanHint = hint as SqlTableScanHint;
              // MSSQL supports only this kind of scan
              if (scanHint != null && scanHint.ScanMethod == SqlTableScanMethod.Index)
              {
                List<string> indexes;
                if (scanHint.Indexes != null)
                {
                  indexes = new List<string>(scanHint.Indexes.Length);
                  for (int i = 0, count = scanHint.Indexes.Length; i < count; i++)
                  {
                    if (scanHint.Indexes[i] == null)
                      continue;
                    indexes.Add(scanHint.Indexes[i].DbName);
                  }
                }
                else
                {
                  indexes = new List<string>(scanHint.IndexNames.Length);
                  for (int i = 0, count = scanHint.IndexNames.Length; i < count; i++)
                  {
                    if (string.IsNullOrEmpty(scanHint.IndexNames[i]))
                      continue;
                    indexes.Add(scanHint.IndexNames[i]);
                  }
                }
                if (indexes.Count > 0)
                {
                  tableHints.Add(
                    string.Format("{0}({1})", Translate(scanHint.ScanMethod), string.Join(", ", indexes.ToArray())));
                }
              }
            }
          }
          return
            base.Translate(context, node, section) +
            (tableHints.Count == 0 ? string.Empty : " WITH (" + string.Join(", ", tableHints.ToArray()) + ")");
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlTrim node, TrimSection section)
    {
      switch (section)
      {
        case TrimSection.Entry:
          if (!SqlExpression.IsNull(node.Pattern))
            node.ReplaceWith(Sql.Trim(node.Expression, node.TrimType));
          switch (node.TrimType)
          {
            case SqlTrimType.Leading:
              return "LTRIM(";
            case SqlTrimType.Trailing:
              return "RTRIM(";
            case SqlTrimType.Both:
              node.ReplaceWith(Sql.Trim(Sql.Trim(node.Expression, SqlTrimType.Trailing), SqlTrimType.Leading));
              return Translate(context, node, section);
          }
          break;
        case TrimSection.From:
          return string.Empty;
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlTableIsolationLevel level)
    {
      switch (level)
      {
        case SqlTableIsolationLevel.HoldLock:
          return "HOLDLOCK";
        case SqlTableIsolationLevel.NoLock:
          return "NOLOCK";
        case SqlTableIsolationLevel.ReadCommitted:
          return "READCOMMITTED";
        case SqlTableIsolationLevel.ReadCommittedLock:
          return "READCOMMITTEDLOCK";
        case SqlTableIsolationLevel.RepeatableRead:
          return "REPEATABLEREAD";
      }
      return base.Translate(level);
    }

    public override string Translate(SqlTableLockType type)
    {
      switch (type)
      {
        case SqlTableLockType.PagLock:
          return "PAGLOCK";
        case SqlTableLockType.RowLock:
          return "ROWLOCK";
        case SqlTableLockType.TabLock:
          return "TABLOCK";
        case SqlTableLockType.TabLockX:
          return "TABLOCKX";
        case SqlTableLockType.UpdLock:
          return "UPDLOCK";
      }
      return base.Translate(type);
    }

    public override string Translate(SqlTableScanMethod method)
    {
      return method == SqlTableScanMethod.Index ? "INDEX" : base.Translate(method);
    }

    public override string Translate(SqlCompilerContext context, SqlUpdate node, UpdateSection section)
    {
      switch (section)
      {
        case UpdateSection.Entry:
          return (node.Top > 0) ? "UPDATE TOP (" + node.Top + ")" : base.Translate(context, node, section);
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlDropSchema node)
    {
      return "DROP SCHEMA " + QuoteIdentifier(node.Schema.DbName);
    }

    public override string Translate(SqlCompilerContext context, SqlDropTable node)
    {
      return "DROP TABLE " + Translate(node.Table);
    }

    public override string Translate(SqlCompilerContext context, SqlDropView node)
    {
      return "DROP VIEW " + Translate(node.View);
    }

    public override string Translate(SqlTrimType type)
    {
      return string.Empty;
    }

    public override string Translate<T>(SqlCompilerContext context, SqlLiteral<T> node)
    {
      if (typeof(T) == typeof(TimeSpan))
        return Convert.ToString(((TimeSpan)(object)node.Value).Ticks / 10000, this);
      return base.Translate(context, node);
    }

    public override string Translate(SqlCompilerContext context, SqlUserFunctionCall node, FunctionCallSection section, int position)
    {
      if (section == FunctionCallSection.Entry)
        switch (node.Name)
        {
          case DateDiffDay:
            return "DATEDIFF(DAY, ";
          case DateDiffMillisecond:
            return "DATEDIFF(MS, ";
          case DateAddYear:
            return "DATEADD(YEAR, ";
          case DateAddMonth:
            return "DATEADD(MONTH, ";
          case DateAddDay:
            return "DATEADD(DAY, ";
          case DateAddHour:
            return "DATEADD(HOUR, ";
          case DateAddMinute:
            return "DATEADD(MINUTE, ";
          case DateAddSecond:
            return "DATEADD(SECOND, ";
          case DateAddMillisecond:
            return "DATEADD(MS, ";
          case DatePartWeekDay:
            return "DATEPART(WEEKDAY, ";
          case DateFirst:
            return "(@@DATEFIRST";
        }

      return base.Translate(context, node, section, position);
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MssqlTranslator"/> class.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected internal MssqlTranslator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}