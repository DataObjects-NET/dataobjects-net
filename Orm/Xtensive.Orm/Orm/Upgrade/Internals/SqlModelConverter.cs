// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.31

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;
using Xtensive.Modelling;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Orm.Upgrade.Model;
using Node = Xtensive.Sql.Model.Node;
using ReferentialAction = Xtensive.Orm.Upgrade.Model.ReferentialAction;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Converts <see cref="SqlExtractionResult"/> to <see cref="targetModel"/>.
  /// </summary>
  internal sealed class SqlModelConverter : SqlModelVisitor<IPathNode>
  {
    private readonly SchemaExtractionResult sourceModel;
    private readonly MappingResolver resolver;
    private readonly ProviderInfo providerInfo;
    private readonly PartialIndexInfoMap partialIndexMap;
    private readonly StorageDriver driver;

    private StorageModel targetModel;
    private TableInfo currentTable;

    /// <summary>
    /// Get the result of conversion specified 
    /// <see cref="Schema"/> to <see cref="targetModel"/>.
    /// </summary>
    /// <returns>The storage model.</returns>
    public StorageModel Run()
    {
      if (targetModel==null) {
        targetModel = new StorageModel();
        foreach (var catalog in sourceModel.Catalogs)
          VisitCatalog(catalog);
      }

      return targetModel;
    }

    #region SqlModelVisitor<IPathNode> implementation

    /// <inheritdoc/>
    protected override IPathNode VisitSchema(Schema schema)
    {
      // Build tables, columns and indexes.
      foreach (var table in schema.Tables)
        Visit(table);

      // Build foreign keys.
      var foreignKeys = schema.Tables.SelectMany(t => t.TableConstraints.OfType<ForeignKey>());
      foreach (var foreignKey in foreignKeys)
        Visit(foreignKey);
      foreach (var sequence in schema.Sequences)
        VisitSequence(sequence);

      return null;
    }

    /// <inheritdoc/>
    protected override IPathNode Visit(Node node)
    {
      if (!providerInfo.Supports(ProviderFeatures.Sequences)) {
        var table = node as Table;
        if (table!=null && IsGeneratorTable(table))
          return VisitGeneratorTable(table);
      }

      return base.Visit(node);
    }

    /// <inheritdoc/>
    protected override IPathNode VisitTable(Table table)
    {
      var tableInfo = new TableInfo(targetModel, resolver.GetNodeName(table));

      currentTable = tableInfo;

      foreach (var column in table.TableColumns)
        Visit(column);

      var primaryKey = table.TableConstraints
        .SingleOrDefault(constraint => constraint is PrimaryKey);
      if (primaryKey!=null)
        Visit(primaryKey);

      foreach (var index in table.Indexes)
        Visit(index);

      currentTable = null;

      return tableInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitTableColumn(TableColumn tableColumn)
    {
      var tableInfo = currentTable;
      var typeInfo = ExtractType(tableColumn);
      var columnInfo = new StorageColumnInfo(tableInfo, tableColumn.Name, typeInfo);
      return columnInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitForeignKey(ForeignKey key)
    {
      var referencingTable = targetModel.Tables[resolver.GetNodeName(key.Owner)];
      var referencingColumns = new List<StorageColumnInfo>();

      foreach (var refColumn in key.Columns)
        referencingColumns.Add(referencingTable.Columns[refColumn.Name]);

      var foreignKeyInfo = new ForeignKeyInfo(referencingTable, key.Name) {
        OnUpdateAction = ConvertReferentialAction(key.OnUpdate),
        OnRemoveAction = ConvertReferentialAction(key.OnDelete)
      };

      var referencedTable = targetModel.Tables[resolver.GetNodeName(key.ReferencedTable)];
      foreignKeyInfo.PrimaryKey = referencedTable.PrimaryIndex;

      foreach (var column in referencingColumns)
        new ForeignKeyColumnRef(foreignKeyInfo, column);

      return foreignKeyInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitPrimaryKey(PrimaryKey key)
    {
      var tableInfo = currentTable;
      var primaryIndexInfo = new PrimaryIndexInfo(tableInfo, key.Name) {IsClustered = key.IsClustered};

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
      var tableInfo = currentTable;
      var name = index.Name.IsNullOrEmpty()
        ? string.Format("FT_{0}", index.DataTable.Name)
        : index.Name;
      var ftIndex = new StorageFullTextIndexInfo(tableInfo, name) {
        FullTextCatalog = index.FullTextCatalog,
        ChangeTrackingMode = ConvertChangeTrackingMode(index.ChangeTrackingMode)
      };
      foreach (var column in index.Columns) {
        var columnInfo = tableInfo.Columns[column.Column.Name];
        string typeColumn = null;
        if (column.TypeColumn!=null)
          typeColumn = tableInfo.Columns[column.TypeColumn.Name].Name;
        new FullTextColumnRef(ftIndex, columnInfo, column.Languages.Single().Name, typeColumn);
      }
      return ftIndex;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitIndex(Index index)
    {
      var tableInfo = currentTable;
      var secondaryIndexInfo = new SecondaryIndexInfo(tableInfo, index.Name) {
        IsUnique = index.IsUnique,
        IsClustered = index.IsClustered,
        Filter = GetFilter(index),
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

    private PartialIndexFilterInfo GetFilter(Index index)
    {
      var tableName = resolver.GetNodeName(index.DataTable);
      var result = partialIndexMap.FindIndex(tableName, index.DbName);
      if (result==null)
        return null;
      return new PartialIndexFilterInfo(result.Filter);
    }

    /// <inheritdoc/>
    protected override IPathNode VisitSequence(Sequence sequence)
    {
      var sequenceInfo = new StorageSequenceInfo(targetModel, resolver.GetNodeName(sequence)) {
        Increment = sequence.SequenceDescriptor.Increment.Value,
        // StartValue = sequence.SequenceDescriptor.StartValue.Value,
      };
      return sequenceInfo;
    }

    protected override IPathNode VisitCatalog(Catalog catalog)
    {
      foreach (var schema in catalog.Schemas)
        VisitSchema(schema);
      return null;
    }

    #endregion

    /// <summary>
    /// Visits the generator table.
    /// </summary>
    /// <param name="generatorTable">The generator table.</param>
    /// <returns>Visit result.</returns>
    private IPathNode VisitGeneratorTable(Table generatorTable)
    {
      var idColumn = generatorTable.TableColumns[0];
      var startValue = idColumn.SequenceDescriptor.StartValue;
      var increment = idColumn.SequenceDescriptor.Increment;
      var type = ExtractType(idColumn);
      var sequence =
        new StorageSequenceInfo(targetModel, resolver.GetNodeName(generatorTable)) {
          Seed = startValue ?? 0,
          Increment = increment ?? 1,
          Type = type,
        };

      return sequence;
    }

    /// <summary>
    /// Extracts the <see cref="StorageTypeInfo"/> from <see cref="TableColumn"/>.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>Data type.</returns>
    private StorageTypeInfo ExtractType(TableColumn column)
    {
      var sqlValueType = column.DataType;
      Type type;
      try {
        type = driver.MapSqlType(sqlValueType.Type);
      }
      catch(ArgumentException) {
        return StorageTypeInfo.Undefined;
      }

      if (column.IsNullable) {
        type = type.ToNullable();
      }

      return new StorageTypeInfo(type, sqlValueType, column.IsNullable, sqlValueType.Length, sqlValueType.Precision, sqlValueType.Scale);
    }

    /// <summary>
    /// Converts the <see cref="Xtensive.Sql.ReferentialAction"/> to 
    /// <see cref="Xtensive.Orm.Upgrade.Model.ReferentialAction"/>.
    /// </summary>
    /// <param name="toConvert">The action to convert.</param>
    /// <returns>Converted action.</returns>
    private ReferentialAction ConvertReferentialAction(Sql.ReferentialAction toConvert)
    {
      switch (toConvert) {
      case Sql.ReferentialAction.NoAction:
        return ReferentialAction.None;
      case Sql.ReferentialAction.Restrict:
        return ReferentialAction.Restrict;
      case Sql.ReferentialAction.Cascade:
        return ReferentialAction.Cascade;
      case Sql.ReferentialAction.SetNull:
        return ReferentialAction.Clear;
      case Sql.ReferentialAction.SetDefault:
        return ReferentialAction.Default;
      default:
        return ReferentialAction.Default;
      }
    }

    /// <summary>
    /// Converts the <see cref="Xtensive.Sql.Model.ChangeTrackingMode"/> to <see cref="Xtensive.Orm.FullTextChangeTrackingMode"/>.
    /// </summary>
    /// <param name="toConvert">The mode to convert.</param>
    /// <returns>Converted mode.</returns>
    private FullTextChangeTrackingMode ConvertChangeTrackingMode(ChangeTrackingMode toConvert)
    {
      switch (toConvert) {
      case ChangeTrackingMode.Auto:
        return FullTextChangeTrackingMode.Auto;
      case ChangeTrackingMode.Manual:
        return FullTextChangeTrackingMode.Manual;
      case ChangeTrackingMode.Off:
        return FullTextChangeTrackingMode.Off;
      case ChangeTrackingMode.OffWithNoPopulation:
        return FullTextChangeTrackingMode.OffWithNoPopulation;
      default:
        return FullTextChangeTrackingMode.Default;
      }
    }

    /// <summary>
    /// Determines whether specific table used as sequence.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>
    /// <see langword="true"/> if table used as sequence; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private bool IsGeneratorTable(Table table)
    {
      int columnNumber = providerInfo.Supports(ProviderFeatures.InsertDefaultValues) ? 1 : 2;
      return table.TableColumns.Count==columnNumber &&
        table.TableColumns[0].SequenceDescriptor!=null;
    }


    // Constructors

    public SqlModelConverter(UpgradeServiceAccessor services, SchemaExtractionResult sourceModel,
      IEnumerable<StoredPartialIndexFilterInfo> partialIndexes)
    {
      ArgumentValidator.EnsureArgumentNotNull(services, "handlers");
      ArgumentValidator.EnsureArgumentNotNull(sourceModel, "sourceModel");
      ArgumentValidator.EnsureArgumentNotNull(partialIndexes, "partialIndexes");

      this.sourceModel = sourceModel;

      resolver = services.MappingResolver;
      providerInfo = services.ProviderInfo;
      driver = services.StorageDriver;

      partialIndexMap = new PartialIndexInfoMap(resolver, partialIndexes);
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
      // http://support.x-tensive.com/question/3643/oracle-database-domainbuild-throws-notsupportedexception
      return null;
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