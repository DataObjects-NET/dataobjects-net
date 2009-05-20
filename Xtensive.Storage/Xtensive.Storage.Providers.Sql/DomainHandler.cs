// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Building;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Mappings;
using Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.PreCompilation;
using Xtensive.Storage.Rse.PreCompilation.Correction;
using Xtensive.Storage.Rse.PreCompilation.Optimization;
using Xtensive.Storage.Rse.Providers;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using SqlModel = Xtensive.Sql.Dom.Database.Model;

namespace Xtensive.Storage.Providers.Sql
{
  public abstract class DomainHandler : Providers.DomainHandler
  {
    private ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor> accessorCache;

    /// <summary>
    /// Gets the storage schema.
    /// </summary>
    public Schema Schema { get; private set; }

    /// <summary>
    /// Gets the model mapping.
    /// </summary>
    public ModelMapping Mapping { get; private set; }

    /// <summary>
    /// Gets the SQL request builder.
    /// </summary>
    public SqlRequestBuilder RequestBuilder { get; private set; }

    /// <summary>
    /// Gets the value type mapper.
    /// </summary>
    public SqlValueTypeMapper ValueTypeMapper { get; private set; }

    /// <summary>
    /// Gets the SQL request cache.
    /// </summary>
    public ThreadSafeDictionary<SqlRequestBuilderTask, SqlUpdateRequest> RequestCache { get; private set; }

    /// <summary>
    /// Gets the connection provider.
    /// </summary>
    internal SqlConnectionProvider ConnectionProvider { get; private set; }

    /// <summary>
    /// Gets the SQL driver.
    /// </summary>
    public SqlDriver Driver { get; private set; }

    protected override IEnumerable<Type> GetCompilerProviderContainerTypes()
    {
      return new[] {
        typeof (NullableMappings),
        typeof (StringMappings),
        typeof (DateTimeMappings),
        typeof (TimeSpanMappings),
        typeof (MathMappings),
        typeof (NumericMappings),
        typeof (DecimalMappings)
      };
    }

    /// <inheritdoc/>
    protected override ICompiler CreateCompiler(BindingCollection<object, ExecutableProvider> compiledSources)
    {
      return new SqlCompiler(Handlers, compiledSources);
    }

    /// <inheritdoc/>
    protected override IPreCompiler CreatePreCompiler()
    {
      return new CompositePreCompiler(
        new ApplyProviderCorrector(true),
        new SkipTakeCorrector(),
        new OrderingCorrector(ResolveOrderingDescriptor, false),
        new RedundantColumnOptimizer(),
        new OrderingCorrector(ResolveOrderingDescriptor, true)
        );
    }

    /// <inheritdoc/>
    protected override IPostCompiler CreatePostCompiler()
    {
      return new SqlOrderbyCorrector(Handlers);
    }

    /// <summary>
    /// Creates (or retrieves from cache) <see cref="DbDataReaderAccessor"/> 
    /// for the specified <see cref="TupleDescriptor"/>.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>A <see cref="DbDataReaderAccessor"/> 
    /// for the specified <see cref="TupleDescriptor"/></returns>
    public DbDataReaderAccessor GetDataReaderAccessor(TupleDescriptor descriptor)
    {
      return accessorCache.GetValue(descriptor, BuildDataReaderAccessor);
    }

    private static ProviderOrderingDescriptor ResolveOrderingDescriptor(CompilableProvider provider)
    {
      bool isOrderSensitive = provider.Type==ProviderType.Skip || provider.Type==ProviderType.Take
        || provider.Type==ProviderType.Seek || provider.Type==ProviderType.Range
        || provider.Type == ProviderType.RowNumber;
      bool preservesOrder = provider.Type==ProviderType.Take
        || provider.Type==ProviderType.Seek || provider.Type==ProviderType.Range
        || provider.Type == ProviderType.RowNumber || provider.Type == ProviderType.Reindex
        || provider.Type == ProviderType.Sort || provider.Type == ProviderType.Range
        || provider.Type == ProviderType.Seek;
      bool isOrderBreaker = provider.Type==ProviderType.Except
        || provider.Type==ProviderType.Intersect || provider.Type==ProviderType.Union
        || provider.Type==ProviderType.Concat || provider.Type==ProviderType.Existence
        || provider.Type==ProviderType.Distinct;
      bool isSorter = provider.Type==ProviderType.Sort || provider.Type==ProviderType.Reindex;
      return new ProviderOrderingDescriptor(isOrderSensitive, preservesOrder, isOrderBreaker,
        isSorter);
    }
    
    /// <summary>
    /// Builds <see cref="DbDataReaderAccessor"/> from specified <see cref="TupleDescriptor"/>.
    /// You should not use this method directly since it does not provide caching.
    /// Use <see cref="GetDataReaderAccessor"/>.
    /// </summary>
    /// <param name="descriptor">The tuple descriptor.</param>
    /// <returns></returns>
    protected virtual DbDataReaderAccessor BuildDataReaderAccessor(TupleDescriptor descriptor)
    {
      var readers = new List<Func<DbDataReader, int, object>>(descriptor.Count);
      var converters = new List<Func<object, object>>(descriptor.Count);

      foreach (var item in descriptor) {
        var typeMapping = ValueTypeMapper.GetTypeMapping(item);
        readers.Add(typeMapping.DataReaderAccessor);
        converters.Add(typeMapping.FromSqlValue);
      }

      return new DbDataReaderAccessor(readers, converters);
    }

    /// <inheritdoc/>
    /// <exception cref="DomainBuilderException">Somethig went wrong.</exception>
    public override void BuildMapping()
    {
      var sessionHandler = ((SessionHandler) BuildingScope.Context.SystemSessionHandler);
      var modelProvider = new SqlModelProvider(sessionHandler.Connection, sessionHandler.Transaction);
      var storageModel = SqlModel.Build(modelProvider);
      Schema = storageModel.DefaultServer.DefaultCatalog.DefaultSchema;
      var domainModel = Handlers.Domain.Model;

      foreach (var type in domainModel.Types) {
        var primaryIndex = type.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
        if (primaryIndex==null || Mapping[primaryIndex]!=null)
          continue;
        var storageTableName = primaryIndex.ReflectedType.MappingName; // Domain.NameBuilder.BuildTableName(primaryIndex);
        var storageTable = Schema.Tables[storageTableName];
        if (storageTable==null)
          throw new DomainBuilderException(string.Format("Can not find table '{0}' in storage.", storageTableName));
        var mapping = Mapping.RegisterMapping(primaryIndex, storageTable);
        foreach (var column in primaryIndex.Columns) {
          var storageColumnName = Domain.NameBuilder.BuildTableColumnName(column);
          var storageColumn = FindColumnByName(storageTable, storageColumnName);
          if (storageColumn==null)
            throw new DomainBuilderException(
              string.Format("Can not find column '{0}' in table '{1}'.", storageColumnName, storageTableName));
          mapping.RegisterMapping(
            column,
            storageColumn,
            ValueTypeMapper.GetTypeMapping(column));
        }
        foreach (var secondaryIndex in type.Indexes.Find(IndexAttributes.Real).Where(i => !i.IsPrimary)) {
          var storageIndexName = secondaryIndex.MappingName;
          var storageIndex = FindIndex(storageTable, storageIndexName);
          if (storageIndex==null)
            throw new DomainBuilderException(
              string.Format("Can not find index '{0}' in table '{1}'.", storageIndexName, storageTableName));
          mapping.RegisterMapping(secondaryIndex, storageIndex);
        }
      }
    }

    private static TableColumn FindColumnByName(Table referencingTable, string columnName)
    {
      return referencingTable.TableColumns.FirstOrDefault(dataTableColumn => dataTableColumn.Name==columnName);
    }

    private static Index FindIndex(Table referencingTable, string indexName)
    {
      return referencingTable.Indexes.FirstOrDefault(i => i.Name==indexName);
    }
    
    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      accessorCache = ThreadSafeDictionary<TupleDescriptor, DbDataReaderAccessor>.Create(new object());
      ConnectionProvider = new SqlConnectionProvider();
      Mapping = new ModelMapping();
      RequestCache = ThreadSafeDictionary<SqlRequestBuilderTask, SqlUpdateRequest>.Create(new object());
      RequestBuilder = Handlers.HandlerFactory.CreateHandler<SqlRequestBuilder>();
      RequestBuilder.Initialize();
    }

    /// <inheritdoc/>
    public override void InitializeFirstSession()
    {
      base.InitializeFirstSession();

      Driver = ((SessionHandler) BuildingContext.Current.SystemSessionHandler).Connection.Driver;
      ValueTypeMapper = Handlers.HandlerFactory.CreateHandler<SqlValueTypeMapper>();
      ValueTypeMapper.Initialize();
    }

    #region Obsolete

    internal static string GetPrimaryIndexColumnName(IndexInfo primaryIndex, ColumnInfo secondaryIndexColumn, IndexInfo secondaryIndex)
    {
      string primaryIndexColumnName = null;
      foreach (ColumnInfo primaryColumn in primaryIndex.Columns)
      {
        if (primaryColumn.Field.Equals(secondaryIndexColumn.Field))
        {
          primaryIndexColumnName = primaryColumn.Name;
          break;
        }
      }
      if (primaryIndexColumnName.IsNullOrEmpty())
        throw new InvalidOperationException(String.Format(
          Strings.ExUnableToFindColumnInPrimaryIndex,
          secondaryIndexColumn.Name,
          secondaryIndex.Name));
      return primaryIndexColumnName;
    }
    
    protected virtual void BuildNewSchema()
    {
      DomainModel domainModel = Handlers.Domain.Model;
      var tables = new Dictionary<IndexInfo, Table>();
      foreach (TypeInfo type in domainModel.Types) {
        IndexInfo primaryIndex = type.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
        if (primaryIndex==null || Mapping[primaryIndex]!=null)
          continue;
        Table table = Schema.CreateTable(Domain.NameBuilder.BuildTableName(primaryIndex));
        tables.Add(primaryIndex, table);
        PrimaryIndexMapping pim = Mapping.RegisterMapping(primaryIndex, table);

        CreateColumns(primaryIndex, table, pim);
        CreateSecondaryIndexes(type, primaryIndex, table, pim);
      }
      if ((Handlers.Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Reference) > 0)
        BuildForeignKeys(domainModel.Associations, tables);
      if ((Handlers.Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Hierarchy) > 0)
        BuildHierarchyReferences(domainModel.Types.Entities, tables);
    }

    protected virtual ISqlCompileUnit GenerateSyncCatalogScript(DomainModel domainModel, Schema existingSchema, Schema newSchema)
    {
      SqlBatch batch = SqlFactory.Batch();
      batch.Add(GenerateClearCatalogScript(existingSchema));
      batch.Add(GenerateBuildCatalogScript(newSchema));
      return batch;
    }

    private void BuildForeignKeys(IEnumerable<AssociationInfo> associations, Dictionary<IndexInfo, Table> tables)
    {
      foreach (AssociationInfo association in associations.Where(associationInfo => associationInfo.IsMaster)) {
        Table referencingTable;
        Table referencedTable;
        IList<ColumnInfo> referencingColumns;
        ICollection<ColumnInfo> referencedColumns;
        string foreignKeyName;
        if (association.UnderlyingType==null) {
          // TODO: Remove comparison with null than Structure association bug fixed.
          if (association.ReferencingType.Indexes.PrimaryIndex==null)
            continue;
          IndexInfo referencingIndex = FindRealIndex(association.ReferencingType.Indexes.PrimaryIndex, association.ReferencingField);
          referencingTable = tables[referencingIndex];
          referencingColumns = association.ReferencingField.ExtractColumns();
          IndexInfo referencedIndex = FindRealIndex(association.ReferencedType.Indexes.PrimaryIndex, null);
          referencedTable = tables[referencedIndex];
          referencedColumns = association.ReferencedType.Indexes.PrimaryIndex.KeyColumns.Keys;
          foreignKeyName = Domain.NameBuilder.BuildForeignKeyName(association, association.ReferencingField);
          CreateForeignKey(foreignKeyName, referencingTable, referencingColumns, referencedTable, referencedColumns);
        }
        else {
          referencingTable = tables[association.UnderlyingType.Indexes.PrimaryIndex];
          foreach (FieldInfo referencingField in association.UnderlyingType.Fields.Where(fieldInfo => fieldInfo.IsEntity)) {
            // Build master reference
            IndexInfo referencedIndex = FindRealIndex(Domain.Model.Types[referencingField.ValueType].Indexes.PrimaryIndex, null);
            referencedTable = tables[referencedIndex];
            referencedColumns = referencedIndex.KeyColumns.Keys;
            referencingColumns = referencingField.ExtractColumns();
            foreignKeyName = Domain.NameBuilder.BuildForeignKeyName(association, referencingField);
            CreateForeignKey(foreignKeyName, referencingTable, referencingColumns, referencedTable, referencedColumns);
          }
        }
      }
    }

    private void BuildHierarchyReferences(IEnumerable<TypeInfo> entities, IDictionary<IndexInfo, Table> tables)
    {
      var indexPairs = new Dictionary<Pair<IndexInfo>, object>();
      foreach (TypeInfo type in entities) {
        if (type.Indexes.PrimaryIndex.IsVirtual) {
          ReadOnlyList<IndexInfo> realPrimaryIndexes = type.Indexes.RealPrimaryIndexes;
          for (int i = 0; i < realPrimaryIndexes.Count - 1; i++) {
            if (realPrimaryIndexes[i]!=realPrimaryIndexes[i + 1]) {
              var pair = new Pair<IndexInfo>(realPrimaryIndexes[i], realPrimaryIndexes[i + 1]);
              indexPairs[pair] = null;
            }
          }
        }
      }
      foreach (Pair<IndexInfo> indexPair in indexPairs.Keys) {
        var referencingIndex = indexPair.First;
        IEnumerable<ColumnInfo> referencingColumns = referencingIndex.KeyColumns.Keys;
        var referencedIndex = indexPair.Second;
        IEnumerable<ColumnInfo> referencedColumns = referencedIndex.KeyColumns.Keys;
        string foreignKeyName = Domain.NameBuilder.BuildForeignKeyName(referencingIndex.ReflectedType, referencedIndex.ReflectedType);
        CreateForeignKey(foreignKeyName, tables[referencedIndex], referencedColumns, tables[referencingIndex], referencingColumns);
      }
    }

    private void CreateForeignKey(string foreignKeyName, Table referencingTable, IEnumerable<ColumnInfo> referencingColumns, Table referencedTable, IEnumerable<ColumnInfo> referencedColumns)
    {
      var foreignKey = referencingTable.CreateForeignKey(foreignKeyName);
      foreach (ColumnInfo column in referencingColumns) {
        var columnName = Domain.NameBuilder.BuildTableColumnName(column);
        var tableColumn = FindColumnByName(referencingTable, columnName);
        foreignKey.Columns.Add(tableColumn);
      }
      foreignKey.ReferencedTable = referencedTable;
      foreach (ColumnInfo keyColumn in referencedColumns) {
        var columnName = Domain.NameBuilder.BuildTableColumnName(keyColumn);
        var tableColumn = FindColumnByName(referencedTable, columnName);
        foreignKey.ReferencedColumns.Add(tableColumn);
      }
    }

    private IndexInfo FindRealIndex(IndexInfo index, FieldInfo field)
    {
      if (index.IsVirtual) {
        foreach (var underlyingIndex in index.UnderlyingIndexes) {
          var result = FindRealIndex(underlyingIndex, field);
          if (result!=null)
            return result;
        }
      }
      else {
        if (field==null || index.Columns.ContainsAny(field.ExtractColumns()))
          return index;
      }
      return null;
    }

    private void CreateSecondaryIndexes(TypeInfo type, IndexInfo primaryIndex, Table table, PrimaryIndexMapping pim)
    {
      foreach (IndexInfo indexInfo in type.Indexes.Find(IndexAttributes.Real).Where(ii => !ii.IsPrimary)) {
        Index index = table.CreateIndex(indexInfo.Name);
        pim.RegisterMapping(indexInfo, index);
        index.IsUnique = indexInfo.IsUnique;
        index.FillFactor = (byte) (indexInfo.FillFactor * 100);
        foreach (KeyValuePair<ColumnInfo, Direction> keyColumn in indexInfo.KeyColumns) {
          string primaryIndexColumnName = GetPrimaryIndexColumnName(primaryIndex, keyColumn.Key, indexInfo);
          index.CreateIndexColumn(table.TableColumns.First(tableColumn => tableColumn.Name==primaryIndexColumnName), keyColumn.Value==Direction.Positive);
        }
        foreach (var nonKeyColumn in indexInfo.IncludedColumns) {
          string primaryIndexColumnName = GetPrimaryIndexColumnName(primaryIndex, nonKeyColumn, indexInfo);
          index.NonkeyColumns.Add(table.TableColumns.First(tableColumn => tableColumn.Name==primaryIndexColumnName));
        }
      }
    }

    private void CreateColumns(IndexInfo primaryIndex, Table table, PrimaryIndexMapping pim)
    {
      var keyColumns = new List<TableColumn>();
      foreach (ColumnInfo columnInfo in primaryIndex.Columns) {
        TableColumn column = table.CreateColumn(Domain.NameBuilder.BuildTableColumnName(columnInfo));
        DataTypeMapping typeMapping = ValueTypeMapper.GetTypeMapping(columnInfo);
        pim.RegisterMapping(columnInfo, column, typeMapping);
        column.DataType = ValueTypeMapper.BuildSqlValueType(columnInfo);
        column.IsNullable = columnInfo.IsNullable;
        if (columnInfo.IsPrimaryKey)
          keyColumns.Add(column);
      }
      if (keyColumns.Count > 0)
        table.CreatePrimaryKey(primaryIndex.Name, keyColumns.ToArray());
    }

    private static SqlBatch GenerateClearCatalogScript(Schema schema)
    {
      SqlBatch batch = SqlFactory.Batch();
      foreach (View view in schema.Views)
        batch.Add(SqlFactory.Drop(view));
      foreach (Table table in schema.Tables) {
        foreach (TableConstraint tableConstraint in table.TableConstraints) {
          var fk = tableConstraint as ForeignKey;
          if (fk!=null)
            batch.Add(SqlFactory.Alter(table, SqlFactory.DropConstraint(tableConstraint)));
        }
      }
      foreach (Table table in schema.Tables)
        batch.Add(SqlFactory.Drop(table));
      foreach (Sequence sequence in schema.Sequences)
        batch.Add(SqlFactory.Drop(sequence));
      return batch;
    }

    private static SqlBatch GenerateBuildCatalogScript(Schema schema)
    {
      SqlBatch batch = SqlFactory.Batch();
      var constraints = new List<Pair<Table, List<TableConstraint>>>();
      foreach (Table table in schema.Tables) {
        var tableConstraints = new List<TableConstraint>(table.TableConstraints.Where(tableConstraint => tableConstraint is ForeignKey));
        constraints.Add(new Pair<Table, List<TableConstraint>>(table, tableConstraints));
        foreach (TableConstraint foreignKeyConstraint in tableConstraints)
          table.TableConstraints.Remove(foreignKeyConstraint);
        batch.Add(SqlFactory.Create(table));
        foreach (Index index in table.Indexes)
          batch.Add(SqlFactory.Create(index));
      }
      foreach (Pair<Table, List<TableConstraint>> constraint in constraints)
        foreach (TableConstraint tableConstraint in constraint.Second)
          batch.Add(SqlFactory.Alter(constraint.First, SqlFactory.AddConstraint(tableConstraint)));
      foreach (Sequence sequence in schema.Sequences)
        batch.Add(SqlFactory.Create(sequence));
      return batch;
    }

    #endregion
  }
}