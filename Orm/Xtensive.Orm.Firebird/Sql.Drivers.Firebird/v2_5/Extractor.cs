// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.13

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Xtensive.Sql.Drivers.Firebird.Resources;
using Xtensive.Sql.Model;
using Constraint = Xtensive.Sql.Model.Constraint;
using Xtensive.Sql.Dml;
using Xtensive.Core;

namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
  internal partial class Extractor : Model.Extractor
  {
    private const int DefaultNumericPrecision = 18;
    private const int DefaultNumericScale = 0;

    private Catalog theCatalog;
    private string targetSchema;

    private string GetSchema(IDataRecord p_row)
    {
      return p_row.GetString(1) ?? theCatalog.DefaultSchema.Name;
    }

    protected override void Initialize()
    {
      theCatalog = new Catalog(Driver.CoreServerInfo.DatabaseName);
    }

    public override Catalog ExtractCatalog(string catalogName)
    {
      targetSchema = null;
      ExtractSchemas();
      ExtractCatalogContents();
      return theCatalog;
    }

    public override Schema ExtractSchema(string catalogName, string schemaName)
    {
      targetSchema = schemaName.ToUpperInvariant();
      theCatalog.CreateSchema(targetSchema);
      ExtractCatalogContents();
      return theCatalog.Schemas[0];
//            return theCatalog.Schemas[targetSchema];
    }

    private void ExtractCatalogContents()
    {
      ExtractTables();
      ExtractTableColumns();
      ExtractViews();
      ExtractViewColumns();
      ExtractIndexes();
      ExtractForeignKeys();
      ExtractCheckConstaints();
      ExtractUniqueAndPrimaryKeyConstraints();
      ExtractSequences();
    }

    private void ExtractSchemas()
    {
      var defaultSchemaName = Driver.CoreServerInfo.DefaultSchemaName.ToUpperInvariant();
      var defaultSchema = theCatalog.CreateSchema(defaultSchemaName);
      theCatalog.DefaultSchema = defaultSchema;
    }

    private void ExtractTables()
    {
      using (var reader = Connection.CreateCommand(GetExtractTablesQuery()).ExecuteReader(CommandBehavior.SingleResult)) {
        while (reader.Read()) {
          var schema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(0)];
          string tableName = reader.GetString(1).Trim();
          int tableType = reader.GetInt16(2);
          bool isTemporary = tableType == 4 || tableType == 5;
          if (isTemporary) {
            var table = schema.CreateTemporaryTable(tableName);
            table.PreserveRows = tableType == 4;
            table.IsGlobal = true;
          }
          else {
            schema.CreateTable(tableName);
          }
        }
      }
    }

    private void ExtractTableColumns()
    {
      using (
        var reader = Connection.CreateCommand(GetExtractTableColumnsQuery()).ExecuteReader(CommandBehavior.SingleResult)
        ) {
        Table table = null;
        int lastColumnId = int.MaxValue;
        while (reader.Read()) {
          int columnSeq = reader.GetInt16(2);
          if (columnSeq <= lastColumnId) {
            var schema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(0)];
            table = schema.Tables[reader.GetString(1).Trim()];
          }
          var column = table.CreateColumn(reader.GetString(3));
          column.DataType = CreateValueType(reader, 4, 6, 7, 8);
          column.IsNullable = ReadBool(reader, 9);
          var defaultValue = ReadStringOrNull(reader, 10);
          if (!string.IsNullOrEmpty(defaultValue))
            column.DefaultValue = SqlDml.Native(defaultValue);
          lastColumnId = columnSeq;
        }
      }
    }

    private void ExtractViews()
    {
      using (var reader = Connection.CreateCommand(GetExtractViewsQuery()).ExecuteReader(CommandBehavior.SingleResult)) {
        while (reader.Read()) {
          var schema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(0)];
          var view = reader.GetString(1).Trim();
          var definition = ReadStringOrNull(reader, 2);
          if (string.IsNullOrEmpty(definition))
            schema.CreateView(view);
          else
            schema.CreateView(view, SqlDml.Native(definition));
        }
      }
    }

    private void ExtractViewColumns()
    {
      using (
        var reader = Connection.CreateCommand(GetExtractViewColumnsQuery()).ExecuteReader(CommandBehavior.SingleResult)) {
        int lastColumnId = int.MaxValue;
        View view = null;
        while (reader.Read()) {
          int columnId = reader.GetInt16(3);
          if (columnId <= lastColumnId) {
            var schema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(0)];
            view = schema.Views[reader.GetString(1).Trim()];
          }
          view.CreateColumn(reader.GetString(2).Trim());
          lastColumnId = columnId;
        }
      }
    }

    private void ExtractIndexes()
    {
      using (var reader = Connection.CreateCommand(GetExtractIndexesQuery()).ExecuteReader(CommandBehavior.SingleResult)
        ) {
        string indexName = string.Empty;
        string lastIndexName = string.Empty;
        Table table = null;
        Index index = null;

        while (reader.Read()) {
          SqlExpression expression = null;
          indexName = reader.GetString(2).Trim();
          if (indexName != lastIndexName) {
            var schema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(0)];
            table = schema.Tables[reader.GetString(1).Trim()];
            index = table.CreateIndex(indexName);
            index.IsUnique = ReadBool(reader, 5);
            index.IsBitmap = false;
            index.IsClustered = false;
            if (!reader.IsDBNull(8)) // expression index
              expression = SqlDml.Native(reader.GetString(8).Trim());
          }
          if (expression == null) {
            var column = table.TableColumns[reader.GetString(6).Trim()];
            bool isDescending = ReadBool(reader, 4);
            index.CreateIndexColumn(column, !isDescending);
          }
          else {
            bool isDescending = ReadBool(reader, 4);
            index.CreateIndexColumn(expression, !isDescending);
          }
          lastIndexName = indexName;
        }
      }
    }

    private void ExtractForeignKeys()
    {
      using (
        var reader = Connection.CreateCommand(GetExtractForeignKeysQuery()).ExecuteReader(CommandBehavior.SingleResult)) {
        int lastColumnPosition = int.MaxValue;
        ForeignKey constraint = null;
        Table referencingTable = null;
        Table referencedTable = null;
        while (reader.Read()) {
          int columnPosition = reader.GetInt16(7);
          if (columnPosition <= lastColumnPosition) {
            var referencingSchema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(0)];
            referencingTable = referencingSchema.Tables[reader.GetString(1).Trim()];
            constraint = referencingTable.CreateForeignKey(reader.GetString(2).Trim());
            ReadConstraintProperties(constraint, reader, 3, 4);
            ReadCascadeAction(constraint, reader, 5);
            var referencedSchema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(8)];
            referencedTable = referencedSchema.Tables[reader.GetString(9).Trim()];
            constraint.ReferencedTable = referencedTable;
          }
          var referencingColumn = referencingTable.TableColumns[reader.GetString(6).Trim()];
          var referencedColumn = referencedTable.TableColumns[reader.GetString(10).Trim()];
          constraint.Columns.Add(referencingColumn);
          constraint.ReferencedColumns.Add(referencedColumn);
          lastColumnPosition = columnPosition;
        }
      }
    }

    private void ExtractUniqueAndPrimaryKeyConstraints()
    {
      using (
        var reader =
          Connection.CreateCommand(GetExtractUniqueAndPrimaryKeyConstraintsQuery()).ExecuteReader(
            CommandBehavior.SingleResult)) {
        Table table = null;
        string constraintName = null;
        string constraintType = null;
        var columns = new List<TableColumn>();
        int lastColumnPosition = -1;
        while (reader.Read()) {
          int columnPosition = reader.GetInt16(5);
          if (columnPosition <= lastColumnPosition) {
            CreateIndexBasedConstraint(table, constraintName, constraintType, columns);
            columns.Clear();
          }
          if (columns.Count == 0) {
            var schema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(0)];
            table = schema.Tables[reader.GetString(1).Trim()];
            constraintName = reader.GetString(2).Trim();
            constraintType = reader.GetString(3).Trim();
          }
          columns.Add(table.TableColumns[reader.GetString(4).Trim()]);
          lastColumnPosition = columnPosition;
        }
        if (columns.Count > 0)
          CreateIndexBasedConstraint(table, constraintName, constraintType, columns);
      }
    }

    private void ExtractCheckConstaints()
    {
      using (
        var reader =
          Connection.CreateCommand(GetExtractCheckConstraintsQuery()).ExecuteReader(CommandBehavior.SingleResult)) {
        while (reader.Read()) {
          var schema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(0)];
          var table = schema.Tables[reader.GetString(1).Trim()];
          var name = reader.GetString(2).Trim();
          var condition = reader.GetString(3).Trim();
          var constraint = table.CreateCheckConstraint(name, condition);
        }
      }
    }

    private void ExtractSequences()
    {
      using (
        var reader = Connection.CreateCommand(GetExtractSequencesQuery()).ExecuteReader(CommandBehavior.SingleResult)) {
        while (reader.Read()) {
          var schema = theCatalog.DefaultSchema; // theCatalog.Schemas[reader.GetString(0)];
          var sequence = schema.CreateSequence(reader.GetString(1).Trim());
          var descriptor = sequence.SequenceDescriptor;
          descriptor.MinValue = 0;
          // TODO: Firebird quickfix, we must implement support for arbitrary incr. in comparer
          descriptor.Increment = 128;
        }
      }

      {
        foreach (var sequence in theCatalog.DefaultSchema.Sequences) {
          using (
            var reader = Connection
              .CreateCommand(
                GetExtractSequenceValueQuery().FormatWith(
                  Driver.Translator.QuoteIdentifier(sequence.Name)))
              .ExecuteReader(CommandBehavior.SingleResult)) {
            while (reader.Read())
              sequence.SequenceDescriptor.MinValue = reader.GetInt64(0);
          }
        }
      }
    }

    private SqlValueType CreateValueType(IDataRecord row,
      int typeNameIndex, int precisionIndex, int scaleIndex, int charLengthIndex)
    {
      string typeName = row[typeNameIndex].ToString().Trim().ToUpper();
      if (typeName == "NUMERIC" || typeName == "DECIMAL") {
        int precision = Convert.ToInt32(row[precisionIndex]);
        int scale = Convert.ToInt32(row[scaleIndex]);
        return new SqlValueType(SqlType.Decimal, precision, scale);
      }
      if (typeName.StartsWith("TIMESTAMP"))
        return new SqlValueType(SqlType.DateTime);
      if (typeName == "VARCHAR" || typeName == "CHAR") {
        int length = Convert.ToInt32(row[charLengthIndex]);
        var sqlType = typeName.Length == 4 ? SqlType.Char : SqlType.VarChar;
        return new SqlValueType(sqlType, length);
      }

      if (typeName == "BLOB SUB TYPE 0")
        return new SqlValueType(SqlType.VarCharMax);

      if (typeName == "BLOB SUB TYPE 1")
        return new SqlValueType(SqlType.VarBinaryMax);

      var typeInfo = Driver.ServerInfo.DataTypes[typeName];
      return typeInfo != null
        ? new SqlValueType(typeInfo.Type)
        : new SqlValueType(typeName);
    }

    private static void CreateIndexBasedConstraint(
      Table table, string constraintName, string constraintType, List<TableColumn> columns)
    {
      switch (constraintType.Trim()) {
      case "PRIMARY KEY":
        table.CreatePrimaryKey(constraintName, columns.ToArray());
        return;
      case "UNIQUE":
        table.CreateUniqueConstraint(constraintName, columns.ToArray());
        return;
      default:
        throw new ArgumentOutOfRangeException("constraintType");
      }
    }

    private static bool ReadBool(IDataRecord row, int index)
    {
      short value = row.IsDBNull(index) ? (short) 0 : Convert.ToInt16(row.GetString(index) ?? "0");
      switch (value) {
      case 1:
        return true;
      case 0:
        return false;
      default:
        throw new ArgumentOutOfRangeException(string.Format(Strings.ExInvalidBooleanSmallintValue, value));
      }
    }

    private static string ReadStringOrNull(IDataRecord row, int index)
    {
      return row.IsDBNull(index) ? null : row.GetString(index).Trim();
    }

    private static void ReadConstraintProperties(Constraint constraint,
      IDataRecord row, int isDeferrableIndex, int isInitiallyDeferredIndex)
    {
      constraint.IsDeferrable = ReadStringOrNull(row, isDeferrableIndex) == "YES";
      constraint.IsInitiallyDeferred = ReadStringOrNull(row, isInitiallyDeferredIndex) == "YES";
    }

    private static void ReadCascadeAction(ForeignKey foreignKey, IDataRecord row, int deleteRuleIndex)
    {
      var deleteRule = ReadStringOrNull(row, deleteRuleIndex);
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
      case "RESTRICT": // behaves like NO ACTION
        foreignKey.OnDelete = ReferentialAction.NoAction;
        return;
      case "SET DEFAULT":
        foreignKey.OnDelete = ReferentialAction.SetDefault;
        return;
      }
    }

    protected override DbDataReader ExecuteReader(ISqlCompileUnit statement)
    {
      var commandText = Connection.Driver.Compile(statement).GetCommandText();
      // base.ExecuteReader(...) cannot be used because it disposes the DbCommand so it makes returned DbDataReader unusable
      return Connection.CreateCommand(commandText).ExecuteReader();
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
