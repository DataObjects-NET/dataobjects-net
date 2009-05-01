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

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Translates actions to Sql.
  /// </summary>
  [Serializable]
  public sealed class SqlActionTranslator
  {
    private const string GeneratorColumnName = "ID";

    private readonly ActionSequence actions;
    private readonly Schema schema;

    private readonly Func<Type, int, SqlValueType> valueTypeBuilder;
    private readonly SqlDriver driver;
    private readonly List<string> commands = new List<string>();
    private readonly List<Table> createdTables = new List<Table>();
    private bool translated;

    private bool IsSequencesAllowed
    {
      get { return driver.ServerInfo.Sequence.Features!=SequenceFeatures.None; }
    }

    /// <summary>
    /// Gets the translation result.
    /// </summary>
    public List<string> UpgradeCommandText
    {
      get
      {
        if (!translated)
          Translate();
        return commands;
      }
    }

    private void Translate()
    {
      // Cleanup
      var cleanup = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment=="Cleanup");
      if (cleanup!=null)
        VisitAction(cleanup);
      // Upgrade
      var upgrade = actions.OfType<GroupingNodeAction>()
        .FirstOrDefault(ga => ga.Comment=="Upgrade");
      if (upgrade!=null)
        VisitAction(upgrade);
      
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
      // TODO: Implement PropertyChangeAction translation
      if (action is PropertyChangeAction)
        return;
      
      // TODO: Implement MoveNodeAction translation
      if (action is MoveNodeAction)
        throw new NotImplementedException();
      
      var node = action.Difference.Source;
      if (node.GetType()==typeof (TableInfo))
        VisitAlterTableAction(action);
      else if (node.GetType()==typeof (ColumnInfo))
        VisitAlterColumnAction(action);
      else if (node.GetType()==typeof (PrimaryIndexInfo))
        VisitAlterPrimaryKeyAction(action);
      else if (node.GetType()==typeof (SecondaryIndexInfo))
        VisitAlterSecondaryIndexAction(action);
      else if (node.GetType()==typeof (ForeignKeyInfo))
        VisitAlterForeignKeyAction(action);
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
      var table = schema.Tables[tableInfo.Name];
      RegisterCommand(SqlFactory.Drop(table));
      schema.Tables.Remove(table);
    }

    private void VisitAlterTableAction(NodeAction action)
    {
      throw new NotImplementedException();
    }

    private void VisitCreateColumnAction(CreateNodeAction createColumnAction)
    {
      var columnInfo = createColumnAction.Difference.Target as ColumnInfo;
      var table = schema.Tables[columnInfo.Parent.Name];
      
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
      var table = schema.Tables[columnInfo.Parent.Name];
      
      // Ensure table is not removed
      if (table==null)
        return;
      
      var column = table.TableColumns[columnInfo.Name];
      RegisterCommand(SqlFactory.Alter(table,
          SqlFactory.DropColumn(column)));
      table.TableColumns.Remove(column);
    }

    private void VisitAlterColumnAction(NodeAction action)
    {
      throw new NotImplementedException();
    }

    private void VisitCreatePrimaryKeyAction(CreateNodeAction action)
    {
      var primaryIndex = action.Difference.Target as PrimaryIndexInfo;
      var table = schema.Tables[primaryIndex.Parent.Name];

      if (createdTables.Contains(table))
        return;

      var primaryKey = CreatePrimaryKey(primaryIndex.Parent, table);
      RegisterCommand(SqlFactory.Alter(table,
          SqlFactory.AddConstraint(primaryKey)));
    }

    private void VisitRemovePrimaryKeyAction(RemoveNodeAction action)
    {
      var primaryIndexInfo = action.Difference.Source as PrimaryIndexInfo;
      var table = schema.Tables[primaryIndexInfo.Parent.Name];

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
      var table = schema.Tables[secondaryIndexInfo.Parent.Name];
      var index = CreateSecondaryIndex(table, secondaryIndexInfo);
      RegisterCommand(SqlFactory.Create(index));
    }

    private void VisitRemoveSecondaryIndexAction(RemoveNodeAction action)
    {
      var secondaryIndexInfo = action.Difference.Source as SecondaryIndexInfo;
      var table = schema.Tables[secondaryIndexInfo.Parent.Name];
      
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
      var table = schema.Tables[foreignKeyInfo.Parent.Name];
      var foreignKey = CreateForeignKey(foreignKeyInfo);

      // If referencedTable table is newly created 
      // set referencing fields to default values
      var referencedTable = foreignKey.ReferencedTable;
      if (createdTables.Contains(referencedTable)) {
        var referencingTable = foreignKey.Table;
        var tableRef = SqlFactory.TableRef(referencingTable);
        foreach (var column in foreignKey.Columns) {
          var update =SqlFactory.Update(tableRef);
          update.Values[tableRef[column.Name]] = SqlFactory.DefaultValue;
          RegisterCommand(update);
        }
      }
      
      RegisterCommand(SqlFactory.Alter(table,
          SqlFactory.AddConstraint(foreignKey)));

      // Remove foreign key from table for correct sql statement order
      table.TableConstraints.Remove(foreignKey);
    }

    private void VisitRemoveForeignKeyAction(RemoveNodeAction action)
    {
      var foreignKeyInfo = action.Difference.Source as ForeignKeyInfo;
      var table = schema.Tables[foreignKeyInfo.Parent.Name];

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
        var sequenceTable = schema.CreateTable(sequenceInfo.Name);
        var idColumn = sequenceTable.CreateColumn(GeneratorColumnName,
          GetSqlType(sequenceInfo.Type));
        idColumn.SequenceDescriptor =
          new SequenceDescriptor(
            idColumn,
            sequenceInfo.Current.HasValue
              ? sequenceInfo.Current
              : sequenceInfo.StartValue,
            sequenceInfo.Increment);
        RegisterCommand(SqlFactory.Create(sequenceTable));
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
        var sequenceTable = schema.Tables[sequenceInfo.Name];
        RegisterCommand(SqlFactory.Drop(sequenceTable));
        schema.Tables.Remove(sequenceTable);
      }
    }

    private void VisitAlterSequenceAction(NodeAction action)
    {
      var sequenceInfo = action.Difference.Source as SequenceInfo;
      if (IsSequencesAllowed) {
        var sequence = schema.Sequences[sequenceInfo.Name];
        var sequenceDescriptor = new SequenceDescriptor(sequence,
          sequenceInfo.StartValue, sequenceInfo.Increment);
        sequence.SequenceDescriptor = sequenceDescriptor;
        RegisterCommand(SqlFactory.Alter(sequence,
            sequenceDescriptor));
      }
      else {
        var sequenceTable = schema.Tables[sequenceInfo.Name];
        var idColumn = sequenceTable.TableColumns[GeneratorColumnName];
        var sequenceDescriptor = new SequenceDescriptor(idColumn,
          sequenceInfo.StartValue, sequenceInfo.Increment);
        idColumn.SequenceDescriptor = sequenceDescriptor;
        RegisterCommand(SqlFactory.Alter(sequenceTable,
            SqlFactory.Alter(idColumn, sequenceDescriptor)));
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
      column.IsNullable = columnInfo.Type.IsNullable;
      return column;
    }

    private ForeignKey CreateForeignKey(ForeignKeyInfo foreignKeyInfo)
    {
      var referencingTable = schema.Tables[foreignKeyInfo.Parent.Name];
      var foreignKey = referencingTable.CreateForeignKey(foreignKeyInfo.Name);
      foreignKey.OnUpdate = ConvertReferentialAction(foreignKeyInfo.OnUpdateAction);
      foreignKey.OnDelete = ConvertReferentialAction(foreignKeyInfo.OnRemoveAction);
      var referncingColumns = foreignKeyInfo.ForeignKeyColumns
        .Select(cr => referencingTable.TableColumns[cr.Value.Name]);
      foreignKey.Columns.AddRange(referncingColumns);
      var referencedTable = schema.Tables[foreignKeyInfo.PrimaryKey.Parent.Name];
      var referencedColumns = foreignKeyInfo.PrimaryKey.KeyColumns
        .Select(cr => referencedTable.TableColumns[cr.Value.Name]);
      foreignKey.ReferencedTable = referencedTable;
      foreignKey.ReferencedColumns.AddRange(referencedColumns);
      return foreignKey;
    }

    private static PrimaryKey CreatePrimaryKey(TableInfo tableInfo, Table table)
    {
      return
        table.CreatePrimaryKey(tableInfo.PrimaryIndex.Name,
          tableInfo.PrimaryIndex.KeyColumns
            .Select(cr => table.TableColumns[cr.Value.Name]).ToArray());
    }

    private static Index CreateSecondaryIndex(Table table, SecondaryIndexInfo indexInfo)
    {
      var index = table.CreateIndex(indexInfo.Name);
      index.IsUnique = indexInfo.IsUnique;
      foreach (var keyColumn in indexInfo.KeyColumns)
        index.CreateIndexColumn(
          table.TableColumns[keyColumn.Value.Name],
          keyColumn.Direction==Direction.Positive);
      index.NonkeyColumns.AddRange(
        indexInfo.IncludedColumns
          .Select(cr => table.TableColumns[cr.Value.Name]).ToArray());
      return index;
    }

    private SqlValueType GetSqlType(TypeInfo typeInfo)
    {
      var type = typeInfo.Type.IsValueType
        && typeInfo.Type.IsNullable()
        ? typeInfo.Type.GetGenericArguments()[0]
        : typeInfo.Type;

      return
        valueTypeBuilder!=null
          ? valueTypeBuilder.Invoke(type, typeInfo.Length)
          : BuildSqlValueType(type, typeInfo.Length);
    }

    private static SqlValueType BuildSqlValueType(Type type, int length)
    {
      var dataType = GetDbType(type);
      return new SqlValueType(dataType, length);
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
      commands.Add(driver.Compile(command).GetCommandText());
    }

    # endregion
    

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SqlActionTranslator(ActionSequence actions, Schema schema, SqlDriver driver,
      Func<Type, int, SqlValueType> valueTypeBuilder)
    {
      ArgumentValidator.EnsureArgumentNotNull(actions, "actions");
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");

      this.schema = schema;
      this.driver = driver;
      this.actions = actions;
      this.valueTypeBuilder = valueTypeBuilder;
    }
  }
}