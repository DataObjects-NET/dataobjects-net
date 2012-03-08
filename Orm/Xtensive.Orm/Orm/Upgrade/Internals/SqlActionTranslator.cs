// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Providers.Sql;
using Xtensive.Reflection;
using Xtensive.Sorting;
using Xtensive.Modelling;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Sql;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Orm.Upgrade.Model;
using ReferentialAction = Xtensive.Sql.ReferentialAction;

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

    private readonly ActionSequence actions;
    private readonly SqlExtractionResult sqlModel;
    private readonly StorageModel sourceModel;
    private readonly StorageModel targetModel;
    private readonly StorageDriver driver;
    private readonly SequenceQueryBuilder sequenceQueryBuilder;
    private readonly SchemaResolver schemaResolver;

    private readonly List<Table> createdTables = new List<Table>();
    private readonly List<Sequence> createdSequences = new List<Sequence>();
    private readonly List<DataAction> clearDataActions = new List<DataAction>();

    private UpgradeActionSequence translationResult;
    
    private Modelling.Comparison.UpgradeStage currentUpgradeStage;
    private bool translated;

    /// <summary>
    /// Translates all registered actions.
    /// </summary>
    public UpgradeActionSequence Translate()
    {
      if (translated)
        throw new InvalidOperationException(Strings.ExCommandsAreAlreadyTranslated);

      translated = true;
      translationResult = new UpgradeActionSequence();

      // Turn off deferred contraints
      if (providerInfo.Supports(ProviderFeatures.DeferrableConstraints))
        RegisterCommand(SqlDdl.Command(SqlCommandType.SetConstraintsAllImmediate), SqlUpgradeStage.PreCleanupData);

      // Data cleanup
      currentUpgradeStage = Modelling.Comparison.UpgradeStage.CleanupData;
      var cleanupData = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==Modelling.Comparison.UpgradeStage.CleanupData.ToString());
      if (cleanupData!=null)
        VisitAction(cleanupData);
      ProcessClearDataActions(false);

      // Prepairing (aka запаривание :-)
      currentUpgradeStage = Modelling.Comparison.UpgradeStage.Prepare;
      var prepareActions = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==Modelling.Comparison.UpgradeStage.Prepare.ToString());
      if (prepareActions!=null)
        VisitAction(prepareActions);

      // Mutual renaming
      currentUpgradeStage = Modelling.Comparison.UpgradeStage.TemporaryRename;
      var renameActions = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==Modelling.Comparison.UpgradeStage.TemporaryRename.ToString());
      if (renameActions!=null)
        VisitAction(renameActions);

      // Upgrading
      currentUpgradeStage = Modelling.Comparison.UpgradeStage.Upgrade;
      var upgradeActions = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==Modelling.Comparison.UpgradeStage.Upgrade.ToString());
      if (upgradeActions!=null)
        VisitAction(upgradeActions);

      // Copying data
      currentUpgradeStage = Modelling.Comparison.UpgradeStage.CopyData;
      var copyDataActions = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==Modelling.Comparison.UpgradeStage.CopyData.ToString());
      if (copyDataActions!=null)
        VisitAction(copyDataActions);

      // Post copying data
      currentUpgradeStage = Modelling.Comparison.UpgradeStage.PostCopyData;
      var postCopyDataActions = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==Modelling.Comparison.UpgradeStage.PostCopyData.ToString());
      if (postCopyDataActions!=null)
        VisitAction(postCopyDataActions);
      ProcessClearDataActions(true);

      // Cleanup
      currentUpgradeStage = Modelling.Comparison.UpgradeStage.Cleanup;
      var cleanupActions = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==Modelling.Comparison.UpgradeStage.Cleanup.ToString());
      if (cleanupActions!=null)
        VisitAction(cleanupActions);

      // Turn on deferred contraints
      if (providerInfo.Supports(ProviderFeatures.DeferrableConstraints))
        RegisterCommand(SqlDdl.Command(SqlCommandType.SetConstraintsAllDeferred));

      return translationResult;
    }

    private void VisitAction(NodeAction action)
    {
      if (action is GroupingNodeAction)
        foreach (var nodeAction in ((GroupingNodeAction) action).Actions)
          VisitAction(nodeAction);
      else if (action is CreateNodeAction)
        VisitCreateAction(action as CreateNodeAction);
      else if (action is RemoveNodeAction)
        VisitRemoveAction(action as RemoveNodeAction);
      else if (action is DataAction)
        VisitDataAction(action as DataAction);
      else
        VisitAlterAction(action);
    }

    private void VisitCreateAction(CreateNodeAction action)
    {
      if (action.Type == typeof(TableInfo))
        VisitCreateTableAction(action);
      else if (action.Type == typeof(StorageColumnInfo))
        VisitCreateColumnAction(action);
      else if (action.Type == typeof(PrimaryIndexInfo))
        VisitCreatePrimaryKeyAction(action);
      else if (action.Type == typeof(SecondaryIndexInfo))
        VisitCreateSecondaryIndexAction(action);
      else if (action.Type == typeof(ForeignKeyInfo))
        VisitCreateForeignKeyAction(action);
      else if (action.Type == typeof(StorageSequenceInfo))
        VisitCreateSequenceAction(action);
      else if (action.Type == typeof(StorageFullTextIndexInfo))
        VisitCreateFullTextIndexAction(action);
    }

    private void VisitRemoveAction(RemoveNodeAction action)
    {
      var node = action.Difference.Source;
      if (node.GetType() == typeof(TableInfo))
        VisitRemoveTableAction(action);
      else if (node.GetType() == typeof(StorageColumnInfo))
        VisitRemoveColumnAction(action);
      else if (node.GetType() == typeof(PrimaryIndexInfo))
        VisitRemovePrimaryKeyAction(action);
      else if (node.GetType() == typeof(SecondaryIndexInfo))
        VisitRemoveSecondaryIndexAction(action);
      else if (node.GetType() == typeof(ForeignKeyInfo))
        VisitRemoveForeignKeyAction(action);
      else if (node.GetType() == typeof(StorageSequenceInfo))
        VisitRemoveSequenceAction(action);
      else if (node.GetType() == typeof(StorageFullTextIndexInfo))
        VisitRemoveFullTextIndexAction(action);
    }

    private void VisitAlterAction(NodeAction action)
    {
      var propertyChangeAction = action as PropertyChangeAction;
      if (propertyChangeAction != null) {
        var changedNode = targetModel.Resolve(propertyChangeAction.Path, true);
        if (changedNode.GetType()==typeof (StorageSequenceInfo))
          VisitAlterSequenceAction(propertyChangeAction);
        else if (changedNode.GetType()==typeof (StorageColumnInfo))
          VisitAlterColumnAction(propertyChangeAction);
        return;
      }

      var moveNodeAction = action as MoveNodeAction;
      if (moveNodeAction==null)
        return;
      var node = moveNodeAction.Difference.Source;
      if (node.GetType()==typeof (TableInfo))
        VisitAlterTableAction(moveNodeAction);
      else if (node.GetType()==typeof (StorageColumnInfo))
        VisitMoveColumnAction(moveNodeAction);
      else if (node.GetType()==typeof (PrimaryIndexInfo))
        VisitAlterPrimaryKeyAction(moveNodeAction);
      else if (node.GetType()==typeof (SecondaryIndexInfo))
        VisitAlterSecondaryIndexAction(moveNodeAction);
      else if (node.GetType()==typeof (ForeignKeyInfo))
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
    
    #region Visit concrete action methods

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
        RegisterCommand(update);
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
      RegisterCommand(update);
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
      RegisterCommand(SqlDdl.Create(table));
    }

    private void VisitRemoveTableAction(RemoveNodeAction action)
    {
      var tableInfo = (TableInfo) action.Difference.Source;
      var table = FindTable(tableInfo);
      RegisterCommand(SqlDdl.Drop(table));
      table.Schema.Tables.Remove(table);
    }

    private void VisitAlterTableAction(MoveNodeAction action)
    {
      var oldTableInfo = sourceModel.Resolve(action.Path, true) as TableInfo;
      var table = FindTable(oldTableInfo);
      if (providerInfo.Supports(ProviderFeatures.TableRename))
        RegisterCommand(SqlDdl.Rename(table, action.Name));
      else {
          var scheme = table.Schema;
          var newTable = scheme.CreateTable(action.Name);
          foreach (var item in table.TableColumns) {
            var column = newTable.CreateColumn(item.Name, item.DataType);
            column.DbName = item.DbName;
            column.IsNullable = item.IsNullable;
          }
          RegisterCommand(SqlDdl.Create(newTable));

          // Copying data from one table to another
          var insert = SqlDml.Insert(SqlDml.TableRef(newTable));
          var select = SqlDml.Select(SqlDml.TableRef(table));
          insert.From = SqlDml.QueryRef(select);
          RegisterCommand(insert);

          // Removing table
          RegisterCommand(SqlDdl.Drop(table));

          scheme.Tables.Remove(newTable);
      }
      oldTableInfo.Name = action.Name;
      RenameSchemaTable(table, action.Name);
    }
    
    private void VisitAlterTableAction(NodeAction action)
    {
      throw new NotSupportedException();
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
      RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddColumn(column)));
    }

    private void VisitRemoveColumnAction(RemoveNodeAction removeColumnAction)
    {
      var columnInfo = (StorageColumnInfo) removeColumnAction.Difference.Source;
      var table = FindTable(columnInfo.Parent);

      // Ensure table is not removed
      if (table==null)
        return;

      var column = FindColumn(table, columnInfo.Name);
      if (column == null)
        return;

      if (!column.DefaultValue.IsNullReference()) {
        var constraint = table.TableConstraints
          .OfType<DefaultConstraint>()
          .FirstOrDefault(defaultConstraint => defaultConstraint.Column==column);
        if (constraint!=null)
          RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(constraint)));
      }
      RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropColumn(column)));
      table.TableColumns.Remove(column);
    }

    private void VisitAlterColumnAction(PropertyChangeAction action)
    {
      var isNewlyCreatedColumn = action.Difference.Source==null;
      if (isNewlyCreatedColumn)
        return; // Properties already initilized

      if (!action.Properties.ContainsKey(ColumnTypePropertyName))
        if (!action.Properties.ContainsKey(ColumnDefaultPropertyName))
          return;
 
      GenerateChangeColumnTypeCommands(action);
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
      if (providerInfo.Supports(ProviderFeatures.ColumnRename)) {
        RegisterCommand(SqlDdl.Rename(column, name), SqlUpgradeStage.Upgrade);
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
      RegisterCommand(addColumnWithNewType, SqlUpgradeStage.Upgrade);

      // Copy data
      var tableRef = SqlDml.TableRef(column.Table);
      var update = SqlDml.Update(tableRef);
      update.Values[tableRef[name]] = tableRef[originalName];
      RegisterCommand(update, SqlUpgradeStage.Upgrade);

      // Drop old column
      if (!column.DefaultValue.IsNullReference()) {
        var constraint = table.TableConstraints
          .OfType<DefaultConstraint>().FirstOrDefault(defaultConstraint => defaultConstraint.Column==column);
        if (constraint!=null)
          RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(constraint)), SqlUpgradeStage.Upgrade);
      }
      var removeOldColumn = SqlDdl.Alter(column.Table, SqlDdl.DropColumn(column));
      RegisterCommand(removeOldColumn, SqlUpgradeStage.Upgrade);
      table.TableColumns.Remove(column);
    }

    private void VisitCreatePrimaryKeyAction(CreateNodeAction action)
    {
      var primaryIndex = (PrimaryIndexInfo) action.Difference.Target;
      var table = FindTable(primaryIndex.Parent);

      // Ensure table is not newly created
      if (createdTables.Contains(table))
        return;

      var primaryKey = CreatePrimaryKey(primaryIndex.Parent, table);
      RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddConstraint(primaryKey)));
    }

    private void VisitRemovePrimaryKeyAction(RemoveNodeAction action)
    {
      var primaryIndexInfo = (PrimaryIndexInfo) action.Difference.Source;
      var table = FindTable(primaryIndexInfo.Parent);

      // Ensure table is not removed
      if (table==null)
        return;
      
      var primaryKey = table.TableConstraints[primaryIndexInfo.Name];

      RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(primaryKey)));
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
      RegisterCommand(SqlDdl.Create(index));
    }

    private void VisitRemoveSecondaryIndexAction(RemoveNodeAction action)
    {
      var secondaryIndexInfo = (SecondaryIndexInfo) action.Difference.Source;
      var table = FindTable(secondaryIndexInfo.Parent);
      
      // Ensure table is not removed
      if (table==null)
        return;
      
      var index = table.Indexes[secondaryIndexInfo.Name];
      RegisterCommand(SqlDdl.Drop(index), SqlUpgradeStage.PreCleanupData);
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

      RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddConstraint(foreignKey)));
    }

    private void VisitRemoveForeignKeyAction(RemoveNodeAction action)
    {
      var foreignKeyInfo = (ForeignKeyInfo) action.Difference.Source;
      var table = FindTable(foreignKeyInfo.Parent);

      // Ensure table is not removed
      if (table==null)
        return;

      var foreignKey = table.TableConstraints[foreignKeyInfo.Name];
      RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(foreignKey)), SqlUpgradeStage.PreCleanupData);
      table.TableConstraints.Remove(foreignKey);
    }

    private void VisitAlterForeignKeyAction(NodeAction action)
    {
      throw new NotSupportedException();
    }

    private void VisitCreateSequenceAction(CreateNodeAction action)
    {
      var sequenceInfo = (StorageSequenceInfo) action.Difference.Target;
      var schema = schemaResolver.ResolveSchema(sqlModel, sequenceInfo.Parent.Name);

      if (providerInfo.Supports(ProviderFeatures.Sequences)) {
        var sequence = schema.CreateSequence(sequenceInfo.Name);
        var descriptor = new SequenceDescriptor(sequence, sequenceInfo.Seed, sequenceInfo.Increment) {
          MinValue = sequenceInfo.Seed
        };
        sequence.SequenceDescriptor = descriptor;
        sequence.DataType = (SqlValueType) sequenceInfo.Type.NativeType;
        RegisterCommand(SqlDdl.Create(sequence));
        createdSequences.Add(sequence);
      }
      else {
        CreateGeneratorTable(sequenceInfo);
      }
    }

    private void VisitRemoveSequenceAction(RemoveNodeAction action)
    {
      var sequenceInfo = (StorageSequenceInfo) action.Difference.Source;
      var schema = schemaResolver.ResolveSchema(sqlModel, sequenceInfo.Parent.Name);
      if (providerInfo.Supports(ProviderFeatures.Sequences)) {
        var sequence = schema.Sequences[sequenceInfo.Name];
        RegisterCommand(SqlDdl.Drop(sequence));
        schema.Sequences.Remove(sequence);
      }
      else {
        DropGeneratorTable(sequenceInfo);
      }
    }

    private void VisitAlterSequenceAction(PropertyChangeAction action)
    {
      var sequenceInfo = (StorageSequenceInfo) targetModel.Resolve(action.Path, true);
      var schema = schemaResolver.ResolveSchema(sqlModel, sequenceInfo.Parent.Name);

      // Check if sequence is not newly created
      if (IsNewlyCreatedSequence(schema, sequenceInfo))
        return;

      var currentValue = GetCurrentSequenceValue(sequenceInfo);
      var newStartValue = currentValue + sequenceInfo.Increment;
      if (providerInfo.Supports(ProviderFeatures.Sequences)) {
        var exisitingSequence = schema.Sequences[sequenceInfo.Name];
        var newSequenceDescriptor = new SequenceDescriptor(exisitingSequence, null, sequenceInfo.Increment);
        newSequenceDescriptor.LastValue = currentValue;
        exisitingSequence.SequenceDescriptor = newSequenceDescriptor;
        RegisterCommand(SqlDdl.Alter(exisitingSequence, newSequenceDescriptor));
      }
      else {
        sequenceInfo.Current = newStartValue;
        DropGeneratorTable(sequenceInfo);
        CreateGeneratorTable(sequenceInfo);
      }
    }

    private bool IsNewlyCreatedSequence(Schema schema, StorageSequenceInfo sequenceInfo)
    {
      
      if (providerInfo.Supports(ProviderFeatures.Sequences))
        return createdSequences.Any(s => s.Name==sequenceInfo.Name && s.Schema==schema);
      else
        return createdTables.Any(t => t.Name==sequenceInfo.Name && t.Schema==schema);
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
      ftIndex.FullTextCatalog = "Default";
      foreach (var column in fullTextIndexInfo.Columns) {
        var tableColumn = FindColumn(table, column.Value.Name);
        var ftColumn = ftIndex.CreateIndexColumn(tableColumn);
        ftColumn.Languages.Add(new Language(column.Configuration));
      }
      var stage = !providerInfo.Supports(ProviderFeatures.TransactionalFullTextDdl)
        ? SqlUpgradeStage.NonTransactionalEpilog
        : GetSqlUpgradeStage(currentUpgradeStage);
      RegisterCommand(SqlDdl.Create(ftIndex), stage);
    }

    private void VisitRemoveFullTextIndexAction(RemoveNodeAction action)
    {
      var fullTextIndexInfo = (StorageFullTextIndexInfo) action.Difference.Source;
      var fullTextSupported = providerInfo.Supports(ProviderFeatures.FullText);
      if (!fullTextSupported)
        return;
      var table = FindTable(fullTextIndexInfo.Parent);
      var ftIndex = table.Indexes[fullTextIndexInfo.Name] ?? table.Indexes.OfType<FullTextIndex>().Single();
      var stage = !providerInfo.Supports(ProviderFeatures.TransactionalFullTextDdl)
        ? SqlUpgradeStage.NonTransactionalProlog
        : GetSqlUpgradeStage(currentUpgradeStage);
      RegisterCommand(SqlDdl.Drop(ftIndex), stage);
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
        where deleteDataHint!=null && deleteDataHint.PostCopy==postCopy
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
      var hint = (DeleteDataHint) action.DataHint;
      var soureTableInfo = (TableInfo) sourceModel.Resolve(hint.SourceTablePath, true);
      var table = SqlDml.TableRef(FindTable(soureTableInfo));
      var delete = SqlDml.Delete(table);
      
      delete.Where = CreateConditionalExpression(hint, table);

      RegisterCommand(delete, postCopy ? SqlUpgradeStage.PostCopyData : SqlUpgradeStage.CleanupData);
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
      foreach (var pair in updatedColumns)
        if (pair.Second==null) {
          if (providerInfo.Supports(ProviderFeatures.UpdateDefaultValues))
            update.Values[table[pair.First.Name]] = SqlDml.DefaultValue;
          else {
            if (pair.First.Type.IsNullable)
              update.Values[table[pair.First.Name]] = SqlDml.Null;
            else {
              var typeCode = Type.GetTypeCode(pair.First.Type.Type);
              switch (typeCode) {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                  update.Values[table[pair.First.Name]] = SqlDml.Literal(0);
                  break;
                case TypeCode.Double:
                case TypeCode.Single:
                  update.Values[table[pair.First.Name]] = SqlDml.Literal(0d);
                  break;
                case TypeCode.Boolean:
                  update.Values[table[pair.First.Name]] = SqlDml.Literal(false);
                  break;
                case TypeCode.Char:
                  update.Values[table[pair.First.Name]] = SqlDml.Literal('0');
                  break;
                case TypeCode.String:
                  update.Values[table[pair.First.Name]] = SqlDml.Literal(string.Empty);
                  break;
                case TypeCode.DateTime:
                  update.Values[table[pair.First.Name]] = SqlDml.Literal(DateTime.MinValue);
                  break;
                case TypeCode.Object:
                  if (pair.First.Type.Type == typeof(Guid))
                    update.Values[table[pair.First.Name]] = SqlDml.Literal(Guid.Empty);
                  else if (pair.First.Type.Type == typeof(TimeSpan))
                    update.Values[table[pair.First.Name]] = SqlDml.Literal(TimeSpan.MinValue);
                  break;
              }
            }
          }
        }
        else
          update.Values[table[pair.First.Name]] = SqlDml.Literal(pair.Second);

      update.Where = CreateConditionalExpression(hint, table);

      RegisterCommand(update, postCopy ? SqlUpgradeStage.PostCopyData : SqlUpgradeStage.CleanupData);
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
      var foreignKeys = sourceModel.Schemas
        .SelectMany(s => s.Tables.SelectMany(table => table.ForeignKeys))
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
      sortedTables.Reverse();
      // TODO: Process removed edges
      
      // Build DML commands
      foreach (var table in sortedTables) {
        var tableRef = SqlDml.TableRef(FindTable(table));
        var delete = SqlDml.Delete(tableRef);
        var typeIds = deleteActions[table];
        foreach (var typeId in typeIds)
          delete.Where |= tableRef[typeIdColumnName]==typeId;
        RegisterCommand(delete, postCopy ? SqlUpgradeStage.PostCopyData : SqlUpgradeStage.CleanupData);
      }
    }

    private void GenerateChangeColumnTypeCommands(PropertyChangeAction action)
    {
      var targetColumn = (StorageColumnInfo) targetModel.Resolve(action.Path, true);
      var sourceColumn = (StorageColumnInfo) sourceModel.Resolve(action.Path, true);
      var column = FindColumn(targetColumn.Parent, targetColumn.Name);
      var table = column.Table;
      var originalName = column.Name;
      
      // Rename old column
      var tempName = GetTemporaryName(column);
      RenameColumn(column, tempName);

      // Create new columns
      var newTypeInfo = targetColumn.Type as StorageTypeInfo;
      var newSqlType = (SqlValueType) newTypeInfo.NativeType;
      var newColumn = table.CreateColumn(originalName, newSqlType);
      
      newColumn.IsNullable = newTypeInfo.IsNullable;
      if (!newColumn.IsNullable)
        newColumn.DefaultValue = GetDefaultValueExpression(targetColumn);

      var addNewColumn = SqlDdl.Alter(table, SqlDdl.AddColumn(newColumn));
      RegisterCommand(addNewColumn, SqlUpgradeStage.Upgrade);

      // Copy values if possible to convert type
      if (TypeConversionVerifier.CanConvert(sourceColumn.Type, newTypeInfo)
        || enforceChangedColumns.Contains(sourceColumn.Path, StringComparer.OrdinalIgnoreCase)) {
        var tableRef = SqlDml.TableRef(table);
        var copyValues = SqlDml.Update(tableRef);
        if (newTypeInfo.IsNullable) {
          if (sourceColumn.Type.Type.StripNullable() == typeof(string) && newSqlType.Length < column.DataType.Length)
            copyValues.Values[tableRef[originalName]] = SqlDml.Cast(SqlDml.Substring(tableRef[tempName], 0, newSqlType.Length), newSqlType);
          else
            copyValues.Values[tableRef[originalName]] = SqlDml.Cast(tableRef[tempName], newSqlType);
        }
        else {
          var getValue = SqlDml.Case();
          getValue.Add(SqlDml.IsNull(tableRef[tempName]), GetDefaultValueExpression(targetColumn));
          getValue.Add(SqlDml.IsNotNull(tableRef[tempName]), SqlDml.Cast(tableRef[tempName], newSqlType));
          copyValues.Values[tableRef[originalName]] = getValue;
        }
        RegisterCommand(copyValues, SqlUpgradeStage.Upgrade, true);
      }

      // Drop old column
      if (!column.DefaultValue.IsNullReference()) {
        var constraint = table.TableConstraints
          .OfType<DefaultConstraint>().FirstOrDefault(defaultConstraint => defaultConstraint.Column==column);
        if (constraint!=null)
          RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(constraint)), SqlUpgradeStage.Cleanup);
      }
      var removeOldColumn = SqlDdl.Alter(table, SqlDdl.DropColumn(table.TableColumns[tempName]));
      RegisterCommand(removeOldColumn, SqlUpgradeStage.Cleanup);
      table.TableColumns.Remove(table.TableColumns[tempName]);
    }
    
    #endregion

    #region Helper methods

    private Table CreateTable(TableInfo tableInfo)
    {
      var schema = schemaResolver.ResolveSchema(sqlModel, tableInfo.Parent.Name);
      var table = schema.CreateTable(tableInfo.Name);
      foreach (var columnInfo in tableInfo.Columns)
        CreateColumn(columnInfo, table);
      if (tableInfo.PrimaryIndex!=null)
        CreatePrimaryKey(tableInfo, table);
      createdTables.Add(table);
      return table;
    }

    private TableColumn CreateColumn(StorageColumnInfo columnInfo, Table table)
    {
      var type = (SqlValueType) columnInfo.Type.NativeType;
      var column = table.CreateColumn(columnInfo.Name, type);
      var isPrimaryKeyColumn = columnInfo.Parent.PrimaryIndex!=null
        && columnInfo.Parent.PrimaryIndex.KeyColumns
          .Any(keyColumn => keyColumn.Value==columnInfo);

      if (!column.IsNullable
        && column.Name!=typeIdColumnName
        && !isPrimaryKeyColumn)
        column.DefaultValue = GetDefaultValueExpression(columnInfo);

      column.IsNullable = columnInfo.Type.IsNullable;
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
        if (column.DataType.Type == SqlType.Geometry ||
          column.DataType.Type == SqlType.Geography) {
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
      var schema = schemaResolver.ResolveSchema(sqlModel, sequenceInfo.Parent.Name);
      var sequenceTable = schema.CreateTable(sequenceInfo.Name);
      createdTables.Add(sequenceTable);
      var idColumn = sequenceTable.CreateColumn(WellKnown.GeneratorColumnName,
        (SqlValueType) sequenceInfo.Type.NativeType);
      idColumn.SequenceDescriptor =
        new SequenceDescriptor(
          idColumn,
          sequenceInfo.Current ?? sequenceInfo.Seed,
          sequenceInfo.Increment);
      sequenceTable.CreatePrimaryKey(string.Format("PK_{0}", sequenceInfo.Name), idColumn);
      if (!providerInfo.Supports(ProviderFeatures.InsertDefaultValues)) {
        var fakeColumn = sequenceTable.CreateColumn(WellKnown.GeneratorFakeColumnName, driver.BuildValueType(typeof(int)));
        fakeColumn.IsNullable = true;
      }
      RegisterCommand(SqlDdl.Create(sequenceTable));
    }

    private void DropGeneratorTable(StorageSequenceInfo sequenceInfo)
    {
      var schema = schemaResolver.ResolveSchema(sqlModel, sequenceInfo.Parent.Name);
      var sequenceTable = schema.Tables[sequenceInfo.Name];
      RegisterCommand(SqlDdl.Drop(sequenceTable));
      schema.Tables.Remove(sequenceTable);
    }

    private void RenameSchemaTable(Table table, string newName)
    {
      var schema = table.Schema;

      // Renamed table must be removed and added with new name
      // for reregistring in dictionary
      schema.Tables.Remove(table);
      table.Name = newName;
      schema.Tables.Add(table);
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

    private string GetTemporaryName(TableColumn column)
    {
      var tempName = string.Format(TemporaryNameFormat, column.Name);
      var counter = 0;
      while (column.Table.Columns.Any(tableColumn => StringComparer.Compare(tableColumn.Name, tempName) == 0))
        tempName = string.Format(TemporaryNameFormat, column.Name + ++counter);

      return tempName;
    }

    private Table FindTable(TableInfo tableInfo)
    {
      var name = tableInfo.Name;
      var schema = schemaResolver.ResolveSchema(sqlModel, tableInfo.Parent.Name);
      return schema.Tables[name];
    }

    private TableColumn FindColumn(Table table, string name)
    {
      return table.TableColumns.
        FirstOrDefault(c => StringComparer.Compare(c.Name, name) == 0);
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
        if (selectColumns.Count() == 0)
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
        if (identityConstantPairs.Count() == 0)
          throw new InvalidOperationException(Strings.ExIncorrectCommandParameters);
        SqlExpression expression = null;
        identityConstantPairs.ForEach(pair => 
          expression &= table[pair.First.Name]==SqlDml.Literal(pair.Second));
        return expression;
      }
    }

    private void RegisterCommand(ISqlCompileUnit command)
    {
      RegisterCommand(command, GetSqlUpgradeStage(currentUpgradeStage), false);
    }

    private void RegisterCommand(ISqlCompileUnit command, SqlUpgradeStage stage)
    {
      RegisterCommand(command, stage, false);
    }

    private void RegisterCommand(ISqlCompileUnit command, SqlUpgradeStage stage, bool inNewBatch)
    {
      var commands = new List<string>();
      if (inNewBatch)
        commands.Add(string.Empty);
      commands.Add(driver.Compile(command).GetCommandText());
      switch(stage) {
        case SqlUpgradeStage.NonTransactionalProlog:
          translationResult.NonTransactionalPrologCommands.AddRange(commands);
          break;
        case SqlUpgradeStage.NonTransactionalEpilog:
          translationResult.NonTransactionalEpilogCommands.AddRange(commands);
          break;
        case SqlUpgradeStage.PreCleanupData:
          translationResult.PreCleanupDataCommands.AddRange(commands);
          break;
        case SqlUpgradeStage.CleanupData:
          translationResult.CleanupDataCommands.AddRange(commands);
          break;
        case SqlUpgradeStage.PreUpgrade:
          translationResult.PreUpgradeCommands.AddRange(commands);
          break;
        case SqlUpgradeStage.Upgrade:
          translationResult.UpgradeCommands.AddRange(commands);
          break;
        case SqlUpgradeStage.CopyData:
          translationResult.CopyDataCommands.AddRange(commands);
          break;
        case SqlUpgradeStage.PostCopyData:
          translationResult.PostCopyDataCommands.AddRange(commands);
          break;
        case SqlUpgradeStage.Cleanup:
          translationResult.CleanupCommands.AddRange(commands);
          break;
        default:
          throw new ArgumentOutOfRangeException("stage");
      }
    }

    private SqlExpression GetDefaultValueExpression(StorageColumnInfo columnInfo)
    {
      var result = columnInfo.DefaultValue==null
        ? (SqlExpression) SqlDml.Null
        : SqlDml.Literal(columnInfo.DefaultValue);
      return result;
    }

    private long? GetCurrentSequenceValue(StorageSequenceInfo sequenceInfo)
    {
      var schema = schemaResolver.ResolveSchema(sqlModel, sequenceInfo.Parent.Name);
      var generatorNode = providerInfo.Supports(ProviderFeatures.Sequences)
        ? schema.Sequences[sequenceInfo.Name]
        : (SchemaNode) schema.Tables[sequenceInfo.Name];
      return sequenceQueryBuilder.Build(generatorNode, 0).ExecuteWith(sqlExecutor);
    }

    private static SqlUpgradeStage GetSqlUpgradeStage(Modelling.Comparison.UpgradeStage stage)
    {
      switch (stage) {
        case Modelling.Comparison.UpgradeStage.CleanupData:
          return SqlUpgradeStage.CleanupData;
        case Modelling.Comparison.UpgradeStage.Prepare:
        case Modelling.Comparison.UpgradeStage.TemporaryRename:
          return SqlUpgradeStage.PreUpgrade;
        case Modelling.Comparison.UpgradeStage.Upgrade:
          return SqlUpgradeStage.Upgrade;
        case Modelling.Comparison.UpgradeStage.CopyData:
          return SqlUpgradeStage.CopyData;
        case Modelling.Comparison.UpgradeStage.PostCopyData:
          return SqlUpgradeStage.PostCopyData;
        case Modelling.Comparison.UpgradeStage.Cleanup:
          return SqlUpgradeStage.Cleanup;
        default:
          throw new ArgumentOutOfRangeException("stage");
      }
    }

    #endregion
    

    // Constructors

    public SqlActionTranslator(
      HandlerAccessor handlers, ISqlExecutor sqlExecutor,
      ActionSequence actions, SqlExtractionResult sqlModel, StorageModel sourceModel, StorageModel targetModel,
      List<string> enforceChangedColumns, bool allowCreateConstraints)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      ArgumentValidator.EnsureArgumentNotNull(sqlExecutor, "sqlExecutor");
      ArgumentValidator.EnsureArgumentNotNull(actions, "actions");
      ArgumentValidator.EnsureArgumentNotNull(sqlModel, "sqlModel");
      ArgumentValidator.EnsureArgumentNotNull(sourceModel, "sourceModel");
      ArgumentValidator.EnsureArgumentNotNull(targetModel, "targetModel");
      ArgumentValidator.EnsureArgumentNotNull(enforceChangedColumns, "enforceChangedColumns");

      driver = handlers.StorageDriver;
      schemaResolver = handlers.SchemaResolver;
      providerInfo = handlers.ProviderInfo;
      sequenceQueryBuilder = handlers.SequenceQueryBuilder;
      providerInfo = driver.ProviderInfo;
      typeIdColumnName = handlers.NameBuilder.TypeIdColumnName;

      this.sqlModel = sqlModel;
      this.actions = actions;
      this.sourceModel = sourceModel;
      this.targetModel = targetModel;
      this.enforceChangedColumns = enforceChangedColumns;
      this.sqlExecutor = sqlExecutor;
      this.allowCreateConstraints = allowCreateConstraints;
    }
  }
}