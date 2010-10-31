// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.31

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Reflection;
using Xtensive.Modelling;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Storage.Model;
using ColumnInfo = Xtensive.Storage.Model.ColumnInfo;
using IndexInfo = Xtensive.Storage.Model.IndexInfo;
using Node = Xtensive.Sql.Model.Node;
using ReferentialAction = Xtensive.Storage.Model.ReferentialAction;
using SequenceInfo = Xtensive.Storage.Model.SequenceInfo;
using SqlRefAction = Xtensive.Sql.ReferentialAction;
using TableInfo = Xtensive.Storage.Model.TableInfo;
using Xtensive.Collections;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Converts <see cref="Catalog"/> to indexing storage model.
  /// </summary>
  public class SqlModelConverter : SqlModelVisitor<IPathNode>
  {
    /// <summary>
    /// Gets the storage info.
    /// </summary>
    protected StorageInfo StorageInfo { get; private set; }

    /// <summary>
    /// Gets the provider info.
    /// </summary>
    protected ProviderInfo ProviderInfo { get; private set;}

    /// <summary>
    /// Gets the schema.
    /// </summary>
    protected Schema Schema { get; private set; }

    /// <summary>
    /// Get the result of conversion specified 
    /// <see cref="Schema"/> to <see cref="StorageInfo"/>.
    /// </summary>
    /// <returns>The storage model.</returns>
    public StorageInfo GetConversionResult()
    {
      if (StorageInfo == null) {
        StorageInfo = new StorageInfo();
        Visit(Schema);
      }

      return StorageInfo;
    }

    #region SqlModelVisitor<IPathNode> implementation

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
      foreach (var sequence in schema.Sequences)
        VisitSequence(sequence);

      return null;
    }

    /// <inheritdoc/>
    protected override IPathNode Visit(Node node)
    {
      if (!ProviderInfo.Supports(ProviderFeatures.Sequences)) {
        var table = node as Table;
        if (table!=null && IsGeneratorTable(table))
          return VisitGeneratorTable(table);
      }

      return base.Visit(node);
    }

    /// <inheritdoc/>
    protected override IPathNode VisitTable(Table table)
    {
      var tableInfo = new TableInfo(StorageInfo, table.Name);

      foreach (var column in table.TableColumns)
        Visit(column);

      var primaryKey = table.TableConstraints
        .SingleOrDefault(constraint=>constraint is PrimaryKey);
      if (primaryKey != null)
        Visit(primaryKey);

      foreach (var index in table.Indexes)
        Visit(index);

      return tableInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitTableColumn(TableColumn tableColumn)
    {
      var tableInfo = StorageInfo.Tables[tableColumn.Table.Name];
      var typeInfo = ExtractType(tableColumn);
      var columnInfo = new ColumnInfo(tableInfo, tableColumn.Name, typeInfo);
      return columnInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitForeignKey(ForeignKey key)
    {
      var referencingTable = StorageInfo.Tables[key.Table.Name];
      var referencingColumns = new List<ColumnInfo>();
      foreach (var refColumn in key.Columns)
        referencingColumns.Add(referencingTable.Columns[refColumn.Name]);

      var foreignKeyInfo = new ForeignKeyInfo(referencingTable, key.Name) {
        OnUpdateAction = ConvertReferentialAction(key.OnUpdate),
        OnRemoveAction = ConvertReferentialAction(key.OnDelete)
      };

      var referencedTable = StorageInfo.Tables[key.ReferencedTable.Name];
      foreignKeyInfo.PrimaryKey = referencedTable.PrimaryIndex;

      foreach (var column in referencingColumns)
        new ForeignKeyColumnRef(foreignKeyInfo, column);

      return foreignKeyInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitPrimaryKey(PrimaryKey key)
    {
      var tableInfo = StorageInfo.Tables[key.Table.Name];
      var primaryIndexInfo = new PrimaryIndexInfo(tableInfo, key.Name);

      foreach (var keyColumn in key.Columns)
        new KeyColumnRef(primaryIndexInfo, tableInfo.Columns[keyColumn.Name],
          Direction.Positive);
      // TODO: Get direction for key columns
      primaryIndexInfo.PopulateValueColumns();

      return primaryIndexInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitFullTextIndex(FullTextIndex index)
    {
      var tableInfo = StorageInfo.Tables[index.DataTable.Name];
      var name = index.Name.IsNullOrEmpty()
        ? string.Format("FT_{0}", index.DataTable.Name)
        : index.Name;
      var ftIndex = new FullTextIndexInfo(tableInfo, name) {
        FullTextCatalog = index.FullTextCatalog
      };
      foreach (var column in index.Columns) {
        var columnInfo = tableInfo.Columns[column.Column.Name];
        new FullTextColumnRef(ftIndex, columnInfo, column.Languages.Single().Name);
      }
      return ftIndex;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitIndex(Index index)
    {
      var tableInfo = StorageInfo.Tables[index.DataTable.Name];
      var secondaryIndexInfo = new SecondaryIndexInfo(tableInfo, index.Name) {
        IsUnique = index.IsUnique
      };

      foreach (var keyColumn in index.Columns) {
        var columnInfo = tableInfo.Columns[keyColumn.Column.Name];
        new KeyColumnRef(secondaryIndexInfo,
          columnInfo, keyColumn.Ascending ? Direction.Positive : Direction.Negative);
      }

      foreach (var valueColumn in index.NonkeyColumns) {
        var columnInfo = tableInfo.Columns[valueColumn.Name];
        new IncludedColumnRef(secondaryIndexInfo, columnInfo);
      }

      secondaryIndexInfo.PopulatePrimaryKeyColumns();

      return secondaryIndexInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitSequence(Sequence sequence)
    {
      Type type = null;
      try {
        type = sequence.DataType.Type.ToClrType();
      }
      catch (ArgumentException) {
        type = null;
      }
      var typeInfo = type!=null ? new TypeInfo(type, null) : TypeInfo.Undefined;

      var sequenceInfo = new SequenceInfo(StorageInfo, sequence.Name) {
        Increment = sequence.SequenceDescriptor.Increment.Value,
        // StartValue = sequence.SequenceDescriptor.StartValue.Value,
        Type = typeInfo
      };
      return sequenceInfo;
    }

    #endregion

    /// <summary>
    /// Visits the generator table.
    /// </summary>
    /// <param name="generatorTable">The generator table.</param>
    /// <returns>Visit result.</returns>
    protected virtual IPathNode VisitGeneratorTable(Table generatorTable)
    {
      var idColumn = generatorTable.TableColumns[0];
      var startValue = idColumn.SequenceDescriptor.StartValue;
      var increment = idColumn.SequenceDescriptor.Increment;
      var type = ExtractType(idColumn);
      var sequence =
        new SequenceInfo(StorageInfo, generatorTable.Name) {
          Seed = startValue ?? 0,
          Increment = increment ?? 1,
          Type = type,
        };

      return sequence;
    }

    /// <summary>
    /// Extracts the <see cref="TypeInfo"/> from <see cref="TableColumn"/>.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>Data type.</returns>
    protected virtual TypeInfo ExtractType(TableColumn column)
    {
      var sqlValueType = column.DataType;
      Type type;
      try {
        type = sqlValueType.Type.ToClrType();
      }
      catch(ArgumentException) {
        return TypeInfo.Undefined;
      }

      if (column.IsNullable 
        && type.IsValueType 
        && !type.IsNullable())
        type = type.ToNullable();
        
      return new TypeInfo(type, column.IsNullable, sqlValueType.Length, sqlValueType.Scale, sqlValueType.Precision, sqlValueType);
    }

    /// <summary>
    /// Converts the <see cref="Xtensive.Sql.ReferentialAction"/> to <see cref="ReferentialAction"/>.
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
    /// Finds the specific index by key columns.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="keyColumns">The key columns.</param>
    /// <returns>The index.</returns>
    protected virtual IndexInfo FindIndex(TableInfo table, List<ColumnInfo> keyColumns)
    {
      var primaryKeyColumns = table.PrimaryIndex.KeyColumns.Select(cr => cr.Value);
      if (primaryKeyColumns.Except(keyColumns)
        .Union(keyColumns.Except(primaryKeyColumns)).Count()==0)
        return table.PrimaryIndex;

      foreach (var index in table.SecondaryIndexes) {
        var secondaryKeyColumns = index.KeyColumns.Select(cr => cr.Value);
        if (secondaryKeyColumns.Except(keyColumns)
          .Union(keyColumns.Except(secondaryKeyColumns)).Count()==0)
          return index;
      }

      return null;
    }

    /// <summary>
    /// Determines whether specific table used as sequence.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>
    /// <see langword="true"/> if table used as sequence; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsGeneratorTable(Table table)
    {
      int columnNumber = ProviderInfo.Supports(ProviderFeatures.InsertDefaultValues) ? 1 : 2;
      return table.TableColumns.Count==columnNumber &&
        table.TableColumns[0].SequenceDescriptor!=null;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storageSchema">The schema.</param>
    /// <param name="providerInfo">The provider info.</param>
    public SqlModelConverter(Schema storageSchema, ProviderInfo providerInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(storageSchema, "storageSchema");
      ArgumentValidator.EnsureArgumentNotNull(providerInfo, "providerInfo");
      
      Schema = storageSchema;
      ProviderInfo = providerInfo;
    }


    #region Not supported

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitUniqueConstraint(UniqueConstraint constraint)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitIndexColumn(IndexColumn indexColumn)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitCatalog(Catalog catalog)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitCharacterSet(CharacterSet characterSet)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitCollation(Collation collation)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitDataTable(DataTable dataTable)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitDataTableColumn(DataTableColumn dataTableColumn)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitDomain(Xtensive.Sql.Model.Domain domain)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitHashPartition(HashPartition hashPartition)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitListPartition(ListPartition listPartition)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitPartition(Partition partition)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitPartitionDescriptor(PartitionDescriptor partitionDescriptor)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitPartitionFunction(PartitionFunction partitionFunction)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitPartitionSchema(PartitionSchema partitionSchema)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitTableConstraint(TableConstraint constraint)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitDomainConstraint(DomainConstraint constraint)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitConstraint(Constraint constraint)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitCheckConstraint(CheckConstraint constraint)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitRangePartition(RangePartition rangePartition)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitSequenceDescriptor(SequenceDescriptor sequenceDescriptor)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitTemporaryTable(TemporaryTable temporaryTable)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitTranslation(Translation translation)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitView(View view)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitViewColumn(ViewColumn viewColumn)
    {
      throw new NotSupportedException();
    }

    #endregion
  }
}