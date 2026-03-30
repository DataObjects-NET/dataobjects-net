// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Xtensive.Sql.Model;
using Xtensive.Sql.Drivers.Oracle.Resources;
using Constraint=Xtensive.Sql.Model.Constraint;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Drivers.Oracle.v09
{
  internal partial class Extractor : Model.Extractor
  {
    private struct ColumnReaderState<TOwner>
    {
      public readonly Catalog Catalog;
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

    protected class ExtractionContext
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

      public ExtractionContext(Catalog catalog, Dictionary<string, string> replacementRegistry)
      {
        Catalog = catalog;
        this.replacementsRegistry = replacementRegistry;
      }
    }

    private const int DefaultDecimalPrecision = 38;
    private const int DefaultDecimalScale = 0;
    private const int DefaultDayPrecision = 2;
    private const int DefaultFSecondsPrecision = 6;

    private readonly object accessGuard = new object();

    private string nonSystemSchemasFilter;

    public override Catalog ExtractCatalog(string catalogName) =>
      ExtractSchemes(catalogName, Array.Empty<string>());

    public override Task<Catalog> ExtractCatalogAsync(string catalogName, CancellationToken token = default) =>
      ExtractSchemesAsync(catalogName, Array.Empty<string>(), token);

    /// <inheritdoc/>
    public override Catalog ExtractSchemes(string catalogName, string[] schemaNames)
    {
      var context = CreateContext(catalogName, schemaNames);

      ExtractSchemas(context);
      EnsureSchemasExist(context.Catalog, schemaNames);
      ExtractCatalogContents(context);
      return context.Catalog;
    }

    public override async Task<Catalog> ExtractSchemesAsync(string catalogName, string[] schemaNames,
      CancellationToken token = default)
    {
      var context = CreateContext(catalogName, schemaNames);

      await ExtractSchemasAsync(context, token).ConfigureAwait(false);
      EnsureSchemasExist(context.Catalog, schemaNames);
      await ExtractCatalogContentsAsync(context, token).ConfigureAwait(false);
      return context.Catalog;
    }

    protected virtual string ToUpperInvariantIfNeeded(string schemaName)
    {
      return schemaName.ToUpperInvariant();
    }

    private ExtractionContext CreateContext(string catalogName, string[] schemaNames)
    {
      var catalog = new Catalog(catalogName, true);
      for(var i = 0; i < schemaNames.Length; i++) {
        schemaNames[i] = ToUpperInvariantIfNeeded(schemaNames[i]);
      }

      var replacements = new Dictionary<string, string>();
      RegisterReplacements(replacements, schemaNames);

      return new ExtractionContext(catalog, replacements);
    }

    private void ExtractSchemas(ExtractionContext context)
    {
      // oracle does not clearly distinct users and schemas.
      // so we extract just users.
      using (var reader = ExecuteReader(context.PerformReplacements(GetExtractSchemasQuery()))) {
        while (reader.Read()) {
          context.Catalog.CreateSchema(reader.GetString(0));
        }
      }

      AssignCatalogDefaultSchema(context.Catalog);
    }

    private async Task ExtractSchemasAsync(ExtractionContext context, CancellationToken token)
    {
      // oracle does not clearly distinct users and schemas.
      // so we extract just users.
      var query = context.PerformReplacements(GetExtractSchemasQuery());
      var reader = await ExecuteReaderAsync(query, token).ConfigureAwait(false);
      await using (reader.ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          context.Catalog.CreateSchema(reader.GetString(0));
        }
      }

      AssignCatalogDefaultSchema(context.Catalog);
    }

    private void AssignCatalogDefaultSchema(Catalog catalog)
    {
      // choosing the default schema
      var defaultSchemaName = Driver.CoreServerInfo.DefaultSchemaName.ToUpperInvariant();
      var defaultSchema = catalog.Schemas[defaultSchemaName];
      catalog.DefaultSchema = defaultSchema;
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
      ExtractSequences(context);
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
      await ExtractSequencesAsync(context, token).ConfigureAwait(false);
    }

    private void ExtractTables(ExtractionContext context)
    {
      using var reader = ExecuteReader(context.PerformReplacements(GetExtractTablesQuery()));
      while (reader.Read()) {
        ReadTableData(reader, context.Catalog);
      }
    }

    private async Task ExtractTablesAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractTablesQuery());
      var reader = await ExecuteReaderAsync(query, token).ConfigureAwait(false);
      await using (reader.ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadTableData(reader, context.Catalog);
        }
      }
    }

    private void ExtractTableColumns(ExtractionContext context)
    {
      using var reader = ExecuteReader(context.PerformReplacements(GetExtractTableColumnsQuery()));
      var state = new ColumnReaderState<Table>(context.Catalog);
      while (reader.Read()) {
        ReadTableColumnData(reader, ref state);
      }
    }

    private async Task ExtractTableColumnsAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractTableColumnsQuery());
      var reader = await ExecuteReaderAsync(query, token).ConfigureAwait(false);
      await using (reader.ConfigureAwait(false)) {
        var state = new ColumnReaderState<Table>(context.Catalog);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadTableColumnData(reader, ref state);
        }
      }
    }

    private void ExtractViews(ExtractionContext context)
    {
      using var reader = ExecuteReader(context.PerformReplacements(GetExtractViewsQuery()));
      while (reader.Read()) {
        ReadViewData(reader, context.Catalog);
      }
    }

    private async Task ExtractViewsAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractViewsQuery());
      var reader = await ExecuteReaderAsync(query, token).ConfigureAwait(false);
      await using (reader.ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadViewData(reader, context.Catalog);
        }
      }
    }

    private void ExtractViewColumns(ExtractionContext context)
    {
      using var reader = ExecuteReader(context.PerformReplacements(GetExtractViewColumnsQuery()));
      var state = new ColumnReaderState<View>(context.Catalog);
      while (reader.Read()) {
        ReadViewColumnData(reader, ref state);
      }
    }

    private async Task ExtractViewColumnsAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractViewColumnsQuery());
      var reader = await ExecuteReaderAsync(query, token).ConfigureAwait(false);
      await using (reader.ConfigureAwait(false)) {
        var state = new ColumnReaderState<View>(context.Catalog);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadViewColumnData(reader, ref state);
        }
      }
    }

    private void ExtractIndexes(ExtractionContext context)
    {
      var query = context.PerformReplacements(GetExtractIndexesQuery());
      using var reader = (OracleDataReader) ExecuteReader(query);
      var state = new IndexColumnReaderState(context.Catalog);
      while (reader.Read()) {
        ReadIndexColumnData(reader, ref state);
      }
    }

    private async Task ExtractIndexesAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractIndexesQuery());
      var reader = (OracleDataReader) await ExecuteReaderAsync(query, token).ConfigureAwait(false);
      await using (reader.ConfigureAwait(false)) {
        var state = new IndexColumnReaderState(context.Catalog);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadIndexColumnData(reader, ref state);
        }
      }
    }

    private void ExtractForeignKeys(ExtractionContext context)
    {
      using var reader = ExecuteReader(context.PerformReplacements(GetExtractForeignKeysQuery()));
      var state = new ForeignKeyReaderState(context.Catalog);
      while (reader.Read()) {
        ReadForeignKeyColumnData(reader, ref state);
      }
    }

    private async Task ExtractForeignKeysAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractForeignKeysQuery());
      var reader = await ExecuteReaderAsync(query, token).ConfigureAwait(false);
      await using (reader.ConfigureAwait(false)) {
        var state = new ForeignKeyReaderState(context.Catalog);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadForeignKeyColumnData(reader, ref state);
        }
      }
    }

    private void ExtractUniqueAndPrimaryKeyConstraints(ExtractionContext context)
    {
      var query = context.PerformReplacements(GetExtractUniqueAndPrimaryKeyConstraintsQuery());
      using var reader = ExecuteReader(query);
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
      var reader = await ExecuteReaderAsync(query, token).ConfigureAwait(false);
      await using (reader.ConfigureAwait(false)) {
        var state = new IndexBasedConstraintReaderState(context.Catalog);
        bool readingCompleted;
        do {
          readingCompleted = !await reader.ReadAsync(token).ConfigureAwait(false);
          ReadIndexBasedConstraintData(reader, ref state, readingCompleted);
        } while (!readingCompleted);
      }
    }

    private void ExtractCheckConstraints(ExtractionContext context)
    {
      using var reader = ExecuteReader(context.PerformReplacements(GetExtractCheckConstraintsQuery()));
      while (reader.Read()) {
        ReadCheckConstraintData(reader, context.Catalog);
      }
    }

    private async Task ExtractCheckConstraintsAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractCheckConstraintsQuery());
      var reader = await ExecuteReaderAsync(query, token).ConfigureAwait(false);
      await using (reader.ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadCheckConstraintData(reader, context.Catalog);
        }
      }
    }

    private void ExtractSequences(ExtractionContext context)
    {
      using var reader = ExecuteReader(context.PerformReplacements(GetExtractSequencesQuery()));
      while (reader.Read()) {
        ReadSequenceData(reader, context.Catalog);
      }
    }

    private async Task ExtractSequencesAsync(ExtractionContext context, CancellationToken token)
    {
      var query = context.PerformReplacements(GetExtractSequencesQuery());
      var reader = await ExecuteReaderAsync(query, token).ConfigureAwait(false);
      await using (reader.ConfigureAwait(false)) {
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          ReadSequenceData(reader, context.Catalog);
        }
      }
    }

    private void ReadTableData(DbDataReader reader, Catalog catalog)
    {
      var schema = catalog.Schemas[reader.GetString(0)];
      var tableName = reader.GetString(1);
      var isTemporary = ReadBool(reader, 2);
      if (isTemporary) {
        var table = schema.CreateTemporaryTable(tableName);
        table.PreserveRows = reader.GetString(3) == "SYS$SESSION";
        table.IsGlobal = true;
      }
      else {
        _ = schema.CreateTable(tableName);
      }
    }

    private void ReadTableColumnData(DbDataReader reader, ref ColumnReaderState<Table> state)
    {
      var columnIndex = ReadInt(reader, 9);
      if (columnIndex <= state.LastColumnIndex) {
        var schema = state.Catalog.Schemas[reader.GetString(0)];
        state.Owner = schema.Tables[reader.GetString(1)];
      }

      var column = state.Owner.CreateColumn(reader.GetString(2));
      column.DataType = CreateValueType(reader, 3, 4, 5, 6);
      column.IsNullable = ReadBool(reader, 7);
      var defaultValue = ReadStringOrNull(reader, 8);
      if (!string.IsNullOrEmpty(defaultValue)) {
        column.DefaultValue = SqlDml.Native(defaultValue);
      }

      state.LastColumnIndex = columnIndex;
    }

    private static void ReadViewData(DbDataReader reader, Catalog catalog)
    {
      var schema = catalog.Schemas[reader.GetString(0)];
      var view = reader.GetString(1);
      var definition = ReadStringOrNull(reader, 2);
      if (string.IsNullOrEmpty(definition)) {
        _ = schema.CreateView(view);
      }
      else {
        _ = schema.CreateView(view, SqlDml.Native(definition));
      }
    }

    private static void ReadViewColumnData(DbDataReader reader, ref ColumnReaderState<View> state)
    {
      var columnId = ReadInt(reader, 3);
      if (columnId <= state.LastColumnIndex) {
        var schema = state.Catalog.Schemas[reader.GetString(0)];
        state.Owner = schema.Views[reader.GetString(1)];
      }

      _ = state.Owner.CreateColumn(reader.GetString(2));
      state.LastColumnIndex = columnId;
    }

    private static void ReadIndexColumnData(OracleDataReader reader, ref IndexColumnReaderState state)
    {
      // it's possible to have table and index in different schemas in oracle.
      // we silently ignore this, indexes are always belong to the same schema as its table.
      var columnIndex = ReadInt(reader, 6);
      if (columnIndex <= state.LastColumnIndex) {
        var schema = state.Catalog.Schemas[reader.GetString(0)];
        state.Table = schema.Tables[reader.GetString(1)];
        state.Index = state.Table.CreateIndex(reader.GetString(2));
        state.Index.IsUnique = ReadBool(reader, 3);
        state.Index.IsBitmap = reader.GetString(4).EndsWith("BITMAP");
        if (!reader.IsDBNull(5)) {
          var pctFree = ReadInt(reader, 5);
          state.Index.FillFactor = (byte) (100 - pctFree);
        }
      }

      var columnName = reader.IsDBNull(9) ? reader.GetString(7) : reader.GetOracleString(9).Value;
      columnName = columnName.Trim('"');
      var column = state.Table.TableColumns[columnName];
      var isAscending = reader.GetString(8) == "ASC";
      _ = state.Index.CreateIndexColumn(column, isAscending);
      state.LastColumnIndex = columnIndex;
    }

    private static void ReadForeignKeyColumnData(DbDataReader reader, ref ForeignKeyReaderState state)
    {
      var lastColumnIndex = ReadInt(reader, 7);
      if (lastColumnIndex <= state.LastColumnIndex) {
        var referencingSchema = state.Catalog.Schemas[reader.GetString(0)];
        state.ReferencingTable = referencingSchema.Tables[reader.GetString(1)];
        state.ForeignKey = state.ReferencingTable.CreateForeignKey(reader.GetString(2));
        ReadConstraintProperties(state.ForeignKey, reader, 3, 4);
        ReadForeignKeyDeleteAction(state.ForeignKey, reader, 5);
        var referencedSchema = state.Catalog.Schemas[reader.GetString(8)];
        state.ReferencedTable = referencedSchema.Tables[reader.GetString(9)];
        state.ForeignKey.ReferencedTable = state.ReferencedTable;
      }

      var referencingColumn = state.ReferencingTable.TableColumns[reader.GetString(6)];
      var referencedColumn = state.ReferencedTable.TableColumns[reader.GetString(10)];
      state.ForeignKey.Columns.Add(referencingColumn);
      state.ForeignKey.ReferencedColumns.Add(referencedColumn);
      state.LastColumnIndex = lastColumnIndex;
    }

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
          var schema = state.Catalog.Schemas[reader.GetString(0)];
          state.Table = schema.Tables[reader.GetString(1)];
          state.ConstraintName = reader.GetString(2);
          state.ConstraintType = reader.GetString(3);
        }

        state.Columns.Add(state.Table.TableColumns[reader.GetString(4)]);
        state.LastColumnIndex = columnIndex;
      }
    }

    private static void ReadCheckConstraintData(DbDataReader reader, Catalog catalog)
    {
      var schema = catalog.Schemas[reader.GetString(0)];
      var table = schema.Tables[reader.GetString(1)];
      var name = reader.GetString(2);
      // It looks like ODP.NET sometimes fail to read a LONG column via GetString
      // It returns empty string instead of the actual value.
      var condition = string.IsNullOrEmpty(reader.GetString(3))
        ? null
        : SqlDml.Native(reader.GetString(3));
      var constraint = table.CreateCheckConstraint(name, condition);
      ReadConstraintProperties(constraint, reader, 4, 5);
    }

    private static void ReadSequenceData(DbDataReader reader, Catalog catalog)
    {
      var schema = catalog.Schemas[reader.GetString(0)];
      var sequence = schema.CreateSequence(reader.GetString(1));
      var descriptor = sequence.SequenceDescriptor;
      descriptor.MinValue = ReadLong(reader, 2);
      descriptor.MaxValue = ReadLong(reader, 3);
      descriptor.Increment = ReadLong(reader, 4);
      descriptor.IsCyclic = ReadBool(reader, 5);
    }

    private SqlValueType CreateValueType(IDataRecord row,
      int typeNameIndex, int precisionIndex, int scaleIndex, int charLengthIndex)
    {
      var typeName = row.GetString(typeNameIndex);
      if (string.Equals(typeName, "NUMBER", StringComparison.OrdinalIgnoreCase)) {
        var precision = row.IsDBNull(precisionIndex) ? DefaultDecimalPrecision : ReadInt(row, precisionIndex);
        var scale = row.IsDBNull(scaleIndex) ? DefaultDecimalScale : ReadInt(row, scaleIndex);
        return new SqlValueType(SqlType.Decimal, precision, scale);
      }
      if (typeName.StartsWith("INTERVAL DAY", StringComparison.OrdinalIgnoreCase)) {
        var dayPrecision = row.IsDBNull(precisionIndex) ? DefaultDayPrecision : ReadInt(row, precisionIndex);
        var fSecondsPrecision = row.IsDBNull(scaleIndex) ? DefaultFSecondsPrecision : ReadInt(row, scaleIndex);

        return (dayPrecision == 0)
          ? new SqlValueType(SqlType.Time)
          : new SqlValueType(SqlType.Interval);
      }
      if (typeName.StartsWith("INTERVAL DAY", StringComparison.OrdinalIgnoreCase)) {
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Interval);
      }
      if (typeName.StartsWith("TIMESTAMP", StringComparison.OrdinalIgnoreCase)) {
        // "timestamp precision" is saved as "scale", ignoring too
        if (typeName.Contains("WITH TIME ZONE", StringComparison.OrdinalIgnoreCase)) {
          return new SqlValueType(SqlType.DateTimeOffset);
        }

        return new SqlValueType(SqlType.DateTime);
      }
      if (string.Equals(typeName, "NVARCHAR2", StringComparison.OrdinalIgnoreCase)
        || string.Equals(typeName, "NCHAR", StringComparison.OrdinalIgnoreCase)) {
        var length = ReadInt(row, charLengthIndex);
        var sqlType = typeName.Length == 5 ? SqlType.Char : SqlType.VarChar;
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
        case "P":
          _ = state.Table.CreatePrimaryKey(state.ConstraintName, state.Columns.ToArray());
          return;
        case "U":
          _ = state.Table.CreateUniqueConstraint(state.ConstraintName, state.Columns.ToArray());
          return;
        default:
          throw new ArgumentOutOfRangeException(nameof(IndexBasedConstraintReaderState.ConstraintType));
      }
    }

    private static bool ReadBool(IDataRecord row, int index)
    {
      var value = row.GetString(index);
      switch (value) {
        case "Y":
        case "YES":
        case "ENABLED":
        case "UNIQUE":
          return true;
        case "N":
        case "NO":
        case "DISABLED":
        case "NONUNIQUE":
          return false;
        default:
          throw new ArgumentOutOfRangeException(string.Format(Strings.ExInvalidBooleanStringX, value));
      }
    }

    private static long ReadLong(IDataRecord row, int index)
    {
      var value = row.GetDecimal(index);

      if (value > long.MaxValue) {
        return long.MaxValue;
      }

      if (value < long.MinValue) {
        return long.MinValue;
      }

      return (long) value;
    }

    private static int ReadInt(IDataRecord row, int index)
    {
      var value = row.GetDecimal(index);

      if (value > int.MaxValue) {
        return int.MaxValue;
      }

      if (value < int.MinValue) {
        return int.MinValue;
      }

      return (int) value;
    }

    private static string ReadStringOrNull(IDataRecord row, int index) =>
      row.IsDBNull(index) ? null : row.GetString(index);

    private static void ReadConstraintProperties(Constraint constraint,
      IDataRecord row, int isDeferrableIndex, int isInitiallyDeferredIndex)
    {
      constraint.IsDeferrable = row.GetString(isDeferrableIndex) == "DEFERRABLE";
      constraint.IsInitiallyDeferred = row.GetString(isInitiallyDeferredIndex) == "DEFERRED";
    }

    private static void ReadForeignKeyDeleteAction(ForeignKey foreignKey, IDataRecord row, int deleteRuleIndex)
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

    protected virtual void RegisterReplacements(Dictionary<string, string> replacements, IReadOnlyCollection<string> targetSchemes)
    {
      var schemaFilter = targetSchemes != null && targetSchemes.Count != 0
        ? MakeSchemaFilter(targetSchemes)
        //? "= " + SqlHelper.QuoteString(targetSchema)
        : GetNonSystemSchemasFilter();
      replacements[SchemaFilterPlaceholder] = schemaFilter;
      replacements[IndexesFilterPlaceholder] = "1 > 0";
      replacements[TableFilterPlaceholder] = "IS NOT NULL";
    }

    private static string MakeSchemaFilter(IReadOnlyCollection<string> targetSchemes)
    {
      var schemaStrings = targetSchemes.Select(SqlHelper.QuoteString);
      var schemaList = string.Join(",", schemaStrings);
      return $"IN ({schemaList})";
    }

    protected override DbDataReader ExecuteReader(string commandText)
    {
      using var command = (OracleCommand) Connection.CreateCommand(commandText);
      // This option is required to access LONG columns
      command.InitialLONGFetchSize = -1;
      return command.ExecuteReader();
    }

    protected override async Task<DbDataReader> ExecuteReaderAsync(string commandText, CancellationToken token = default)
    {
      var command = (OracleCommand) Connection.CreateCommand(commandText);
      await using (command.ConfigureAwait(false)) {
        // This option is required to access LONG columns
        command.InitialLONGFetchSize = -1;
        return await command.ExecuteReaderAsync(token).ConfigureAwait(false);
      }
    }

    protected override DbDataReader ExecuteReader(ISqlCompileUnit statement)
    {
      var commandText = Connection.Driver.Compile(statement).GetCommandText();
      return ExecuteReader(commandText);
    }

    protected override Task<DbDataReader> ExecuteReaderAsync(ISqlCompileUnit statement, CancellationToken token = default)
    {
      var commandText = Connection.Driver.Compile(statement).GetCommandText();
      return ExecuteReaderAsync(commandText, token);
    }

    private string GetNonSystemSchemasFilter()
    {
      if (nonSystemSchemasFilter == null) {
        lock (accessGuard) {
          if (nonSystemSchemasFilter == null) {
            var schemaStrings = GetSystemSchemas().Select(SqlHelper.QuoteString).ToArray();
            var schemaList = string.Join(",", schemaStrings);
            nonSystemSchemasFilter = $"NOT IN ({schemaList})";
          }
        }
      }
      return nonSystemSchemasFilter;
    }

    private static IEnumerable<string> GetSystemSchemas() =>
      new[] {
        "ANONYMOUS",
        "APEX_PUBLIC_USER",
        "APEX_040000",
        "CTXSYS",
        "DBSNMP",
        "DIP",
        "EXFSYS",
        "FLOWS_%",
        "FLOWS_FILES",
        "LBACSYS",
        "MDDATA",
        "MDSYS",
        "MGMT_VIEW",
        "OLAPSYS",
        "ORACLE_OCM",
        "ORDDATA",
        "ORDPLUGINS",
        "ORDSYS",
        "OUTLN",
        "OWBSYS",
        "SI_INFORMTN_SCHEMA",
        "SPATIAL_CSW_ADMIN_USR",
        "SPATIAL_WFS_ADMIN_USR",
        "SYS",
        "SYSMAN",
        "SYSTEM",
        "WKPROXY",
        "WKSYS",
        "WK_TEST",
        "WMSYS",
        "XDB",
        "XS$NULL",
      };

    private static void EnsureSchemasExist(Catalog catalog, string[] schemaNames)
    {
      foreach (var schemaName in schemaNames) {
        var realSchemaName = schemaName.ToUpperInvariant();
        var schema = catalog.Schemas[realSchemaName];
        if (schema==null) {
          catalog.CreateSchema(realSchemaName);
        }
      }
    }


    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
