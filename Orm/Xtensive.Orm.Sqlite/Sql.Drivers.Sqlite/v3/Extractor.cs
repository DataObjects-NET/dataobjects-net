// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Drivers.Sqlite.v3
{
  internal class Extractor : Model.Extractor
  {
    private struct ForeignKeyReaderState
    {
      public readonly Table ReferencingTable;
      public Table ReferencedTable;
      public ForeignKey ForeignKey;
      public int LastColumnIndex;

      public ForeignKeyReaderState(Table referencingTable) : this()
      {
        ReferencingTable = referencingTable;
        LastColumnIndex = int.MaxValue;
      }
    }

    public const string PrimaryKeyName = "PrimaryKey";

    internal const string DefaultSchemaName = "Main";

    private const string SqliteSequence = "sqlite_sequence";
    private const string SqliteMaster = "sqlite_master";

    /// <inheritdoc/>
    public override Catalog ExtractCatalog(string catalogName) =>
      ExtractSchemes(catalogName, new[] {DefaultSchemaName});

    /// <inheritdoc/>
    public override Task<Catalog> ExtractCatalogAsync(string catalogName, CancellationToken token = default) =>
      ExtractSchemesAsync(catalogName, new[] {DefaultSchemaName}, token);

    /// <inheritdoc/>
    public override Catalog ExtractSchemes(string catalogName, string[] schemaNames)
    {
      var catalog = new Catalog(catalogName);
      var schema = catalog.CreateSchema(schemaNames[0]);
      ExtractCatalogContents(schema);
      return catalog;
    }

    /// <inheritdoc/>
    public override async Task<Catalog> ExtractSchemesAsync(
      string catalogName, string[] schemaNames, CancellationToken token = default)
    {
      var catalog = new Catalog(catalogName);
      var schema = catalog.CreateSchema(schemaNames[0]);
      await ExtractCatalogContentsAsync(schema, token).ConfigureAwait(false);
      return catalog;
    }

    private void ExtractCatalogContents(Schema schema)
    {
      ExtractTables(schema);
      ExtractViews(schema);
      ExtractColumns(schema);
      ExtractIndexes(schema);
      ExtractForeignKeys(schema);
    }

    private async Task ExtractCatalogContentsAsync(Schema schema, CancellationToken token)
    {
      await ExtractTablesAsync(schema, token).ConfigureAwait(false);
      await ExtractViewsAsync(schema, token).ConfigureAwait(false);
      await ExtractColumnsAsync(schema, token).ConfigureAwait(false);
      await ExtractIndexesAsync(schema, token).ConfigureAwait(false);
      await ExtractForeignKeysAsync(schema, token).ConfigureAwait(false);
    }

    private const string ExtractTablesQuery =
      "SELECT [name] FROM [Main].[sqlite_master] WHERE type = 'table' AND name NOT LIKE 'sqlite?_%' ESCAPE '?'";

    private const string ExtractViewsQuery =
      "SELECT [name], sql FROM [Main].[sqlite_master] WHERE type = 'view' AND name NOT LIKE 'sqlite?_%' ESCAPE '?'";

    private void ExtractTables(Schema schema)
    {
      using var cmd = Connection.CreateCommand(ExtractTablesQuery);
      using var reader = cmd.ExecuteReader();
      while (reader.Read()) {
        _ = schema.CreateTable(reader.GetString(0));
      }
    }

    private async Task ExtractTablesAsync(Schema schema, CancellationToken token)
    {
      var cmd = Connection.CreateCommand(ExtractTablesQuery);
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            _ = schema.CreateTable(reader.GetString(0));
          }
        }
      }
    }

    private static string BuildTableExistenceCheckQuery(string tableName) =>
      $"SELECT name FROM {SqliteMaster} WHERE type = 'table' AND name='{tableName}'";

    private bool DoesTableExist(string tableName)
    {
      var select = BuildTableExistenceCheckQuery(tableName);
      using var cmd = Connection.CreateCommand(select);
      using IDataReader reader = cmd.ExecuteReader();
      return reader.Read();
    }

    private async Task<bool> DoesTableExistAsync(string tableName, CancellationToken token)
    {
      var select = BuildTableExistenceCheckQuery(tableName);
      var cmd = Connection.CreateCommand(select);
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          return await reader.ReadAsync(token).ConfigureAwait(false);
        }
      }
    }

    private static string BuildIncrementValueQuery(string tableName) =>
      $"SELECT seq from {SqliteSequence} WHERE name = '{tableName}' ";

    private int? GetIncrementValue(string tableName)
    {
      if (!DoesTableExist(SqliteSequence)) {
        return null;
      }

      var select = BuildIncrementValueQuery(tableName);
      using var cmd = Connection.CreateCommand(select);
      using IDataReader reader = cmd.ExecuteReader();
      while (reader.Read()) {
        return ReadNullableInt(reader, "seq");
      }

      return null;
    }

    private async Task<int?> GetIncrementValueAsync(string tableName, CancellationToken token)
    {
      if (!await DoesTableExistAsync(SqliteSequence, token).ConfigureAwait(false)) {
        return null;
      }

      var select = BuildIncrementValueQuery(tableName);
      var cmd = Connection.CreateCommand(select);
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            return ReadNullableInt(reader, "seq");
          }

          return null;
        }
      }
    }

    private static string BuildExtractTableColumnsQuery(string tableName) => $"PRAGMA table_info([{tableName}])";

    private void ExtractColumns(Schema schema)
    {
      foreach (var table in schema.Tables) {
        var primaryKeyItems = new Dictionary<int, TableColumn>();
        var select = BuildExtractTableColumnsQuery(table.Name);
        using var cmd = Connection.CreateCommand(select);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
          ReadTableColumnData(reader, table, primaryKeyItems);
        }

        if (primaryKeyItems.Count > 0) {
          // Auto Increment
          var incrementValue = GetIncrementValue(table.Name);
          CreateTablePrimaryKey(table, primaryKeyItems, incrementValue);
        }
      }
    }

    private async Task ExtractColumnsAsync(Schema schema, CancellationToken token)
    {
      foreach (var table in schema.Tables) {
        var primaryKeyItems = new Dictionary<int, TableColumn>();
        var select = BuildExtractTableColumnsQuery(table.Name);
        var cmd = Connection.CreateCommand(select);
        await using (cmd.ConfigureAwait(false)) {
          var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
          await using (reader.ConfigureAwait(false)) {
            while (await reader.ReadAsync(token).ConfigureAwait(false)) {
              ReadTableColumnData(reader, table, primaryKeyItems);
            }
          }
        }

        if (primaryKeyItems.Count > 0) {
          // Auto Increment
          var incrementValue = await GetIncrementValueAsync(table.Name, token).ConfigureAwait(false);
          CreateTablePrimaryKey(table, primaryKeyItems, incrementValue);
        }
      }
    }

    private void ReadTableColumnData(IDataReader reader, Table table, Dictionary<int, TableColumn> primaryKeyItems)
    {
      // Column Name
      var tableColumn = table.CreateColumn(reader.GetString(1));

      // Column Type
      tableColumn.DataType = ParseValueType(reader.GetString(2));

      // IsNullable
      tableColumn.IsNullable = ReadInt(reader, 3) == 0;

      // Default Value
      var defaultValue = ReadStringOrNull(reader, 4);
      if (!string.IsNullOrEmpty(defaultValue)
        && !string.Equals("NULL", defaultValue, StringComparison.OrdinalIgnoreCase)) {
        tableColumn.DefaultValue = defaultValue;
      }

      var primaryKeyPosition = ReadInt(reader, 5);
      if (primaryKeyPosition > 0) {
        primaryKeyItems.Add(primaryKeyPosition, tableColumn);
      }
    }

    private static void CreateTablePrimaryKey(
      Table table, Dictionary<int, TableColumn> primaryKeyItems, int? incrementValue)
    {
      if (primaryKeyItems.TryGetValue(1, out var tableColumn)) {
        if (incrementValue != null) {
          tableColumn.SequenceDescriptor = new SequenceDescriptor(tableColumn, incrementValue, 1);
        }
      }

      table.CreatePrimaryKey(PrimaryKeyName,
        primaryKeyItems.OrderBy(i => i.Key).Select(i => i.Value).ToArray());
    }

    private void ExtractViews(Schema schema)
    {
      using var reader = ExecuteReader(ExtractViewsQuery);
      while (reader.Read()) {
        ReadViewData(reader, schema);
      }
    }

    private async Task ExtractViewsAsync(Schema schema, CancellationToken token)
    {
      var reader = await ExecuteReaderAsync(ExtractViewsQuery, token).ConfigureAwait(false);
      await using (reader.ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadViewData(reader, schema);
        }
      }
    }

    private void ReadViewData(DbDataReader reader, Schema schema)
    {
      var view = reader.GetString(0);
      var definition = ReadStringOrNull(reader, 1);
      if (string.IsNullOrEmpty(definition)) {
        _ = schema.CreateView(view);
      }
      else {
        _ = schema.CreateView(view, SqlDml.Native(definition));
      }
    }

    private static string BuildExtractIndexQuery(string tableName) => $"PRAGMA index_list([{tableName}])";

    private void ExtractIndexes(Schema schema)
    {
      foreach (var table in schema.Tables) {
        var query = BuildExtractIndexQuery(table.Name);
        using var cmd = Connection.CreateCommand(query);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
          if (ReadIndexData(reader, table, out var index)) {
            ExtractIndexColumns(table, index);
          }
        }
      }
    }

    private async Task ExtractIndexesAsync(Schema schema, CancellationToken token)
    {
      foreach (var table in schema.Tables) {
        var query = BuildExtractIndexQuery(table.Name);
        var cmd = Connection.CreateCommand(query);
        await using (cmd.ConfigureAwait(false)) {
          var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
          await using (reader.ConfigureAwait(false)) {
            while (await reader.ReadAsync(token).ConfigureAwait(false)) {
              if (ReadIndexData(reader, table, out var index)) {
                await ExtractIndexColumnsAsync(table, index, token).ConfigureAwait(false);
              }
            }
          }
        }
      }
    }

    private static bool ReadIndexData(IDataReader reader, Table table, out Index index)
    {
      index = null;
      var indexName = ReadStringOrNull(reader, 1);
      if (indexName.StartsWith("sqlite_autoindex_", StringComparison.Ordinal)) {
        // Special index used for primary keys
        // It should be hidden here, because PK are already extracted in ExtractColumns()
        return false;
      }

      var unique = reader.GetBoolean(2);
      index = table.CreateIndex(indexName);
      index.IsUnique = unique;
      return true;
    }

    private static string BuildExtractIndexColumnsQuery(string indexName) => $"PRAGMA index_info([{indexName}])";

    private void ExtractIndexColumns(Table table, Index index)
    {
      var query = BuildExtractIndexColumnsQuery(index.Name);
      using var cmd = Connection.CreateCommand(query);
      using IDataReader reader = cmd.ExecuteReader();
      while (reader.Read()) {
        index.CreateIndexColumn(table.TableColumns[ReadStringOrNull(reader, 2)]);
      }
    }

    private async Task ExtractIndexColumnsAsync(Table table, Index index, CancellationToken token)
    {
      var query = BuildExtractIndexColumnsQuery(index.Name);
      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            index.CreateIndexColumn(table.TableColumns[ReadStringOrNull(reader, 2)]);
          }
        }
      }
    }

    private static string BuildExtractForeignKeysQuery(string tableName) => $"PRAGMA foreign_key_list([{tableName}])";

    private void ExtractForeignKeys(Schema schema)
    {
      // PRAGMA foreign_key_list - retuns column list for all foreign key
      // row structure:
      // 0 - id (numeric value, identifier, unique across foreign keys but non-unique across results)
      // 1 - seq (numeric value, describes order within foreign key)
      // 2 - table (string value, foreign table name)
      // 3 - from (string value, local column name)
      // 4 - to (string value, referenced column)
      // 5 - on_update (string value, action on update)
      // 6 - on_delete (string value, action on delete)
      // 7 - match (string value, always NONE)

      foreach (var table in schema.Tables) {
        var query = BuildExtractForeignKeysQuery(table.Name);

        var state = new ForeignKeyReaderState(table);
        using var cmd = Connection.CreateCommand(query);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
          ReadForeignKeyColumnData(reader, table, ref state);
        }
      }
    }

    private async Task ExtractForeignKeysAsync(Schema schema,CancellationToken token)
    {
      foreach (var table in schema.Tables) {
        var query = BuildExtractForeignKeysQuery(table.Name);

        var state = new ForeignKeyReaderState(table);
        var cmd = Connection.CreateCommand(query);
        await using (cmd.ConfigureAwait(false)) {
          var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
          await using (reader.ConfigureAwait(false)) {
            while (await reader.ReadAsync(token).ConfigureAwait(false)) {
              ReadForeignKeyColumnData(reader, table, ref state);
            }
          }
        }
      }
    }

    private static void ReadForeignKeyColumnData(DbDataReader reader, Table table, ref ForeignKeyReaderState state)
    {
      var columnIndex = ReadInt(reader, 1);
      if (columnIndex <= state.LastColumnIndex) {
        var foreignKeyName = string.Format(
          CultureInfo.InvariantCulture, "FK_{0}_{1}", state.ReferencingTable.Name, ReadStringOrNull(reader, 2));

        state.ForeignKey = state.ReferencingTable.CreateForeignKey(foreignKeyName);

        ReadCascadeAction(state.ForeignKey, reader, 6);
        var referencedSchema = table.Schema; //Schema same as current
        var referencedTable = referencedSchema.Tables[ReadStringOrNull(reader, 2)];
        state.ReferencedTable = referencedTable;
        state.ForeignKey.ReferencedTable = referencedTable;
      }

      var referencingColumn = state.ReferencingTable.TableColumns[reader.GetString(3)];
      var referencedColumn = state.ReferencedTable.TableColumns[reader.GetString(4)];
      state.ForeignKey.Columns.Add(referencingColumn);
      state.ForeignKey.ReferencedColumns.Add(referencedColumn);
      state.LastColumnIndex = columnIndex;
    }

    private SqlValueType ParseValueType(string typeDefinition)
    {
      var typeName = ParseTypeName(typeDefinition);

      // First try predefined names first
      var typeInfo = Driver.ServerInfo.DataTypes[typeName];
      if (typeInfo!=null) {
        return new SqlValueType(typeInfo.Type);
      }

      // If it didn't succeed use generic matching algorithm
      // (rules are taken from sqlite docs)

      // (1) If the declared type contains the string "INT" then it is assigned INTEGER affinity.
      if (typeName.Contains("int")) {
        return new SqlValueType(SqlType.Int64);
      }

      // (2) If the declared type of the column contains any of the strings "CHAR", "CLOB", or "TEXT"
      // then that column has TEXT affinity.
      if (typeName.Contains("char") || typeName.Contains("clob") || typeName.Contains("text")) {
        return new SqlValueType(SqlType.VarCharMax);
      }

      // (3) If the declared type for a column contains the string "BLOB"
      // or if no type is specified then the column has affinity NONE.
      if (typeName.Contains("blob") || typeName==string.Empty) {
        return new SqlValueType(SqlType.VarBinaryMax);
      }

      // (4) If the declared type for a column contains any of the strings
      // "REAL", "FLOA", or "DOUB" then the column has REAL affinity.
      if (typeName.Contains("real") || typeName.Contains("floa") || typeName.Contains("doub")) {
        return new SqlValueType(SqlType.Double);
      }

      // (5) Otherwise, the affinity is NUMERIC.
      return new SqlValueType(SqlType.Decimal);
    }

    private static string ParseTypeName(string typeDefinition)
    {
      var result = typeDefinition
        .SkipWhile(char.IsWhiteSpace)
        .TakeWhile(ch => char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
        .ToArray();
      return new string(result).ToLowerInvariant();
    }

    private static int ReadInt(IDataRecord row, int index)
    {
      var value = row.GetDecimal(index);
      return value > int.MaxValue ? int.MaxValue : (int) value;
    }

    private static string ReadStringOrNull(IDataRecord row, int index) =>
      row.IsDBNull(index) ? null : row.GetString(index);

    private static int? ReadNullableInt(IDataRecord reader, string column) =>
      Convert.IsDBNull(reader[column]) ? null : (int?) Convert.ToInt32(reader[column]);

    private static void ReadCascadeAction(ForeignKey foreignKey, IDataRecord row, int deleteRuleIndex)
    {
      var deleteRule = row.GetString(deleteRuleIndex);
      switch (deleteRule) {
      case "CASCADE":
        foreignKey.OnDelete = ReferentialAction.Cascade;
        return;
      case "SET NULL":
        foreignKey.OnDelete = ReferentialAction.SetNull;
        return;
      case "NO ACTION":
        foreignKey.OnDelete = ReferentialAction.NoAction;
        return;
      }
    }

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
