// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.31

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Database;
using SqlModel = Xtensive.Sql.Dom.Database.Model;
using SqlRefAction = Xtensive.Sql.Dom.ReferentialAction;
using Xtensive.Modelling;

namespace Xtensive.Storage.Indexing.Model
{
  /// <summary>
  /// Converts <see cref="Xtensive.Sql.Dom.Database.Model"/> to indexing storage model.
  /// </summary>
  public class SqlModelConverter : SqlModelVisitor<IPathNode>
  {
    private StorageInfo storageInfo;
    private ServerInfo serverInfo;

    /// <summary>
    /// Converts the specified model <see cref="Schema"/> to <see cref="StorageInfo"/>.
    /// </summary>
    /// <param name="schema">The schema.</param>
    /// <param name="server">The server info.</param>
    /// <returns></returns>
    public StorageInfo Convert(Schema schema, ServerInfo server)
    {
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      
      serverInfo = server;
      storageInfo = new StorageInfo(schema.Name);

      Visit(schema);

      return storageInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitSchema(Schema schema)
    {
      // Build tables, columns and indexes.
      foreach (var table in schema.Tables)
        Visit(table);

      // Build foreign keys.
      var foreignKeys = schema.Tables.SelectMany(
        t => t.TableConstraints.OfType<ForeignKey>());
      foreach (var foreignKey in foreignKeys)
        Visit(foreignKey);

      return null;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitTable(Table table)
    {
      var tableInfo = new TableInfo(storageInfo, table.Name);

      foreach (var column in table.TableColumns)
        Visit(column);

      var primaryKey = table.TableConstraints.OfType<PrimaryKey>().FirstOrDefault();
      if (primaryKey!=null)
        Visit(primaryKey);

      foreach (var index in table.Indexes)
        Visit(index);

      return tableInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitTableColumn(TableColumn tableColumn)
    {
      var tableInfo = storageInfo.Tables[tableColumn.Table.Name];
      var columnInfo = new ColumnInfo(tableInfo, tableColumn.Name, ExtractType(tableColumn))
        {
          AllowNulls = tableColumn.IsNullable
        };
      return columnInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitForeignKey(ForeignKey key)
    {
      var tableInfo = storageInfo.Tables[key.Table.Name];

      var foreignKeyInfo = new ForeignKeyInfo(tableInfo, key.Name)
      {
        OnUpdateAction = ConvertReferentialAction(key.OnUpdate),
        OnRemoveAction = ConvertReferentialAction(key.OnDelete)
      };
      // ToDo: Complete this!
      var referencedTable = tableInfo.Model.Tables[key.ReferencedTable.Name];
      var referencingTable = tableInfo.Model.Tables[key.Table.Name];
      var referencingColumns = new List<ColumnInfo>();
      foreach (var refColumn in key.Columns)
        referencingColumns.Add(referencingTable.Columns[refColumn.Name]);
      
      foreignKeyInfo.ReferencingIndex = FindIndex(referencingTable, referencingColumns);
      foreignKeyInfo.ReferencedIndex = referencedTable.PrimaryIndex;

      return foreignKeyInfo;
    }
    
    /// <inheritdoc/>
    protected override IPathNode VisitPrimaryKey(PrimaryKey key)
    {
      var tableInfo = storageInfo.Tables[key.Table.Name];
      var primaryIndexInfo = new PrimaryIndexInfo(tableInfo, key.Name);

      foreach (var keyColumn in key.Columns)
        new KeyColumnRef(primaryIndexInfo, tableInfo.Columns[keyColumn.Name],
          primaryIndexInfo.KeyColumns.Count, Direction.Positive);
      // ToDo: Get direction for key columns.

      var valueColumns = tableInfo.Columns.Except(
        primaryIndexInfo.KeyColumns.Select(cr => cr.Value));

      foreach (var valueColumn in valueColumns)
        new ValueColumnRef(primaryIndexInfo, valueColumn,
          primaryIndexInfo.ValueColumns.Count);

      return primaryIndexInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitIndex(Index index)
    {
      var tableInfo = storageInfo.Tables[index.DataTable.Name];
      var secondaryIndexInfo = new SecondaryIndexInfo(tableInfo, index.Name)
        {
          IsUnique = index.IsUnique
        };

      foreach (var keyColumn in index.Columns) {
        var columnInfo = tableInfo.Columns[keyColumn.Column.Name];
        new KeyColumnRef(secondaryIndexInfo,
          columnInfo, secondaryIndexInfo.KeyColumns.Count,
          keyColumn.Ascending ? Direction.Positive : Direction.Negative);
      }

      foreach (var valueColumn in index.NonkeyColumns) {
        var columnInfo = tableInfo.Columns[valueColumn.Name];
        new ValueColumnRef(secondaryIndexInfo, columnInfo,
          secondaryIndexInfo.ValueColumns.Count);
      }

      return secondaryIndexInfo;
    }

    
    /// <summary>
    /// Extracts the <see cref="TypeInfo"/> from <see cref="TableColumn"/>.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>Data type.</returns>
    protected virtual TypeInfo ExtractType(TableColumn column)
    {
      return new TypeInfo(
        ConvertType(column.DataType.DataType),
        column.Collation != null ? column.Collation.Name : null,
        column.DataType.Size);
    }

    /// <summary>
    /// Converts the <see cref="Xtensive.Sql.Dom.ReferentialAction"/> to 
    /// <see cref="Xtensive.Storage.Indexing.Model.ReferentialAction"/>.
    /// </summary>
    /// <param name="toConvert">The action to convert.</param>
    /// <returns>Converted action.</returns>
    protected virtual ReferentialAction ConvertReferentialAction(SqlRefAction toConvert)
    {
      switch (toConvert) {
        case SqlRefAction.NoAction:
          return ReferentialAction.None;
        case SqlRefAction.Restrict:
          return ReferentialAction.Restrict;
        case SqlRefAction.Cascade:
          return ReferentialAction.Cascade;
        case SqlRefAction.SetNull:
          return ReferentialAction.Clear;
        case SqlRefAction.SetDefault:
          return ReferentialAction.Default;
        default:
          return ReferentialAction.Default;
      }
    }

    /// <summary>
    /// Converts the <see cref="SqlDataType"/> to <see cref="Type"/>.
    /// </summary>
    /// <param name="toConvert"><see cref="SqlDataType"/> to convert.</param>
    /// <returns>Converted type.</returns>
    protected virtual Type ConvertType(SqlDataType toConvert)
    {
      return serverInfo.DataTypes[toConvert].Type;
    }

    /// <summary>
    /// Finds the specific index by key columns.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="keyColumns">The key columns.</param>
    /// <returns>The index.</returns>
    protected virtual IndexInfo FindIndex(TableInfo table, List<ColumnInfo> keyColumns)
    {
      foreach (SecondaryIndexInfo index in table.SecondaryIndexes) {
        var secondaryKeyColumns = index.KeyColumns.Select(cr => cr.Value);
        if (secondaryKeyColumns.Except(keyColumns).Count()==0)
          return index;
      }

      return null;
    }

    #region Not implemented

    /// <inheritdoc/>
    protected override IPathNode VisitUniqueConstraint(UniqueConstraint constraint)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitIndexColumn(IndexColumn indexColumn)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitCatalog(Catalog catalog)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitCharacterSet(CharacterSet characterSet)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitCollation(Collation collation)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitDataTable(DataTable dataTable)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitDataTableColumn(DataTableColumn dataTableColumn)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitDomain(Domain domain)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitHashPartition(HashPartition hashPartition)
    {
      throw new System.NotImplementedException();
    }
    
    /// <inheritdoc/>
    protected override IPathNode VisitListPartition(ListPartition listPartition)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitModel(SqlModel model)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitPartition(Partition partition)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitPartitionDescriptor(PartitionDescriptor partitionDescriptor)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitPartitionFunction(PartitionFunction partitionFunction)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitPartitionSchema(PartitionSchema partitionSchema)
    {
      throw new System.NotImplementedException();
    }
    
    /// <inheritdoc/>
    protected override IPathNode VisitTableConstraint(TableConstraint constraint)
    {
      throw new System.NotImplementedException();
    }
    
    /// <inheritdoc/>
    protected override IPathNode VisitDomainConstraint(DomainConstraint constraint)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitConstraint(Constraint constraint)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitCheckConstraint(CheckConstraint constraint)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitRangePartition(RangePartition rangePartition)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitSequence(Sequence sequence)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitSequenceDescriptor(SequenceDescriptor sequenceDescriptor)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitServer(Server server)
    {
      throw new System.NotImplementedException();
    }
    
    /// <inheritdoc/>
    protected override IPathNode VisitTemporaryTable(TemporaryTable temporaryTable)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitTranslation(Translation translation)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitUser(User user)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitView(View view)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitViewColumn(ViewColumn viewColumn)
    {
      throw new System.NotImplementedException();
    }

    #endregion
  }
}