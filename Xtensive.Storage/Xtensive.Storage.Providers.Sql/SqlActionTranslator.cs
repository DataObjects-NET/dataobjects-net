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
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Modelling.Actions;
using Xtensive.Core;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom;
using System.Collections.Generic;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using ColumnInfo = Xtensive.Storage.Indexing.Model.ColumnInfo;
using TableInfo = Xtensive.Storage.Indexing.Model.TableInfo;
using SqlRefAction = Xtensive.Sql.Dom.ReferentialAction;
using ReferentialAction = Xtensive.Storage.Indexing.Model.ReferentialAction;
using SequenceInfo = Xtensive.Storage.Indexing.Model.SequenceInfo;
using SqlDomain = Xtensive.Sql.Dom.Database.Domain;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using System.Text;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Translates upgrade <see cref="NodeAction"/>s to Sql.
  /// </summary>
  [Serializable]
  public sealed class SqlActionTranslator
  {
    
    private readonly bool buildDomainForTimeSpan;
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
    
    private readonly List<Table> createdTables = new List<Table>();
    private readonly List<PropertyChangeAction> changeColumnTypeActions = new List<PropertyChangeAction>();
    private UpgradeStage stage;
    private bool translated;

    private bool IsSequencesAllowed
    {
      get { return driver.ServerInfo.Sequence.Features!=SequenceFeatures.None; }
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
      var prepare = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==UpgradeStage.Prepare.ToString());
      if (prepare!=null)
        VisitAction(prepare);
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
        throw new InvalidOperationException("Incorrect command parameters.");

      var fromTable = SqlFactory.TableRef(FindTable(copiedColumns[0].First.Parent.Name));
      var toTable = SqlFactory.TableRef(FindTable(copiedColumns[0].Second.Parent.Name));

      var select = SqlFactory.Select(fromTable);
      identityColumns.Apply(pair => select.Columns.Add(fromTable[pair.First.Name]));
      copiedColumns.Apply(pair => select.Columns.Add(fromTable[pair.First.Name]));
      var selectRef = SqlFactory.QueryRef(select, "th");
      
      var update = SqlFactory.Update(toTable);
      update.From = selectRef;
      copiedColumns.Apply(pair => update.Values[toTable[pair.Second.Name]] = selectRef[pair.First.Name]);
      identityColumns.Apply(pair => update.Where &= toTable[pair.Second.Name]==selectRef[pair.First.Name]);

      RegisterCommand(update);
    }

    private void VisitDeleteDataAction(DataAction action)
    {
      var hint = action.DataHint as DeleteDataHint;
      var soureTableInfo = sourceModel.Resolve(hint.SourceTablePath) as TableInfo;
      var table = SqlFactory.TableRef(FindTable(soureTableInfo.Name));
      var delete = SqlFactory.Delete(table);
      
      delete.Where = CreateConditionalExpression(hint, table);

      RegisterCommand(delete);
    }

    private void VisitUpdateDataAction(DataAction action)
    {
      var hint = action.DataHint as  UpdateDataHint;
      var soureTableInfo = sourceModel.Resolve(hint.SourceTablePath) as TableInfo;
      var table = SqlFactory.TableRef(FindTable(soureTableInfo.Name));
      var update = SqlFactory.Update(table);

      var updatedColumns = hint.UpdateParameter
        .Select(pair => new Pair<ColumnInfo, object>(
          sourceModel.Resolve(pair.First) as ColumnInfo,
          pair.Second)).ToArray();
      if (updatedColumns.Length==0)
          throw new InvalidOperationException("Incorrect command parameters.");
      foreach (var pair in updatedColumns)
        if (pair.Second==null)
          update.Values[table[pair.First.Name]] = SqlFactory.DefaultValue;
        else
          update.Values[table[pair.First.Name]] = SqlFactory.Literal(pair.Second);

      update.Where = CreateConditionalExpression(hint, table);
      
      RegisterCommand(update);
    }

    private void VisitCreateTableAction(CreateNodeAction action)
    {
      var tableInfo = action.Difference.Target as TableInfo;
      var table = CreateTable(tableInfo);
      RegisterCommand(SqlFactory.Create(table));
    }

    private void VisitRemoveTableAction(RemoveNodeAction action)
    {
      var tableInfo = action.Difference.Source as TableInfo;
      var table = FindTable(tableInfo.Name);
      RegisterCommand(SqlFactory.Drop(table));
      schema.Tables.Remove(table);
    }

    private void VisitAlterTableAction(MoveNodeAction action)
    {
      var oldTableInfo = sourceModel.Resolve(action.Path) as TableInfo;
      var table = FindTable(oldTableInfo.Name);
      RegisterCommand(SqlFactory.Rename(table, action.Name));
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
      column.IsNullable = column.IsNullable;
      if (!column.IsNullable)
        column.DefaultValue = GetDefaultValue(columnInfo);
      RegisterCommand(
        SqlFactory.Alter(table,
          SqlFactory.AddColumn(column)));
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
          .OfType<DefaultConstraint>().FirstOrDefault(defaultConstraint => defaultConstraint.Column==column);
        if (constraint!=null)
          RegisterCommand(SqlFactory.Alter(table,
            SqlFactory.DropConstraint(constraint)));
      }
      RegisterCommand(SqlFactory.Alter(table,
        SqlFactory.DropColumn(column)));
      table.TableColumns.Remove(column);
    }

    private void VisitAlterColumnAction(PropertyChangeAction action)
    {
      var isNewlyCreatedColumn = action.Difference.Source==null;
      if (isNewlyCreatedColumn)
        return; // Properties already initilized

      if (!action.Properties.ContainsKey("Type"))
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
        RegisterCommand(SqlFactory.Rename(column, action.Name));
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
      RegisterCommand(SqlFactory.Alter(table,
          SqlFactory.AddConstraint(primaryKey)));
    }

    private void VisitRemovePrimaryKeyAction(RemoveNodeAction action)
    {
      var primaryIndexInfo = action.Difference.Source as PrimaryIndexInfo;
      var table = FindTable(primaryIndexInfo.Parent.Name);

      // Ensure table is not removed
      if (table==null)
        return;
      
      var primaryKey = table.TableConstraints[primaryIndexInfo.Name];

      RegisterCommand(SqlFactory.Alter(table,
          SqlFactory.DropConstraint(primaryKey)));
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
      RegisterCommand(SqlFactory.Create(index));
    }

    private void VisitRemoveSecondaryIndexAction(RemoveNodeAction action)
    {
      var secondaryIndexInfo = action.Difference.Source as SecondaryIndexInfo;
      var table = FindTable(secondaryIndexInfo.Parent.Name);
      
      // Ensure table is not removed
      if (table==null)
        return;
      
      var index = table.Indexes[secondaryIndexInfo.Name];
      RegisterCommand(SqlFactory.Drop(index));
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

      RegisterCommand(SqlFactory.Alter(table,
          SqlFactory.AddConstraint(foreignKey)));
    }

    private void VisitRemoveForeignKeyAction(RemoveNodeAction action)
    {
      var foreignKeyInfo = action.Difference.Source as ForeignKeyInfo;
      var table = FindTable(foreignKeyInfo.Parent.Name);

      // Ensure table is not removed
      if (table==null)
        return;

      var foreignKey = table.TableConstraints[foreignKeyInfo.Name];
      RegisterCommand(SqlFactory.Alter(table,
          SqlFactory.DropConstraint(foreignKey)));
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
        sequence.DataType = GetSqlType(sequenceInfo.Type);
        RegisterCommand(SqlFactory.Create(sequence));
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
        RegisterCommand(SqlFactory.Drop(sequence));
        schema.Sequences.Remove(sequence);
      }
      else {
        DropGeneratorTable(sequenceInfo);
      }
    }

    private void VisitAlterSequenceAction(PropertyChangeAction action)
    {
      var sequenceInfo = targetModel.Resolve(action.Path) as SequenceInfo;
      if (IsSequencesAllowed) {
        var sequence = schema.Sequences[sequenceInfo.Name];
        var sequenceDescriptor = new SequenceDescriptor(sequence,
          sequenceInfo.StartValue, sequenceInfo.Increment);
        sequenceDescriptor.MinValue = sequenceInfo.StartValue;
        sequence.SequenceDescriptor = sequenceDescriptor;
        RegisterCommand(SqlFactory.Alter(sequence,
          sequenceDescriptor));
      }
      else if (!createdTables.Any(table => table.Name==sequenceInfo.Name)) {
        DropGeneratorTable(sequenceInfo);
        CreateGeneratorTable(sequenceInfo);
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
      var renameColumn = SqlFactory.Rename(column, tempName);
      RegisterCommand(renameColumn, UpgradeStage.Upgrade);
      RenameSchemaColumn(column, tempName);

      // Create new columns
      var newTypeInfo = action.Properties["Type"] as TypeInfo;
      var type = GetSqlType(newTypeInfo);
      var newColumn = table.CreateColumn(originalName, type);
      newColumn.IsNullable = newTypeInfo.IsNullable;
      if (!newColumn.IsNullable)
        newColumn.DefaultValue = GetDefaultValue(targetColumn);
      var addColumnWithNewType = SqlFactory.Alter(column.Table, SqlFactory.AddColumn(newColumn));
      RegisterCommand(addColumnWithNewType, UpgradeStage.Upgrade);

      // Copy values if possible to convert type
      if (Upgrade.TypeConversionVerifier.CanConvert(sourceColumn.Type, newTypeInfo)) {
        var tableRef = SqlFactory.TableRef(column.Table);
        var copyValues = SqlFactory.Update(tableRef);
        copyValues.Values[tableRef[originalName]] = SqlFactory.Cast(tableRef[tempName], type);
        RegisterCommand(copyValues, UpgradeStage.DataManipulate);
      }

      // Drop old column
      if (column.DefaultValue!=null) {
        var constraint = table.TableConstraints
          .OfType<DefaultConstraint>().FirstOrDefault(defaultConstraint => defaultConstraint.Column==column);
        if (constraint!=null)
          RegisterCommand(SqlFactory.Alter(table,
            SqlFactory.DropConstraint(constraint)),
            UpgradeStage.Cleanup);
      }
      var removeOldColumn = SqlFactory.Alter(column.Table, SqlFactory.DropColumn(column));
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
      if(tableInfo.PrimaryIndex!=null)
        CreatePrimaryKey(tableInfo, table);
      createdTables.Add(table);
      return table;
    }

    private TableColumn CreateColumn(ColumnInfo columnInfo, Table table)
    {
      var type = GetSqlType(columnInfo.Type);
      var column = table.CreateColumn(columnInfo.Name, type);
      if (buildDomainForTimeSpan && (columnInfo.Type.Type==typeof (TimeSpan)
        || columnInfo.Type.Type==typeof (TimeSpan?)))
        column.Domain = GetTimeSpanDomain();
      column.IsNullable = columnInfo.Type.IsNullable;
      return column;
    }

    private ForeignKey CreateForeignKey(ForeignKeyInfo foreignKeyInfo)
    {
      var referencingTable = FindTable(foreignKeyInfo.Parent.Name);
      var foreignKey = referencingTable.CreateForeignKey(foreignKeyInfo.Name);
      foreignKey.OnUpdate = ConvertReferentialAction(foreignKeyInfo.OnUpdateAction);
      foreignKey.OnDelete = ConvertReferentialAction(foreignKeyInfo.OnRemoveAction);
      var referncingColumns = foreignKeyInfo.ForeignKeyColumns
        .Select(cr => FindColumn(referencingTable, cr.Value.Name));
      foreignKey.Columns.AddRange(referncingColumns);
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
            .Select(cr => FindColumn(table, cr.Value.Name)).ToArray());
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
      var idColumn = sequenceTable.CreateColumn(SqlWellknown.GeneratorColumnName,
        GetSqlType(sequenceInfo.Type));
      var currentValue = GetCurrentSequenceValue(sequenceInfo.Name);
      idColumn.SequenceDescriptor =
        new SequenceDescriptor(
          idColumn,
          currentValue ?? sequenceInfo.StartValue,
          sequenceInfo.Increment);
      RegisterCommand(SqlFactory.Create(sequenceTable));
    }

    private SqlDomain GetTimeSpanDomain()
    {
      var domain = schema.Domains[SqlWellknown.TimeSpanDomainName];
      if (domain == null) {
        var sqlValueType = GetSqlType(new TypeInfo(typeof (TimeSpan)));
        domain = schema.CreateDomain(SqlWellknown.TimeSpanDomainName, sqlValueType);
        RegisterCommand(SqlFactory.Create(domain), UpgradeStage.Prepare);
      }
      return domain;
    }

    private void DropGeneratorTable(SequenceInfo sequenceInfo)
    {
      var sequenceTable = FindTable(sequenceInfo.Name);
      RegisterCommand(SqlFactory.Drop(sequenceTable));
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
      var tempName = string.Format("Temp_{0}", column.Name);
      var counter = 0;
      while (column.Table.Columns.Any(tableColumn=>tableColumn.Name==tempName))
        tempName = string.Format("Temp_{0}", column.Name + ++counter);

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

      return valueTypeMapper.BuildSqlValueType(type, typeInfo.Length ?? 0);
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
          throw new InvalidOperationException("Incorrect command parameters.");

        var identifiedTable = SqlFactory.TableRef(FindTable(selectColumns[0].Parent.Name));
        var select = SqlFactory.Select(identifiedTable);
        selectColumns.Apply(column => select.Columns.Add(identifiedTable[column.Name]));
        identityColumnPairs.Apply(pair =>
          select.Where &= identifiedTable[pair.First.Name]==table[pair.Second.Name]);
        identityConstantPairs.Apply(pair =>
          select.Where &= identifiedTable[pair.First.Name]==SqlFactory.Literal(pair.Second));
        return SqlFactory.Exists(select);
      }
      else {
        var identityConstantPairs = hint.Identities
          .Where(pair => pair.IsIdentifiedByConstant).Select(pair =>
            new Pair<ColumnInfo, string>(
              sourceModel.Resolve(pair.Source) as ColumnInfo,
              pair.Target)).ToArray();
        if (identityConstantPairs.Count() == 0)
          throw new InvalidOperationException("Incorrect command parameters.");
        SqlExpression expression = null;
        identityConstantPairs.Apply(pair => expression &= table[pair.First.Name]==SqlFactory.Literal(pair.Second));
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

    private long? GetCurrentSequenceValue(string sequenceInfoName)
    {
      var sequenceInfo = sourceModel.Sequences.FirstOrDefault(si => si.Name==sequenceInfoName);
      return sequenceInfo==null ? null : sequenceInfo.Current;
    }

    private SqlExpression GetDefaultValue(ColumnInfo columnInfo)
    {
      if (columnInfo.DefaultValue==null)
        return null;

      var defaultValueType = columnInfo.DefaultValue.GetType();
      var mapping =
        columnInfo.Type.Length.HasValue
          ? valueTypeMapper.GetTypeMapping(defaultValueType, columnInfo.Type.Length.Value)
          : valueTypeMapper.GetTypeMapping(defaultValueType);
      var value = mapping.ToSqlValue!=null
        ? mapping.ToSqlValue.Invoke(columnInfo.DefaultValue)
        : columnInfo.DefaultValue;
      if (value == null)
        return null;
      
      return SqlFactory.Literal(value, value.GetType());
    }

    # endregion
    

    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="actions">The actions to translate.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="driver">The driver.</param>
    /// <param name="valueTypeMapper">The value type mapper.</param>
    /// <param name="sourceModel">The source model.</param>
    /// <param name="targetModel">The target model.</param>
    /// <param name="buildDomainForTimeSpan">if set to <see langword="true"/> build domain for time span column types.</param>
    public SqlActionTranslator(ActionSequence actions, Schema schema, SqlDriver driver,
      SqlValueTypeMapper valueTypeMapper, StorageInfo sourceModel, 
      StorageInfo targetModel, bool buildDomainForTimeSpan)
    {
      ArgumentValidator.EnsureArgumentNotNull(actions, "actions");
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(valueTypeMapper, "valueTypeMapper");
      
      this.buildDomainForTimeSpan = buildDomainForTimeSpan;
      this.schema = schema;
      this.driver = driver;
      this.actions = actions;
      this.valueTypeMapper = valueTypeMapper;
      this.sourceModel = sourceModel;
      this.targetModel = targetModel;
    }
  }
}