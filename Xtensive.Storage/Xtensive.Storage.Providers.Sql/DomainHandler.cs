// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Providers.Sql.Resources;
using ColumnInfo = Xtensive.Storage.Model.ColumnInfo;
using IndexInfo  = Xtensive.Storage.Model.IndexInfo;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using SqlModel = Xtensive.Sql.Dom.Database.Model;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Providers.Sql
{
  public abstract class DomainHandler : Providers.DomainHandler
  {
    private Schema schema;
    private readonly Dictionary<IndexInfo, Table> realIndexes = new Dictionary<IndexInfo, Table>();
    private SqlConnectionProvider connectionProvider;

    public Schema Schema
    {
      get { return schema; }
    }

    internal SqlConnectionProvider ConnectionProvider
    {
      get { return connectionProvider; }
    }

    /// <inheritdoc/>
    protected override CompilationContext BuildCompilationContext()
    {
      return new CompilationContext(new Compilers.Compiler(Handlers));
    }

    /// <inheritdoc/>
    public override void Build()
    {
      SessionHandler sessionHandler = ((SessionHandler)BuildingScope.Context.SystemSessionHandler);      
      var modelProvider = new SqlModelProvider(sessionHandler.Connection, sessionHandler.Transaction);
      SqlModel existingModel = SqlModel.Build(modelProvider);
      string serverName = existingModel.DefaultServer.Name;
      string catalogName = Handlers.Domain.Configuration.ConnectionInfo.Resource;
      string schemaName = existingModel.DefaultServer.Catalogs[catalogName].DefaultSchema.Name;
      SqlModel newModel = BuildSqlModel(serverName, catalogName, schemaName);
      ISqlCompileUnit syncScript = GenerateSyncCatalogScript(Handlers.Domain.Model, existingModel.DefaultServer.Catalogs[catalogName], newModel.DefaultServer.Catalogs[catalogName]);
      sessionHandler.ExecuteNonQuery(syncScript);
      schema = SqlModel.Build(modelProvider).DefaultServer.Catalogs[catalogName].DefaultSchema;
    }

    public virtual Table GetTable(IndexInfo indexInfo)
    {
      Table table;
      if (!realIndexes.TryGetValue(indexInfo, out table))
        throw new InvalidOperationException(String.Format(Strings.ExTypeHasNoPrimaryIndex, indexInfo.Name));
      return table;
    }

    public abstract SqlDataType GetSqlDataType(Type type, int? length);

    public virtual SqlValueType GetSqlType(Type type, int? length)
    {
      // TODO: Get this data from Connection.Driver.ServerInfo.DataTypes
      var result = (length==null)
        ? new SqlValueType(GetSqlDataType(type, null))
        : new SqlValueType(GetSqlDataType(type, length.Value), length.Value);
      return result;
    }

    public static string GetPrimaryIndexColumnName(IndexInfo primaryIndex, ColumnInfo secondaryIndexColumn, IndexInfo secondaryIndex)
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
      connectionProvider = new SqlConnectionProvider();
    }

    #region Build related methods

    protected virtual SqlModel BuildSqlModel(string serverName, string catalogName, string schemaName)
    {
      var model = new SqlModel();
      Server server = model.CreateServer(serverName);
      Catalog catalog = server.CreateCatalog(catalogName);
      Schema schema = catalog.CreateSchema(schemaName);
      foreach (TypeInfo type in Handlers.Domain.Model.Types) {
        IndexInfo primaryIndex = type.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
        if (primaryIndex!=null && !realIndexes.ContainsKey(primaryIndex)) {
          Table table = schema.CreateTable(primaryIndex.ReflectedType.Name);
          realIndexes.Add(primaryIndex, table);
          var keyColumns = new List<TableColumn>();
          foreach (ColumnInfo column in primaryIndex.Columns) {
            TableColumn tableColumn = table.CreateColumn(column.Name, GetSqlType(column.ValueType, column.Length));
            tableColumn.IsNullable = column.IsNullable;
            if (column.IsPrimaryKey)
              keyColumns.Add(tableColumn);
          }
          table.CreatePrimaryKey(primaryIndex.Name, keyColumns.ToArray());
          // Primary key included columns
          if (primaryIndex.IncludedColumns.Count > 0) {
            Index index = table.CreateIndex(primaryIndex.Name + "_IncludedColumns");
            index.IsUnique = false;
            index.Filegroup = "\"default\"";
            foreach (ColumnInfo includedColumn in primaryIndex.IncludedColumns) {
              ColumnInfo includedColumn1 = includedColumn;
              index.CreateIndexColumn(table.TableColumns.First(tableColumn => tableColumn.Name==includedColumn1.Name));
            }
          }
          // Secondary indexes
          foreach (IndexInfo secondaryIndex in type.Indexes.Find(IndexAttributes.Real).Where(indexInfo => !indexInfo.IsPrimary)) {
            Index index = table.CreateIndex(secondaryIndex.Name);
            index.IsUnique = secondaryIndex.IsUnique;
            index.FillFactor = (byte) (secondaryIndex.FillFactor * 10);
            index.Filegroup = "\"default\"";
            foreach (ColumnInfo secondaryIndexColumn in secondaryIndex.Columns.Where(columnInfo => !columnInfo.IsPrimaryKey && !columnInfo.IsSystem)) {
              string primaryIndexColumnName = GetPrimaryIndexColumnName(primaryIndex, secondaryIndexColumn, secondaryIndex);
              index.CreateIndexColumn(table.TableColumns.First(tableColumn => tableColumn.Name==primaryIndexColumnName));
            }
            foreach (var nonKeyColumn in secondaryIndex.IncludedColumns) {
              string primaryIndexColumnName = GetPrimaryIndexColumnName(primaryIndex, nonKeyColumn, secondaryIndex);
              index.NonkeyColumns.Add(table.TableColumns.First(tableColumn => tableColumn.Name==primaryIndexColumnName));
            }
          }
        }
      }
      return model;
    }

    protected virtual ISqlCompileUnit GenerateSyncCatalogScript(DomainModel domainModel, Catalog existingCatalog, Catalog newCatalog)
    {
      SqlBatch batch = SqlFactory.Batch();
      batch.Add(GenerateClearCatalogScript(existingCatalog));
      batch.Add(GenerateBuildCatalogScript(newCatalog));
      return batch;
    }

    #endregion

    #region Private / internal methods

    private static SqlBatch GenerateClearCatalogScript(Catalog catalog)
    {
      SqlBatch batch = SqlFactory.Batch();
      Schema schema = catalog.DefaultSchema;
      foreach (View view in schema.Views)
        batch.Add(SqlFactory.Drop(view));
      schema.Views.Clear();
      foreach (Table table in schema.Tables)
        batch.Add(SqlFactory.Drop(table));
      schema.Tables.Clear();
      return batch;
    }

    private static SqlBatch GenerateBuildCatalogScript(Catalog catalog)
    {
      SqlBatch batch = SqlFactory.Batch();
      foreach (Table table in catalog.DefaultSchema.Tables) {
        batch.Add(SqlFactory.Create(table));
        foreach (Index index in table.Indexes) {
          batch.Add(SqlFactory.Create(index));
        }
      }
      return batch;
    }

    #endregion
  }
}
