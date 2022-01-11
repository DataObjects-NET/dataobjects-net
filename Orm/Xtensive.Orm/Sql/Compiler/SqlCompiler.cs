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

    protected readonly SqlValueType decimalType;
    protected readonly SqlTranslator translator;

    protected SqlCompilerConfiguration configuration;
    protected SqlCompilerContext context;

    private void AppendCollectionDelimiterIfNecessary(System.Action action)
    {
      if (!context.Output.StartOfCollection) {
        action();
      }
      context.Output.StartOfCollection = false;
    }

    public SqlCompilationResult Compile(ISqlCompileUnit unit, SqlCompilerConfiguration compilerConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(unit, "unit");
      configuration = compilerConfiguration;
      context = new SqlCompilerContext(configuration);
      unit.AcceptVisitor(this);
      return new SqlCompilationResult(context.Output.Children, context.ParameterNameProvider.NameTable);
    }

    public virtual void Visit(SqlAggregate node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Expression.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlAlterDomain node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, AlterDomainSection.Entry);
        switch (node.Action) {
          case SqlAddConstraint action:
            var constraint = (DomainConstraint) action.Constraint;
            AppendTranslated(node, AlterDomainSection.AddConstraint);
            AppendTranslated(constraint, ConstraintSection.Entry);
            AppendTranslated(constraint, ConstraintSection.Check);
            constraint.Condition.AcceptVisitor(this);
            AppendTranslated(constraint, ConstraintSection.Exit);
            break;
          case SqlDropConstraint action:
            AppendTranslated(node, AlterDomainSection.DropConstraint);
            AppendTranslated(action.Constraint, ConstraintSection.Entry);
            break;
          case SqlSetDefault action:
            AppendTranslated(node, AlterDomainSection.SetDefault);
            action.DefaultValue.AcceptVisitor(this);
            break;
          case SqlDropDefault _:
            AppendTranslated(node, AlterDomainSection.DropDefault);
            break;
        }
        AppendTranslated(node, AlterDomainSection.Exit);
      }
    }

    public virtual void Visit(SqlAlterPartitionFunction node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlAlterPartitionScheme node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlFragment node)
    {
      using (context.EnterScope(node))
      using (context.EnterScope(0)) {
        node.Expression.AcceptVisitor(this);
      }
    }

    public virtual void Visit(SqlAlterTable node)
    {
      using var _ = context.EnterScope(node);

      AppendTranslated(node, AlterTableSection.Entry);
      switch (node.Action) {
        case SqlAddColumn sqlAddColumn: {
          var column = sqlAddColumn.Column;
          AppendTranslated(node, AlterTableSection.AddColumn);
          Visit(column);
        }
        break;
        case SqlDropDefault sqlDropDefault: {
          var column = sqlDropDefault.Column;
          AppendTranslated(node, AlterTableSection.AlterColumn);
          AppendTranslated(column, TableColumnSection.Entry);
          AppendTranslated(column, TableColumnSection.DropDefault);
        }
        break;
        case SqlSetDefault action:
          AppendTranslated(node, AlterTableSection.AlterColumn);
          AppendTranslated(action.Column, TableColumnSection.Entry);
          AppendTranslated(action.Column, TableColumnSection.SetDefault);
          action.DefaultValue.AcceptVisitor(this);
          break;
        case SqlDropColumn action:
          AppendTranslated(node, AlterTableSection.DropColumn);
          AppendTranslated(action.Column, TableColumnSection.Entry);
          AppendTranslated(node, AlterTableSection.DropBehavior);
          break;
        case SqlAlterIdentityInfo action:
          AppendTranslated(node, AlterTableSection.AlterColumn);
          AppendTranslated(action.Column, TableColumnSection.Entry);
          if ((action.InfoOption & SqlAlterIdentityInfoOptions.RestartWithOption) != 0)
            AppendTranslated(action.SequenceDescriptor, SequenceDescriptorSection.RestartValue);
          if ((action.InfoOption & SqlAlterIdentityInfoOptions.IncrementByOption) != 0) {
            if (action.SequenceDescriptor.Increment.HasValue && action.SequenceDescriptor.Increment.Value == 0)
              throw new SqlCompilerException(Strings.ExIncrementMustNotBeZero);
            AppendTranslated(action.Column, TableColumnSection.SetIdentityInfoElement);
            AppendTranslated(action.SequenceDescriptor, SequenceDescriptorSection.Increment);
          }
          if ((action.InfoOption & SqlAlterIdentityInfoOptions.MaxValueOption) != 0) {
            AppendTranslated(action.Column, TableColumnSection.SetIdentityInfoElement);
            AppendTranslated(action.SequenceDescriptor, SequenceDescriptorSection.AlterMaxValue);
          }
          if ((action.InfoOption & SqlAlterIdentityInfoOptions.MinValueOption) != 0) {
            AppendTranslated(action.Column, TableColumnSection.SetIdentityInfoElement);
            AppendTranslated(action.SequenceDescriptor, SequenceDescriptorSection.AlterMinValue);
          }
          if ((action.InfoOption & SqlAlterIdentityInfoOptions.CycleOption) != 0) {
            AppendTranslated(action.Column, TableColumnSection.SetIdentityInfoElement);
            AppendTranslated(action.SequenceDescriptor, SequenceDescriptorSection.IsCyclic);
          }
          break;
        case SqlAddConstraint action: {
          var constraint = action.Constraint as TableConstraint;
          AppendTranslated(node, AlterTableSection.AddConstraint);
          Visit(constraint);
        }
        break;
        case SqlDropConstraint action: {
          var constraint = action.Constraint as TableConstraint;
          AppendTranslated(node, AlterTableSection.DropConstraint);
          AppendTranslated(constraint, ConstraintSection.Entry);
          AppendTranslated(node, AlterTableSection.DropBehavior);
        }
        break;
        case SqlRenameColumn action:
          AppendTranslated(node, AlterTableSection.RenameColumn);
          AppendTranslated(action.Column, TableColumnSection.Entry);
          AppendTranslated(node, AlterTableSection.To);
          translator.TranslateIdentifier(context.Output, action.NewName);
          break;
      }
      AppendTranslated(node, AlterTableSection.Exit);
    }

    public virtual void Visit(SqlAlterSequence node)
    {
      AppendTranslated(node, NodeSection.Entry);
      if ((node.InfoOption & SqlAlterIdentityInfoOptions.RestartWithOption) != 0) {
        AppendTranslated(node.SequenceDescriptor, SequenceDescriptorSection.RestartValue);
      }
      if ((node.InfoOption & SqlAlterIdentityInfoOptions.IncrementByOption) != 0) {
        if (node.SequenceDescriptor.Increment.HasValue && node.SequenceDescriptor.Increment.Value == 0)
          throw new SqlCompilerException(Strings.ExIncrementMustNotBeZero);
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
      AppendTranslated(node, NodeSection.Exit);
    }

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
        for (int i = 0; i < lengthMinusOne; i++) {
          Visit((SqlPlaceholder) items[i]);
          AppendRowItemDelimiter();
        }
        Visit((SqlPlaceholder) items[lengthMinusOne]);
      }
      else {
        for (int i = 0; i < lengthMinusOne; i++) {
          AppendTranslatedLiteral(items[i]);
          AppendRowItemDelimiter();
        }
        AppendTranslatedLiteral(items[lengthMinusOne]);
      }
      AppendTranslated(node, ArraySection.Exit);
    }

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

    public virtual void Visit(SqlBatch node)
    {
      using (context.EnterScope(node)) {
        var statements = FlattenBatch(node).ToArray();
        if (statements.Length == 0)
          return;
        if (statements.Length == 1) {
          statements[0].AcceptVisitor(this);
          return;
        }
        context.Output.Append(translator.BatchBegin);
        using (context.EnterCollectionScope()) {
          foreach (var item in statements) {
            item.AcceptVisitor(this);
            AppendDelimiter(translator.BatchItemDelimiter, SqlDelimiterType.Column);
          }
        }
        context.Output.Append(translator.BatchEnd);
      }
    }

    public virtual void Visit(SqlBetween node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, BetweenSection.Entry);
        node.Expression.AcceptVisitor(this);
        AppendTranslated(node, BetweenSection.Between);
        node.Left.AcceptVisitor(this);
        AppendTranslated(node, BetweenSection.And);
        node.Right.AcceptVisitor(this);
        AppendTranslated(node, BetweenSection.Exit);
      }
    }

    public virtual void Visit(SqlBinary node)
    {
      if (node.NodeType == SqlNodeType.In || node.NodeType == SqlNodeType.NotIn) {
        var row = node.Right as SqlRow;
        var array = node.Right as SqlArray;
        bool isEmptyIn = !row.IsNullReference() && row.Count == 0 || !array.IsNullReference() && array.Length == 0;
        if (isEmptyIn) {
          SqlDml.Literal(node.NodeType == SqlNodeType.NotIn).AcceptVisitor(this);
          return;
        }
      }

      if (node.NodeType == SqlNodeType.Or || node.NodeType == SqlNodeType.And) {
        var expressions = RecursiveBinaryLogicExtractor.Extract(node);
        using (context.EnterScope(node)) {
          AppendTranslated(node, NodeSection.Entry);
          expressions[0].AcceptVisitor(this);
          foreach (var operand in expressions.Skip(1)) {
            AppendTranslated(node.NodeType);
            operand.AcceptVisitor(this);
          }
          AppendTranslated(node, NodeSection.Exit);
        }
        return;
      }

      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Left.AcceptVisitor(this);
        AppendTranslated(node.NodeType);
        node.Right.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlBreak node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlCase node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, CaseSection.Entry);

        if (!node.Value.IsNullReference()) {
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

        if (!node.Else.IsNullReference()) {
          AppendTranslated(node, CaseSection.Else);
          node.Else.AcceptVisitor(this);
        }

        AppendTranslated(node, CaseSection.Exit);
      }
    }

    public virtual void Visit(SqlCast node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Operand.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlCloseCursor node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlCollate node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Operand.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlColumnRef node)
    {
      AppendTranslated(node, ColumnSection.Entry);
    }

    public virtual void Visit(SqlContainsTable node)
    {
      throw SqlHelper.NotSupported(Strings.FullTextQueries);
    }

    public virtual void Visit(SqlNative node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlNativeHint node)
    {
      // nothing
    }

    public virtual void Visit(SqlConcat node)
    {
      AppendTranslated(node, NodeSection.Entry);
      bool first = true;
      foreach (var item in node) {
        if (!first) {
          AppendTranslated(node.NodeType);
        }
        item.AcceptVisitor(this);
        first = false;
      }

      AppendTranslated(node, NodeSection.Exit);
    }

    public virtual void Visit(SqlContainer node)
    {
      throw new SqlCompilerException(Strings.ExSqlContainerExpressionCanNotBeCompiled);
    }

    public virtual void Visit(SqlContinue node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlCommand node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlCreateAssertion node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Assertion.Condition.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlCreateCharacterSet node)
    {
      //      ArgumentValidator.EnsureArgumentNotNull(node.CharacterSet.CharacterSetSource, "CharacterSetSource");
      //      AppendTranslated(node);
    }

    public virtual void Visit(SqlCreateCollation node)
    {
      //      ArgumentValidator.EnsureArgumentNotNull(node.Collation.CharacterSet, "CharacterSet");
      //      AppendTranslated(node);
    }

    public virtual void Visit(SqlCreateDomain node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.Domain.DataType, "DataType");
      using (context.EnterScope(node)) {
        AppendTranslated(node, CreateDomainSection.Entry);
        if (!node.Domain.DefaultValue.IsNullReference()) {
          AppendTranslated(node, CreateDomainSection.DomainDefaultValue);
          node.Domain.DefaultValue.AcceptVisitor(this);
        }
        if (node.Domain.DomainConstraints.Count != 0) {
          using (context.EnterCollectionScope())
            foreach (DomainConstraint constraint in node.Domain.DomainConstraints) {
              AppendTranslated(constraint, ConstraintSection.Entry);
              AppendTranslated(constraint, ConstraintSection.Check);
              constraint.Condition.AcceptVisitor(this);
              AppendTranslated(constraint, ConstraintSection.Exit);
            }
        }
        if (node.Domain.Collation != null) {
          AppendTranslated(node, CreateDomainSection.DomainCollate);
        }
        AppendTranslated(node, CreateDomainSection.Exit);
      }
    }

    public virtual void Visit(SqlCreateIndex node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.Index.DataTable, "DataTable");
      AppendTranslated(node, CreateIndexSection.Entry);
      if (node.Index.Columns.Count > 0) {
        AppendTranslated(node, CreateIndexSection.ColumnsEnter);
        using (context.EnterCollectionScope()) {
          foreach (var item in node.Index.Columns) {
            AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
            Visit(node, item);
          }
        }
        AppendTranslated(node, CreateIndexSection.ColumnsExit);
      }
      if (node.Index.NonkeyColumns != null && node.Index.NonkeyColumns.Count != 0 && CheckFeature(IndexFeatures.NonKeyColumns)) {
        AppendTranslated(node, CreateIndexSection.NonkeyColumnsEnter);
        using (context.EnterCollectionScope()) {
          foreach (var item in node.Index.NonkeyColumns) {
            AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
            translator.TranslateIdentifier(context.Output, item.Name);
          }
        }
        AppendTranslated(node, CreateIndexSection.NonkeyColumnsExit);
      }

      AppendTranslated(node, CreateIndexSection.StorageOptions);

      if (!node.Index.Where.IsNullReference() && CheckFeature(IndexFeatures.Filtered)) {
        AppendTranslated(node, CreateIndexSection.Where);
        using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableQualifiedColumns)) {
          node.Index.Where.AcceptVisitor(this);
        }
      }
      AppendTranslated(node, CreateIndexSection.Exit);
    }

    public virtual void Visit(SqlCreateIndex node, IndexColumn item)
    {
      if (!item.Expression.IsNullReference() && CheckFeature(IndexFeatures.Expressions)) {
        using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableQualifiedColumns))
          item.Expression.AcceptVisitor(this);
      }
      else {
        translator.TranslateIdentifier(context.Output, item.Column.Name);
        if (!(node.Index.IsFullText || node.Index.IsSpatial) && CheckFeature(IndexFeatures.SortOrder))
          context.Output.Append(translator.TranslateSortOrder(item.Ascending));
      }
    }

    public virtual void Visit(SqlCreatePartitionFunction node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.PartitionFunction.DataType, "DataType");
      AppendTranslated(node);
    }

    public virtual void Visit(SqlCreatePartitionScheme node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.PartitionSchema.PartitionFunction, "PartitionFunction");
      AppendTranslated(node);
    }

    public virtual void Visit(SqlCreateSchema node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        AppendDdlStatementDelimiter();
        if (node.Schema.Assertions.Count > 0)
          using (context.EnterCollectionScope())
            foreach (Assertion assertion in node.Schema.Assertions) {
              new SqlCreateAssertion(assertion).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }

        if (node.Schema.CharacterSets.Count > 0)
          using (context.EnterCollectionScope())
            foreach (CharacterSet characterSet in node.Schema.CharacterSets) {
              new SqlCreateCharacterSet(characterSet).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }

        if (node.Schema.Collations.Count > 0)
          using (context.EnterCollectionScope())
            foreach (Collation collation in node.Schema.Collations) {
              AppendCollectionDelimiterIfNecessary(AppendDdlStatementDelimiter);
              new SqlCreateCollation(collation).AcceptVisitor(this);
            }

        if (node.Schema.Domains.Count > 0)
          using (context.EnterCollectionScope())
            foreach (Domain domain in node.Schema.Domains) {
              new SqlCreateDomain(domain).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }

        if (node.Schema.Sequences.Count > 0)
          using (context.EnterCollectionScope())
            foreach (Sequence sequence in node.Schema.Sequences) {
              new SqlCreateSequence(sequence).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }

        if (node.Schema.Tables.Count > 0)
          using (context.EnterCollectionScope())
            foreach (Table table in node.Schema.Tables) {
              new SqlCreateTable(table).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
              if (table.Indexes.Count > 0)
                using (context.EnterCollectionScope())
                  foreach (Index index in table.Indexes) {
                    new SqlCreateIndex(index).AcceptVisitor(this);
                    AppendDdlStatementDelimiter();
                  }
            }

        if (node.Schema.Translations.Count > 0)
          using (context.EnterCollectionScope())
            foreach (Translation translation in node.Schema.Translations) {
              new SqlCreateTranslation(translation).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }

        if (node.Schema.Views.Count > 0)
          using (context.EnterCollectionScope())
            foreach (View view in node.Schema.Views) {
              new SqlCreateView(view).AcceptVisitor(this);
              AppendDdlStatementDelimiter();
            }

        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlCreateSequence node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.Sequence.SequenceDescriptor, "SequenceDescriptor");
      using (context.EnterScope(node)) {
        if (node.Sequence.SequenceDescriptor.Increment.HasValue && node.Sequence.SequenceDescriptor.Increment.Value == 0)
          throw new SqlCompilerException(Strings.ExIncrementMustNotBeZero);
        if (string.IsNullOrEmpty(node.Sequence.Name))
          throw new SqlCompilerException(Strings.ExNameMustBeNotNullOrEmpty);
        if (node.Sequence.Schema == null)
          throw new SqlCompilerException(Strings.ExSchemaMustBeNotNull);
        if (node.Sequence.SequenceDescriptor.MaxValue.HasValue &&
            node.Sequence.SequenceDescriptor.MinValue.HasValue &&
            node.Sequence.SequenceDescriptor.MaxValue.Value <= node.Sequence.SequenceDescriptor.MinValue.Value)
          throw new SqlCompilerException(Strings.ExTheMaximumValueMustBeGreaterThanTheMinimumValue);
        if (node.Sequence.SequenceDescriptor.StartValue.HasValue &&
            (node.Sequence.SequenceDescriptor.MaxValue.HasValue &&
             node.Sequence.SequenceDescriptor.MaxValue.Value < node.Sequence.SequenceDescriptor.StartValue.Value ||
             node.Sequence.SequenceDescriptor.MinValue.HasValue &&
             node.Sequence.SequenceDescriptor.MinValue.Value > node.Sequence.SequenceDescriptor.StartValue.Value))
          throw new SqlCompilerException(Strings.ExTheStartValueShouldBeBetweenTheMinimumAndMaximumValue);
        AppendTranslated(node, NodeSection.Entry);
        AppendTranslated(node.Sequence.SequenceDescriptor, SequenceDescriptorSection.StartValue);
        AppendTranslated(node.Sequence.SequenceDescriptor, SequenceDescriptorSection.Increment);
        AppendTranslated(node.Sequence.SequenceDescriptor, SequenceDescriptorSection.MaxValue);
        AppendTranslated(node.Sequence.SequenceDescriptor, SequenceDescriptorSection.MinValue);
        AppendTranslated(node.Sequence.SequenceDescriptor, SequenceDescriptorSection.IsCyclic);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlCreateTable node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, CreateTableSection.Entry);
        AppendTranslated(node, CreateTableSection.TableElementsEntry);

        // ReSharper disable ConditionIsAlwaysTrueOrFalse
        // ReSharper disable RedundantAssignment

        bool hasItems = false;

        if (node.Table.Columns.Count > 0)
          hasItems = VisitCreateTableColumns(node, node.Table.TableColumns, hasItems);
        if (node.Table.TableConstraints.Count > 0)
          hasItems = VisitCreateTableConstraints(node, node.Table.TableConstraints, hasItems);

        // ReSharper restore ConditionIsAlwaysTrueOrFalse
        // ReSharper restore RedundantAssignment

        AppendTranslated(node, CreateTableSection.TableElementsExit);
        if (node.Table.PartitionDescriptor != null) {
          AppendDelimiter(translator.ColumnDelimiter, SqlDelimiterType.Column);
          AppendTranslated(node, CreateTableSection.Partition);
        }
        AppendTranslated(node, CreateTableSection.Exit);
      }
    }

    protected virtual bool VisitCreateTableConstraints(SqlCreateTable node, IEnumerable<TableConstraint> constraints, bool hasItems)
    {
      using (context.EnterCollectionScope()) {
        foreach (TableConstraint constraint in constraints) {
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

    protected virtual bool VisitCreateTableColumns(SqlCreateTable node, IEnumerable<TableColumn> columns, bool hasItems)
    {
      using (context.EnterCollectionScope()) {
        foreach (var column in columns) {
          // Skipping computed columns
          if (!column.Expression.IsNullReference() && !CheckFeature(ColumnFeatures.Computed))
            continue;
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

    public virtual void Visit(SqlCreateTranslation node)
    {
      //      ArgumentValidator.EnsureArgumentNotNull(node.Translation.SourceCharacterSet, "SourceCharacterSet");
      //      ArgumentValidator.EnsureArgumentNotNull(node.Translation.TargetCharacterSet, "TargetCharacterSet");
      //      ArgumentValidator.EnsureArgumentNotNull(node.Translation.TranslationSource, "TranslationSource");
      //      AppendTranslated(node);
    }

    public virtual void Visit(SqlCreateView node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        if (!node.View.Definition.IsNullReference())
          node.View.Definition.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlCursor node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDeclareCursor node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, DeclareCursorSection.Entry);
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
        AppendTranslated(node, DeclareCursorSection.Exit);
      }
    }

    public virtual void Visit(SqlDeclareVariable node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDefaultValue node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDelete node)
    {
      VisitDeleteDefault(node);
    }

    public void VisitDeleteDefault(SqlDelete node)
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

    public virtual void VisitDeleteEntry(SqlDelete node)
    {
      AppendTranslated(node, DeleteSection.Entry);
    }

    public virtual void VisitDeleteDelete(SqlDelete node)
    {
      if (node.Delete == null)
        throw new SqlCompilerException(Strings.ExTablePropertyIsNotSet);

      using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableAliasing)) {
        node.Delete.AcceptVisitor(this);
      }
    }

    public virtual void VisitDeleteFrom(SqlDelete node)
    {
      if (CheckFeature(QueryFeatures.DeleteFrom) && node.From != null) {
        AppendTranslated(node, DeleteSection.From);
        node.From.AcceptVisitor(this);
      }
    }

    public virtual void VisitDeleteWhere(SqlDelete node)
    {
      if (!node.Where.IsNullReference()) {
        AppendTranslated(node, DeleteSection.Where);
        node.Where.AcceptVisitor(this);
      }
    }

    public virtual void VisitDeleteLimit(SqlDelete node)
    {
      if (!node.Limit.IsNullReference()) {
        if (!CheckFeature(QueryFeatures.DeleteLimit))
          throw new NotSupportedException(Strings.ExStorageIsNotSupportedLimitationOfRowCountToDelete);
        AppendTranslated(node, DeleteSection.Limit);
        node.Limit.AcceptVisitor(this);
      }
    }

    public virtual void VisitDeleteExit(SqlDelete node)
    {
      AppendTranslated(node, DeleteSection.Exit);
    }

    public virtual void Visit(SqlDropAssertion node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDropCharacterSet node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDropCollation node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDropDomain node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDropIndex node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node.Index.DataTable, "DataTable");
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDropPartitionFunction node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDropPartitionScheme node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDropSchema node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDropSequence node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDropTable node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDropTranslation node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlDropView node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlFastFirstRowsHint node)
    {
      // nothing
    }

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
          if (CheckFeature(QueryFeatures.MulticolumnIn))
            TranslateDynamicFilterViaInOperator(node);
          else
            TranslateDynamicFilterViaMultipleComparisons(node);
          break;
      }
    }

    public virtual void Visit(SqlFetch node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, FetchSection.Entry);
        if (!node.RowCount.IsNullReference())
          node.RowCount.AcceptVisitor(this);
        AppendTranslated(node, FetchSection.Targets);
        foreach (ISqlCursorFetchTarget item in node.Targets) {
          item.AcceptVisitor(this);
        }
        AppendTranslated(node, FetchSection.Exit);
      }
    }

    public virtual void Visit(SqlForceJoinOrderHint node)
    {
      // nothing
    }

    public virtual void Visit(SqlFreeTextTable node)
    {
      throw SqlHelper.NotSupported(Strings.FullTextQueries);
    }

    public virtual void Visit(SqlFunctionCall node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, FunctionCallSection.Entry, -1);
        if (node.Arguments.Count > 0) {
          using (context.EnterCollectionScope()) {
            int argumentPosition = 0;
            foreach (SqlExpression item in node.Arguments) {
              AppendCollectionDelimiterIfNecessary(() => AppendTranslated(node, FunctionCallSection.ArgumentDelimiter, argumentPosition));
              AppendTranslated(node, FunctionCallSection.ArgumentEntry, argumentPosition);
              item.AcceptVisitor(this);
              AppendTranslated(node, FunctionCallSection.ArgumentExit, argumentPosition);
              argumentPosition++;
            }
          }
        }
        AppendTranslated(node, FunctionCallSection.Exit, -1);
      }
    }

    public virtual void Visit(SqlCustomFunctionCall node)
    {
      throw new NotSupportedException(string.Format(Strings.ExFunctionXIsNotSupported, node.FunctionType));
    }

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

    public virtual void Visit(SqlInsert node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, InsertSection.Entry);

        if (node.Into == null)
          throw new SqlCompilerException(Strings.ExTablePropertyIsNotSet);

        using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableAliasing)) {
          node.Into.AcceptVisitor(this);
        }

        AppendTranslated(node, InsertSection.ColumnsEntry);
        if (node.Values.Keys.Count > 0)
          using (context.EnterCollectionScope())
            foreach (SqlColumn item in node.Values.Keys) {
              AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
              translator.TranslateIdentifier(context.Output, item.Name);
            }
        AppendTranslated(node, InsertSection.ColumnsExit);

        if (node.Values.Keys.Count == 0 && node.From == null) {
          AppendTranslated(node, InsertSection.DefaultValues);
        }
        else {
          if (node.From != null)
            using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableAliasing)) {
              node.From.AcceptVisitor(this);
            }
          else {
            AppendTranslated(node, InsertSection.ValuesEntry);
            using (context.EnterCollectionScope())
              foreach (SqlExpression item in node.Values.Values) {
                AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
                item.AcceptVisitor(this);
              }
            AppendTranslated(node, InsertSection.ValuesExit);
          }
        }
        AppendTranslated(node, InsertSection.Exit);
      }
    }

    public virtual void Visit(SqlJoinExpression node)
    {
      var leftIsJoin = node.Left is SqlJoinedTable;
      if (leftIsJoin)
        node.Left.AcceptVisitor(this);
      using (context.EnterScope(node)) {
        AppendTranslated(node, JoinSection.Entry);
        if (!leftIsJoin)
          node.Left.AcceptVisitor(this);
        AppendTranslated(node, JoinSection.Specification);
        node.Right.AcceptVisitor(this);
        if (!node.Expression.IsNullReference()) {
          AppendTranslated(node, JoinSection.Condition);
          node.Expression.AcceptVisitor(this);
        }
        AppendTranslated(node, JoinSection.Exit);
      }
    }

    public virtual void Visit(SqlJoinHint node)
    {
      // nothing
    }

    public virtual void Visit(SqlLike node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, LikeSection.Entry);
        node.Expression.AcceptVisitor(this);
        AppendTranslated(node, LikeSection.Like);
        node.Pattern.AcceptVisitor(this);
        if (!node.Escape.IsNullReference()) {
          AppendTranslated(node, LikeSection.Escape);
          node.Escape.AcceptVisitor(this);
        }
        AppendTranslated(node, LikeSection.Exit);
      }
    }

    public virtual void Visit(SqlLiteral node)
    {
      AppendTranslatedLiteral(node.GetValue());
    }

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

    public virtual void Visit(SqlNull node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlNextValue node)
    {
      AppendTranslated(node, NodeSection.Entry);
      AppendTranslated(node.Sequence);
      AppendTranslated(node, NodeSection.Exit);
    }

    public virtual void Visit(SqlOpenCursor node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlOrder node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        if (!node.Expression.IsNullReference())
          node.Expression.AcceptVisitor(this);
        else if (node.Position > 0)
          context.Output.Append(node.Position.ToString());
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlParameterRef node)
    {
      var name = string.IsNullOrEmpty(node.Name)
        ? context.ParameterNameProvider.GetName(node.Parameter)
        : node.Name;
      context.Output.Append(translator.ParameterPrefix + name);
    }

    public virtual void Visit(SqlQueryRef node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, TableSection.Entry);
        node.Query.AcceptVisitor(this);
        AppendTranslated(node, TableSection.Exit);
        AppendTranslated(node, TableSection.AliasDeclaration);
      }
    }

    public virtual void Visit(SqlQueryExpression node)
    {
      var leftIsQueryExpression = node.Left is SqlQueryExpression;
      using (context.EnterScope(node)) {
        AppendTranslated(node, QueryExpressionSection.Entry);
        if (!leftIsQueryExpression)
          context.Output.Append("(");
        node.Left.AcceptVisitor(this);
        if (!leftIsQueryExpression)
          context.Output.Append(")");
        AppendTranslated(node.NodeType);
        AppendTranslated(node, QueryExpressionSection.All);
        context.Output.Append("(");
        node.Right.AcceptVisitor(this);
        context.Output.Append(")");
        AppendTranslated(node, QueryExpressionSection.Exit);
      }
    }

    public virtual void Visit(SqlRow node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        using (context.EnterCollectionScope()) {
          foreach (SqlExpression item in node) {
            AppendCollectionDelimiterIfNecessary(AppendRowItemDelimiter);
            item.AcceptVisitor(this);
          }
        }
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlRowNumber node)
    {
      if (!CheckFeature(QueryFeatures.RowNumber))
        throw SqlHelper.NotSupported(QueryFeatures.RowNumber);

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

    public virtual void Visit(SqlRenameTable node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlRound node)
    {
      SqlExpression result;
      switch (node.Mode) {
        case MidpointRounding.ToEven:
          result = node.Length.IsNullReference()
            ? SqlHelper.BankersRound(node.Argument)
            : SqlHelper.BankersRound(node.Argument, node.Length);
          break;
        case MidpointRounding.AwayFromZero:
          result = node.Length.IsNullReference()
            ? SqlHelper.RegularRound(node.Argument)
            : SqlHelper.RegularRound(node.Argument, node.Length);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      if (node.Type == TypeCode.Decimal)
        result = SqlDml.Cast(result, decimalType);
      result.AcceptVisitor(this);
    }

    public virtual void Visit(SqlSelect node)
    {
      VisitSelectDefault(node);
    }

    public void VisitSelectDefault(SqlSelect node)
    {
      using (context.EnterScope(node)) {
        var comment = node.Comment;
        VisitCommentIfBefore(comment);
        AppendTranslated(node, SelectSection.Entry);
        VisitCommentIfWithin(comment);
        VisitSelectHints(node);
        VisitSelectColumns(node);
        VisitSelectFrom(node);
        VisitSelectWhere(node);
        VisitSelectGroupBy(node);
        VisitSelectOrderBy(node);
        VisitSelectLimitOffset(node);
        VisitSelectLock(node);
        AppendTranslated(node, SelectSection.Exit);
        VisitCommentIfAfter(comment);
      }
    }

    public virtual void VisitSelectHints(SqlSelect node)
    {
      var hints = node.Hints;
      if (hints.Count == 0)
        return;
      using (context.EnterCollectionScope()) {
        AppendTranslated(node, SelectSection.HintsEntry);
        hints[0].AcceptVisitor(this);
        for (int i = 1; i < hints.Count; i++) {
          AppendDelimiter(translator.HintDelimiter);
          hints[i].AcceptVisitor(this);
        }
        AppendTranslated(node, SelectSection.HintsExit);
      }
    }

    public virtual void VisitSelectColumns(SqlSelect node)
    {
      if (node.Columns.Count == 0) {
        Visit(SqlDml.Asterisk);
        return;
      }
      using (context.EnterCollectionScope()) {
        foreach (SqlColumn item in node.Columns) {
          if (item is SqlColumnStub)
            continue;
          var cr = item as SqlColumnRef;
          if (!cr.IsNullReference() && cr.SqlColumn is SqlColumnStub)
            continue;

          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          if (!cr.IsNullReference()) {
            cr.SqlColumn.AcceptVisitor(this);
            AppendTranslated(cr, ColumnSection.AliasDeclaration);
          }
          else
            item.AcceptVisitor(this);
        }
      }
    }

    public virtual void VisitSelectFrom(SqlSelect node)
    {
      if (node.From == null)
        return;

      AppendTranslated(node, SelectSection.From);

      var joinedFrom = node.From as SqlJoinedTable;
      var linearJoinRequired = CheckFeature(QueryFeatures.StrictJoinSyntax) && joinedFrom != null;

      if (!linearJoinRequired) {
        node.From.AcceptVisitor(this);
        return;
      }

      var joinSequence = JoinSequence.Build(joinedFrom);
      var previous = joinSequence.Pivot;
      previous.AcceptVisitor(this);

      for (int i = 0; i < joinSequence.Tables.Count; i++) {
        var table = joinSequence.Tables[i];
        var type = joinSequence.JoinTypes[i];
        var condition = joinSequence.Conditions[i];
        var join = new SqlJoinExpression(type, previous, table, condition);

        AppendTranslated(join, JoinSection.Specification);
        table.AcceptVisitor(this);
        if (!condition.IsNullReference()) {
          AppendTranslated(join, JoinSection.Condition);
          condition.AcceptVisitor(this);
        }

        previous = table;
      }
    }

    public virtual void VisitSelectWhere(SqlSelect node)
    {
      if (node.Where.IsNullReference())
        return;
      AppendTranslated(node, SelectSection.Where);
      node.Where.AcceptVisitor(this);
    }

    public virtual void VisitSelectGroupBy(SqlSelect node)
    {
      if (node.GroupBy.Count <= 0)
        return;
      // group by
      AppendTranslated(node, SelectSection.GroupBy);
      using (context.EnterCollectionScope()) {
        foreach (SqlColumn item in node.GroupBy) {
          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          var cr = item as SqlColumnRef;
          if (!cr.IsNullReference())
            cr.SqlColumn.AcceptVisitor(this);
          else
            item.AcceptVisitor(this);
        }
      }
      if (node.Having.IsNullReference())
        return;
      // having
      AppendTranslated(node, SelectSection.Having);
      node.Having.AcceptVisitor(this);
    }

    public virtual void VisitSelectOrderBy(SqlSelect node)
    {
      if (node.OrderBy.Count <= 0)
        return;
      AppendTranslated(node, SelectSection.OrderBy);
      using (context.EnterCollectionScope()) {
        foreach (SqlOrder item in node.OrderBy) {
          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          item.AcceptVisitor(this);
        }
      }
    }

    public virtual void VisitSelectLimitOffset(SqlSelect node)
    {
      if (!node.Limit.IsNullReference()) {
        AppendTranslated(node, SelectSection.Limit);
        node.Limit.AcceptVisitor(this);
        AppendTranslated(node, SelectSection.LimitEnd);
      }
      if (!node.Offset.IsNullReference()) {
        AppendTranslated(node, SelectSection.Offset);
        node.Offset.AcceptVisitor(this);
        AppendTranslated(node, SelectSection.OffsetEnd);
      }
    }

    public virtual void VisitSelectLock(SqlSelect node)
    {
      if (node.Lock != SqlLockType.Empty) {
        context.Output.Append(translator.Translate(node.Lock));
        AppendSpaceIfNecessary();
      }
    }

    public virtual void Visit(SqlStatementBlock node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        using (context.EnterCollectionScope()) {
          foreach (SqlStatement item in node) {
            item.AcceptVisitor(this);
            AppendDelimiter(translator.BatchItemDelimiter, SqlDelimiterType.Column);
          }
        }
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlSubQuery node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Query.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlTableColumn node)
    {
      AppendTranslated(node, NodeSection.Entry);
    }

    public virtual void Visit(SqlTableRef node)
    {
      AppendTranslated(node, TableSection.Entry);
      AppendTranslated(node, TableSection.AliasDeclaration);
    }

    public virtual void Visit(SqlTrim node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, TrimSection.Entry);
        context.Output.Append(translator.Translate(node.TrimType));
        AppendSpaceIfNecessary();
        if (node.TrimCharacters != null) {
          AppendTranslatedLiteral(node.TrimCharacters);
        }
        AppendTranslated(node, TrimSection.From);
        node.Expression.AcceptVisitor(this);
        AppendTranslated(node, TrimSection.Exit);
      }
    }

    public virtual void Visit(SqlUnary node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Operand.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlMetadata node)
    {
      node.Expression.AcceptVisitor(this);
    }

    public virtual void Visit(SqlUpdate node)
    {
      VisitUpdateDefault(node);
    }

    public void VisitUpdateDefault(SqlUpdate node)
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

    public virtual void VisitUpdateEntry(SqlUpdate node)
    {
      AppendTranslated(node, UpdateSection.Entry);
    }

    public virtual void VisitUpdateUpdate(SqlUpdate node)
    {
      if (node.Update == null)
        throw new SqlCompilerException(Strings.ExTablePropertyIsNotSet);

      using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableAliasing)) {
        node.Update.AcceptVisitor(this);
      }
    }

    public virtual void VisitUpdateSet(SqlUpdate node)
    {
      AppendTranslated(node, UpdateSection.Set);

      using (context.EnterCollectionScope()) {
        foreach (ISqlLValue item in node.Values.Keys) {
          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          var tc = item as SqlTableColumn;
          if (!tc.IsNullReference() && tc.SqlTable != node.Update)
            throw new SqlCompilerException(string.Format(Strings.ExUnboundColumn, tc.Name));
          translator.TranslateIdentifier(context.Output, tc.Name);
          AppendTranslated(SqlNodeType.Equals);
          SqlExpression value = node.Values[item];
          value.AcceptVisitor(this);
        }
      }
    }

    public virtual void VisitUpdateFrom(SqlUpdate node)
    {
      if (Driver.ServerInfo.Query.Features.Supports(QueryFeatures.UpdateFrom) && node.From != null) {
        AppendTranslated(node, UpdateSection.From);
        node.From.AcceptVisitor(this);
      }
    }

    public virtual void VisitUpdateWhere(SqlUpdate node)
    {
      if (!node.Where.IsNullReference()) {
        AppendTranslated(node, UpdateSection.Where);
        node.Where.AcceptVisitor(this);
      }
    }

    public virtual void VisitUpdateLimit(SqlUpdate node)
    {
      if (!node.Limit.IsNullReference()) {
        if (!Driver.ServerInfo.Query.Features.Supports(QueryFeatures.UpdateLimit))
          throw new NotSupportedException(Strings.ExStorageIsNotSupportedLimitationOfRowCountToUpdate);
        AppendTranslated(node, UpdateSection.Limit);
        node.Limit.AcceptVisitor(this);
      }
    }

    public virtual void VisitUpdateExit(SqlUpdate node)
    {
      AppendTranslated(node, UpdateSection.Exit);
    }

    public virtual void Visit(SqlPlaceholder node)
    {
      context.Output.AppendPlaceholderWithId(node.Id);
      AppendSpaceIfNecessary();
    }

    public virtual void Visit(SqlUserColumn node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, NodeSection.Entry);
        node.Expression.AcceptVisitor(this);
        AppendTranslated(node, NodeSection.Exit);
      }
    }

    public virtual void Visit(SqlUserFunctionCall node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, FunctionCallSection.Entry, -1);
        if (node.Arguments.Count > 0) {
          using (context.EnterCollectionScope()) {
            int argumentPosition = 0;
            foreach (SqlExpression item in node.Arguments) {
              AppendCollectionDelimiterIfNecessary(() => AppendTranslated(node, FunctionCallSection.ArgumentDelimiter, argumentPosition++));
              item.AcceptVisitor(this);
            }
          }
        }
        AppendTranslated(node, FunctionCallSection.Exit, -1);
      }
    }

    public virtual void Visit(SqlVariable node)
    {
      AppendTranslated(node);
    }

    public virtual void Visit(SqlVariant node)
    {
      using (context.EnterMainVariantScope(node.Id))
        node.Main.AcceptVisitor(this);

      using (context.EnterAlternativeVariantScope(node.Id))
        node.Alternative.AcceptVisitor(this);
    }

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

    public virtual void Visit(TableColumn column)
    {
      AppendTranslated(column, TableColumnSection.Entry);
      if (column.Expression.IsNullReference()) {
        if (column.Domain == null)
          ArgumentValidator.EnsureArgumentNotNull(column.DataType, "DataType");
        AppendTranslated(column, TableColumnSection.Type);
      }
      if (column.Collation != null) {
        AppendTranslated(column, TableColumnSection.Collate);
      }
      if (!column.DefaultValue.IsNullReference()) {
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
      else if (!column.Expression.IsNullReference()) {
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

    private void Visit(TableConstraint constraint)
    {
      using (context.EnterScope(context.NamingOptions & ~SqlCompilerNamingOptions.TableQualifiedColumns)) {
        AppendTranslated(constraint, ConstraintSection.Entry);
        var checkConstraint = constraint as CheckConstraint;
        if (checkConstraint != null)
          VisitCheckConstraint(checkConstraint);
        var uniqueConstraint = constraint as UniqueConstraint;
        if (uniqueConstraint != null)
          VisitUniqueConstraint(uniqueConstraint);
        if (constraint is ForeignKey foreignKey) {
          VisitForeignKeyConstraint(foreignKey);
        }
        AppendTranslated(constraint, ConstraintSection.Exit);
      }
    }

    private void VisitForeignKeyConstraint(ForeignKey constraint)
    {
      AppendTranslated(constraint, ConstraintSection.ForeignKey);
      if (constraint.ReferencedColumns.Count == 0)
        throw new SqlCompilerException(Strings.ExReferencedColumnsCountCantBeLessThenOne);
      if (constraint.Columns.Count == 0)
        throw new SqlCompilerException(Strings.ExReferencingColumnsCountCantBeLessThenOne);
      using (context.EnterCollectionScope()) {
        foreach (TableColumn column in constraint.Columns) {
          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          AppendTranslated(column, TableColumnSection.Entry);
        }
      }

      AppendTranslated(constraint, ConstraintSection.ReferencedColumns);
      using (context.EnterCollectionScope()) {
        foreach (TableColumn tc in constraint.ReferencedColumns) {
          AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
          AppendTranslated(tc, TableColumnSection.Entry);
        }
      }
    }

    private void VisitUniqueConstraint(UniqueConstraint constraint)
    {
      if (constraint is PrimaryKey)
        AppendTranslated(constraint, ConstraintSection.PrimaryKey);
      else
        AppendTranslated(constraint, ConstraintSection.Unique);

      if (constraint.Columns.Count > 0)
        using (context.EnterCollectionScope())
          foreach (TableColumn tc in constraint.Columns) {
            AppendCollectionDelimiterIfNecessary(AppendColumnDelimiter);
            AppendTranslated(tc, TableColumnSection.Entry);
          }
    }

    private void VisitCheckConstraint(CheckConstraint constraint)
    {
      AppendTranslated(constraint, ConstraintSection.Check);
      constraint.Condition.AcceptVisitor(this);
    }

    public virtual void Visit(SqlExtract node)
    {
      using (context.EnterScope(node)) {
        AppendTranslated(node, ExtractSection.Entry);
        var part = node.DateTimePart != SqlDateTimePart.Nothing
          ? translator.Translate(node.DateTimePart)
          : node.IntervalPart != SqlIntervalPart.Nothing
            ? translator.Translate(node.IntervalPart)
            : translator.Translate(node.DateTimeOffsetPart);
        context.Output.Append(part);
        AppendSpaceIfNecessary();
        AppendTranslated(node, ExtractSection.From);
        node.Operand.AcceptVisitor(this);
        AppendTranslated(node, ExtractSection.Exit);
      }
    }

    public virtual void Visit(SqlComment node)
    {
      translator.Translate(context.Output, node);
    }

    public virtual void VisitCommentIfBefore(SqlComment node)
    {
      if (configuration.CommentLocation != SqlCommentLocation.BeforeStatement)
        return;
      Visit(node);
      context.Output.Append(translator.NewLine);
    }

    public virtual void VisitCommentIfWithin(SqlComment node)
    {
      if (configuration.CommentLocation != SqlCommentLocation.WithinStatement)
        return;
      Visit(node);
    }

    public virtual void VisitCommentIfAfter(SqlComment node)
    {
      if (configuration.CommentLocation != SqlCommentLocation.AfterStatement)
        return;
      context.Output.Append(translator.NewLine);
      Visit(node);
    }

    private void TranslateDynamicFilterViaInOperator(SqlDynamicFilter node)
    {
      int numberOfExpressions = node.Expressions.Count;
      bool isMulticolumn = numberOfExpressions > 1;
      string delimiter = translator.RowItemDelimiter + " ";
      context.Output.Append(translator.OpeningParenthesis);
      SqlExpression filteredExpression = isMulticolumn ? SqlDml.Row(node.Expressions) : node.Expressions[0];
      filteredExpression.AcceptVisitor(this);
      AppendTranslated(SqlNodeType.In);
      context.Output.Append(translator.RowBegin);
      using (context.EnterCycleBodyScope(node.Id, delimiter)) {
        if (isMulticolumn) {
          context.Output.Append(translator.RowBegin);
          for (int i = 0; i < numberOfExpressions; i++) {
            context.Output.AppendCycleItem(i);
            if (i != numberOfExpressions - 1) {
              AppendRowItemDelimiter();
            }
          }
          context.Output.Append(translator.RowEnd);
        }
        else
          context.Output.AppendCycleItem(0);
      }
      using (context.EnterCycleEmptyCaseScope(node.Id)) {
        SqlExpression nullExpression = isMulticolumn
          ? SqlDml.Row(Enumerable.Repeat(SqlDml.Null, numberOfExpressions).ToArray())
          : (SqlExpression) SqlDml.Null;
        nullExpression.AcceptVisitor(this);
        // Append "and false" to avoid result beeing "undefined" rather than "false"
        context.Output.Append(translator.ClosingParenthesis);
        AppendTranslated(SqlNodeType.And);
        context.Output.Append(translator.OpeningParenthesis);
        WriteFalseExpression();
      }
      context.Output.Append(translator.RowEnd);
      context.Output.Append(translator.ClosingParenthesis);
    }

    private void TranslateDynamicFilterViaMultipleComparisons(SqlDynamicFilter node)
    {
      var expressions = node.Expressions;
      int numberOfExpressions = expressions.Count;
      string delimiter = " " + translator.Translate(SqlNodeType.Or) + " ";
      using (context.EnterCycleBodyScope(node.Id, delimiter)) {
        context.Output.Append(translator.OpeningParenthesis);
        for (int i = 0; i < numberOfExpressions; i++) {
          expressions[i].AcceptVisitor(this);
          AppendTranslated(SqlNodeType.Equals);
          context.Output.AppendCycleItem(i);
          if (i != numberOfExpressions - 1) {
            AppendSpaceIfNecessary();
            AppendTranslated(SqlNodeType.And);
          }
        }
        context.Output.Append(translator.ClosingParenthesis);
      }
      using (context.EnterCycleEmptyCaseScope(node.Id)) {
        WriteFalseExpression();
      }
    }

    protected void AppendDelimiter(string text, SqlDelimiterType type = SqlDelimiterType.Row)
    {
      switch (type) {
        case SqlDelimiterType.Column:
          context.Output.Append(translator.NewLine);
          context.Output.AppendIndent();
          break;
      }
      context.Output.Append(text);
      AppendSpaceIfNecessary();
    }


    protected void AppendColumnDelimiter()
    {
      AppendDelimiter(translator.ColumnDelimiter);
    }

    protected void AppendDdlStatementDelimiter()
    {
      AppendDelimiter(translator.DdlStatementDelimiter, SqlDelimiterType.Column);
    }

    protected void AppendRowItemDelimiter()
    {
      AppendDelimiter(translator.RowItemDelimiter);
    }

    protected void AppendSpaceIfNecessary()
    {
      context.Output.AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlNodeType nodeType)
    {
      AppendSpaceIfNecessary();
      context.Output.Append(translator.Translate(nodeType));
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlAggregate node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlArray node, ArraySection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlBetween node, BetweenSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlBinary node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlBreak node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCase node, CaseSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
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
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlAlterDomain node, AlterDomainSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlAlterSequence node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlAlterTable node, AlterTableSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCloseCursor node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCollate node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlColumnRef node, ColumnSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlConcat node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(TableColumn column, TableColumnSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, column, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(Constraint constraint, ConstraintSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, constraint, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCommand node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlContinue node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCreateAssertion node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCreateDomain node, CreateDomainSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCreateIndex node, CreateIndexSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCreatePartitionFunction node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, descriptor, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlAlterPartitionFunction node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCreatePartitionScheme node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCreateSchema node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCreateSequence node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCreateTable node, CreateTableSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCreateView node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlCursor node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDeclareCursor node, DeclareCursorSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDeclareVariable node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDefaultValue node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDelete node, DeleteSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropAssertion node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropCharacterSet node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropCollation node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropDomain node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropIndex node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropPartitionFunction node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropPartitionScheme node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropSchema node)
    {
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropSequence node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropTable node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropTranslation node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlDropView node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlFetch node, FetchSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlFunctionCall node, FunctionCallSection section, int position)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section, position);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlUserFunctionCall node, FunctionCallSection section, int position)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section, position);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlIf node, IfSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlInsert node, InsertSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlJoinExpression node, JoinSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlLike node, LikeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlMatch node, MatchSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlNull node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlNextValue node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SchemaNode node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlOpenCursor node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlOrder node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlQueryRef node, TableSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlQueryExpression node, QueryExpressionSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlRow node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlRowNumber node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlRenameTable node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlSelect node, SelectSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlStatementBlock node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlSubQuery node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlTableColumn node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlTableRef node, TableSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlTrim node, TrimSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlUnary node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlUpdate node, UpdateSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlUserColumn node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlVariable node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
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

    protected void AppendTranslated(SqlAlterPartitionScheme node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlNative node)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslated(SqlAssignment node, NodeSection section)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, node, section);
      AppendSpaceIfNecessary();
    }

    protected void AppendTranslatedLiteral(object literalValue)
    {
      AppendSpaceIfNecessary();
      translator.Translate(context, literalValue);
      AppendSpaceIfNecessary();
    }

    private void WriteFalseExpression()
    {
      SqlDml.Equals(SqlDml.Literal(1), SqlDml.Literal(0)).AcceptVisitor(this);
    }

    private static IEnumerable<SqlStatement> FlattenBatch(SqlBatch batch)
    {
      foreach (SqlStatement statement in batch) {
        if (statement is SqlBatch nestedBatch) {
          foreach (var nestedStatement in FlattenBatch(nestedBatch))
            yield return nestedStatement;
        }
        else {
          yield return statement;
        }
      }
    }

    protected bool CheckFeature(ColumnFeatures features)
    {
      return Driver.ServerInfo.Column.Features.Supports(features);
    }

    protected bool CheckFeature(IndexFeatures features)
    {
      return Driver.ServerInfo.Index.Features.Supports(features);
    }

    protected bool CheckFeature(QueryFeatures features)
    {
      return Driver.ServerInfo.Query.Features.Supports(features);
    }

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
    }
  }
}
