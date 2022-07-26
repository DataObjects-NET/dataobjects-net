// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Index = Xtensive.Sql.Model.Index;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// SQL translator.
  /// </summary>
  public abstract class SqlTranslator : SqlDriverBound
  {
    protected readonly bool supportsClusteredIndexes;
    protected readonly bool supportsExplicitJoinOrder;
    protected readonly bool supportsMultischemaQueries;


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

    public virtual SqlHelper.EscapeSetup EscapeSetup => SqlHelper.EscapeSetup.WithBrackets;

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

    public virtual string DateOnlyFormatString => throw new NotImplementedException();

    /// <summary>
    /// Gets the time span format string.
    /// See <see cref="SqlHelper.TimeSpanToString"/> for details.
    /// </summary>
    public abstract string TimeSpanFormatString { get; }

    public virtual string TimeOnlyFormatString => throw new NotImplementedException();

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

    #region Translate methods for nodes and expressions that write to output directly

    /// <summary>
    /// Translates <see cref="SqlAggregate"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlAggregate node, NodeSection section)
    {
      var output = context.Output;
      switch (section) {
        case NodeSection.Entry:
          Translate(context.Output, node.NodeType);
          _ = output.AppendOpeningPunctuation(node.Distinct ? "(DISTINCT" : "(");
          break;
        case NodeSection.Exit:
          _ = output.AppendClosingPunctuation(")");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlAlterDomain"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlAlterDomain node, AlterDomainSection section)
    {
      var output = context.Output;
      switch (section) {
        case AlterDomainSection.Entry:
          _ = output.Append("ALTER DOMAIN ");
          Translate(context, node.Domain);
          break;
        case AlterDomainSection.AddConstraint:
          _ = output.Append("ADD");
          break;
        case AlterDomainSection.DropConstraint:
          _ = output.Append("DROP");
          break;
        case AlterDomainSection.SetDefault:
          _ = output.Append("SET DEFAULT");
          break;
        case AlterDomainSection.DropDefault:
          _ = output.Append("DROP DEFAULT");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlAlterPartitionFunction"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlAlterPartitionFunction node)
    {
      var output = context.Output;
      _ = output.Append("ALTER PARTITION FUNCTION ");
      TranslateIdentifier(output, node.PartitionFunction.DbName);
      _ = output.Append("()")
        .Append(node.Option == SqlAlterPartitionFunctionOption.Split ? " SPLIT RANGE (" : " MERGE RANGE (")
        .Append(node.Boundary)
        .Append(")");
    }

    /// <summary>
    /// Translates <see cref="SqlAlterPartitionScheme"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlAlterPartitionScheme node)
    {
      var output = context.Output;
      _ = output.Append("ALTER PARTITION SCHEME ");
      TranslateIdentifier(output, node.PartitionSchema.DbName);
      _ = output.Append(" NEXT USED");
      if (!string.IsNullOrEmpty(node.Filegroup)) {
        _ = output.AppendSpace().Append(node.Filegroup);
      }
    }

    /// <summary>
    /// Translates <see cref="SqlAlterTable"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      var output = context.Output;
      switch (section) {
        case AlterTableSection.Entry:
          _ = output.Append("ALTER TABLE ");
          Translate(context, node.Table);
          break;
        case AlterTableSection.AddColumn:
          _ = output.Append("ADD COLUMN");
          break;
        case AlterTableSection.AlterColumn:
          _ = output.Append("ALTER COLUMN");
          break;
        case AlterTableSection.DropColumn:
          _ = output.Append("DROP COLUMN");
          break;
        case AlterTableSection.AddConstraint:
          _ = output.Append("ADD");
          break;
        case AlterTableSection.DropConstraint:
          _ = output.Append("DROP");
          break;
        case AlterTableSection.RenameColumn:
          _ = output.Append("RENAME COLUMN");
          break;
        case AlterTableSection.To:
          _ = output.Append("TO");
          break;
        case AlterTableSection.DropBehavior when node.Action is SqlCascadableAction cascadableAction:
          _ = output.Append(cascadableAction.Cascade ? "CASCADE" : "RESTRICT");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlAlterSequence"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlAlterSequence node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          _ = context.Output.Append("ALTER SEQUENCE ");
          Translate(context, node.Sequence);
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="TableColumn"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="column">Column to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      var output = context.Output;
      switch (section) {
        case TableColumnSection.Entry:
          TranslateIdentifier(output, column.DbName);
          break;
        case TableColumnSection.Type:
          if (column.Domain == null) {
            _ = output.Append(Translate(column.DataType));
          }
          else {
            Translate(context, column.Domain);
          }
          break;
        case TableColumnSection.DefaultValue:
          _ = output.Append("DEFAULT");
          break;
        case TableColumnSection.DropDefault:
          _ = output.Append("DROP DEFAULT");
          break;
        case TableColumnSection.SetDefault:
          _ = output.Append("SET DEFAULT");
          break;
        case TableColumnSection.GenerationExpressionExit:
          _ = output.Append(column.IsPersisted ? ") PERSISTED" : ")");
          break;
        case TableColumnSection.SetIdentityInfoElement:
          _ = output.Append("SET");
          break;
        case TableColumnSection.NotNull:
          _ = output.Append("NOT NULL");
          break;
        case TableColumnSection.Collate:
          _ = output.Append("COLLATE ");
          Translate(context, column.Collation);
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="Constraint"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="constraint">Constraint to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      var output = context.Output;
      switch (section) {
        case ConstraintSection.Entry when !string.IsNullOrEmpty(constraint.DbName):
          _ = output.Append("CONSTRAINT ");
          TranslateIdentifier(output, constraint.DbName);
          break;
        case ConstraintSection.Check:
          _ = output.AppendOpeningPunctuation("CHECK (");
          break;
        case ConstraintSection.PrimaryKey:
          _ = output.AppendOpeningPunctuation("PRIMARY KEY (");
          break;
        case ConstraintSection.Unique:
          _ = output.AppendOpeningPunctuation("UNIQUE (");
          break;
        case ConstraintSection.ForeignKey:
          _ = output.AppendOpeningPunctuation("FOREIGN KEY (");
          break;
        case ConstraintSection.ReferencedColumns: {
          var fk = (ForeignKey) constraint;
          _ = output.Append(") REFERENCES ");
          Translate(context, fk.ReferencedColumns[0].DataTable);
          _ = output.AppendOpeningPunctuation(" (");
        }
        break;
        case ConstraintSection.Exit: {
          _ = output.AppendClosingPunctuation(")");
          if (constraint is ForeignKey fk) {
            if (fk.MatchType != SqlMatchType.None) {
              _ = output.Append(" MATCH ");
              Translate(output, fk.MatchType);
            }
            if (fk.OnUpdate != ReferentialAction.NoAction) {
              _ = output.Append(" ON UPDATE ");
              Translate(output, fk.OnUpdate);
            }
            if (fk.OnDelete != ReferentialAction.NoAction) {
              _ = output.Append(" ON DELETE ");
              Translate(output, fk.OnDelete);
            }
          }
          if (constraint.IsDeferrable.HasValue) {
            _ = output.Append(constraint.IsDeferrable.Value ? " DEFERRABLE" : " NOT DEFERRABLE");
          }
          if (constraint.IsInitiallyDeferred.HasValue) {
            _ = output.Append(constraint.IsInitiallyDeferred.Value ? " INITIALLY DEFERRED" : " INITIALLY IMMEDIATE");
          }
        }
        break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlArray"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlArray node, ArraySection section) =>
      _ = context.Output.Append(section switch {
        ArraySection.Entry => "(",
        ArraySection.Exit => ")",
        ArraySection.EmptyArray => "(NULL)",
        _ => throw new ArgumentOutOfRangeException(nameof(section))
      });

    /// <summary>
    /// Translates <see cref="SqlAssignment"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlAssignment node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          _ = context.Output.Append("SET");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlBetween"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlBetween node, BetweenSection section)
    {
      switch (section) {
        case BetweenSection.Between:
          Translate(context.Output, node.NodeType);
          break;
        case BetweenSection.And:
          _ = context.Output.Append("AND");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlBinary"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlBinary node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry when node.NodeType != SqlNodeType.RawConcat:
          _ = context.Output.AppendOpeningPunctuation(OpeningParenthesis);
          break;
        case NodeSection.Exit when node.NodeType != SqlNodeType.RawConcat:
          _ = context.Output.Append(ClosingParenthesis);
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlBreak"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlBreak node) => context.Output.Append("BREAK");

    /// <summary>
    /// Translates <see cref="SqlCase"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCase node, CaseSection section)
    {
      switch (section) {
        case CaseSection.Entry:
          _ = context.Output.Append("(CASE");
          break;
        case CaseSection.Else:
          _ = context.Output.Append("ELSE");
          break;
        case CaseSection.Exit:
          _ = context.Output.Append("END)");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlCase"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="item"></param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCase node, SqlExpression item, CaseSection section)
    {
      switch (section) {
        case CaseSection.When:
          _ = context.Output.Append("WHEN");
          break;
        case CaseSection.Then:
          _ = context.Output.Append("THEN");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlCast"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          _ = context.Output.AppendOpeningPunctuation("CAST(");
          break;
        case NodeSection.Exit:
          _ = context.Output.Append(" AS ")
            .Append(Translate(node.Type))
            .Append(")");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlCloseCursor"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCloseCursor node) =>
      _ = context.Output.Append("CLOSE ").Append(node.Cursor.Name);

    /// <summary>
    /// Translates <see cref="SqlCollate"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCollate node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          _ = context.Output.Append("COLLATE ").Append(node.Collation.DbName);
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlColumnRef"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Node to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlColumnRef node, ColumnSection section)
    {
      var output = context.Output;
      switch (section) {
        case ColumnSection.Entry:
          TranslateIdentifier(output, node.Name);
          break;
        case ColumnSection.AliasDeclaration when !string.IsNullOrEmpty(node.Name):
          _ = output.Append(" AS ");
          TranslateIdentifier(output, node.Name);
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlComment"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="comment">Comment to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlComment comment)
    {
      if (comment?.Text == null) {
        return;
      }

      if (comment.Text.IndexOfAny(new char[] { '*', '/' }) != -1) {
        throw new ArgumentException(string.Format(Strings.ExArgumentContainsInvalidCharacters, nameof(comment), "*/"));
      }

      _ = context.Output.Append($"/*{comment.Text}*/");
    }

    /// <summary>
    /// Translates <see cref="SqlConcat"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlConcat node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          _ = context.Output.AppendOpeningPunctuation("(");
          break;
        case NodeSection.Exit:
          _ = context.Output.AppendClosingPunctuation(")");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlContinue"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlContinue node) => context.Output.Append("CONTINUE");

    /// <summary>
    /// Translates <see cref="SqlCreateAssertion"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCreateAssertion node, NodeSection section)
    {
      var output = context.Output;
      switch (section) {
        case NodeSection.Entry:
          _ = output.Append("CREATE ASSERTION ");
          Translate(context, node.Assertion);
          _ = output.Append(" CHECK");
          break;
        case NodeSection.Exit:
          var assertion = node.Assertion;
          if (assertion.IsDeferrable.HasValue) {
            _ = output.Append(assertion.IsDeferrable.Value ? " DEFERRABLE" : " NOT DEFERRABLE");
          }
          if (assertion.IsInitiallyDeferred.HasValue) {
            _ = output.Append(assertion.IsInitiallyDeferred.Value ? " INITIALLY DEFERRED" : " INITIALLY IMMEDIATE");
          }
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlCreateCharacterSet"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
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

    /// <summary>
    /// Translates <see cref="SqlCreateCollation"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
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

    /// <summary>
    /// Translates <see cref="SqlCreateDomain"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCreateDomain node, CreateDomainSection section)
    {
      var output = context.Output;
      switch (section) {
        case CreateDomainSection.Entry:
          _ = output.Append("CREATE DOMAIN ");
          Translate(context, node.Domain);
          _ = output.Append(" AS ").Append(Translate(node.Domain.DataType));
          break;
        case CreateDomainSection.DomainDefaultValue:
          _ = output.Append("DEFAULT");
          break;
        case CreateDomainSection.DomainCollate:
          _ = output.Append("COLLATE ");
          Translate(context, node.Domain.Collation);
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlCreateIndex"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      var output = context.Output;
      switch (section) {
        case CreateIndexSection.Entry:
          var index = node.Index;
          if (index.IsFullText) {
            _ = output.Append("CREATE FULLTEXT INDEX ON ");
            Translate(context, index.DataTable);
            return;
          }

          _ = output.Append("CREATE ");
          if (index.IsUnique) {
            _ = output.Append("UNIQUE ");
          }
          else if (index.IsBitmap) {
            _ = output.Append("BITMAP ");
          }
          else if (index.IsSpatial) {
            _ = output.Append("SPATIAL ");
          }

          if (index.IsClustered && supportsClusteredIndexes) {
            _ = output.Append("CLUSTERED ");
          }

          _ = output.Append("INDEX ");
          TranslateIdentifier(output, index.DbName);
          _ = output.Append(" ON ");
          Translate(context, index.DataTable);
          break;
        case CreateIndexSection.ColumnsEnter:
          _ = output.AppendOpeningPunctuation("(");
          break;
        case CreateIndexSection.ColumnsExit:
          _ = output.AppendClosingPunctuation(")");
          break;
        case CreateIndexSection.NonkeyColumnsEnter:
          _ = output.AppendOpeningPunctuation("INCLUDE (");
          break;
        case CreateIndexSection.NonkeyColumnsExit:
          _ = output.AppendClosingPunctuation(")");
          break;
        case CreateIndexSection.Where:
          _ = output.Append("WHERE");
          break;
        case CreateIndexSection.Exit:
          index = node.Index;
          if (index.FillFactor.HasValue) {
            _ = output.Append($"WITH (FILLFACTOR = {index.FillFactor.Value})");
          }
          if (index.PartitionDescriptor != null) {
            _ = output.Append(" ");
            Translate(output, index.PartitionDescriptor, true);
          }
          else if (!string.IsNullOrEmpty(index.Filegroup)) {
            _ = output.Append($" ON {index.Filegroup}");
          }
          else if (index is FullTextIndex ftIndex) {
            _ = output.Append(" KEY INDEX ");
            TranslateIdentifier(output, ftIndex.UnderlyingUniqueIndex);
          }
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlCreatePartitionFunction"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCreatePartitionFunction node)
    {
      var output = context.Output;
      PartitionFunction pf = node.PartitionFunction;
      _ = output.Append("CREATE PARTITION FUNCTION ");
      TranslateIdentifier(output, pf.DbName);
      _ = output.AppendOpeningPunctuation(" (")
        .Append(Translate(pf.DataType))
        .Append(")")
        .Append(" AS RANGE ")
        .Append(pf.BoundaryType == BoundaryType.Left ? "LEFT" : "RIGHT")
        .AppendOpeningPunctuation(" FOR VALUES (");

      var first = true;
      foreach (var value in pf.BoundaryValues) {
        if (first)
          first = false;
        else
          _ = output.Append(RowItemDelimiter);
        var t = Type.GetTypeCode(value.GetType());
        if ((t is TypeCode.String or TypeCode.Char)) {
          TranslateString(output, value);
        }
        else {
          _ = output.Append(value);
        }
        first = false;
      }

      _ = output.Append(")");
    }

    /// <summary>
    /// Translates <see cref="SqlCreatePartitionScheme"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCreatePartitionScheme node)
    {
      var output = context.Output;
      var ps = node.PartitionSchema;
      _ = output.Append("CREATE PARTITION SCHEME ");
      TranslateIdentifier(output, ps.DbName);
      _ = output.Append(" AS PARTITION ");
      TranslateIdentifier(output, ps.PartitionFunction.DbName);
      if (ps.Filegroups.Count <= 1) {
        _ = output.Append(" ALL");
      }

      _ = output.Append(" TO (");
      var first = true;
      foreach (var filegroup in ps.Filegroups) {
        if (first)
          first = false;
        else
          _ = output.Append(RowItemDelimiter);
        _ = output.Append(filegroup);
      }
      _ = output.Append(")");
    }

    /// <summary>
    /// Translates <see cref="SqlCreateSchema"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCreateSchema node, NodeSection section)
    {
      var output = context.Output;
      switch (section) {
        case NodeSection.Entry:
          _ = output.Append("CREATE SCHEMA ");
          if (!string.IsNullOrEmpty(node.Schema.DbName)) {
            TranslateIdentifier(output, node.Schema.DbName);
          }
          if (node.Schema.Owner != null) {
            _ = output.Append(" AUTHORIZATION ");
            TranslateIdentifier(output, node.Schema.Owner);
          }
          if (node.Schema.DefaultCharacterSet != null) {
            _ = output.Append(" DEFAULT CHARACTER SET ");
            Translate(context, node.Schema.DefaultCharacterSet);
          }
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlCreateSequence"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCreateSequence node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          _ = context.Output.Append("CREATE SEQUENCE ");
          Translate(context, node.Sequence);
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlCreateTable"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      var output = context.Output;
      switch (section) {
        case CreateTableSection.Entry: {
          _ = output.Append("CREATE ");
          if (node.Table is TemporaryTable tempTable) {
            _ = output.Append(tempTable.IsGlobal ? "GLOBAL TEMPORARY " : "LOCAL TEMPORARY ");
          }
          _ = output.Append("TABLE ");
          Translate(context, node.Table);
          break;
        }
        case CreateTableSection.TableElementsEntry:
          _ = output.AppendOpeningPunctuation("(");
          break;
        case CreateTableSection.TableElementsExit:
          _ = output.AppendOpeningPunctuation(")");
          break;
        case CreateTableSection.Partition:
          Translate(output, node.Table.PartitionDescriptor, true);
          break;
        case CreateTableSection.Exit: {
          if (!string.IsNullOrEmpty(node.Table.Filegroup)) {
            _ = output.Append(" ON ");
            TranslateIdentifier(output, node.Table.Filegroup);
          }
          if (node.Table is TemporaryTable tempTable) {
            _ = output.Append(tempTable.PreserveRows ? " ON COMMIT PRESERVE ROWS" : " ON COMMIT DELETE ROWS");
          }
          break;
        }
      }
    }

    /// <summary>
    /// Translates <see cref="SequenceDescriptor"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="descriptor">Sequence descriptor to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      TranslateSequenceDescriptorDefault(context, descriptor, section);
    }

    /// <summary>
    /// Default translation method that is used by <see cref="Translate(SqlCompilerContext, SequenceDescriptor, SequenceDescriptorSection)"/>
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="descriptor">Sequence descriptor to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    protected void TranslateSequenceDescriptorDefault(SqlCompilerContext context, SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      var output = context.Output;
      switch (section) {
        case SequenceDescriptorSection.StartValue when descriptor.StartValue.HasValue:
          _ = output.Append("START WITH ").Append(descriptor.StartValue.Value);
          break;
        case SequenceDescriptorSection.RestartValue when descriptor.StartValue.HasValue:
          _ = output.Append("RESTART WITH ").Append(descriptor.StartValue.Value);
          break;
        case SequenceDescriptorSection.Increment when descriptor.Increment.HasValue:
          _ = output.Append("INCREMENT BY ").Append(descriptor.Increment.Value);
          break;
        case SequenceDescriptorSection.MaxValue when descriptor.MaxValue.HasValue:
          _ = output.Append("MAXVALUE ").Append(descriptor.MaxValue.Value);
          break;
        case SequenceDescriptorSection.MinValue when descriptor.MinValue.HasValue:
          _ = output.Append("MINVALUE ").Append(descriptor.MinValue.Value);
          break;
        case SequenceDescriptorSection.AlterMaxValue:
          _ = descriptor.MaxValue.HasValue
            ? output.Append("MAXVALUE ").Append(descriptor.MaxValue.Value)
            : output.Append("NO MAXVALUE");
          break;
        case SequenceDescriptorSection.AlterMinValue:
          _ = descriptor.MinValue.HasValue
            ? output.Append("MINVALUE ").Append(descriptor.MinValue.Value)
            : output.Append("NO MINVALUE");
          break;
        case SequenceDescriptorSection.IsCyclic when descriptor.IsCyclic.HasValue:
          _ = output.Append(descriptor.IsCyclic.Value ? "CYCLE" : "NO CYCLE");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlCreateTranslation"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
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

    /// <summary>
    /// Translates <see cref="SqlCreateView"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCreateView node, NodeSection section)
    {
      var output = context.Output;
      switch (section) {
        case NodeSection.Entry:
          _ = output.Append("CREATE VIEW ");
          Translate(context, node.View);
          if (node.View.ViewColumns.Count > 0) {
            _ = output.Append(" (");
            var first = true;
            foreach (DataTableColumn c in node.View.ViewColumns) {
              if (first)
                first = false;
              else
                _ = output.Append(ColumnDelimiter);
              _ = output.Append(c.DbName);
            }
            _ = output.Append(")");
          }
          _ = output.Append(" AS");
          break;
        case NodeSection.Exit:
          switch (node.View.CheckOptions) {
            case CheckOptions.Cascaded:
              _ = output.Append("WITH CASCADED CHECK OPTION");
              break;
            case CheckOptions.Local:
              _ = output.Append("WITH LOCAL CHECK OPTION");
              break;
          }
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlCursor"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCursor node) => context.Output.Append(node.Name);

    /// <summary>
    /// Translates <see cref="SqlDeclareCursor"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDeclareCursor node, DeclareCursorSection section)
    {
      var output = context.Output;
      switch (section) {
        case DeclareCursorSection.Entry:
          _ = output.Append("DECLARE ").Append(node.Cursor.Name);
          break;
        case DeclareCursorSection.Sensivity:
          if (node.Cursor.Insensitive)
            _ = output.Append("INSENSITIVE");
          break;
        case DeclareCursorSection.Scrollability:
          if (node.Cursor.Scroll)
            _ = output.Append("SCROLL");
          break;
        case DeclareCursorSection.Cursor:
          _ = output.Append("CURSOR");
          break;
        case DeclareCursorSection.For:
          _ = output.Append("FOR");
          break;
        case DeclareCursorSection.Holdability:
          _ = output.Append(node.Cursor.WithHold ? "WITH HOLD" : "WITHOUT HOLD");
          break;
        case DeclareCursorSection.Returnability:
          _ = output.Append(node.Cursor.WithReturn ? "WITH RETURN " : "WITHOUT RETURN ");
          break;
        case DeclareCursorSection.Updatability:
          _ = output.Append(node.Cursor.ReadOnly ? "FOR READ ONLY" : "FOR UPDATE");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlDeclareVariable"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDeclareVariable node)
    {
      _ = context.Output.Append("DECLARE @")
        .Append(node.Variable.Name)
        .Append(" AS ")
        .Append(Translate(node.Variable.Type));
    }

    /// <summary>
    /// Translates <see cref="SqlDefaultValue"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDefaultValue node) => context.Output.Append("DEFAULT");

    /// <summary>
    /// Translates <see cref="SqlDelete"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDelete node, DeleteSection section)
    {
      _ = context.Output.Append(section switch {
        DeleteSection.Entry => "DELETE FROM",
        DeleteSection.From => "FROM",
        DeleteSection.Where => "WHERE",
        DeleteSection.Limit => "LIMIT",
        _ => string.Empty
      });
    }

    /// <summary>
    /// Translates <see cref="SqlDropAssertion"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropAssertion node)
    {
      _ = context.Output.Append("DROP ASSERTION ");
      Translate(context, node.Assertion);
    }

    /// <summary>
    /// Translates <see cref="SqlDropCharacterSet"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropCharacterSet node)
    {
      _ = context.Output.Append("DROP CHARACTER SET ");
      Translate(context, node.CharacterSet);
    }

    /// <summary>
    /// Translates <see cref="SqlDropCollation"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropCollation node)
    {
      _ = context.Output.Append("DROP COLLATION ");
      Translate(context, node.Collation);
    }

    /// <summary>
    /// Translates <see cref="SqlDropDomain"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropDomain node)
    {
      _ = context.Output.Append("DROP DOMAIN ");
      Translate(context, node.Domain);
      _ = context.Output.Append(node.Cascade ? " CASCADE" : " RESTRICT");
    }

    /// <summary>
    /// Translates <see cref="SqlDropIndex"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      var output = context.Output;
      if (!node.Index.IsFullText) {
        _ = output.Append("DROP INDEX ");
        TranslateIdentifier(output, node.Index.DbName);
        _ = output.Append(" ON ");
      }
      else {
        _ = output.Append("DROP FULLTEXT INDEX ON ");
      }

      Translate(context, node.Index.DataTable);
    }

    /// <summary>
    /// Translates <see cref="SqlDropPartitionFunction"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropPartitionFunction node)
    {
      _ = context.Output.Append("DROP PARTITION FUNCTION ");
      TranslateIdentifier(context.Output, node.PartitionFunction.DbName);
    }

    /// <summary>
    /// Translates <see cref="SqlDropPartitionScheme"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropPartitionScheme node)
    {
      _ = context.Output.Append("DROP PARTITION SCHEME ");
      TranslateIdentifier(context.Output, node.PartitionSchema.DbName);
    }

    /// <summary>
    /// Translates <see cref="SqlDropSchema"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropSchema node)
    {
      var output = context.Output;
      _ = output.Append("DROP SCHEMA ");
      TranslateIdentifier(output, node.Schema.DbName);
      _ = output.Append(node.Cascade ? " CASCADE" : " RESTRICT");
    }

    /// <summary>
    /// Translates <see cref="SqlDropSequence"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropSequence node)
    {
      _ = context.Output.Append("DROP SEQUENCE ");
      Translate(context, node.Sequence);
      _ = context.Output.Append(node.Cascade ? " CASCADE" : " RESTRICT");
    }

    /// <summary>
    /// Translates <see cref="SqlDropTable"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropTable node)
    {
      _ = context.Output.Append("DROP TABLE ");
      Translate(context, node.Table);
      _ = context.Output.Append(node.Cascade ? " CASCADE" : " RESTRICT");
    }

    public virtual void Translate(SqlCompilerContext context, SqlTruncateTable node)
    {
      _ = context.Output.Append("TRUNCATE TABLE ");
      Translate(context, node.Table);
    }

    /// <summary>
    /// Translates <see cref="SqlDropTranslation"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropTranslation node)
    {
      _ = context.Output.Append("DROP TRANSLATION ");
      Translate(context, node.Translation);
    }

    /// <summary>
    /// Translates <see cref="SqlDropView"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlDropView node)
    {
      _ = context.Output.Append("DROP VIEW ");
      Translate(context, node.View);
      _ = context.Output.Append(node.Cascade ? " CASCADE" : " RESTRICT");
    }

    /// <summary>
    /// Translates <see cref="SqlFetch"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlFetch node, FetchSection section)
    {
      var output = context.Output;
      switch (section) {
        case FetchSection.Entry:
          _ = context.Output.Append("FETCH ")
            .Append(node.Option.ToString());
          break;
        case FetchSection.Targets:
          _ = context.Output.Append("FROM ")
            .Append(node.Cursor.Name);
          if (node.Targets.Count != 0) {
            _ = context.Output.Append(" INTO");
          }
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlFunctionCall"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    /// <param name="position">Argument position.</param>
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
              Translate(output, node.FunctionType);
              break;
            case SqlFunctionType.Position when Driver.ServerInfo.StringIndexingBase > 0:
              _ = output.AppendOpeningPunctuation("(");
              Translate(output, node.FunctionType);
              _ = output.AppendOpeningPunctuation("(");
              break;
            default:
              Translate(output, node.FunctionType);
              _ = output.Append(node.Arguments.Count == 0 ? "()" : "(");
              break;
          }
          break;
        case FunctionCallSection.ArgumentEntry:
          switch (node.FunctionType) {
            case SqlFunctionType.Position when position == 1:
              _ = output.Append("IN");
              break;
            case SqlFunctionType.Substring:
              switch (position) {
                case 1:
                  _ = output.Append("FROM");
                  break;
                case 2:
                  _ = output.Append("FOR");
                  break;
              }
              break;
          }
          break;
        case FunctionCallSection.ArgumentExit when node.FunctionType == SqlFunctionType.Substring
            && position == 1
            && Driver.ServerInfo.StringIndexingBase > 0:
          _ = output.Append("+ ").Append(Driver.ServerInfo.StringIndexingBase);
          break;
        case FunctionCallSection.ArgumentDelimiter:
          switch (node.FunctionType) {
            case SqlFunctionType.Position:
            case SqlFunctionType.Substring:
              break;
            default:
              _ = output.Append(ArgumentDelimiter);
              break;
          }
          break;
        case FunctionCallSection.Exit:
          if (node.FunctionType == SqlFunctionType.Position && Driver.ServerInfo.StringIndexingBase > 0) {
            _ = output.Append(") - ")
              .Append(Driver.ServerInfo.StringIndexingBase)
              .Append(")");
          }
          else if (node.Arguments.Count != 0) {
            _ = output.Append(")");
          }
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlExtract"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="extract">Expression to translate</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlExtract extract, ExtractSection section)
    {
      _ = context.Output.Append(section switch {
        ExtractSection.Entry => "EXTRACT(",
        ExtractSection.From => "FROM",
        ExtractSection.Exit => ")",
        _ => string.Empty
      });
    }

    /// <summary>
    /// Translates <see cref="SqlIf"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlIf node, IfSection section)
    {
      _ = context.Output.Append(section switch {
        IfSection.Entry => "IF",
        IfSection.True => "BEGIN",
        IfSection.False => "END BEGIN",
        IfSection.Exit => "END",
        _ => string.Empty
      });
    }

    /// <summary>
    /// Translates <see cref="SqlInsert"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlInsert node, InsertSection section)
    {
      var output = context.Output;
      switch (section) {
        case InsertSection.Entry:
          _ = output.Append("INSERT INTO");
          break;
        case InsertSection.ColumnsEntry when node.Values.Columns.Count > 0:
          _ = output.AppendOpeningPunctuation("(");
          break;
        case InsertSection.ColumnsExit when node.Values.Columns.Count > 0:
          _ = output.Append(")");
          break;
        case InsertSection.From:
          _ = output.Append("FROM");
          break;
        case InsertSection.ValuesEntry:
          _ = output.AppendOpeningPunctuation("VALUES (");
          break;
        case InsertSection.ValuesExit:
          _ = output.Append(")");
          break;
        case InsertSection.DefaultValues:
          _ = output.Append("DEFAULT VALUES");
          break;
        case InsertSection.NewRow:
          _ = output.Append("), (");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlJoinExpression"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Node to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlJoinExpression node, JoinSection section)
    {
      var output = context.Output;
      var traversalPath = context.GetTraversalPath().Skip(1);
      var explicitJoinOrder = supportsExplicitJoinOrder
        && traversalPath.FirstOrDefault() is SqlJoinExpression;

      switch (section) {
        case JoinSection.Entry when explicitJoinOrder:
          _ = output.Append("(");
          break;
        case JoinSection.Specification:
          var isNatural = node.Expression is null
            && node.JoinType != SqlJoinType.CrossJoin
            && node.JoinType != SqlJoinType.UnionJoin;
          if (isNatural) {
            _ = output.Append("NATURAL ");
          }
          Translate(output, node.JoinType);
          _ = output.Append(" JOIN");
          break;
        case JoinSection.Condition:
          _ = output.Append(node.JoinType == SqlJoinType.UsingJoin ? "USING" : "ON");
          break;
        case JoinSection.Exit when explicitJoinOrder:
          _ = output.Append(")");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlLike"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlLike node, LikeSection section)
    {
      var output = context.Output;
      switch (section) {
        case LikeSection.Entry:
          _ = output.Append("(");
          break;
        case LikeSection.Exit:
          _ = output.AppendClosingPunctuation(")");
          break;
        case LikeSection.Like:
          _ = output.Append(node.Not ? "NOT LIKE" : "LIKE");
          break;
        case LikeSection.Escape:
          _ = output.Append("ESCAPE");
          break;
      }
    }

    /// <summary>
    /// Translates literal values like numbers, string, char, TimeSpan, DateTime values, etc.
    /// and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="literalValue">Value to translate.</param>
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
          _ = output.Append(((DateTime) literalValue).ToString(DateTimeFormatString, DateTimeFormat));
          return;
        case TypeCode.Single:
          _ = output.Append(((float) literalValue).ToString(FloatFormatString, FloatNumberFormat));
          return;
        case TypeCode.Double:
          _ = output.Append(((double) literalValue).ToString(DoubleFormatString, DoubleNumberFormat));
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
          _ = output.Append(Convert.ToString(literalValue, IntegerNumberFormat));
          return;
      }
      switch (literalValue) {
        case TimeSpan timeSpan:
          _ = output.Append(SqlHelper.TimeSpanToString(timeSpan, TimeSpanFormatString));
          break;
        case TypeInfo typeInfo:
          output.AppendPlaceholderWithId(typeInfo);
          break;
        case Guid:
        case byte[]:
          throw new NotSupportedException(string.Format(Strings.ExTranslationOfLiteralOfTypeXIsNotSupported, literalType.GetShortName()));
#if DO_DATEONLY
        case DateOnly dateOnly:
          output.Append(dateOnly.ToString(DateOnlyFormatString, DateTimeFormat));
          break;
        case TimeOnly timeOnly:
          output.Append(timeOnly.ToString(TimeOnlyFormatString, DateTimeFormat));
          break;
#endif
        default:
          _ = output.Append(literalValue.ToString());
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlMatch"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlMatch node, MatchSection section)
    {
      switch (section) {
        case MatchSection.Specification:
          var output = context.Output;
          _ = output.Append(" MATCH ");
          if (node.Unique) {
            _ = output.Append("UNIQUE ");
          }
          Translate(output, node.MatchType);
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlNative"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlNative node) => context.Output.Append(node.Value);

    /// <summary>
    /// Translates <see cref="SqlNextValue"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          _ = context.Output.Append("NEXT VALUE FOR ");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlNull"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlNull node) =>
      context.Output.Append("NULL");

    /// <summary>
    /// Translates <see cref="SqlOpenCursor"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlOpenCursor node) =>
      context.Output.Append("OPEN ").Append(node.Cursor.Name);

    /// <summary>
    /// Translates <see cref="SqlOrder"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Node to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlOrder node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          TranslateSortOrder(context.Output, node.Ascending);
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlQueryExpression"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlQueryExpression node, QueryExpressionSection section)
    {
      switch (section) {
        case QueryExpressionSection.All when node.All:
          _ = context.Output.Append(" ALL");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlQueryRef"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlQueryRef node, TableSection section)
    {
      switch (section) {
        case TableSection.Entry when !(node.Query is SqlFreeTextTable || node.Query is SqlContainsTable):
          _ = context.Output.AppendOpeningPunctuation("(");
          break;
        case TableSection.Exit when !(node.Query is SqlFreeTextTable || node.Query is SqlContainsTable):
          _ = context.Output.Append(")");
          break;
        case TableSection.AliasDeclaration:
          var alias = context.TableNameProvider.GetName(node);
          if (!string.IsNullOrEmpty(alias)) {
            TranslateIdentifier(context.Output, alias);
          }
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlRow"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlRow node, NodeSection section)
    {
      _ = section switch {
        NodeSection.Entry => context.Output.AppendOpeningPunctuation("("),
        NodeSection.Exit => context.Output.AppendClosingPunctuation(")"),
        _ => throw new ArgumentOutOfRangeException(nameof(section))
      };
    }

    /// <summary>
    /// Translates <see cref="SqlRowNumber"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlRowNumber node, NodeSection section)
    {
      _ = section switch {
        NodeSection.Entry => context.Output.Append("ROW_NUMBER() OVER(ORDER BY"),
        NodeSection.Exit => context.Output.Append(")"),
        _ => throw new ArgumentOutOfRangeException(nameof(section)),
      };
    }

    /// <summary>
    /// Translates <see cref="SqlRenameTable"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlRenameTable node)
    {
      _ = context.Output.Append("ALTER TABLE ");
      Translate(context, node.Table);
      _ = context.Output.Append(" RENAME TO ");
      TranslateIdentifier(context.Output, node.NewName);
    }

    /// <summary>
    /// Translates <see cref="SqlSelect"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      _ = context.Output.Append(section switch {
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

    /// <summary>
    /// Translates <see cref="SqlStatementBlock"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlStatementBlock node, NodeSection section)
    {
      _ = section switch {
        NodeSection.Entry => context.Output.Append("BEGIN"),
        NodeSection.Exit => context.Output.Append("END"),
        _ => throw new ArgumentOutOfRangeException(nameof(section)),
      };
    }

    /// <summary>
    /// Translates <see cref="SqlSubQuery"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlSubQuery node, NodeSection section)
    {
      _ = section switch {
        NodeSection.Entry => context.Output.AppendOpeningPunctuation("("),
        NodeSection.Exit => context.Output.Append(")"),
        _ => throw new ArgumentOutOfRangeException(nameof(section))
      };
    }

    /// <summary>
    /// Translates <see cref="SqlTable"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Node to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlTable node, NodeSection section) =>
      TranslateIdentifier(context.Output, context.TableNameProvider.GetName(node));

    /// <summary>
    /// Translates <see cref="SqlTableColumn"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Node to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlTableColumn node, NodeSection section)
    {
      var output = context.Output;
      if ((context.NamingOptions & SqlCompilerNamingOptions.TableQualifiedColumns) != 0) {
        Translate(context, node.SqlTable, NodeSection.Entry);
        _ = output.Append(".");
      }
      if ((object) node == (object) node.SqlTable.Asterisk) {
        _ = output.Append(node.Name);
      }
      else {
        TranslateIdentifier(output, node.Name);
      }
    }

    /// <summary>
    /// Translates <see cref="SqlTableRef"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Node to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlTableRef node, TableSection section)
    {
      switch (section) {
        case TableSection.Entry:
          Translate(context, node.DataTable);
          break;
        case TableSection.AliasDeclaration:
          var alias = context.TableNameProvider.GetName(node);
          if (alias != node.DataTable.DbName) {
            TranslateIdentifier(context.Output, alias);
          }
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlTrim"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlTrim node, TrimSection section)
    {
      _ = section switch {
        TrimSection.Entry => context.Output.AppendOpeningPunctuation("TRIM("),
        TrimSection.From => context.Output.Append("FROM"),
        TrimSection.Exit => context.Output.Append(")"),
        _ => throw new ArgumentOutOfRangeException(nameof(section)),
      };
    }

    /// <summary>
    /// Translates <see cref="SqlUnary"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlUnary node, NodeSection section)
    {
      var output = context.Output;
      var omitParenthesis =
        node.NodeType is SqlNodeType.Exists or SqlNodeType.All or SqlNodeType.Some or SqlNodeType.Any;

      var isNullCheck = node.NodeType is SqlNodeType.IsNull or SqlNodeType.IsNotNull;

      switch (section) {
        case NodeSection.Entry:
          if (!omitParenthesis) {
            _ = output.AppendOpeningPunctuation("(");
          }
          if (!isNullCheck) {
            Translate(output, node.NodeType);
          }
          break;
        case NodeSection.Exit:
          if (isNullCheck) {
            _ = output.AppendSpaceIfNecessary();
            Translate(output, node.NodeType);
          }

          if (!omitParenthesis) {
            _ = output.AppendClosingPunctuation(")");
          }
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(section));
      }
    }

    /// <summary>
    /// Translates <see cref="SqlUpdate"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlUpdate node, UpdateSection section)
    {
      _ = context.Output.Append(section switch {
        UpdateSection.Entry => "UPDATE",
        UpdateSection.Set => "SET",
        UpdateSection.From => "FROM",
        UpdateSection.Where => (node.Where is SqlCursor) ? "WHERE CURRENT OF" : "WHERE",
        UpdateSection.Limit => "LIMIT",
        _ => string.Empty
      });
    }

    /// <summary>
    /// Translates <see cref="SqlUserColumn"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Node to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlUserColumn node, NodeSection section)
    {
    }

    /// <summary>
    /// Translates <see cref="SqlUserFunctionCall"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    /// <param name="position">Argument postion.</param>
    public virtual void Translate(SqlCompilerContext context, SqlUserFunctionCall node, FunctionCallSection section, int position)
    {
      switch (section) {
        case FunctionCallSection.Entry:
          _ = context.Output.Append(node.Name).AppendOpeningPunctuation("(");
          break;
        case FunctionCallSection.Exit:
          _ = context.Output.AppendClosingPunctuation(")");
          break;
        default:
          Translate(context, node as SqlFunctionCall, section, position);
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlVariable"/> expression and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Expression to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlVariable node) => context.Output.Append("@").Append(node.Name);

    /// <summary>
    /// Translates <see cref="SqlWhile"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    /// <param name="section">Particular section to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlWhile node, WhileSection section)
    {
      switch (section) {
        case WhileSection.Entry:
          _ = context.Output.AppendOpeningPunctuation("WHILE (");
          break;
        case WhileSection.Statement:
          _ = context.Output.AppendClosingPunctuation(") BEGIN");
          break;
        case WhileSection.Exit:
          _ = context.Output.Append("END");
          break;
      }
    }

    /// <summary>
    /// Translates <see cref="SqlCommand"/> statement and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Statement to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SqlCommand node)
    {
      _ = context.Output.Append(node.CommandType switch {
        SqlCommandType.SetConstraintsAllDeferred => "SET CONSTRAINTS ALL DEFERRED",
        SqlCommandType.SetConstraintsAllImmediate => "SET CONSTRAINTS ALL IMMEDIATE",
        _ => throw new NotSupportedException(string.Format(Strings.ExOperationXIsNotSupported, node.CommandType))
      });
    }

    /// <summary>
    /// Translates <see cref="SchemaNode"/> node to string.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Node to translate.</param>
    /// <returns>Result of translation</returns>
    /// <exception cref="NotSupportedException"/>
    public virtual string TranslateToString(SqlCompilerContext context, SchemaNode node) => throw new NotSupportedException();

    /// <summary>
    /// Translates <see cref="SchemaNode"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="node">Node to translate.</param>
    public virtual void Translate(SqlCompilerContext context, SchemaNode node)
    {
      var schemaQualified = node.Schema != null && supportsMultischemaQueries;

      var output = context.Output;

      if (!schemaQualified) {
        TranslateIdentifier(output, node.DbName);
        return;
      }

      var dbQualified = node.Schema.Catalog != null
        && context.HasOptions(SqlCompilerNamingOptions.DatabaseQualifiedObjects);
      var actualizer = context.SqlNodeActualizer;


      var setup = EscapeSetup;

      if (dbQualified) {
        TranslateIdentifier(output, actualizer.Actualize(node.Schema.Catalog));
        _ = output.AppendLiteral(setup.Delimiter);
      }

      if (context.ParametrizeSchemaNames) {
        _ = output.AppendLiteral(setup.Opener);
        output.AppendPlaceholderWithId(node.Schema);
        _ = output.AppendLiteral(setup.Closer);
      }
      else {
        TranslateIdentifier(output, actualizer.Actualize(node.Schema));
      }
      _ = output.AppendLiteral(setup.Delimiter);

      TranslateIdentifier(output, dbQualified ? node.GetDbNameInternal() : node.DbName);
    }

    /// <summary>
    /// Translates <see cref="Collation"/> node and writes result to to <see cref="SqlCompilerContext.Output"/>.
    /// </summary>
    /// <param name="context">The compiler context.</param>
    /// <param name="collation">Colation to translate</param>
    public virtual void Translate(SqlCompilerContext context, Collation collation)
    {
      TranslateString(context.Output, collation.DbName);
    }

    #endregion

    #region Enums and other types that require translation to string

    public virtual void Translate(IOutput output, SqlNodeType type)
    {
      _ = output.Append(type switch {
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
      });
    }

    /// <summary>
    /// Translates <see cref="SqlNodeType"/>.
    /// </summary>
    /// <param name="type">Enum value to translate.</param>
    /// <returns>SQL variant of node</returns>
    public virtual string TranslateToString(SqlNodeType type) =>
      // only two nodes need to be traslated to string
      type switch {
        SqlNodeType.Or => "OR",
        SqlNodeType.Modulo => "%",
        _ => throw new NotSupportedException(string.Format(Strings.ExOperationXIsNotSupported, type))
      };

    /// <summary>
    /// Translates <see cref="SqlJoinType"/> and writes the result to the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="type">Enum value to translate.</param>
    /// <returns>SQL variant of join type.</returns>
    public virtual void Translate(IOutput output, SqlJoinType type)
    {
      _ = output.Append(type switch {
        SqlJoinType.CrossJoin => "CROSS",
        SqlJoinType.FullOuterJoin => "FULL OUTER",
        SqlJoinType.InnerJoin => "INNER",
        SqlJoinType.LeftOuterJoin => "LEFT OUTER",
        SqlJoinType.UnionJoin => "UNION",
        SqlJoinType.RightOuterJoin => "RIGHT OUTER",
        SqlJoinType.CrossApply or SqlJoinType.LeftOuterApply => throw SqlHelper.NotSupported(QueryFeatures.CrossApply),
        _ => string.Empty
      });
    }

    public virtual void Translate(IOutput output, SqlMatchType type)
    {
      _ = output.Append(type switch {
        SqlMatchType.Full => "FULL",
        SqlMatchType.Partial => "PARTIAL",
        _ => string.Empty
      });
    }

    public virtual void Translate(IOutput output, ReferentialAction action)
    {
      _ = output.Append(action switch {
        ReferentialAction.Cascade => "CASCADE",
        ReferentialAction.SetDefault => "SET DEFAULT",
        ReferentialAction.SetNull => "SET NULL",
        _ => string.Empty,
      });
    }

    /// <summary>
    /// Translates <see cref="SqlValueType"/>.
    /// </summary>
    /// <param name="type">SQL type representation.</param>
    /// <returns>Translated SQL type.</returns>
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

    /// <summary>
    /// Translates <see cref="SqlFunctionType"/> and writes the result to the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="type">Enum value to translate.</param>
    public virtual void Translate(IOutput output, SqlFunctionType type)
    {
      _ = output.Append(type switch {
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
      });
    }

    /// <summary>
    /// Translates <see cref="SqlFunctionType"/>.
    /// </summary>
    /// <param name="type">Enum value to translate.</param>
    /// <returns>SQL variant of function.</returns>
    public virtual string TranslateToString(SqlFunctionType type) =>
      // most of the cases aren't in use, commented to keep the full list
      type switch {
        SqlFunctionType.CharLength or SqlFunctionType.BinaryLength => "LENGTH",
        //SqlFunctionType.Concat => "CONCAT",
        //SqlFunctionType.CurrentDate => "CURRENT_DATE",
        //SqlFunctionType.CurrentTime => "CURRENT_TIME",
        //SqlFunctionType.CurrentTimeStamp => "CURRENT_TIMESTAMP",
        //SqlFunctionType.Lower => "LOWER",
        //SqlFunctionType.Position => "POSITION",
        //SqlFunctionType.Substring => "SUBSTRING",
        //SqlFunctionType.Upper => "UPPER",
        SqlFunctionType.Abs => "ABS",
        //SqlFunctionType.Acos => "ACOS",
        //SqlFunctionType.Asin => "ASIN",
        //SqlFunctionType.Atan => "ATAN",
        //SqlFunctionType.Atan2 => "ATAN2",
        //SqlFunctionType.Ceiling => "CEILING",
        //SqlFunctionType.Coalesce => "COALESCE",
        //SqlFunctionType.Cos => "COS",
        //SqlFunctionType.Cot => "COT",
        SqlFunctionType.CurrentUser => "CURRENT_USER",
        //SqlFunctionType.Degrees => "DEGREES",
        //SqlFunctionType.Exp => "EXP",
        //SqlFunctionType.Floor => "FLOOR",
        //SqlFunctionType.Log => "LOG",
        //SqlFunctionType.Log10 => "LOG10",
        //SqlFunctionType.NullIf => "NULLIF",
        //SqlFunctionType.Pi => "PI",
        //SqlFunctionType.Power => "POWER",
        //SqlFunctionType.Radians => "RADIANS",
        SqlFunctionType.Rand => "RAND",
        //SqlFunctionType.Replace => "REPLACE",
        SqlFunctionType.Round => "ROUND",
        //SqlFunctionType.Truncate => "TRUNCATE",
        SqlFunctionType.SessionUser => "SESSION_USER",
        //SqlFunctionType.Sign => "SIGN",
        //SqlFunctionType.Sin => "SIN",
        //SqlFunctionType.Sqrt => "SQRT",
        //SqlFunctionType.Square => "SQUARE",
        SqlFunctionType.SystemUser => "SYSTEM_USER",
        //SqlFunctionType.Tan => "TAN",
        _ => throw new NotSupportedException(string.Format(Strings.ExFunctionXIsNotSupported, type))
      };

    /// <summary>
    /// Translates <see cref="SqlTrimType"/> and writes the result to the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="type">Enum value to translate.</param>
    public virtual void Translate(IOutput output, SqlTrimType type)
    {
      _ = output.Append(type switch {
        SqlTrimType.Leading => "LEADING",
        SqlTrimType.Trailing => "TRAILING",
        SqlTrimType.Both => "BOTH",
        _ => string.Empty
      });
    }

    /// <summary>
    /// Translates <see cref="SqlDateTimePart"/> writes the result to the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="dateTimePart">Enum value to translate.</param>
    public virtual void Translate(IOutput output, SqlDateTimePart dateTimePart)
    {
      _ = output.Append(dateTimePart switch {
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
        _ => throw new ArgumentOutOfRangeException(nameof(dateTimePart))
      });
    }

    /// <summary>
    /// Translates <see cref="SqlDateTimeOffsetPart"/> and writes result to the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="dateTimeOffsetPart">Enum value to translate.</param>
    public virtual void Translate(IOutput output, SqlDateTimeOffsetPart dateTimeOffsetPart)
    {
      _ = output.Append(dateTimeOffsetPart switch {
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
        _ => throw new ArgumentOutOfRangeException(nameof(dateTimeOffsetPart))
      });
    }

    /// <summary>
    /// Translates <see cref="SqlIntervalPart"/> and writes result to the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="intervalPart">Enum value to translate.</param>
    public virtual void Translate(IOutput output, SqlIntervalPart intervalPart)
    {
      _ = output.Append(intervalPart switch {
        SqlIntervalPart.Day => "DAY",
        SqlIntervalPart.Hour => "HOUR",
        SqlIntervalPart.Minute => "MINUTE",
        SqlIntervalPart.Second => "SECOND",
        SqlIntervalPart.Millisecond => "MILLISECOND",
        SqlIntervalPart.Nanosecond => "NANOSECOND",
        _ => throw new ArgumentOutOfRangeException(nameof(intervalPart))
      });
    }

    /// <summary>
    /// Translates <see cref="SqlLockType"/> and writes the result to the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="lockType">Enum value to translate.</param>
    /// <returns>SQL variant of lock.</returns>
    public virtual void Translate(IOutput output, SqlLockType lockType)
    {
      throw new NotSupportedException(string.Format(Strings.ExLockXIsNotSupported, lockType.ToString(true)));
    }

    /// <summary>
    /// Translates <see cref="SqlJoinMethod"/>.
    /// </summary>
    /// <param name="method">Enum value to translate.</param>
    /// <returns>SQL variant of method.</returns>
    public virtual string Translate(SqlJoinMethod method)
    {
      return string.Empty;
    }

    #endregion

    #region Other types that can be written directly to output

    /// <summary>
    /// Writes accending/descending marks to the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">Translation output.</param>
    /// <param name="ascending">Direction flag.</param>
    public virtual void TranslateSortOrder(IOutput output, bool ascending) =>
      _ = output.Append(ascending ? "ASC" : "DESC");

    /// <summary>
    /// Writes translated string value to <paramref name="output"/>
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="str">The string.</param>
    public virtual void TranslateString(IOutput output, string str)
    {
      // this is more effecient than SqlHelper.QuoteString()
      _ = output.AppendLiteral('\'');
      foreach (var ch in str) {
        TranslateChar(output, ch);
      }
      _ = output.AppendLiteral('\'');
    }

    /// <summary>
    /// Writes translated char value to <paramref name="output"/>
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="ch">The character.</param>
    protected virtual void TranslateChar(IOutput output, char ch)
    {
      switch (ch) {
        case '\0':
          break;
        case '\'':
          _ = output.AppendLiteral("''");
          break;
        default:
          _ = output.AppendLiteral(ch);
          break;
      }
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


    /// <summary>
    /// Translates identifier names (one or several) and writes result to <paramref name="output"/>
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="name">The identifier.</param>
    /// <param name="moreNames">Additional names (optional).</param>
    public void TranslateIdentifier(IOutput output, string name, params string[] moreNames)
    {
      if (string.IsNullOrEmpty(name))
        return;

      var setup = EscapeSetup;
      _ = output.AppendLiteral(setup.Opener);
      foreach (var ch in name) {
        if (ch == setup.Closer) {
          _ = output.AppendLiteral(setup.EscapeCloser1)
            .AppendLiteral(setup.EscapeCloser2);
        }
        else {
          _ = output.AppendLiteral(ch);
        }
      }
      _ = output.AppendLiteral(setup.Closer);

      if (moreNames?.Length == 0)
        return;

      foreach (var aName in moreNames) {
        if (!string.IsNullOrEmpty(aName)) {
          _ = output.AppendLiteral(setup.Delimiter)
            .AppendLiteral(setup.Opener);
          foreach (var ch in aName) {
            if (ch == setup.Closer) {
              _ = output.AppendLiteral(setup.EscapeCloser1)
                .AppendLiteral(setup.EscapeCloser2);
            }
            else {
              _ = output.AppendLiteral(ch);
            }
          }
          _ = output.AppendLiteral(setup.Closer);
        }
      }
    }

    public void TranslateIdentifier(IOutput output, string name)
    {
      if (string.IsNullOrEmpty(name))
        return;

      var setup = EscapeSetup;
      _ = output.AppendLiteral(setup.Opener);
      foreach (var ch in name) {
        if (ch == setup.Closer) {
          _ = output.AppendLiteral(setup.EscapeCloser1)
            .AppendLiteral(setup.EscapeCloser2);
        }
        else {
          _ = output.AppendLiteral(ch);
        }
      }
      _ = output.AppendLiteral(setup.Closer);
    }

    #endregion

    #region Methods that are used ouside SqlTranslators/SqlCompilers

    /// <summary>
    /// Builds the batch from specified SQL statements.
    /// </summary>
    /// <param name="statements">The statements.</param>
    /// <returns>String containing the whole batch.</returns>
    public virtual string BuildBatch(IReadOnlyList<string> statements)
    {
      if (statements.Count == 0) {
        return string.Empty;
      }
      var expectedLength = BatchBegin.Length + BatchEnd.Length
        + ((BatchItemDelimiter.Length + NewLine.Length) * statements.Count)
        + statements.Sum(statement => statement.Length);
      var builder = new StringBuilder(expectedLength);
      _ = builder.Append(BatchBegin);
      foreach (var statement in statements) {
        var statementAsSpan = (ReadOnlySpan<char>) statement;
        var actualStatement = statementAsSpan
          .Trim()
          .TryCutPrefix(BatchBegin)
          .TryCutSuffix(BatchEnd)
          .TryCutSuffix(NewLine)
          .TryCutSuffix(BatchItemDelimiter)
          .Trim();
        if (actualStatement.Length == 0)
          continue;
        _ = builder.Append(actualStatement)
          .Append(BatchItemDelimiter)
          .Append(NewLine);
      }
      _ = builder.Append(BatchEnd);
      return builder.ToString();
    }

    /// <summary>
    /// Returns quoted string.
    /// </summary>
    /// <param name="str">Unquoted string.</param>
    /// <returns>Quoted string.</returns>
    /// <remarks>
    /// Use TranslateString instead of this method within SqlTranslators/SqlCompilers where possible.
    /// </remarks>
    public virtual string QuoteString(string str)
    {
      //Use TranslateString instead of this method within SqlTranslators/SqlCompilers where possible
      return SqlHelper.QuoteString(str);
    }

    /// <summary>
    /// Returns string holding quoted identifier name.
    /// </summary>
    /// <param name="names">An <see cref="Array"/> of unquoted identifier name parts.</param>
    /// <returns>Quoted identifier name.</returns>
    /// <remarks>
    /// Use TranslateIdentifier instead of this method within SqlTranslators/SqlCompilers where possible.
    /// </remarks>
    public string QuoteIdentifier(params string[] names) =>
      //Use TranslateIdentifier instead of this method within SqlTranslators/SqlCompilers where possible
      SqlHelper.Quote(EscapeSetup, names);

    #endregion

    private void Translate(IOutput output, PartitionDescriptor partitionDescriptor, bool withOn)
    {
      if (partitionDescriptor.PartitionSchema != null) {
        if (withOn) {
          _ = output.Append("ON ");
        }
        TranslateIdentifier(output, partitionDescriptor.PartitionSchema.DbName);
        _ = output.Append(" (");
        TranslateIdentifier(output, partitionDescriptor.Column.DbName);
        _ = output.Append(")");
      }
      else {
        _ = output.Append("PARTITION BY ");
        switch (partitionDescriptor.PartitionMethod) {
          case PartitionMethod.Hash:
            _ = output.Append("HASH");
            break;
          case PartitionMethod.List:
            _ = output.Append("LIST");
            break;
          case PartitionMethod.Range:
            _ = output.Append("RANGE");
            break;
        }
        _ = output.Append(" (");
        TranslateIdentifier(output, partitionDescriptor.Column.DbName);
        _ = output.Append(")");
        if (partitionDescriptor.Partitions == null) {
          _ = output.Append(" PARTITIONS " + partitionDescriptor.PartitionAmount);
        }
        else {
          _ = output.Append(" (");
          var first = true;
          switch (partitionDescriptor.PartitionMethod) {
            case PartitionMethod.Hash:
              foreach (HashPartition p in partitionDescriptor.Partitions) {
                if (first)
                  first = false;
                else
                  _ = output.Append(ColumnDelimiter);
                _ = output.Append(" PARTITION ");
                TranslateIdentifier(output, p.DbName);
                if (!string.IsNullOrEmpty(p.Filegroup)) {
                  _ = output.Append(" TABLESPACE ")
                    .Append(p.Filegroup);
                }
              }
              break;
            case PartitionMethod.List:
              foreach (ListPartition p in partitionDescriptor.Partitions) {
                if (first)
                  first = false;
                else
                  _ = output.Append(ColumnDelimiter);
                _ = output.Append(" PARTITION ");
                TranslateIdentifier(output, p.DbName);
                _ = output.Append(" VALUES (");
                var firstValue = true;
                foreach (var v in p.Values) {
                  if (firstValue)
                    firstValue = false;
                  else
                    _ = output.Append(RowItemDelimiter);

                  var t = Type.GetTypeCode(v.GetType());
                  if (t is TypeCode.String or TypeCode.Char) {
                    TranslateString(output, v);
                  }
                  else {
                    _ = output.Append(v);
                  }
                }
                _ = output.Append(")");
                if (!string.IsNullOrEmpty(p.Filegroup)) {
                  _ = output.Append(" TABLESPACE ").Append(p.Filegroup);
                }
              }
              break;
            case PartitionMethod.Range:
              foreach (RangePartition p in partitionDescriptor.Partitions) {
                if (first)
                  first = false;
                else
                  _ = output.Append(ColumnDelimiter);
                _ = output.Append(" PARTITION ");
                TranslateIdentifier(output, p.DbName);
                _ = output.Append(" VALUES LESS THAN (");

                var t = Type.GetTypeCode(p.Boundary.GetType());
                if (t is TypeCode.String or TypeCode.Char) {
                  TranslateString(output, p.Boundary);
                }
                else {
                  _ = output.Append(p.Boundary);
                }
                _ = output.Append(")");

                if (!string.IsNullOrEmpty(p.Filegroup)) {
                  _ = output.Append(" TABLESPACE " + p.Filegroup);
                }
              }
              break;
          }
          _ = output.Append(")");
        }
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlTranslator"/> class.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected SqlTranslator(SqlDriver driver)
      : base(driver)
    {
      var serverInfo = driver.ServerInfo;
      supportsClusteredIndexes = serverInfo.Index.Features.Supports(IndexFeatures.Clustered);

      var queryFeatures = serverInfo.Query.Features;
      supportsExplicitJoinOrder = queryFeatures.Supports(QueryFeatures.ExplicitJoinOrder);
      supportsMultischemaQueries = queryFeatures.Supports(QueryFeatures.MultischemaQueries);
    }
  }
}
