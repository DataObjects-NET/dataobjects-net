// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Globalization;
using System.Text;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Ddl;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Compiler
{
  public class SqlTranslator: IFormatProvider
  {
    protected DateTimeFormatInfo dateTimeFormat;
    private readonly SqlDriver driver;
    protected NumberFormatInfo numberFormat;

    public virtual string BatchStatementDelimiter
    {
      get { return ";"; }
    }

    public virtual string ArgumentDelimiter
    {
      get { return ","; }
    }

    public virtual string ColumnDelimiter
    {
      get { return ","; }
    }

    public virtual string RowItemDelimiter
    {
      get { return ","; }
    }

    public virtual string OrderDelimiter
    {
      get { return ","; }
    }

    public virtual string ValueDelimiter
    {
      get { return ","; }
    }

    public virtual string WhenDelimiter
    {
      get { return string.Empty; }
    }

    public virtual string DdlStatementDelimiter
    {
      get { return string.Empty; }
    }

    public virtual string HintDelimiter
    {
      get { return string.Empty; }
    }

    /// <summary>
    /// Gets the driver.
    /// </summary>
    /// <value>The driver.</value>
    protected SqlDriver Driver
    {
      get { return driver; }
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public virtual void Initialize()
    {
      numberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
      dateTimeFormat = (DateTimeFormatInfo)CultureInfo.InvariantCulture.DateTimeFormat.Clone();
    }

    public virtual string Translate(SqlCompilerContext context, SqlAggregate node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return Translate(node.NodeType) + "(" + ((node.Distinct) ? "DISTINCT" : string.Empty);
        case NodeSection.Exit:
          return ")";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlAlterDomain node, AlterDomainSection section)
    {
      switch (section) {
        case AlterDomainSection.Entry:
          return "ALTER DOMAIN " + Translate(node.Domain);
        case AlterDomainSection.AddConstraint:
          return "ADD";
        case AlterDomainSection.DropConstraint:
          return "DROP";
        case AlterDomainSection.SetDefault:
          return "SET DEFAULT";
        case AlterDomainSection.DropDefault:
          return "DROP DEFAULT";
        default:
          return String.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlAlterPartitionFunction node)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("ALTER PARTITION FUNCTION " + QuoteIdentifier(node.PartitionFunction.DbName) + "()");
      if (node.Option == SqlAlterPartitionFunctionOption.Split)
        sb.Append(" SPLIT RANGE (");
      else
        sb.Append(" MERGE RANGE (");
      sb.Append(node.Boundary.ToString(this) + ")");
      return sb.ToString();
    }

    public virtual string Translate(SqlCompilerContext context, SqlAlterPartitionScheme node)
    {
      return
        "ALTER PARTITION SCHEME " + QuoteIdentifier(node.PartitionSchema.DbName) + " NEXT USED" +
        (String.IsNullOrEmpty(node.Filegroup) ? "" : " " + node.Filegroup);
    }

    public virtual string Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      switch (section) {
        case AlterTableSection.Entry:
          return "ALTER TABLE " + Translate(node.Table);
        case AlterTableSection.AddColumn:
          return "ADD COLUMN";
        case AlterTableSection.AlterColumn:
          return "ALTER COLUMN";
        case AlterTableSection.DropColumn:
          return "DROP COLUMN";
        case AlterTableSection.AddConstraint:
          return "ADD";
        case AlterTableSection.DropConstraint:
          return "DROP";
        default:
          return String.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlAlterSequence node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "ALTER SEQUENCE " + Translate(node.Sequence);
        default:
          return String.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, bool cascade, AlterTableSection section)
    {
      switch (section) {
        case AlterTableSection.DropBehavior:
          return cascade ? "CASCADE" : "RESTRICT";
        default:
          return String.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      switch (section) {
        case TableColumnSection.Entry:
          return QuoteIdentifier(column.DbName);
        case TableColumnSection.Type:
          if (column.Domain == null)
            return Translate(column.DataType);
          else
            return Translate(column.Domain);
        case TableColumnSection.DefaultValue:
          return "DEFAULT";
        case TableColumnSection.DropDefault:
          return "DROP DEFAULT";
        case TableColumnSection.SetDefault:
          return "SET DEFAULT";
          //        case TableColumnSection.GeneratedEntry:
          //          if (column.GeneratedAlways)
          //            return "GENERATED ALWAYS (";
          //          else
          //            return "GENERATED BY DEFAULT (";
          //        case TableColumnSection.GeneratedExit:
          //          return ")";
          //        case TableColumnSection.GenerationExpressionEntry:
          //          return "GENERATED ALWAYS (";
        case TableColumnSection.GenerationExpressionExit:
          return ")" + (column.IsPersisted ? " PERSISTED" : "");
        case TableColumnSection.SetIdentityInfoElement:
          return "SET";
        case TableColumnSection.NotNull:
          return "NOT NULL";
        case TableColumnSection.Exit:
          if (column.Collation != null)
            return "COLLATE " + Translate(column.Collation);
          return String.Empty;
        default:
          return String.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      switch (section) {
        case ConstraintSection.Entry: {
          if (!String.IsNullOrEmpty(constraint.DbName))
            return "CONSTRAINT " + QuoteIdentifier(constraint.DbName);
          return String.Empty;
        }
        case ConstraintSection.Check:
          return "CHECK (";
        case ConstraintSection.PrimaryKey:
          return "PRIMARY KEY (";
        case ConstraintSection.Unique:
          return "UNIQUE (";
        case ConstraintSection.ForeignKey:
          return "FOREIGN KEY (";
        case ConstraintSection.ReferencedColumns: {
          ForeignKey fk = constraint as ForeignKey;
          return ") REFERENCES " + Translate(fk.ReferencedColumns[0].DataTable) + " (";
        }
        case ConstraintSection.Exit: {
          ForeignKey fk = constraint as ForeignKey;
          StringBuilder sb = new StringBuilder();
          sb.Append(")");
          if (fk != null) {
            if (fk.MatchType != SqlMatchType.None)
              sb.Append(" MATCH " + Translate(fk.MatchType));
            if (fk.OnUpdate != ReferentialAction.NoAction)
              sb.Append(" ON UPDATE " + Translate(fk.OnUpdate));
            if (fk.OnDelete != ReferentialAction.NoAction)
              sb.Append(" ON DELETE " + Translate(fk.OnDelete));
          }
          if (constraint.IsDeferrable.HasValue) {
            if (constraint.IsDeferrable.Value)
              sb.Append(" DEFERRABLE");
            else
              sb.Append(" NOT DEFERRABLE");
          }
          if (constraint.IsInitiallyDeferred.HasValue) {
            if (constraint.IsInitiallyDeferred.Value)
              sb.Append(" INITIALLY DEFERRED");
            else
              sb.Append(" INITIALLY IMMEDIATE");
          }
          return sb.ToString();
        }
        default:
          return String.Empty;
      }
    }

    public virtual string Translate<T>(SqlCompilerContext context, SqlArray<T> node)
    {
      T[] values = node.Values;
      int count = values.Length;
      if (count == 0)
        return "(NULL)";

      TypeCode type = Type.GetTypeCode(typeof (T));
      bool quote = (type == TypeCode.Char || type == TypeCode.String || typeof(T) == typeof(Guid));
      string[] buffer = new string[count];

      for (int index = 0; index < count; index++)
        buffer[index] = quote ? QuoteString(Convert.ToString(values[index], this)) : Convert.ToString(values[index], this);
      if (count == 1)
        return "(" + buffer[0] + ")";
      else {
        buffer[0] = "(" + buffer[0];
        buffer[count - 1] += ")";
        return String.Join(RowItemDelimiter, buffer);
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlAssignment node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "SET";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlBatch node, NodeSection section)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlBetween node, BetweenSection section)
    {
      switch (section) {
        case BetweenSection.Between:
          return Translate(node.NodeType);
        case BetweenSection.And:
          return "AND";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlBinary node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          //          return (node.NodeType==SqlNodeType.Or) ? "(" : string.Empty;
          return "(";
        case NodeSection.Exit:
          //          return (node.NodeType==SqlNodeType.Or) ? ")" : string.Empty;
          return ")";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlBreak node)
    {
      return "BREAK";
    }

    public virtual string Translate(SqlCompilerContext context, SqlCase node, CaseSection section)
    {
      switch (section) {
        case CaseSection.Entry:
          return "CASE";
        case CaseSection.Else:
          return "ELSE";
        case CaseSection.Exit:
          return "END";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlCase node, SqlExpression item, CaseSection section)
    {
      switch (section) {
        case CaseSection.When:
          return "WHEN";
        case CaseSection.Then:
          return "THEN";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "CAST(";
        case NodeSection.Exit:
          return " AS " + Translate(node.Type) + ")";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlCloseCursor node)
    {
      return "CLOSE " + node.Cursor.Name;
    }

    public virtual string Translate(SqlCompilerContext context, SqlCollate node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          return "COLLATE " + node.Collation.DbName;
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlColumnRef node, ColumnSection section)
    {
      switch (section) {
        case ColumnSection.Entry:
          return QuoteIdentifier(node.Name);
        case ColumnSection.AliasDeclaration:
          return (string.IsNullOrEmpty(node.Name)) ? string.Empty : "AS " + QuoteIdentifier(node.Name);
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlContinue node)
    {
      return "CONTINUE";
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateAssertion node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "CREATE ASSERTION " + Translate(node.Assertion) + " CHECK";
        case NodeSection.Exit:
          StringBuilder sb = new StringBuilder();
          if (node.Assertion.IsDeferrable.HasValue) {
            if (node.Assertion.IsDeferrable.Value)
              sb.Append(" DEFERRABLE");
            else
              sb.Append(" NOT DEFERRABLE");
          }
          if (node.Assertion.IsInitiallyDeferred.HasValue) {
            if (node.Assertion.IsInitiallyDeferred.Value)
              sb.Append(" INITIALLY DEFERRED");
            else
              sb.Append(" INITIALLY IMMEDIATE");
          }
          return sb.ToString();
        default:
          return string.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateCharacterSet node)
    {
      StringBuilder sb = new StringBuilder();
      //      sb.Append("CREATE CHARACTER SET "+Translate(node.CharacterSet));
      //      sb.Append(" AS GET "+Translate(node.CharacterSet.CharacterSetSource));
      //      if (node.CharacterSet.Collate!=null)
      //        sb.Append(" COLLATE "+Translate(node.CharacterSet.Collate));
      //      else if (node.CharacterSet.CollationSource!=null) {
      //        sb.Append(" COLLATION FROM ");
      //        if (node.CharacterSet.CollationSource is IDefaultCollation)
      //          sb.Append("DEFAULT");
      //        else if (node.CharacterSet.CollationSource is ICollation) {
      //          ICollation collationSource = node.CharacterSet.CollationSource as ICollation;
      //          sb.Append(Translate(collationSource));
      //        }
      //        else if (node.CharacterSet.CollationSource is IDescendingCollation) {
      //          IDescendingCollation collationSource = node.CharacterSet.CollationSource as IDescendingCollation;
      //          sb.Append("DESC ("+Translate(collationSource.Collation)+")");
      //        }
      //        else if (node.CharacterSet.CollationSource is IExternalCollation)
      //          sb.Append("EXTERNAL ("+QuoteString(((IExternalCollation)node.CharacterSet.CollationSource).Name)+")");
      //        else if (node.CharacterSet.CollationSource is ITranslationCollation) {
      //          ITranslationCollation collationSource = node.CharacterSet.CollationSource as ITranslationCollation;
      //          sb.Append("TRANSLATION "+Translate(collationSource.Translation));
      //          if (collationSource.ThenCollation!=null)
      //            sb.Append(" THEN COLLATION "+Translate(collationSource.ThenCollation));
      //        }
      //      }
      return sb.ToString();
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateCollation node)
    {
      StringBuilder sb = new StringBuilder();
      //      sb.Append("CREATE COLLATION "+Translate(node.Collation));
      //      sb.Append(" FOR "+Translate(node.Collation.CharacterSet));
      //      sb.Append(" FROM ");
      //      if (node.Collation.CollationSource is IDefaultCollation)
      //        sb.Append("DEFAULT");
      //      else if (node.Collation.CollationSource is ICollation) {
      //        ICollation collationSource = node.Collation.CollationSource as ICollation;
      //        sb.Append(Translate(collationSource));
      //      }
      //      else if (node.Collation.CollationSource is IDescendingCollation) {
      //        IDescendingCollation collationSource = node.Collation.CollationSource as IDescendingCollation;
      //        sb.Append("DESC ("+Translate(collationSource.Collation)+")");
      //      }
      //      else if (node.Collation.CollationSource is IExternalCollation)
      //        sb.Append("EXTERNAL ("+QuoteString(((IExternalCollation)node.Collation.CollationSource).Name)+")");
      //      else if (node.Collation.CollationSource is ITranslationCollation) {
      //        ITranslationCollation collationSource = node.Collation.CollationSource as ITranslationCollation;
      //        sb.Append("TRANSLATION "+Translate(collationSource.Translation));
      //        if (collationSource.ThenCollation!=null)
      //          sb.Append(" THEN COLLATION "+Translate(collationSource.ThenCollation));
      //      }
      //      if (node.Collation.PadSpace.HasValue) {
      //        if (node.Collation.PadSpace.Value)
      //          sb.Append(" PAD SPACE");
      //        else
      //          sb.Append(" NO PAD");
      //      }
      return sb.ToString();
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateDomain node, CreateDomainSection section)
    {
      switch (section) {
        case CreateDomainSection.Entry:
          StringBuilder sb = new StringBuilder();
          sb.Append("CREATE DOMAIN " + Translate(node.Domain));
          sb.Append(" AS " + Translate(node.Domain.DataType));
          return sb.ToString();
        case CreateDomainSection.DomainDefaultValue:
          return "DEFAULT";
        case CreateDomainSection.DomainCollate:
          return "COLLATE " + Translate(node.Domain.Collation);
        default:
          return string.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateIndex node)
    {
      Index index = node.Index;
      StringBuilder sb = new StringBuilder();
      sb.Append("CREATE ");
      if (index.IsUnique)
        sb.Append("UNIQUE ");
      else if (index.IsBitmap)
        sb.Append("BITMAP ");
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
      if (index.NonkeyColumns != null && index.NonkeyColumns.Count != 0) {
        sb.Append(" INCLUDE (");
        first = true;
        foreach (DataTableColumn column in index.NonkeyColumns) {
          if (first)
            first = false;
          else
            sb.AppendFormat(RowItemDelimiter);
          sb.Append(QuoteIdentifier(column.DbName));
        }
        sb.Append(")");
      }
      if (index.FillFactor.HasValue) {
        sb.Append(" WITH (FILLFACTOR = " + index.FillFactor.Value + ")");
      }
      //StringBuilder options = new StringBuilder();
      //bool addOptions = false;
      //options.Append(" WITH (");
      //options.Append(Translate(index.RelationalIndexOption, RelationalIndexOption.PadIndex,ref addOptions));
      //if (index.FillFactor.HasValue) {
      //  if (addOptions)
      //    options.AppendFormat(RowItemDelimiter);
      //  else
      //    addOptions = true;
      //  options.Append("FILLFACTOR = "+index.FillFactor.Value);
      //}
      //options.Append(Translate(index.RelationalIndexOption, RelationalIndexOption.SortInTEMPDB, ref addOptions));
      //options.Append(Translate(index.RelationalIndexOption, RelationalIndexOption.IgnoreDupKey, ref addOptions));
      //options.Append(Translate(index.RelationalIndexOption, RelationalIndexOption.StatisticsNorecompute, ref addOptions));
      //options.Append(Translate(index.RelationalIndexOption, RelationalIndexOption.DropExisting, ref addOptions));
      //options.Append(Translate(index.RelationalIndexOption, RelationalIndexOption.Online, ref addOptions));
      //options.Append(Translate(index.RelationalIndexOption, RelationalIndexOption.AllowRowLocks, ref addOptions));
      //options.Append(Translate(index.RelationalIndexOption, RelationalIndexOption.AllowPageLocks, ref addOptions));
      //if (index.MaxDegreeOfParallelism.HasValue) {
      //  if (addOptions)
      //    options.AppendFormat(RowItemDelimiter);
      //  else
      //    addOptions = true;
      //  options.Append("MAXDOP = " + index.MaxDegreeOfParallelism.Value);
      //}
      //options.Append(")");
      //if (addOptions)
      //  sb.Append(options.ToString());
      if (index.PartitionDescriptor != null) {
        sb.Append(" " + Translate(index.PartitionDescriptor, true));
      }
      else if (!String.IsNullOrEmpty(index.Filegroup))
        sb.Append(" ON " + index.Filegroup);
      return sb.ToString();
    }

    //
    //    private string Translate(
    //      SqlCompilerContext context, RelationalIndexOption indexOptions, RelationalIndexOption option, ref bool addOptions)
    //    {
    //      bool on = (indexOptions&option)!=0;
    //      if (on && ((RelationalIndexOption.Default&option)==0)) {
    //        if (addOptions)
    //          return RowItemDelimiter+Translate(option)+" = ON";
    //        else {
    //          addOptions = true;
    //          return Translate(option)+" = ON";
    //        }
    //      }
    //      else if (!on && ((RelationalIndexOption.Default&option)!=0)) {
    //        if (addOptions)
    //          return RowItemDelimiter+Translate(option)+" = OFF";
    //        else {
    //          addOptions = true;
    //          return Translate(option)+" = OFF";
    //        }
    //      }
    //      return String.Empty;
    //    }

    public virtual string Translate(SqlCompilerContext context, SqlCreatePartitionFunction node)
    {
      PartitionFunction pf = node.PartitionFunction;
      StringBuilder sb = new StringBuilder();
      sb.Append("CREATE PARTITION FUNCTION " + QuoteIdentifier(pf.DbName));
      sb.Append(" (" + Translate(pf.DataType) + ")");
      sb.Append(" AS RANGE ");
      if (pf.BoundaryType == BoundaryType.Left)
        sb.Append("LEFT");
      else
        sb.Append("RIGHT");
      sb.Append(" FOR VALUES (");
      bool first = true;
      foreach (string value in pf.BoundaryValues) {
        if (first)
          first = false;
        else
          sb.Append(RowItemDelimiter);
        TypeCode t = Type.GetTypeCode(value.GetType());
        if (t == TypeCode.String || t == TypeCode.Char)
          sb.Append(QuoteString(value.ToString(this)));
        else
          sb.Append(value.ToString(this));
      }
      sb.Append(")");
      return sb.ToString();
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreatePartitionScheme node)
    {
      PartitionSchema ps = node.PartitionSchema;
      StringBuilder sb = new StringBuilder();
      sb.Append("CREATE PARTITION SCHEME " + QuoteIdentifier(ps.DbName));
      sb.Append(" AS PARTITION " + QuoteIdentifier(ps.PartitionFunction.DbName));
      if (ps.Filegroups.Count <= 1)
        sb.Append(" ALL");
      sb.Append(" TO (");
      bool first = true;
      foreach (string filegroup in ps.Filegroups) {
        if (first)
          first = false;
        else
          sb.Append(RowItemDelimiter);
        sb.Append(filegroup);
      }
      sb.Append(")");
      return sb.ToString();
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateSchema node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          StringBuilder sb = new StringBuilder();
          sb.Append("CREATE SCHEMA ");
          if (!String.IsNullOrEmpty(node.Schema.DbName))
            sb.Append(QuoteIdentifier(node.Schema.DbName) + (node.Schema.Owner == null ? "" : " "));
          if (node.Schema.Owner != null)
            sb.Append("AUTHORIZATION " + QuoteIdentifier(node.Schema.Owner.DbName));
          if (node.Schema.DefaultCharacterSet != null)
            sb.Append("DEFAULT CHARACTER SET " + Translate(node.Schema.DefaultCharacterSet));
          return sb.ToString();
        default:
          return String.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateSequence node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return
            "CREATE SEQUENCE " + Translate(node.Sequence) +
            (node.Sequence.DataType != null ? " AS " + Translate(node.Sequence.DataType) : "");
        default:
          return String.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      switch (section) {
        case CreateTableSection.Entry: {
          StringBuilder sb = new StringBuilder();
          sb.Append("CREATE ");
          TemporaryTable temporaryTable = node.Table as TemporaryTable;
          if (temporaryTable != null) {
            if (temporaryTable.IsGlobal)
              sb.Append("GLOBAL ");
            else
              sb.Append("LOCAL ");
            sb.Append("TEMPORARY ");
          }
          sb.Append("TABLE " + Translate(node.Table));
          return sb.ToString();
        }
        case CreateTableSection.TableElementsEntry:
          return "(";
        case CreateTableSection.TableElementsExit:
          return ")";
        case CreateTableSection.Partition:
          return Translate(node.Table.PartitionDescriptor, true);
        case CreateTableSection.Exit: {
          string result = string.IsNullOrEmpty(node.Table.Filegroup)
                            ? string.Empty
                            : " ON " + QuoteIdentifier(node.Table.Filegroup);
          TemporaryTable temporaryTable = node.Table as TemporaryTable;
          if (temporaryTable != null) {
            result += temporaryTable.PreserveRows ? "ON COMMIT PRESERVE ROWS" : "ON COMMIT DELETE ROWS";
          }
          return result;
        }
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SequenceDescriptor descriptor,
                                    SequenceDescriptorSection section)
    {
      switch (section) {
        case SequenceDescriptorSection.StartValue:
          if (descriptor.StartValue.HasValue)
            return "START WITH " + descriptor.StartValue.Value;
          return String.Empty;
        case SequenceDescriptorSection.RestartValue:
          if (descriptor.StartValue.HasValue)
            return "RESTART WITH " + descriptor.StartValue.Value;
          return String.Empty;
        case SequenceDescriptorSection.Increment:
          if (descriptor.Increment.HasValue)
            return "INCREMENT BY " + descriptor.Increment.Value;
          return String.Empty;
        case SequenceDescriptorSection.MaxValue:
          if (descriptor.MaxValue.HasValue)
            return "MAXVALUE " + descriptor.MaxValue.Value;
          return String.Empty;
        case SequenceDescriptorSection.MinValue:
          if (descriptor.MinValue.HasValue)
            return "MINVALUE " + descriptor.MinValue.Value;
          return String.Empty;
        case SequenceDescriptorSection.AlterMaxValue:
          if (descriptor.MaxValue.HasValue)
            return "MAXVALUE " + descriptor.MaxValue.Value;
          else
            return "NO MAXVALUE";
        case SequenceDescriptorSection.AlterMinValue:
          if (descriptor.MinValue.HasValue)
            return "MINVALUE " + descriptor.MinValue.Value;
          else
            return "NO MINVALUE";
        case SequenceDescriptorSection.IsCyclic:
          if (descriptor.IsCyclic.HasValue)
            return descriptor.IsCyclic.Value ? "CYCLE" : "NO CYCLE";
          return String.Empty;
        default:
          return String.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateTranslation node)
    {
      StringBuilder sb = new StringBuilder();
      //      sb.Append("CREATE TRANSLATION "+Translate(node.Translation));
      //      sb.Append(" FOR "+Translate(node.Translation.SourceCharacterSet));
      //      sb.Append(" TO "+Translate(node.Translation.SourceCharacterSet)+" FROM ");
      //      if (node.Translation.TranslationSource is IIdentityTranslation)
      //        sb.Append("IDENTITY");
      //      else if (node.Translation.TranslationSource is IExternalTranslation)
      //        sb.Append("EXTERNAL ("+QuoteString(((IExternalTranslation)node.Translation.TranslationSource).Name)+")");
      //      else if (node.Translation.TranslationSource is ITranslation)
      //        sb.Append(Translate((ITranslation)node.Translation.TranslationSource));
      return sb.ToString();
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateView node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          StringBuilder sb = new StringBuilder();
          sb.Append("CREATE VIEW " + Translate(node.View));
          if (node.View.ViewColumns.Count > 0) {
            sb.Append(" (");
            bool first = true;
            foreach (DataTableColumn c in node.View.ViewColumns) {
              if (first)
                first = false;
              else
                sb.Append(ColumnDelimiter);
              sb.Append(c.DbName);
            }
            sb.Append(")");
          }
          sb.Append(" AS");
          return sb.ToString();
        case NodeSection.Exit:
          switch (node.View.CheckOptions) {
            case CheckOptions.Cascaded:
              return "WITH CASCADED CHECK OPTION";
            case CheckOptions.Local:
              return "WITH LOCAL CHECK OPTION";
            default:
              return string.Empty;
          }
        default:
          return string.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlCursor node)
    {
      return node.Name;
    }

    public virtual string Translate(SqlCompilerContext context, SqlDeclareCursor node, DeclareCursorSection section)
    {
      switch (section) {
        case DeclareCursorSection.Entry:
          return "DECLARE " + node.Cursor.Name;
        case DeclareCursorSection.Sensivity:
          return (node.Cursor.Insensitive ? "INSENSITIVE " : string.Empty);
        case DeclareCursorSection.Scrollability:
          return (node.Cursor.Scroll ? "SCROLL " : string.Empty);
        case DeclareCursorSection.Cursor:
          return "CURSOR ";
        case DeclareCursorSection.For:
          return "FOR";
        case DeclareCursorSection.Holdability:
          return (node.Cursor.WithHold ? "WITH HOLD " : "WITHOUT HOLD ");
        case DeclareCursorSection.Returnability:
          return (node.Cursor.WithReturn ? "WITH RETURN " : "WITHOUT RETURN ");
        case DeclareCursorSection.Updatability:
          return (node.Cursor.ReadOnly) ? "FOR READ ONLY" : "FOR UPDATE";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlDeclareVariable node)
    {
      return "DECLARE @" + node.Variable.Name + " AS " + Translate(node.Variable.Type);
    }

    public virtual string Translate(SqlCompilerContext context, SqlDefaultValue node)
    {
      return "DEFAULT";
    }

    public virtual string Translate(SqlCompilerContext context, SqlDelete node, DeleteSection section)
    {
      switch (section) {
        case DeleteSection.Entry:
          return "DELETE FROM";
        case DeleteSection.Where:
          return "WHERE";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropAssertion node)
    {
      return "DROP ASSERTION " + Translate(node.Assertion);
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropCharacterSet node)
    {
      return "DROP CHARACTER SET " + Translate(node.CharacterSet);
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropCollation node)
    {
      return "DROP COLLATION " + Translate(node.Collation);
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropDomain node)
    {
      return "DROP DOMAIN " + Translate(node.Domain) + (node.Cascade ? " CASCADE" : " RESTRICT");
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      return "DROP INDEX " + QuoteIdentifier(node.Index.DbName) + " ON " + Translate(node.Index.DataTable);
      //bool addOptions = false;
      //StringBuilder sb = new StringBuilder();
      //sb.Append(" WITH (");
      //if (node.MaxDegreeOfParallelism.HasValue) {
      //  sb.Append("MAXDOP = "+node.MaxDegreeOfParallelism.Value);
      //  addOptions = true;
      //}
      //if (node.Online.HasValue) {
      //  if (addOptions)
      //    sb.Append(RowItemDelimiter);
      //  else
      //    addOptions = true;
      //  sb.Append("ONLINE = "+(node.Online.Value ? "ON" : "OFF"));
      //}
      //if (node.PartitionDescriptor!=null) {
      //  if (addOptions)
      //    sb.Append(RowItemDelimiter);
      //  else
      //    addOptions = true;
      //  sb.Append("MOVE TO "+Translate(context, node.PartitionDescriptor, false));
      //}
      //else if (!String.IsNullOrEmpty(node.Filegroup)) {
      //  if (addOptions)
      //    sb.Append(RowItemDelimiter);
      //  else
      //    addOptions = true;
      //  sb.Append("MOVE TO "+node.Filegroup);
      //}
      //sb.Append(")");
      //if (addOptions)
      //  return result+sb.ToString();
      //else
      //  return result;
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropPartitionFunction node)
    {
      return "DROP PARTITION FUNCTION " + QuoteIdentifier(node.PartitionFunction.DbName);
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropPartitionScheme node)
    {
      return "DROP PARTITION SCHEME " + QuoteIdentifier(node.PartitionSchema.DbName);
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropSchema node)
    {
      return "DROP SCHEMA " + QuoteIdentifier(node.Schema.DbName) + (node.Cascade ? " CASCADE" : " RESTRICT");
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropSequence node)
    {
      return "DROP SEQUENCE " + Translate(node.Sequence) + (node.Cascade ? " CASCADE" : " RESTRICT");
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropTable node)
    {
      return "DROP TABLE " + Translate(node.Table) + (node.Cascade ? " CASCADE" : " RESTRICT");
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropTranslation node)
    {
      return "DROP TRANSLATION " + Translate(node.Translation);
    }

    public virtual string Translate(SqlCompilerContext context, SqlDropView node)
    {
      return "DROP VIEW " + Translate(node.View) + (node.Cascade ? " CASCADE" : " RESTRICT");
    }

    public virtual string Translate(SqlCompilerContext context, SqlFastFirstRowsHint node)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlFetch node, FetchSection section)
    {
      switch (section) {
        case FetchSection.Entry:
          return "FETCH " + node.Option;
        case FetchSection.Targets:
          return "FROM " + node.Cursor.Name + ((node.Targets.Count != 0) ? " INTO" : string.Empty);
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlForceJoinOrderHint node)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
    {
      switch (section) {
        case FunctionCallSection.Entry:
          switch(node.FunctionType) {
            case SqlFunctionType.CurrentUser:
            case SqlFunctionType.SessionUser:
            case SqlFunctionType.SystemUser:
            case SqlFunctionType.User:
              return Translate(node.FunctionType);
          }
          if (node.FunctionType == SqlFunctionType.Position && driver.ServerInfo.StringIndexingBase > 0)
            return "(" + Translate(node.FunctionType) + "(";
          return (node.Arguments.Count == 0) ? Translate(node.FunctionType) + "()" : Translate(node.FunctionType) + "(";
        case FunctionCallSection.ArgumentEntry:
          switch (node.FunctionType) {
            case SqlFunctionType.Position:
              if (position == 1)
                return "IN";
              return string.Empty;
            case SqlFunctionType.Extract:
              if (position == 1)
                return "FROM";
              return string.Empty;
            case SqlFunctionType.Substring:
              if (position == 1)
                return "FROM";
              else if (position == 2)
                return "FOR";
              return String.Empty;
            default:
              return String.Empty;
          }
        case FunctionCallSection.ArgumentExit:
          if (node.FunctionType == SqlFunctionType.Substring && position == 1)
            return driver.ServerInfo.StringIndexingBase > 0 ? "+ " + driver.ServerInfo.StringIndexingBase : String.Empty;
          break;
        case FunctionCallSection.ArgumentDelimiter:
          switch(node.FunctionType) {
            case SqlFunctionType.Position:
            case SqlFunctionType.Extract:
            case SqlFunctionType.Substring:
              return String.Empty;
            default:
              return ArgumentDelimiter;
          }
        case FunctionCallSection.Exit:
          if (node.FunctionType == SqlFunctionType.Position && driver.ServerInfo.StringIndexingBase > 0)
            return
              ") - " + driver.ServerInfo.StringIndexingBase + ")";
          return (node.Arguments.Count != 0) ? ")" : string.Empty;
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlIf node, IfSection section)
    {
      switch (section) {
        case IfSection.Entry:
          return "IF";
        case IfSection.True:
          return "BEGIN";
        case IfSection.False:
          return "END BEGIN";
        case IfSection.Exit:
          return "END";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlInsert node, InsertSection section)
    {
      switch (section) {
        case InsertSection.Entry:
          return "INSERT INTO";
        case InsertSection.ColumnsEntry:
          return (node.Values.Keys.Count > 0) ? "(" : string.Empty;
        case InsertSection.ColumnsExit:
          return (node.Values.Keys.Count > 0) ? ")" : string.Empty;
        case InsertSection.ValuesEntry:
          return "VALUES (";
        case InsertSection.ValuesExit:
          return ")";
        case InsertSection.DefaultValues:
          return "DEFAULT VALUES";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlJoinExpression node, JoinSection section)
    {
      bool explicitJoinOrder = (driver.ServerInfo.Query.Features & QueryFeatures.ExplicitJoinOrder)>0;
      switch (section) {
        case JoinSection.Entry:
          return explicitJoinOrder ? "(" : string.Empty;
        case JoinSection.Specification:
          bool natural = false;
          if (SqlExpression.IsNull(node.Expression) && node.JoinType != SqlJoinType.CrossJoin &&
              node.JoinType != SqlJoinType.UnionJoin)
            natural = true;
          return
            (natural ? "NATURAL " : string.Empty) + Translate(node.JoinType) + " JOIN";
        case JoinSection.Condition:
          return
            node.JoinType == SqlJoinType.UsingJoin ? "USING" : "ON";
        case JoinSection.Exit:
          return explicitJoinOrder ? ")" : string.Empty;
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlJoinHint node)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlLike node, LikeSection section)
    {
      switch (section) {
        case LikeSection.Like:
          if (node.Not)
            return "NOT LIKE";
          else
            return "LIKE";
        case LikeSection.Escape:
          return "ESCAPE";
      }
      return string.Empty;
    }

    public virtual string Translate<T>(SqlCompilerContext context, SqlLiteral<T> node)
    {
      TypeCode t = Type.GetTypeCode(typeof (T));
      if (t == TypeCode.String || t == TypeCode.Char || typeof(T) == typeof(Guid))
        return QuoteString(Convert.ToString(node.Value, this));
      return Convert.ToString(node.Value, this);
    }

    public virtual string Translate(SqlCompilerContext context, SqlMatch node, MatchSection section)
    {
      switch (section) {
        case MatchSection.Specification:
          return " MATCH " + ((node.Unique) ? "UNIQUE " : string.Empty) + Translate(node.MatchType);
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlNative node)
    {
      return node.Value;
    }

    public virtual string Translate(SqlCompilerContext context, SqlNativeHint node)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch(section) {
        case NodeSection.Entry:
          return "NEXT VALUE FOR ";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlNull node)
    {
      return "NULL";
    }

    public virtual string Translate(SqlCompilerContext context, SqlOpenCursor node)
    {
      return "OPEN " + node.Cursor.Name;
    }

    public virtual string Translate(SqlCompilerContext context, SqlOrder node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          return (node.Ascending) ? "ASC" : "DESC";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlQueryExpression node, QueryExpressionSection section)
    {
      switch(section) {
        case QueryExpressionSection.All:
          return node.All ? " ALL" : string.Empty;
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlQueryRef node, TableSection section)
    {
      switch (section) {
        case TableSection.Entry:
          return "(";
        case TableSection.Exit:
          return ")";
        case TableSection.AliasDeclaration:
          return (string.IsNullOrEmpty(node.Name)) ? string.Empty : QuoteIdentifier(node.Name);
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlParameterRef node)
    {
      return "@" + node.Parameter.ParameterName;
    }

    public virtual string Translate(SqlCompilerContext context, SqlRow node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "(";
        case NodeSection.Exit:
          return ")";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
        case SelectSection.Entry:
          return (node.Distinct) ? "SELECT DISTINCT" : "SELECT";
        case SelectSection.From:
          return "FROM";
        case SelectSection.Where:
          return "WHERE";
        case SelectSection.GroupBy:
          return "GROUP BY";
        case SelectSection.Having:
          return "HAVING";
        case SelectSection.OrderBy:
          return "ORDER BY";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlStatementBlock node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "BEGIN";
        case NodeSection.Exit:
          return "End";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlSubQuery node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "(";
        case NodeSection.Exit:
          return ")";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlTable node, NodeSection section)
    {
      return QuoteIdentifier(node.Name);
    }

    public virtual string Translate(SqlCompilerContext context, SqlTableColumn node, NodeSection section)
    {
      return
        Translate(context, node.SqlTable, NodeSection.Entry) + "." +
        (((object)node == (object)node.SqlTable.Asterisk) ? node.Name : QuoteIdentifier(node.Name));
    }

    public virtual string Translate(SqlCompilerContext context, SqlTableLockHint node)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlTableRef node, TableSection section)
    {
      switch (section) {
        case TableSection.Entry:
          return Translate(node.DataTable);
        case TableSection.AliasDeclaration:
          return (node.Name != node.DataTable.DbName) ? " " + QuoteIdentifier(node.Name) : string.Empty;
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlTableScanHint node)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlTrim node, TrimSection section)
    {
      switch (section) {
        case TrimSection.Entry:
          return "TRIM(";
        case TrimSection.From:
          return "FROM";
        case TrimSection.Exit:
          return ")";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlUnary node, NodeSection section)
    {
      string result = string.Empty;
      switch (section) {
        case NodeSection.Entry:
          if (
            !(node.NodeType == SqlNodeType.Exists || node.NodeType == SqlNodeType.All ||
              node.NodeType == SqlNodeType.Some || node.NodeType == SqlNodeType.Any))
            result += "(";
          if (node.NodeType != SqlNodeType.IsNull)
            result += Translate(node.NodeType);
          break;
        case NodeSection.Exit:
          if (node.NodeType == SqlNodeType.IsNull)
            result += Translate(node.NodeType);
          if (
            !(node.NodeType == SqlNodeType.Exists || node.NodeType == SqlNodeType.All ||
              node.NodeType == SqlNodeType.Some || node.NodeType == SqlNodeType.Any))
            result += ")";
          break;
      }
      return result;
    }

    public virtual string Translate(SqlCompilerContext context, SqlUpdate node, UpdateSection section)
    {
      switch (section) {
        case UpdateSection.Entry:
          return "UPDATE";
        case UpdateSection.Set:
          return "SET";
        case UpdateSection.From:
          return "FROM";
        case UpdateSection.Where:
          return (node.Where is SqlCursor) ? "WHERE CURRENT OF" : "WHERE";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlUsePlanHint node)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlUserColumn node, NodeSection section)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlUserFunctionCall node, FunctionCallSection section, int position)
    {
      switch (section) {
        case FunctionCallSection.Entry:
          return node.Name + "(";
        case FunctionCallSection.Exit:
          return ")";
      }
      return Translate(context, node as SqlFunctionCall, section, position);
    }

    public virtual string Translate(SqlCompilerContext context, SqlVariable node)
    {
      return "@" + node.Name;
    }

    public virtual string Translate(SqlCompilerContext context, SqlWhile node, WhileSection section)
    {
      switch (section) {
        case WhileSection.Entry:
          return "WHILE (";
        case WhileSection.Statement:
          return ") BEGIN";
        case WhileSection.Exit:
          return "END";
      }
      return string.Empty;
    }

    public virtual string Translate(SqlNodeType type)
    {
      switch (type) {
        case SqlNodeType.All:
          return "ALL";
        case SqlNodeType.Any:
          return "ANY";
        case SqlNodeType.Some:
          return "SOME";
        case SqlNodeType.Exists:
          return "EXISTS";
        case SqlNodeType.BitAnd:
          return "&";
        case SqlNodeType.BitNot:
          return "~";
        case SqlNodeType.BitOr:
          return "|";
        case SqlNodeType.BitXor:
          return "^";
        case SqlNodeType.In:
          return "IN";
        case SqlNodeType.Between:
          return "BETWEEN";
        case SqlNodeType.And:
          return "AND";
        case SqlNodeType.Or:
          return "OR";
        case SqlNodeType.IsNull:
          return "IS NULL";
        case SqlNodeType.Not:
          return "NOT";
        case SqlNodeType.NotBetween:
          return "NOT BETWEEN";
        case SqlNodeType.NotIn:
          return "NOT IN";
        case SqlNodeType.GreaterThan:
          return ">";
        case SqlNodeType.GreaterThanOrEquals:
          return ">=";
        case SqlNodeType.LessThan:
          return "<";
        case SqlNodeType.LessThanOrEquals:
          return "<=";
        case SqlNodeType.Equals:
        case SqlNodeType.Assign:
          return "=";
        case SqlNodeType.NotEquals:
          return "<>";
        case SqlNodeType.Add:
          return "+";
        case SqlNodeType.Subtract:
        case SqlNodeType.Negate:
          return "-";
        case SqlNodeType.Multiply:
          return "*";
        case SqlNodeType.Modulo:
          return "%";
        case SqlNodeType.Divide:
          return "/";
        case SqlNodeType.Avg:
          return "AVG";
        case SqlNodeType.Count:
          return "COUNT";
        case SqlNodeType.Max:
          return "MAX";
        case SqlNodeType.Min:
          return "MIN";
        case SqlNodeType.Sum:
          return "SUM";
        case SqlNodeType.Concat:
          return "||";
        case SqlNodeType.Unique:
          return "UNIQUE";
        case SqlNodeType.Union:
          return "UNION";
        case SqlNodeType.Intersect:
          return "INTERSECT";
        case SqlNodeType.Except:
          return "EXCEPT";
        case SqlNodeType.Overlaps:
          return "OVERLAPS";
        default:
          return string.Empty;
      }
    }

    public virtual string Translate(SqlJoinType type)
    {
      switch (type) {
        case SqlJoinType.CrossJoin:
          return "CROSS";
        case SqlJoinType.FullOuterJoin:
          return "FULL OUTER";
        case SqlJoinType.InnerJoin:
          return "INNER";
        case SqlJoinType.LeftOuterJoin:
          return "LEFT OUTER";
        case SqlJoinType.UnionJoin:
          return "UNION";
        case SqlJoinType.RightOuterJoin:
          return "RIGHT OUTER";
        default:
          return string.Empty;
      }
    }

    public virtual string Translate(SqlMatchType type)
    {
      switch (type) {
        case SqlMatchType.Full:
          return "FULL";
        case SqlMatchType.Partial:
          return "PARTIAL";
        default:
          return string.Empty;
      }
    }

    private string Translate(PartitionDescriptor partitionDescriptor, bool withOn)
    {
      if (partitionDescriptor.PartitionSchema != null)
        return
          (withOn ? "ON " : "") + QuoteIdentifier(partitionDescriptor.PartitionSchema.DbName) + " (" +
          QuoteIdentifier(partitionDescriptor.Column.DbName) + ")";
      else {
        StringBuilder sb = new StringBuilder();
        sb.Append("PARTITION BY ");
        switch (partitionDescriptor.PartitionMethod) {
          case PartitionMethod.Hash:
            sb.Append("HASH");
            break;
          case PartitionMethod.List:
            sb.Append("LIST");
            break;
          case PartitionMethod.Range:
            sb.Append("RANGE");
            break;
        }
        sb.Append(" (" + QuoteIdentifier(partitionDescriptor.Column.DbName) + ")");
        if (partitionDescriptor.Partitions == null)
          sb.Append(" PARTITIONS " + partitionDescriptor.PartitionAmount);
        else {
          sb.Append(" (");
          bool first = true;
          switch (partitionDescriptor.PartitionMethod) {
            case PartitionMethod.Hash:
              foreach (HashPartition p in partitionDescriptor.Partitions) {
                if (first)
                  first = false;
                else
                  sb.Append(ColumnDelimiter);
                sb.Append("PARTITION " + QuoteIdentifier(p.DbName) +
                          (String.IsNullOrEmpty(p.Filegroup) ? "" : " TABLESPACE " + p.Filegroup));
              }
              break;
            case PartitionMethod.List:
              foreach (ListPartition p in partitionDescriptor.Partitions) {
                if (first)
                  first = false;
                else
                  sb.Append(ColumnDelimiter);
                sb.Append("PARTITION " + QuoteIdentifier(p.DbName) + " VALUES (");
                bool firstValue = true;
                foreach (string v in p.Values) {
                  if (firstValue)
                    firstValue = false;
                  else
                    sb.Append(RowItemDelimiter);
                  TypeCode t = Type.GetTypeCode(v.GetType());
                  if (t == TypeCode.String || t == TypeCode.Char)
                    sb.Append(QuoteString(v.ToString(this)));
                  else
                    sb.Append(v.ToString(this));
                }
                sb.Append(")");
                if (!String.IsNullOrEmpty(p.Filegroup))
                  sb.Append(" TABLESPACE " + p.Filegroup);
              }
              break;
            case PartitionMethod.Range:
              foreach (RangePartition p in partitionDescriptor.Partitions) {
                if (first)
                  first = false;
                else
                  sb.Append(ColumnDelimiter);
                sb.Append("PARTITION " + QuoteIdentifier(p.DbName) + " VALUES LESS THAN (");
                TypeCode t = Type.GetTypeCode(p.Boundary.GetType());
                if (t == TypeCode.String || t == TypeCode.Char)
                  sb.Append(QuoteString(p.Boundary.ToString(this)));
                else
                  sb.Append(p.Boundary.ToString(this));
                sb.Append(")");
                if (!String.IsNullOrEmpty(p.Filegroup))
                  sb.Append(" TABLESPACE " + p.Filegroup);
              }
              break;
          }
          sb.Append(")");
        }
        return sb.ToString();
      }
    }

    public virtual string Translate(SchemaNode node)
    {
      if (node.Schema != null)
        return QuoteIdentifier(node.Schema.DbName, node.DbName);
      else
        return QuoteIdentifier(node.DbName);
    }

    public virtual string Translate(SqlJoinMethod method)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlTableIsolationLevel level)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlTableLockType type)
    {
      return string.Empty;
    }

    public virtual string Translate(SqlTableScanMethod method)
    {
      return string.Empty;
    }

    public virtual string Translate(ReferentialAction action)
    {
      switch (action) {
        case ReferentialAction.Cascade:
          return "CASCADE";
        case ReferentialAction.SetDefault:
          return "SET DEFAULT";
        case ReferentialAction.SetNull:
          return "SET NULL";
        default:
          return string.Empty;
      }
    }

    public virtual string Translate(SqlValueType type)
    {
      DataTypeInfo dti = driver.ServerInfo.DataTypes[type.DataType];
      if (dti == null)
        throw new NotSupportedException(String.Format("Type '{0}' is not supported.", type.DataType));
      string[] nativeTypes = dti.GetNativeTypes();
      string text = nativeTypes[0];

      if (type.Size != 0)
        return text + "(" + type.Size + ")";
      else if (type.Precision != 0 && type.Scale != 0)
        return text + "(" + type.Precision + "," + type.Scale + ")";
      else if (type.Precision != 0)
        return text + "(" + type.Precision + ")";
      return text;
    }

    //
    //    public virtual string Translate(RelationalIndexOption option)
    //    {
    //      switch (option) {
    //        case RelationalIndexOption.AllowPageLocks:
    //          return "ALLOW_PAGE_LOCKS";
    //        case RelationalIndexOption.AllowRowLocks:
    //          return "ALLOW_ROW_LOCKS";
    //        case RelationalIndexOption.DropExisting:
    //          return "DROP_EXISTING";
    //        case RelationalIndexOption.IgnoreDupKey:
    //          return "IGNORE_DUP_KEY";
    //        case RelationalIndexOption.Online:
    //          return "ONLINE";
    //        case RelationalIndexOption.PadIndex:
    //          return "PAD_INDEX";
    //        case RelationalIndexOption.SortInTEMPDB:
    //          return "SORT_IN_TEMPDB";
    //        case RelationalIndexOption.StatisticsNorecompute:
    //          return "STATISTICS_NORECOMPUTE";
    //        default:
    //          return String.Empty;
    //      }
    //    }

    public virtual string Translate(SqlFunctionType type)
    {
      switch (type) {
        case SqlFunctionType.BitLength:
        case SqlFunctionType.CharLength:
        case SqlFunctionType.Length:
        case SqlFunctionType.OctetLength:
          return "LENGTH";
        case SqlFunctionType.Concat:
          return "CONCAT";
          //        case SqlFunctionType.Convert:
          //          return "CONVERT";
        case SqlFunctionType.CurrentDate:
          return "CURRENTDATE";
        case SqlFunctionType.CurrentTime:
          return "CURRENTTIME";
        case SqlFunctionType.CurrentTimeStamp:
          return "CURRENTTIMESTAMP";
        case SqlFunctionType.Extract:
          return "EXTRACT";
        case SqlFunctionType.Lower:
          return "LOWER";
        case SqlFunctionType.Position:
          return "POSITION";
        case SqlFunctionType.Substring:
          return "SUBSTRING";
          //        case SqlFunctionType.Translate:
          //          return "TRANSLATE";
//        case SqlFunctionType.Trim:
//          return "TRIM";
        case SqlFunctionType.Upper:
          return "UPPER";
        case SqlFunctionType.Abs:
          return "ABS";
        case SqlFunctionType.Acos:
          return "ACOS";
        case SqlFunctionType.Asin:
          return "ASIN";
        case SqlFunctionType.Atan:
          return "ATAN";
        case SqlFunctionType.Ceiling:
          return "CEILING";
        case SqlFunctionType.Coalesce:
          return "COALESCE";
        case SqlFunctionType.Cos:
          return "COS";
        case SqlFunctionType.Cot:
          return "COT";
        case SqlFunctionType.CurrentUser:
          return "CURRENT_USER";
        case SqlFunctionType.Degrees:
          return "DEGREES";
        case SqlFunctionType.Exp:
          return "EXP";
        case SqlFunctionType.Floor:
          return "FLOOR";
        case SqlFunctionType.Log:
          return "LOG";
        case SqlFunctionType.Log10:
          return "LOG10";
        case SqlFunctionType.Pi:
          return "PI";
        case SqlFunctionType.Power:
          return "POWER";
        case SqlFunctionType.Radians:
          return "RADIANS";
        case SqlFunctionType.Rand:
          return "RAND";
        case SqlFunctionType.Replace:
          return "REPLACE";
        case SqlFunctionType.Round:
          return "ROUND";
        case SqlFunctionType.SessionUser:
          return "SESSION_USER";
        case SqlFunctionType.Sign:
          return "SIGN";
        case SqlFunctionType.Sin:
          return "SIN";
        case SqlFunctionType.Sqrt:
          return "SQRT";
        case SqlFunctionType.Square:
          return "SQUARE";
        case SqlFunctionType.SystemUser:
          return "SYSTEM_USER";
        case SqlFunctionType.Tan:
          return "TAN";
        default:
          return string.Empty;
      }
    }

    public virtual string Translate(SqlTrimType type)
    {
      switch (type) {
        case SqlTrimType.Leading:
          return "LEADING";
        case SqlTrimType.Trailing:
          return "TRAILING";
        case SqlTrimType.Both:
          return "BOTH";
        default:
          return string.Empty;
      }
    }

    /// <summary>
    /// Returns quoted string.
    /// </summary>
    /// <param name="str">Unquoted string.</param>
    /// <returns>Quoted string.</returns>
    public virtual string QuoteString(string str)
    {
      return "'" + str.Replace("'", "''") + "'";
    }

    /// <summary>
    /// Returns string holding quoted identifier name.
    /// </summary>
    /// <param name="names">An <see cref="Array"/> of unquoted identifier name parts.</param>
    /// <returns>Quoted identifier name.</returns>
    public virtual string QuoteIdentifier(params string[] names)
    {
      return "[" + string.Join("].[", names) + "]";
    }

    #region IFormatProvider Members

    ///<summary>
    ///Gets an object that provides formatting services for the specified type.
    ///</summary>
    ///
    ///<returns>
    ///The current instance, if formatType is the same type as the current instance; otherwise, null.
    ///</returns>
    ///
    ///<param name="formatType">An object that specifies the type of format object to get. </param><filterpriority>1</filterpriority>
    public object GetFormat(Type formatType)
    {
      if (formatType == typeof (NumberFormatInfo))
        return numberFormat;
      if (formatType == typeof (DateTimeFormatInfo))
        return dateTimeFormat;
      return null;
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlTranslator"/> class.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected SqlTranslator(SqlDriver driver)
    {
      this.driver = driver;
    }
  }
}