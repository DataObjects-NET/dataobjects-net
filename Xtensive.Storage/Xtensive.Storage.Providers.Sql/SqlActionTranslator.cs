// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.24

using System;
using System.Linq;
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

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Translates actions to Sql.
  /// </summary>
  [Serializable]
  public sealed class SqlActionTranslator
  {
    private readonly ActionSequence actions;
    private readonly Schema schema;

    private readonly Func<Type, int, SqlValueType> valueTypeBuilder;
    private readonly StorageInfo sourceModel;
    private readonly StorageInfo targetModel;
    private readonly SqlDriver driver;
    private readonly List<string> preUpgradeCommands = new List<string>();
    private readonly List<string> upgradeCommands = new List<string>();
    private readonly List<string> dataManipulateCommands = new List<string>();
    private readonly List<string> postUpgradeCommands = new List<string>();
    
    private readonly List<Table> createdTables = new List<Table>();
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
      // Mutual rename
      stage = UpgradeStage.CyclicRename;
      var cycleRename = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment==UpgradeStage.CyclicRename.ToString());
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
        foreach (var nodeAction in ((GroupingNodeAction)action).Actions)
          VisitAction(nodeAction);
      else if (action is CreateNodeAction)
        VisitCreateAction(action as CreateNodeAction);
      else if (action is RemoveNodeAction)
        VisitRemoveAction(action as RemoveNodeAction);
      else if (action is CopyDataAction)
        VisitCopyDataAction(action as CopyDataAction);
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

    private void VisitCopyDataAction(CopyDataAction action)
    {
      var fromColumnInfo = sourceModel.Resolve(action.Path) as ColumnInfo;
      var toColumnInfo = targetModel.Resolve(action.NewPath) as ColumnInfo;
      var identityPairs = action.Parameters
        .Select(copyParameter=>new Pair<ColumnInfo>(
          sourceModel.Resolve(copyParameter.SourcePath) as ColumnInfo, 
           sourceModel.Resolve(copyParameter.TargetPath) as ColumnInfo)).ToArray();
      
      var fromTable = SqlFactory.TableRef(FindTable(fromColumnInfo.Parent.Name));
      var toTable = SqlFactory.TableRef(FindTable(toColumnInfo.Parent.Name));

      var select = SqlFactory.Select(fromTable);
      select.Columns.Add(fromTable[fromColumnInfo.Name]);
      foreach (var identityColumns in identityPairs)
        select.Columns.Add(fromTable[identityColumns.First.Name]);
      var selectRef = SqlFactory.QueryRef(select, "th");
      
      var update = SqlFactory.Update(toTable);
      update.Values[toTable[toColumnInfo.Name]] = selectRef[fromColumnInfo.Name];
      update.From = selectRef;
      foreach (var identityColumns in identityPairs)
        update.Where = toTable[identityColumns.Second.Name]==selectRef[identityColumns.First.Name];  
      
      RegisterCommand(update);
    }

    # region Visit concrete action methods

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
      table.Name = action.Name;
    }
    
    private void VisitAlterTableAction(NodeAction action)
    {
      throw new NotImplementedException();
    }

    private void VisitCreateColumnAction(CreateNodeAction createColumnAction)
    {
      var columnInfo = createColumnAction.Difference.Target as ColumnInfo;
      var table = FindTable(columnInfo.Parent.Name);
      
      if (createdTables.Contains(table))
        return;
      
      var column = CreateColumn(columnInfo, table);
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
      RegisterCommand(SqlFactory.Alter(table,
          SqlFactory.DropColumn(column)));
      table.TableColumns.Remove(column);
    }

    private void VisitAlterColumnAction(NodeAction action)
    {
      throw new NotImplementedException();
    }

    private void VisitMoveColumnAction(MoveNodeAction action)
    {
      var movementInfo = ((NodeDifference) action.Difference).MovementInfo;
      if ((movementInfo & MovementInfo.NameChanged) != 0) {
        // Process name changing
        var oldColumnInfo = sourceModel.Resolve(action.Path) as ColumnInfo;
        var column = FindColumn(oldColumnInfo.Parent.Name, oldColumnInfo.Name);
        RegisterCommand(SqlFactory.Rename(column, action.Name));
        oldColumnInfo.Name = action.Name;
        column.Name = action.Name;
      }
    }

    private void VisitCreatePrimaryKeyAction(CreateNodeAction action)
    {
      var primaryIndex = action.Difference.Target as PrimaryIndexInfo;
      var table = FindTable(primaryIndex.Parent.Name);

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
      throw new NotImplementedException();
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
      throw new NotImplementedException();
    }
    
    private void VisitCreateForeignKeyAction(CreateNodeAction action)
    {
      var foreignKeyInfo = action.Difference.Target as ForeignKeyInfo;
      var table = FindTable(foreignKeyInfo.Parent.Name);
      var foreignKey = CreateForeignKey(foreignKeyInfo);

      // If referencedTable table is newly created, 
      // set referencing fields to default values
      var referencedTable = foreignKey.ReferencedTable;
      if (createdTables.Contains(referencedTable)) {
        var referencingTable = foreignKey.Table;
        var tableRef = SqlFactory.TableRef(referencingTable);
        foreach (var column in foreignKey.Columns) {
          var update = SqlFactory.Update(tableRef);
          update.Values[tableRef[column.Name]] = SqlFactory.DefaultValue;
          RegisterCommand(update);
        }
      }
      
      RegisterCommand(SqlFactory.Alter(table,
          SqlFactory.AddConstraint(foreignKey)));

      // Remove foreign key from table for correct sql statement order
      // table.TableConstraints.Remove(foreignKey);
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
      throw new NotImplementedException();
    }

    private void VisitCreateSequenceAction(CreateNodeAction action)
    {
      var sequenceInfo = action.Difference.Target as SequenceInfo;
      if (IsSequencesAllowed) {
        var sequence = schema.CreateSequence(sequenceInfo.Name);
        sequence.SequenceDescriptor = new SequenceDescriptor(sequence,
          sequenceInfo.StartValue, sequenceInfo.Increment);
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
        sequence.SequenceDescriptor = sequenceDescriptor;
        RegisterCommand(SqlFactory.Alter(sequence,
          sequenceDescriptor));
      }
      else if (!createdTables.Any(table => table.Name==sequenceInfo.Name)) {
        DropGeneratorTable(sequenceInfo);
        CreateGeneratorTable(sequenceInfo);
      }
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
      if (columnInfo.Type.Type==typeof (TimeSpan)
        || columnInfo.Type.Type==typeof (TimeSpan?))
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
        RegisterPreCommand(SqlFactory.Create(domain));
      }
      return domain;
    }

    private void DropGeneratorTable(SequenceInfo sequenceInfo)
    {
      var sequenceTable = FindTable(sequenceInfo.Name);
      RegisterCommand(SqlFactory.Drop(sequenceTable));
      schema.Tables.Remove(sequenceTable);
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

      return
        valueTypeBuilder!=null
          ? valueTypeBuilder.Invoke(type, typeInfo.Length ?? 0)
          : BuildSqlValueType(type, typeInfo.Length);
    }

    private static SqlValueType BuildSqlValueType(Type type, int? length)
    {
      var dataType = GetDbType(type);
      if (length.HasValue)
        return new SqlValueType(dataType, length.Value);
      return new SqlValueType(dataType);
    }

    private static SqlDataType GetDbType(Type type)
    {
      if (type.IsValueType && type.IsNullable())
        type = type.GetGenericArguments()[0];
      
      TypeCode typeCode = Type.GetTypeCode(type);
      switch (typeCode) {
      case TypeCode.Object:
        if (type==typeof (byte[]))
          return SqlDataType.Binary;
        if (type == typeof(Guid))
          return SqlDataType.Guid;
        throw new ArgumentOutOfRangeException();
      case TypeCode.Boolean:
        return SqlDataType.Boolean;
      case TypeCode.Char:
        return SqlDataType.Char;
      case TypeCode.SByte:
        return SqlDataType.SByte;
      case TypeCode.Byte:
        return SqlDataType.Byte;
      case TypeCode.Int16:
        return SqlDataType.Int16;
      case TypeCode.UInt16:
        return SqlDataType.UInt16;
      case TypeCode.Int32:
        return SqlDataType.Int32;
      case TypeCode.UInt32:
        return SqlDataType.UInt32;
      case TypeCode.Int64:
        return SqlDataType.Int64;
      case TypeCode.UInt64:
        return SqlDataType.UInt64;
      case TypeCode.Single:
        return SqlDataType.Float;
      case TypeCode.Double:
        return SqlDataType.Double;
      case TypeCode.Decimal:
        return SqlDataType.Decimal;
      case TypeCode.DateTime:
        return SqlDataType.DateTime;
      case TypeCode.String:
        return SqlDataType.VarChar;
      default:
        throw new ArgumentOutOfRangeException();
      }
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
    
    private void RegisterCommand(ISqlCompileUnit command)
    {
      var commandText = driver.Compile(command).GetCommandText();
      switch (stage) {
        case UpgradeStage.Prepare:
        case UpgradeStage.CyclicRename:
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

    private void RegisterPreCommand(ISqlCompileUnit command)
    {
      preUpgradeCommands.Add(driver.Compile(command).GetCommandText());
    }
    
    private long? GetCurrentSequenceValue(string sequenceInfoName)
    {
      var sequenceInfo = sourceModel.Sequences.FirstOrDefault(si => si.Name==sequenceInfoName);
      return sequenceInfo==null ? null : sequenceInfo.Current;
    }

    # endregion
    

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="actions">The actions to translate.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="driver">The driver.</param>
    /// <param name="valueTypeBuilder">The value type builder.</param>
    /// <param name="sourceModel">The source model.</param>
    /// <param name="targetModel">The target model.</param>
    public SqlActionTranslator(ActionSequence actions, Schema schema, SqlDriver driver,
      Func<Type, int, SqlValueType> valueTypeBuilder, 
      StorageInfo sourceModel, StorageInfo targetModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(actions, "actions");
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");

      this.schema = schema;
      this.driver = driver;
      this.actions = actions;
      this.valueTypeBuilder = valueTypeBuilder;
      this.sourceModel = sourceModel;
      this.targetModel = targetModel;
    }
  }
}