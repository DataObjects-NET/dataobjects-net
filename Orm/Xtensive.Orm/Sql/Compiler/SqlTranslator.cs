// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Index = Xtensive.Sql.Model.Index;

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

    public virtual string NewLine => "\r\n";

    public virtual string OpeningParenthesis => "(";
    public virtual string ClosingParenthesis => ")";

    public virtual string BatchBegin => string.Empty;
    public virtual string BatchEnd => string.Empty;
    public virtual string BatchItemDelimiter => ";";

    public virtual string RowBegin => "(";
    public virtual string RowEnd => ")";
    public virtual string RowItemDelimiter => ",";

    public virtual string ArgumentDelimiter => ",";
    public virtual string ColumnDelimiter => ",";
    public virtual string WhenDelimiter => string.Empty;
    public virtual string DdlStatementDelimiter => string.Empty;
    public virtual string HintDelimiter => string.Empty;

    /// <summary>
    /// Gets the float format string.
    /// See <see cref="double.ToString(string)"/> for details.
    /// </summary>
    public string FloatFormatString { get; protected set; } = "0.0######";

    /// <summary>
    /// Gets the double format string.
    /// See <see cref="double.ToString(string)"/> for details.
    /// </summary>
    public string DoubleFormatString { get; protected set; } = "0.0##############";

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

    public virtual void Translate(SqlCompilerContext context, SqlAggregate node, NodeSection section)
    {
      var output = context.Output;
      switch (section) {
        case NodeSection.Entry:
          output.Append(Translate(node.NodeType))
            .Append("(")
            .Append(node.Distinct ? "DISTINCT" : string.Empty);
          break;
        case NodeSection.Exit:
          output.AppendClosingPunctuation(")");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlAlterDomain node, AlterDomainSection section)
    {
      var output = context.Output;
      switch (section) {
        case AlterDomainSection.Entry:
          output.Append("ALTER DOMAIN ");
          Translate(context, node.Domain);
          break;
        case AlterDomainSection.AddConstraint:
          output.Append("ADD");
          break;
        case AlterDomainSection.DropConstraint:
          output.Append("DROP");
          break;
        case AlterDomainSection.SetDefault:
          output.Append("SET DEFAULT");
          break;
        case AlterDomainSection.DropDefault:
          output.Append("DROP DEFAULT");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlAlterPartitionFunction node)
    {
      context.Output.Append("ALTER PARTITION FUNCTION ");
      TranslateIdentifier(context.Output, node.PartitionFunction.DbName);
      context.Output.Append("()")
        .Append(node.Option == SqlAlterPartitionFunctionOption.Split ? " SPLIT RANGE (" : " MERGE RANGE (")
        .Append(node.Boundary)
        .Append(")");
    }

    public virtual void Translate(SqlCompilerContext context, SqlAlterPartitionScheme node)
    {
      var output = context.Output;
      output.Append("ALTER PARTITION SCHEME ");
      TranslateIdentifier(output, node.PartitionSchema.DbName);
      output.Append(" NEXT USED");
      if (!string.IsNullOrEmpty(node.Filegroup)) {
        output.Append(" ")
          .Append(node.Filegroup);
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      var output = context.Output;
      switch (section) {
        case AlterTableSection.Entry:
          output.Append("ALTER TABLE ");
          Translate(context, node.Table);
          break;
        case AlterTableSection.AddColumn:
          output.Append("ADD COLUMN");
          break;
        case AlterTableSection.AlterColumn:
          output.Append("ALTER COLUMN");
          break;
        case AlterTableSection.DropColumn:
          output.Append("DROP COLUMN");
          break;
        case AlterTableSection.AddConstraint:
          output.Append("ADD");
          break;
        case AlterTableSection.DropConstraint:
          output.Append("DROP");
          break;
        case AlterTableSection.RenameColumn:
          output.Append("RENAME COLUMN");
          break;
        case AlterTableSection.To:
          output.Append("TO");
          break;
        case AlterTableSection.DropBehavior when node.Action is SqlCascadableAction cascadableAction:
          output.Append(cascadableAction.Cascade ? "CASCADE" : "RESTRICT");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlAlterSequence node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          context.Output.Append("ALTER SEQUENCE ");
          Translate(context, node.Sequence);
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      var output = context.Output;
      switch (section) {
        case TableColumnSection.Entry:
          TranslateIdentifier(output, column.DbName);
          break;
        case TableColumnSection.Type:
          if (column.Domain == null) {
            output.Append(Translate(column.DataType));
          }
          else {
            Translate(context, column.Domain);
          }
          break;
        case TableColumnSection.DefaultValue:
          output.Append("DEFAULT");
          break;
        case TableColumnSection.DropDefault:
          output.Append("DROP DEFAULT");
          break;
        case TableColumnSection.SetDefault:
          output.Append("SET DEFAULT");
          break;
        case TableColumnSection.GenerationExpressionExit:
          output.Append(")").Append(column.IsPersisted ? " PERSISTED" : "");
          break;
        case TableColumnSection.SetIdentityInfoElement:
          output.Append("SET");
          break;
        case TableColumnSection.NotNull:
          output.Append("NOT NULL");
          break;
        case TableColumnSection.Collate:
          output.Append("COLLATE ");
          Translate(output, column.Collation);
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      var output = context.Output;
      switch (section) {
        case ConstraintSection.Entry when !String.IsNullOrEmpty(constraint.DbName):
          output.Append("CONSTRAINT ");
          TranslateIdentifier(output, constraint.DbName);
          break;
        case ConstraintSection.Check:
          output.AppendPunctuation("CHECK (");
          break;
        case ConstraintSection.PrimaryKey:
          output.AppendPunctuation("PRIMARY KEY (");
          break;
        case ConstraintSection.Unique:
          output.AppendPunctuation("UNIQUE (");
          break;
        case ConstraintSection.ForeignKey:
          output.AppendPunctuation("FOREIGN KEY (");
          break;
        case ConstraintSection.ReferencedColumns: {
          var fk = (ForeignKey) constraint;
          output.Append(") REFERENCES ");
          Translate(context, fk.ReferencedColumns[0].DataTable);
          output.AppendPunctuation(" (");
        }
        break;
        case ConstraintSection.Exit: {
          output.Append(")");
          if (constraint is ForeignKey fk) {
            if (fk.MatchType != SqlMatchType.None)
              output.Append(" MATCH ").Append(Translate(fk.MatchType));
            if (fk.OnUpdate != ReferentialAction.NoAction)
              output.Append(" ON UPDATE ").Append(Translate(fk.OnUpdate));
            if (fk.OnDelete != ReferentialAction.NoAction)
              output.Append(" ON DELETE ").Append(Translate(fk.OnDelete));
          }
          if (constraint.IsDeferrable.HasValue) {
            if (constraint.IsDeferrable.Value)
              output.Append(" DEFERRABLE");
            else
              output.Append(" NOT DEFERRABLE");
          }
          if (constraint.IsInitiallyDeferred.HasValue) {
            if (constraint.IsInitiallyDeferred.Value)
              output.Append(" INITIALLY DEFERRED");
            else
              output.Append(" INITIALLY IMMEDIATE");
          }
        }
        break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlArray node, ArraySection section)
    {
      context.Output.Append(section switch {
        ArraySection.Entry => "(",
        ArraySection.Exit => ")",
        ArraySection.EmptyArray => "(NULL)",
        _ => throw new ArgumentOutOfRangeException("section")
      });
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

    public virtual void Translate(SqlCompilerContext context, SqlAssignment node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          context.Output.Append("SET");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlBetween node, BetweenSection section)
    {
      switch (section) {
        case BetweenSection.Between:
          context.Output.Append(Translate(node.NodeType));
          break;
        case BetweenSection.And:
          context.Output.Append("AND");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlBinary node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry when node.NodeType != SqlNodeType.RawConcat:
          context.Output.Append(OpeningParenthesis);
          break;
        case NodeSection.Exit when node.NodeType != SqlNodeType.RawConcat:
          context.Output.Append(ClosingParenthesis);
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlBreak node)
    {
      context.Output.Append("BREAK");
    }

    public virtual void Translate(SqlCompilerContext context, SqlCase node, CaseSection section)
    {
      switch (section) {
        case CaseSection.Entry:
          context.Output.Append("(CASE");
          break;
        case CaseSection.Else:
          context.Output.Append("ELSE");
          break;
        case CaseSection.Exit:
          context.Output.Append("END)");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlCase node, SqlExpression item, CaseSection section)
    {
      switch (section) {
        case CaseSection.When:
          context.Output.Append("WHEN");
          break;
        case CaseSection.Then:
          context.Output.Append("THEN");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          context.Output.Append("CAST(");
          break;
        case NodeSection.Exit:
          context.Output.Append(" AS ").Append(Translate(node.Type)).Append(")");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlCloseCursor node)
    {
      context.Output.Append("CLOSE ").Append(node.Cursor.Name);
    }

    public virtual void Translate(SqlCompilerContext context, SqlCollate node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          context.Output.Append("COLLATE ").Append(node.Collation.DbName);
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlColumnRef node, ColumnSection section)
    {
      var output = context.Output;
      switch (section) {
        case ColumnSection.Entry:
          TranslateIdentifier(output, node.Name);
          break;
        case ColumnSection.AliasDeclaration when !string.IsNullOrEmpty(node.Name):
          output.Append("AS ");
          TranslateIdentifier(output, node.Name);
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlConcat node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          context.Output.AppendPunctuation("(");
          break;
        case NodeSection.Exit:
          context.Output.AppendClosingPunctuation(")");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlContinue node)
    {
      context.Output.Append("CONTINUE");
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreateAssertion node, NodeSection section)
    {
      var output = context.Output;
      switch (section) {
        case NodeSection.Entry:
          output.Append("CREATE ASSERTION ");
          Translate(context, node.Assertion);
          output.Append(" CHECK");
          break;
        case NodeSection.Exit:
          if (node.Assertion.IsDeferrable.HasValue) {
            output.Append(node.Assertion.IsDeferrable.Value ? " DEFERRABLE" : " NOT DEFERRABLE");
          }
          if (node.Assertion.IsInitiallyDeferred.HasValue) {
            output.Append(node.Assertion.IsInitiallyDeferred.Value ? " INITIALLY DEFERRED" : " INITIALLY IMMEDIATE");
          }
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreateCharacterSet node)
    {
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
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreateCollation node)
    {
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
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreateDomain node, CreateDomainSection section)
    {
      var output = context.Output;
      switch (section) {
        case CreateDomainSection.Entry:
          output.Append("CREATE DOMAIN ");
          Translate(context, node.Domain);
          output.Append(" AS ").Append(Translate(node.Domain.DataType));
          break;
        case CreateDomainSection.DomainDefaultValue:
          output.Append("DEFAULT");
          break;
        case CreateDomainSection.DomainCollate:
          output.Append("COLLATE ");
          Translate(output, node.Domain.Collation);
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      var output = context.Output;
      switch (section) {
        case CreateIndexSection.Entry:
          Index index = node.Index;
          if (index.IsFullText) {
            output.Append("CREATE FULLTEXT INDEX ON ");
            Translate(context, index.DataTable);
            return;
          }
          output.Append("CREATE ");
          if (index.IsUnique)
            output.Append("UNIQUE ");
          else if (index.IsBitmap)
            output.Append("BITMAP ");
          else if (index.IsSpatial)
            output.Append("SPATIAL ");
          if (Driver.ServerInfo.Index.Features.Supports(IndexFeatures.Clustered))
            if (index.IsClustered)
              output.Append("CLUSTERED ");
          output.Append("INDEX ");
          TranslateIdentifier(output, index.DbName);
          output.Append(" ON ");
          Translate(context, index.DataTable);
          break;
        case CreateIndexSection.ColumnsEnter:
          output.AppendPunctuation("(");
          break;
        case CreateIndexSection.ColumnsExit:
          output.AppendClosingPunctuation(")");
          break;
        case CreateIndexSection.NonkeyColumnsEnter:
          output.AppendPunctuation(" INCLUDE (");
          break;
        case CreateIndexSection.NonkeyColumnsExit:
          output.AppendClosingPunctuation(")");
          break;
        case CreateIndexSection.Where:
          output.Append(" WHERE");
          break;
        case CreateIndexSection.Exit:
          index = node.Index;
          if (index.FillFactor.HasValue) {
            output.Append(" WITH (FILLFACTOR = " + index.FillFactor.Value + ")");
          }
          if (index.PartitionDescriptor != null) {
            output.Append(" ");
            Translate(output, index.PartitionDescriptor, true);
          }
          else if (!String.IsNullOrEmpty(index.Filegroup))
            output.Append(" ON " + index.Filegroup);
          else if (index is FullTextIndex) {
            var ftindex = index as FullTextIndex;
            output.Append(" KEY INDEX ");
            TranslateIdentifier(output, ftindex.UnderlyingUniqueIndex);
          }
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreatePartitionFunction node)
    {
      var output = context.Output;
      PartitionFunction pf = node.PartitionFunction;
      output.Append("CREATE PARTITION FUNCTION ");
      TranslateIdentifier(output, pf.DbName);
      output.Append(" (")
        .Append(Translate(pf.DataType))
        .Append(")")
        .Append(" AS RANGE ")
        .Append(pf.BoundaryType == BoundaryType.Left ? "LEFT" : "RIGHT")
        .Append(" FOR VALUES (");
      bool first = true;
      foreach (string value in pf.BoundaryValues) {
        if (first)
          first = false;
        else
          output.Append(RowItemDelimiter);
        TypeCode t = Type.GetTypeCode(value.GetType());
        if (t == TypeCode.String || t == TypeCode.Char)
          output.Append(QuoteString(value));
        else
          output.Append(value);
      }
      output.Append(")");
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreatePartitionScheme node)
    {
      var output = context.Output;
      PartitionSchema ps = node.PartitionSchema;
      output.Append("CREATE PARTITION SCHEME ");
      TranslateIdentifier(output, ps.DbName);
      output.Append(" AS PARTITION ");
      TranslateIdentifier(output, ps.PartitionFunction.DbName);
      if (ps.Filegroups.Count <= 1)
        output.Append(" ALL");
      output.Append(" TO (");
      bool first = true;
      foreach (string filegroup in ps.Filegroups) {
        if (first)
          first = false;
        else
          output.Append(RowItemDelimiter);
        output.Append(filegroup);
      }
      output.Append(")");
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreateSchema node, NodeSection section)
    {
      var output = context.Output;
      switch (section) {
        case NodeSection.Entry:
          output.Append("CREATE SCHEMA ");
          if (!String.IsNullOrEmpty(node.Schema.DbName)) {
            TranslateIdentifier(output, node.Schema.DbName);
            output.Append(node.Schema.Owner == null ? "" : " ");
          }
          if (node.Schema.Owner != null) {
            output.Append("AUTHORIZATION ");
            TranslateIdentifier(output, node.Schema.Owner);
          }
          if (node.Schema.DefaultCharacterSet != null) {
            output.Append("DEFAULT CHARACTER SET ");
            Translate(context, node.Schema.DefaultCharacterSet);
          }
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreateSequence node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          context.Output.Append("CREATE SEQUENCE ");
          Translate(context, node.Sequence);
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      var output = context.Output;
      switch (section) {
        case CreateTableSection.Entry: {
          output.Append("CREATE ");
          var temporaryTable = node.Table as TemporaryTable;
          if (temporaryTable != null) {
            if (temporaryTable.IsGlobal)
              output.Append("GLOBAL ");
            else
              output.Append("LOCAL ");
            output.Append("TEMPORARY ");
          }
          output.Append("TABLE ");
          Translate(context, node.Table);
          break;
        }
        case CreateTableSection.TableElementsEntry:
          output.AppendPunctuation("(");
          break;
        case CreateTableSection.TableElementsExit:
          output.Append(")");
          break;
        case CreateTableSection.Partition:
          Translate(output, node.Table.PartitionDescriptor, true);
          break;
        case CreateTableSection.Exit: {
          if (!string.IsNullOrEmpty(node.Table.Filegroup)) {
            output.Append(" ON ");
            TranslateIdentifier(output, node.Table.Filegroup);
          }
          if (node.Table is TemporaryTable temporaryTable) {
            output.Append(temporaryTable.PreserveRows ? "ON COMMIT PRESERVE ROWS" : "ON COMMIT DELETE ROWS");
          }
          break;
        }
      }
    }

    public virtual void Translate(SqlCompilerContext context, SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      TranslateSequenceDescriptorDefault(context, descriptor, section);
    }

    protected void TranslateSequenceDescriptorDefault(SqlCompilerContext context, SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      var output = context.Output;
      switch (section) {
        case SequenceDescriptorSection.StartValue when descriptor.StartValue.HasValue:
          output.Append("START WITH ").Append(descriptor.StartValue.Value);
          break;
        case SequenceDescriptorSection.RestartValue when descriptor.StartValue.HasValue:
          output.Append("RESTART WITH ").Append(descriptor.StartValue.Value);
          break;
        case SequenceDescriptorSection.Increment when descriptor.Increment.HasValue:
          output.Append("INCREMENT BY ").Append(descriptor.Increment.Value);
          break;
        case SequenceDescriptorSection.MaxValue when descriptor.MaxValue.HasValue:
          output.Append("MAXVALUE ").Append(descriptor.MaxValue.Value);
          break;
        case SequenceDescriptorSection.MinValue when descriptor.MinValue.HasValue:
          output.Append("MINVALUE ").Append(descriptor.MinValue.Value);
          break;
        case SequenceDescriptorSection.AlterMaxValue:
          if (descriptor.MaxValue.HasValue) {
            output.Append("MAXVALUE ").Append(descriptor.MaxValue.Value);
          }
          else {
            output.Append("NO MAXVALUE");
          }
          break;
        case SequenceDescriptorSection.AlterMinValue:
          if (descriptor.MinValue.HasValue) {
            output.Append("MINVALUE ").Append(descriptor.MinValue.Value);
          }
          else {
            output.Append("NO MINVALUE");
          }
          break;
        case SequenceDescriptorSection.IsCyclic when descriptor.IsCyclic.HasValue:
          output.Append(descriptor.IsCyclic.Value ? "CYCLE" : "NO CYCLE");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreateTranslation node)
    {
      //      var output = context.Output;
      //      output.Append("CREATE TRANSLATION "+Translate(node.Translation));
      //      output.Append(" FOR "+Translate(node.Translation.SourceCharacterSet));
      //      output.Append(" TO "+Translate(node.Translation.SourceCharacterSet)+" FROM ");
      //      if (node.Translation.TranslationSource is IIdentityTranslation)
      //        output.Append("IDENTITY");
      //      else if (node.Translation.TranslationSource is IExternalTranslation)
      //        output.Append("EXTERNAL ("+QuoteString(((IExternalTranslation)node.Translation.TranslationSource).Name)+")");
      //      else if (node.Translation.TranslationSource is ITranslation)
      //        output.Append(Translate((ITranslation)node.Translation.TranslationSource));
    }

    public virtual void Translate(SqlCompilerContext context, SqlCreateView node, NodeSection section)
    {
      var output = context.Output;
      switch (section) {
        case NodeSection.Entry:
          output.Append("CREATE VIEW ");
          Translate(context, node.View);
          if (node.View.ViewColumns.Count > 0) {
            output.Append(" (");
            bool first = true;
            foreach (DataTableColumn c in node.View.ViewColumns) {
              if (first)
                first = false;
              else
                output.Append(ColumnDelimiter);
              output.Append(c.DbName);
            }
            output.Append(")");
          }
          output.Append(" AS");
          break;
        case NodeSection.Exit:
          switch (node.View.CheckOptions) {
            case CheckOptions.Cascaded:
              output.Append("WITH CASCADED CHECK OPTION");
              break;
            case CheckOptions.Local:
              output.Append("WITH LOCAL CHECK OPTION");
              break;
          }
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlCursor node)
    {
      context.Output.Append(node.Name);
    }

    public virtual void Translate(SqlCompilerContext context, SqlDeclareCursor node, DeclareCursorSection section)
    {
      var output = context.Output;
      switch (section) {
        case DeclareCursorSection.Entry:
          output.Append("DECLARE ").Append(node.Cursor.Name);
          break;
        case DeclareCursorSection.Sensivity:
          output.Append(node.Cursor.Insensitive ? "INSENSITIVE " : string.Empty);
          break;
        case DeclareCursorSection.Scrollability:
          output.Append(node.Cursor.Scroll ? "SCROLL " : string.Empty);
          break;
        case DeclareCursorSection.Cursor:
          output.Append("CURSOR ");
          break;
        case DeclareCursorSection.For:
          output.Append("FOR");
          break;
        case DeclareCursorSection.Holdability:
          output.Append(node.Cursor.WithHold ? "WITH HOLD " : "WITHOUT HOLD ");
          break;
        case DeclareCursorSection.Returnability:
          output.Append(node.Cursor.WithReturn ? "WITH RETURN " : "WITHOUT RETURN ");
          break;
        case DeclareCursorSection.Updatability:
          output.Append(node.Cursor.ReadOnly ? "FOR READ ONLY" : "FOR UPDATE");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlDeclareVariable node)
    {
      context.Output.Append("DECLARE @")
        .Append(node.Variable.Name)
        .Append(" AS ")
        .Append(Translate(node.Variable.Type));
    }

    public virtual void Translate(SqlCompilerContext context, SqlDefaultValue node)
    {
      context.Output.Append("DEFAULT");
    }

    public virtual void Translate(SqlCompilerContext context, SqlDelete node, DeleteSection section)
    {
      context.Output.Append(section switch {
        DeleteSection.Entry => "DELETE FROM",
        DeleteSection.From => "FROM",
        DeleteSection.Where => "WHERE",
        DeleteSection.Limit => "LIMIT",
        _ => string.Empty
      });
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropAssertion node)
    {
      context.Output.Append("DROP ASSERTION ");
      Translate(context, node.Assertion);
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropCharacterSet node)
    {
      context.Output.Append("DROP CHARACTER SET ");
      Translate(context, node.CharacterSet);
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropCollation node)
    {
      context.Output.Append("DROP COLLATION ");
      Translate(context, node.Collation);
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropDomain node)
    {
      context.Output.Append("DROP DOMAIN ");
      Translate(context, node.Domain);
      context.Output.Append(node.Cascade ? " CASCADE" : " RESTRICT");
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      var output = context.Output;
      if (!node.Index.IsFullText) {
        output.Append("DROP INDEX ");
        TranslateIdentifier(output, node.Index.DbName);
        output.Append(" ON ");
      }
      else
        output.Append("DROP FULLTEXT INDEX ON ");
      Translate(context, node.Index.DataTable);
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropPartitionFunction node)
    {
      context.Output.Append("DROP PARTITION FUNCTION ");
      TranslateIdentifier(context.Output, node.PartitionFunction.DbName);
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropPartitionScheme node)
    {
      context.Output.Append("DROP PARTITION SCHEME ");
      TranslateIdentifier(context.Output, node.PartitionSchema.DbName);
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropSchema node)
    {
      var output = context.Output;
      output.Append("DROP SCHEMA ");
      TranslateIdentifier(output, node.Schema.DbName);
      output.Append(node.Cascade ? " CASCADE" : " RESTRICT");
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropSequence node)
    {
      context.Output.Append("DROP SEQUENCE ");
      Translate(context, node.Sequence);
      context.Output.Append(node.Cascade ? " CASCADE" : " RESTRICT");
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropTable node)
    {
      context.Output.Append("DROP TABLE ");
      Translate(context, node.Table);
      context.Output.Append(node.Cascade ? " CASCADE" : " RESTRICT");
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropTranslation node)
    {
      context.Output.Append("DROP TRANSLATION ");
      Translate(context, node.Translation);
    }

    public virtual void Translate(SqlCompilerContext context, SqlDropView node)
    {
      context.Output.Append("DROP VIEW ");
      Translate(context, node.View);
      context.Output.Append(node.Cascade ? " CASCADE" : " RESTRICT");
    }

    public virtual void Translate(SqlCompilerContext context, SqlFetch node, FetchSection section)
    {
      switch (section) {
        case FetchSection.Entry:
          context.Output.Append("FETCH ")
            .Append(node.Option.ToString());
          break;
        case FetchSection.Targets:
          context.Output.Append("FROM ")
            .Append(node.Cursor.Name)
            .Append((node.Targets.Count != 0) ? " INTO" : string.Empty);
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
    {
      var output = context.Output;
      switch (section) {
        case FunctionCallSection.Entry:
          switch (node.FunctionType) {
            case SqlFunctionType.CurrentUser:
            case SqlFunctionType.SessionUser:
            case SqlFunctionType.SystemUser:
            case SqlFunctionType.User:
              output.Append(Translate(node.FunctionType));
              break;
            case SqlFunctionType.Position when Driver.ServerInfo.StringIndexingBase > 0:
              output.Append("(").Append(Translate(node.FunctionType)).Append("(");
              break;
            default:
              if (node.Arguments.Count == 0) {
                output.Append(Translate(node.FunctionType)).Append("()");
              }
              else {
                output.Append(Translate(node.FunctionType)).Append("(");
              }
              break;
          }
          break;
        case FunctionCallSection.ArgumentEntry:
          switch (node.FunctionType) {
            case SqlFunctionType.Position when position == 1:
              output.Append("IN");
              break;
            case SqlFunctionType.Substring:
              switch (position) {
                case 1:
                  output.Append("FROM");
                  break;
                case 2:
                  output.Append("FOR");
                  break;
              }
              break;
          }
          break;
        case FunctionCallSection.ArgumentExit when node.FunctionType == SqlFunctionType.Substring
            && position == 1
            && Driver.ServerInfo.StringIndexingBase > 0:
          output.Append("+ ").Append(Driver.ServerInfo.StringIndexingBase);
          break;
        case FunctionCallSection.ArgumentDelimiter:
          switch (node.FunctionType) {
            case SqlFunctionType.Position:
            case SqlFunctionType.Substring:
              break;
            default:
              output.Append(ArgumentDelimiter);
              break;
          }
          break;
        case FunctionCallSection.Exit:
          if (node.FunctionType == SqlFunctionType.Position && Driver.ServerInfo.StringIndexingBase > 0) {
            output.Append(") - ").Append(Driver.ServerInfo.StringIndexingBase).Append(")");
          }
          else if (node.Arguments.Count != 0) {
            output.Append(")");
          }
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlExtract extract, ExtractSection section)
    {
      context.Output.Append(section switch {
        ExtractSection.Entry => "EXTRACT(",
        ExtractSection.From => "FROM",
        ExtractSection.Exit => ")",
        _ => string.Empty
      });
    }

    public virtual void Translate(SqlCompilerContext context, SqlIf node, IfSection section)
    {
      context.Output.Append(section switch {
        IfSection.Entry => "IF",
        IfSection.True => "BEGIN",
        IfSection.False => "END BEGIN",
        IfSection.Exit => "END",
        _ => string.Empty
      });
    }

    public virtual void Translate(SqlCompilerContext context, SqlInsert node, InsertSection section)
    {
      var output = context.Output;
      switch (section) {
        case InsertSection.Entry:
          output.Append("INSERT INTO");
          break;
        case InsertSection.ColumnsEntry when node.Values.Keys.Count > 0:
          output.AppendPunctuation("(");
          break;
        case InsertSection.ColumnsExit when node.Values.Keys.Count > 0:
          output.AppendClosingPunctuation(")");
          break;
        case InsertSection.From:
          output.Append("FROM");
          break;
        case InsertSection.ValuesEntry:
          output.AppendPunctuation("VALUES (");
          break;
        case InsertSection.ValuesExit:
          output.AppendClosingPunctuation(")");
          break;
        case InsertSection.DefaultValues:
          output.Append("DEFAULT VALUES");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlJoinExpression node, JoinSection section)
    {
      var output = context.Output;
      var traversalPath = context.GetTraversalPath().Skip(1);
      var explicitJoinOrder =
        Driver.ServerInfo.Query.Features.Supports(QueryFeatures.ExplicitJoinOrder)
        && traversalPath.FirstOrDefault() is SqlJoinExpression;
      switch (section) {
        case JoinSection.Entry when explicitJoinOrder:
          output.Append("(");
          break;
        case JoinSection.Specification:
          bool isNatural = node.Expression.IsNullReference()
            && node.JoinType != SqlJoinType.CrossJoin
            && node.JoinType != SqlJoinType.UnionJoin;
          if (isNatural) {
            output.Append("NATURAL ");
          }
          output.Append(Translate(node.JoinType))
            .Append(" JOIN");
          break;
        case JoinSection.Condition:
          output.Append(node.JoinType == SqlJoinType.UsingJoin ? "USING" : "ON");
          break;
        case JoinSection.Exit when explicitJoinOrder:
          output.Append(")");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlLike node, LikeSection section)
    {
      var output = context.Output;
      switch (section) {
        case LikeSection.Entry:
          output.AppendPunctuation("(");
          break;
        case LikeSection.Exit:
          output.AppendClosingPunctuation(")");
          break;
        case LikeSection.Like:
          output.Append(node.Not ? "NOT LIKE" : "LIKE");
          break;
        case LikeSection.Escape:
          output.Append("ESCAPE");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, object literalValue)
    {
      var output = context.Output;
      var literalType = literalValue.GetType();
      switch (Type.GetTypeCode(literalType)) {
        case TypeCode.Char:
        case TypeCode.String:
          TranslateString(output, literalValue.ToString());
          return;
        case TypeCode.DateTime:
          output.Append(((DateTime) literalValue).ToString(DateTimeFormatString, DateTimeFormat));
          return;
        case TypeCode.Single:
          output.Append(((float) literalValue).ToString(FloatFormatString, FloatNumberFormat));
          return;
        case TypeCode.Double:
          output.Append(((double) literalValue).ToString(DoubleFormatString, DoubleNumberFormat));
          return;
        case TypeCode.Byte:
        case TypeCode.SByte:
        case TypeCode.Int16:
        case TypeCode.UInt16:
        case TypeCode.Int32:
        case TypeCode.UInt32:
        case TypeCode.Int64:
        case TypeCode.UInt64:
        case TypeCode.Decimal:
          output.Append(Convert.ToString(literalValue, IntegerNumberFormat));
          return;
      }
      switch (literalValue) {
        case TimeSpan timeSpan:
          output.Append(SqlHelper.TimeSpanToString(timeSpan, TimeSpanFormatString));
          break;
        case Guid:
        case byte[]:
          throw new NotSupportedException(string.Format(Strings.ExTranslationOfLiteralOfTypeXIsNotSupported, literalType.GetShortName()));
        default:
          output.Append(literalValue.ToString());
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlMatch node, MatchSection section)
    {
      switch (section) {
        case MatchSection.Specification:
          context.Output.Append(" MATCH ");
          if (node.Unique) {
            context.Output.Append("UNIQUE ");
          }
          context.Output.Append(Translate(node.MatchType));
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlNative node)
    {
      context.Output.Append(node.Value);
    }

    public virtual void Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          context.Output.Append("NEXT VALUE FOR ");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlNull node)
    {
      context.Output.Append("NULL");
    }

    public virtual void Translate(SqlCompilerContext context, SqlOpenCursor node)
    {
      context.Output.Append("OPEN ").Append(node.Cursor.Name);
    }

    public virtual void Translate(SqlCompilerContext context, SqlOrder node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          context.Output.Append(node.Ascending ? "ASC" : "DESC");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlQueryExpression node, QueryExpressionSection section)
    {
      switch (section) {
        case QueryExpressionSection.All when node.All:
          context.Output.Append(" ALL");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlQueryRef node, TableSection section)
    {
      switch (section) {
        case TableSection.Entry when !(node.Query is SqlFreeTextTable || node.Query is SqlContainsTable):
          context.Output.AppendPunctuation("(");
          break;
        case TableSection.Exit when !(node.Query is SqlFreeTextTable || node.Query is SqlContainsTable):
          context.Output.AppendClosingPunctuation(")");
          break;
        case TableSection.AliasDeclaration:
          string alias = context.TableNameProvider.GetName(node);
          if (!string.IsNullOrEmpty(alias)) {
            TranslateIdentifier(context.Output, alias);
          }
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlRow node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          context.Output.AppendPunctuation("(");
          break;
        case NodeSection.Exit:
          context.Output.AppendClosingPunctuation(")");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlRowNumber node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          context.Output.Append("ROW_NUMBER() OVER(ORDER BY");
          break;
        case NodeSection.Exit:
          context.Output.AppendClosingPunctuation(")");
          break;
        default:
          throw new ArgumentOutOfRangeException("section");
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlRenameTable node)
    {
      context.Output.Append("ALTER TABLE ");
      Translate(context, node.Table);
      context.Output.Append(" RENAME TO ");
      TranslateIdentifier(context.Output, node.NewName);
    }

    public virtual void Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      context.Output.Append(section switch {
        SelectSection.Entry => node.Distinct ? "SELECT DISTINCT" : "SELECT",
        SelectSection.From => "FROM",
        SelectSection.Where => "WHERE",
        SelectSection.GroupBy => "GROUP BY",
        SelectSection.Having => "HAVING",
        SelectSection.OrderBy => "ORDER BY",
        SelectSection.Limit => "LIMIT",
        SelectSection.Offset => "OFFSET",
        _ => string.Empty
      });
    }

    public virtual void Translate(SqlCompilerContext context, SqlStatementBlock node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          context.Output.Append("BEGIN");
          break;
        case NodeSection.Exit:
          context.Output.Append("End");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlSubQuery node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          context.Output.AppendPunctuation("(");
          break;
        case NodeSection.Exit:
          context.Output.AppendClosingPunctuation(")");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlTable node, NodeSection section)
    {
      TranslateIdentifier(context.Output, context.TableNameProvider.GetName(node));
    }

    public virtual void Translate(SqlCompilerContext context, SqlTableColumn node, NodeSection section)
    {
      var output = context.Output;
      if ((context.NamingOptions & SqlCompilerNamingOptions.TableQualifiedColumns) != 0) {
        Translate(context, node.SqlTable, NodeSection.Entry);
        output.Append(".");
      }
      if ((object) node == (object) node.SqlTable.Asterisk) {
        output.Append(node.Name);
      }
      else {
        TranslateIdentifier(output, node.Name);
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlTableRef node, TableSection section)
    {
      switch (section) {
        case TableSection.Entry:
          Translate(context, node.DataTable);
          break;
        case TableSection.AliasDeclaration:
          string alias = context.TableNameProvider.GetName(node);
          if (alias != node.DataTable.DbName) {
            context.Output.Append(" ");
            TranslateIdentifier(context.Output, alias);
          }
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlTrim node, TrimSection section)
    {
      switch (section) {
        case TrimSection.Entry:
          context.Output.Append("TRIM(");
          break;
        case TrimSection.From:
          context.Output.Append("FROM");
          break;
        case TrimSection.Exit:
          context.Output.AppendClosingPunctuation(")");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlUnary node, NodeSection section)
    {
      var output = context.Output;
      var omitParenthesis =
        node.NodeType == SqlNodeType.Exists
        || node.NodeType == SqlNodeType.All
        || node.NodeType == SqlNodeType.Some
        || node.NodeType == SqlNodeType.Any;

      var isNullCheck = node.NodeType == SqlNodeType.IsNull || node.NodeType == SqlNodeType.IsNotNull;

      switch (section) {
        case NodeSection.Entry:
          if (!omitParenthesis)
            output.AppendPunctuation("(");
          if (!isNullCheck)
            output.Append(Translate(node.NodeType));
          break;
        case NodeSection.Exit:
          if (isNullCheck)
            output.Append(Translate(node.NodeType));
          if (!omitParenthesis)
            output.AppendClosingPunctuation(")");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlUpdate node, UpdateSection section)
    {
      context.Output.Append(section switch {
        UpdateSection.Entry => "UPDATE",
        UpdateSection.Set => "SET",
        UpdateSection.From => "FROM",
        UpdateSection.Where => (node.Where is SqlCursor) ? "WHERE CURRENT OF" : "WHERE",
        UpdateSection.Limit => "LIMIT",
        _ => string.Empty
      });
    }

    public virtual void Translate(SqlCompilerContext context, SqlUserColumn node, NodeSection section)
    {
    }

    public virtual void Translate(SqlCompilerContext context, SqlUserFunctionCall node, FunctionCallSection section, int position)
    {
      switch (section) {
        case FunctionCallSection.Entry:
          context.Output.Append(node.Name).Append("(");
          break;
        case FunctionCallSection.Exit:
          context.Output.Append(")");
          break;
        default:
          Translate(context, node as SqlFunctionCall, section, position);
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlVariable node)
    {
      context.Output.Append("@").Append(node.Name);
    }

    public virtual void Translate(SqlCompilerContext context, SqlWhile node, WhileSection section)
    {
      switch (section) {
        case WhileSection.Entry:
          context.Output.AppendPunctuation("WHILE (");
          break;
        case WhileSection.Statement:
          context.Output.Append(") BEGIN");
          break;
        case WhileSection.Exit:
          context.Output.Append("END");
          break;
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlCommand node)
    {
      context.Output.Append(node.CommandType switch {
        SqlCommandType.SetConstraintsAllDeferred => "SET CONSTRAINTS ALL DEFERRED",
        SqlCommandType.SetConstraintsAllImmediate => "SET CONSTRAINTS ALL IMMEDIATE",
        _ => throw new NotSupportedException(string.Format(Strings.ExOperationXIsNotSupported, node.CommandType))
      });
    }

    public virtual string Translate(SqlNodeType type) =>
      type switch {
        SqlNodeType.All => "ALL",
        SqlNodeType.Any => "ANY",
        SqlNodeType.Some => "SOME",
        SqlNodeType.Exists => "EXISTS",
        SqlNodeType.BitAnd => "&",
        SqlNodeType.BitNot => "~",
        SqlNodeType.BitOr => "|",
        SqlNodeType.BitXor => "^",
        SqlNodeType.In => "IN",
        SqlNodeType.Between => "BETWEEN",
        SqlNodeType.And => "AND",
        SqlNodeType.Or => "OR",
        SqlNodeType.IsNull => "IS NULL",
        SqlNodeType.IsNotNull => "IS NOT NULL",
        SqlNodeType.Not => "NOT",
        SqlNodeType.NotBetween => "NOT BETWEEN",
        SqlNodeType.NotIn => "NOT IN",
        SqlNodeType.GreaterThan => ">",
        SqlNodeType.GreaterThanOrEquals => ">=",
        SqlNodeType.LessThan => "<",
        SqlNodeType.LessThanOrEquals => "<=",
        SqlNodeType.Equals or SqlNodeType.Assign => "=",
        SqlNodeType.NotEquals => "<>",
        SqlNodeType.Add => "+",
        SqlNodeType.Subtract or SqlNodeType.Negate => "-",
        SqlNodeType.Multiply => "*",
        SqlNodeType.Modulo => "%",
        SqlNodeType.Divide => "/",
        SqlNodeType.Avg => "AVG",
        SqlNodeType.Count => "COUNT",
        SqlNodeType.Max => "MAX",
        SqlNodeType.Min => "MIN",
        SqlNodeType.Sum => "SUM",
        SqlNodeType.Concat => "||",
        SqlNodeType.Unique => "UNIQUE",
        SqlNodeType.Union => "UNION",
        SqlNodeType.Intersect => "INTERSECT",
        SqlNodeType.Except => "EXCEPT",
        SqlNodeType.Overlaps => "OVERLAPS",
        SqlNodeType.RawConcat => string.Empty,
        _ => throw new NotSupportedException(string.Format(Strings.ExOperationXIsNotSupported, type))
      };

    public virtual string Translate(SqlJoinType type) =>
      type switch {
        SqlJoinType.CrossJoin => "CROSS",
        SqlJoinType.FullOuterJoin => "FULL OUTER",
        SqlJoinType.InnerJoin => "INNER",
        SqlJoinType.LeftOuterJoin => "LEFT OUTER",
        SqlJoinType.UnionJoin => "UNION",
        SqlJoinType.RightOuterJoin => "RIGHT OUTER",
        SqlJoinType.CrossApply or SqlJoinType.LeftOuterApply => throw SqlHelper.NotSupported(QueryFeatures.CrossApply),
        _ => string.Empty
      };

    public virtual string Translate(SqlMatchType type) =>
      type switch {
        SqlMatchType.Full => "FULL",
        SqlMatchType.Partial => "PARTIAL",
        _ => string.Empty
      };

    private void Translate(IOutput output, PartitionDescriptor partitionDescriptor, bool withOn)
    {
      if (partitionDescriptor.PartitionSchema != null) {
        output.Append((withOn ? "ON " : ""));
        TranslateIdentifier(output, partitionDescriptor.PartitionSchema.DbName);
        output.Append(" (");
        TranslateIdentifier(output, partitionDescriptor.Column.DbName);
        output.Append(")");
      }
      else {
        output.Append("PARTITION BY ");
        switch (partitionDescriptor.PartitionMethod) {
          case PartitionMethod.Hash:
            output.Append("HASH");
            break;
          case PartitionMethod.List:
            output.Append("LIST");
            break;
          case PartitionMethod.Range:
            output.Append("RANGE");
            break;
        }
        output.Append(" (");
        TranslateIdentifier(output, partitionDescriptor.Column.DbName);
        output.Append(")");
        if (partitionDescriptor.Partitions == null)
          output.Append(" PARTITIONS " + partitionDescriptor.PartitionAmount);
        else {
          output.Append(" (");
          bool first = true;
          switch (partitionDescriptor.PartitionMethod) {
            case PartitionMethod.Hash:
              foreach (HashPartition p in partitionDescriptor.Partitions) {
                if (first)
                  first = false;
                else
                  output.Append(ColumnDelimiter);
                output.Append("PARTITION ");
                TranslateIdentifier(output, p.DbName);
                output.Append(string.IsNullOrEmpty(p.Filegroup) ? "" : " TABLESPACE " + p.Filegroup);
              }
              break;
            case PartitionMethod.List:
              foreach (ListPartition p in partitionDescriptor.Partitions) {
                if (first)
                  first = false;
                else
                  output.Append(ColumnDelimiter);
                output.Append("PARTITION ");
                TranslateIdentifier(output, p.DbName);
                output.Append(" VALUES (");
                bool firstValue = true;
                foreach (string v in p.Values) {
                  if (firstValue)
                    firstValue = false;
                  else
                    output.Append(RowItemDelimiter);
                  TypeCode t = Type.GetTypeCode(v.GetType());
                  if (t == TypeCode.String || t == TypeCode.Char)
                    TranslateString(output, v);
                  else
                    output.Append(v);
                }
                output.Append(")");
                if (!String.IsNullOrEmpty(p.Filegroup))
                  output.Append(" TABLESPACE ").Append(p.Filegroup);
              }
              break;
            case PartitionMethod.Range:
              foreach (RangePartition p in partitionDescriptor.Partitions) {
                if (first)
                  first = false;
                else
                  output.Append(ColumnDelimiter);
                output.Append("PARTITION ");
                TranslateIdentifier(output, p.DbName);
                output.Append(" VALUES LESS THAN (");
                TypeCode t = Type.GetTypeCode(p.Boundary.GetType());
                if (t == TypeCode.String || t == TypeCode.Char)
                  output.Append(QuoteString(p.Boundary));
                else
                  output.Append(p.Boundary);
                output.Append(")");
                if (!String.IsNullOrEmpty(p.Filegroup))
                  output.Append(" TABLESPACE " + p.Filegroup);
              }
              break;
          }
          output.Append(")");
        }
      }
    }

    public virtual string TranslateToString(SqlCompilerContext context, SchemaNode node) => throw new NotSupportedException();

    public virtual void Translate(SqlCompilerContext context, SchemaNode node)
    {
      var schemaQualified = node.Schema != null
        && Driver.ServerInfo.Query.Features.Supports(QueryFeatures.MultischemaQueries);

      var output = context.Output;

      if (!schemaQualified) {
        TranslateIdentifier(output, node.DbName);
        return;
      }

      var dbQualified = node.Schema.Catalog != null
        && context.HasOptions(SqlCompilerNamingOptions.DatabaseQualifiedObjects);
      var actualizer = context.SqlNodeActualizer;

      if (dbQualified) {
        TranslateIdentifier(output, actualizer.Actualize(node.Schema.Catalog), actualizer.Actualize(node.Schema), node.GetDbNameInternal());
      }
      else {
        TranslateIdentifier(output, actualizer.Actualize(node.Schema), node.DbName);
      }
    }

    public virtual void Translate(IOutput output, Collation collation)
    {
      TranslateString(output, collation.DbName);
    }

    public virtual string Translate(ReferentialAction action) =>
      action switch {
        ReferentialAction.Cascade => "CASCADE",
        ReferentialAction.SetDefault => "SET DEFAULT",
        ReferentialAction.SetNull => "SET NULL",
        _ => string.Empty
      };

    public virtual string Translate(SqlValueType type)
    {
      if (type.TypeName != null) {
        return type.TypeName;
      }

      var dataTypeInfo = Driver.ServerInfo.DataTypes[type.Type];
      if (dataTypeInfo == null) {
        throw new NotSupportedException(string.Format(Strings.ExTypeXIsNotSupported,
          (type.Type == SqlType.Unknown) ? type.TypeName : type.Type.Name));
      }

      var typeName = dataTypeInfo.NativeTypes.First();

      if (type.Length.HasValue) {
        return $"{typeName}({type.Length})";
      }
      if (type.Precision.HasValue && type.Scale.HasValue) {
        return $"{typeName}({type.Precision},{type.Scale})";
      }
      if (type.Precision.HasValue) {
        return $"{typeName}({type.Precision})";
      }
      return typeName;
    }

    public virtual string Translate(SqlFunctionType type) =>
      type switch {
        SqlFunctionType.CharLength or SqlFunctionType.BinaryLength => "LENGTH",
        SqlFunctionType.Concat => "CONCAT",
        SqlFunctionType.CurrentDate => "CURRENT_DATE",
        SqlFunctionType.CurrentTime => "CURRENT_TIME",
        SqlFunctionType.CurrentTimeStamp => "CURRENT_TIMESTAMP",
        SqlFunctionType.Lower => "LOWER",
        SqlFunctionType.Position => "POSITION",
        SqlFunctionType.Substring => "SUBSTRING",
        SqlFunctionType.Upper => "UPPER",
        SqlFunctionType.Abs => "ABS",
        SqlFunctionType.Acos => "ACOS",
        SqlFunctionType.Asin => "ASIN",
        SqlFunctionType.Atan => "ATAN",
        SqlFunctionType.Atan2 => "ATAN2",
        SqlFunctionType.Ceiling => "CEILING",
        SqlFunctionType.Coalesce => "COALESCE",
        SqlFunctionType.Cos => "COS",
        SqlFunctionType.Cot => "COT",
        SqlFunctionType.CurrentUser => "CURRENT_USER",
        SqlFunctionType.Degrees => "DEGREES",
        SqlFunctionType.Exp => "EXP",
        SqlFunctionType.Floor => "FLOOR",
        SqlFunctionType.Log => "LOG",
        SqlFunctionType.Log10 => "LOG10",
        SqlFunctionType.NullIf => "NULLIF",
        SqlFunctionType.Pi => "PI",
        SqlFunctionType.Power => "POWER",
        SqlFunctionType.Radians => "RADIANS",
        SqlFunctionType.Rand => "RAND",
        SqlFunctionType.Replace => "REPLACE",
        SqlFunctionType.Round => "ROUND",
        SqlFunctionType.Truncate => "TRUNCATE",
        SqlFunctionType.SessionUser => "SESSION_USER",
        SqlFunctionType.Sign => "SIGN",
        SqlFunctionType.Sin => "SIN",
        SqlFunctionType.Sqrt => "SQRT",
        SqlFunctionType.Square => "SQUARE",
        SqlFunctionType.SystemUser => "SYSTEM_USER",
        SqlFunctionType.Tan => "TAN",
        _ => throw new NotSupportedException(string.Format(Strings.ExFunctionXIsNotSupported, type))
      };

    public virtual string Translate(SqlTrimType type) =>
      type switch {
        SqlTrimType.Leading => "LEADING",
        SqlTrimType.Trailing => "TRAILING",
        SqlTrimType.Both => "BOTH",
        _ => string.Empty
      };

    public virtual string Translate(SqlDateTimePart dateTimePart) =>
      dateTimePart switch {
        SqlDateTimePart.Year => "YEAR",
        SqlDateTimePart.Month => "MONTH",
        SqlDateTimePart.Day => "DAY",
        SqlDateTimePart.Hour => "HOUR",
        SqlDateTimePart.Minute => "MINUTE",
        SqlDateTimePart.Second => "SECOND",
        SqlDateTimePart.Millisecond => "MILLISECOND",
        SqlDateTimePart.Nanosecond => "NANOSECOND",
        SqlDateTimePart.TimeZoneHour => "TIMEZONE_HOUR",
        SqlDateTimePart.TimeZoneMinute => "TIMEZONE_MINUTE",
        SqlDateTimePart.DayOfYear => "DAYOFYEAR",
        SqlDateTimePart.DayOfWeek => "DAYOFWEEK",
        _ => throw new ArgumentOutOfRangeException("dateTimePart")
      };

    public virtual string Translate(SqlDateTimeOffsetPart dateTimeOffsetPart) =>
      dateTimeOffsetPart switch {
        SqlDateTimeOffsetPart.Year => "YEAR",
        SqlDateTimeOffsetPart.Month => "MONTH",
        SqlDateTimeOffsetPart.Day => "DAY",
        SqlDateTimeOffsetPart.Hour => "HOUR",
        SqlDateTimeOffsetPart.Minute => "MINUTE",
        SqlDateTimeOffsetPart.Second => "SECOND",
        SqlDateTimeOffsetPart.Millisecond => "MILLISECOND",
        SqlDateTimeOffsetPart.Nanosecond => "NANOSECOND",
        SqlDateTimeOffsetPart.TimeZoneHour => "TZoffset",
        SqlDateTimeOffsetPart.TimeZoneMinute => "TZoffset",
        SqlDateTimeOffsetPart.DayOfYear => "DAYOFYEAR",
        SqlDateTimeOffsetPart.DayOfWeek => "WEEKDAY",
        _ => throw new ArgumentOutOfRangeException("dateTimeOffsetPart")
      };

    public virtual string Translate(SqlIntervalPart intervalPart) =>
      intervalPart switch {
        SqlIntervalPart.Day => "DAY",
        SqlIntervalPart.Hour => "HOUR",
        SqlIntervalPart.Minute => "MINUTE",
        SqlIntervalPart.Second => "SECOND",
        SqlIntervalPart.Millisecond => "MILLISECOND",
        SqlIntervalPart.Nanosecond => "NANOSECOND",
        _ => throw new ArgumentOutOfRangeException("intervalPart")
      };

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

    protected virtual void TranslateStringChar(IOutput output, char ch)
    {
      switch (ch) {
        case '\0':
          break;
        case '\'':
          output.AppendLiteral("''");
          break;
        default:
          output.AppendLiteral(ch);
          break;
      }
    }

    public virtual void TranslateString(IOutput output, string str)
    {
      output.AppendLiteral('\'');
      foreach (var ch in str) {
        TranslateStringChar(output, ch);
      }
      output.AppendLiteral('\'');
    }

    public virtual SqlHelper.EscapeSetup EscapeSetup => SqlHelper.EscapeSetup.WithBrackets;

    /// <summary>
    /// Returns string holding quoted identifier name.
    /// </summary>
    /// <param name="names">An <see cref="Array"/> of unquoted identifier name parts.</param>
    /// <returns>Quoted identifier name.</returns>
    public string QuoteIdentifier(params string[] names) =>
      SqlHelper.Quote(EscapeSetup, names);

    public void TranslateIdentifier(IOutput output, string name)
    {
      if (string.IsNullOrEmpty(name))
        return;

      var setup = EscapeSetup;
      output.AppendLiteral(setup.Opener);
      foreach (var ch in name) {
        if (ch == setup.Closer) {
          output.AppendLiteral(setup.EscapeCloser1)
            .AppendLiteral(setup.EscapeCloser2);
        } else {
          output.AppendLiteral(ch);
        }
      }
      output.AppendLiteral(setup.Closer);
    }

    public void TranslateIdentifier(IOutput output, params string[] names)
    {
      var setup = EscapeSetup;
      var first = true;
      foreach (var name in names) {
        if (!string.IsNullOrEmpty(name)) {
          if (!first) {
            output.AppendLiteral(setup.Delimiter);
          }
          output.AppendLiteral(setup.Opener);
          foreach (var ch in name) {
            if (ch == setup.Closer) {
              output.AppendLiteral(setup.EscapeCloser1)
                .AppendLiteral(setup.EscapeCloser2);
            }
            else {
              output.AppendLiteral(ch);
            }
          }
          output.AppendLiteral(setup.Closer);
          first = false;
        }
      }
    }

    /// <summary>
    /// Builds the batch from specified SQL statements.
    /// </summary>
    /// <param name="statements">The statements.</param>
    /// <returns>String containing the whole batch.</returns>
    public virtual string BuildBatch(string[] statements)
    {
      if (statements.Length == 0)
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
        if (actualStatement.Length == 0)
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
