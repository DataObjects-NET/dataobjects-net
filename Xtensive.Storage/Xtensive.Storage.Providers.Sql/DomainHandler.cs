// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Threading;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Building;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Mappings;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Compilation;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using SqlModel = Xtensive.Sql.Dom.Database.Model;

namespace Xtensive.Storage.Providers.Sql
{
  public abstract class DomainHandler : Providers.DomainHandler
  {
    private Schema existingSchema;

    public DomainModelMapping MappingSchema { get; private set; }

    public Schema Schema { get; private set; }

    public SqlRequestBuilder SqlRequestBuilder { get; private set; }

    public SqlValueTypeMapper ValueTypeMapper { get; private set; }

    public ThreadSafeDictionary<SqlRequestBuilderTask, SqlUpdateRequest> SqlRequestCache { get; private set; }

    internal SqlConnectionProvider ConnectionProvider { get; private set; }

    public SqlDriver SqlDriver { get; private set; }

    protected override ICompiler BuildCompiler()
    {
      return new SqlCompiler(Handlers);
    }

    /// <inheritdoc/>
    public void Compile(SqlRequest request)
    {
      request.Compile(this);
    }

    /// <inheritdoc/>
    public override void BuildRecreate()
    {
      var sessionHandler = ((SessionHandler) BuildingScope.Context.SystemSessionHandler);
      var modelProvider = new SqlModelProvider(sessionHandler.Connection, sessionHandler.Transaction);
      ISqlCompileUnit syncScript = GenerateSyncCatalogScript(Handlers.Domain.Model, existingSchema, Schema);
      sessionHandler.ExecuteNonQuery(syncScript);
      string catalogName = Handlers.Domain.Configuration.ConnectionInfo.Resource;
      Schema = SqlModel.Build(modelProvider).DefaultServer.Catalogs[catalogName].DefaultSchema;
    }

    public override void BuildRecycling()
    {
      throw new NotImplementedException();
    }

    public override StorageConformity CheckStorageConformity()
    {
      // TODO: Implement
      return StorageConformity.Match;
      throw new NotImplementedException();
    }

    public override void DeleteRecycledData()
    {
      throw new NotImplementedException();
    }

    public override void BuildPerform()
    {
      // todo: Implement
      BuildRecreate();
//      throw new System.NotImplementedException();
    }

    internal static string GetPrimaryIndexColumnName(IndexInfo primaryIndex, ColumnInfo secondaryIndexColumn, IndexInfo secondaryIndex)
    {
      string primaryIndexColumnName = null;
      foreach (ColumnInfo primaryColumn in primaryIndex.Columns) {
        if (primaryColumn.Field.Equals(secondaryIndexColumn.Field)) {
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

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      ConnectionProvider = new SqlConnectionProvider();
      MappingSchema = new DomainModelMapping();
      SqlRequestCache = ThreadSafeDictionary<SqlRequestBuilderTask, SqlUpdateRequest>.Create(new object());
      SqlRequestBuilder = Handlers.HandlerFactory.CreateHandler<SqlRequestBuilder>();
      SqlRequestBuilder.Initialize();
    }

    public override void InitializeSystemSession()
    {
      base.InitializeSystemSession();
      SqlDriver = ((SessionHandler)BuildingContext.Current.SystemSessionHandler).Connection.Driver;
      ValueTypeMapper = Handlers.HandlerFactory.CreateHandler<SqlValueTypeMapper>();
      ValueTypeMapper.Initialize();

      var sessionHandler = ((SessionHandler)BuildingScope.Context.SystemSessionHandler);
      var modelProvider = new SqlModelProvider(sessionHandler.Connection, sessionHandler.Transaction);
      SqlModel existingModel = SqlModel.Build(modelProvider);
      string serverName = existingModel.DefaultServer.Name;
      string catalogName = Handlers.Domain.Configuration.ConnectionInfo.Resource;
      existingSchema = existingModel.DefaultServer.Catalogs[catalogName].DefaultSchema;
      string schemaName = existingSchema.Name;

      var sqlModel = new SqlModel();
      Server server = sqlModel.CreateServer(serverName);
      Catalog catalog = server.CreateCatalog(catalogName);
      Schema = catalog.CreateSchema(schemaName);
      BuildNewSchema();
    }

    #region Build related methods

    protected virtual void BuildNewSchema()
    {
      DomainModel domainModel = Handlers.Domain.Model;
      var tables = new Dictionary<IndexInfo, Table>();
      foreach (TypeInfo type in domainModel.Types) {
        IndexInfo primaryIndex = type.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
        if (primaryIndex==null || MappingSchema[primaryIndex]!=null)
          continue;
        Table table = Schema.CreateTable(Domain.NameBuilder.BuildTableName(primaryIndex));
        tables.Add(primaryIndex, table);
        PrimaryIndexMapping pim = MappingSchema.RegisterMapping(primaryIndex, table);

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

    #endregion

    #region Private / internal methods

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
          foreignKeyName = Domain.NameBuilder.BuildForeignKeyName(association);
        }
        else {
          referencingTable = tables[association.UnderlyingType.Indexes.PrimaryIndex];
          IndexInfo referencedIndex = FindRealIndex(association.ReferencingType.Indexes.PrimaryIndex, null);
          referencedTable = tables[referencedIndex];
          referencedColumns = referencedIndex.KeyColumns.Keys;
          FieldInfo referencingField = association.UnderlyingType.Fields.First(fieldInfo => fieldInfo.IsEntity && fieldInfo.ValueType==association.ReferencingType.UnderlyingType);
          referencingColumns = referencingField.ExtractColumns();
          foreignKeyName = Domain.NameBuilder.BuildForeignKeyName(association);
        }
        CreateForeignKey(foreignKeyName, referencingTable, referencingColumns, referencedTable, referencedColumns);
      }
    }

    private void BuildHierarchyReferences(ICountable<TypeInfo> entities, Dictionary<IndexInfo, Table> tables)
    {
      var indexPairs = new Dictionary<Pair<IndexInfo>, object>();
      foreach (TypeInfo type in entities) {
        if (type.Indexes.PrimaryIndex.IsVirtual) {
          List<IndexInfo> realPrimaryIndexes = GetRealPrimaryIndexes(type.Indexes.PrimaryIndex);
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

    private TableColumn FindColumnByName(Table referencingTable, string columnName)
    {
      return referencingTable.TableColumns.First(dataTableColumn => dataTableColumn.Name==columnName);
    }

    private List<IndexInfo> GetRealPrimaryIndexes(IndexInfo index)
    {
      var result = new List<IndexInfo>();
      foreach (IndexInfo underlyingIndex in index.UnderlyingIndexes) {
        if (underlyingIndex.IsPrimary && !underlyingIndex.IsVirtual)
          result.Add(underlyingIndex);
        else
          result.AddRange(GetRealPrimaryIndexes(underlyingIndex));
      }
      return result;
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