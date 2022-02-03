// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;
using Xtensive.Sorting;
using Xtensive.Modelling;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Internals;
using Xtensive.Sql;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Orm.Upgrade.Model;
using Activator = System.Activator;
using Index = Xtensive.Sql.Model.Index;
using ReferentialAction = Xtensive.Sql.ReferentialAction;
using TableInfo = Xtensive.Orm.Upgrade.Model.TableInfo;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Translates upgrade <see cref="NodeAction"/>s to SQL.
  /// </summary>
  internal sealed class SqlActionTranslator
  {
    private const string SubqueryAliasName = "th";
    private const string TemporaryNameFormat = "Temp{0}";
    private const string SubqueryTableAliasNameFormat = "a{0}";
    private const string ColumnTypePropertyName = "Type";
    private const string ColumnDefaultPropertyName = "Default";

    private static readonly StringComparer StringComparer = StringComparer.OrdinalIgnoreCase;

    private readonly ProviderInfo providerInfo;
    private readonly string typeIdColumnName;
    private readonly List<string> enforceChangedColumns = new List<string>();
    private readonly ISqlExecutor sqlExecutor;
    private readonly bool allowCreateConstraints;

    private readonly string collationName;
    private readonly ActionSequence actions;
    private readonly SchemaExtractionResult sqlModel;
    private readonly StorageModel sourceModel;
    private readonly StorageModel targetModel;
    private readonly StorageDriver driver;
    private readonly SequenceQueryBuilder sequenceQueryBuilder;
    private readonly MappingResolver resolver;

    private readonly List<Table> createdTables = new List<Table>();
    private readonly List<Sequence> createdSequences = new List<Sequence>();
    private readonly List<DataAction> clearDataActions = new List<DataAction>();
    private readonly HashSet<TableColumn> recreatedColumns = new HashSet<TableColumn>();
    private readonly Dictionary<string, SequenceDescriptor> removedGeneratorDescriptors = new Dictionary<string, SequenceDescriptor>();


    private UpgradeActionSequenceBuilder currentOutput;

    private bool translated;

    /// <summary>
    /// Translates all registered actions.
    /// </summary>
    public UpgradeActionSequence Translate()
    {
      if (translated)
        throw new InvalidOperationException(Strings.ExCommandsAreAlreadyTranslated);

      translated = true;
      currentOutput = new UpgradeActionSequenceBuilder(driver, new UpgradeActionSequence(), SqlUpgradeStage.PreCleanupData);

      // Turn off deferred contraints
      if (providerInfo.Supports(ProviderFeatures.DeferrableConstraints))
        currentOutput.RegisterCommand(SqlDdl.Command(SqlCommandType.SetConstraintsAllImmediate));

      // Data cleanup
      ProcessActions(Modelling.Comparison.UpgradeStage.CleanupData, SqlUpgradeStage.CleanupData);
      ProcessClearDataActions(false);

      // Prepairing (aka запаривание :-)
      ProcessActions(Modelling.Comparison.UpgradeStage.Prepare, SqlUpgradeStage.PreUpgrade);

      // Mutual renaming
      ProcessActions(Modelling.Comparison.UpgradeStage.TemporaryRename, SqlUpgradeStage.PreUpgrade);

      // Upgrading
      ProcessActions(Modelling.Comparison.UpgradeStage.Upgrade, SqlUpgradeStage.Upgrade);

      // Copying data
      ProcessActions(Modelling.Comparison.UpgradeStage.CopyData, SqlUpgradeStage.CopyData);

      // Post copying data
      ProcessActions(Modelling.Comparison.UpgradeStage.PostCopyData, SqlUpgradeStage.PostCopyData);
      ProcessClearDataActions(true);

      // Cleanup
      ProcessActions(Modelling.Comparison.UpgradeStage.Cleanup, SqlUpgradeStage.Cleanup);

      // Turn on deferred contraints
      if (providerInfo.Supports(ProviderFeatures.DeferrableConstraints))
        currentOutput.RegisterCommand(SqlDdl.Command(SqlCommandType.SetConstraintsAllDeferred));

      return currentOutput.Result;
    }

    private void VisitAction(NodeAction action)
    {
      if (action is GroupingNodeAction groupingNodeAction) {
        foreach (var nodeAction in groupingNodeAction.Actions)
          VisitAction(nodeAction);
      }
      else if (action is CreateNodeAction createNodeAction)
        VisitCreateAction(createNodeAction);
      else if (action is RemoveNodeAction removeNodeAction)
        VisitRemoveAction(removeNodeAction);
      else if (action is DataAction dataAction)
        VisitDataAction(dataAction);
      else
        VisitAlterAction(action);
    }

    private void VisitCreateAction(CreateNodeAction action)
    {
      if (action.Type == WellKnownUpgradeTypes.TableInfo)
        VisitCreateTableAction(action);
      else if (action.Type == WellKnownUpgradeTypes.StorageColumnInfo)
        VisitCreateColumnAction(action);
      else if (action.Type == WellKnownUpgradeTypes.PrimaryIndexInfo)
        VisitCreatePrimaryKeyAction(action);
      else if (action.Type == WellKnownUpgradeTypes.SecondaryIndexInfo)
        VisitCreateSecondaryIndexAction(action);
      else if (action.Type == WellKnownUpgradeTypes.ForeignKeyInfo)
        VisitCreateForeignKeyAction(action);
      else if (action.Type == WellKnownUpgradeTypes.StorageSequenceInfo)
        VisitCreateSequenceAction(action);
      else if (action.Type == WellKnownUpgradeTypes.StorageFullTextIndexInfo)
        VisitCreateFullTextIndexAction(action);
    }

    private void VisitRemoveAction(RemoveNodeAction action)
    {
      var node = action.Difference.Source;
      if (node is TableInfo)
        VisitRemoveTableAction(action);
      else if (node is StorageColumnInfo)
        VisitRemoveColumnAction(action);
      else if (node is PrimaryIndexInfo)
        VisitRemovePrimaryKeyAction(action);
      else if (node is SecondaryIndexInfo)
        VisitRemoveSecondaryIndexAction(action);
      else if (node is ForeignKeyInfo)
        VisitRemoveForeignKeyAction(action);
      else if (node is StorageSequenceInfo)
        VisitRemoveSequenceAction(action);
      else if (node is StorageFullTextIndexInfo)
        VisitRemoveFullTextIndexAction(action);
    }

    private void VisitAlterAction(NodeAction action)
    {
      if (action is PropertyChangeAction propertyChangeAction) {
        var changedNode = targetModel.Resolve(propertyChangeAction.Path, true);
        if (changedNode is StorageSequenceInfo)
          VisitAlterSequenceAction(propertyChangeAction);
        else if (changedNode is StorageColumnInfo)
          VisitAlterColumnAction(propertyChangeAction);
        return;
      }

      if (!(action is MoveNodeAction moveNodeAction))
        return;
      var node = moveNodeAction.Difference.Source;
      if (node is TableInfo)
        VisitAlterTableAction(moveNodeAction);
      else if (node is StorageColumnInfo)
        VisitMoveColumnAction(moveNodeAction);
      else if (node is PrimaryIndexInfo)
        VisitAlterPrimaryKeyAction(moveNodeAction);
      else if (node is SecondaryIndexInfo)
        VisitAlterSecondaryIndexAction(moveNodeAction);
      else if (node is ForeignKeyInfo)
        VisitAlterForeignKeyAction(moveNodeAction);
    }

    private void VisitDataAction(DataAction dataAction)
    {
      var hint = dataAction.DataHint;
      var copyDataHint = hint as CopyDataHint;
      if (copyDataHint != null) {
        VisitCopyDataAction(dataAction);
        return;
      }
      var deleteDataHint = hint as DeleteDataHint;
      if (deleteDataHint != null) {
        VisitDeleteDataAction(dataAction);
        return;
      }
      var updateDataHint = hint as UpdateDataHint;
      if (updateDataHint != null) {
        VisitUpdateDataAction(dataAction);
        return;
      }
    }

    /// <exception cref="InvalidOperationException">Can not create copy command
    /// with specific hint parameters.</exception>
    private void VisitCopyDataAction(DataAction action)
    {
      var hint = (CopyDataHint) action.DataHint;
      var copiedColumns = hint.CopiedColumns
        .Select(pair => new Pair<StorageColumnInfo>(
          sourceModel.Resolve(pair.First, true) as StorageColumnInfo,
          targetModel.Resolve(pair.Second, true) as StorageColumnInfo)).ToArray();
      var identityColumns = hint.Identities
        .Select(pair => new Pair<StorageColumnInfo>(
          sourceModel.Resolve(pair.Source, true) as StorageColumnInfo,
          targetModel.Resolve(pair.Target, true) as StorageColumnInfo)).ToArray();
      if (copiedColumns.Length == 0 || identityColumns.Length == 0)
        throw new InvalidOperationException(Strings.ExIncorrectCommandParameters);

      var fromTable = FindTable(copiedColumns[0].First.Parent);
      var toTable = FindTable(copiedColumns[0].Second.Parent);
      var toTableRef = SqlDml.TableRef(toTable);
      var update = SqlDml.Update(toTableRef);

      if (fromTable == toTable) {
        copiedColumns.ForEach(pair => update.Values[toTableRef[pair.Second.Name]] = toTableRef[pair.First.Name]);
        currentOutput.RegisterCommand(update);
        return;
      }

      if (providerInfo.Supports(ProviderFeatures.UpdateFrom)) {
        var fromTableRef = SqlDml.TableRef(fromTable);
        var select = SqlDml.Select(fromTableRef);
        identityColumns.ForEach(pair => select.Columns.Add(fromTableRef[pair.First.Name]));
        copiedColumns.ForEach(pair => select.Columns.Add(fromTableRef[pair.First.Name]));
        var selectRef = SqlDml.QueryRef(select, SubqueryAliasName);
        update.From = selectRef;
        copiedColumns.ForEach(pair => update.Values[toTableRef[pair.Second.Name]] = selectRef[pair.First.Name]);
        identityColumns.ForEach(pair => update.Where &= toTableRef[pair.Second.Name]==selectRef[pair.First.Name]);
      }
      else {
        foreach (var columnPair in copiedColumns) {
          var fromTableRef = SqlDml.TableRef(fromTable);
          var select = SqlDml.Select(fromTableRef);
          foreach (var identityColumnPair in identityColumns)
            select.Where &= toTableRef[identityColumnPair.Second.Name]==fromTableRef[identityColumnPair.First.Name];
          select.Columns.Add(fromTableRef[columnPair.First.Name]);
          update.Values[toTableRef[columnPair.Second.Name]] = select;
          update.Where = SqlDml.Exists(select);
        }
      }
      currentOutput.RegisterCommand(update);
    }

    private void VisitDeleteDataAction(DataAction action)
    {
      clearDataActions.Add(action);
    }

    private void VisitUpdateDataAction(DataAction action)
    {
      clearDataActions.Add(action);
    }

    private void VisitCreateTableAction(CreateNodeAction action)
    {
      var tableInfo = action.Difference.Target as TableInfo;
      var table = CreateTable(tableInfo);
      currentOutput.RegisterCommand(SqlDdl.Create(table));
    }

    private void VisitRemoveTableAction(RemoveNodeAction action)
    {
      var tableInfo = (TableInfo) action.Difference.Source;
      var table = FindTable(tableInfo);
      if (sqlModel.LockedTables.TryGetValue(resolver.GetNodeName(table), out var lockedTable)) {
        throw new SchemaSynchronizationException(lockedTable);
      }
      currentOutput.RegisterCommand(SqlDdl.Drop(table));

      if (providerInfo.Supports(ProviderFeatures.Sequences) && IsGeneratorTable(table, out var sequenceDescriptor)) {
        sequenceDescriptor.LastValue = GetLastValueOfTableBasedSequenceQuery(table);
        removedGeneratorDescriptors.Add(resolver.GetNodeName(table), sequenceDescriptor);
      }
      _ = table.Schema.Tables.Remove(table);
    }

    private void VisitAlterTableAction(MoveNodeAction action)
    {
      var oldTableInfo = (TableInfo) sourceModel.Resolve(action.Path, true);
      var oldTable = FindTable(oldTableInfo);

      var newTableNode = resolver.Resolve(sqlModel, action.Name);
      var canRename =
        providerInfo.Supports(ProviderFeatures.TableRename)
        && newTableNode.Schema==oldTable.Schema;

      if (canRename)
        currentOutput.RegisterCommand(SqlDdl.Rename(oldTable, newTableNode.Name));
      else
        RecreateTableWithNewName(oldTable, newTableNode.Schema, newTableNode.Name);

      oldTableInfo.Name = action.Name;

      RenameSchemaTable(oldTable, oldTable.Schema, newTableNode.Schema, newTableNode.Name);
    }

    private void RecreateTableWithNewName(Table oldTable, Schema newSchema, string newName)
    {
      string lockedTable;
      sqlModel.LockedTables.TryGetValue(resolver.GetNodeName(oldTable), out lockedTable);
      if (!lockedTable.IsNullOrEmpty())
        throw new SchemaSynchronizationException(lockedTable);
      var newTable = newSchema.CreateTable(newName);
      // Clone table definition
      foreach (var oldColumn in oldTable.TableColumns) {
        var newColumn = newTable.CreateColumn(oldColumn.Name, oldColumn.DataType);
        newColumn.DbName = oldColumn.DbName;
        newColumn.IsNullable = oldColumn.IsNullable;
        if (oldColumn.DefaultValue!=null && (oldColumn.DefaultValue is SqlLiteral<string> || oldColumn.DefaultValue is SqlLiteral<char>)) {
          var stringLiteral = oldColumn.DefaultValue as SqlLiteral<string>;
          if (stringLiteral!=null)
            newColumn.DefaultValue = SqlDml.Literal(TryUnquoteLiteral(stringLiteral.GetValue().ToString()));

          var charLiteral = oldColumn.DefaultValue as SqlLiteral<char>;
          if (charLiteral!=null) {
            var unquotedLiteral = TryUnquoteLiteral(charLiteral.GetValue().ToString());
            newColumn.DefaultValue = SqlDml.Literal(string.IsNullOrEmpty(unquotedLiteral) ? '\0' : Convert.ToChar(unquotedLiteral, CultureInfo.InvariantCulture));
          }
        }
        else {
          newColumn.DefaultValue = oldColumn.DefaultValue;
        }
        recreatedColumns.Add(oldColumn);
      }

      // Clone primary key
      var oldPrimaryKey = oldTable.TableConstraints.OfType<PrimaryKey>().FirstOrDefault();
      if (oldPrimaryKey!=null) {
        var newPrimaryKey = newTable.CreatePrimaryKey(oldPrimaryKey.Name);
        newPrimaryKey.IsClustered = oldPrimaryKey.IsClustered;
        foreach (var oldColumn in oldPrimaryKey.Columns)
          newPrimaryKey.Columns.Add(newTable.TableColumns[oldColumn.Name]);
      }

      // Skip cloning FKs:
      // Only SQLite can not add FK via separate statement and FKs are disabled for it
      // So there is not reason to clone FKs here

      currentOutput.RegisterCommand(SqlDdl.Create(newTable));

      // Copying data from one table to another
      var oldTableRef = SqlDml.TableRef(oldTable);
      var select = SqlDml.Select(oldTableRef);

      foreach (var item in oldTable.Columns)
        select.Columns.Add(oldTableRef[item.Name]);

      var insert = SqlDml.Insert(SqlDml.TableRef(newTable));
      insert.From = select;
      currentOutput.RegisterCommand(insert);

      // Removing table
      currentOutput.RegisterCommand(SqlDdl.Drop(oldTable));

      newTable.Schema.Tables.Remove(newTable);
    }

    private void VisitCreateColumnAction(CreateNodeAction createColumnAction)
    {
      var columnInfo = (StorageColumnInfo) createColumnAction.Difference.Target;
      var table = FindTable(columnInfo.Parent);

      // Ensure table is not newly created
      if (createdTables.Contains(table))
        return;

      var column = CreateColumn(columnInfo, table);
      if (columnInfo.DefaultValue != null)
        column.DefaultValue = SqlDml.Literal(columnInfo.DefaultValue);
      if (!string.IsNullOrEmpty(columnInfo.DefaultSqlExpression))
        column.DefaultValue = SqlDml.Native(columnInfo.DefaultSqlExpression);
      currentOutput.RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddColumn(column)));
    }

    private void VisitRemoveColumnAction(RemoveNodeAction removeColumnAction)
    {
      var columnInfo = (StorageColumnInfo) removeColumnAction.Difference.Source;

      // Ensure table is not removed
      var table = FindTable(columnInfo.Parent);
      if (table==null)
        return;

      var column = FindColumn(table, columnInfo.Name);
      if (column==null)
        return;

      DropColumn(column, currentOutput);
    }

    private void DropColumn(TableColumn column, UpgradeActionSequenceBuilder commandOutput)
    {
      DropDefaultConstraint(column, commandOutput);

      var table = column.Table;

      if (providerInfo.Supports(ProviderFeatures.ColumnDrop)) {
        commandOutput.RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropColumn(column)));
        table.TableColumns.Remove(column);
        return;
      }

      // Save original and temporary names
      var name = table.Name;
      var tempName = GetTemporaryName(table);

      // Recreate table without dropped column
      table.TableColumns.Remove(column);
      RecreateTableWithNewName(table, table.Schema, tempName);
      RenameSchemaTable(table, table.Schema, table.Schema, tempName);

      // Rename table back to original name
      if (providerInfo.Supports(ProviderFeatures.TableRename))
        currentOutput.RegisterCommand(SqlDdl.Rename(table, name));
      else
        RecreateTableWithNewName(table, table.Schema, name);
      RenameSchemaTable(table, table.Schema, table.Schema, name);
    }

    private void DropDefaultConstraint(TableColumn column, UpgradeActionSequenceBuilder commandOutput)
    {
      if (column.DefaultValue.IsNullReference())
        return;
      var table = column.Table;
      var constraint = table.TableConstraints.OfType<DefaultConstraint>().FirstOrDefault(c => c.Column==column);
      if (constraint==null)
        return;
      if (recreatedColumns.Contains(column))
        constraint.NameIsStale = true;
      commandOutput.RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(constraint)));
    }

    private void VisitAlterColumnAction(PropertyChangeAction action)
    {
      var isNewlyCreatedColumn = action.Difference.Source==null;
      if (isNewlyCreatedColumn)
        return; // Properties already initialized

      if (!action.Properties.ContainsKey(ColumnTypePropertyName))
        if (!action.Properties.ContainsKey(ColumnDefaultPropertyName))
          return;

      ChangeColumnType(action);
    }

    private void VisitMoveColumnAction(MoveNodeAction action)
    {
      var movementInfo = ((NodeDifference) action.Difference).MovementInfo;
      if ((movementInfo & MovementInfo.NameChanged)!=0) {
        // Process name changing
        var oldColumnInfo = (StorageColumnInfo) sourceModel.Resolve(action.Path, true);
        var column = FindColumn(oldColumnInfo.Parent, oldColumnInfo.Name);
        RenameColumn(column, action.Name);
        oldColumnInfo.Name = action.Name;
      }
      else
        throw new NotSupportedException();
    }

    private void RenameColumn(TableColumn column, string name)
    {
      var upgradeOutput = currentOutput.ForStage(SqlUpgradeStage.Upgrade);

      if (providerInfo.Supports(ProviderFeatures.ColumnRename)) {
        upgradeOutput.RegisterCommand(SqlDdl.Rename(column, name));
        RenameSchemaColumn(column, name);
        return;
      }

      var table = column.Table;
      var originalName = column.Name;

      // Create new column
      var newColumn = table.CreateColumn(name, column.DataType);
      newColumn.IsNullable = column.IsNullable;
      newColumn.DefaultValue = column.DefaultValue;
      var addColumnWithNewType = SqlDdl.Alter(column.Table, SqlDdl.AddColumn(newColumn));
      upgradeOutput.RegisterCommand(addColumnWithNewType);

      // Copy data
      var tableRef = SqlDml.TableRef(column.Table);
      var update = SqlDml.Update(tableRef);
      update.Values[tableRef[name]] = tableRef[originalName];
      upgradeOutput.RegisterCommand(update);

      // Drop old column
      DropColumn(column, upgradeOutput);
    }

    private void VisitCreatePrimaryKeyAction(CreateNodeAction action)
    {
      var primaryIndex = (PrimaryIndexInfo) action.Difference.Target;
      var table = FindTable(primaryIndex.Parent);

      // Ensure table is not newly created
      if (createdTables.Contains(table))
        return;

      var primaryKey = CreatePrimaryKey(primaryIndex.Parent, table);
      currentOutput.RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddConstraint(primaryKey)));
    }

    private void VisitRemovePrimaryKeyAction(RemoveNodeAction action)
    {
      var primaryIndexInfo = (PrimaryIndexInfo) action.Difference.Source;
      var table = FindTable(primaryIndexInfo.Parent);

      // Ensure table is not removed
      if (table==null)
        return;

      var primaryKey = table.TableConstraints[primaryIndexInfo.Name];

      currentOutput.RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(primaryKey)));
      table.TableConstraints.Remove(primaryKey);
    }

    private void VisitAlterPrimaryKeyAction(NodeAction action)
    {
      throw new NotSupportedException();
    }

    private void VisitCreateSecondaryIndexAction(CreateNodeAction action)
    {
      var secondaryIndexInfo = (SecondaryIndexInfo) action.Difference.Target;
      var table = FindTable(secondaryIndexInfo.Parent);
      var index = CreateSecondaryIndex(table, secondaryIndexInfo);
      if (index.IsUnique && !allowCreateConstraints)
        return;
      currentOutput.RegisterCommand(SqlDdl.Create(index));
    }

    private void VisitRemoveSecondaryIndexAction(RemoveNodeAction action)
    {
      var preCleanupDataOutput = currentOutput.ForStage(SqlUpgradeStage.PreCleanupData);

      var secondaryIndexInfo = (SecondaryIndexInfo) action.Difference.Source;
      var table = FindTable(secondaryIndexInfo.Parent);

      // Ensure table is not removed
      if (table==null)
        return;

      var index = table.Indexes[secondaryIndexInfo.Name];
      preCleanupDataOutput.RegisterCommand(SqlDdl.Drop(index));
      table.Indexes.Remove(index);
    }

    private void VisitAlterSecondaryIndexAction(NodeAction action)
    {
      throw new NotSupportedException();
    }

    private void VisitCreateForeignKeyAction(CreateNodeAction action)
    {
      if (!allowCreateConstraints)
        return;

      var foreignKeyInfo = (ForeignKeyInfo) action.Difference.Target;
      var table = FindTable(foreignKeyInfo.Parent);
      var foreignKey = CreateForeignKey(foreignKeyInfo);

      currentOutput.RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddConstraint(foreignKey)));
    }

    private void VisitRemoveForeignKeyAction(RemoveNodeAction action)
    {
      var preCleanupDataOutput = currentOutput.ForStage(SqlUpgradeStage.PreCleanupData);

      var foreignKeyInfo = (ForeignKeyInfo) action.Difference.Source;
      var table = FindTable(foreignKeyInfo.Parent);

      // Ensure table is not removed
      if (table==null)
        return;

      var foreignKey = table.TableConstraints[foreignKeyInfo.Name];
      preCleanupDataOutput.RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(foreignKey)));
      table.TableConstraints.Remove(foreignKey);
    }

    private void VisitAlterForeignKeyAction(NodeAction action)
    {
      throw new NotSupportedException();
    }

    private void VisitCreateSequenceAction(CreateNodeAction action)
    {
      var sequenceInfo = (StorageSequenceInfo) action.Difference.Target;
      if (providerInfo.Supports(ProviderFeatures.Sequences)) {
        var node = resolver.Resolve(sqlModel, sequenceInfo.Name);
        var sequence = node.Schema.CreateSequence(node.Name);

        if (!removedGeneratorDescriptors.TryGetValue(sequenceInfo.Name, out var oldDescriptor)) {
          var descriptor = new SequenceDescriptor(sequence, sequenceInfo.Seed, sequenceInfo.Increment) {
            MinValue = sequenceInfo.Seed,
          };
          sequence.SequenceDescriptor = descriptor;
          currentOutput.RegisterCommand(SqlDdl.Create(sequence));
          createdSequences.Add(sequence);
          return;
        }

        oldDescriptor.StartValue = oldDescriptor.LastValue;
        sequence.SequenceDescriptor = oldDescriptor;
        currentOutput.RegisterCommand(SqlDdl.Create(sequence));
        createdSequences.Add(sequence);
      }
      else {
        CreateGeneratorTable(sequenceInfo);
      }
    }

    private void VisitRemoveSequenceAction(RemoveNodeAction action)
    {
      var sequenceInfo = (StorageSequenceInfo) action.Difference.Source;
      if (providerInfo.Supports(ProviderFeatures.Sequences)) {
        var node = resolver.Resolve(sqlModel, sequenceInfo.Name);
        var sequence = node.GetSequence();
        currentOutput.RegisterCommand(SqlDdl.Drop(sequence));
        node.Schema.Sequences.Remove(sequence);
      }
      else {
        DropGeneratorTable(sequenceInfo);
      }
    }

    private void VisitAlterSequenceAction(PropertyChangeAction action)
    {
      var sequenceInfo = (StorageSequenceInfo) targetModel.Resolve(action.Path, true);
      var node = resolver.Resolve(sqlModel, sequenceInfo.Name);

      // Check if sequence is not newly created
      if (IsNewlyCreatedSequence(node))
        return;

      var currentValue = GetCurrentSequenceValue(sequenceInfo);
      var newStartValue = currentValue + sequenceInfo.Increment;

      if (providerInfo.Supports(ProviderFeatures.Sequences)) {
        var exisitingSequence = node.GetSequence();
        var newSequenceDescriptor = new SequenceDescriptor(exisitingSequence, null, sequenceInfo.Increment);
        newSequenceDescriptor.LastValue = currentValue;
        exisitingSequence.SequenceDescriptor = newSequenceDescriptor;
        currentOutput.RegisterCommand(SqlDdl.Alter(exisitingSequence, newSequenceDescriptor));
      }
      else {
        sequenceInfo.Current = newStartValue;
        DropGeneratorTable(sequenceInfo);
        CreateGeneratorTable(sequenceInfo);
      }
    }

    private bool IsNewlyCreatedSequence(MappingResolveResult node)
    {
      if (providerInfo.Supports(ProviderFeatures.Sequences))
        return createdSequences.Any(s => s.Name==node.Name && s.Schema==node.Schema);
      else
        return createdTables.Any(t => t.Name==node.Name && t.Schema==node.Schema);
    }

    private void VisitCreateFullTextIndexAction(CreateNodeAction action)
    {
      var fullTextIndexInfo = (StorageFullTextIndexInfo) action.Difference.Target;
      var fullTextSupported = providerInfo.Supports(ProviderFeatures.FullText);
      if (!fullTextSupported)
        return;
      var table = FindTable(fullTextIndexInfo.Parent);
      var ftIndex = table.CreateFullTextIndex(fullTextIndexInfo.Name);
      ftIndex.UnderlyingUniqueIndex = fullTextIndexInfo.Parent.PrimaryIndex.EscapedName;
      ftIndex.FullTextCatalog = fullTextIndexInfo.FullTextCatalog;
      ftIndex.ChangeTrackingMode = ConvertFullTextChangeTrackingMode(fullTextIndexInfo.ChangeTrackingMode);

      foreach (var column in fullTextIndexInfo.Columns) {
        var tableColumn = FindColumn(table, column.Value.Name);
        var ftColumn = ftIndex.CreateIndexColumn(tableColumn);
        ftColumn.TypeColumn = (!column.TypeColumnName.IsNullOrEmpty() && providerInfo.Supports(ProviderFeatures.FullTextColumnDataTypeSpecification))
          ? FindColumn(table, column.TypeColumnName)
          : null;
        ftColumn.Languages.Add(new Language(column.Configuration));
      }

      var fullTextOutput = !providerInfo.Supports(ProviderFeatures.TransactionalFullTextDdl)
        ? currentOutput.ForStage(SqlUpgradeStage.NonTransactionalEpilog)
        : currentOutput;
      fullTextOutput.RegisterCommand(SqlDdl.Create(ftIndex));
    }

    private void VisitRemoveFullTextIndexAction(RemoveNodeAction action)
    {
      var fullTextIndexInfo = (StorageFullTextIndexInfo) action.Difference.Source;
      var fullTextSupported = providerInfo.Supports(ProviderFeatures.FullText);
      if (!fullTextSupported)
        return;
      var table = FindTable(fullTextIndexInfo.Parent);
      var ftIndex = table.Indexes[fullTextIndexInfo.Name] ?? table.Indexes.OfType<FullTextIndex>().Single();
      var fullTextOutput = !providerInfo.Supports(ProviderFeatures.TransactionalFullTextDdl)
        ? currentOutput.ForStage(SqlUpgradeStage.NonTransactionalProlog)
        : currentOutput;
      fullTextOutput.RegisterCommand(SqlDdl.Drop(ftIndex));
      table.Indexes.Remove(ftIndex);
    }

    private void ProcessClearDataActions(bool postCopy)
    {
      var updateActions = (
        from action in clearDataActions
        let updateDataHint = action.DataHint as UpdateDataHint
        where updateDataHint!=null
        select action
        ).ToList();
      var deleteActions = (
        from action in clearDataActions
        let deleteDataHint = action.DataHint as DeleteDataHint
        where deleteDataHint!=null && deleteDataHint.IsPostCopyCleanup==postCopy
        select action
        ).ToList();

      var deleteFromConnectorTableActions = new List<DataAction>();
      var deleteFromAncestorTableActions = new List<DataAction>();
      foreach (var deleteAction in deleteActions) {
        var hint = deleteAction.DataHint as DeleteDataHint;
        if (hint.Identities.Any(pair => !pair.IsIdentifiedByConstant))
          deleteFromConnectorTableActions.Add(deleteAction);
        else
          deleteFromAncestorTableActions.Add(deleteAction);
      }

      updateActions.ForEach(
        a => ProcessUpdateDataAction(a, postCopy));
      deleteFromConnectorTableActions.ForEach(
        a => ProcessDeleteDataAction(a, postCopy));
      ProcessClearAncestorsActions(deleteFromAncestorTableActions, postCopy);

      // Necessary, since this method is called twice on upgrade
      clearDataActions.Clear();
    }

    private void ProcessDeleteDataAction(DataAction action, bool postCopy)
    {
      var deleteOutput = currentOutput.ForStage(postCopy ? SqlUpgradeStage.PostCopyData : SqlUpgradeStage.CleanupData);

      var hint = (DeleteDataHint) action.DataHint;
      var soureTableInfo = (TableInfo) sourceModel.Resolve(hint.SourceTablePath, true);
      var table = SqlDml.TableRef(FindTable(soureTableInfo));
      var delete = SqlDml.Delete(table);

      delete.Where = CreateConditionalExpression(hint, table);

      deleteOutput.RegisterCommand(delete);
    }

    /// <exception cref="InvalidOperationException">Can not create update command
    /// with specific hint parameters.</exception>
    private void ProcessUpdateDataAction(DataAction action, bool postCopy)
    {
      var hint = (UpdateDataHint) action.DataHint;
      var soureTableInfo = (TableInfo) sourceModel.Resolve(hint.SourceTablePath, true);
      var table = SqlDml.TableRef(FindTable(soureTableInfo));
      var update = SqlDml.Update(table);

      var updatedColumns = hint.UpdateParameter
        .Select(pair => new Pair<StorageColumnInfo, object>(
          sourceModel.Resolve(pair.First, true) as StorageColumnInfo,
          pair.Second)).ToArray();

      if (updatedColumns.Length==0)
        throw new InvalidOperationException(Strings.ExIncorrectCommandParameters);

      foreach (var pair in updatedColumns) {
        var column = pair.First;
        var value = pair.Second;

        if (value==null) {
          if (providerInfo.Supports(ProviderFeatures.UpdateDefaultValues))
            update.Values[table[column.Name]] = SqlDml.DefaultValue;
          else {
            if (column.Type.IsNullable)
              update.Values[table[column.Name]] = SqlDml.Null;
            else
              update.Values[table[column.Name]] = GetDefaultValueExpression(column);
          }
        }
        else
          update.Values[table[column.Name]] = SqlDml.Literal(value);
      }

      update.Where = CreateConditionalExpression(hint, table);

      var updateOutput = currentOutput.ForStage(postCopy ? SqlUpgradeStage.PostCopyData : SqlUpgradeStage.CleanupData);

      updateOutput.RegisterCommand(update);
    }

    private void ProcessClearAncestorsActions(List<DataAction> originalActions, bool postCopy)
    {
      if (originalActions.Count==0)
        return;

      // Merge actions
      var deleteActions = new Dictionary<TableInfo, List<string>>();
      foreach (var action in originalActions) {
        var sourceTableInfo = sourceModel.Resolve(action.DataHint.SourceTablePath, true) as TableInfo;
        List<string> list;
        if (!deleteActions.TryGetValue(sourceTableInfo, out list)) {
          list = new List<string>();
          deleteActions.Add(sourceTableInfo, list);
        }
        list.AddRange(action.DataHint.Identities.Select(pair => pair.Target));
      }

      // Sort actions topologicaly according with foreign keys
      var nodes = new List<Xtensive.Sorting.Node<TableInfo, ForeignKeyInfo>>();
      var foreignKeys = sourceModel.Tables
        .SelectMany(table => table.ForeignKeys)
        .ToList();
      foreach (var pair in deleteActions)
        nodes.Add(new Xtensive.Sorting.Node<TableInfo, ForeignKeyInfo>(pair.Key));
      foreach (var foreignKey in foreignKeys) {
        ForeignKeyInfo foreignKeyInfo = foreignKey;
        var referencedNode = nodes.FirstOrDefault(node => node.Item==foreignKeyInfo.PrimaryKey.Parent);
        var referencingNode = nodes.FirstOrDefault(node => node.Item==foreignKeyInfo.Parent);
        if (referencedNode!=null && referencingNode!=null)
            new NodeConnection<TableInfo, ForeignKeyInfo>(referencedNode, referencingNode, foreignKey).BindToNodes();
      }
      List<NodeConnection<TableInfo, ForeignKeyInfo>> edges;
      var sortedTables = TopologicalSorter.Sort(nodes, out edges);
      // TODO: Process removed edges

      // Build DML commands
      for (var i = sortedTables.Count; i-- > 0;) {
        var table = sortedTables[i];
        var tableRef = SqlDml.TableRef(FindTable(table));
        var delete = SqlDml.Delete(tableRef);
        var typeIds = deleteActions[table];
        foreach (var typeId in typeIds)
          delete.Where |= tableRef[typeIdColumnName]==typeId;

        var clearOutput = currentOutput.ForStage(postCopy ? SqlUpgradeStage.PostCopyData : SqlUpgradeStage.CleanupData);

        clearOutput.RegisterCommand(delete);
      }
    }

    private void ChangeColumnType(PropertyChangeAction action)
    {
      var targetColumn = (StorageColumnInfo) targetModel.Resolve(action.Path, true);
      var sourceColumn = (StorageColumnInfo) sourceModel.Resolve(action.Path, true);
      var column = FindColumn(targetColumn.Parent, targetColumn.Name);
      var table = column.Table;
      var originalName = column.Name;

      var upgradeOutput = currentOutput.ForStage(SqlUpgradeStage.Upgrade);
      var cleanupOutput = currentOutput.ForStage(SqlUpgradeStage.Cleanup);

      // Rename old column
      var tempName = GetTemporaryName(column);
      if(recreatedColumns.Contains(column)) {
        recreatedColumns.Remove(column);
        RenameColumn(column, tempName);
        recreatedColumns.Add(column);
      }
      else
        RenameColumn(column, tempName);

      // Create new columns
      var newTypeInfo = targetColumn.Type;
      var newSqlType = (SqlValueType) newTypeInfo.NativeType;
      var newColumn = table.CreateColumn(originalName, newSqlType);

      newColumn.IsNullable = newTypeInfo.IsNullable;
      if (!newColumn.IsNullable)
        newColumn.DefaultValue = GetDefaultValueExpression(targetColumn);

      var addNewColumn = SqlDdl.Alter(table, SqlDdl.AddColumn(newColumn));

      upgradeOutput.RegisterCommand(addNewColumn);

      // Copy values if possible to convert type
      if (TypeConversionVerifier.CanConvert(sourceColumn.Type, newTypeInfo)
        || enforceChangedColumns.Contains(sourceColumn.Path, StringComparer)) {
        var tableRef = SqlDml.TableRef(table);
        var copyValues = SqlDml.Update(tableRef);
        if (newTypeInfo.IsNullable) {
          if (sourceColumn.Type.Type.StripNullable()==WellKnownTypes.String && newSqlType.Length < column.DataType.Length)
            copyValues.Values[tableRef[originalName]] = SqlDml.Cast(SqlDml.Substring(tableRef[tempName], 0, newSqlType.Length), newSqlType);
          else
            copyValues.Values[tableRef[originalName]] = SqlDml.Cast(tableRef[tempName], newSqlType);
        }
        else {
          var getValue = SqlDml.Case();
          getValue.Add(SqlDml.IsNull(tableRef[tempName]), GetDefaultValueExpression(targetColumn));

          if (newSqlType.Type==SqlType.DateTimeOffset)
            getValue.Add(SqlDml.IsNotNull(tableRef[tempName]), SqlDml.DateTimeToDateTimeOffset(tableRef[tempName]));
          else if (newSqlType.Type==SqlType.DateTime && providerInfo.Supports(ProviderFeatures.DateTimeOffsetEmulation))
            getValue.Add(SqlDml.IsNotNull(tableRef[tempName]), SqlDml.Cast(SqlDml.Extract(SqlDateTimeOffsetPart.DateTime, tableRef[tempName]), newSqlType));
          else
            getValue.Add(SqlDml.IsNotNull(tableRef[tempName]), SqlDml.Cast(tableRef[tempName], newSqlType));

          copyValues.Values[tableRef[originalName]] = getValue;
        }
        upgradeOutput.BreakBatch();
        upgradeOutput.RegisterCommand(copyValues);
      }

      // Drop old column
      DropColumn(table.TableColumns[tempName], cleanupOutput);
    }

    private Table CreateTable(TableInfo tableInfo)
    {
      var node = resolver.Resolve(sqlModel, tableInfo.Name);
      var table = node.Schema.CreateTable(node.Name);
      foreach (var columnInfo in tableInfo.Columns)
        CreateColumn(columnInfo, table);
      if (tableInfo.PrimaryIndex!=null)
        CreatePrimaryKey(tableInfo, table);
      createdTables.Add(table);
      return table;
    }

    private TableColumn CreateColumn(StorageColumnInfo columnInfo, Table table)
    {
      var type = columnInfo.Type.NativeType;
      var column = table.CreateColumn(columnInfo.Name, type);
      var isPrimaryKeyColumn =
        columnInfo.Parent.PrimaryIndex!=null
        && columnInfo.Parent.PrimaryIndex.KeyColumns.Any(keyColumn => keyColumn.Value==columnInfo);

      if (!column.IsNullable && column.Name != typeIdColumnName)
        if (!isPrimaryKeyColumn)
          column.DefaultValue = GetDefaultValueExpression(columnInfo);
        else if (!string.IsNullOrEmpty(columnInfo.DefaultSqlExpression))
          column.DefaultValue = SqlDml.Native(columnInfo.DefaultSqlExpression);

      column.IsNullable = columnInfo.Type.IsNullable;

      if (columnInfo.Type.Type==WellKnownTypes.String && collationName!=null)
        column.Collation = table.Schema.Collations[collationName] ?? new Collation(table.Schema, collationName);

      return column;
    }

    private ForeignKey CreateForeignKey(ForeignKeyInfo foreignKeyInfo)
    {
      var referencingTable = FindTable(foreignKeyInfo.Parent);
      var foreignKey = referencingTable.CreateForeignKey(foreignKeyInfo.Name);
      foreignKey.OnUpdate = ConvertReferentialAction(foreignKeyInfo.OnUpdateAction);
      foreignKey.OnDelete = ConvertReferentialAction(foreignKeyInfo.OnRemoveAction);
      foreignKey.IsDeferrable = providerInfo.Supports(ProviderFeatures.DeferrableConstraints);
      foreignKey.IsInitiallyDeferred = foreignKey.IsDeferrable;
      var referencingColumns = foreignKeyInfo.ForeignKeyColumns
        .Select(cr => FindColumn(referencingTable, cr.Value.Name));
      foreignKey.Columns.AddRange(referencingColumns);
      var referencedTable = FindTable(foreignKeyInfo.PrimaryKey.Parent);
      var referencedColumns = foreignKeyInfo.PrimaryKey.KeyColumns
        .Select(cr => FindColumn(referencedTable, cr.Value.Name));
      foreignKey.ReferencedTable = referencedTable;
      foreignKey.ReferencedColumns.AddRange(referencedColumns);
      return foreignKey;
    }

    private PrimaryKey CreatePrimaryKey(TableInfo tableInfo, Table table)
    {
      var columns = tableInfo.PrimaryIndex.KeyColumns.Select(cr => FindColumn(table, cr.Value.Name)).ToArray();
      var primaryKey = table.CreatePrimaryKey(tableInfo.PrimaryIndex.Name, columns);
      // For storages that do not support clustered indexes
      // PrimaryIndexInfo.IsClustered will be forcibly set to false by DomainModelConverter.
      primaryKey.IsClustered = tableInfo.PrimaryIndex.IsClustered;
      return primaryKey;
    }

    private Index CreateSecondaryIndex(Table table, SecondaryIndexInfo indexInfo)
    {
      if (indexInfo.KeyColumns.Count == 1) {
        var column = FindColumn(table, indexInfo.KeyColumns[0].Value.Name);
        if (driver.ServerInfo.DataTypes[column.DataType.Type].Features.Supports(DataTypeFeatures.Spatial)) {

          var spatialIndex = table.CreateSpatialIndex(indexInfo.Name);
          spatialIndex.CreateIndexColumn(column);
          return spatialIndex;
        }
      }

      var index = table.CreateIndex(indexInfo.Name);
      index.IsUnique = indexInfo.IsUnique;
      index.IsClustered = indexInfo.IsClustered;
      foreach (var keyColumn in indexInfo.KeyColumns)
        index.CreateIndexColumn(
          FindColumn(table, keyColumn.Value.Name),
          keyColumn.Direction==Direction.Positive);
      index.NonkeyColumns.AddRange(
        indexInfo.IncludedColumns
          .Select(cr => FindColumn(table, cr.Value.Name)).ToArray());
      if (indexInfo.Filter!=null)
        index.Where = SqlDml.Native(indexInfo.Filter.Expression);
      return index;
    }

    private void CreateGeneratorTable(StorageSequenceInfo sequenceInfo)
    {
      var node = resolver.Resolve(sqlModel, sequenceInfo.Name);
      var sequenceTable = node.Schema.CreateTable(node.Name);
      createdTables.Add(sequenceTable);
      var idColumn = sequenceTable.CreateColumn(WellKnown.GeneratorColumnName,
        (SqlValueType) sequenceInfo.Type.NativeType);
      idColumn.SequenceDescriptor =
        new SequenceDescriptor(
          idColumn,
          sequenceInfo.Current ?? sequenceInfo.Seed,
          sequenceInfo.Increment);
      sequenceTable.CreatePrimaryKey($"PK_{sequenceInfo.Name}", idColumn);
      if (!providerInfo.Supports(ProviderFeatures.InsertDefaultValues)) {
        var fakeColumn = sequenceTable.CreateColumn(WellKnown.GeneratorFakeColumnName, driver.MapValueType(WellKnownTypes.Int32));
        fakeColumn.IsNullable = true;
      }
      currentOutput.RegisterCommand(SqlDdl.Create(sequenceTable));
    }

    private void DropGeneratorTable(StorageSequenceInfo sequenceInfo)
    {
      var node = resolver.Resolve(sqlModel, sequenceInfo.Name);
      var sequenceTable = node.GetTable();
      string lockedTable;
      sqlModel.LockedTables.TryGetValue(resolver.GetNodeName(sequenceTable), out lockedTable);
      if (!lockedTable.IsNullOrEmpty())
        throw new SchemaSynchronizationException(lockedTable);
      currentOutput.RegisterCommand(SqlDdl.Drop(sequenceTable));
      node.Schema.Tables.Remove(sequenceTable);
    }

    private void RenameSchemaTable(Table table, Schema oldSchema, Schema newSchema, string newName)
    {
      var schema = table.Schema;

      // Renamed table must be removed and added with new name
      // for reregistring in dictionary
      oldSchema.Tables.Remove(table);
      table.Name = newName;
      newSchema.Tables.Add(table);
    }

    private void RenameSchemaColumn(TableColumn column, string newName)
    {
      // Renamed column must be removed and added with new name
      // for reregistring in dictionary
      var table = column.Table;
      table.TableColumns.Remove(column);
      column.Name = newName;
      table.TableColumns.Add(column);
    }

    private string GetTemporaryName(Table table)
    {
      var tempName = string.Format(TemporaryNameFormat, table.Name);
      var counter = 0;
      while (table.Schema.Tables.Any(t => StringComparer.Compare(t.Name, tempName)==0))
        tempName = string.Format(TemporaryNameFormat, table.Name + ++counter);
      return tempName;
    }

    private string GetTemporaryName(TableColumn column)
    {
      var tempName = string.Format(TemporaryNameFormat, column.Name);
      var counter = 0;
      while (column.Table.Columns.Any(tableColumn => StringComparer.Equals(tableColumn.Name, tempName)))
        tempName = string.Format(TemporaryNameFormat, column.Name + ++counter);
      return tempName;
    }

    private Table FindTable(TableInfo tableInfo)
    {
      return resolver.Resolve(sqlModel, tableInfo.Name).GetTable();
    }

    private TableColumn FindColumn(Table table, string name)
    {
      return table.TableColumns[name];
    }

    private TableColumn FindColumn(TableInfo tableInfo, string columnName)
    {
      return FindTable(tableInfo).TableColumns[columnName];
    }

    private static ReferentialAction ConvertReferentialAction(Model.ReferentialAction toConvert)
    {
      switch (toConvert) {
      case Model.ReferentialAction.None:
        return ReferentialAction.NoAction;
      case Model.ReferentialAction.Restrict:
        return ReferentialAction.Restrict;
      case Model.ReferentialAction.Cascade:
        return ReferentialAction.Cascade;
      case Model.ReferentialAction.Clear:
        return ReferentialAction.SetNull;
      default:
        return ReferentialAction.Restrict;
      }
    }

    private static ChangeTrackingMode ConvertFullTextChangeTrackingMode(FullTextChangeTrackingMode toConvert)
    {
      switch (toConvert) {
      case FullTextChangeTrackingMode.Auto:
        return ChangeTrackingMode.Auto;
      case FullTextChangeTrackingMode.Manual:
        return ChangeTrackingMode.Manual;
      case FullTextChangeTrackingMode.Off:
        return ChangeTrackingMode.Off;
      case FullTextChangeTrackingMode.OffWithNoPopulation:
        return ChangeTrackingMode.OffWithNoPopulation;
      default:
        return ChangeTrackingMode.Default;
      }
    }

    /// <exception cref="InvalidOperationException">Can not create expression
    /// with specific hint parameters.</exception>
    private SqlExpression CreateConditionalExpression(DataHint hint, SqlTableRef table)
    {
      if (hint.Identities.Any(pair => !pair.IsIdentifiedByConstant)) {
        var identityColumnPairs = hint.Identities
          .Where(pair => !pair.IsIdentifiedByConstant).Select(pair =>
            new Pair<StorageColumnInfo, StorageColumnInfo>(
              sourceModel.Resolve(pair.Target, true) as StorageColumnInfo,
              sourceModel.Resolve(pair.Source, true) as StorageColumnInfo)).ToArray();
        var identityConstantPairs = hint.Identities
          .Where(pair => pair.IsIdentifiedByConstant).Select(pair =>
            new Pair<StorageColumnInfo, string>(
              sourceModel.Resolve(pair.Source, true) as StorageColumnInfo,
              pair.Target)).ToArray();
        var selectColumns = identityColumnPairs.Select(columnPair => columnPair.First)
          .Concat(identityConstantPairs.Select(constantPair => constantPair.First)).ToArray();
        if (!selectColumns.Any())
          throw new InvalidOperationException(Strings.ExIncorrectCommandParameters);

        var identifiedTable = FindTable(selectColumns[0].Parent);
        var identifiedTableRef = SqlDml.TableRef(identifiedTable,
          string.Format(SubqueryTableAliasNameFormat, identifiedTable.Name));
        var select = SqlDml.Select(identifiedTableRef);
        selectColumns.ForEach(column => select.Columns.Add(identifiedTableRef[column.Name]));
        identityColumnPairs.ForEach(pair =>
          select.Where &= identifiedTableRef[pair.First.Name]==table[pair.Second.Name]);
        identityConstantPairs.ForEach(pair =>
          select.Where &= identifiedTableRef[pair.First.Name]==SqlDml.Literal(pair.Second));
        return SqlDml.Exists(select);
      }
      else {
        var identityConstantPairs = hint.Identities
          .Where(pair => pair.IsIdentifiedByConstant).Select(pair =>
            new Pair<StorageColumnInfo, string>(
              sourceModel.Resolve(pair.Source, true) as StorageColumnInfo,
              pair.Target)).ToArray();
        if (!identityConstantPairs.Any())
          throw new InvalidOperationException(Strings.ExIncorrectCommandParameters);
        SqlExpression expression = null;
        identityConstantPairs.ForEach(pair =>
          expression &= table[pair.First.Name]==SqlDml.Literal(pair.Second));
        return expression;
      }
    }

    private SqlExpression GetDefaultValueExpression(StorageColumnInfo columnInfo)
    {
      if (!string.IsNullOrEmpty(columnInfo.DefaultSqlExpression))
        return SqlDml.Native(columnInfo.DefaultSqlExpression);
      if (columnInfo.DefaultValue!=null)
        return SqlDml.Literal(columnInfo.DefaultValue);
      var type = columnInfo.Type.Type;
      return type.IsValueType && !type.IsNullable() ? SqlDml.Literal(Activator.CreateInstance(type)) : null;
    }

    private string TryUnquoteLiteral(string stringToUnquote)
    {
      if (stringToUnquote.StartsWith("N", StringComparison.Ordinal)) {
        var unquotedSting = stringToUnquote.Remove(0, 1).Trim(new[] {'\''}).Replace("''", "'");
        return unquotedSting;
      }
      return stringToUnquote;
    }

    private long? GetCurrentSequenceValue(StorageSequenceInfo sequenceInfo)
    {
      var node = resolver.Resolve(sqlModel, sequenceInfo.Name);
      var generatorNode = providerInfo.Supports(ProviderFeatures.Sequences)
        ? (SchemaNode) node.GetSequence()
        : node.GetTable();
      return sequenceQueryBuilder.BuildNextValueQuery(generatorNode, 0).ExecuteWith(sqlExecutor);
    }

    private bool IsGeneratorTable(Table table, out SequenceDescriptor sequenceDescriptor)
    {
      var columnNumber = providerInfo.Supports(ProviderFeatures.InsertDefaultValues) ? 1 : 2;
      sequenceDescriptor = table.TableColumns[0].SequenceDescriptor;
      return table.TableColumns.Count == columnNumber && sequenceDescriptor != null;
    }

    private long GetLastValueOfTableBasedSequenceQuery(Table table)
    {
      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      var delete = SqlDml.Delete(tableRef);

      if (!providerInfo.Supports(ProviderFeatures.InsertDefaultValues)) {
        var fakeColumn = table.TableColumns[WellKnown.GeneratorFakeColumnName];
        insert.Values[tableRef[fakeColumn.Name]] = SqlDml.Null;
      }

      var result = SqlDml.Batch();
      if (providerInfo.Supports(ProviderFeatures.AutoIncrementSettingsInMemory)) {
        result.Add(delete);
      }

      result.Add(insert);
      result.Add(SqlDml.Select(SqlDml.LastAutoGeneratedId()));
      return Convert.ToInt64(sqlExecutor.ExecuteScalar(driver.Compile(result).GetCommandText()));
    }


    private void ProcessActions(Modelling.Comparison.UpgradeStage modellingStage, SqlUpgradeStage stage)
    {
      currentOutput = currentOutput.ForStage(stage);
      var actionName = modellingStage.ToString();
      var groupingAction = actions
        .OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==actionName);
      if (groupingAction!=null)
        VisitAction(groupingAction);
    }

    // Constructors

    public SqlActionTranslator(
      HandlerAccessor handlers, ISqlExecutor sqlExecutor, MappingResolver resolver,
      ActionSequence actions, SchemaExtractionResult sqlModel, StorageModel sourceModel, StorageModel targetModel,
      List<string> enforceChangedColumns, bool allowCreateConstraints)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      ArgumentValidator.EnsureArgumentNotNull(sqlExecutor, "sqlExecutor");
      ArgumentValidator.EnsureArgumentNotNull(resolver, "resolver");
      ArgumentValidator.EnsureArgumentNotNull(actions, "actions");
      ArgumentValidator.EnsureArgumentNotNull(sqlModel, "sqlModel");
      ArgumentValidator.EnsureArgumentNotNull(sourceModel, "sourceModel");
      ArgumentValidator.EnsureArgumentNotNull(targetModel, "targetModel");
      ArgumentValidator.EnsureArgumentNotNull(enforceChangedColumns, "enforceChangedColumns");

      driver = handlers.StorageDriver;
      providerInfo = handlers.ProviderInfo;
      sequenceQueryBuilder = handlers.SequenceQueryBuilder;
      providerInfo = handlers.ProviderInfo;
      typeIdColumnName = handlers.NameBuilder.TypeIdColumnName;

      this.resolver = resolver;
      this.sqlModel = sqlModel;
      this.actions = actions;
      this.sourceModel = sourceModel;
      this.targetModel = targetModel;
      this.enforceChangedColumns = enforceChangedColumns;
      this.sqlExecutor = sqlExecutor;
      this.allowCreateConstraints = allowCreateConstraints;

      if (providerInfo.Supports(ProviderFeatures.Collations)) {
        var collation = handlers.Domain.Configuration.Collation;
        if (!string.IsNullOrEmpty(collation))
          collationName = collation;
      }
    }
  }
}
