// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Reflection;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Resources;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// SQL translator.
  /// </summary>
  public abstract class SqlTranslator : SqlDriverBound
  {
    public DateTimeFormatInfo DateTimeFormat { get; private set; }
    public NumberFormatInfo IntegerNumberFormat { get; private set; }
    public NumberFormatInfo FloatNumberFormat { get; private set; }
    public NumberFormatInfo DoubleNumberFormat { get; private set; }

    public virtual string NewLine { get { return "\r\n"; } }

    public virtual string OpeningParenthesis { get { return "("; } }
    public virtual string ClosingParenthesis { get { return ")"; } }

    public virtual string BatchBegin { get { return string.Empty; } }
    public virtual string BatchEnd { get { return string.Empty; } }
    public virtual string BatchItemDelimiter { get { return ";"; } }
    
    public virtual string RowBegin { get { return "("; } }
    public virtual string RowEnd { get { return ")"; } }
    public virtual string RowItemDelimiter { get { return ","; } }

    public virtual string ArgumentDelimiter { get { return ","; } }
    public virtual string ColumnDelimiter { get { return ","; } }
    public virtual string WhenDelimiter { get { return string.Empty; } }
    public virtual string DdlStatementDelimiter { get { return string.Empty; } }
    public virtual string HintDelimiter { get { return string.Empty; } }

    /// <summary>
    /// Gets the float format string.
    /// See <see cref="double.ToString(string)"/> for details.
    /// </summary>
    public virtual string FloatFormatString { get { return "0.0######"; } }
    
    /// <summary>
    /// Gets the double format string.
    /// See <see cref="double.ToString(string)"/> for details.
    /// </summary>
    public virtual string DoubleFormatString { get { return "0.0##############"; } }

    /// <summary>
    /// Gets the date time format string.
    /// See <see cref="DateTime.ToString(string)"/> for details.
    /// </summary>
    public abstract string DateTimeFormatString { get; }

    /// <summary>
    /// Gets the time span format string.
    /// See <see cref="SqlHelper.TimeSpanToString"/> for details.
    /// </summary>
    public abstract string TimeSpanFormatString { get; }
    
    /// <summary>
    /// Gets the parameter prefix.
    /// </summary>
    public string ParameterPrefix { get; private set; }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public virtual void Initialize()
    {
      IntegerNumberFormat = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
      FloatNumberFormat = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
      DoubleNumberFormat = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
      DateTimeFormat = (DateTimeFormatInfo) CultureInfo.InvariantCulture.DateTimeFormat.Clone();

      var queryInfo = Driver.ServerInfo.Query;
      ParameterPrefix = queryInfo.Features.Supports(QueryFeatures.ParameterPrefix)
        ? queryInfo.ParameterPrefix
        : string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlAggregate node, NodeSection section)
    {
      switch (section) {
      case NodeSection.Entry:
        return Translate(node.NodeType) + "(" + (node.Distinct ? "DISTINCT" : string.Empty);
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
      var builder = new StringBuilder();
      builder.Append("ALTER PARTITION FUNCTION " + QuoteIdentifier(node.PartitionFunction.DbName) + "()");
      if (node.Option == SqlAlterPartitionFunctionOption.Split)
        builder.Append(" SPLIT RANGE (");
      else
        builder.Append(" MERGE RANGE (");
      builder.Append(node.Boundary + ")");
      return builder.ToString();
    }

    public virtual string Translate(SqlCompilerContext context, SqlAlterPartitionScheme node)
    {
      return
        "ALTER PARTITION SCHEME " + QuoteIdentifier(node.PartitionSchema.DbName) + " NEXT USED" +
        (string.IsNullOrEmpty(node.Filegroup) ? "" : " " + node.Filegroup);
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
      case AlterTableSection.RenameColumn:
        return "RENAME COLUMN";
      case AlterTableSection.To:
        return "TO";
      case AlterTableSection.DropBehavior:
        var cascadableAction = node.Action as SqlCascadableAction;
        if (cascadableAction==null)
          return string.Empty;
        return cascadableAction.Cascade ? "CASCADE" : "RESTRICT";
      default:
        return string.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlAlterSequence node, NodeSection section)
    {
      switch (section) {
      case NodeSection.Entry:
        return "ALTER SEQUENCE " + Translate(node.Sequence);
      default:
        return string.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      switch (section) {
      case TableColumnSection.Entry:
        return QuoteIdentifier(column.DbName);
      case TableColumnSection.Type:
        return column.Domain==null ? Translate(column.DataType) : Translate(column.Domain);
      case TableColumnSection.DefaultValue:
        return "DEFAULT";
      case TableColumnSection.DropDefault:
        return "DROP DEFAULT";
      case TableColumnSection.SetDefault:
        return "SET DEFAULT";
      case TableColumnSection.GenerationExpressionExit:
        return ")" + (column.IsPersisted ? " PERSISTED" : "");
      case TableColumnSection.SetIdentityInfoElement:
        return "SET";
      case TableColumnSection.NotNull:
        return "NOT NULL";
      case TableColumnSection.Collate:
        return "COLLATE " + Translate(column.Collation);
      default:
        return string.Empty;
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

    public virtual string Translate(SqlCompilerContext context, SqlArray node, ArraySection section)
    {
      switch (section) {
      case ArraySection.Entry:
        return "(";
      case ArraySection.Exit:
        return ")";
      case ArraySection.EmptyArray:
        return "(NULL)";
      default:
        throw new ArgumentOutOfRangeException("section");
      }
    }

//      Type itemType = node.ItemType;
//      object[] values = node.GetValues();
//      int count = values.Length;
//      if (count==0)
//        return "(NULL)";
//      var buffer = new string[count];
//      for (int index = 0; index < count; index++)
//        buffer[index] = Translate(context, (SqlLiteral) SqlDml.Literal(values[index], itemType));
//      if (count==1)
//        return "(" + buffer[0] + ")";
//
//      buffer[0] = "(" + buffer[0];
//      buffer[count - 1] += ")";
//      return String.Join(RowItemDelimiter, buffer);
//    }

    public virtual string Translate(SqlCompilerContext context, SqlAssignment node, NodeSection section)
    {
      switch (section) {
      case NodeSection.Entry:
        return "SET";
      }
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
         return (node.NodeType==SqlNodeType.RawConcat) ? string.Empty : OpeningParenthesis;
      case NodeSection.Exit:
         return (node.NodeType==SqlNodeType.RawConcat) ? string.Empty : ClosingParenthesis;
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
        return "(CASE";
      case CaseSection.Else:
        return "ELSE";
      case CaseSection.Exit:
        return "END)";
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

    public virtual string Translate(SqlCompilerContext context, SqlConcat node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "(";
        case NodeSection.Exit:
          return ")";
        default:
          return string.Empty;
      }
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

    public virtual string Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      switch (section) {
        case CreateIndexSection.Entry:
          Index index = node.Index;
          if (index.IsFullText) {
            return "CREATE FULLTEXT INDEX ON " + Translate(index.DataTable);
          }
          var builder = new StringBuilder();
          builder.Append("CREATE ");
          if (index.IsUnique)
            builder.Append("UNIQUE ");
          else if (index.IsBitmap)
            builder.Append("BITMAP ");
          else if (index.IsSpatial)
            builder.Append("SPATIAL ");
          if (Driver.ServerInfo.Index.Features.Supports(IndexFeatures.Clustered))
            if (index.IsClustered)
              builder.Append("CLUSTERED ");
          builder.Append("INDEX " + QuoteIdentifier(index.DbName));
          builder.Append(" ON " + Translate(index.DataTable));
          return builder.ToString();
        case CreateIndexSection.ColumnsEnter:
          return "(";
        case CreateIndexSection.ColumnsExit:
          return ")";
        case CreateIndexSection.NonkeyColumnsEnter:
          return " INCLUDE (";
        case CreateIndexSection.NonkeyColumnsExit:
          return ")";
        case CreateIndexSection.Where:
          return " WHERE";
        case CreateIndexSection.Exit:
          index = node.Index;
          builder = new StringBuilder();
          if (index.FillFactor.HasValue) {
            builder.Append(" WITH (FILLFACTOR = " + index.FillFactor.Value + ")");
          }
          if (index.PartitionDescriptor != null) {
            builder.Append(" " + Translate(index.PartitionDescriptor, true));
          }
          else if (!String.IsNullOrEmpty(index.Filegroup))
            builder.Append(" ON " + index.Filegroup);
          else if (index is FullTextIndex) {
            var ftindex = index as FullTextIndex;
            builder.Append(" KEY INDEX " + QuoteIdentifier(ftindex.UnderlyingUniqueIndex));
          }
          return builder.ToString();
        default:
          return string.Empty;
      }
    }

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
          sb.Append(QuoteString(value));
        else
          sb.Append(value);
      }
      sb.Append(")");
      return sb.ToString();
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreatePartitionScheme node)
    {
      PartitionSchema ps = node.PartitionSchema;
      var sb = new StringBuilder();
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
          var sb = new StringBuilder();
          sb.Append("CREATE SCHEMA ");
          if (!String.IsNullOrEmpty(node.Schema.DbName))
            sb.Append(QuoteIdentifier(node.Schema.DbName) + (node.Schema.Owner == null ? "" : " "));
          if (node.Schema.Owner != null)
            sb.Append("AUTHORIZATION " + QuoteIdentifier(node.Schema.Owner));
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
          "CREATE SEQUENCE " + Translate(node.Sequence);
      default:
        return string.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      switch (section) {
        case CreateTableSection.Entry: {
          var sb = new StringBuilder();
          sb.Append("CREATE ");
          var temporaryTable = node.Table as TemporaryTable;
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
          var temporaryTable = node.Table as TemporaryTable;
          if (temporaryTable != null) {
            result += temporaryTable.PreserveRows ? "ON COMMIT PRESERVE ROWS" : "ON COMMIT DELETE ROWS";
          }
          return result;
        }
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context,
      SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      switch (section) {
      case SequenceDescriptorSection.StartValue:
        if (descriptor.StartValue.HasValue)
          return "START WITH " + descriptor.StartValue.Value;
        return string.Empty;
      case SequenceDescriptorSection.RestartValue:
        if (descriptor.StartValue.HasValue)
          return "RESTART WITH " + descriptor.StartValue.Value;
        return string.Empty;
      case SequenceDescriptorSection.Increment:
        if (descriptor.Increment.HasValue)
          return "INCREMENT BY " + descriptor.Increment.Value;
        return string.Empty;
      case SequenceDescriptorSection.MaxValue:
        if (descriptor.MaxValue.HasValue)
          return "MAXVALUE " + descriptor.MaxValue.Value;
        return string.Empty;
      case SequenceDescriptorSection.MinValue:
        if (descriptor.MinValue.HasValue)
          return "MINVALUE " + descriptor.MinValue.Value;
        return string.Empty;
      case SequenceDescriptorSection.AlterMaxValue:
        return descriptor.MaxValue.HasValue ? "MAXVALUE " + descriptor.MaxValue.Value : "NO MAXVALUE";
      case SequenceDescriptorSection.AlterMinValue:
        return descriptor.MinValue.HasValue ? "MINVALUE " + descriptor.MinValue.Value : "NO MINVALUE";
      case SequenceDescriptorSection.IsCyclic:
        if (descriptor.IsCyclic.HasValue)
          return descriptor.IsCyclic.Value ? "CYCLE" : "NO CYCLE";
        return string.Empty;
      default:
        return string.Empty;
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlCreateTranslation node)
    {
      var sb = new StringBuilder();
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
          var sb = new StringBuilder();
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
      case DeleteSection.From:
        return "FROM";
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
      if (!node.Index.IsFullText)
        return "DROP INDEX " + QuoteIdentifier(node.Index.DbName) + " ON " + Translate(node.Index.DataTable);
      else 
        return "DROP FULLTEXT INDEX ON " + Translate(node.Index.DataTable);
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
          if (node.FunctionType==SqlFunctionType.Position && Driver.ServerInfo.StringIndexingBase > 0)
            return "(" + Translate(node.FunctionType) + "(";
          return (node.Arguments.Count == 0) ? Translate(node.FunctionType) + "()" : Translate(node.FunctionType) + "(";
        case FunctionCallSection.ArgumentEntry:
          switch (node.FunctionType) {
          case SqlFunctionType.Position:
            return position == 1 ? "IN" : string.Empty;
          case SqlFunctionType.Substring:
            switch (position) {
            case 1:
              return "FROM";
            case 2:
              return "FOR";
            default:
              return string.Empty;
            }
          default:
            return string.Empty;
          }
        case FunctionCallSection.ArgumentExit:
          if (node.FunctionType==SqlFunctionType.Substring && position == 1)
            return Driver.ServerInfo.StringIndexingBase > 0 ? "+ " + Driver.ServerInfo.StringIndexingBase : string.Empty;
          break;
        case FunctionCallSection.ArgumentDelimiter:
          switch(node.FunctionType) {
            case SqlFunctionType.Position:
            case SqlFunctionType.Substring:
              return String.Empty;
            default:
              return ArgumentDelimiter;
          }
        case FunctionCallSection.Exit:
          if (node.FunctionType==SqlFunctionType.Position && Driver.ServerInfo.StringIndexingBase > 0)
            return
              ") - " + Driver.ServerInfo.StringIndexingBase + ")";
          return (node.Arguments.Count != 0) ? ")" : string.Empty;
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlExtract extract, ExtractSection section)
    {
      switch (section) {
      case ExtractSection.Entry:
        return "EXTRACT(";
      case ExtractSection.From:
        return "FROM";
      case ExtractSection.Exit:
        return ")";
      default:
        return string.Empty;
      }
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
      bool explicitJoinOrder = Driver.ServerInfo.Query.Features.Supports(QueryFeatures.ExplicitJoinOrder);
      switch (section) {
      case JoinSection.Entry:
        return explicitJoinOrder ? "(" : string.Empty;
      case JoinSection.Specification:
        bool natural = false;
        if (node.Expression.IsNullReference() && node.JoinType != SqlJoinType.CrossJoin &&
            node.JoinType != SqlJoinType.UnionJoin)
          natural = true;
        return
          (natural ? "NATURAL " : string.Empty) + Translate(node.JoinType) + " JOIN";
      case JoinSection.Condition:
        return
          node.JoinType==SqlJoinType.UsingJoin ? "USING" : "ON";
      case JoinSection.Exit:
        return explicitJoinOrder ? ")" : string.Empty;
      }
      return string.Empty;
    }

    public virtual string Translate(SqlCompilerContext context, SqlLike node, LikeSection section)
    {
      switch (section) {
      case LikeSection.Entry:
        return "(";
      case LikeSection.Exit:
        return ")";
      case LikeSection.Like:
        return node.Not ? "NOT LIKE" : "LIKE";
      case LikeSection.Escape:
        return "ESCAPE";
      default:
        return string.Empty;
      }
    }
    
    public virtual string Translate(SqlCompilerContext context, object literalValue)
    {
      var literalType = literalValue.GetType();
      switch (Type.GetTypeCode(literalType)) {
      case TypeCode.Char:
      case TypeCode.String:
        return QuoteString(literalValue.ToString());
      case TypeCode.DateTime:
        return ((DateTime) literalValue).ToString(DateTimeFormatString, DateTimeFormat);
      case TypeCode.Single:
        return ((float) literalValue).ToString(FloatFormatString, FloatNumberFormat);
      case TypeCode.Double:
        return ((double) literalValue).ToString(DoubleFormatString, DoubleNumberFormat);
      case TypeCode.Byte:
      case TypeCode.SByte:
      case TypeCode.Int16:
      case TypeCode.UInt16:
      case TypeCode.Int32:
      case TypeCode.UInt32:
      case TypeCode.Int64:
      case TypeCode.UInt64:
      case TypeCode.Decimal:
        return Convert.ToString(literalValue, IntegerNumberFormat);
      }
      if (literalType == typeof(TimeSpan))
        return SqlHelper.TimeSpanToString(((TimeSpan) literalValue), TimeSpanFormatString);
      if (literalType==typeof(Guid) || literalType==typeof(byte[]))
        throw new NotSupportedException(string.Format(
          Strings.ExTranslationOfLiteralOfTypeXIsNotSupported, literalType.GetShortName()));
      return literalValue.ToString();
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
        return node.Query is SqlFreeTextTable ? String.Empty : "(";
      case TableSection.Exit:
        return node.Query is SqlFreeTextTable ? String.Empty : ")";
      case TableSection.AliasDeclaration:
          string alias = context.TableNameProvider.GetName(node);
        return (string.IsNullOrEmpty(alias)) ? string.Empty : QuoteIdentifier(alias);
      }
      return string.Empty;
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

    public virtual string Translate(SqlCompilerContext context, SqlRowNumber node, NodeSection section)
    {
      switch (section) {
      case NodeSection.Entry:
        return "ROW_NUMBER() OVER(ORDER BY";
      case NodeSection.Exit:
        return ")";
      default:
        throw new ArgumentOutOfRangeException("section");
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlRenameTable node)
    {
      return string.Format("ALTER TABLE {0} RENAME TO {1}", Translate(node.Table), QuoteIdentifier(node.NewName));
    }

    public virtual string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
      case SelectSection.Entry:
        return node.Distinct ? "SELECT DISTINCT" : "SELECT";
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
      case SelectSection.Limit:
        return "LIMIT";
      case SelectSection.Offset:
        return "OFFSET";
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
      return QuoteIdentifier(context.TableNameProvider.GetName(node));
    }

    public virtual string Translate(SqlCompilerContext context, SqlTableColumn node, NodeSection section)
    {
      if ((context.NamingOptions & SqlCompilerNamingOptions.TableQualifiedColumns) == 0)
        return (((object)node == (object)node.SqlTable.Asterisk) ? node.Name : QuoteIdentifier(node.Name));
      else
        return
          Translate(context, node.SqlTable, NodeSection.Entry) + "." +
          (((object)node == (object)node.SqlTable.Asterisk) ? node.Name : QuoteIdentifier(node.Name));
    }

    public virtual string Translate(SqlCompilerContext context, SqlTableRef node, TableSection section)
    {
      switch (section) {
      case TableSection.Entry:
        return Translate(node.DataTable);
      case TableSection.AliasDeclaration:
          string alias = context.TableNameProvider.GetName(node);
        return (alias != node.DataTable.DbName) ? " " + QuoteIdentifier(alias) : string.Empty;
      }
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
        if (node.NodeType != SqlNodeType.IsNull && node.NodeType != SqlNodeType.IsNotNull)
          result += Translate(node.NodeType);
        break;
      case NodeSection.Exit:
        if (node.NodeType == SqlNodeType.IsNull || node.NodeType == SqlNodeType.IsNotNull)
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

    public virtual string Translate(SqlCompilerContext context, SqlCommand node)
    {
      switch (node.CommandType) {
      case SqlCommandType.SetConstraintsAllDeferred:
        return "SET CONSTRAINTS ALL DEFERRED";
      case SqlCommandType.SetConstraintsAllImmediate:
        return "SET CONSTRAINTS ALL IMMEDIATE";
      default:
        throw new NotSupportedException(string.Format(Strings.ExOperationXIsNotSupported, node.CommandType));
      }
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
      case SqlNodeType.IsNotNull:
        return "IS NOT NULL";
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
      case SqlNodeType.RawConcat:
        return string.Empty;
      default:
        throw new NotSupportedException(string.Format(Strings.ExOperationXIsNotSupported, type));
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
      case SqlJoinType.CrossApply:
      case SqlJoinType.LeftOuterApply:
        throw SqlHelper.NotSupported(QueryFeatures.CrossApply);
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
                    sb.Append(QuoteString(v));
                  else
                    sb.Append(v);
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
                  sb.Append(QuoteString(p.Boundary));
                else
                  sb.Append(p.Boundary);
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
      return node.Schema != null
        ? QuoteIdentifier(node.Schema.DbName, node.DbName)
        : QuoteIdentifier(node.DbName);
    }

    public virtual string Translate(Collation collation)
    {
      return QuoteString(collation.DbName);
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
      if (type.TypeName!=null)
        return type.TypeName;

      DataTypeInfo dti = Driver.ServerInfo.DataTypes[type.Type];
      if (dti==null)
        throw new NotSupportedException(String.Format(Strings.ExTypeXIsNotSupported, type.Type));
      string text = dti.NativeTypes.First();

      if (type.Length.HasValue)
        return text + "(" + type.Length + ")";
      if (type.Precision.HasValue && type.Scale.HasValue)
        return text + "(" + type.Precision + "," + type.Scale + ")";
      if (type.Precision.HasValue)
        return text + "(" + type.Precision + ")";
      return text;
    }

    public virtual string Translate(SqlFunctionType type)
    {
      switch (type) {
      case SqlFunctionType.CharLength:
      case SqlFunctionType.BinaryLength:
        return "LENGTH";
      case SqlFunctionType.Concat:
        return "CONCAT";
      case SqlFunctionType.CurrentDate:
        return "CURRENT_DATE";
      case SqlFunctionType.CurrentTime:
        return "CURRENT_TIME";
      case SqlFunctionType.CurrentTimeStamp:
        return "CURRENT_TIMESTAMP";
      case SqlFunctionType.Lower:
        return "LOWER";
      case SqlFunctionType.Position:
        return "POSITION";
      case SqlFunctionType.Substring:
        return "SUBSTRING";
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
      case SqlFunctionType.Atan2:
        return "ATAN2";
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
      case SqlFunctionType.NullIf:
        return "NULLIF";
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
      case SqlFunctionType.Truncate:
        return "TRUNCATE";
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
        throw new NotSupportedException(string.Format(Strings.ExFunctionXIsNotSupported, type));
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

    public virtual string Translate(SqlDateTimePart dateTimePart)
    {
      switch (dateTimePart) {
      case SqlDateTimePart.Year:
        return "YEAR";
      case SqlDateTimePart.Month:
        return "MONTH";
      case SqlDateTimePart.Day:
        return "DAY";
      case SqlDateTimePart.Hour:
        return "HOUR";
      case SqlDateTimePart.Minute:
        return "MINUTE";
      case SqlDateTimePart.Second:
        return "SECOND";
      case SqlDateTimePart.Millisecond:
        return "MILLISECOND";
      case SqlDateTimePart.Nanosecond:
        return "NANOSECOND";
      case SqlDateTimePart.TimeZoneHour:
        return "TIMEZONE_HOUR";
      case SqlDateTimePart.TimeZoneMinute:
        return "TIMEZONE_MINUTE";
      case SqlDateTimePart.DayOfYear:
        return "DAYOFYEAR";
      case SqlDateTimePart.DayOfWeek:
        return "DAYOFWEEK";
      default:
        throw new ArgumentOutOfRangeException("dateTimePart");
      }
    }

    public virtual string Translate(SqlIntervalPart intervalPart)
    {
      switch (intervalPart) {
      case SqlIntervalPart.Day:
        return "DAY";
      case SqlIntervalPart.Hour:
        return "HOUR";
      case SqlIntervalPart.Minute:
        return "MINUTE";
      case SqlIntervalPart.Second:
        return "SECOND";
      case SqlIntervalPart.Millisecond:
        return "MILLISECOND";
      case SqlIntervalPart.Nanosecond:
        return "NANOSECOND";
      default:
        throw new ArgumentOutOfRangeException("intervalPart");
      }
    }

    public virtual string Translate(SqlLockType lockType)
    {
      throw new NotSupportedException(string.Format(Strings.ExLockXIsNotSupported, lockType.ToString(true)));
    }

    public virtual string Translate(SqlJoinMethod method)
    {
      return string.Empty;
    }

    /// <summary>
    /// Returns quoted string.
    /// </summary>
    /// <param name="str">Unquoted string.</param>
    /// <returns>Quoted string.</returns>
    public virtual string QuoteString(string str)
    {
      return SqlHelper.QuoteString(str);
    }

    /// <summary>
    /// Returns string holding quoted identifier name.
    /// </summary>
    /// <param name="names">An <see cref="Array"/> of unquoted identifier name parts.</param>
    /// <returns>Quoted identifier name.</returns>
    public virtual string QuoteIdentifier(params string[] names)
    {
      return SqlHelper.QuoteIdentifierWithBrackets(names);
    }

    /// <summary>
    /// Builds the batch from specified SQL statements.
    /// </summary>
    /// <param name="statements">The statements.</param>
    /// <returns>String containing the whole batch.</returns>
    public virtual string BuildBatch(string[] statements)
    {
      if (statements.Length==0)
        return string.Empty;
      var expectedLength = BatchBegin.Length + BatchEnd.Length +
        statements.Sum(statement => statement.Length + BatchItemDelimiter.Length + NewLine.Length);
      var builder = new StringBuilder(expectedLength);
      builder.Append(BatchBegin);
      foreach (var statement in statements) {
        var actualStatement = statement
          .TryCutPrefix(BatchBegin)
          .TryCutSuffix(BatchEnd)
          .TryCutSuffix(NewLine)
          .TryCutSuffix(BatchItemDelimiter)
          .Trim();
        if (actualStatement.Length==0)
          continue;
        builder.Append(actualStatement);
        builder.Append(BatchItemDelimiter);
        builder.Append(NewLine);
      }
      builder.Append(BatchEnd);
      return builder.ToString();
    }

    public virtual string TranslateSortOrder(bool ascending)
    {
      return ascending ? "ASC" : "DESC";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlTranslator"/> class.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected SqlTranslator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}