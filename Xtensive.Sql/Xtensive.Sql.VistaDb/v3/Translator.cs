// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Text;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.VistaDb.v3
{
  internal class Translator: SqlTranslator
  {
    public override void Initialize()
    {
      base.Initialize();
      //numberFormat.NumberDecimalSeparator = ".";
      dateTimeFormat.ShortDatePattern = "\\'yyyy'-'MM'-'dd";
      dateTimeFormat.LongTimePattern = "HH':'mm':'ss'.'fff\\'";
    }

    public override string Translate(SqlFunctionType functionType)
    {
      switch (functionType) {
      case SqlFunctionType.CurrentDate:
        return "GETDATE";
      case SqlFunctionType.CharLength:
        return "LEN";
      }
      return base.Translate(functionType);
    }

    public override string Translate(SqlNodeType type)
    {
      switch(type) {
      case SqlNodeType.Concat:
        return "+";
      }

      return base.Translate(type);
    }

    public override string Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
    {
      switch (section) {
      case FunctionCallSection.ArgumentEntry:
        return String.Empty;
      case FunctionCallSection.ArgumentDelimiter:
        return ArgumentDelimiter;
      default:
        return base.Translate(context, node, section, position);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
      case SelectSection.Entry:
        return (node.Limit > 0) ? "SELECT TOP " + node.Limit : base.Translate(context, node, section);
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SchemaNode node)
    {
      return QuoteIdentifier(node.DbName);
    }

    public override string Translate(SqlCompilerContext context, SqlDropTable node)
    {
      return "DROP TABLE " + QuoteIdentifier(node.Table.DbName);
    }

    public override string Translate(SqlCompilerContext context, SqlDropView node)
    {
      return "DROP VIEW " + QuoteIdentifier(node.View.DbName);
    }

    public override string Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      return "DROP INDEX " + QuoteIdentifier(node.Index.DataTable.DbName) + "." + QuoteIdentifier(node.Index.DbName);
    }

    public override string Translate(SqlCompilerContext context, SqlTableColumn node, NodeSection section)
    {
      if (context.GetTraversalPath()[0].NodeType == SqlNodeType.Insert)
        return (((object)node == (object)node.SqlTable.Asterisk) ? node.Name : QuoteIdentifier(node.Name));
      else
        return Translate(context, node.SqlTable, NodeSection.Entry) + "." +
          (((object)node == (object)node.SqlTable.Asterisk) ? node.Name : QuoteIdentifier(node.Name));

    }

    public override string Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      switch (section) {
      case TableColumnSection.Type:
        StringBuilder sb = new StringBuilder();
        if (column.DataType != null)
          sb.Append(Translate(column.DataType));
        return sb.ToString();
      case TableColumnSection.GenerationExpressionEntry:
        return "AS (";
      case TableColumnSection.GeneratedEntry:
      case TableColumnSection.GeneratedExit:
      case TableColumnSection.SetIdentityInfoElement:
      case TableColumnSection.Exit:
        return string.Empty;
      default:
        return base.Translate(context, column, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SequenceDescriptor descriptor,
      SequenceDescriptorSection section)
    {
      switch (section) {
      case SequenceDescriptorSection.StartValue:
      case SequenceDescriptorSection.RestartValue:
        if (descriptor.StartValue.HasValue)
          return "IDENTITY (" + descriptor.StartValue.Value + RowItemDelimiter;
        return string.Empty;
      case SequenceDescriptorSection.Increment:
        if (descriptor.Increment.HasValue)
          return descriptor.Increment.Value + ")";
        return string.Empty;
      default:
        return string.Empty;
      }
    }

    public override string Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      switch (section) {
      case ConstraintSection.Exit:
        ForeignKey fk = constraint as ForeignKey;
        if (fk != null) {
          if (fk.OnUpdate != ReferentialAction.NoAction)
            return ") ON UPDATE " + Translate(fk.OnUpdate);
          if (fk.OnDelete != ReferentialAction.NoAction)
            return ") ON DELETE " + Translate(fk.OnUpdate);
        }
        return ")";
      default:
        return base.Translate(context, constraint, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      switch (section) {
      case CreateTableSection.Entry: {
        StringBuilder sb = new StringBuilder();
        sb.Append("CREATE ");
        TemporaryTable tmpTable = node.Table as TemporaryTable;
        if (tmpTable != null) {
          if (tmpTable.IsGlobal)
            tmpTable.DbName = "##" + tmpTable.Name;
          else
            tmpTable.DbName = "#" + tmpTable.Name;
        }
        sb.Append("TABLE " + Translate(node.Table));
        return sb.ToString();
      }
      case CreateTableSection.Exit: {
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
      switch (section) {
      case NodeSection.Exit:
        return string.Empty;
      default:
        return base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      switch (section) {
      case AlterTableSection.AddColumn:
        return "ADD";
      default:
        return base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlCompilerContext context, bool cascade, AlterTableSection section)
    {
      return string.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlCreateIndex node)
    {
      Index index = node.Index;
      StringBuilder sb = new StringBuilder();
      sb.Append("CREATE ");
      if (index.IsUnique)
        sb.Append("UNIQUE ");
      if (index.IsClustered)
        sb.Append("CLUSTERED ");
      sb.Append("INDEX " + QuoteIdentifier(index.DbName));
      sb.Append(" ON " + Translate(index.DataTable) + " (");
      bool first = true;
      foreach (IndexColumn column in index.Columns) {
        if (first)
          first = false;
        else
          sb.AppendFormat(RowItemDelimiter);
        sb.Append(QuoteIdentifier(column.DbName));
        if (column.Ascending)
          sb.Append(" ASC");
        else
          sb.Append(" DESC");
      }
      sb.Append(")");

      return sb.ToString();
    }

    public override string Translate(SqlCompilerContext context, SqlQueryExpression node, QueryExpressionSection section)
    {
      if (node.All && section == QueryExpressionSection.All && (node.NodeType == SqlNodeType.Except || node.NodeType == SqlNodeType.Intersect))
        return string.Empty;
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, Table node, SqlRenameAction action)
    {
      return string.Format("EXEC sp_rename ('{0}', '{1}')", QuoteIdentifier(node.DbName), action.Name);
    }

    public override string Translate(SqlCompilerContext context, TableColumn node, SqlRenameAction action)
    {
      return string.Format("EXEC sp_rename ('{0}', '{1}', 'COLUMN')", QuoteIdentifier(node.Table.DbName, node.DbName), action.Name);
    }
    
    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}