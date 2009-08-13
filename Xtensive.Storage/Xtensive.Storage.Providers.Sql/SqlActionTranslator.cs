// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.24

using System;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Actions;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Storage.Indexing.Model;
using System.Collections.Generic;
using ColumnInfo = Xtensive.Storage.Indexing.Model.ColumnInfo;
using TableInfo = Xtensive.Storage.Indexing.Model.TableInfo;
using SqlRefAction = Xtensive.Sql.ReferentialAction;
using ReferentialAction = Xtensive.Storage.Indexing.Model.ReferentialAction;
using SequenceInfo = Xtensive.Storage.Indexing.Model.SequenceInfo;
using SqlDomain = Xtensive.Sql.Model.Domain;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Core.Sorting;
using Xtensive.Sql.Ddl;

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
    private readonly Func<ISqlCompileUnit, object> commandExecutor;

    private readonly ActionSequence actions;
    private readonly Schema schema;
    private readonly SqlValueTypeMapper valueTypeMapper;
    private readonly StorageInfo sourceModel;
    private readonly StorageInfo targetModel;
    private readonly SqlDriver driver;
    
    private readonly List<string> preUpgradeCommands = new List<string>();
    private readonly List<string> upgradeCommands = new List<string>();
    private readonly List<string> dataManipulateCommands = new List<string>();
    private readonly List<string> postUpgradeCommands = new List<string>();
    
    private bool translated;
    private readonly List<Table> createdTables = new List<Table>();
    private readonly List<Sequence> createdSequences = new List<Sequence>();
    private readonly List<PropertyChangeAction> changeColumnTypeActions = new List<PropertyChangeAction>();
    private readonly List<DataAction> clearDataActions = new List<DataAction>();
    private UpgradeStage stage;
    
    private bool IsSequencesAllowed
    {
      get { return providerInfo.SupportSequences; }
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
          Translate();
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
          Translate();
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
          Translate();
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
          Translate();
        return postUpgradeCommands;
      }
    }

    private void Translate()
    {
      // Prepairing
      stage = UpgradeStage.Prepare;
      // Turn off deferred contraints
      if (providerInfo.SupportsDeferredForeignKeyConstraints)
        RegisterCommand(SqlDdl.Command(SqlCommandType.SetConstraintsAllImmediate));
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
      if (providerInfo.SupportsDeferredForeignKeyConstraints)
        RegisterCommand(SqlDdl.Command(SqlCommandType.SetConstraintsAllDeferred));
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
      if (action.Type==typeof (TableInfo))
        VisitCreateTableAction(action);
      else if (action.Type==typeof (ColumnInfo))
        VisitCreateColumnAction(action);
      else if (action.Type==typeof (PrimaryIndexInfo))
        VisitCreatePrimaryKeyAction(action);
      else if (action.Type==typeof (SecondaryIndexInfo))
        VisitCreateSecondaryIndexAction(action);
      else if (action.Type==typeof (ForeignKeyInfo))
        VisitCreateForeignKeyAction(action);
      else if (action.Type==typeof (SequenceInfo))
        VisitCreateSequenceAction(action);
    }

    private void VisitRemoveAction(RemoveNodeAction action)
    {
      var node = action.Difference.Source;
      if (node.GetType()==typeof (TableInfo))
        VisitRemoveTableAction(action);
      else if (node.GetType()==typeof (ColumnInfo))
        VisitRemoveColumnAction(action);
      else if (node.GetType()==typeof (PrimaryIndexInfo))
        VisitRemovePrimaryKeyAction(action);
      else if (node.GetType()==typeof (SecondaryIndexInfo))
        VisitRemoveSecondaryIndexAction(action);
      else if (node.GetType()==typeof (ForeignKeyInfo))
        VisitRemoveForeignKeyAction(action);
      else if (node.GetType()==typeof (SequenceInfo))
        VisitRemoveSequenceAction(action);
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
    
    # region Visit concrete action methods

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

      var fromTable = SqlDml.TableRef(FindTable(copiedColumns[0].First.Parent.Name));
      var toTable = SqlDml.TableRef(FindTable(copiedColumns[0].Second.Parent.Name));

      var select = SqlDml.Select(fromTable);
      identityColumns.Apply(pair => select.Columns.Add(fromTable[pair.First.Name]));
      copiedColumns.Apply(pair => select.Columns.Add(fromTable[pair.First.Name]));
      var selectRef = SqlDml.QueryRef(select, SubqueryAliasName);
      
      var update = SqlDml.Update(toTable);
      update.From = selectRef;
      copiedColumns.Apply(pair => update.Values[toTable[pair.Second.Name]] = selectRef[pair.First.Name]);
      identityColumns.Apply(pair => update.Where &= toTable[pair.Second.Name]==selectRef[pair.First.Name]);

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
      var tableInfo = action.Difference.Source as TableInfo;
      var table = FindTable(tableInfo.Name);
      RegisterCommand(SqlDdl.Drop(table));
      schema.Tables.Remove(table);
    }

    private void VisitAlterTableAction(MoveNodeAction action)
    {
      var oldTableInfo = sourceModel.Resolve(action.Path) as TableInfo;
      var table = FindTable(oldTableInfo.Name);
      RegisterCommand(SqlDdl.Rename(table, action.Name));
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
      RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddColumn(column)));
    }

    private void VisitRemoveColumnAction(RemoveNodeAction removeColumnAction)
    {
      var columnInfo = removeColumnAction.Difference.Source as ColumnInfo;
      var table = FindTable(columnInfo.Parent.Name);

      // Ensure table is not removed
      if (table==null)
        return;

      var column = FindColumn(table, columnInfo.Name);
      if (column.DefaultValue!=null) {
        var constraint = table.TableConstraints
          .OfType<DefaultConstraint>()
          .FirstOrDefault(defaultConstraint => defaultConstraint.Column==column);
        if (constraint!=null)
          RegisterCommand(SqlDdl.Alter(table,
            SqlDdl.DropConstraint(constraint)));
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
        RegisterCommand(SqlDdl.Rename(column, action.Name));
        oldColumnInfo.Name = action.Name;
        RenameSchemaColumn(column, action.Name);
      }
      else
        throw new NotSupportedException();
    }

    private void VisitCreatePrimaryKeyAction(CreateNodeAction action)
    {
      var primaryIndex = action.Difference.Target as PrimaryIndexInfo;
      var table = FindTable(primaryIndex.Parent.Name);

      // Ensure table is not newly created
      if (createdTables.Contains(table))
        return;

      var primaryKey = CreatePrimaryKey(primaryIndex.Parent, table);
      RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddConstraint(primaryKey)));
    }

    private void VisitRemovePrimaryKeyAction(RemoveNodeAction action)
    {
      var primaryIndexInfo = action.Difference.Source as PrimaryIndexInfo;
      var table = FindTable(primaryIndexInfo.Parent.Name);

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
      var secondaryIndexInfo = action.Difference.Target as SecondaryIndexInfo;
      var table = FindTable(secondaryIndexInfo.Parent.Name);
      var index = CreateSecondaryIndex(table, secondaryIndexInfo);
      RegisterCommand(SqlDdl.Create(index));
    }

    private void VisitRemoveSecondaryIndexAction(RemoveNodeAction action)
    {
      var secondaryIndexInfo = action.Difference.Source as SecondaryIndexInfo;
      var table = FindTable(secondaryIndexInfo.Parent.Name);
      
      // Ensure table is not removed
      if (table==null)
        return;
      
      var index = table.Indexes[secondaryIndexInfo.Name];
      RegisterCommand(SqlDdl.Drop(index));
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

      RegisterCommand(SqlDdl.Alter(table, SqlDdl.AddConstraint(foreignKey)));
    }

    private void VisitRemoveForeignKeyAction(RemoveNodeAction action)
    {
      var foreignKeyInfo = action.Difference.Source as ForeignKeyInfo;
      var table = FindTable(foreignKeyInfo.Parent.Name);

      // Ensure table is not removed
      if (table==null)
        return;

      var foreignKey = table.TableConstraints[foreignKeyInfo.Name];
      RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(foreignKey)));
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
          sequenceInfo.StartValue, sequenceInfo.Increment);
        sequence.SequenceDescriptor.MinValue = sequenceInfo.StartValue;
        sequence.DataType = GetSqlType(sequenceInfo.OriginalType);
        RegisterCommand(SqlDdl.Create(sequence));
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
        RegisterCommand(SqlDdl.Drop(sequence));
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
        var newSequenceDescriptor = new SequenceDescriptor(exisitingSequence,
          newStartValue, sequenceInfo.Increment);
        //newSequenceDescriptor.MinValue = newStartValue;
        exisitingSequence.SequenceDescriptor = newSequenceDescriptor;
        RegisterCommand(SqlDdl.Alter(exisitingSequence, newSequenceDescriptor));
      }
      else {
        sequenceInfo.Current = newStartValue;
        DropGeneratorTable(sequenceInfo);
        CreateGeneratorTable(sequenceInfo);
      }
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

      RegisterCommand(delete, UpgradeStage.Prepare);
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
        throw new InvalidOperationException(Resources.Strings.ExIncorrectCommandParameters);
      foreach (var pair in updatedColumns)
        if (pair.Second==null)
          update.Values[table[pair.First.Name]] = SqlDml.DefaultValue;
        else
          update.Values[table[pair.First.Name]] = SqlDml.Literal(pair.Second);

      update.Where = CreateConditionalExpression(hint, table);
      
      RegisterCommand(update, UpgradeStage.Prepare);
    }

    private void ProcessClearAncestorsActions(List<DataAction> originalActions)
    {
      if (originalActions.Count==0)
        return;

      // Merge actions
      var deleteActions = new Dictionary<TableInfo, List<string>>();
      foreach (var action in originalActions) {
        var soureTableInfo = sourceModel.Resolve(action.DataHint.SourceTablePath) as TableInfo;
        List<string> list;
        if (!deleteActions.TryGetValue(soureTableInfo, out list)) {
          list = new List<string>();
          deleteActions.Add(soureTableInfo, list);
        }
        list.AddRange(action.DataHint.Identities.Select(pair => pair.Target));
      }

      // Sort actions topologicaly according to foreign keys
      var nodes = new List<Node<TableInfo, ForeignKeyInfo>>();
      var foreignKeys = sourceModel.Tables.SelectMany(table => table.ForeignKeys).ToList();
      foreach (var pair in deleteActions)
        nodes.Add(new Node<TableInfo, ForeignKeyInfo>(pair.Key));
      foreach (var foreignKey in foreignKeys) {
        var referencedNode = nodes.FirstOrDefault(node => node.Item==foreignKey.PrimaryKey.Parent);
        var referencingNode = nodes.FirstOrDefault(node => node.Item==foreignKey.Parent);
        if (referencedNode!=null && referencingNode!=null)
          referencingNode.AddConnection(
            new NodeConnection<TableInfo, ForeignKeyInfo>(
              referencedNode, referencingNode, foreignKey));
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
        RegisterCommand(delete, UpgradeStage.Prepare);
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
      var renameColumn = SqlDdl.Rename(column, tempName);
      RegisterCommand(renameColumn, UpgradeStage.Upgrade);
      RenameSchemaColumn(column, tempName);

      // Create new columns
      var newTypeInfo = action.Properties[ColumnTypePropertyName] as TypeInfo;
      var type = GetSqlType(newTypeInfo);
      var newColumn = table.CreateColumn(originalName, type);
      newColumn.IsNullable = newTypeInfo.IsNullable;
      if (!newColumn.IsNullable)
        newColumn.DefaultValue = GetDefaultValueExpression(targetColumn);
      var addColumnWithNewType = SqlDdl.Alter(column.Table, SqlDdl.AddColumn(newColumn));
      RegisterCommand(addColumnWithNewType, UpgradeStage.Upgrade);

      // Copy values if possible to convert type
      if (Upgrade.TypeConversionVerifier.CanConvert(sourceColumn.Type, newTypeInfo)
        || enforceChangedColumns.Contains(sourceColumn.Path)) {
        var tableRef = SqlDml.TableRef(column.Table);
        var copyValues = SqlDml.Update(tableRef);
        if (newTypeInfo.IsNullable)
          copyValues.Values[tableRef[originalName]] = SqlDml.Cast(tableRef[tempName], type);
        else {
          var getValue = SqlDml.Case();
          getValue.Add(SqlDml.IsNull(tableRef[tempName]), GetDefaultValueExpression(targetColumn));
          getValue.Add(SqlDml.IsNotNull(tableRef[tempName]), SqlDml.Cast(tableRef[tempName], type));
          copyValues.Values[tableRef[originalName]] = getValue;
        }
        RegisterCommand(copyValues, UpgradeStage.DataManipulate);
      }

      // Drop old column
      if (column.DefaultValue!=null) {
        var constraint = table.TableConstraints
          .OfType<DefaultConstraint>().FirstOrDefault(defaultConstraint => defaultConstraint.Column==column);
        if (constraint!=null)
          RegisterCommand(SqlDdl.Alter(table, SqlDdl.DropConstraint(constraint)), UpgradeStage.Cleanup);
      }
      var removeOldColumn = SqlDdl.Alter(column.Table, SqlDdl.DropColumn(column));
      RegisterCommand(removeOldColumn, UpgradeStage.Cleanup);
      column.Table.TableColumns.Remove(column);
    }
    
    # endregion

    # region Helper methods

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
      foreignKey.IsDeferrable = providerInfo.SupportsDeferredForeignKeyConstraints;
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
          sequenceInfo.Current ?? sequenceInfo.StartValue,
          sequenceInfo.Increment);
      RegisterCommand(SqlDdl.Create(sequenceTable));
    }

    private void DropGeneratorTable(SequenceInfo sequenceInfo)
    {
      var sequenceTable = FindTable(sequenceInfo.Name);
      RegisterCommand(SqlDdl.Drop(sequenceTable));
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

      return valueTypeMapper.BuildSqlValueType(type, typeInfo.Length, typeInfo.Precision, typeInfo.Scale);
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

    private void RegisterCommand(ISqlCompileUnit command)
    {
      RegisterCommand(command, stage);
    }

    private void RegisterCommand(ISqlCompileUnit command, UpgradeStage stage)
    {
      var commandText = driver.Compile(command).GetCommandText();
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
        ? SqlDml.Null
        : SqlDml.Literal(columnInfo.DefaultValue, columnInfo.DefaultValue.GetType());
      var type = columnInfo.Type.Type;
      if (type.IsNullable())
        type = type.GetGenericArguments()[0];
      var mapping = driver.TypeMappings[type];
      if (mapping.ParameterCastRequired)
        result = SqlDml.Cast(result, mapping.BuildSqlType(columnInfo.Type.Length, null, null));
      return result;
    }

    private long? GetCurrentSequenceValue(string sequenceInfoName)
    {
      var selectNextValue = KeyGeneratorFactory.GetNextValueStatement(driver, schema, sequenceInfoName);
      return Convert.ToInt64(commandExecutor.Invoke(selectNextValue));
    }

    # endregion
    

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
    /// <param name="valueTypeMapper">The value type mapper.</param>
    /// <param name="typeIdColumnName">Name of the type id column.</param>
    /// <param name="enforceChangedColumns">Columns thats types must be changed 
    /// enforced (without type conversion verification).</param>
    public SqlActionTranslator(ActionSequence actions, Schema schema, 
      StorageInfo sourceModel, StorageInfo targetModel, 
      ProviderInfo providerInfo, SqlDriver driver, 
      SqlValueTypeMapper valueTypeMapper, string typeIdColumnName, 
      List<string> enforceChangedColumns, Func<ISqlCompileUnit, object> commandExecutor)
    {
      
      ArgumentValidator.EnsureArgumentNotNull(actions, "actions");
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(providerInfo, "providerInfo");
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(typeIdColumnName, "typeIdColumnName");
      
      this.typeIdColumnName = typeIdColumnName;
      this.providerInfo = providerInfo;
      this.schema = schema;
      this.driver = driver;
      this.actions = actions;
      this.valueTypeMapper = valueTypeMapper;
      this.sourceModel = sourceModel;
      this.targetModel = targetModel;
      this.enforceChangedColumns = enforceChangedColumns;
      this.commandExecutor = commandExecutor;
    }
  }
}