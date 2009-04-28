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
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Xtensive.Core;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom;
using System.Collections.Generic;
using ColumnInfo=Xtensive.Storage.Indexing.Model.ColumnInfo;
using TableInfo=Xtensive.Storage.Indexing.Model.TableInfo;
using SqlRefAction = Xtensive.Sql.Dom.ReferentialAction;
using ReferentialAction = Xtensive.Storage.Indexing.Model.ReferentialAction;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Translates actions to Sql.
  /// </summary>
  [Serializable]
  public sealed class SqlActionTranslator
  {
    private readonly ActionSequence actions;
    private readonly StorageInfo newModel;
    private readonly StorageInfo oldModel;
    private readonly Schema sourceSchema;
    private readonly Schema targetSchema;
    private SqlBatch batch;
    private List<Table> createdTables;
    private readonly Func<Type, int, SqlValueType> valueTypeBuilder;

    /// <summary>
    /// Translates actions to Sql.
    /// </summary>
    /// <returns></returns>
    public SqlBatch Translate()
    {
      createdTables = new List<Table>();
      batch = SqlFactory.Batch();
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

      return batch;
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
      if (action.Type==typeof(TableInfo))
        VisitCreateTableAction(action);
      else if (action.Type==typeof(ColumnInfo))
        VisitCreateColumnAction(action);
      else if (action.Type==typeof(PrimaryIndexInfo))
        VisitCreatePrimaryKeyAction(action);
      else if (action.Type==typeof(SecondaryIndexInfo))
        VisitCreateSecondaryIndexAction(action);
      else if (action.Type==typeof(ForeignKeyInfo))
        VisitCreateForeignKeyAction(action);
    }

    private void VisitRemoveAction(RemoveNodeAction action)
    {
      var node = oldModel.Resolve(action.Path);
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
    }

    private void VisitAlterAction(NodeAction action)
    {
      // TODO: Implement PropertyChangeAction translation
      if (action is PropertyChangeAction)
        return;

      // TODO: Implement MoveNodeAction translation
      if (action is MoveNodeAction)
        throw new NotImplementedException();
      
      var node = newModel.Resolve(action.Path);
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

    # region Visit concrete actions methods

    private void VisitCreateTableAction(CreateNodeAction action)
    {
      var tableInfo = newModel.Tables[action.Name];
      var table = CreateTable(tableInfo);

      // Mark table as newly created
      createdTables.Add(table);

      batch.Add(SqlFactory.Create(table));
    }

    private void VisitRemoveTableAction(RemoveNodeAction action)
    {
      var tableInfo = oldModel.Resolve(action.Path) as TableInfo;
      var table = sourceSchema.Tables[tableInfo.Name];
      batch.Add(SqlFactory.Drop(table));
    }

    private void VisitAlterTableAction(NodeAction action)
    {
      throw new NotImplementedException();
    }

    private void VisitCreateColumnAction(CreateNodeAction createColumnAction)
    {
      var tableInfo = newModel.Resolve(createColumnAction.Path) as TableInfo;
      var columnInfo = tableInfo.Columns[createColumnAction.Name];
      var table = targetSchema.Tables[columnInfo.Parent.Name];
      
      // Ensure table is not newly created
      if (createdTables.Contains(table))
        return;
      
      var column = CreateColumn(columnInfo, table);
      batch.Add(
        SqlFactory.Alter(table,
          SqlFactory.AddColumn(column)));
    }
    
    private void VisitRemoveColumnAction(RemoveNodeAction removeColumnAction)
    {
      var columnInfo = oldModel.Resolve(removeColumnAction.Path) as ColumnInfo;
      var table = sourceSchema.Tables[columnInfo.Parent.Name];
      if (table==null)
        return;
      var column = table.TableColumns[columnInfo.Name];

      // Remove column from target schema
      RemoveColumn(column);

      batch.Add(
        SqlFactory.Alter(table,
          SqlFactory.DropColumn(column)));
    }

    private void VisitAlterColumnAction(NodeAction action)
    {
      throw new NotImplementedException();
    }

    private void VisitCreatePrimaryKeyAction(CreateNodeAction action)
    {
      var tableInfo = newModel.Resolve(action.Path) as TableInfo;
      var table = targetSchema.Tables[tableInfo.Name];

      // Ensure table is not newly created
      if (createdTables.Contains(table))
        return;
      
      var primaryKey = CreatePrimaryKey(tableInfo, table);
      batch.Add(
        SqlFactory.Alter(table,
          SqlFactory.AddConstraint(primaryKey)));
    }

    private void VisitRemovePrimaryKeyAction(RemoveNodeAction action)
    {
      var primaryIndexInfo = oldModel.Resolve(action.Path) as PrimaryIndexInfo;
      var table = sourceSchema.Tables[primaryIndexInfo.Parent.Name];
      if (table==null)
        return;
      var primaryKey = table.TableConstraints[primaryIndexInfo.Name];

      // Remove key from target schema
      RemovePrimaryKey(primaryKey as PrimaryKey);

      batch.Add(
        SqlFactory.Alter(table,
          SqlFactory.DropConstraint(primaryKey)));
    }

    private void VisitAlterPrimaryKeyAction(NodeAction action)
    {
      throw new NotImplementedException();
    }
    
    private void VisitCreateSecondaryIndexAction(CreateNodeAction action)
    {
      var tableInfo = newModel.Resolve(action.Path) as TableInfo;
      var secondaryIndexInfo = tableInfo.SecondaryIndexes[action.Name];
      var table = targetSchema.Tables[tableInfo.Name];
      var index = CreateSecondaryIndex(table, secondaryIndexInfo);
      batch.Add(SqlFactory.Create(index));
    }

    private void VisitRemoveSecondaryIndexAction(RemoveNodeAction action)
    {
      var secondaryIndexInfo = oldModel.Resolve(action.Path) as SecondaryIndexInfo;
      var table = sourceSchema.Tables[secondaryIndexInfo.Parent.Name];
      if (table==null)
        return;
      var index = table.Indexes[secondaryIndexInfo.Name];
      
      // Remove index from target schema
      RemoveIndex(index);

      batch.Add(SqlFactory.Drop(index));
    }

    private void VisitAlterSecondaryIndexAction(NodeAction action)
    {
      throw new NotImplementedException();
    }
    
    private void VisitCreateForeignKeyAction(CreateNodeAction action)
    {
      var tableInfo = newModel.Resolve(action.Path) as TableInfo;
      var foreignKeyInfo = tableInfo.ForeignKeys[action.Name];
      var table = targetSchema.Tables[tableInfo.Name];
      var foreignKey = CreateForeignKey(foreignKeyInfo);
      batch.Add(
        SqlFactory.Alter(table,
          SqlFactory.AddConstraint(foreignKey)));

      // Remove foreign key from table for correct sql statement order
      table.TableConstraints.Remove(foreignKey);
    }

    private void VisitRemoveForeignKeyAction(RemoveNodeAction action)
    {
      var foreignKeyInfo = oldModel.Resolve(action.Path) as ForeignKeyInfo;
      var table = sourceSchema.Tables[foreignKeyInfo.Parent.Name];
      if (table==null)
        return;
      var foreignKey = table.TableConstraints[foreignKeyInfo.Name];

      // Remove key from target schema
      RemoveForeignKey(foreignKey as ForeignKey);

      batch.Add(
        SqlFactory.Alter(table,
          SqlFactory.DropConstraint(foreignKey)));
    }

    private void VisitAlterForeignKeyAction(NodeAction action)
    {
      throw new NotImplementedException();
    }

    # endregion

    # region Helpers methods

    private void RemoveIndex(Index index)
    {
      var targetTable = targetSchema.Tables[index.DataTable.Name];
      var targetIndex = targetTable.Indexes[index.Name];
      targetTable.Indexes.Remove(targetIndex);
    }

    private void RemovePrimaryKey(PrimaryKey primaryKey)
    {
      var targetTable = targetSchema.Tables[primaryKey.Table.Name];
      var targetPrimaryKey = targetTable.TableConstraints[primaryKey.Name];
      targetTable.TableConstraints.Remove(targetPrimaryKey);
    }

    private void RemoveForeignKey(ForeignKey foreignKey)
    {
      var targetTable = targetSchema.Tables[foreignKey.Table.Name];
      var targetForeignKey = targetTable.TableConstraints[foreignKey.Name];
      targetTable.TableConstraints.Remove(targetForeignKey);
    }

    private void RemoveColumn(TableColumn column)
    {
      var targetTable = targetSchema.Tables[column.DataTable.Name];
      var tableColumn = targetTable.TableColumns[column.Name];
      targetTable.TableColumns.Remove(tableColumn);
    }

    private Table CreateTable(TableInfo tableInfo)
    {
      var table = targetSchema.CreateTable(tableInfo.Name);
      foreach (var columnInfo in tableInfo.Columns)
        CreateColumn(columnInfo, table);

      if (tableInfo.PrimaryIndex!=null)
        CreatePrimaryKey(tableInfo, table);

      return table;
    }

    private TableColumn CreateColumn(ColumnInfo columnInfo, Table table)
    {
      var type = GetSqlType(columnInfo);
      var column = table.CreateColumn(columnInfo.Name, type);
      column.IsNullable = columnInfo.Type.IsNullable;
      if (columnInfo.Sequence != null) {
        column.SequenceDescriptor =
          new SequenceDescriptor(column,
            columnInfo.Sequence.StartValue,
            columnInfo.Sequence.Increment);
      }
      return column;
    }

    private ForeignKey CreateForeignKey(ForeignKeyInfo foreignKeyInfo)
    {
      var referencingTable = targetSchema.Tables[foreignKeyInfo.Parent.Name];
      var foreignKey = referencingTable.CreateForeignKey(foreignKeyInfo.Name);
      foreignKey.OnUpdate = ConvertReferentialAction(foreignKeyInfo.OnUpdateAction);
      foreignKey.OnDelete = ConvertReferentialAction(foreignKeyInfo.OnRemoveAction);
      var referncingColumns = foreignKeyInfo.ForeignKeyColumns
        .Select(cr => referencingTable.TableColumns[cr.Value.Name]);
      foreignKey.Columns.AddRange(referncingColumns);
      var referencedTable = targetSchema.Tables[foreignKeyInfo.PrimaryKey.Parent.Name];
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

    private SqlValueType GetSqlType(ColumnInfo columnInfo)
    {
      var type = columnInfo.Type.Type.IsValueType
        && columnInfo.Type.Type.IsNullable()
        ? columnInfo.Type.Type.GetGenericArguments()[0]
        : columnInfo.Type.Type;

      return
        valueTypeBuilder!=null
          ? valueTypeBuilder.Invoke(type, columnInfo.Type.Length)
          : BuildSqlValueType(type, columnInfo.Type.Length);
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
    
    # endregion

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SqlActionTranslator(ActionSequence actions, StorageInfo newModel,
      StorageInfo oldModel, Schema sourceSchema, Schema targetSchema, 
      Func<Type, int, SqlValueType> valueTypeBuilder)
    {
      ArgumentValidator.EnsureArgumentNotNull(actions, "actions");
      ArgumentValidator.EnsureArgumentNotNull(newModel, "newModel");
      ArgumentValidator.EnsureArgumentNotNull(oldModel, "oldModel");
      ArgumentValidator.EnsureArgumentNotNull(sourceSchema, "sourceSchema");
      ArgumentValidator.EnsureArgumentNotNull(targetSchema, "targetSchema");

      this.actions = actions;
      this.oldModel = oldModel;
      this.sourceSchema = sourceSchema;
      this.targetSchema = targetSchema;
      this.valueTypeBuilder = valueTypeBuilder;
      this.newModel = newModel;
    }
  }
}