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
    protected class ExtractionContext
    {
      private readonly Dictionary<string, string> replacementsRegistry;

      public readonly Catalog Catalog;
      public readonly string QuotedCatalogName;

      public readonly IReadOnlyCollection<string> TargetSchemas;
      public readonly Dictionary<int, Schema> SchemaIndex;
      public readonly Dictionary<Schema, int> ReversedSchemaIndex;
      public readonly Dictionary<int, Domain> DomainIndex;
      public readonly Dictionary<int, string> TypeNameIndex;
      public readonly Dictionary<int, ColumnResolver> ColumnResolverIndex;

      public void RegisterReplacement(string placeholder, string value)
      {
        if (placeholder == CatalogPlaceholder)
          throw new ArgumentException("Cannot override catalog name placeholder.");
        replacementsRegistry[placeholder] = value;
      }

      public string PerformReplacements(string rawQueryText)
      {
        foreach (var registry in replacementsRegistry) {
          rawQueryText = rawQueryText.Replace(registry.Key, registry.Value);
        }
        return rawQueryText;
      }

      public ExtractionContext(Catalog catalog, IReadOnlyCollection<string> targetSchemas)
      {
        ArgumentValidator.EnsureArgumentNotNull(catalog, nameof(catalog));
        ArgumentValidator.EnsureArgumentNotNull(targetSchemas, nameof(targetSchemas));

        Catalog = catalog;
        TargetSchemas = targetSchemas;
        QuotedCatalogName = SqlHelper.QuoteIdentifierWithBrackets(new[] { catalog.Name });

        SchemaIndex = new Dictionary<int, Schema>();
        ReversedSchemaIndex = new Dictionary<Schema, int>();
        DomainIndex = new Dictionary<int, Domain>();
        TypeNameIndex = new Dictionary<int, string>();
        ColumnResolverIndex = new Dictionary<int, ColumnResolver>();
        replacementsRegistry = new Dictionary<string, string>();

        replacementsRegistry[CatalogPlaceholder] = QuotedCatalogName;
      }
    }

    protected const string SysTablesFilterPlaceholder = "{SYSTABLE_FILTER}";
    protected const string CatalogPlaceholder = "{CATALOG}";
    protected const string SchemaFilterPlaceholder = "{SCHEMA_FILTER}";

    public override Catalog ExtractCatalog(string catalogName) =>
      ExtractSchemes(catalogName, Array.Empty<string>());

    public override Task<Catalog> ExtractCatalogAsync(string catalogName, CancellationToken token = default) =>
      ExtractSchemesAsync(catalogName, Array.Empty<string>(), token);

    public override Catalog ExtractSchemes(string catalogName, string[] schemaNames)
    {
      var context = CreateContext(catalogName, schemaNames);
      ExtractCatalogContents(context);
      return context.Catalog;
    }

    public override async Task<Catalog> ExtractSchemesAsync(
      string catalogName, string[] schemaNames, CancellationToken token = default)
    {
      var context = CreateContext(catalogName, schemaNames);
      await ExtractCatalogContentsAsync(context, token).ConfigureAwait(false);
      return context.Catalog;
    }

    protected virtual void ExtractCatalogContents(ExtractionContext context)
    {
      ExtractSchemas(context);
      RegisterReplacements(context);
      ExtractTypes(context);
      ExtractTablesAndViews(context);
      ExtractColumns(context);
      ExtractIndexes(context);
      ExtractForeignKeys(context);
      ExtractFulltextIndexes(context);
    }

    protected virtual async Task ExtractCatalogContentsAsync(ExtractionContext context, CancellationToken token)
    {
      await ExtractSchemasAsync(context, token).ConfigureAwait(false);
      RegisterReplacements(context);
      await ExtractTypesAsync(context, token).ConfigureAwait(false);
      await ExtractTablesAndViewsAsync(context, token).ConfigureAwait(false);
      await ExtractColumnsAsync(context, token).ConfigureAwait(false);
      await ExtractIndexesAsync(context, token).ConfigureAwait(false);
      await ExtractForeignKeysAsync(context, token).ConfigureAwait(false);
      await ExtractFulltextIndexesAsync(context, token).ConfigureAwait(false);
    }

    protected virtual void RegisterReplacements(ExtractionContext context)
    {
      context.RegisterReplacement(SchemaFilterPlaceholder, MakeSchemaFilter(context));
      context.RegisterReplacement(SysTablesFilterPlaceholder, "1 > 0");
    }

    private ExtractionContext CreateContext(string catalogName, string[] schemaNames)
    {
      var catalog = new Catalog(catalogName);
      var context = new ExtractionContext(catalog, schemaNames);
      foreach (var schemaName in schemaNames) {
        _ = catalog.CreateSchema(schemaName);
      }
      return context;
    }

    // All schemas
    private void ExtractSchemas(ExtractionContext context)
    {
      var query = BuildExtractSchemasQuery(context);

      using (var cmd = Connection.CreateCommand(query))
      using (var reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          ReadSchemaData(reader, context);
        }
      }
    }

    private async Task ExtractSchemasAsync(ExtractionContext context, CancellationToken token)
    {
      var query = BuildExtractSchemasQuery(context);

      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadSchemaData(reader, context);
          }
        }
      }
    }

    private string BuildExtractSchemasQuery(ExtractionContext context)
    {
      var query = $@"
  SELECT
    s.schema_id,
    s.name,
    dp.name
  FROM {context.QuotedCatalogName}.sys.schemas AS s
  INNER JOIN {context.QuotedCatalogName}.sys.database_principals AS dp
    ON s.principal_id = dp.principal_id
  WHERE s.schema_id < 16384";

      return query;
    }

    private void ReadSchemaData(DbDataReader reader, ExtractionContext context)
    {
      var identifier = reader.GetInt32(0);
      var name = reader.GetString(1);
      var schema = context.Catalog.Schemas[name] ?? context.Catalog.CreateSchema(name);

      context.SchemaIndex[identifier] = schema;
      context.ReversedSchemaIndex[schema] = identifier;
      schema.Owner = reader.GetString(2);
    }

    // Types & domains must be extracted for all schemas
    private void ExtractTypes(ExtractionContext context)
    {
      var query = BuildExtractTypesQuery(context);

      using var command = Connection.CreateCommand(query);
      using var reader = command.ExecuteReader();
      while (reader.Read()) {
        ReadTypeData(reader, context);
      }
    }

    private async Task ExtractTypesAsync(ExtractionContext context, CancellationToken token)
    {
      var query = BuildExtractTypesQuery(context);

      var command = Connection.CreateCommand(query);
      await using (command.ConfigureAwait(false)) {
        var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadTypeData(reader, context);
          }
        }
      }
    }

    private string BuildExtractTypesQuery(ExtractionContext context)
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
      query = context.PerformReplacements(query);
      return query;
    }

    private void ReadTypeData(DbDataReader reader, ExtractionContext context)
    {
      var userTypeId = reader.GetInt32(1);
      int systemTypeId = reader.GetByte(2);
      var name = reader.GetString(3);
      context.TypeNameIndex[userTypeId] = name;

      // Type is not user-defined
      if (!reader.GetBoolean(7)) {
        return;
      }

      // Unknown system type
      string systemTypeName;
      if (!context.TypeNameIndex.TryGetValue(systemTypeId, out systemTypeName)) {
        return;
      }

      var currentSchema = context.SchemaIndex[reader.GetInt32(0)];
      var dataType = GetValueType(systemTypeName, reader.GetByte(4), reader.GetByte(5), reader.GetInt16(6));
      var domain = currentSchema.CreateDomain(name, dataType);
      context.DomainIndex[userTypeId] = domain;
    }

    private void ExtractTablesAndViews(ExtractionContext context)
    {
      var query = BuildExtractTablesAndViewsQuery(context);

      using var cmd = Connection.CreateCommand(query);
      using var reader = cmd.ExecuteReader();
      while (reader.Read()) {
        ReadTableOrViewData(reader, context);
      }
    }

    private async Task ExtractTablesAndViewsAsync(ExtractionContext context, CancellationToken token)
    {
      var query = BuildExtractTablesAndViewsQuery(context);

      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadTableOrViewData(reader, context);
          }
        }
      }
    }

    private string BuildExtractTablesAndViewsQuery(ExtractionContext context)
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
      query = context.PerformReplacements(query);
      return query;
    }

    private void ReadTableOrViewData(DbDataReader reader, ExtractionContext context)
    {
      var currentSchema = context.SchemaIndex[reader.GetInt32(0)];
      var tableType = reader.GetInt32(3);
      DataTable dataTable;
      if (tableType == 0) {
        dataTable = currentSchema.CreateTable(reader.GetString(2));
      }
      else {
        dataTable = currentSchema.CreateView(reader.GetString(2));
      }

      context.ColumnResolverIndex[reader.GetInt32(1)] = new ColumnResolver(dataTable);
    }

    private void ExtractColumns(ExtractionContext context)
    {
      var query = BuildExtractColumnsQuery(context);

      var currentTableId = 0;
      ColumnResolver columnResolver = null;
      using (var cmd = Connection.CreateCommand(query))
      using (var reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          ReadColumnData(context, reader, ref currentTableId, ref columnResolver);
        }
      }

      query = BuildExtractIdentityColumnsQuery(context);

      using (var cmd = Connection.CreateCommand(query))
      using (var reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          ReadIdentityColumnData(reader, context);
        }
      }
    }

    private async Task ExtractColumnsAsync(ExtractionContext context, CancellationToken token)
    {
      var query = BuildExtractColumnsQuery(context);

      var currentTableId = 0;
      var cmd = Connection.CreateCommand(query);
      ColumnResolver columnResolver = null;
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadColumnData(context, reader, ref currentTableId, ref columnResolver);
          }
        }
      }

      query = BuildExtractIdentityColumnsQuery(context);

      cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadIdentityColumnData(reader, context);
          }
        }
      }
    }

    private string BuildExtractColumnsQuery(ExtractionContext context)
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
      query = context.PerformReplacements(query);
      return query;
    }

    private void ReadColumnData(ExtractionContext context, DbDataReader reader, ref int currentTableId, ref ColumnResolver columnResolver)
    {
      var tableId = reader.GetInt32(1);
      var columnId = reader.GetInt32(2);
      GetDataTable(tableId, context, ref currentTableId, ref columnResolver);


      // Table column
      if (columnResolver.Table is Table table) {
        var typeId = reader.GetInt32(4);
        var sqlDataType = GetValueType(context.TypeNameIndex[typeId], reader.GetByte(5), reader.GetByte(6), reader.GetInt16(7));
        var column = table.CreateColumn(reader.GetString(3), sqlDataType);
        var count = table.TableColumns.Count;

        // <-db column index is not equal to column position in table.Columns. This is common after column removals or insertions.
        columnResolver.RegisterColumnMapping(columnId, count - 1);

        // Domain
        if (context.DomainIndex.TryGetValue(typeId, out var domain)) {
          column.Domain = domain;
        }

        // Collation
        if (!reader.IsDBNull(8)) {
          var currentSchema = context.SchemaIndex[reader.GetInt32(0)];
          var collationName = reader.GetString(8);
          column.Collation = currentSchema.Collations[collationName] ?? currentSchema.CreateCollation(collationName);
        }

        // Nullability
        column.IsNullable = reader.GetBoolean(9);

        // Default constraint
        if (!reader.IsDBNull(11)) {
          _ = table.CreateDefaultConstraint(reader.GetString(11), column);
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
        _ = view.CreateColumn(reader.GetString(3));
      }
    }

    private string BuildExtractIdentityColumnsQuery(ExtractionContext context)
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
      query = context.PerformReplacements(query);
      return query;
    }

    private void ReadIdentityColumnData(DbDataReader reader, ExtractionContext context)
    {
      var dataColumn = context.ColumnResolverIndex[reader.GetInt32(1)].GetColumn(reader.GetInt32(2));

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

    private void ExtractCheckConstraints()
    {
      // TODO: Implement
    }

    protected virtual string BuildExtractIndexesQuery(ExtractionContext context)
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
      query = context.PerformReplacements(query);
      return query;
    }

    private void ExtractIndexes(ExtractionContext context)
    {
      var query = BuildExtractIndexesQuery(context);
      const int spatialIndexType = 4;

      var tableId = 0;
      ColumnResolver table = null;
      Index index = null;
      PrimaryKey primaryKey = null;
      UniqueConstraint uniqueConstraint = null;
      using var cmd = Connection.CreateCommand(query);
      using var reader = cmd.ExecuteReader();
      while (reader.Read()) {
        ReadIndexColumnData(reader, context,
          ref tableId, spatialIndexType, ref primaryKey, ref uniqueConstraint, ref index, ref table);
      }
    }

    private async Task ExtractIndexesAsync(ExtractionContext context, CancellationToken token)
    {
      var query = BuildExtractIndexesQuery(context);
      const int spatialIndexType = 4;

      var tableId = 0;
      ColumnResolver table = null;
      Index index = null;
      PrimaryKey primaryKey = null;
      UniqueConstraint uniqueConstraint = null;
      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadIndexColumnData(reader, context,
              ref tableId, spatialIndexType, ref primaryKey, ref uniqueConstraint, ref index, ref table);
          }
        }
      }
    }

    private void ReadIndexColumnData(DbDataReader reader, ExtractionContext context,
      ref int tableId, int spatialIndexType, ref PrimaryKey primaryKey,
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
        GetDataTable(reader.GetInt32(1), context, ref tableId, ref table);
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
            _ = index.CreateIndexColumn(table.GetColumn(columnId), !reader.GetBoolean(13));
          }
        }
      }
    }

    private void ExtractForeignKeys(ExtractionContext context)
    {
      var query = BuildExtractForeignKeysQuery(context);

      int tableId = 0;
      ColumnResolver referencingTable = null;
      ColumnResolver referencedTable = null;
      ForeignKey foreignKey = null;
      using var cmd = Connection.CreateCommand(query);
      using var reader = cmd.ExecuteReader();
      while (reader.Read()) {
        ReadForeignKeyColumnData(reader, context, ref tableId, ref foreignKey, ref referencingTable, ref referencedTable);
      }
    }

    private async Task ExtractForeignKeysAsync(ExtractionContext context, CancellationToken token)
    {
      var query = BuildExtractForeignKeysQuery(context);

      int tableId = 0;
      ColumnResolver referencingTable = null;
      ColumnResolver referencedTable = null;
      ForeignKey foreignKey = null;
      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadForeignKeyColumnData(reader, context, ref tableId, ref foreignKey, ref referencingTable, ref referencedTable);
          }
        }
      }
    }

    private string BuildExtractForeignKeysQuery(ExtractionContext context)
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
      query = context.PerformReplacements(query);
      return query;
    }

    private void ReadForeignKeyColumnData(DbDataReader reader, ExtractionContext context, ref int tableId, ref ForeignKey foreignKey,
      ref ColumnResolver referencingTable, ref ColumnResolver referencedTable)
    {
      // First column in constraint => new constraint
      if (reader.GetInt32(5) == 1) {
        GetDataTable(reader.GetInt32(6), context, ref tableId, ref referencingTable);
        foreignKey = ((Table) referencingTable.Table).CreateForeignKey(reader.GetString(2));
        referencedTable = context.ColumnResolverIndex[reader.GetInt32(8)];
        foreignKey.ReferencedTable = (Table) referencedTable.Table;
        foreignKey.OnDelete = GetReferentialAction(reader.GetByte(3));
        foreignKey.OnUpdate = GetReferentialAction(reader.GetByte(4));
      }

      foreignKey.Columns.Add((TableColumn) referencingTable.GetColumn(reader.GetInt32(7)));
      foreignKey.ReferencedColumns.Add((TableColumn) referencedTable.GetColumn(reader.GetInt32(9)));
    }

    protected virtual void ExtractFulltextIndexes(ExtractionContext context)
    {
      var query = BuildExtractFullTextIndexesQuery(context);

      var currentTableId = 0;
      ColumnResolver table = null;
      FullTextIndex index = null;
      using var cmd = Connection.CreateCommand(query);
      using var reader = cmd.ExecuteReader();
      while (reader.Read()) {
        ReadFullTextIndexColumnData(reader, context, ref currentTableId, ref table, ref index);
      }
    }

    protected virtual async Task ExtractFulltextIndexesAsync(ExtractionContext context, CancellationToken token)
    {
      var query = BuildExtractFullTextIndexesQuery(context);

      var currentTableId = 0;
      ColumnResolver table = null;
      FullTextIndex index = null;
      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadFullTextIndexColumnData(reader, context, ref currentTableId, ref table, ref index);
          }
        }
      }
    }

    private string BuildExtractFullTextIndexesQuery(ExtractionContext context)
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
      query = context.PerformReplacements(query);
      return query;
    }

    private void ReadFullTextIndexColumnData(DbDataReader reader, ExtractionContext context,
      ref int currentTableId, ref ColumnResolver table, ref FullTextIndex index)
    {
      var nextTableId = reader.GetInt32(1);
      if (currentTableId != nextTableId) {
        GetDataTable(nextTableId, context, ref currentTableId, ref table);
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
      if (typeInfo != null) {
        type = typeInfo.Type;
        typeName = null;
      }
      else {
        type = SqlType.Unknown;
      }

      int? precision = numericPrecision;
      int? scale = numericScale;

      if (typeInfo != null && typeInfo.MaxPrecision == null) {
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
        if (type == SqlType.VarChar) {
          type = SqlType.VarCharMax;
        }

        if (type == SqlType.VarBinary) {
          type = SqlType.VarBinaryMax;
        }
      }

      if (typeInfo != null) {
        if (typeInfo.MaxLength == null) {
          // resetting length for types that do not require specifying it
          size = null;
        }
        else if (size != null && size > 1) {
          if (type == SqlType.Char ||
            type == SqlType.VarChar ||
            type == SqlType.VarCharMax) {
            size /= 2;
          }
        }
      }

      return new SqlValueType(type, typeName, size, precision, scale);
    }

    private void GetDataTable(int id, ExtractionContext context, ref int currentId, ref ColumnResolver currentObj)
    {
      if (id == currentId) {
        return;
      }

      currentObj = context.ColumnResolverIndex[id];
      currentId = id;
    }

    private string MakeSchemaFilter(ExtractionContext context)
    {
      if (context.TargetSchemas.Count == 0) {
        return " > 0";
      }

      var builder = new StringBuilder();
      foreach (var targetSchemaName in context.TargetSchemas) {
        var schema = context.Catalog.Schemas[targetSchemaName];
        if (schema == null) {
          throw new InvalidOperationException("Cannot build schema filter because schemas haven't been extracted yet. Extract schemas first.");
        }
        if (!context.ReversedSchemaIndex.TryGetValue(schema, out var schemaId)) {
          continue;
        }

        _ = builder.Append(builder.Length == 0
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