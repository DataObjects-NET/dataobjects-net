// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.08.11

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using DataTable = Xtensive.Sql.Model.DataTable;
using Domain = Xtensive.Sql.Model.Domain;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Drivers.SqlServer.v09
{
  internal class Extractor : Model.Extractor
  {
    private readonly Dictionary<int, Schema> schemaIndex = new Dictionary<int, Schema>();
    private readonly Dictionary<Schema, int> reversedSchemaIndex = new Dictionary<Schema, int>();
    private readonly Dictionary<int, Domain> domainIndex = new Dictionary<int, Domain>();
    private readonly Dictionary<int, string> typeNameIndex = new Dictionary<int, string>();
    private readonly Dictionary<int, ColumnResolver> columnResolverIndex = new Dictionary<int, ColumnResolver>();
    private readonly Dictionary<string, string> replacementsRegistry = new Dictionary<string, string>();

    protected const string SysTablesFilterPlaceholder = "{SYSTABLE_FILTER}";
    protected const string CatalogPlaceholder = "{CATALOG}";
    protected const string SchemaFilterPlaceholder = "{SCHEMA_FILTER}";

    private Catalog catalog;
    private readonly HashSet<string> targetSchemes = new HashSet<string>();

    public override Catalog ExtractCatalog(string catalogName) =>
      ExtractSchemes(catalogName, Array.Empty<string>());

    public override Task<Catalog> ExtractCatalogAsync(string catalogName, CancellationToken token = default) =>
      ExtractSchemesAsync(catalogName, Array.Empty<string>(), token);

    public override Catalog ExtractSchemes(string catalogName, string[] schemaNames)
    {
      CreateCatalog(catalogName, schemaNames);
      ExtractCatalogContents();
      return catalog;
    }

    public override async Task<Catalog> ExtractSchemesAsync(
      string catalogName, string[] schemaNames, CancellationToken token = default)
    {
      CreateCatalog(catalogName, schemaNames);
      await ExtractCatalogContentsAsync(token);
      return catalog;
    }

    private void CreateCatalog(string catalogName, string[] schemaNames)
    {
      catalog = new Catalog(catalogName);
      targetSchemes.UnionWith(schemaNames);
      foreach (var schemaName in schemaNames) {
        catalog.CreateSchema(schemaName);
      }
    }

    protected virtual void ExtractCatalogContents()
    {
      ExtractSchemas();
      ExtractTypes();
      ExtractTablesAndViews();
      ExtractColumns();
      ExtractIndexes();
      ExtractForeignKeys();
      ExtractFulltextIndexes();
    }

    protected virtual async Task ExtractCatalogContentsAsync(CancellationToken token)
    {
      await ExtractSchemasAsync(token).ConfigureAwait(false);
      await ExtractTypesAsync(token).ConfigureAwait(false);
      await ExtractTablesAndViewsAsync(token).ConfigureAwait(false);
      await ExtractColumnsAsync(token).ConfigureAwait(false);
      await ExtractIndexesAsync(token).ConfigureAwait(false);
      await ExtractForeignKeysAsync(token).ConfigureAwait(false);
      await ExtractFulltextIndexesAsync(token).ConfigureAwait(false);
    }

    // All schemas
    private void ExtractSchemas()
    {
      var query = BuildExtractSchemasQuery();

      using (var cmd = Connection.CreateCommand(query))
      using (var reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          ReadSchemaData(reader);
        }
      }

      RegisterReplacements(replacementsRegistry);
    }

    private async Task ExtractSchemasAsync(CancellationToken token)
    {
      var query = BuildExtractSchemasQuery();

      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false))
      await using (var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadSchemaData(reader);
        }
      }

      RegisterReplacements(replacementsRegistry);
    }

    private string BuildExtractSchemasQuery()
    {
      var query = @"
  SELECT
    s.schema_id,
    s.name,
    dp.name
  FROM {CATALOG}.sys.schemas AS s
  INNER JOIN {CATALOG}.sys.database_principals AS dp
    ON s.principal_id = dp.principal_id
  WHERE s.schema_id < 16384";
      RegisterReplacements(replacementsRegistry);
      query = PerformReplacements(query);
      return query;
    }

    private void ReadSchemaData(DbDataReader reader)
    {
      var identifier = reader.GetInt32(0);
      var name = reader.GetString(1);
      var schema = !targetSchemes.Contains(name) ? catalog.CreateSchema(name) : catalog.Schemas[name];

      schemaIndex[identifier] = schema;
      reversedSchemaIndex[schema] = identifier;
      schema.Owner = reader.GetString(2);
    }

    // Types & domains must be extracted for all schemas
    private void ExtractTypes()
    {
      var query = BuildExtractTypesQuery();

      using var command = Connection.CreateCommand(query);
      using var reader = command.ExecuteReader();
      while (reader.Read()) {
        ReadTypeData(reader);
      }
    }

    private async Task ExtractTypesAsync(CancellationToken token)
    {
      var query = BuildExtractTypesQuery();

      var command = Connection.CreateCommand(query);
      await using (command.ConfigureAwait(false))
      await using (var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadTypeData(reader);
        }
      }
    }

    private string BuildExtractTypesQuery()
    {
      var query = @"
  SELECT 
    schema_id,
    user_type_id,
    system_type_id,
    name,
    precision,
    scale,
    max_length,
    is_user_defined 
  FROM {CATALOG}.sys.types
  ORDER BY 
    is_user_defined,
    user_type_id";
      query = PerformReplacements(query);
      return query;
    }

    private void ReadTypeData(DbDataReader reader)
    {
      var userTypeId = reader.GetInt32(1);
      int systemTypeId = reader.GetByte(2);
      var name = reader.GetString(3);
      typeNameIndex[userTypeId] = name;

      // Type is not user-defined
      if (!reader.GetBoolean(7)) {
        return;
      }

      // Unknown system type
      if (!typeNameIndex.TryGetValue(systemTypeId, out _)) {
        return;
      }

      var currentSchema = GetSchema(reader.GetInt32(0));
      var dataType = GetValueType(typeNameIndex[systemTypeId], reader.GetByte(4), reader.GetByte(5), reader.GetInt16(6));
      var domain = currentSchema.CreateDomain(name, dataType);
      domainIndex[userTypeId] = domain;
    }

    private void ExtractTablesAndViews()
    {
      var query = BuildExtractTablesAndViewsQuery();

      using var cmd = Connection.CreateCommand(query);
      using var reader = cmd.ExecuteReader();
      while (reader.Read()) {
        ReadTableOrViewData(reader);
      }
    }

    private async Task ExtractTablesAndViewsAsync(CancellationToken token)
    {
      var query = BuildExtractTablesAndViewsQuery();

      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false))
      await using (var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadTableOrViewData(reader);
        }
      }
    }

    private string BuildExtractTablesAndViewsQuery()
    {
      var query = @"
  SELECT
    t.schema_id,
    t.object_id,
    t.name,
    t.type
  FROM (
    SELECT
      schema_id,
      object_id,
      name,
      0 type
    FROM {CATALOG}.sys.tables
    WHERE {SYSTABLE_FILTER}
    UNION 
    SELECT
      schema_id,
      object_id,
      name,
      1 type
    FROM {CATALOG}.sys.views
    ) AS t
  WHERE t.schema_id {SCHEMA_FILTER}
  ORDER BY t.schema_id, t.object_id";
      query = PerformReplacements(query);
      return query;
    }

    private void ReadTableOrViewData(DbDataReader reader)
    {
      var currentSchema = GetSchema(reader.GetInt32(0));
      var tableType = reader.GetInt32(3);
      DataTable dataTable;
      if (tableType == 0) {
        dataTable = currentSchema.CreateTable(reader.GetString(2));
      }
      else {
        dataTable = currentSchema.CreateView(reader.GetString(2));
      }

      columnResolverIndex[reader.GetInt32(1)] = new ColumnResolver(dataTable);
    }

    private void ExtractColumns()
    {
      var query = BuildExtractColumnsQuery();

      var currentTableId = 0;
      ColumnResolver columnResolver = null;
      using (var cmd = Connection.CreateCommand(query))
      using (var reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          ReadColumnData(reader, ref currentTableId, ref columnResolver);
        }
      }

      query = BuildExtractIdentityColumnsQuery();

      using (var cmd = Connection.CreateCommand(query))
      using (var reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          ReadIdentityColumnData(reader);
        }
      }
    }

    private async Task ExtractColumnsAsync(CancellationToken token)
    {
      var query = BuildExtractColumnsQuery();

      var currentTableId = 0;
      ColumnResolver columnResolver = null;
      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false))
      await using (var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadColumnData(reader, ref currentTableId, ref columnResolver);
        }
      }

      query = BuildExtractIdentityColumnsQuery();

      cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false))
      await using (var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadIdentityColumnData(reader);
        }
      }
    }

    private string BuildExtractColumnsQuery()
    {
      var query = @"
  SELECT
    t.schema_id,
    c.object_id,
    c.column_id,
    c.name,
    c.user_type_id,
    c.precision,
    c.scale,
    c.max_length,
    c.collation_name,
    c.is_nullable,
    c.is_identity,
    dc.name,
    dc.definition,
    cc.is_persisted,
    cc.definition
  FROM {CATALOG}.sys.columns AS c 
  INNER JOIN (
    SELECT
      schema_id,
      object_id,
      0 as type
    FROM {CATALOG}.sys.tables
    WHERE {SYSTABLE_FILTER}
    UNION
    SELECT
      schema_id,
      object_id,
      1 AS type
    FROM {CATALOG}.sys.views
    ) AS t ON c.object_id = t.object_id
  LEFT OUTER JOIN {CATALOG}.sys.default_constraints AS dc
    ON c.object_id = dc.parent_object_id 
      AND c.column_id = dc.parent_column_id
  LEFT OUTER JOIN {CATALOG}.sys.computed_columns AS cc 
    ON c.object_id = cc.object_id 
      AND c.column_id = cc.column_id
  WHERE t.schema_id {SCHEMA_FILTER}
  ORDER BY
    t.schema_id,
    c.object_id,
    c.column_id";
      query = PerformReplacements(query);
      return query;
    }

    private void ReadColumnData(DbDataReader reader, ref int currentTableId, ref ColumnResolver columnResolver)
    {
      var tableId = reader.GetInt32(1);
      var columnId = reader.GetInt32(2);
      GetDataTable(tableId, ref currentTableId, ref columnResolver);
      // Table column
      if (columnResolver.Table is Table table) {
        var typeId = reader.GetInt32(4);
        var sqlDataType = GetValueType(typeNameIndex[typeId], reader.GetByte(5), reader.GetByte(6), reader.GetInt16(7));
        var column = table.CreateColumn(reader.GetString(3), sqlDataType);
        var count = table.TableColumns.Count;

        // <-db column index is not equal to column position in table.Columns. This is common after column removals or insertions.
        columnResolver.RegisterColumnMapping(columnId, count - 1);

        // Domain
        if (domainIndex.TryGetValue(typeId, out var domain)) {
          column.Domain = domain;
        }

        // Collation
        if (!reader.IsDBNull(8)) {
          var currentSchema = GetSchema(reader.GetInt32(0));
          var collationName = reader.GetString(8);
          column.Collation = currentSchema.Collations[collationName] ?? currentSchema.CreateCollation(collationName);
        }

        // Nullability
        column.IsNullable = reader.GetBoolean(9);

        // Default constraint
        if (!reader.IsDBNull(11)) {
          table.CreateDefaultConstraint(reader.GetString(11), column);
          column.DefaultValue = reader.GetString(12).StripRoundBrackets();
        }

        // Computed column
        if (!reader.IsDBNull(13)) {
          column.IsPersisted = reader.GetBoolean(13);
          column.Expression = SqlDml.Native(reader.GetString(14));
        }
      }
      else {
        var view = (View) columnResolver.Table;
        view.CreateColumn(reader.GetString(3));
      }
    }

    private string BuildExtractIdentityColumnsQuery()
    {
      var query = @"
  SELECT
    t.schema_id,
    ic.object_id,
    ic.column_id,
    ic.seed_value,
    ic.increment_value,
    ic.last_value
  FROM {CATALOG}.sys.identity_columns AS ic 
  INNER JOIN {CATALOG}.sys.tables AS t 
    ON ic.object_id = t.object_id
  WHERE seed_value IS NOT NULL
    AND increment_value IS NOT NULL
    AND {SYSTABLE_FILTER}
    AND t.schema_id {SCHEMA_FILTER}
  ORDER BY
    t.schema_id,
    ic.object_id";
      query = PerformReplacements(query);
      return query;
    }

    private void ReadIdentityColumnData(DbDataReader reader)
    {
      var dataColumn = columnResolverIndex[reader.GetInt32(1)].GetColumn(reader.GetInt32(2));

      var tableColumn = (TableColumn) dataColumn;
      tableColumn.SequenceDescriptor = new SequenceDescriptor(tableColumn);
      if (!reader.IsDBNull(3)) {
        tableColumn.SequenceDescriptor.StartValue = Convert.ToInt64(reader.GetValue(3));
      }

      if (!reader.IsDBNull(4)) {
        tableColumn.SequenceDescriptor.Increment = Convert.ToInt64(reader.GetValue(4));
      }

      if (!reader.IsDBNull(5)) {
        tableColumn.SequenceDescriptor.LastValue = Convert.ToInt64(reader.GetValue(5));
      }
    }

    public void ExtractCheckConstraints()
    {
      // TODO: Implement
    }

    protected virtual string BuildExtractIndexesQuery()
    {
      var query = @"
  SELECT
    t.schema_id,
    t.object_id,
    t.type,
    i.index_id,
    i.name,
    i.type,
    i.is_primary_key,
    i.is_unique,
    i.is_unique_constraint,
    i.fill_factor,
    ic.column_id,
    0,
    ic.key_ordinal,
    ic.is_descending_key,
    ic.is_included_column,
    NULL,
    NULL
  FROM {CATALOG}.sys.indexes i
  INNER JOIN (
    SELECT
      schema_id,
      object_id,
      0 AS type
    FROM {CATALOG}.sys.tables
    WHERE {SYSTABLE_FILTER}
    UNION
    SELECT
      schema_id,
      object_id,
      1 AS type
    FROM {CATALOG}.sys.views
    ) AS t
      ON i.object_id = t.object_id
  INNER JOIN {CATALOG}.sys.index_columns ic
    ON i.object_id = ic.object_id
      AND i.index_id = ic.index_id
  WHERE i.type IN(1, 2, 4)
    AND schema_id {SCHEMA_FILTER}
  ORDER BY
    t.schema_id,
    t.object_id,
    i.index_id,
    ic.is_included_column,
    ic.key_ordinal,
    ic.index_column_id";
      query = PerformReplacements(query);
      return query;
    }

    private void ExtractIndexes()
    {
      var query = BuildExtractIndexesQuery();
      const int spatialIndexType = 4;

      var tableId = 0;
      ColumnResolver table = null;
      Index index = null;
      PrimaryKey primaryKey = null;
      UniqueConstraint uniqueConstraint = null;
      using var cmd = Connection.CreateCommand(query);
      using var reader = cmd.ExecuteReader();
      while (reader.Read()) {
        ReadIndexColumnData(
          reader, ref tableId, spatialIndexType, ref primaryKey, ref uniqueConstraint, ref index, ref table);
      }
    }

    private async Task ExtractIndexesAsync(CancellationToken token)
    {
      var query = BuildExtractIndexesQuery();
      const int spatialIndexType = 4;

      var tableId = 0;
      ColumnResolver table = null;
      Index index = null;
      PrimaryKey primaryKey = null;
      UniqueConstraint uniqueConstraint = null;
      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false))
      await using (var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadIndexColumnData(
            reader, ref tableId, spatialIndexType, ref primaryKey, ref uniqueConstraint, ref index, ref table);
        }
      }
    }

    private void ReadIndexColumnData(DbDataReader reader, ref int tableId, int spatialIndexType, ref PrimaryKey primaryKey,
      ref UniqueConstraint uniqueConstraint, ref Index index, ref ColumnResolver table)
    {
      var columnId = reader.GetInt32(10);
      int indexType = reader.GetByte(5);

      // First column in index => new index or index is spatial (always has exactly one column)
      if (reader.GetByte(12) == 1 || indexType == spatialIndexType) {
        primaryKey = null;
        uniqueConstraint = null;
        index = null;
        // Table could be changed only on new index creation
        GetDataTable(reader.GetInt32(1), ref tableId, ref table);
        var indexId = reader.GetInt32(3);
        var indexName = reader.GetString(4);

        // Index is a part of primary key constraint
        if (reader.GetBoolean(6)) {
          primaryKey = ((Table) table.Table).CreatePrimaryKey(indexName);
          if (Driver.ServerInfo.PrimaryKey.Features.Supports(PrimaryKeyConstraintFeatures.Clustered)) {
            primaryKey.IsClustered = reader.GetByte(5) == 1;
          }
        }
        else {
          // Spatial index
          if (indexType == spatialIndexType) {
            index = table.Table.CreateSpatialIndex(indexName);
            index.FillFactor = reader.GetByte(9);
          }
          else {
            index = table.Table.CreateIndex(indexName);
            index.IsUnique = reader.GetBoolean(7);
            if (Driver.ServerInfo.Index.Features.Supports(IndexFeatures.Clustered)) {
              index.IsClustered = reader.GetByte(5) == 1;
            }

            index.FillFactor = reader.GetByte(9);
            if (!reader.IsDBNull(15) && reader.GetBoolean(15)) {
              index.Where = SqlDml.Native(reader.GetString(16));
            }

            // Index is a part of unique constraint
            if (reader.GetBoolean(8)) {
              uniqueConstraint = ((Table) table.Table).CreateUniqueConstraint(indexName);
              if (index.IsClustered
                && Driver.ServerInfo.UniqueConstraint.Features.Supports(UniqueConstraintFeatures.Clustered)) {
                uniqueConstraint.IsClustered = true;
              }
            }
          }
        }
      }

      // Column is a part of a primary index
      if (reader.GetBoolean(6)) {
        primaryKey.Columns.Add((TableColumn) table.GetColumn(columnId));
      }
      else {
        // Column is a part of unique constraint
        if (reader.GetBoolean(8)) {
          uniqueConstraint.Columns.Add((TableColumn) table.GetColumn(columnId));
        }

        if (index != null) {
          // Column is non key column
          if (reader.GetBoolean(14)) {
            index.NonkeyColumns.Add(table.GetColumn(columnId));
          }
          else {
            index.CreateIndexColumn(table.GetColumn(columnId), !reader.GetBoolean(13));
          }
        }
      }
    }

    private void ExtractForeignKeys()
    {
      var query = BuildExtractForeignKeysQuery();

      int tableId = 0, constraintId = 0;
      ColumnResolver referencingTable = null;
      ColumnResolver referencedTable = null;
      ForeignKey foreignKey = null;
      using var cmd = Connection.CreateCommand(query);
      using var reader = cmd.ExecuteReader();
      while (reader.Read()) {
        ReadForeignKeyColumnData(reader, ref tableId, ref foreignKey, ref referencingTable, ref referencedTable);
      }
    }

    private async Task ExtractForeignKeysAsync(CancellationToken token)
    {
      var query = BuildExtractForeignKeysQuery();

      int tableId = 0, constraintId = 0;
      ColumnResolver referencingTable = null;
      ColumnResolver referencedTable = null;
      ForeignKey foreignKey = null;
      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false))
      await using (var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadForeignKeyColumnData(reader, ref tableId, ref foreignKey, ref referencingTable, ref referencedTable);
        }
      }
    }

    private string BuildExtractForeignKeysQuery()
    {
      var query = @"
  SELECT
    fk.schema_id,
    fk.object_id,
    fk.name,
    fk.delete_referential_action,
    fk.update_referential_action,
    fkc.constraint_column_id,
    fkc.parent_object_id,
    fkc.parent_column_id,
    fkc.referenced_object_id,
    fkc.referenced_column_id
  FROM {CATALOG}.sys.foreign_keys fk
  INNER JOIN {CATALOG}.sys.foreign_key_columns fkc
    ON fk.object_id = fkc.constraint_object_id
  WHERE fk.schema_id {SCHEMA_FILTER}
  ORDER BY
    fk.schema_id,
    fkc.parent_object_id,
    fk.object_id,
    fkc.constraint_column_id";
      query = PerformReplacements(query);
      return query;
    }

    private void ReadForeignKeyColumnData(DbDataReader reader, ref int tableId, ref ForeignKey foreignKey,
      ref ColumnResolver referencingTable, ref ColumnResolver referencedTable)
    {
      // First column in constraint => new constraint
      if (reader.GetInt32(5) == 1) {
        GetDataTable(reader.GetInt32(6), ref tableId, ref referencingTable);
        foreignKey = ((Table) referencingTable.Table).CreateForeignKey(reader.GetString(2));
        referencedTable = columnResolverIndex[reader.GetInt32(8)];
        foreignKey.ReferencedTable = (Table) referencedTable.Table;
        foreignKey.OnDelete = GetReferentialAction(reader.GetByte(3));
        foreignKey.OnUpdate = GetReferentialAction(reader.GetByte(4));
      }

      foreignKey.Columns.Add((TableColumn) referencingTable.GetColumn(reader.GetInt32(7)));
      foreignKey.ReferencedColumns.Add((TableColumn) referencedTable.GetColumn(reader.GetInt32(9)));
    }

    protected virtual void ExtractFulltextIndexes()
    {
      var query = BuildExtractFullTextIndexesQuery();

      var currentTableId = 0;
      ColumnResolver table = null;
      FullTextIndex index = null;
      using var cmd = Connection.CreateCommand(query);
      using var reader = cmd.ExecuteReader();
      while (reader.Read()) {
        ReadFullTextIndexColumnData(reader, ref currentTableId, ref table, ref index);
      }
    }

    protected virtual async Task ExtractFulltextIndexesAsync(CancellationToken token)
    {
      var query = BuildExtractFullTextIndexesQuery();

      var currentTableId = 0;
      ColumnResolver table = null;
      FullTextIndex index = null;
      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false))
      await using (var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadFullTextIndexColumnData(reader, ref currentTableId, ref table, ref index);
        }
      }
    }

    private string BuildExtractFullTextIndexesQuery()
    {
      var query = @"
  SELECT
    t.schema_id,
    fic.object_id,
    fi.unique_index_id,
    fc.name,
    fc.is_default,
    fic.column_id,
    fic.type_column_id,
    fl.name,
    i.name,
    fi.change_tracking_state,
    fi.crawl_start_date
  FROM {CATALOG}.sys.tables t
  INNER JOIN {CATALOG}.sys.fulltext_index_columns AS fic
    ON t.object_id = fic.object_id 
  INNER JOIN {CATALOG}.sys.fulltext_languages AS fl
    ON fic.language_id = fl.lcid
  INNER JOIN {CATALOG}.sys.fulltext_indexes fi
    ON fic.object_id = fi.object_id
  INNER JOIN {CATALOG}.sys.fulltext_catalogs fc
    ON fc.fulltext_catalog_id = fi.fulltext_catalog_id
  INNER JOIN {CATALOG}.sys.indexes AS i 
    ON fic.object_id = i.object_id
      AND fi.unique_index_id = i.index_id
  WHERE {SYSTABLE_FILTER}
    AND t.schema_id {SCHEMA_FILTER}
  ORDER BY
    t.schema_id,
    fic.object_id,
    fic.column_id";
      query = PerformReplacements(query);
      return query;
    }

    private void ReadFullTextIndexColumnData(DbDataReader reader, ref int currentTableId, ref ColumnResolver table,
      ref FullTextIndex index)
    {
      var nextTableId = reader.GetInt32(1);
      if (currentTableId != nextTableId) {
        GetDataTable(nextTableId, ref currentTableId, ref table);
        index = table.Table.CreateFullTextIndex(string.Empty);
        index.FullTextCatalog = reader.GetBoolean(4)
          ? null
          : reader.GetString(3);
        index.UnderlyingUniqueIndex = reader.GetString(8);
        index.ChangeTrackingMode = GetChangeTrackingMode(reader.GetString(9), reader.IsDBNull(10));
      }

      var column = index.CreateIndexColumn(table.GetColumn(reader.GetInt32(5)));
      column.TypeColumn = (reader.IsDBNull(6)) ? null : table.GetColumn(reader.GetInt32(6));
      column.Languages.Add(new Language(reader.GetString(7)));
    }

    protected string PerformReplacements(string query)
    {
      foreach (var registry in replacementsRegistry) {
        query = query.Replace(registry.Key, registry.Value);
      }

      return query;
    }

    private static ReferentialAction GetReferentialAction(int actionCode) =>
      actionCode switch {
        0 => ReferentialAction.NoAction,
        1 => ReferentialAction.Cascade,
        2 => ReferentialAction.SetNull,
        _ => ReferentialAction.SetDefault
      };

    private static ChangeTrackingMode GetChangeTrackingMode(string mode, bool isPopulationOff) =>
      mode switch {
        "A" => ChangeTrackingMode.Auto,
        "M" => ChangeTrackingMode.Manual,
        "O" => isPopulationOff ? ChangeTrackingMode.OffWithNoPopulation : ChangeTrackingMode.Off,
        _ => ChangeTrackingMode.Default
      };

    // Do not touch this!!! The author is Denis Krjuchkov
    private SqlValueType GetValueType(string dataType, int numericPrecision, int numericScale, int maxLength)
    {
      var typeName = dataType;
      var typeInfo = Connection.Driver.ServerInfo.DataTypes[typeName];
      SqlType type;
      if (typeInfo!=null) {
        type = typeInfo.Type;
        typeName = null;
      }
      else {
        type = SqlType.Unknown;
      }

      int? precision = numericPrecision;
      int? scale = numericScale;

      if (typeInfo!=null && typeInfo.MaxPrecision==null) {
        // resetting precision & scale for types that do not require specifying them
        precision = null;
        scale = null;
      }

      if (numericPrecision > 0 || numericScale > 0) {
        maxLength = 0;
      }

      int? size = maxLength;
      if (size <= 0) {
        size = null;
        if (type==SqlType.VarChar) {
          type = SqlType.VarCharMax;
        }

        if (type==SqlType.VarBinary) {
          type = SqlType.VarBinaryMax;
        }
      }
      if (typeInfo!=null) {
        if (typeInfo.MaxLength==null) {
          // resetting length for types that do not require specifying it
          size = null;
        }
        else if (size!=null && size > 1) {
          if (type==SqlType.Char ||
            type==SqlType.VarChar ||
            type==SqlType.VarCharMax) {
            size /= 2;
          }
        }
      }

      return new SqlValueType(type, typeName, size, precision, scale);
    }

    protected Schema GetSchema(int id) => schemaIndex[id];

    private void GetDataTable(int id, ref int currentId, ref ColumnResolver currentObj)
    {
      if (id == currentId) {
        return;
      }

      currentObj = columnResolverIndex[id];
      currentId = id;
    }

    protected virtual void RegisterReplacements(Dictionary<string, string> replacements)
    {
      var catalogRef = Driver.Translator.QuoteIdentifier(catalog.Name);

      //string schemaFilter = (this.schema)!=null
      //  ? " = " + schemaId
      //  : " > 0";

      replacements[SchemaFilterPlaceholder] = MakeSchemaFilter();
      replacements[SysTablesFilterPlaceholder] = "1 > 0";
      replacements[CatalogPlaceholder] = catalogRef;
    }

    private string MakeSchemaFilter()
    {
      if (targetSchemes.Count==0) {
        return " > 0";
      }

      var builder = new StringBuilder();
      foreach (var targetScheme in targetSchemes) {
        if (!reversedSchemaIndex.TryGetValue(catalog.Schemas[targetScheme], out var schemaId)) {
          continue;
        }

        builder.Append(builder.Length == 0
          ? schemaId.ToString(CultureInfo.InvariantCulture)
          : $", {schemaId.ToString(CultureInfo.InvariantCulture)}");
      }
      return $" IN ({builder})";
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}