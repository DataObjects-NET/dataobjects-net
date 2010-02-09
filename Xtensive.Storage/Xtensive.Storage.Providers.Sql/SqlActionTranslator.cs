// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Sorting;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Sql;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers.Sql.Resources;
using ColumnInfo = Xtensive.Storage.Indexing.Model.ColumnInfo;
using ReferentialAction = Xtensive.Storage.Indexing.Model.ReferentialAction;
using SequenceInfo = Xtensive.Storage.Indexing.Model.SequenceInfo;
using SqlRefAction = Xtensive.Sql.ReferentialAction;
using TableInfo = Xtensive.Storage.Indexing.Model.TableInfo;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Translates upgrade <see cref="NodeAction"/>s to Sql.
  /// </summary>
  internal sealed class SqlActionTranslator
  {
    private const string SubqueryAliasName = "th";
    private const string TemporaryNameFormat = "Temp{0}";
    private const string SubqueryTableAliasNameFormat = "a{0}";
    private const string ColumnTypePropertyName = "Type";

    private readonly ProviderInfo providerInfo;
    private readonly string typeIdColumnName;
    private readonly List<string> enforceChangedColumns = new List<string>();
    private readonly Func<ISqlCompileUnit, object> scalarExecutor;
    private readonly Func<ISqlCompileUnit, int> nonQueryExecutor;
    private readonly Func<ProviderInfo, Schema, string, ISqlCompileUnit> getNextImplementationHandler;

    private readonly ActionSequence actions;
    private readonly Schema schema;
    private readonly StorageInfo sourceModel;
    private readonly StorageInfo targetModel;
    private readonly Driver driver;
    
    private readonly List<string> preUpgradeCommands = new List<string>();
    private readonly List<string> upgradeCommands = new List<string>();
    private readonly List<string> dataManipulateCommands = new List<string>();
    private readonly List<string> postUpgradeCommands = new List<string>();
    private readonly List<string> nonTransactionalEpilogCommands = new List<string>();
    private readonly List<string> nonTransactionalPrologCommands = new List<string>();
    
    private bool translated;
    private readonly List<Table> createdTables = new List<Table>();
    private readonly List<Sequence> createdSequences = new List<Sequence>();
    private readonly List<PropertyChangeAction> changeColumnTypeActions = new List<PropertyChangeAction>();
    private readonly List<DataAction> clearDataActions = new List<DataAction>();
    private UpgradeStage stage;

    private bool IsSequencesAllowed
    {
      get { return providerInfo.Supports(ProviderFeatures.Sequences); }
    }

    /// <summary>
    /// Gets the command thats must be 
    /// executed before upgrade commands.
    /// </summary>
    public List<string> PreUpgradeCommands
    {
      get
      {
        if (!translated)
          throw new InvalidOperationException("Commands are not translated yet.");
        return preUpgradeCommands;
      }
    }

    /// <summary>
    /// Gets the translation result.
    /// </summary>
    public List<string> UpgradeCommands
    {
      get
      {
        if (!translated)
          throw new InvalidOperationException("Commands are not translated yet.");
        return upgradeCommands;
      }
    }

    /// <summary>
    /// Gets the data manipulate commands.
    /// </summary>
    public List<string> DataManipulateCommands
    {
      get
      {
        if (!translated)
          throw new InvalidOperationException("Commands are not translated yet.");
        return dataManipulateCommands;
      }
    }

    /// <summary>
    /// Gets the post upgrade commands, thats
    /// must be executed after data manipulate commands.
    /// </summary>
    public List<string> PostUpgradeCommands
    {
      get
      {
        if (!translated)
          throw new InvalidOperationException("Commands are not translated yet.");
        return postUpgradeCommands;
      }
    }

    /// <summary>
    /// Gets the non transactional commands that should be executed non transactionally.
    /// </summary>
    public List<string> NonTransactionalPrologCommands
    {
      get
      {
        if (!translated)
          throw new InvalidOperationException("Commands are not translated yet.");
        return nonTransactionalPrologCommands;
      }
    }


    /// <summary>
    /// Gets the non transactional commands that should be executed non transactionally.
    /// </summary>
    public List<string> NonTransactionalEpilogCommands
    {
      get
      {
        if (!translated)
          throw new InvalidOperationException("Commands are not translated yet.");
        return nonTransactionalEpilogCommands;
      }
    }

    /// <summary>
    /// Translates all registered actions.
    /// </summary>
    public void Translate()
    {
      // Prepairing
      stage = UpgradeStage.Prepare;
      // Turn off deferred contraints
      if (providerInfo.Supports(ProviderFeatures.DeferrableConstraints))
        RegisterCommand(SqlDdl.Command(SqlCommandType.SetConstraintsAllImmediate), NonTransactionalStage.None);
      var prepare = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==UpgradeStage.Prepare.ToString());
      if (prepare!=null)
        VisitAction(prepare);
      ProcessClearDataActions();
      // Mutual renaming
      stage = UpgradeStage.TemporaryRename;
      var cycleRename = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==UpgradeStage.TemporaryRename.ToString());
      if (cycleRename!=null)
        VisitAction(cycleRename);
      // Upgrading
      stage = UpgradeStage.Upgrade;
      var upgrade = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==UpgradeStage.Upgrade.ToString());
      if (upgrade!=null)
        VisitAction(upgrade);
      // Data manipulating
      stage = UpgradeStage.DataManipulate;
      // Process column type changes
      changeColumnTypeActions.Apply(GenerateChangeColumnTypeCommands);
      var dataManipulate = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==UpgradeStage.DataManipulate.ToString());
      if (dataManipulate!=null)
        VisitAction(dataManipulate);
      // Cleanup
      stage = UpgradeStage.Cleanup;
      var cleanup = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==UpgradeStage.Cleanup.ToString());
      if (cleanup!=null)
        VisitAction(cleanup);
      // Turn on deferred contraints
      if (providerInfo.Supports(ProviderFeatures.DeferrableConstraints))
        RegisterCommand(SqlDdl.Command(SqlCommandType.SetConstraintsAllDeferred), NonTransactionalStage.None);
      translated = true;
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
      else if (action.Type == typeof(ColumnInfo))
        VisitCreateColumnAction(action);
      else if (action.Type == typeof(PrimaryIndexInfo))
        VisitCreatePrimaryKeyAction(action);
      else if (action.Type == typeof(SecondaryIndexInfo))
        VisitCreateSecondaryIndexAction(action);
      else if (action.Type == typeof(ForeignKeyInfo))
        VisitCreateForeignKeyAction(action);
      else if (action.Type == typeof(SequenceInfo))
        VisitCreateSequenceAction(action);
      else if (action.Type == typeof(FullTextIndexInfo))
        VisitCreateFullTextIndexAction(action);
    }

    private void VisitRemoveAction(RemoveNodeAction action)
    {
      var node = action.Difference.Source;
      if (node.GetType() == typeof(TableInfo))
        VisitRemoveTableAction(action);
      else if (node.GetType() == typeof(ColumnInfo))
        VisitRemoveColumnAction(action);
      else if (node.GetType() == typeof(PrimaryIndexInfo))
        VisitRemovePrimaryKeyAction(action);
      else if (node.GetType() == typeof(SecondaryIndexInfo))
        VisitRemoveSecondaryIndexAction(action);
      else if (node.GetType() == typeof(ForeignKeyInfo))
        VisitRemoveForeignKeyAction(action);
      else if (node.GetType() == typeof(SequenceInfo))
        VisitRemoveSequenceAction(action);
      else if (node.GetType() == typeof(FullTextIndexInfo))
        VisitRemoveFullTextIndexAction(action);
    }

    private void VisitAlterAction(NodeAction action)
    {
      var propertyChangeAction = action as PropertyChangeAction;
      if (propertyChangeAction != null) {
        var changedNode = targetModel.Resolve(propertyChangeAction.Path);
        if (changedNode.GetType()==typeof (SequenceInfo))
          VisitAlterSequenceAction(propertyChangeAction);
        else if (changedNode.GetType()==typeof (ColumnInfo))
          VisitAlterColumnAction(propertyChangeAction);
        return;
      }

      var moveNodeAction = action as MoveNodeAction;
      if (moveNodeAction==null)
        return;
      var node = moveNodeAction.Difference.Source;
      if (node.GetType()==typeof (TableInfo))
        VisitAlterTableAction(moveNodeAction);
      else if (node.GetType()==typeof (ColumnInfo))
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
      var hint = action.DataHint as CopyDataHint;
      var copiedColumns = hint.CopiedColumns
        .Select(pair => new Pair<ColumnInfo>(
          sourceModel.Resolve(pair.First) as ColumnInfo,
          targetModel.Resolve(pair.Second) as ColumnInfo)).ToArray();
      var identityColumns = hint.Identities
        .Select(pair => new Pair<ColumnInfo>(
          sourceModel.Resolve(pair.Source) as ColumnInfo,
          targetModel.Resolve(pair.Target) as ColumnInfo)).ToArray();
      if (copiedColumns.Length == 0 || identityColumns.Length == 0)
        throw new InvalidOperationException(Strings.ExIncorrectCommandParameters);

      var fromTable = FindTable(copiedColumns[0].First.Parent.Name);
      var toTable = FindTable(copiedColumns[0].Second.Parent.Name);
      var toTableRef = SqlDml.TableRef(toTable);
      var update = SqlDml.Update(toTableRef);

      if (providerInfo.Supports(ProviderFeatures.UpdateFrom)) {
        var fromTableRef = SqlDml.TableRef(fromTable);
        var select = SqlDml.Select(fromTableRef);
        identityColumns.Apply(pair => select.Columns.Add(fromTableRef[pair.First.Name]));
        copiedColumns.Apply(pair => select.Columns.Add(fromTableRef[pair.First.Name]));
        var selectRef = SqlDml.QueryRef(select, SubqueryAliasName);
        update.From = selectRef;
        copiedColumns.Apply(pair => update.Values[toTableRef[pair.Second.Name]] = selectRef[pair.First.Name]);
        identityColumns.Apply(pair => update.Where &= toTableRef[pair.Second.Name]==selectRef[pair.First.Name]);
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
      RegisterCommand(update, NonTransactionalStage.None);
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
      RegisterCommand(SqlDdl.Create(table), NonTransactionalStage.None);
    }

    private void VisitRemoveTableAction(RemoveNodeAction action)
    {
      var tableInfo = action.Difference.Source as TableInfo;
      var table = FindTable(tableInfo.Name);
      RegisterCommand(SqlDdl.Drop(table), NonTransactionalStage.None);
      schema.Tables.Remove(table);
    }

    private void VisitAlterTableAction(MoveNodeAction action)
    {
      var oldTableInfo = sourceModel.Resolve(action.Path) as TableInfo;
      var table = FindTable(oldTableInfo.Name);
      RegisterCommand(SqlDdl.Rename(table, action.Name), NonTransactionalStage.None);
      oldTableInfo.Name = action.Name;
      RenameSchemaTable(table, action.Name);
    }
    
    private void VisitAlterTableAction(NodeAction action)
    {
      throw new NotSupportedException();
    }

    private void VisitCreateColumnAction(CreateNodeAction createColumnAction)
    {
      var columnInfo = createColumnAction.Difference.Target as ColumnInfo;
      var table = FindTable(columnInfo.Parent.Name);
      
      // Ensure table is not newly created
      if (createdTables.Contains(table))
        return;
      
      var column = CreateColumn(columnInfo, table);
      if (columnInfo.DefaultValue != null)
        column.DefaultValue = SqlDml.Literal(columnInfo.DefaultValue);
      RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddColumn(column)), NonTransactionalStage.None);
    }

    private void VisitRemoveColumnAction(RemoveNodeAction removeColumnAction)
    {
      var columnInfo = removeColumnAction.Difference.Source as ColumnInfo;
      var table = FindTable(columnInfo.Parent.Name);

      // Ensure table is not removed
      if (table==null)
        return;

      var column = FindColumn(table, columnInfo.Name);
      if (column == null)
        return;

      if (column.DefaultValue!=null) {
        var constraint = table.TableConstraints
          .OfType<DefaultConstraint>()
          .FirstOrDefault(defaultConstraint => defaultConstraint.Column==column);
        if (constraint!=null)
          RegisterCommand(SqlDdl.Alter(table,
            SqlDdl.DropConstraint(constraint)), NonTransactionalStage.None);
      }
      RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropColumn(column)), NonTransactionalStage.None);
      table.TableColumns.Remove(column);
    }

    private void VisitAlterColumnAction(PropertyChangeAction action)
    {
      var isNewlyCreatedColumn = action.Difference.Source==null;
      if (isNewlyCreatedColumn)
        return; // Properties already initilized

      if (!action.Properties.ContainsKey(ColumnTypePropertyName))
        return;
      
      changeColumnTypeActions.Add(action);
    }

    private void VisitMoveColumnAction(MoveNodeAction action)
    {
      var movementInfo = ((NodeDifference) action.Difference).MovementInfo;
      if ((movementInfo & MovementInfo.NameChanged)!=0) {
        // Process name changing
        var oldColumnInfo = sourceModel.Resolve(action.Path) as ColumnInfo;
        var column = FindColumn(oldColumnInfo.Parent.Name, oldColumnInfo.Name);
        RenameColumn(column, action.Name);
        oldColumnInfo.Name = action.Name;
      }
      else
        throw new NotSupportedException();
    }

    private void RenameColumn(TableColumn column, string name)
    {
      if (providerInfo.Supports(ProviderFeatures.ColumnRename)) {
        RegisterCommand(SqlDdl.Rename(column, name), UpgradeStage.Upgrade, NonTransactionalStage.None);
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
      RegisterCommand(addColumnWithNewType, UpgradeStage.Upgrade, NonTransactionalStage.None);

      // Copy data
      var tableRef = SqlDml.TableRef(column.Table);
      var update = SqlDml.Update(tableRef);
      update.Values[tableRef[name]] = tableRef[originalName];
      RegisterCommand(update, UpgradeStage.Upgrade, NonTransactionalStage.None);

      // Drop old column
      if (column.DefaultValue!=null) {
        var constraint = table.TableConstraints
          .OfType<DefaultConstraint>().FirstOrDefault(defaultConstraint => defaultConstraint.Column==column);
        if (constraint!=null)
          RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(constraint)), UpgradeStage.Upgrade, NonTransactionalStage.None);
      }
      var removeOldColumn = SqlDdl.Alter(column.Table, SqlDdl.DropColumn(column));
      RegisterCommand(removeOldColumn, UpgradeStage.Upgrade, NonTransactionalStage.None);
      table.TableColumns.Remove(column);
    }

    private void VisitCreatePrimaryKeyAction(CreateNodeAction action)
    {
      var primaryIndex = action.Difference.Target as PrimaryIndexInfo;
      var table = FindTable(primaryIndex.Parent.Name);

      // Ensure table is not newly created
      if (createdTables.Contains(table))
        return;

      var primaryKey = CreatePrimaryKey(primaryIndex.Parent, table);
      RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddConstraint(primaryKey)), NonTransactionalStage.None);
    }

    private void VisitRemovePrimaryKeyAction(RemoveNodeAction action)
    {
      var primaryIndexInfo = action.Difference.Source as PrimaryIndexInfo;
      var table = FindTable(primaryIndexInfo.Parent.Name);

      // Ensure table is not removed
      if (table==null)
        return;
      
      var primaryKey = table.TableConstraints[primaryIndexInfo.Name];

      RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(primaryKey)), NonTransactionalStage.None);
      table.TableConstraints.Remove(primaryKey);
    }

    private void VisitAlterPrimaryKeyAction(NodeAction action)
    {
      throw new NotSupportedException();
    }
    
    private void VisitCreateSecondaryIndexAction(CreateNodeAction action)
    {
      var secondaryIndexInfo = action.Difference.Target as SecondaryIndexInfo;
      var table = FindTable(secondaryIndexInfo.Parent.Name);
      var index = CreateSecondaryIndex(table, secondaryIndexInfo);
      RegisterCommand(SqlDdl.Create(index), NonTransactionalStage.None);
    }

    private void VisitRemoveSecondaryIndexAction(RemoveNodeAction action)
    {
      var secondaryIndexInfo = action.Difference.Source as SecondaryIndexInfo;
      var table = FindTable(secondaryIndexInfo.Parent.Name);
      
      // Ensure table is not removed
      if (table==null)
        return;
      
      var index = table.Indexes[secondaryIndexInfo.Name];
      RegisterCommand(SqlDdl.Drop(index), NonTransactionalStage.None);
      table.Indexes.Remove(index);
    }

    private void VisitAlterSecondaryIndexAction(NodeAction action)
    {
      throw new NotSupportedException();
    }
    
    private void VisitCreateForeignKeyAction(CreateNodeAction action)
    {
      var foreignKeyInfo = action.Difference.Target as ForeignKeyInfo;
      var table = FindTable(foreignKeyInfo.Parent.Name);
      var foreignKey = CreateForeignKey(foreignKeyInfo);

      RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddConstraint(foreignKey)), NonTransactionalStage.None);
    }

    private void VisitRemoveForeignKeyAction(RemoveNodeAction action)
    {
      var foreignKeyInfo = action.Difference.Source as ForeignKeyInfo;
      var table = FindTable(foreignKeyInfo.Parent.Name);

      // Ensure table is not removed
      if (table==null)
        return;

      var foreignKey = table.TableConstraints[foreignKeyInfo.Name];
      RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(foreignKey)), NonTransactionalStage.None);
      table.TableConstraints.Remove(foreignKey);
    }

    private void VisitAlterForeignKeyAction(NodeAction action)
    {
      throw new NotSupportedException();
    }

    private void VisitCreateSequenceAction(CreateNodeAction action)
    {
      var sequenceInfo = action.Difference.Target as SequenceInfo;
      if (IsSequencesAllowed) {
        var sequence = schema.CreateSequence(sequenceInfo.Name);
        sequence.SequenceDescriptor = new SequenceDescriptor(sequence,
          sequenceInfo.Seed, sequenceInfo.Increment);
        sequence.SequenceDescriptor.MinValue = sequenceInfo.Seed;
        sequence.DataType = GetSqlType(sequenceInfo.OriginalType);
        RegisterCommand(SqlDdl.Create(sequence), NonTransactionalStage.None);
        createdSequences.Add(sequence);
      }
      else {
        CreateGeneratorTable(sequenceInfo);
      }
    }

    private void VisitRemoveSequenceAction(RemoveNodeAction action)
    {
      var sequenceInfo = action.Difference.Source as SequenceInfo;
      if (IsSequencesAllowed) {
        var sequence = schema.Sequences[sequenceInfo.Name];
        RegisterCommand(SqlDdl.Drop(sequence), NonTransactionalStage.None);
        schema.Sequences.Remove(sequence);
      }
      else {
        DropGeneratorTable(sequenceInfo);
      }
    }

    private void VisitAlterSequenceAction(PropertyChangeAction action)
    {
      var sequenceInfo = targetModel.Resolve(action.Path) as SequenceInfo;

      // Check if sequence is not newly created
      if ((IsSequencesAllowed 
        && createdSequences.Any(sequence => sequence.Name==sequenceInfo.Name))
        || createdTables.Any(table => table.Name==sequenceInfo.Name))
        return;

      var currentValue = GetCurrentSequenceValue(sequenceInfo.Name);
      var newStartValue = currentValue + sequenceInfo.Increment;
      if (IsSequencesAllowed) {
        var exisitingSequence = schema.Sequences[sequenceInfo.Name];
        var newSequenceDescriptor = new SequenceDescriptor(exisitingSequence, null, sequenceInfo.Increment);
        exisitingSequence.SequenceDescriptor = newSequenceDescriptor;
        RegisterCommand(SqlDdl.Alter(exisitingSequence, newSequenceDescriptor), NonTransactionalStage.None);
      }
      else {
        sequenceInfo.Current = newStartValue;
        DropGeneratorTable(sequenceInfo);
        CreateGeneratorTable(sequenceInfo);
      }
    }

    private void VisitCreateFullTextIndexAction(CreateNodeAction action)
    {
      var fullTextIndexInfo = (FullTextIndexInfo) action.Difference.Target;
      var fullTextSupported = providerInfo.Supports(ProviderFeatures.FullText);
      if (!fullTextSupported)
        return;
      var table = FindTable(fullTextIndexInfo.Parent.Name);
      var ftIndex = table.CreateFullTextIndex(fullTextIndexInfo.Name);
      ftIndex.UnderlyingUniqueIndex = fullTextIndexInfo.Parent.PrimaryIndex.EscapedName;
      ftIndex.FullTextCatalog = "Default";
      foreach (var column in fullTextIndexInfo.Columns) {
        var tableColumn = FindColumn(table, column.Value.Name);
        var ftColumn = ftIndex.CreateIndexColumn(tableColumn);
        ftColumn.Languages.Add(new Language(column.Language));
      }
      var transactionalStage = providerInfo.Supports(ProviderFeatures.FullTextDdlIsNotTransactional)
        ? NonTransactionalStage.Epilog
        : NonTransactionalStage.None;
      RegisterCommand(SqlDdl.Create(ftIndex), transactionalStage);
    }

    private void VisitRemoveFullTextIndexAction(RemoveNodeAction action)
    {
      var fullTextIndexInfo = (FullTextIndexInfo)action.Difference.Source;
      var fullTextSupported = providerInfo.Supports(ProviderFeatures.FullText);
      if (!fullTextSupported)
        return;
      var table = FindTable(fullTextIndexInfo.Parent.Name);
      var ftIndex = table.Indexes[fullTextIndexInfo.Name]
        ?? table.Indexes.OfType<FullTextIndex>().Single();

      var transactionalStage = providerInfo.Supports(ProviderFeatures.FullTextDdlIsNotTransactional)
        ? NonTransactionalStage.Prolog
        : NonTransactionalStage.None;
      RegisterCommand(SqlDdl.Drop(ftIndex), transactionalStage);
      table.Indexes.Remove(ftIndex);
    }

    private void ProcessClearDataActions()
    {
      var updateActions = clearDataActions.Where(action => action.DataHint is UpdateDataHint).ToList();
      var deleteFromConnectorTableActions = new List<DataAction>();
      var deleteFromAncestorTableActions = new List<DataAction>();
      foreach (var deleteAction in clearDataActions.Where(action=>action.DataHint is DeleteDataHint)) {
        var hint = deleteAction.DataHint as DeleteDataHint;
        if (hint.Identities.Any(pair => !pair.IsIdentifiedByConstant))
          deleteFromConnectorTableActions.Add(deleteAction);
        else
          deleteFromAncestorTableActions.Add(deleteAction);
      }

      updateActions.Apply(ProcessUpdateDataAction);
      deleteFromConnectorTableActions.Apply(ProcessDeleteDataAction);
      ProcessClearAncestorsActions(deleteFromAncestorTableActions);
    }

    private void ProcessDeleteDataAction(DataAction action)
    {
      var hint = action.DataHint as DeleteDataHint;
      var soureTableInfo = sourceModel.Resolve(hint.SourceTablePath) as TableInfo;
      var table = SqlDml.TableRef(FindTable(soureTableInfo.Name));
      var delete = SqlDml.Delete(table);
      
      delete.Where = CreateConditionalExpression(hint, table);

      RegisterCommand(delete, UpgradeStage.Prepare, NonTransactionalStage.None);
    }

    /// <exception cref="InvalidOperationException">Can not create update command 
    /// with specific hint parameters.</exception>
    private void ProcessUpdateDataAction(DataAction action)
    {
      var hint = action.DataHint as  UpdateDataHint;
      var soureTableInfo = sourceModel.Resolve(hint.SourceTablePath) as TableInfo;
      var table = SqlDml.TableRef(FindTable(soureTableInfo.Name));
      var update = SqlDml.Update(table);

      var updatedColumns = hint.UpdateParameter
        .Select(pair => new Pair<ColumnInfo, object>(
          sourceModel.Resolve(pair.First) as ColumnInfo,
          pair.Second)).ToArray();
      if (updatedColumns.Length==0)
        throw new InvalidOperationException(Strings.ExIncorrectCommandParameters);
      foreach (var pair in updatedColumns)
        if (pair.Second==null)
          update.Values[table[pair.First.Name]] = SqlDml.DefaultValue;
        else
          update.Values[table[pair.First.Name]] = SqlDml.Literal(pair.Second);

      update.Where = CreateConditionalExpression(hint, table);
      
      RegisterCommand(update, UpgradeStage.Prepare, NonTransactionalStage.None);
    }

    private void ProcessClearAncestorsActions(List<DataAction> originalActions)
    {
      if (originalActions.Count==0)
        return;

      // Merge actions
      var deleteActions = new Dictionary<TableInfo, List<string>>();
      foreach (var action in originalActions) {
        var sourceTableInfo = sourceModel.Resolve(action.DataHint.SourceTablePath) as TableInfo;
        List<string> list;
        if (!deleteActions.TryGetValue(sourceTableInfo, out list)) {
          list = new List<string>();
          deleteActions.Add(sourceTableInfo, list);
        }
        list.AddRange(action.DataHint.Identities.Select(pair => pair.Target));
      }

      // Sort actions topologicaly according to foreign keys
      var nodes = new List<Node<TableInfo, ForeignKeyInfo>>();
      var foreignKeys = sourceModel.Tables.SelectMany(table => table.ForeignKeys).ToList();
      foreach (var pair in deleteActions)
        nodes.Add(new Node<TableInfo, ForeignKeyInfo>(pair.Key));
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
        var tableRef = SqlDml.TableRef(FindTable(table.Name));
        var delete = SqlDml.Delete(tableRef);
        var typeIds = deleteActions[table];
        foreach (var typeId in typeIds)
          delete.Where |= tableRef[typeIdColumnName]==typeId;
        RegisterCommand(delete, UpgradeStage.Prepare, NonTransactionalStage.None);
      }
    }

    private void GenerateChangeColumnTypeCommands(PropertyChangeAction action)
    {
      var targetColumn = targetModel.Resolve(action.Path) as ColumnInfo;
      var sourceColumn = sourceModel.Resolve(action.Path) as ColumnInfo;
      var column = FindColumn(targetColumn.Parent.Name, targetColumn.Name);
      var table = column.Table;
      var originalName = column.Name;
      
      // Rename old column
      var tempName = GetTemporaryName(column);
      RenameColumn(column, tempName);

      // Create new columns
      var newTypeInfo = action.Properties[ColumnTypePropertyName] as TypeInfo;
      var type = GetSqlType(newTypeInfo);
      var newColumn = table.CreateColumn(originalName, type);
      newColumn.IsNullable = newTypeInfo.IsNullable;
      if (!newColumn.IsNullable)
        newColumn.DefaultValue = GetDefaultValueExpression(targetColumn);
      var addColumnWithNewType = SqlDdl.Alter(table, SqlDdl.AddColumn(newColumn));
      RegisterCommand(addColumnWithNewType, UpgradeStage.Upgrade, NonTransactionalStage.None);

      // Copy values if possible to convert type
      if (Upgrade.TypeConversionVerifier.CanConvert(sourceColumn.Type, newTypeInfo)
        || enforceChangedColumns.Contains(sourceColumn.Path)) {
        var tableRef = SqlDml.TableRef(table);
        var copyValues = SqlDml.Update(tableRef);
        if (newTypeInfo.IsNullable)
          copyValues.Values[tableRef[originalName]] = SqlDml.Cast(tableRef[tempName], type);
        else {
          var getValue = SqlDml.Case();
          getValue.Add(SqlDml.IsNull(tableRef[tempName]), GetDefaultValueExpression(targetColumn));
          getValue.Add(SqlDml.IsNotNull(tableRef[tempName]), SqlDml.Cast(tableRef[tempName], type));
          copyValues.Values[tableRef[originalName]] = getValue;
        }
        RegisterCommand(copyValues, UpgradeStage.DataManipulate, NonTransactionalStage.None);
      }

      // Drop old column
      if (column.DefaultValue!=null) {
        var constraint = table.TableConstraints
          .OfType<DefaultConstraint>().FirstOrDefault(defaultConstraint => defaultConstraint.Column==column);
        if (constraint!=null)
          RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(constraint)), UpgradeStage.Cleanup, NonTransactionalStage.None);
      }
      var removeOldColumn = SqlDdl.Alter(table, SqlDdl.DropColumn(table.TableColumns[tempName]));
      RegisterCommand(removeOldColumn, UpgradeStage.Cleanup, NonTransactionalStage.None);
      table.TableColumns.Remove(table.TableColumns[tempName]);
    }
    
    #endregion

    #region Helper methods

    private Table CreateTable(TableInfo tableInfo)
    {
      var table = schema.CreateTable(tableInfo.Name);
      foreach (var columnInfo in tableInfo.Columns)
        CreateColumn(columnInfo, table);
      if (tableInfo.PrimaryIndex!=null)
        CreatePrimaryKey(tableInfo, table);
      createdTables.Add(table);
      return table;
    }

    private TableColumn CreateColumn(ColumnInfo columnInfo, Table table)
    {
      var type = GetSqlType(columnInfo.OriginalType);
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
      var referencingTable = FindTable(foreignKeyInfo.Parent.Name);
      var foreignKey = referencingTable.CreateForeignKey(foreignKeyInfo.Name);
      foreignKey.OnUpdate = ConvertReferentialAction(foreignKeyInfo.OnUpdateAction);
      foreignKey.OnDelete = ConvertReferentialAction(foreignKeyInfo.OnRemoveAction);
      foreignKey.IsDeferrable = providerInfo.Supports(ProviderFeatures.DeferrableConstraints);
      foreignKey.IsInitiallyDeferred = foreignKey.IsDeferrable;
      var referencingColumns = foreignKeyInfo.ForeignKeyColumns
        .Select(cr => FindColumn(referencingTable, cr.Value.Name));
      foreignKey.Columns.AddRange(referencingColumns);
      var referencedTable = FindTable(foreignKeyInfo.PrimaryKey.Parent.Name);
      var referencedColumns = foreignKeyInfo.PrimaryKey.KeyColumns
        .Select(cr => FindColumn(referencedTable, cr.Value.Name));
      foreignKey.ReferencedTable = referencedTable;
      foreignKey.ReferencedColumns.AddRange(referencedColumns);
      return foreignKey;
    }

    private PrimaryKey CreatePrimaryKey(TableInfo tableInfo, Table table)
    {
      return
        table.CreatePrimaryKey(tableInfo.PrimaryIndex.Name,
          tableInfo.PrimaryIndex.KeyColumns
            .Select(cr => FindColumn(table, cr.Value.Name))
            .ToArray());
    }

    private Index CreateSecondaryIndex(Table table, SecondaryIndexInfo indexInfo)
    {
      var index = table.CreateIndex(indexInfo.Name);
      index.IsUnique = indexInfo.IsUnique;
      foreach (var keyColumn in indexInfo.KeyColumns)
        index.CreateIndexColumn(
          FindColumn(table, keyColumn.Value.Name),
          keyColumn.Direction==Direction.Positive);
      index.NonkeyColumns.AddRange(
        indexInfo.IncludedColumns
          .Select(cr => FindColumn(table, cr.Value.Name)).ToArray());
      return index;
    }

    private void CreateGeneratorTable(SequenceInfo sequenceInfo)
    {
      var sequenceTable = schema.CreateTable(sequenceInfo.Name);
      createdTables.Add(sequenceTable);
      var idColumn = sequenceTable.CreateColumn(WellKnown.GeneratorColumnName,
        GetSqlType(sequenceInfo.OriginalType));
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
      RegisterCommand(SqlDdl.Create(sequenceTable), NonTransactionalStage.None);
    }

    private void DropGeneratorTable(SequenceInfo sequenceInfo)
    {
      var sequenceTable = FindTable(sequenceInfo.Name);
      RegisterCommand(SqlDdl.Drop(sequenceTable), NonTransactionalStage.None);
      schema.Tables.Remove(sequenceTable);
    }

    private void RenameSchemaTable(Table table, string newName)
    {
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
      while (column.Table.Columns.Any(tableColumn=>tableColumn.Name==tempName))
        tempName = string.Format(TemporaryNameFormat, column.Name + ++counter);

      return tempName;
    }

    private Table FindTable(string name)
    {
      return schema.Tables.FirstOrDefault(t => t.Name==name);
    }

    private TableColumn FindColumn(Table table, string name)
    {
      return table.TableColumns.
        FirstOrDefault(c => c.Name==name);
    }

    private TableColumn FindColumn(string tableName, string columnName)
    {
      return FindTable(tableName).TableColumns.
        FirstOrDefault(c => c.Name==columnName);
    }

    private SqlValueType GetSqlType(TypeInfo typeInfo)
    {
      var type = typeInfo.Type.IsValueType
        && typeInfo.Type.IsNullable()
        ? typeInfo.Type.GetGenericArguments()[0]
        : typeInfo.Type;

      return driver.BuildValueType(type, typeInfo.Length, typeInfo.Precision, typeInfo.Scale);
    }

    private static SqlRefAction ConvertReferentialAction(ReferentialAction toConvert)
    {
      switch (toConvert) {
      case ReferentialAction.None:
        return SqlRefAction.NoAction;
      case ReferentialAction.Restrict:
        return SqlRefAction.Restrict;
      case ReferentialAction.Cascade:
        return SqlRefAction.Cascade;
      case ReferentialAction.Clear:
        return SqlRefAction.SetNull;
      default:
        return SqlRefAction.Restrict;
      }
    }

    /// <exception cref="InvalidOperationException">Can not create expression 
    /// with specific hint parameters.</exception>
    private SqlExpression CreateConditionalExpression(DataHint hint, SqlTableRef table)
    {
      if (hint.Identities.Any(pair => !pair.IsIdentifiedByConstant)) {
        var identityColumnPairs = hint.Identities
          .Where(pair => !pair.IsIdentifiedByConstant).Select(pair =>
            new Pair<ColumnInfo, ColumnInfo>(
              sourceModel.Resolve(pair.Target) as ColumnInfo,
              sourceModel.Resolve(pair.Source) as ColumnInfo)).ToArray();
        var identityConstantPairs = hint.Identities
          .Where(pair => pair.IsIdentifiedByConstant).Select(pair =>
            new Pair<ColumnInfo, string>(
              sourceModel.Resolve(pair.Source) as ColumnInfo,
              pair.Target)).ToArray();
        var selectColumns = identityColumnPairs.Select(columnPair => columnPair.First)
          .Concat(identityConstantPairs.Select(constantPair => constantPair.First)).ToArray();
        if (selectColumns.Count() == 0)
          throw new InvalidOperationException(Strings.ExIncorrectCommandParameters);

        var identifiedTable = FindTable(selectColumns[0].Parent.Name);
        var identifiedTableRef = SqlDml.TableRef(identifiedTable, 
          string.Format(SubqueryTableAliasNameFormat, identifiedTable.Name));
        var select = SqlDml.Select(identifiedTableRef);
        selectColumns.Apply(column => select.Columns.Add(identifiedTableRef[column.Name]));
        identityColumnPairs.Apply(pair =>
          select.Where &= identifiedTableRef[pair.First.Name]==table[pair.Second.Name]);
        identityConstantPairs.Apply(pair =>
          select.Where &= identifiedTableRef[pair.First.Name]==SqlDml.Literal(pair.Second));
        return SqlDml.Exists(select);
      }
      else {
        var identityConstantPairs = hint.Identities
          .Where(pair => pair.IsIdentifiedByConstant).Select(pair =>
            new Pair<ColumnInfo, string>(
              sourceModel.Resolve(pair.Source) as ColumnInfo,
              pair.Target)).ToArray();
        if (identityConstantPairs.Count() == 0)
          throw new InvalidOperationException(Strings.ExIncorrectCommandParameters);
        SqlExpression expression = null;
        identityConstantPairs.Apply(pair => 
          expression &= table[pair.First.Name]==SqlDml.Literal(pair.Second));
        return expression;
      }
    }

    private void RegisterCommand(ISqlCompileUnit command, NonTransactionalStage nonTransactionalStage)
    {
      RegisterCommand(command, stage, nonTransactionalStage);
    }

    private void RegisterCommand(ISqlCompileUnit command, UpgradeStage stage, NonTransactionalStage nonTransactionalStage)
    {
      var commandText = driver.Compile(command).GetCommandText();
      switch(nonTransactionalStage) {
        case NonTransactionalStage.Prolog:
          nonTransactionalPrologCommands.Add(commandText);
          return;
        case NonTransactionalStage.Epilog:
          nonTransactionalEpilogCommands.Add(commandText);
          return;
      }
      switch (stage) {
        case UpgradeStage.Prepare:
        case UpgradeStage.TemporaryRename:
          preUpgradeCommands.Add(commandText);
          break;
        case UpgradeStage.Upgrade:
          upgradeCommands.Add(commandText);
          break;
        case UpgradeStage.DataManipulate:
          dataManipulateCommands.Add(commandText);
          break;
        case UpgradeStage.Cleanup:
          postUpgradeCommands.Add(commandText);
          break;
      }
    }

    private SqlExpression GetDefaultValueExpression(ColumnInfo columnInfo)
    {
      var result = columnInfo.DefaultValue==null
        ? (SqlExpression) SqlDml.Null
        : SqlDml.Literal(columnInfo.DefaultValue);
      var type = columnInfo.Type.Type;
      if (type.IsNullable())
        type = type.GetGenericArguments()[0];
      var mapping = driver.GetTypeMapping(type);
      if (mapping.ParameterCastRequired)
        result = SqlDml.Cast(result, mapping.BuildSqlType(columnInfo.Type.Length, null, null));
      return result;
    }

    private long? GetCurrentSequenceValue(string sequenceInfoName)
    {
      var script = getNextImplementationHandler.Invoke(providerInfo, schema, sequenceInfoName);
      
      if (providerInfo.Supports(ProviderFeatures.Sequences))
        return Convert.ToInt64(scalarExecutor.Invoke(script));

      var batch = script as SqlBatch;
      if (batch == null || providerInfo.Supports(ProviderFeatures.DdlBatches))
        return Convert.ToInt64(scalarExecutor.Invoke(script));

      ArgumentValidator.EnsureArgumentIsInRange(batch.Count, 2, 2, "batch.Count");
      nonQueryExecutor.Invoke((ISqlCompileUnit) batch[0]);
      return Convert.ToInt64(scalarExecutor.Invoke((ISqlCompileUnit) batch[1]));
    }

    #endregion
    

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="actions">The actions to translate.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="sourceModel">The source model.</param>
    /// <param name="targetModel">The target model.</param>
    /// <param name="providerInfo">The provider info.</param>
    /// <param name="driver">The driver.</param>
    /// <param name="typeIdColumnName">Name of the type id column.</param>
    /// <param name="enforceChangedColumns">Columns thats types must be changed 
    /// enforced (without type conversion verification).</param>
    public SqlActionTranslator(
      ActionSequence actions, 
      Schema schema, 
      StorageInfo sourceModel, 
      StorageInfo targetModel, 
      ProviderInfo providerInfo, 
      Driver driver, 
      string typeIdColumnName, 
      List<string> enforceChangedColumns, 
      Func<ISqlCompileUnit, object> scalarExecutor, 
      Func<ISqlCompileUnit, int> nonQueryExecutor,
      Func<ProviderInfo, Schema, string, ISqlCompileUnit> getNextImplementationHandler)
    {
      
      ArgumentValidator.EnsureArgumentNotNull(actions, "actions");
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(providerInfo, "providerInfo");
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(typeIdColumnName, "typeIdColumnName");
      ArgumentValidator.EnsureArgumentNotNull(scalarExecutor, "scalarExecutor");
      ArgumentValidator.EnsureArgumentNotNull(nonQueryExecutor, "nonQueryExecutor");
      ArgumentValidator.EnsureArgumentNotNull(getNextImplementationHandler, "getNextImplementationHandler");
      
      this.typeIdColumnName = typeIdColumnName;
      this.providerInfo = providerInfo;
      this.schema = schema;
      this.driver = driver;
      this.actions = actions;
      this.sourceModel = sourceModel;
      this.targetModel = targetModel;
      this.enforceChangedColumns = enforceChangedColumns;
      this.scalarExecutor = scalarExecutor;
      this.nonQueryExecutor = nonQueryExecutor;
      this.getNextImplementationHandler = getNextImplementationHandler;
    }
  }
}