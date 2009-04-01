// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.31


using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql.Dom.Database;
using SqlModel = Xtensive.Sql.Dom.Database.Model;
using SqlRefAction = Xtensive.Sql.Dom.ReferentialAction;
using Xtensive.Modelling;

namespace Xtensive.Storage.Indexing.Model
{
  /// <summary>
  /// Convert <see cref="Xtensive.Sql.Dom.Database.Model"/> to indexing storage model.
  /// </summary>
  public class SqlModelConverter : ModelVisitor<IPathNode>
  {
    private StorageInfo storageInfo;


    /// <summary>
    /// Converts the specified model <see cref="Schema"/> to <see cref="StorageInfo"/>.
    /// </summary>
    /// <param name="schema">The schema.</param>
    /// <returns></returns>
    public StorageInfo Convert(Schema schema)
    {
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");

      storageInfo = new StorageInfo(schema.Name);

      Visit(schema);

      return storageInfo;
    }


    private static IndexInfo FindIndex(TableInfo table, List<ColumnInfo> keyColumns)
    {
      if (table.PrimaryIndex != null) {
        var primaryKeyColumns = table.PrimaryIndex.KeyColumns.Select(cr => cr.Value);
        if (primaryKeyColumns.Except(keyColumns).Count() == 0)
          return table.PrimaryIndex;
      }

      foreach(SecondaryIndexInfo index in table.SecondaryIndexes) {
        var secondaryKeyColumns = index.KeyColumns.Select(cr => cr.Value);
        if (secondaryKeyColumns.Except(keyColumns).Count() == 0)
          return index;
      }

      return null;
    }
    
    /// <inheritdoc/>
    protected override IPathNode VisitSchema(Schema schema)
    {
      // Build tables, columns and indexes.
      foreach (var table in schema.Tables)
        VisitTable(table);

      // Buils foreign keys.
      foreach (var table in schema.Tables)
        foreach (var constraint in table.TableConstraints)
        {
          var foreignKey = constraint as ForeignKey;
          if (foreignKey != null)
            VisitForeignKey(foreignKey);
        }

      return null;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitTable(Table table)
    {
      if (storageInfo.Tables.Contains(table.Name))
        return storageInfo.Tables[table.Name];

      var tableInfo = new TableInfo(storageInfo, table.Name);

      // Columns.
      foreach (var column in table.TableColumns)
        VisitTableColumn(column);

      // Primary index.
      foreach (var constraint in table.TableConstraints) {
        // ToDo: Fix this!
        var primaryKey = constraint as UniqueConstraint;
        if (primaryKey!=null) {
          VisitUniqueConstraint(primaryKey);
          break;
        }
      }

      // Secondary indexes.
      foreach (var index in table.Indexes)
        VisitIndex(index);

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
      var tableInfo = Visit(key.Table) as TableInfo;

      var foreignKeyInfo = new ForeignKeyInfo(tableInfo, key.Name)
      {
        OnUpdateAction = ConvertReferentialAction(key.OnUpdate),
        OnRemoveAction = ConvertReferentialAction(key.OnDelete)
      };
      var referencedTable = tableInfo.Model.Tables[key.ReferencedTable.Name];
      var referencedColumns = new List<ColumnInfo>();
      foreach (var refColumn in key.Columns)
        referencedColumns.Add(referencedTable.Columns[refColumn.Name]);
      foreignKeyInfo.ReferencedIndex = FindIndex(referencedTable, referencedColumns);

      return foreignKeyInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitUniqueConstraint(UniqueConstraint constraint)
    {
      var tableInfo = storageInfo.Tables[constraint.Table.Name];
      var primaryIndexInfo = new PrimaryIndexInfo(tableInfo, constraint.Name);

      foreach (var pkColumn in constraint.Columns)
        new KeyColumnRef(primaryIndexInfo, tableInfo.Columns[pkColumn.Name],
          primaryIndexInfo.KeyColumns.Count, Direction.Positive);
      // ToDo: Get direction for key columns.
      // ToDo: Add value columns.

      return primaryIndexInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitIndex(Index index)
    {
      var tableInfo = storageInfo.Tables[index.DataTable.Name];
      var secondaryIndexInfo = new SecondaryIndexInfo(tableInfo, index.Name);
      
      foreach (var keyColumn in index.Columns)
        VisitIndexColumn(keyColumn);

      foreach (var valueColumn in index.NonkeyColumns)
        new ValueColumnRef(secondaryIndexInfo, tableInfo.Columns[valueColumn.Name],
          secondaryIndexInfo.ValueColumns.Count);

      return secondaryIndexInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitIndexColumn(IndexColumn indexColumn)
    {
      var tableInfo = storageInfo.Tables[indexColumn.Index.DataTable.Name];
      var indexInfo = tableInfo.SecondaryIndexes[indexColumn.Index.Name];
      var columnInfo = tableInfo.Columns[indexColumn.Name];

      return new KeyColumnRef(indexInfo,
        columnInfo, indexInfo.KeyColumns.Count,
        indexColumn.Ascending ? Direction.Positive : Direction.Negative);
    }
    
    /// <summary>
    /// Extracts the <see cref="TypeInfo"/> from <see cref="TableColumn"/>.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>Data type.</returns>
    protected virtual TypeInfo ExtractType(TableColumn column)
    {
      return new TypeInfo(
        // ToDo Convert SqlDataType to Type.
        typeof(int),
        // ToDo: Convert Collation to string.
        column.Collation != null ? column.Collation.ToString() : string.Empty,
        column.DataType.Size);
    }

    /// <summary>
    /// Converts the <see cref="Xtensive.Sql.Dom.ReferentialAction"/> to 
    /// <see cref="Xtensive.Storage.Indexing.Model.ReferentialAction"/>.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>Converted action.</returns>
    protected virtual ReferentialAction ConvertReferentialAction(SqlRefAction action)
    {
      switch (action)
      {
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


    #region Not implemented

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
    protected override IPathNode VisitPrimaryKey(PrimaryKey key)
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