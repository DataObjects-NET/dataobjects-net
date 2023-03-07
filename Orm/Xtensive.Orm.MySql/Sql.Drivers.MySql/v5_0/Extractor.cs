// Copyright (C) 2011-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Sql.Drivers.MySql.Resources;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Drivers.MySql.v5_0
{
  internal partial class Extractor : Model.Extractor
  {
    private struct ColumnReaderState<TOwner>
    {
      public readonly Catalog Catalog;
      public Schema Schema;
      public TOwner Owner;
      public int LastColumnIndex;

      public ColumnReaderState(Catalog catalog) : this()
      {
        Catalog = catalog;
        LastColumnIndex = int.MaxValue;
      }
    }

    private struct IndexColumnReaderState
    {
      public readonly Catalog Catalog;
      public Table Table;
      public Index Index;
      public int LastColumnIndex;

      public IndexColumnReaderState(Catalog catalog) : this()
      {
        Catalog = catalog;
        LastColumnIndex = int.MaxValue;
      }
    }

    private struct ForeignKeyReaderState
    {
      public readonly Catalog Catalog;
      public Table ReferencingTable;
      public Table ReferencedTable;
      public ForeignKey ForeignKey;
      public int LastColumnIndex;

      public ForeignKeyReaderState(Catalog catalog) : this()
      {
        Catalog = catalog;
        LastColumnIndex = int.MaxValue;
      }
    }

    private struct IndexBasedConstraintReaderState
    {
      public readonly Catalog Catalog;
      public readonly List<TableColumn> Columns;
      public Table Table;
      public string ConstraintName;
      public string ConstraintType;
      public int LastColumnIndex;

      public IndexBasedConstraintReaderState(Catalog catalog) : this()
      {
        Catalog = catalog;
        Columns = new List<TableColumn>();
        LastColumnIndex = -1;
      }
    }

    private class ExtractionContext
    {
      private readonly Dictionary<string, string> replacementsRegistry;

      public readonly Catalog Catalog;

      public string PerformReplacements(string query)
      {
        foreach (var registry in replacementsRegistry) {
          query = query.Replace(registry.Key, registry.Value);
        }
        return query;
      }

      private static void RegisterReplacements(string targetSchema, Dictionary<string, string> replacements)
      {
        var schemaFilter = targetSchema != null
          ? "= " + SqlHelper.QuoteString(targetSchema)
          : "NOT IN ('INFORMATION_SCHEMA', 'MYSQL', 'PERFORMANCE_SCHEMA')";

        replacements[SchemaFilterPlaceholder] = schemaFilter;
        replacements[IndexesFilterPlaceholder] = "1 > 0";
        replacements[TableFilterPlaceholder] = "IS NOT NULL";
      }

      public ExtractionContext(Catalog catalog, string targetSchema)
      {
        Catalog = catalog;
        replacementsRegistry = new Dictionary<string, string>();
        RegisterReplacements(targetSchema, replacementsRegistry);
      }
    }

    private const int DefaultPrecision = 38;
    private const int DefaultScale = 0;

    /// <inheritdoc/>
    public override Catalog ExtractCatalog(string catalogName) =>
      ExtractSchemes(catalogName, new[] { Driver.CoreServerInfo.DefaultSchemaName });

    /// <inheritdoc/>
    public override Task<Catalog> ExtractCatalogAsync(string catalogName, CancellationToken token = default) =>
      ExtractSchemesAsync(catalogName, new[] { Driver.CoreServerInfo.DefaultSchemaName }, token);

    /// <inheritdoc/>
    public override Catalog ExtractSchemes(string catalogName, string[] schemaNames)
    {
      var context = CreateContext(catalogName, schemaNames);
      ExtractCatalogContents(context);
      return context.Catalog;
    }

    /// <inheritdoc/>
    public override async Task<Catalog> ExtractSchemesAsync(
      string catalogName, string[] schemaNames, CancellationToken token = default)
    {
      var context = CreateContext(catalogName, schemaNames);
      await ExtractCatalogContentsAsync(context, token).ConfigureAwait(false);
      return context.Catalog;
    }

    private ExtractionContext CreateContext(string catalogName, string[] schemaNames)
    {
      var catalog = new Catalog(catalogName);
      if (schemaNames.Length == 0) {
        return new ExtractionContext(catalog, null);
      }
      else {
        var targetSchema = schemaNames[0];
        _ = catalog.CreateSchema(targetSchema);
        return new ExtractionContext(catalog, targetSchema);
      }
    }

    private void ExtractCatalogContents(ExtractionContext context)
    {
      ExtractTables(context);
      ExtractTableColumns(context);
      ExtractViews(context);
      ExtractViewColumns(context);
      ExtractIndexes(context);
      ExtractForeignKeys(context);
      ExtractCheckConstraints(context);
      ExtractUniqueAndPrimaryKeyConstraints(context);
    }

    private async Task ExtractCatalogContentsAsync(ExtractionContext context, CancellationToken token)
    {
      await ExtractTablesAsync(context, token).ConfigureAwait(false);
      await ExtractTableColumnsAsync(context, token).ConfigureAwait(false);
      await ExtractViewsAsync(context, token).ConfigureAwait(false);
      await ExtractViewColumnsAsync(context, token).ConfigureAwait(false);
      await ExtractIndexesAsync(context, token).ConfigureAwait(false);
      await ExtractForeignKeysAsync(context, token).ConfigureAwait(false);
      await ExtractCheckConstraintsAsync(context, token).ConfigureAwait(false);
      await ExtractUniqueAndPrimaryKeyConstraintsAsync(context, token).ConfigureAwait(false);
    }

    private void ExtractTables(ExtractionContext context)
    {
      var query = context.PerformReplacements(GetExtractTablesQuery());
      using var command = Connection.CreateCommand(query);
      using var reader = command.ExecuteReader();
      while (reader.Read()) {
        ReadTableData(reader, context.Catalog);
      }
    }

    private async Task ExtractTablesAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractTablesQuery());
      var command = Connection.CreateCommand(query);
      await using (command.ConfigureAwait(false)) {
        var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadTableData(reader, context.Catalog);
          }
        }
      }
    }

    private void ExtractTableColumns(ExtractionContext context)
    {
      var query = context.PerformReplacements(GetExtractTableColumnsQuery());
      using var command = Connection.CreateCommand(query);
      using var reader = command.ExecuteReader();
      var state = new ColumnReaderState<Table>(context.Catalog);
      while (reader.Read()) {
        ReadTableColumnData(reader, ref state);
      }
    }

    private async Task ExtractTableColumnsAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractTableColumnsQuery());
      var command = Connection.CreateCommand(query);
      await using (command.ConfigureAwait(false)) {
        var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          var state = new ColumnReaderState<Table>(context.Catalog);
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadTableColumnData(reader, ref state);
          }
        }
      }
    }

    private void ExtractViews(ExtractionContext context)
    {
      var query = context.PerformReplacements(GetExtractViewsQuery());
      using var command = Connection.CreateCommand(query);
      using var reader = command.ExecuteReader();
      while (reader.Read()) {
        ReadViewData(reader, context.Catalog);
      }
    }

    private async Task ExtractViewsAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractViewsQuery());
      var command = Connection.CreateCommand(query);
      await using (command.ConfigureAwait(false)) {
        var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadViewData(reader, context.Catalog);
          }
        }
      }
    }

    private void ExtractViewColumns(ExtractionContext context)
    {
      var query = context.PerformReplacements(GetExtractViewColumnsQuery());
      using var command = Connection.CreateCommand(query);
      using var reader = command.ExecuteReader();
      var state = new ColumnReaderState<View>(context.Catalog);
      while (reader.Read()) {
        ReadViewColumnData(reader, ref state);
      }
    }

    private async Task ExtractViewColumnsAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractViewColumnsQuery());
      var command = Connection.CreateCommand(query);
      await using (command.ConfigureAwait(false)) {
        var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          var state = new ColumnReaderState<View>(context.Catalog);
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadViewColumnData(reader, ref state);
          }
        }
      }
    }

    private void ExtractIndexes(ExtractionContext context)
    {
      var query = context.PerformReplacements(GetExtractIndexesQuery());
      using var command = Connection.CreateCommand(query);
      using var reader = command.ExecuteReader();
      var state = new IndexColumnReaderState(context.Catalog);
      while (reader.Read()) {
        ReadIndexColumnData(reader, ref state);
      }
    }

    private async Task ExtractIndexesAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractIndexesQuery());
      var command = Connection.CreateCommand(query);
      await using (command.ConfigureAwait(false)) {
        var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          var state = new IndexColumnReaderState(context.Catalog);
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadIndexColumnData(reader, ref state);
          }
        }
      }
    }

    private void ExtractForeignKeys(ExtractionContext context)
    {
      var query = context.PerformReplacements(GetExtractForeignKeysQuery());
      using var command = Connection.CreateCommand(query);
      using var reader = command.ExecuteReader();
      var state = new ForeignKeyReaderState(context.Catalog);
      while (reader.Read()) {
        ReadForeignKeyColumnData(reader, ref state);
      }
    }

    private async Task ExtractForeignKeysAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractForeignKeysQuery());
      var command = Connection.CreateCommand(query);
      await using (command.ConfigureAwait(false)) {
        var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          var state = new ForeignKeyReaderState(context.Catalog);
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadForeignKeyColumnData(reader, ref state);
          }
        }
      }
    }

    private void ExtractUniqueAndPrimaryKeyConstraints(ExtractionContext context)
    {
      var query = context.PerformReplacements(GetExtractUniqueAndPrimaryKeyConstraintsQuery());
      using var command = Connection.CreateCommand(query);
      using var reader = command.ExecuteReader();
      var state = new IndexBasedConstraintReaderState(context.Catalog);
      bool readingCompleted;
      do {
        readingCompleted = !reader.Read();
        ReadIndexBasedConstraintData(reader, ref state, readingCompleted);
      } while (!readingCompleted);
    }

    private async Task ExtractUniqueAndPrimaryKeyConstraintsAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractUniqueAndPrimaryKeyConstraintsQuery());
      var command = Connection.CreateCommand(query);
      await using (command.ConfigureAwait(false)) {
        var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          var state = new IndexBasedConstraintReaderState(context.Catalog);
          bool readingCompleted;
          do {
            readingCompleted = !await reader.ReadAsync(token).ConfigureAwait(false);
            ReadIndexBasedConstraintData(reader, ref state, readingCompleted);
          } while (!readingCompleted);
        }
      }
    }

    //--- ExtractCheckConstraints
    //  0   -   constraint_schema,
    //  1   -   constraint_name,
    //  2   -   table_schema,
    //  3   -   table_name,
    //  4   -   constraint_type
    private static void ExtractCheckConstraints(ExtractionContext context)
    {
      #region Commented Code

      //NOT yet supported!
      //using (var reader = ExecuteReader(GetExtractCheckConstraintsQuery()))
      //{
      //    while (reader.Read())
      //    {
      //        var schema = theCatalog.Schemas[reader.GetString(0)];
      //        var table = schema.Tables[reader.GetString(3)];
      //        var name = reader.GetString(1);

      //        // It returns empty string instead of the actual value.
      //        var condition = string.IsNullOrEmpty(reader.GetString(3))
      //          ? null
      //          : SqlDml.Native(reader.GetString(3));
      //        var constraint = table.CreateCheckConstraint(name, condition);
      //        // ReadConstraintProperties(constraint, reader, 4, 5);
      //    }
      //}

      #endregion
    }

    private static Task ExtractCheckConstraintsAsync(ExtractionContext context, CancellationToken token) => Task.CompletedTask;

    // ---- ReadTableData
    //  0   table_schema,
    //  1   table_name,
    //  2   table_type
    private static void ReadTableData(DbDataReader reader, Catalog catalog)
    {
      var schemaName = reader.GetString(0);
      var schema = catalog.Schemas[schemaName];
      var tableName = reader.GetString(1);
      _ = schema.CreateTable(tableName);
    }

    // ---- ReadTableColumnData
    //    0     table_schema
    //    1     table_name
    //    2     ordinal_position
    //    3     column_name
    //    4     data_type
    //    5     is_nullable
    //    6     column_type
    //    7     character_maximum_length
    //    8     numeric_precision
    //    9     numeric_scale
    //   10    collation_name
    //   11    column_key
    //   12    column_default
    //   13    Extra
    private void ReadTableColumnData(DbDataReader reader, ref ColumnReaderState<Table> state)
    {
      var columnIndex = ReadInt(reader, 2);
      if (columnIndex <= state.LastColumnIndex) {
        //Schema
        state.Schema = state.Catalog.Schemas[reader.GetString(0)];

        //Table
        state.Owner = state.Schema.Tables[reader.GetString(1)];
      }

      //Column
      var column = state.Owner.CreateColumn(reader.GetString(3));

      //Collation
      var collationName = ReadStringOrNull(reader, 10);
      if (!string.IsNullOrEmpty(collationName)) {
        column.Collation = state.Schema.Collations[collationName] ?? state.Schema.CreateCollation(collationName);
      }

      //Data type
      column.DataType = CreateValueType(reader, 4, 8, 9, 7);

      //Nullable
      column.IsNullable = ReadBool(reader, 5);

      //Default
      var defaultValue = ReadStringOrNull(reader, 12);
      if (!string.IsNullOrEmpty(defaultValue)) {
        column.DefaultValue = SqlDml.Native(defaultValue);
      }

      // AutoIncrement
      if (ReadIfAutoIncrement(reader, 13)) {
        column.SequenceDescriptor = new SequenceDescriptor(column, ReadAutoIncrementValue(reader, 14), 1);
      }

      //Column number.
      state.LastColumnIndex = columnIndex;
    }

    //---- ReadViewData
    //   0      table_schema,
    //   1      table_name,
    //   2      view_definition
    private static void ReadViewData(DbDataReader reader, Catalog catalog)
    {
      var schema = catalog.Schemas[reader.GetString(0)];
      var view = reader.GetString(1);
      var definition = ReadStringOrNull(reader, 2);
      _ = string.IsNullOrEmpty(definition)
        ? schema.CreateView(view)
        : schema.CreateView(view, SqlDml.Native(definition));
    }

    //---- ReadViewColumnData
    //   0      table_schema,
    //   1      table_name,
    //   2      column_name,
    //   3      ordinal_position,
    //   4      view_definition
    private static void ReadViewColumnData(DbDataReader reader, ref ColumnReaderState<View> state)
    {
      var columnIndex = ReadInt(reader, 3);
      if (columnIndex <= state.LastColumnIndex) {
        var schema = state.Catalog.Schemas[reader.GetString(0)];
        state.Owner = schema.Views[reader.GetString(1)];
      }

      _ = state.Owner.CreateColumn(reader.GetString(2));
      state.LastColumnIndex = columnIndex;
    }

    //---- ReadIndexColumnData
    //  0       table_schema,
    //  1       table_name,
    //  2       index_name,
    //  3       non_unique,
    //  4       index_type,
    //  5       seq_in_index,
    //  6       column_name,
    //  7       cardinality,
    //  8       sub_part,
    //  9       nullable
    private static void ReadIndexColumnData(DbDataReader reader, ref IndexColumnReaderState state)
    {
      var columnIndex = ReadInt(reader, 5);
      if (columnIndex <= state.LastColumnIndex) {
        var schema = state.Catalog.Schemas[reader.GetString(0)];
        state.Table = schema.Tables[reader.GetString(1)];
        if (IsFullTextIndex(reader, 4)) {
          _ = state.Table.CreateFullTextIndex(reader.GetString(2));
        }
        else {
          state.Index = state.Table.CreateIndex(reader.GetString(2));
          state.Index.IsUnique = reader.GetInt32(3) == 0;
        }
      }

      var column = state.Table.TableColumns[reader.GetString(6)];
      _ = state.Index.CreateIndexColumn(column);

      state.LastColumnIndex = columnIndex;
    }

    //----  ReadForeignKeyColumnData
    //  0       constraint_schema,
    //  1       table_name,
    //  2       constraint_name,
    //  3       delete_rule,
    //  4       update_rule
    //  5       column_name,
    //  6       ordinal_position,
    //  7       referenced_table_schema,
    //  8       referenced_table_name,
    //  9       referenced_column_name
    private static void ReadForeignKeyColumnData(DbDataReader reader, ref ForeignKeyReaderState state)
    {
      var columnIndex = ReadInt(reader, 6);
      if (columnIndex <= state.LastColumnIndex) {
        var referencingSchema = state.Catalog.Schemas[reader.GetString(0)];
        state.ReferencingTable = referencingSchema.Tables[reader.GetString(1)];
        state.ForeignKey = state.ReferencingTable.CreateForeignKey(reader.GetString(2));
        ReadForeignKeyActions(state.ForeignKey, reader, 3, 4);
        var referencedSchema = state.Catalog.Schemas[reader.GetString(0)]; //Schema same as current
        state.ReferencedTable = referencedSchema.Tables[reader.GetString(8)];
        state.ForeignKey.ReferencedTable = state.ReferencedTable;
      }

      var referencingColumn = state.ReferencingTable.TableColumns[reader.GetString(5)];
      var referencedColumn = state.ReferencedTable.TableColumns[reader.GetString(9)];
      state.ForeignKey.Columns.Add(referencingColumn);
      state.ForeignKey.ReferencedColumns.Add(referencedColumn);
      state.LastColumnIndex = columnIndex;
    }

    //---- ReadIndexBasedConstraintData
    //   0      constraint_schema,
    //   1      table_name,
    //   2      constraint_name,
    //  3       constraint_type,
    //  4       column_name,
    //  5       ordinal_position
    private static void ReadIndexBasedConstraintData(DbDataReader reader, ref IndexBasedConstraintReaderState state,
      bool readingCompleted)
    {
      if (readingCompleted) {
        if (state.Columns.Count > 0) {
          CreateIndexBasedConstraint(ref state);
        }
      }
      else {
        var columnIndex = ReadInt(reader, 5);
        if (columnIndex <= state.LastColumnIndex) {
          CreateIndexBasedConstraint(ref state);
          state.Columns.Clear();
        }

        if (state.Columns.Count == 0) {
          var schemaName = reader.GetString(0);
          var tableName = reader.GetString(1);
          var schema = state.Catalog.Schemas[schemaName];
          if (schema == null) {
            throw new InvalidOperationException($"Schema '{schemaName}' is not found");
          }

          state.Table = schema.Tables[tableName];
          if (state.Table == null) {
            throw new InvalidOperationException($"Table '{tableName}' is not found in schema '{schemaName}'");
          }

          state.ConstraintName = reader.GetString(2);
          state.ConstraintType = reader.GetString(3);
        }

        state.Columns.Add(state.Table.TableColumns[reader.GetString(4)]);
        state.LastColumnIndex = columnIndex;
      }
    }

    private SqlValueType CreateValueType(IDataRecord row,
      int typeNameIndex, int precisionIndex, int scaleIndex, int charLengthIndex)
    {
      var typeName = row.GetString(typeNameIndex).ToUpperInvariant();
      var dataTypeName = row.GetString(6).ToUpperInvariant();

      var precision = row.IsDBNull(precisionIndex) ? DefaultPrecision : ReadInt(row, precisionIndex);
      var scale = row.IsDBNull(scaleIndex) ? DefaultScale : ReadInt(row, scaleIndex);

      var decimalAssignedNames = new List<string>() { "DECIMAL", "NUMERIC", "NUMBER" };
      var doubleAssignedNames = new List<string>() { "DOUBLE", "REAL" };

      if (decimalAssignedNames.Contains(typeName)) {
        return new SqlValueType(SqlType.Decimal, precision, scale);
      }

      if(doubleAssignedNames.Contains(typeName)) {
        return new SqlValueType(SqlType.Double);
      }

      if (dataTypeName.Equals("TINYINT(1)", StringComparison.Ordinal)) {
        return new SqlValueType(SqlType.Boolean);
      }

      if (typeName.StartsWith("TINYINT", StringComparison.Ordinal)) {
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Int8);
      }

      if (typeName.StartsWith("SMALLINT", StringComparison.Ordinal)) {
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Int16);
      }

      if (typeName.StartsWith("MEDIUMINT", StringComparison.Ordinal)) {
        // There is not 34bit Int in SqlType
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Int32);
      }

      if (typeName.StartsWith("INT", StringComparison.Ordinal)) {
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Int32);
      }

      if (typeName.StartsWith("BIGINT", StringComparison.Ordinal)) {
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Int64);
      }
#if NET6_0_OR_GREATER
      if (typeName.Equals("TIME", StringComparison.Ordinal) || typeName.StartsWith("TIME(")) {
        return new SqlValueType(SqlType.Time);
      }
      else if (typeName.StartsWith("TIME", StringComparison.Ordinal)) {
        // "timestamp precision" is saved as "scale", ignoring too
        return new SqlValueType(SqlType.DateTime);
      }
#else
      if (typeName.StartsWith("TIME", StringComparison.Ordinal)) {
        // "timestamp precision" is saved as "scale", ignoring too
        return new SqlValueType(SqlType.DateTime);
      }
#endif
      if (typeName.StartsWith("YEAR", StringComparison.Ordinal)) {
        // "timestamp precision" is saved as "scale", ignoring too
        return new SqlValueType(SqlType.Decimal, 4, 0);
      }

      if (typeName.Equals("LONGTEXT", StringComparison.Ordinal)) {
        return new SqlValueType(SqlType.VarCharMax);
      }

      if (typeName.Contains("TEXT", StringComparison.Ordinal)) {
        var length = ReadInt(row, charLengthIndex);
        return new SqlValueType(SqlType.VarCharMax, length);
      }

      if (typeName.Contains("BLOB", StringComparison.Ordinal)) {
        return new SqlValueType(SqlType.VarBinaryMax);
      }

      if (typeName.Equals("BINARY", StringComparison.Ordinal)) {
        var length = ReadInt(row, charLengthIndex);
        return new SqlValueType(SqlType.Binary, length);
      }
      if (typeName.Equals("VARBINARY", StringComparison.Ordinal)) {
        var length = ReadInt(row, charLengthIndex);
        
        return new SqlValueType(SqlType.VarBinary, length);
      }

      if (typeName.Equals("VARCHAR", StringComparison.Ordinal)
        || typeName.Equals("CHAR", StringComparison.Ordinal)) {
        var length = Convert.ToInt32(row[charLengthIndex]);
        var sqlType = typeName.Length==4 ? SqlType.Char : SqlType.VarChar;
        return new SqlValueType(sqlType, length);
      }
      var typeInfo = Driver.ServerInfo.DataTypes[typeName];
      return typeInfo != null
        ? new SqlValueType(typeInfo.Type)
        : new SqlValueType(typeName);
    }

    private static void CreateIndexBasedConstraint(ref IndexBasedConstraintReaderState state)
    {
      switch (state.ConstraintType) {
        case "PRIMARY KEY":
          _ = state.Table.CreatePrimaryKey(state.ConstraintName, state.Columns.ToArray());
          return;
        case "UNIQUE":
          _ = state.Table.CreateUniqueConstraint(state.ConstraintName, state.Columns.ToArray());
          return;
        default:
          throw new ArgumentOutOfRangeException(nameof(IndexBasedConstraintReaderState.ConstraintType));
      }
    }

    private static bool ReadBool(IDataRecord row, int index)
    {
      var value = row.GetString(index);
      return value switch {
        "Y" or "YES" or "ENABLED" or "UNIQUE" => true,
        "N" or "NO" or "DISABLED" or "NONUNIQUE" => false,
        _ => throw new ArgumentOutOfRangeException(string.Format(Strings.ExInvalidBooleanStringX, value)),
      };
    }

    private static bool IsFullTextIndex(IDataRecord row, int index) =>
      ReadStringOrNull(row, index).Equals("FULLTEXT", StringComparison.OrdinalIgnoreCase);

    private static bool ReadIfAutoIncrement(IDataRecord row, int index) =>
      ReadStringOrNull(row, index).Equals("AUTO_INCREMENT", StringComparison.OrdinalIgnoreCase);

    private static int ReadAutoIncrementValue(IDataRecord row, int index) =>
      row.IsDBNull(index) ? 1 : ReadInt(row, index);

    private static int ReadInt(IDataRecord row, int index)
    {
      var value = row.GetDecimal(index);
      return value > int.MaxValue ? int.MaxValue : (int) value;
    }

    private static string ReadStringOrNull(IDataRecord row, int index) =>
      row.IsDBNull(index) ? null : row.GetString(index);

    private static void ReadForeignKeyActions(ForeignKey foreignKey, IDataRecord row, int deleteRuleIndex, int updateRuleIndex)
    {
      var deleteRule = row.GetString(deleteRuleIndex);
      foreignKey.OnDelete = GetEnumAction(deleteRule);

      var updateRule = row.GetString(updateRuleIndex);
      foreignKey.OnUpdate = GetEnumAction(updateRule);


      static ReferentialAction GetEnumAction(in string rawActionName)
      {
        return rawActionName switch {
          "CASCADE" => ReferentialAction.Cascade,
          "SET NULL" => ReferentialAction.SetNull,
          "NO ACTION" => ReferentialAction.NoAction,
          "RESTRICT" => ReferentialAction.NoAction,
          "SET DEFAULT" => ReferentialAction.SetDefault,
          _ => throw new ArgumentOutOfRangeException()
        };
      }
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}