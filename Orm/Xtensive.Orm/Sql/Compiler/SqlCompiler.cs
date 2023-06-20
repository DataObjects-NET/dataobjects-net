// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
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
  /// Base class for any SQL compiler.
  /// </summary>
  public class SqlCompiler : SqlDriverBound,
    ISqlVisitor
  {
    private static readonly Type SqlPlaceholderType = typeof(SqlPlaceholder);

    private readonly QueryFeatures queryFeatures;

    protected readonly SqlValueType decimalType;
    protected readonly SqlTranslator translator;

    protected SqlCompilerConfiguration configuration;
    protected SqlCompilerContext context;

    /// <summary>
    /// Compiles implementors of <see cref="ISqlCompileUnit"/>.
    /// </summary>
    /// <param name="unit">Instance to compile.</param>
    /// <param name="compilerConfiguration">Configuration for compiler.</param>
    /// <returns></returns>
    public SqlCompilationResult Compile(ISqlCompileUnit unit, SqlCompilerConfiguration compilerConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(unit, "unit");
      configuration = compilerConfiguration;
      context = new SqlCompilerContext(configuration);
      unit.AcceptVisitor(this);
      return new SqlCompilationResult(context.Output.Children, context.ParameterNameProvider.NameTable);
    }

    #region Visitors

    /// <summary>
    /// Visits <see cref="SqlAggregate"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlAggregate node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        node.Expression.AcceptVisitor(this);
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlAlterDomain"/> statements and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlAlterDomain node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        if (node.Action is SqlAddConstraint addConstraint) {
          var constraint = (DomainConstraint) addConstraint.Constraint;
          AppendTranslated(node, AlterDomainSection.AddConstraint);
          AppendTranslated(constraint, ConstraintSection.Entry);
          AppendTranslated(constraint, ConstraintSection.Check);
          constraint.Condition.AcceptVisitor(this);
          AppendTranslated(constraint, ConstraintSection.Exit);
        }
        else if (node.Action is SqlDropConstraint dropConstraint) {
          AppendTranslated(node, AlterDomainSection.DropConstraint);
          AppendTranslated(dropConstraint.Constraint, ConstraintSection.Entry);
        }
        else if (node.Action is SqlSetDefault setDefault) {
          AppendTranslated(node, AlterDomainSection.SetDefault);
          setDefault.DefaultValue.AcceptVisitor(this);
        }
        else if (node.Action is SqlDropDefault) {
          AppendTranslated(node, AlterDomainSection.DropDefault);
        }
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlAlterPartitionFunction"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlAlterPartitionFunction node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlAlterPartitionScheme"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlAlterPartitionScheme node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlFragment"/> node and translates its parts.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public virtual void Visit(SqlFragment node)
    {
      using (context.EnterScope(node))
      using (context.EnterScope(0)) {
        node.Expression.AcceptVisitor(this);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlAlterTable"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlAlterTable node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        if (node.Action is SqlAddColumn addColumn) {
          AppendTranslated(node, AlterTableSection.AddColumn);
          Visit(addColumn.Column);
        }
        else if (node.Action is SqlDropDefault dropDefault) {
          var column = dropDefault.Column;
          AppendTranslated(node, AlterTableSection.AlterColumn);
          AppendTranslated(column, TableColumnSection.Entry);
          AppendTranslated(column, TableColumnSection.DropDefault);
        }
        else if (node.Action is SqlSetDefault setDefault) {
          var column = setDefault.Column;
          AppendTranslated(node, AlterTableSection.AlterColumn);
          AppendTranslated(column, TableColumnSection.Entry);
          AppendTranslated(column, TableColumnSection.SetDefault);
          setDefault.DefaultValue.AcceptVisitor(this);
        }
        else if (node.Action is SqlDropColumn dropColumn) {
          AppendTranslated(node, AlterTableSection.DropColumn);
          AppendTranslated(dropColumn.Column, TableColumnSection.Entry);
          AppendTranslated(node, AlterTableSection.DropBehavior);
        }
        else if (node.Action is SqlAlterIdentityInfo alterIndentityInfo) {
          var action = node.Action as SqlAlterIdentityInfo;
          var column = alterIndentityInfo.Column;
          var descriptor = alterIndentityInfo.SequenceDescriptor;
          AppendTranslated(node, AlterTableSection.AlterColumn);
          AppendTranslated(column, TableColumnSection.Entry);
          if ((alterIndentityInfo.InfoOption & SqlAlterIdentityInfoOptions.RestartWithOption) != 0) {
            AppendTranslated(alterIndentityInfo.SequenceDescriptor, SequenceDescriptorSection.RestartValue);
          }

          if ((action.InfoOption & SqlAlterIdentityInfoOptions.IncrementByOption) != 0) {
            if (descriptor.Increment.HasValue && descriptor.Increment.Value == 0) {
              throw new SqlCompilerException(Strings.ExIncrementMustNotBeZero);
            }

            AppendTranslated(column, TableColumnSection.SetIdentityInfoElement);
            AppendTranslated(descriptor, SequenceDescriptorSection.Increment);
          }
          if ((action.InfoOption & SqlAlterIdentityInfoOptions.MaxValueOption) != 0) {
            AppendTranslated(column, TableColumnSection.SetIdentityInfoElement);
            AppendTranslated(descriptor, SequenceDescriptorSection.AlterMaxValue);
          }
          if ((action.InfoOption & SqlAlterIdentityInfoOptions.MinValueOption) != 0) {
            AppendTranslated(column, TableColumnSection.SetIdentityInfoElement);
            AppendTranslated(descriptor, SequenceDescriptorSection.AlterMinValue);
          }
          if ((action.InfoOption & SqlAlterIdentityInfoOptions.CycleOption) != 0) {
            AppendTranslated(column, TableColumnSection.SetIdentityInfoElement);
            AppendTranslated(descriptor, SequenceDescriptorSection.IsCyclic);
          }
        }
        else if (node.Action is SqlAddConstraint addConstraint) {
          var constraint = addConstraint.Constraint as TableConstraint;
          AppendTranslated(node, AlterTableSection.AddConstraint);
          Visit(constraint);
        }
        else if (node.Action is SqlDropConstraint dropConstraint) {
          var constraint = dropConstraint.Constraint as TableConstraint;
          AppendTranslated(node, AlterTableSection.DropConstraint);
          AppendTranslated(constraint, ConstraintSection.Entry);
          AppendTranslated(node, AlterTableSection.DropBehavior);
        }
        else if (node.Action is SqlRenameColumn renameColumn) {
          AppendTranslated(node, AlterTableSection.RenameColumn);
          AppendTranslated(renameColumn.Column, TableColumnSection.Entry);
          AppendTranslated(node, AlterTableSection.To);
          translator.TranslateIdentifier(context.Output, renameColumn.NewName);
        }
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlAlterSequence"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlAlterSequence node)
    {
      AppendTranslatedEntry(node);
      if ((node.InfoOption & SqlAlterIdentityInfoOptions.RestartWithOption) != 0) {
        AppendTranslated(node.SequenceDescriptor, SequenceDescriptorSection.RestartValue);
      }
      if ((node.InfoOption & SqlAlterIdentityInfoOptions.IncrementByOption) != 0) {
        if (node.SequenceDescriptor.Increment.HasValue && node.SequenceDescriptor.Increment.Value == 0) {
          throw new SqlCompilerException(Strings.ExIncrementMustNotBeZero);
        }
        AppendTranslated(node.SequenceDescriptor, SequenceDescriptorSection.Increment);
      }
      if ((node.InfoOption & SqlAlterIdentityInfoOptions.MaxValueOption) != 0) {
        AppendTranslated(node.SequenceDescriptor, SequenceDescriptorSection.AlterMaxValue);
      }
      if ((node.InfoOption & SqlAlterIdentityInfoOptions.MinValueOption) != 0) {
        AppendTranslated(node.SequenceDescriptor, SequenceDescriptorSection.AlterMinValue);
      }
      if ((node.InfoOption & SqlAlterIdentityInfoOptions.CycleOption) != 0) {
        AppendTranslated(node.SequenceDescriptor, SequenceDescriptorSection.IsCyclic);
      }
      AppendTranslatedExit(node);
    }

    /// <summary>
    /// Visits <see cref="SqlArray"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlArray node)
    {
      var items = node.GetValues();
      if (items.Length == 0) {
        AppendTranslated(node, ArraySection.EmptyArray);
        return;
      }
      AppendTranslated(node, ArraySection.Entry);
      var lengthMinusOne = items.Length - 1;
      if (node.ItemType == SqlPlaceholderType) {
        for (var i = 0; i < lengthMinusOne; i++) {
          Visit((SqlPlaceholder) items[i]);
          AppendRowItemDelimiter();
        }
        Visit((SqlPlaceholder) items[lengthMinusOne]);
      }
      else {
        for (var i = 0; i < lengthMinusOne; i++) {
          AppendTranslatedLiteral(items[i]);
          AppendRowItemDelimiter();
        }
        AppendTranslatedLiteral(items[lengthMinusOne]);
      }
      AppendTranslated(node, ArraySection.Exit);
    }

    /// <summary>
    /// Visits <see cref="SqlAssignment"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlAssignment node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Left.AcceptVisitor(this);
        AppendTranslated(node.NodeType);
        node.Right.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlBatch"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlBatch node)
    {
      using (context.EnterScope(node)) {
        var statements = FlattenBatch(node).ToArray();
        if (statements.Length == 0) {
          return;
        }
        if (statements.Length == 1) {
          statements[0].AcceptVisitor(this);
          return;
        }
        _ = context.Output.Append(translator.BatchBegin);
        using (context.EnterCollectionScope()) {
          foreach (var item in statements) {
            item.AcceptVisitor(this);
            AppendBatchDelimiter();
            _ = context.Output.AppendNewLine(translator.NewLine);
          }
        }
        _ = context.Output.Append(translator.BatchEnd);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlBetween"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlBetween node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        node.Expression.AcceptVisitor(this);
        AppendTranslated(node, BetweenSection.Between);
        node.Left.AcceptVisitor(this);
        AppendTranslated(node, BetweenSection.And);
        node.Right.AcceptVisitor(this);
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlBinary"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlBinary node)
    {
      if (node.NodeType is SqlNodeType.In or SqlNodeType.NotIn) {
        var row = node.Right as SqlRow;
        var array = node.Right as SqlArray;
        var isEmptyIn = (row is not null && row.Count == 0) || (array is not null && array.Length == 0);
        if (isEmptyIn) {
          SqlDml.Literal(node.NodeType == SqlNodeType.NotIn).AcceptVisitor(this);
          return;
        }
      }

      if (node.NodeType is SqlNodeType.Or or SqlNodeType.And) {
        var expressions = RecursiveBinaryLogicExtractor.Extract(node);
        using (context.EnterScope(node)) {
          AppendTranslatedEntry(node);
          expressions[0].AcceptVisitor(this);
          foreach (var operand in expressions.Skip(1)) {
            AppendTranslated(node.NodeType);
            operand.AcceptVisitor(this);
          }
          AppendTranslatedExit(node);
        }
        return;
      }

      if (node.NodeType == SqlNodeType.RawConcat) {
        AppendSpace();
        translator.Translate(context, node, NodeSection.Entry);
        node.Left.AcceptVisitor(this);
        AppendSpace();
        translator.Translate(context.Output, node.NodeType);
        node.Right.AcceptVisitor(this);
        translator.Translate(context, node, NodeSection.Exit);

        return;
      }

      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        node.Left.AcceptVisitor(this);
        AppendTranslated(node.NodeType);
        node.Right.AcceptVisitor(this);
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlBreak"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlBreak node) => AppendTranslated(node);

    /// <summary>
    /// Visits <see cref="SqlCase"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlCase node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);

        if (node.Value is not null) {
          AppendTranslated(node, CaseSection.Value);
          node.Value.AcceptVisitor(this);
        }

        using (context.EnterCollectionScope()) {
          foreach (KeyValuePair<SqlExpression, SqlExpression> item in node) {
            AppendCollectionDelimiterIfNecessary(() => AppendDelimiter(translator.WhenDelimiter));
            AppendTranslated(node, item.Key, CaseSection.When);
            item.Key.AcceptVisitor(this);
            AppendTranslated(node, item.Value, CaseSection.Then);
            item.Value.AcceptVisitor(this);
          }
        }

        if (node.Else is not null) {
          AppendTranslated(node, CaseSection.Else);
          node.Else.AcceptVisitor(this);
        }

        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlCast"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlCast node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Operand.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlCloseCursor"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCloseCursor node) => AppendTranslated(node);

    /// <summary>
    /// Visits <see cref="SqlCollate"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlCollate node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Operand.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlColumnRef"/> and translates its parts.
    /// </summary>
    /// <param name="node">Column reference to visit.</param>
    public virtual void Visit(SqlColumnRef node) => AppendTranslated(node, ColumnSection.Entry);

    /// <summary>
    /// Visits <see cref="SqlContainsTable"/> table and translates its parts.
    /// </summary>
    /// <param name="node">Table to visit.</param>
    public virtual void Visit(SqlContainsTable node) =>
      throw SqlHelper.NotSupported(Strings.FullTextQueries);

    /// <summary>
    /// Visits <see cref="SqlNative"/> expression and translates it.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlNative node) => AppendTranslated(node);

    /// <summary>
    /// Visits <see cref="SqlNativeHint"/> node and translates its parts.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public virtual void Visit(SqlNativeHint node)
    {
      // nothing
    }

    /// <summary>
    /// Visits <see cref="SqlConcat"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlConcat node)
    {
      AppendTranslatedEntry(node);
      var first = true;
      foreach (var item in node) {
        if (!first) {
          AppendTranslated(node.NodeType);
        }
        item.AcceptVisitor(this);
        first = false;
      }
      AppendTranslatedExit(node);
    }

    /// <summary>
    /// Visits <see cref="SqlContainer"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlContainer node) =>
      throw new SqlCompilerException(Strings.ExSqlContainerExpressionCanNotBeCompiled);

    /// <summary>
    /// Visits <see cref="SqlContinue"/> statement and translates it.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlContinue node) => AppendTranslated(node);

    /// <summary>
    /// Visits <see cref="SqlCommand"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCommand node) => AppendTranslated(node);

    /// <summary>
    /// Visits <see cref="SqlCreateAssertion"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreateAssertion node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        node.Assertion.Condition.AcceptVisitor(this);
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlCreateCharacterSet"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreateCharacterSet node)
    {
      //      ArgumentValidator.EnsureArgumentNotNull(node.CharacterSet.CharacterSetSource, "CharacterSetSource");
      //      AppendTranslated(node);
    }

    /// <summary>
    /// Visits <see cref="SqlCreateCollation"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreateCollation node)
    {
      //      ArgumentValidator.EnsureArgumentNotNull(node.Collation.CharacterSet, "CharacterSet");
      //      AppendTranslated(node);
    }

    /// <summary>
    /// Visits <see cref="SqlCreateDomain"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreateDomain node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.Domain.DataType, "DataType");

      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        if (node.Domain.DefaultValue is not null) {
          AppendTranslated(node, CreateDomainSection.DomainDefaultValue);
          node.Domain.DefaultValue.AcceptVisitor(this);
        }
        if (node.Domain.DomainConstraints.Count != 0) {
          using (context.EnterCollectionScope()) {
            foreach (var constraint in node.Domain.DomainConstraints) {
              AppendTranslated(constraint, ConstraintSection.Entry);
              AppendTranslated(constraint, ConstraintSection.Check);
              constraint.Condition.AcceptVisitor(this);
              AppendTranslated(constraint, ConstraintSection.Exit);
            }
          }
        }
        if (node.Domain.Collation != null) {
          AppendTranslated(node, CreateDomainSection.DomainCollate);
        }
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlCreateIndex"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreateIndex node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.Index.DataTable, "DataTable");

      AppendTranslatedEntry(node);
      if (node.Index.Columns.Count > 0) {
        AppendSpaceIfNecessary();
        translator.Translate(context, node, CreateIndexSection.ColumnsEnter);
        using (context.EnterCollectionScope()) {
          foreach (var item in node.Index.Columns) {
            AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
            Visit(node, item);
          }
        }
        translator.Translate(context, node, CreateIndexSection.ColumnsExit);
        AppendSpaceIfNecessary();
      }
      if (node.Index.NonkeyColumns != null && node.Index.NonkeyColumns.Count != 0 && CheckFeature(IndexFeatures.NonKeyColumns)) {
        translator.Translate(context, node, CreateIndexSection.NonkeyColumnsEnter);
        AppendSpaceIfNecessary();
        using (context.EnterCollectionScope()) {
          foreach (var item in node.Index.NonkeyColumns) {
            AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
            translator.TranslateIdentifier(context.Output, item.Name);
          }
        }
        translator.Translate(context, node, CreateIndexSection.NonkeyColumnsExit);
      }

      AppendTranslated(node, CreateIndexSection.StorageOptions);

      if (node.Index.Where is not null && CheckFeature(IndexFeatures.Filtered)) {
        AppendSpaceIfNecessary();
        translator.Translate(context, node, CreateIndexSection.Where);
        AppendSpace();
        using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableQualifiedColumns)) {
          node.Index.Where.AcceptVisitor(this);
        }
      }
      AppendTranslatedExit(node);
    }

    /// <summary>
    /// Visits <see cref="IndexColumn"/> and translates it.
    /// </summary>
    /// <param name="node">The <see cref="SqlCreateIndex"/> the <paramref name="item"/> belongs to</param>
    /// <param name="item">Column to visit.</param>
    public virtual void Visit(SqlCreateIndex node, IndexColumn item)
    {
      if (item.Expression is not null && CheckFeature(IndexFeatures.Expressions)) {
        using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableQualifiedColumns)) {
          item.Expression.AcceptVisitor(this);
        }
      }
      else {
        translator.TranslateIdentifier(context.Output, item.Column.Name);
        if (!(node.Index.IsFullText || node.Index.IsSpatial) && CheckFeature(IndexFeatures.SortOrder)) {
          AppendSpaceIfNecessary();
          translator.TranslateSortOrder(context.Output, item.Ascending);
        }
      }
    }

    /// <summary>
    /// Visits <see cref="SqlCreatePartitionFunction"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreatePartitionFunction node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.PartitionFunction.DataType, "DataType");
      translator.Translate(context, node);
    }

    /// <summary>
    /// Visits <see cref="SqlCreatePartitionScheme"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreatePartitionScheme node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.PartitionSchema.PartitionFunction, "PartitionFunction");
      translator.Translate(context, node);
    }

    /// <summary>
    /// Visits <see cref="SqlCreateSchema"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreateSchema node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        AppendDdlStatementDelimiter();
        if (node.Schema.Assertions.Count > 0) {
          using (context.EnterCollectionScope()) {
            foreach (var assertion in node.Schema.Assertions) {
              new SqlCreateAssertion(assertion).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }
          }
        }

        if (node.Schema.CharacterSets.Count > 0) {
          using (context.EnterCollectionScope()) {
            foreach (var characterSet in node.Schema.CharacterSets) {
              new SqlCreateCharacterSet(characterSet).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }
          }
        }

        if (node.Schema.Collations.Count > 0) {
          using (context.EnterCollectionScope()) {
            foreach (var collation in node.Schema.Collations) {
              AppendCollectionDelimiterIfNecessary(AppendDdlStatementDelimiter);
              new SqlCreateCollation(collation).AcceptVisitor(this);
            }
          }
        }

        if (node.Schema.Domains.Count > 0) {
          using (context.EnterCollectionScope()) {
            foreach (var domain in node.Schema.Domains) {
              new SqlCreateDomain(domain).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }
          }
        }

        if (node.Schema.Sequences.Count > 0) {
          using (context.EnterCollectionScope()) {
            foreach (var sequence in node.Schema.Sequences) {
              new SqlCreateSequence(sequence).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }
          }
        }

        if (node.Schema.Tables.Count > 0) {
          using (context.EnterCollectionScope()) {
            foreach (var table in node.Schema.Tables) {
              new SqlCreateTable(table).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
              if (table.Indexes.Count > 0) {
                using (context.EnterCollectionScope()) {
                  foreach (var index in table.Indexes) {
                    new SqlCreateIndex(index).AcceptVisitor(this);
                    AppendDdlStatementDelimiter();
                  }
                }
              }
            }
          }
        }

        if (node.Schema.Translations.Count > 0) {
          using (context.EnterCollectionScope()) {
            foreach (var translation in node.Schema.Translations) {
              new SqlCreateTranslation(translation).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }
          }
        }

        if (node.Schema.Views.Count > 0) {
          using (context.EnterCollectionScope()) {
            foreach (var view in node.Schema.Views) {
              new SqlCreateView(view).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }
          }
        }

        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlCreateSequence"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreateSequence node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.Sequence.SequenceDescriptor, "SequenceDescriptor");

      if (node.Sequence.SequenceDescriptor.Increment.HasValue && node.Sequence.SequenceDescriptor.Increment.Value == 0) {
        throw new SqlCompilerException(Strings.ExIncrementMustNotBeZero);
      }
      if (string.IsNullOrEmpty(node.Sequence.Name)) {
        throw new SqlCompilerException(Strings.ExNameMustBeNotNullOrEmpty);
      }
      if (node.Sequence.Schema == null) {
        throw new SqlCompilerException(Strings.ExSchemaMustBeNotNull);
      }
      if (node.Sequence.SequenceDescriptor.MaxValue.HasValue &&
          node.Sequence.SequenceDescriptor.MinValue.HasValue &&
          node.Sequence.SequenceDescriptor.MaxValue.Value <= node.Sequence.SequenceDescriptor.MinValue.Value) {
        throw new SqlCompilerException(Strings.ExTheMaximumValueMustBeGreaterThanTheMinimumValue);
      }
      if (node.Sequence.SequenceDescriptor.StartValue.HasValue &&
          (node.Sequence.SequenceDescriptor.MaxValue.HasValue &&
           node.Sequence.SequenceDescriptor.MaxValue.Value < node.Sequence.SequenceDescriptor.StartValue.Value ||
           node.Sequence.SequenceDescriptor.MinValue.HasValue &&
           node.Sequence.SequenceDescriptor.MinValue.Value > node.Sequence.SequenceDescriptor.StartValue.Value)) {
        throw new SqlCompilerException(Strings.ExTheStartValueShouldBeBetweenTheMinimumAndMaximumValue);
      }

      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        AppendTranslated(node.Sequence.SequenceDescriptor, SequenceDescriptorSection.StartValue);
        AppendTranslated(node.Sequence.SequenceDescriptor, SequenceDescriptorSection.Increment);
        AppendTranslated(node.Sequence.SequenceDescriptor, SequenceDescriptorSection.MaxValue);
        AppendTranslated(node.Sequence.SequenceDescriptor, SequenceDescriptorSection.MinValue);
        AppendTranslated(node.Sequence.SequenceDescriptor, SequenceDescriptorSection.IsCyclic);
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlCreateTable"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreateTable node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        AppendTranslated(node, CreateTableSection.TableElementsEntry);

        // ReSharper disable ConditionIsAlwaysTrueOrFalse
        // ReSharper disable RedundantAssignment

        var hasItems = false;

        if (node.Table.Columns.Count > 0) {
          hasItems = VisitCreateTableColumns(node, node.Table.TableColumns, hasItems);
        }

        if (node.Table.TableConstraints.Count > 0) {
          _ = VisitCreateTableConstraints(node, node.Table.TableConstraints, hasItems);
        }

        // ReSharper restore ConditionIsAlwaysTrueOrFalse
        // ReSharper restore RedundantAssignment

        AppendTranslated(node, CreateTableSection.TableElementsExit);
        if (node.Table.PartitionDescriptor != null) {
          AppendDelimiter(translator.ColumnDelimiter, SqlDelimiterType.Column);
          AppendTranslated(node, CreateTableSection.Partition);
        }
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits list of <see cref="TableConstraint"/>s translates parts of them.
    /// </summary>
    /// <param name="node">The <see cref="SqlCreateTable"/> constrains belong to</param>
    /// <param name="constraints">List of constraints.</param>
    /// <param name="hasItems">Flag indicating that constraint list should start with <see cref="SqlTranslator.ColumnDelimiter"/></param>
    /// <returns>Flag that tells whether there were constraints.</returns>
    protected virtual bool VisitCreateTableConstraints(SqlCreateTable node, IEnumerable<TableConstraint> constraints, bool hasItems)
    {
      using (context.EnterCollectionScope()) {
        foreach (var constraint in constraints) {
          if (hasItems) {
            AppendDelimiter(translator.ColumnDelimiter, SqlDelimiterType.Column);
          }
          else {
            hasItems = true;
          }
          Visit(constraint);
        }
      }
      return hasItems;
    }

    /// <summary>
    /// Visits list of <see cref="SqlTableColumn"/>s statement and translates parts of them.
    /// </summary>
    /// <param name="node">The <see cref="SqlCreateTable"/> the columns belong to.</param>
    /// <param name="columns">List of columns.</param>
    /// <param name="hasItems">Flag indicating that constraint list should start with <see cref="SqlTranslator.ColumnDelimiter"/></param>
    /// <returns>Flag that tells whether there were columns.</returns>
    protected virtual bool VisitCreateTableColumns(SqlCreateTable node, IEnumerable<TableColumn> columns, bool hasItems)
    {
      using (context.EnterCollectionScope()) {
        foreach (var column in columns) {
          // Skipping computed columns
          if (column.Expression is not null && !CheckFeature(ColumnFeatures.Computed)) {
            continue;
          }
          if (hasItems) {
            AppendDelimiter(translator.ColumnDelimiter, SqlDelimiterType.Column);
          }
          else {
            hasItems = true;
          }
          Visit(column);
        }
      }

      return hasItems;
    }

    /// <summary>
    /// Visits <see cref="SqlCreateTranslation"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreateTranslation node)
    {
      //      ArgumentValidator.EnsureArgumentNotNull(node.Translation.SourceCharacterSet, "SourceCharacterSet");
      //      ArgumentValidator.EnsureArgumentNotNull(node.Translation.TargetCharacterSet, "TargetCharacterSet");
      //      ArgumentValidator.EnsureArgumentNotNull(node.Translation.TranslationSource, "TranslationSource");
      //      AppendTranslated(node);
    }

    /// <summary>
    /// Visits <see cref="SqlCreateView"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlCreateView node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        if (node.View.Definition is not null) {
          node.View.Definition.AcceptVisitor(this);
        }
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlCursor"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlCursor node) => AppendTranslated(node);

    /// <summary>
    /// Visits <see cref="SqlDeclareCursor"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDeclareCursor node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        AppendTranslated(node, DeclareCursorSection.Sensivity);
        AppendTranslated(node, DeclareCursorSection.Scrollability);
        AppendTranslated(node, DeclareCursorSection.Cursor);
        AppendTranslated(node, DeclareCursorSection.Holdability);
        AppendTranslated(node, DeclareCursorSection.Returnability);
        AppendTranslated(node, DeclareCursorSection.For);

        node.Cursor.Query.AcceptVisitor(this);
        if (node.Cursor.Columns.Count != 0) {
          foreach (SqlColumnRef item in node.Cursor.Columns) {
            item.AcceptVisitor(this);
          }
        }
        AppendTranslated(node, DeclareCursorSection.Updatability);
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlDeclareVariable"/> statement and translates it.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDeclareVariable node) => AppendTranslated(node);

    /// <summary>
    /// Visits <see cref="SqlDefaultValue"/> expression and translates it.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlDefaultValue node) => AppendTranslated(node);

    /// <summary>
    /// Visits <see cref="SqlDelete"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDelete node) => VisitDeleteDefault(node);

    /// <summary>
    /// Default visitor for <see cref="SqlDelete"/> statement parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected void VisitDeleteDefault(SqlDelete node)
    {
      using (context.EnterScope(node)) {
        VisitDeleteEntry(node);
        VisitDeleteDelete(node);
        VisitDeleteFrom(node);
        VisitDeleteWhere(node);
        VisitDeleteLimit(node);
        VisitDeleteExit(node);
      }
    }

    /// <summary>
    /// Visits entry part of <see cref="SqlDelete"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitDeleteEntry(SqlDelete node) => AppendTranslatedEntry(node);

    /// <summary>
    /// Visits DELETE part of <see cref="SqlDelete"/> statement.
    /// </summary>
    /// <param name="node">Stetement to visit.</param>
    protected virtual void VisitDeleteDelete(SqlDelete node)
    {
      if (node.Delete == null) {
        throw new SqlCompilerException(Strings.ExTablePropertyIsNotSet);
      }

      using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableAliasing)) {
        node.Delete.AcceptVisitor(this);
      }
    }

    /// <summary>
    /// Visits FROM part of <see cref="SqlDelete"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitDeleteFrom(SqlDelete node)
    {
      if (CheckFeature(QueryFeatures.DeleteFrom) && node.From != null) {
        AppendTranslated(node, DeleteSection.From);
        node.From.AcceptVisitor(this);
      }
    }

    /// <summary>
    /// Visits WHERE part of <see cref="SqlDelete"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitDeleteWhere(SqlDelete node)
    {
      if (node.Where is not null) {
        AppendTranslated(node, DeleteSection.Where);
        node.Where.AcceptVisitor(this);
      }
    }

    /// <summary>
    /// Visits LIMIT part of <see cref="SqlDelete"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitDeleteLimit(SqlDelete node)
    {
      if (node.Limit is not null) {
        if (!CheckFeature(QueryFeatures.DeleteLimit)) {
          throw new NotSupportedException(Strings.ExStorageIsNotSupportedLimitationOfRowCountToDelete);
        }

        AppendTranslated(node, DeleteSection.Limit);
        node.Limit.AcceptVisitor(this);
      }
    }

    /// <summary>
    /// Visits exit part of <see cref="SqlDelete"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitDeleteExit(SqlDelete node) => AppendTranslatedExit(node);

    /// <summary>
    /// Visits <see cref="SqlDropAssertion"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropAssertion node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlDropCharacterSet"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropCharacterSet node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlDropCollation"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropCollation node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlDropDomain"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropDomain node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlDropIndex"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropIndex node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.Index.DataTable, "DataTable");
      translator.Translate(context, node);
    }

    /// <summary>
    /// Visits <see cref="SqlDropPartitionFunction"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropPartitionFunction node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlDropPartitionScheme"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropPartitionScheme node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlDropSchema"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropSchema node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlDropSequence"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropSequence node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlDropTable"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropTable node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlDropTranslation"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropTranslation node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlDropView"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlDropView node) => translator.Translate(context, node);

    public virtual void Visit(SqlTruncateTable node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlFastFirstRowsHint"/> node and translates its parts.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public virtual void Visit(SqlFastFirstRowsHint node)
    {
      // nothing
    }

    /// <summary>
    /// Visits <see cref="SqlDynamicFilter"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlDynamicFilter node)
    {
      switch (node.Expressions.Count) {
        case 0:
          SqlDml.Literal(true).AcceptVisitor(this);
          break;
        case 1:
          TranslateDynamicFilterViaInOperator(node);
          break;
        default:
          if (CheckFeature(QueryFeatures.MulticolumnIn)) {
            TranslateDynamicFilterViaInOperator(node);
          }
          else {
            TranslateDynamicFilterViaMultipleComparisons(node);
          }
          break;
      }
    }

    /// <summary>
    /// Visits <see cref="SqlFetch"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlFetch node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        if (node.RowCount is not null) {
          node.RowCount.AcceptVisitor(this);
        }
        AppendTranslated(node, FetchSection.Targets);
        foreach (var item in node.Targets) {
          item.AcceptVisitor(this);
        }
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlForceJoinOrderHint"/> node and translates its parts.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public virtual void Visit(SqlForceJoinOrderHint node)
    {
      // nothing
    }

    /// <summary>
    /// Visits <see cref="SqlFreeTextTable"/> table and translates it.
    /// </summary>
    /// <param name="node">Table to visit.</param>
    public virtual void Visit(SqlFreeTextTable node) => throw SqlHelper.NotSupported(Strings.FullTextQueries);

    /// <summary>
    /// Visits <see cref="SqlFunctionCall"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlFunctionCall node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        
        if (node.Arguments.Count > 0) {
          using (context.EnterCollectionScope()) {
            var argumentPosition = 0;
            foreach (var item in node.Arguments) {
              AppendCollectionDelimiterIfNecessary(() => AppendTranslated(node, FunctionCallSection.ArgumentDelimiter, argumentPosition));
              AppendSpaceIfNecessary();
              translator.Translate(context, node, FunctionCallSection.ArgumentEntry, argumentPosition);
              AppendSpaceIfNecessary();
              item.AcceptVisitor(this);
              AppendSpaceIfNecessary();
              translator.Translate(context, node, FunctionCallSection.ArgumentExit, argumentPosition);
              argumentPosition++;
            }
          }
        }
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlCustomFunctionCall"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlCustomFunctionCall node) =>
      throw new NotSupportedException(string.Format(Strings.ExFunctionXIsNotSupported, node.FunctionType));

    /// <summary>
    /// Visits <see cref="SqlIf"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlIf node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, IfSection.Entry);

        node.Condition.AcceptVisitor(this);

        AppendTranslated(node, IfSection.True);
        node.True.AcceptVisitor(this);

        if (node.False != null) {
          AppendTranslated(node, IfSection.False);
          node.False.AcceptVisitor(this);
        }

        AppendTranslated(node, IfSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlInsert"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlInsert node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);

        if (node.Into == null) {
          throw new SqlCompilerException(Strings.ExTablePropertyIsNotSet);
        }

        using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableAliasing)) {
          node.Into.AcceptVisitor(this);
        }

        AppendTranslated(node, InsertSection.ColumnsEntry);
        var columns = node.ValueRows.Columns;
        if (columns.Count > 0) {
          using (context.EnterCollectionScope()) {
            foreach (var item in columns) {
              AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
              translator.TranslateIdentifier(context.Output, item.Name);
            }
          }
        }

        AppendTranslated(node, InsertSection.ColumnsExit);
        if (node.ValueRows.Count == 0 && node.From == null) {
          AppendTranslated(node, InsertSection.DefaultValues);
        }
        else {
          if (node.From != null) {
            using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableAliasing)) {
              node.From.AcceptVisitor(this);
            }
          }
          else {
            AppendTranslated(node, InsertSection.ValuesEntry);
            bool firstRow = true;
            foreach (var row in node.ValueRows) {
              if (!firstRow) {
                _ = context.Output.Append(translator.ColumnDelimiter);
                AppendSpaceIfNecessary();
              }
              firstRow = false;
              row.AcceptVisitor(this);
            }
            AppendTranslated(node, InsertSection.ValuesExit);
          }
        }
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlJoinExpression"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlJoinExpression node)
    {
      var leftIsJoin = node.Left is SqlJoinedTable;
      if (leftIsJoin) {
        node.Left.AcceptVisitor(this);
      }
      using (context.EnterScope(node)) {
        AppendTranslated(node, JoinSection.Entry);
        if (!leftIsJoin) {
          node.Left.AcceptVisitor(this);
        }
        AppendTranslated(node, JoinSection.Specification);
        node.Right.AcceptVisitor(this);
        if (node.Expression is not null) {
          AppendTranslated(node, JoinSection.Condition);
          node.Expression.AcceptVisitor(this);
        }
        AppendTranslated(node, JoinSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlJoinHint"/> node and translates its parts.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public virtual void Visit(SqlJoinHint node)
    {
      // nothing
    }

    /// <summary>
    /// Visits <see cref="SqlLike"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlLike node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        node.Expression.AcceptVisitor(this);
        AppendTranslated(node, LikeSection.Like);
        node.Pattern.AcceptVisitor(this);
        if (node.Escape is not null) {
          AppendTranslated(node, LikeSection.Escape);
          node.Escape.AcceptVisitor(this);
        }
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlLiteral"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlLiteral node) => AppendTranslatedLiteral(node.GetValue());

    /// <summary>
    /// Visits <see cref="SqlMatch"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlMatch node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, MatchSection.Entry);
        node.Value.AcceptVisitor(this);
        AppendTranslated(node, MatchSection.Specification);
        node.SubQuery.AcceptVisitor(this);
        AppendTranslated(node, MatchSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlNull"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlNull node) => AppendTranslated(node);

    /// <summary>
    /// Visits <see cref="SqlNextValue"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlNextValue node)
    {
      AppendTranslated(node, NodeSection.Entry);
      AppendTranslated(node.Sequence);
      AppendTranslated(node, NodeSection.Exit);
    }

    /// <summary>
    /// Visits <see cref="SqlOpenCursor"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlOpenCursor node) => AppendTranslated(node);

    /// <summary>
    /// Visits <see cref="SqlOrder"/> node and translates its parts.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public virtual void Visit(SqlOrder node)
    {
      using (context.EnterScope(node)) {
        AppendSpaceIfNecessary();
        translator.Translate(context, node, NodeSection.Entry);
        if (node.Expression is not null) {
          node.Expression.AcceptVisitor(this);
        }
        else if (node.Position > 0) {
          _ = context.Output.Append(node.Position.ToString());
        }
        AppendSpace();
        translator.Translate(context, node, NodeSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlParameterRef"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlParameterRef node)
    {
      var name = string.IsNullOrEmpty(node.Name)
        ? context.ParameterNameProvider.GetName(node.Parameter)
        : node.Name;
      _ = context.Output.Append(translator.ParameterPrefix + name);
    }

    /// <summary>
    /// Visits <see cref="SqlQueryRef"/> node and translates its parts.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public virtual void Visit(SqlQueryRef node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, TableSection.Entry);
        node.Query.AcceptVisitor(this);
        translator.Translate(context, node, TableSection.Exit);
        AppendTranslated(node, TableSection.AliasDeclaration);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlQueryExpression"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlQueryExpression node)
    {
      var leftIsQueryExpression = node.Left is SqlQueryExpression;
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        if (!leftIsQueryExpression) {
          _ = context.Output.AppendOpeningPunctuation("(");
        }

        node.Left.AcceptVisitor(this);
        if (!leftIsQueryExpression) {
          _ = context.Output.Append(")");
        }
        AppendTranslated(node.NodeType);
        AppendTranslated(node, QueryExpressionSection.All);
        _ = context.Output.AppendOpeningPunctuation("(");
        node.Right.AcceptVisitor(this);
        _ = context.Output.Append(")");
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlRow"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlRow node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        using (context.EnterCollectionScope()) {
          foreach (var item in node) {
            AppendCollectionDelimiterIfNecessary(AppendRowItemDelimiter);
            item.AcceptVisitor(this);
          }
        }
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlRowNumber"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlRowNumber node)
    {
      if (!CheckFeature(QueryFeatures.RowNumber)) {
        throw SqlHelper.NotSupported(QueryFeatures.RowNumber);
      }

      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        using (context.EnterCollectionScope()) {
          foreach (var item in node.OrderBy) {
            AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
            item.AcceptVisitor(this);
          }
        }
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlRenameTable"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlRenameTable node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlRound"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlRound node)
    {
      var result = node.Mode switch {
        MidpointRounding.ToEven => node.Length is null
          ? SqlHelper.BankersRound(node.Argument)
          : SqlHelper.BankersRound(node.Argument, node.Length),
        MidpointRounding.AwayFromZero => node.Length is null
          ? SqlHelper.RegularRound(node.Argument)
          : SqlHelper.RegularRound(node.Argument, node.Length),
        _ => throw new ArgumentOutOfRangeException(),
      };
      if (node.Type == TypeCode.Decimal) {
        result = SqlDml.Cast(result, decimalType);
      }
      result.AcceptVisitor(this);
    }

    /// <summary>
    /// Visits <see cref="SqlSelect"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to translate</param>
    public virtual void Visit(SqlSelect node) => VisitSelectDefault(node);

    /// <summary>
    /// Default visitor for <see cref="SqlSelect"/>.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected void VisitSelectDefault(SqlSelect node)
    {
      using (context.EnterScope(node)) {
        var comment = node.Comment;
        VisitCommentIfBefore(comment);
        AppendTranslatedEntry(node);
        VisitCommentIfWithin(comment);
        VisitSelectHints(node);
        VisitSelectColumns(node);
        VisitSelectFrom(node);
        VisitSelectWhere(node);
        VisitSelectGroupBy(node);
        VisitSelectOrderBy(node);
        VisitSelectLimitOffset(node);
        VisitSelectLock(node);
        AppendTranslatedExit(node);
        VisitCommentIfAfter(comment);
      }
    }

    /// <summary>
    /// Visits HINTS part of <see cref="SqlSelect"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitSelectHints(SqlSelect node)
    {
      var hints = node.Hints;
      if (hints.Count == 0) {
        return;
      }

      using (context.EnterCollectionScope()) {
        AppendTranslated(node, SelectSection.HintsEntry);
        hints[0].AcceptVisitor(this);
        for (var i = 1; i < hints.Count; i++) {
          AppendDelimiter(translator.HintDelimiter);
          hints[i].AcceptVisitor(this);
        }
        AppendTranslated(node, SelectSection.HintsExit);
      }
    }

    /// <summary>
    /// Visits COLUMNS part of <see cref="SqlSelect"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitSelectColumns(SqlSelect node)
    {
      if (node.Columns.Count == 0) {
        AppendSpaceIfNecessary();
        Visit(SqlDml.Asterisk);
        AppendSpaceIfNecessary();
        return;
      }
      using (context.EnterCollectionScope()) {
        foreach (var item in node.Columns) {
          if (item is SqlColumnStub) {
            continue;
          }

          var cr = item as SqlColumnRef;
          if (cr is not null && cr.SqlColumn is SqlColumnStub) {
            continue;
          }

          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          if (cr is not null) {
            AppendSpaceIfNecessary();
            cr.SqlColumn.AcceptVisitor(this);
            translator.Translate(context, cr, ColumnSection.AliasDeclaration);
          }
          else {
            AppendSpaceIfNecessary();
            item.AcceptVisitor(this);
          }
        }
      }
    }

    /// <summary>
    /// Visits FROM part of <see cref="SqlSelect"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitSelectFrom(SqlSelect node)
    {
      if (node.From == null) {
        return;
      }

      AppendSpace();
      translator.Translate(context, node, SelectSection.From);
      AppendSpace();

      var joinedFrom = node.From as SqlJoinedTable;
      var linearJoinRequired = CheckFeature(QueryFeatures.StrictJoinSyntax) && joinedFrom != null;

      if (!linearJoinRequired) {
        node.From.AcceptVisitor(this);
        return;
      }

      var joinSequence = JoinSequence.Build(joinedFrom);
      var previous = joinSequence.Pivot;
      previous.AcceptVisitor(this);

      for (var i = 0; i < joinSequence.Tables.Count; i++) {
        var table = joinSequence.Tables[i];
        var type = joinSequence.JoinTypes[i];
        var condition = joinSequence.Conditions[i];
        var join = new SqlJoinExpression(type, previous, table, condition);

        AppendTranslated(join, JoinSection.Specification);
        table.AcceptVisitor(this);
        if (condition is not null) {
          AppendTranslated(join, JoinSection.Condition);
          condition.AcceptVisitor(this);
        }

        previous = table;
      }
    }

    /// <summary>
    /// Visits WHERE part of <see cref="SqlSelect"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitSelectWhere(SqlSelect node)
    {
      if (node.Where is null) {
        return;
      }

      AppendTranslated(node, SelectSection.Where);
      node.Where.AcceptVisitor(this);
    }

    /// <summary>
    /// Visits GROUP BY part of <see cref="SqlSelect"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitSelectGroupBy(SqlSelect node)
    {
      if (node.GroupBy.Count <= 0) {
        return;
      }
      // group by
      AppendSpace();
      translator.Translate(context, node, SelectSection.GroupBy);
      AppendSpace();
      using (context.EnterCollectionScope()) {
        foreach (var item in node.GroupBy) {
          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          var cr = item as SqlColumnRef;
          if (cr is not null) {
            cr.SqlColumn.AcceptVisitor(this);
          }
          else {
            item.AcceptVisitor(this);
          }
        }
      }
      if (node.Having is null) {
        return;
      }
      // having
      AppendTranslated(node, SelectSection.Having);
      node.Having.AcceptVisitor(this);
    }

    /// <summary>
    /// Visits ORDER BY part of <see cref="SqlSelect"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitSelectOrderBy(SqlSelect node)
    {
      if (node.OrderBy.Count <= 0) {
        return;
      }

      AppendSpace();
      translator.Translate(context, node, SelectSection.OrderBy);
      AppendSpace();
      using (context.EnterCollectionScope()) {
        foreach (var item in node.OrderBy) {
          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          item.AcceptVisitor(this);
        }
      }
    }

    /// <summary>
    /// Visits limits part of <see cref="SqlSelect"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitSelectLimitOffset(SqlSelect node)
    {
      if (node.Limit is not null) {
        AppendTranslated(node, SelectSection.Limit);
        node.Limit.AcceptVisitor(this);
        AppendTranslated(node, SelectSection.LimitEnd);
      }
      if (node.Offset is not null) {
        AppendTranslated(node, SelectSection.Offset);
        node.Offset.AcceptVisitor(this);
        AppendTranslated(node, SelectSection.OffsetEnd);
      }
    }

    /// <summary>
    /// Visits LOCK part of <see cref="SqlSelect"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitSelectLock(SqlSelect node)
    {
      if (node.Lock != SqlLockType.Empty) {
        translator.Translate(context.Output, node.Lock);
        AppendSpaceIfNecessary();
      }
    }

    /// <summary>
    /// Visits <see cref="SqlStatementBlock"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlStatementBlock node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        using (context.EnterCollectionScope()) {
          foreach (SqlStatement item in node) {
            item.AcceptVisitor(this);
            AppendBatchDelimiter();
          }
        }
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlSubQuery"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlSubQuery node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Query.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlTableColumn"/> node and translates its parts.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public virtual void Visit(SqlTableColumn node) => translator.Translate(context, node, NodeSection.Entry);

    /// <summary>
    /// Visits <see cref="SqlTableRef"/> node and translates its parts.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public virtual void Visit(SqlTableRef node)
    {
      AppendTranslated(node, TableSection.Entry);
      AppendTranslated(node, TableSection.AliasDeclaration);
    }

    /// <summary>
    /// Visits <see cref="SqlTrim"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlTrim node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, TrimSection.Entry);
        translator.Translate(context.Output, node.TrimType);
        AppendSpaceIfNecessary();
        if (node.TrimCharacters != null) {
          AppendTranslatedLiteral(node.TrimCharacters);
        }
        AppendTranslated(node, TrimSection.From);
        node.Expression.AcceptVisitor(this);
        AppendTranslated(node, TrimSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlUnary"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlUnary node)
    {
      using (context.EnterScope(node)) {
        AppendSpaceIfNecessary();
        AppendTranslated(node, NodeSection.Entry);
        node.Operand.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlMetadata"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlMetadata node) => node.Expression.AcceptVisitor(this);

    /// <summary>
    /// Visits <see cref="SqlUpdate"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlUpdate node) => VisitUpdateDefault(node);

    /// <summary>
    /// Default visitor for <see cref="SqlUpdate"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected void VisitUpdateDefault(SqlUpdate node)
    {
      using (context.EnterScope(node)) {
        VisitUpdateEntry(node);
        VisitUpdateUpdate(node);
        VisitUpdateSet(node);
        VisitUpdateFrom(node);
        VisitUpdateWhere(node);
        VisitUpdateLimit(node);
        VisitUpdateExit(node);
      }
    }

    /// <summary>
    /// Visits entry part of <see cref="SqlUpdate"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitUpdateEntry(SqlUpdate node) => AppendTranslatedEntry(node);

    /// <summary>
    /// Visits UPDATE part of <see cref="SqlUpdate"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitUpdateUpdate(SqlUpdate node)
    {
      if (node.Update == null) {
        throw new SqlCompilerException(Strings.ExTablePropertyIsNotSet);
      }

      using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableAliasing)) {
        node.Update.AcceptVisitor(this);
      }
    }

    /// <summary>
    /// Visits SET part of <see cref="SqlUpdate"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitUpdateSet(SqlUpdate node)
    {
      AppendTranslated(node, UpdateSection.Set);

      using (context.EnterCollectionScope()) {
        foreach (var item in node.Values.Keys) {
          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          var tc = item as SqlTableColumn;
          if (tc is not null && tc.SqlTable != node.Update)
            throw new SqlCompilerException(string.Format(Strings.ExUnboundColumn, tc.Name));
          translator.TranslateIdentifier(context.Output, tc.Name);
          AppendTranslated(SqlNodeType.Equals);
          var value = node.Values[item];
          value.AcceptVisitor(this);
        }
      }
    }

    /// <summary>
    /// Visits FROM part of <see cref="SqlUpdate"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitUpdateFrom(SqlUpdate node)
    {
      if (Driver.ServerInfo.Query.Features.Supports(QueryFeatures.UpdateFrom) && node.From != null) {
        AppendTranslated(node, UpdateSection.From);
        node.From.AcceptVisitor(this);
      }
    }

    /// <summary>
    /// Visits WHERE part of <see cref="SqlUpdate"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitUpdateWhere(SqlUpdate node)
    {
      if (node.Where is not null) {
        AppendTranslated(node, UpdateSection.Where);
        node.Where.AcceptVisitor(this);
      }
    }

    /// <summary>
    /// Visits LIMIT part of <see cref="SqlUpdate"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitUpdateLimit(SqlUpdate node)
    {
      if (node.Limit is not null) {
        if (!Driver.ServerInfo.Query.Features.Supports(QueryFeatures.UpdateLimit)) {
          throw new NotSupportedException(Strings.ExStorageIsNotSupportedLimitationOfRowCountToUpdate);
        }

        AppendTranslated(node, UpdateSection.Limit);
        node.Limit.AcceptVisitor(this);
      }
    }

    /// <summary>
    /// Visits end part of <see cref="SqlUpdate"/> statement.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    protected virtual void VisitUpdateExit(SqlUpdate node) => AppendTranslatedExit(node);

    /// <summary>
    /// Visits <see cref="SqlPlaceholder"/> expression.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlPlaceholder node)
    {
      context.Output.AppendPlaceholderWithId(node.Id);
    }

    /// <summary>
    /// Visits <see cref="SqlUserColumn"/> node.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public virtual void Visit(SqlUserColumn node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        node.Expression.AcceptVisitor(this);
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlUserFunctionCall"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlUserFunctionCall node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);

        if (node.Arguments.Count > 0) {
          using (context.EnterCollectionScope()) {
            var argumentPosition = 0;
            foreach (var item in node.Arguments) {
              AppendCollectionDelimiterIfNecessary(() => AppendTranslated(node, FunctionCallSection.ArgumentDelimiter, argumentPosition++));
              item.AcceptVisitor(this);
            }
          }
        }

        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlVariable"/> expression and translates it.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlVariable node) => AppendTranslated(node);

    /// <summary>
    /// Visits <see cref="SqlVariant"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlVariant node)
    {
      using (context.EnterMainVariantScope(node.Id)) {
        node.Main.AcceptVisitor(this);
      }

      using (context.EnterAlternativeVariantScope(node.Id)) {
        node.Alternative.AcceptVisitor(this);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlWhile"/> statement and translates its parts.
    /// </summary>
    /// <param name="node">Statement to visit.</param>
    public virtual void Visit(SqlWhile node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, WhileSection.Entry);
        node.Condition.AcceptVisitor(this);
        AppendTranslated(node, WhileSection.Statement);
        node.Statement.AcceptVisitor(this);
        AppendTranslated(node, WhileSection.Exit);
      }
    }

    /// <summary>
    /// Visits <see cref="TableColumn"/> and translates it.
    /// </summary>
    /// <param name="column">Column to visit.</param>
    public virtual void Visit(TableColumn column)
    {
      AppendTranslated(column, TableColumnSection.Entry);
      if (column.Expression is null) {
        if (column.Domain == null) {
          ArgumentValidator.EnsureArgumentNotNull(column.DataType, "DataType");
        }

        AppendTranslated(column, TableColumnSection.Type);
      }
      if (column.Collation != null) {
        AppendTranslated(column, TableColumnSection.Collate);
      }
      if (column.DefaultValue is not null) {
        AppendTranslated(column, TableColumnSection.DefaultValue);
        column.DefaultValue.AcceptVisitor(this);
      }
      else if (column.SequenceDescriptor != null) {
        AppendTranslated(column, TableColumnSection.GeneratedEntry);
        AppendTranslated(column.SequenceDescriptor, SequenceDescriptorSection.StartValue);
        AppendTranslated(column.SequenceDescriptor, SequenceDescriptorSection.Increment);
        AppendTranslated(column.SequenceDescriptor, SequenceDescriptorSection.MaxValue);
        AppendTranslated(column.SequenceDescriptor, SequenceDescriptorSection.MinValue);
        AppendTranslated(column.SequenceDescriptor, SequenceDescriptorSection.IsCyclic);
        AppendTranslated(column, TableColumnSection.GeneratedExit);
      }
      else if (column.Expression is not null) {
        AppendTranslated(column, TableColumnSection.GenerationExpressionEntry);
        using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableQualifiedColumns)) {
          column.Expression.AcceptVisitor(this);
        }
        AppendTranslated(column, TableColumnSection.GenerationExpressionExit);
      }
      if (!column.IsNullable) {
        AppendTranslated(column, TableColumnSection.NotNull);
      }
      AppendTranslated(column, TableColumnSection.Exit);
    }

    /// <summary>
    /// Visits <see cref="SqlExtract"/> expression and translates its parts.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlExtract node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        if (node.DateTimePart!= SqlDateTimePart.Nothing) {
          translator.Translate(context.Output, node.DateTimePart);
        }
        else if (node.IntervalPart!= SqlIntervalPart.Nothing) {
          translator.Translate(context.Output, node.IntervalPart);
        }
        else {
          translator.Translate(context.Output, node.DateTimeOffsetPart);
        }
        AppendTranslated(node, ExtractSection.From);
        node.Operand.AcceptVisitor(this);
        AppendTranslatedExit(node);
      }
    }

    /// <summary>
    /// Visits <see cref="SqlComment"/> expression and translates it.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    public virtual void Visit(SqlComment node) => translator.Translate(context, node);

    /// <summary>
    /// Visits <see cref="SqlComment"/> expression and translates it if setting is set it to be before statement.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    protected virtual void VisitCommentIfBefore(SqlComment node)
    {
      if (node is null || configuration.CommentLocation != SqlCommentLocation.BeforeStatement) {
        return;
      }

      Visit(node);
      _ = context.Output.AppendNewLine(translator.NewLine);
    }

    /// <summary>
    /// Visits <see cref="SqlComment"/> expression and translates it if setting is set it to be within statement.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    protected virtual void VisitCommentIfWithin(SqlComment node)
    {
      if (node is null || configuration.CommentLocation != SqlCommentLocation.WithinStatement) {
        return;
      }
      AppendSpaceIfNecessary();
      Visit(node);
      AppendSpaceIfNecessary();
    }

    /// <summary>
    /// Visits <see cref="SqlComment"/> expression and translates it if setting is set it to be after statement.
    /// </summary>
    /// <param name="node">Expression to visit.</param>
    protected virtual void VisitCommentIfAfter(SqlComment node)
    {
      if (node is null || configuration.CommentLocation != SqlCommentLocation.AfterStatement) {
        return;
      }
      _ = context.Output.AppendNewLine(translator.NewLine);
      Visit(node);
    }

    private void Visit(TableConstraint constraint)
    {
      using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableQualifiedColumns)) {
        AppendTranslated(constraint, ConstraintSection.Entry);
        if (constraint is CheckConstraint checkConstraint) {
          VisitCheckConstraint(checkConstraint);
        }
        if (constraint is UniqueConstraint uniqueConstraint) {
          VisitUniqueConstraint(uniqueConstraint);
        }
        if (constraint is ForeignKey foreignKey) {
          VisitForeignKeyConstraint(foreignKey);
        }
        AppendTranslated(constraint, ConstraintSection.Exit);
      }
    }

    private void VisitForeignKeyConstraint(ForeignKey constraint)
    {
      if (constraint.ReferencedColumns.Count == 0) {
        throw new SqlCompilerException(Strings.ExReferencedColumnsCountCantBeLessThenOne);
      }
      if (constraint.Columns.Count == 0) {
        throw new SqlCompilerException(Strings.ExReferencingColumnsCountCantBeLessThenOne);
      }
      AppendTranslated(constraint, ConstraintSection.ForeignKey);
      using (context.EnterCollectionScope()) {
        foreach (var column in constraint.Columns) {
          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          AppendSpaceIfNecessary();
          translator.Translate(context, column, TableColumnSection.Entry);
        }
      }

      AppendTranslated(constraint, ConstraintSection.ReferencedColumns);
      using (context.EnterCollectionScope()) {
        foreach (var tc in constraint.ReferencedColumns) {
          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          AppendSpaceIfNecessary();
          translator.Translate(context, tc, TableColumnSection.Entry);
        }
      }
    }

    private void VisitUniqueConstraint(UniqueConstraint constraint)
    {
      if (constraint is PrimaryKey) {
        AppendTranslated(constraint, ConstraintSection.PrimaryKey);
      }
      else {
        AppendTranslated(constraint, ConstraintSection.Unique);
      }

      if (constraint.Columns.Count > 0) {
        using (context.EnterCollectionScope()) {
          foreach (var tc in constraint.Columns) {
            AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
            AppendSpaceIfNecessary();
            translator.Translate(context, tc, TableColumnSection.Entry);
          }
        }
      }
    }

    private void VisitCheckConstraint(CheckConstraint constraint)
    {
      AppendTranslated(constraint, ConstraintSection.Check);
      constraint.Condition.AcceptVisitor(this);
    }

    #endregion

    #region Append methods - helpers to write to output

    protected void AppendDelimiter(string delimiter, SqlDelimiterType type = SqlDelimiterType.Row)
    {
      switch (type) {
        case SqlDelimiterType.Column:
          _ = context.Output.AppendNewLine(translator.NewLine);
          context.Output.AppendIndent();
          break;
      }
      //_ = context.Output.AppendClosingPunctuation(delimiter);
      _ = context.Output.Append(delimiter);
      AppendSpaceIfNecessary();
    }

    protected void AppendBatchDelimiter() =>
      context.Output.AppendClosingPunctuation(translator.BatchItemDelimiter);

    protected void AppendColumnDelimiter() =>
      AppendDelimiter(translator.ColumnDelimiter);

    protected void AppendDdlStatementDelimiter() =>
      AppendDelimiter(translator.DdlStatementDelimiter, SqlDelimiterType.Column);

    protected void AppendRowItemDelimiter() =>
      AppendDelimiter(translator.RowItemDelimiter);

    protected void AppendCollectionDelimiterIfNecessary(System.Action action)
    {
      if (!context.Output.StartOfCollection) {
        action();
      }
      context.Output.StartOfCollection = false;
    }

    protected void AppendSpaceIfNecessary() => context.Output.AppendSpaceIfNecessary();

    protected void AppendSpace() => context.Output.AppendSpace();

    protected void AppendTranslated(SqlNodeType nodeType)
    {
      AppendSpace();
      translator.Translate(context.Output, nodeType);
      AppendSpace();
    }

    protected void AppendTranslatedEntry(SqlAggregate node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, NodeSection.Entry);
    }
    protected void AppendTranslatedExit(SqlAggregate node) =>
      translator.Translate(context, node, NodeSection.Exit);

    protected void AppendTranslated(SqlArray node, ArraySection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SqlBetween node, BetweenSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlBetween node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, BetweenSection.Entry);
    }

    protected void AppendTranslatedExit(SqlBetween node) =>
      translator.Translate(context, node, BetweenSection.Exit);

    protected void AppendTranslatedEntry(SqlBinary node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, NodeSection.Entry);
    }

    protected void AppendTranslatedExit(SqlBinary node) =>
      translator.Translate(context, node, NodeSection.Exit);

    protected void AppendTranslated(SqlBreak node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
    }

    protected void AppendTranslated(SqlCase node, CaseSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlCase node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, CaseSection.Entry);
    }

    protected void AppendTranslatedExit(SqlCase node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, CaseSection.Exit);
    }

    protected void AppendTranslated(SqlCase node, SqlExpression item, CaseSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, item, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCast node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SqlAlterDomain node, AlterDomainSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlAlterDomain node) =>
      translator.Translate(context, node, AlterDomainSection.Entry);

    protected void AppendTranslatedExit(SqlAlterDomain node) =>
      translator.Translate(context, node, AlterDomainSection.Exit);

    protected void AppendTranslatedEntry(SqlAlterSequence node) =>
      translator.Translate(context, node, NodeSection.Entry);

    protected void AppendTranslatedExit(SqlAlterSequence node) =>
      translator.Translate(context, node, NodeSection.Exit);

    protected void AppendTranslated(SqlAlterTable node, AlterTableSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlAlterTable node) =>
      translator.Translate(context, node, AlterTableSection.Entry);

    protected void AppendTranslatedExit(SqlAlterTable node) =>
      translator.Translate(context, node, AlterTableSection.Exit);

    protected void AppendTranslated(SqlCloseCursor node) =>
      translator.Translate(context, node);

    protected void AppendTranslated(SqlCollate node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SqlColumnRef node, ColumnSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlConcat node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, NodeSection.Entry);
    }

    protected void AppendTranslatedExit(SqlConcat node)
    {
      translator.Translate(context, node, NodeSection.Exit);
    }

    protected void AppendTranslated(TableColumn column, TableColumnSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, column, section);
    }

    protected void AppendTranslated(Constraint constraint, ConstraintSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, constraint, section);
    }

    protected void AppendTranslated(SqlCommand node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
    }

    protected void AppendTranslated(SqlContinue node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
    }

    protected void AppendTranslatedEntry(SqlCreateAssertion node)
    {
      translator.Translate(context, node, NodeSection.Entry);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedExit(SqlCreateAssertion node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, NodeSection.Entry);
    }

    protected void AppendTranslated(SqlCreateDomain node, CreateDomainSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslatedEntry(SqlCreateDomain node)
    {
      translator.Translate(context, node, CreateDomainSection.Entry);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedExit(SqlCreateDomain node) =>
      translator.Translate(context, node, CreateDomainSection.Exit);

    protected void AppendTranslated(SqlCreateIndex node, CreateIndexSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslatedEntry(SqlCreateIndex node)
    {
      translator.Translate(context, node, CreateIndexSection.Entry);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedExit(SqlCreateIndex node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, CreateIndexSection.Exit);
    }

    protected void AppendTranslated(SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, descriptor, section);
    }

    protected void AppendTranslatedEntry(SqlCreateSchema node)
    {
      translator.Translate(context, node, NodeSection.Entry);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedExit(SqlCreateSchema node) =>
      translator.Translate(context, node, NodeSection.Exit);

    protected void AppendTranslatedEntry(SqlCreateSequence node)
    {
      translator.Translate(context, node, NodeSection.Entry);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedExit(SqlCreateSequence node) =>
      translator.Translate(context, node, NodeSection.Exit);

    protected void AppendTranslated(SqlCreateTable node, CreateTableSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslatedEntry(SqlCreateTable node)
    {
      translator.Translate(context, node, CreateTableSection.Entry);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedExit(SqlCreateTable node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, CreateTableSection.Exit);
    }

    protected void AppendTranslatedEntry(SqlCreateView node)
    {
      translator.Translate(context, node, NodeSection.Entry);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedExit(SqlCreateView node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, NodeSection.Exit);
    }

    protected void AppendTranslated(SqlCursor node) =>
      translator.Translate(context, node);

    protected void AppendTranslated(SqlDeclareCursor node, DeclareCursorSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslatedEntry(SqlDeclareCursor node) =>
      translator.Translate(context, node, DeclareCursorSection.Entry);

    protected void AppendTranslatedExit(SqlDeclareCursor node) =>
      translator.Translate(context, node, DeclareCursorSection.Exit);

    protected void AppendTranslated(SqlDeclareVariable node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
    }

    protected void AppendTranslated(SqlDefaultValue node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
    }

    protected void AppendTranslated(SqlDelete node, DeleteSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlDelete node)
    {
      translator.Translate(context, node, DeleteSection.Entry);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedExit(SqlDelete node) =>
      translator.Translate(context, node, DeleteSection.Exit);

    protected void AppendTranslated(SqlFetch node, FetchSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslatedEntry(SqlFetch node) =>
      translator.Translate(context, node, FetchSection.Entry);

    protected void AppendTranslatedExit(SqlFetch node) =>
      translator.Translate(context, node, FetchSection.Exit);

    protected void AppendTranslated(SqlFunctionCall node, FunctionCallSection section, int position)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section, position);
    }

    protected void AppendTranslatedEntry(SqlFunctionCall node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, FunctionCallSection.Entry, -1);
    }

    protected void AppendTranslatedExit(SqlFunctionCall node) =>
      translator.Translate(context, node, FunctionCallSection.Exit, -1);

    protected void AppendTranslated(SqlUserFunctionCall node, FunctionCallSection section, int position)
    {
      translator.Translate(context, node, section, position);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlUserFunctionCall node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, FunctionCallSection.Entry, -1);
    }

    protected void AppendTranslatedExit(SqlUserFunctionCall node) =>
      translator.Translate(context, node, FunctionCallSection.Exit, -1);

    protected void AppendTranslated(SqlIf node, IfSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SqlInsert node, InsertSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslatedEntry(SqlInsert node)
    {
      translator.Translate(context, node, InsertSection.Entry);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedExit(SqlInsert node) =>
      translator.Translate(context, node, InsertSection.Exit);

    protected void AppendTranslated(SqlJoinExpression node, JoinSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SqlLike node, LikeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlLike node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, LikeSection.Entry);
    }

    protected void AppendTranslatedExit(SqlLike node) =>
      translator.Translate(context, node, LikeSection.Exit);

    protected void AppendTranslated(SqlMatch node, MatchSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SqlNull node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
    }

    protected void AppendTranslated(SqlNextValue node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SchemaNode node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
    }

    protected void AppendTranslated(SqlOpenCursor node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
    }

    protected void AppendTranslated(SqlQueryRef node, TableSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SqlQueryExpression node, QueryExpressionSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlQueryExpression node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, QueryExpressionSection.Entry);
    }

    protected void AppendTranslatedExit(SqlQueryExpression node) =>
      translator.Translate(context, node, QueryExpressionSection.Exit);

    protected void AppendTranslated(SqlRow node, NodeSection section) =>
      translator.Translate(context, node, section);

    protected void AppendTranslated(SqlRowNumber node, NodeSection section) =>
      translator.Translate(context, node, section);

    protected void AppendTranslated(SqlSelect node, SelectSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlSelect node)
    {
      translator.Translate(context, node, SelectSection.Entry);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedExit(SqlSelect node) =>
      translator.Translate(context, node, SelectSection.Exit);

    protected void AppendTranslated(SqlStatementBlock node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SqlSubQuery node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SqlTableRef node, TableSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SqlTrim node, TrimSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
    }

    protected void AppendTranslated(SqlUnary node, NodeSection section) =>
      translator.Translate(context, node, section);

    protected void AppendTranslated(SqlUpdate node, UpdateSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlUpdate node) =>
      translator.Translate(context, node, UpdateSection.Entry);

    protected void AppendTranslatedExit(SqlUpdate node) =>
      translator.Translate(context, node, UpdateSection.Exit);

    protected void AppendTranslatedEntry(SqlUserColumn node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, NodeSection.Entry);
    }

    protected void AppendTranslatedExit(SqlUserColumn node) =>
      translator.Translate(context, node, NodeSection.Exit);

    protected void AppendTranslated(SqlVariable node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
    }

    protected void AppendTranslated(SqlWhile node, WhileSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlExtract extract, ExtractSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, extract, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedEntry(SqlExtract extract)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, extract, ExtractSection.Entry);
    }

    protected void AppendTranslatedExit(SqlExtract extract) =>
      translator.Translate(context, extract, ExtractSection.Exit);

    protected void AppendTranslated(SqlNative node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlAssignment node, NodeSection section) =>
      translator.Translate(context, node, section);

    protected void AppendTranslatedLiteral(object literalValue)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, literalValue);
    }

    #endregion

    private void TranslateDynamicFilterViaInOperator(SqlDynamicFilter node)
    {
      var numberOfExpressions = node.Expressions.Count;
      var isMulticolumn = numberOfExpressions > 1;
      var delimiter = translator.RowItemDelimiter + " ";
      _ = context.Output.Append(translator.OpeningParenthesis);
      var filteredExpression = isMulticolumn ? SqlDml.Row(node.Expressions) : node.Expressions[0];
      filteredExpression.AcceptVisitor(this);
      AppendTranslated(SqlNodeType.In);
      _ = context.Output.Append(translator.RowBegin);
      using (context.EnterCycleBodyScope(node.Id, delimiter)) {
        if (isMulticolumn) {
          _ = context.Output.Append(translator.RowBegin);
          for (int i = 0; i < numberOfExpressions; i++) {
            context.Output.AppendCycleItem(i);
            if (i != numberOfExpressions - 1) {
              AppendRowItemDelimiter();
            }
          }
          _ = context.Output.Append(translator.RowEnd);
        }
        else {
          context.Output.AppendCycleItem(0);
        }
      }
      using (context.EnterCycleEmptyCaseScope(node.Id)) {
        var nullExpression = isMulticolumn
          ? SqlDml.Row(Enumerable.Repeat(SqlDml.Null, numberOfExpressions).ToArray())
          : (SqlExpression) SqlDml.Null;
        nullExpression.AcceptVisitor(this);
        // Append "and false" to avoid result beeing "undefined" rather than "false"
        _ = context.Output.Append(translator.ClosingParenthesis);
        AppendTranslated(SqlNodeType.And);
        _ = context.Output.Append(translator.OpeningParenthesis);
        WriteFalseExpression();
      }
      _ = context.Output.Append(translator.RowEnd);
      _ = context.Output.Append(translator.ClosingParenthesis);
    }

    private void TranslateDynamicFilterViaMultipleComparisons(SqlDynamicFilter node)
    {
      var expressions = node.Expressions;
      var numberOfExpressions = expressions.Count;
      var delimiter = " " + translator.TranslateToString(SqlNodeType.Or) + " ";
      using (context.EnterCycleBodyScope(node.Id, delimiter)) {
        _ = context.Output.Append(translator.OpeningParenthesis);
        for (var i = 0; i < numberOfExpressions; i++) {
          expressions[i].AcceptVisitor(this);
          AppendTranslated(SqlNodeType.Equals);
          context.Output.AppendCycleItem(i);
          if (i != numberOfExpressions - 1) {
            AppendSpaceIfNecessary();
            AppendTranslated(SqlNodeType.And);
          }
        }
        _ = context.Output.Append(translator.ClosingParenthesis);
      }
      using (context.EnterCycleEmptyCaseScope(node.Id)) {
        WriteFalseExpression();
      }
    }

    private void WriteFalseExpression() =>
      SqlDml.Equals(SqlDml.Literal(1), SqlDml.Literal(0)).AcceptVisitor(this);

    private static IEnumerable<SqlStatement> FlattenBatch(SqlBatch batch)
    {
      foreach (var statement in batch) {
        if (statement is SqlBatch nestedBatch) {
          foreach (var nestedStatement in FlattenBatch(nestedBatch)) {
            yield return nestedStatement;
          }
        }
        else {
          yield return statement;
        }
      }
    }

    protected bool CheckFeature(ColumnFeatures features) => Driver.ServerInfo.Column.Features.Supports(features);

    protected bool CheckFeature(IndexFeatures features) => Driver.ServerInfo.Index.Features.Supports(features);

    protected bool CheckFeature(QueryFeatures features) => queryFeatures.Supports(features);

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected SqlCompiler(SqlDriver driver)
      : base(driver)
    {
      translator = driver.Translator;
      decimalType = driver.TypeMappings[WellKnownTypes.Decimal].MapType();
      queryFeatures = Driver.ServerInfo.Query.Features;
    }
  }
}
