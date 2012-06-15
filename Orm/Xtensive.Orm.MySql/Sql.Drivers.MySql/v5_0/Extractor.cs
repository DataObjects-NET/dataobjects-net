﻿// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Xtensive.Sql.Drivers.MySql.Resources;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.MySql.v5_0
{
  internal partial class Extractor : Model.Extractor
  {
    private const int DefaultPrecision = 38;
    private const int DefaultScale = 0;

    private readonly Dictionary<string, string> replacementsRegistry = new Dictionary<string, string>();
    private string targetSchema;
    private Catalog theCatalog;

    /// <inheritdoc/>
    protected override void Initialize()
    {
      theCatalog = new Catalog(Driver.CoreServerInfo.DatabaseName);
    }

    /// <param name="catalogName"> </param>
    /// <inheritdoc/>
    public override Catalog ExtractCatalog(string catalogName)
    {
      targetSchema = null;
      RegisterReplacements(replacementsRegistry);
      var schema = ExtractSchema(string.Empty, Driver.CoreServerInfo.DefaultSchemaName);
      return schema.Catalog;
    }

    /// <inheritdoc/>
    public override Schema ExtractSchema(string catalogName, string schemaName)
    {
      targetSchema = schemaName;
      var result = theCatalog.CreateSchema(targetSchema);
      RegisterReplacements(replacementsRegistry);
      ExtractCatalogContents();
      return result;
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
    }

    // ---- ExtractTables
    //  0   table_schema, 
    //  1   table_name, 
    //  2   table_type
    private void ExtractTables()
    {
      using (DbDataReader reader = ExecuteReader(GetExtractTablesQuery())) {
        while (reader.Read()) {
          var schemaName = reader.GetString(0);
          var schema = theCatalog.Schemas[schemaName];
          var tableName = reader.GetString(1);
          schema.CreateTable(tableName);
        }
      }
    }

    // ---- ExtractTableColumns
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
    private void ExtractTableColumns()
    {
      using (DbDataReader reader = ExecuteReader(GetExtractTableColumnsQuery())) {
        Table table = null;
        Schema schema = null;
        TableColumn column = null;
        int lastColumnId = int.MaxValue;
        while (reader.Read()) {
          int columnId = ReadInt(reader, 2);
          if (columnId <= lastColumnId) {
            //Schema
            schema = theCatalog.Schemas[reader.GetString(0)];

            //Table
            table = schema.Tables[reader.GetString(1)];
          }

          //Column
          column = table.CreateColumn(reader.GetString(3));

          //Collation
          string collationName = ReadStringOrNull(reader, 10);
          if (!string.IsNullOrEmpty(collationName))
            column.Collation = schema.Collations[collationName] ?? schema.CreateCollation(collationName);

          //Data type
          column.DataType = CreateValueType(reader, 4, 8, 9, 7);

          //Nullable
          column.IsNullable = ReadBool(reader, 5);

          //Default
          string defaultValue = ReadStringOrNull(reader, 12);
          if (!string.IsNullOrEmpty(defaultValue))
            column.DefaultValue = SqlDml.Native(defaultValue);

          // AutoIncrement
          if (ReadAutoIncrement(reader, 13))
            column.SequenceDescriptor = new SequenceDescriptor(column, ReadInt(reader, 14), 1);

          //Column number.
          lastColumnId = columnId;
        }
      }
    }

    //---- ExtractViews
    //   0      table_schema,
    //   1      table_name,
    //   2      view_definition
    private void ExtractViews()
    {
      using (DbDataReader reader = ExecuteReader(GetExtractViewsQuery())) {
        while (reader.Read()) {
          Schema schema = theCatalog.Schemas[reader.GetString(0)];
          string view = reader.GetString(1);
          string definition = ReadStringOrNull(reader, 2);
          if (string.IsNullOrEmpty(definition))
            schema.CreateView(view);
          else
            schema.CreateView(view, SqlDml.Native(definition));
        }
      }
    }

    //---- ExtractViewColumns
    //   0      table_schema,
    //   1      table_name,
    //   2      column_name,
    //   3      ordinal_position,
    //   4      view_definition
    private void ExtractViewColumns()
    {
      using (DbDataReader reader = ExecuteReader(GetExtractViewColumnsQuery())) {
        int lastColumnId = int.MaxValue;
        View view = null;
        while (reader.Read()) {
          int columnId = ReadInt(reader, 3);
          if (columnId <= lastColumnId) {
            Schema schema = theCatalog.Schemas[reader.GetString(0)];
            view = schema.Views[reader.GetString(1)];
          }
          view.CreateColumn(reader.GetString(2));
          lastColumnId = columnId;
        }
      }
    }

    //---- ExtractIndexes
    //   0      table_schema,
    //  1       table_name,
    //  2       index_name,
    //  3       non_unique,
    //  4       index_type,
    //  5       seq_in_index,
    //  6       column_name,
    //  7       cardinality,
    //  8       sub_part,
    //  9       nullable
    private void ExtractIndexes()
    {
      using (DbDataReader reader = ExecuteReader(GetExtractIndexesQuery())) {
        int lastColumnPosition = int.MaxValue;
        Table table = null;
        Index index = null;
        while (reader.Read()) {
          int columnPosition = ReadInt(reader, 5);
          if (columnPosition <= lastColumnPosition) {
            Schema schema = theCatalog.Schemas[reader.GetString(0)];
            table = schema.Tables[reader.GetString(1)];
            if (IsFullTextIndex(reader, 4)) {
              table.CreateFullTextIndex(reader.GetString(2));
            }
            else {
              index = table.CreateIndex(reader.GetString(2));
              index.IsUnique = reader.GetInt32(3)==0;
            }
          }
          TableColumn column = table.TableColumns[reader.GetString(6)];
          index.CreateIndexColumn(column);

          lastColumnPosition = columnPosition;
        }
      }
    }

    //....  ExtractForeignKeys
    //  0       constraint_schema,
    //  1       table_name,
    //  2       constraint_name,
    //  3       delete_rule,
    //  4       column_name,
    //  5       ordinal_position,
    //  6       referenced_table_schema,
    //  7       referenced_table_name,
    //  8       referenced_column_name
    private void ExtractForeignKeys()
    {
      using (DbDataReader reader = ExecuteReader(GetExtractForeignKeysQuery())) {
        int lastColumnPosition = int.MaxValue;
        ForeignKey constraint = null;
        Table referencingTable = null;
        Table referencedTable = null;
        while (reader.Read()) {
          int columnPosition = ReadInt(reader, 5);
          if (columnPosition <= lastColumnPosition) {
            Schema referencingSchema = theCatalog.Schemas[reader.GetString(0)];
            referencingTable = referencingSchema.Tables[reader.GetString(1)];
            constraint = referencingTable.CreateForeignKey(reader.GetString(2));
            //ReadConstraintProperties(constraint, reader, 3, 4);
            ReadCascadeAction(constraint, reader, 3);
            Schema referencedSchema = theCatalog.Schemas[reader.GetString(0)]; //Schema same as current
            referencedTable = referencedSchema.Tables[reader.GetString(7)];
            constraint.ReferencedTable = referencedTable;
          }
          TableColumn referencingColumn = referencingTable.TableColumns[reader.GetString(4)];
          TableColumn referencedColumn = referencedTable.TableColumns[reader.GetString(8)];
          constraint.Columns.Add(referencingColumn);
          constraint.ReferencedColumns.Add(referencedColumn);
          lastColumnPosition = columnPosition;
        }
      }
    }

    //---- ExtractUniqueAndPrimaryKeyConstraints
    //   0      constraint_schema,
    //   1      table_name,
    //   2      constraint_name,
    //  3       constraint_type,
    //  4       column_name,
    //  5       ordinal_position
    private void ExtractUniqueAndPrimaryKeyConstraints()
    {
      using (DbDataReader reader = ExecuteReader(GetExtractUniqueAndPrimaryKeyConstraintsQuery())) {
        Table table = null;
        string constraintName = null;
        string constraintType = null;
        var columns = new List<TableColumn>();
        int lastColumnPosition = -1;
        while (reader.Read()) {
          int columnPosition = ReadInt(reader, 5);
          if (columnPosition <= lastColumnPosition) {
            CreateIndexBasedConstraint(table, constraintName, constraintType, columns);
            columns.Clear();
          }
          if (columns.Count==0) {
            var schemaName = reader.GetString(0);
            var tableName = reader.GetString(1);
            var schema = theCatalog.Schemas[schemaName];
            if (schema==null)
              throw new InvalidOperationException(string.Format("Schema '{0}' is not found", schemaName));
            table = schema.Tables[tableName];
            if (table==null)
              throw new InvalidOperationException(string.Format("Table '{0}' is not found in schema '{1}'", tableName, schemaName));
            constraintName = reader.GetString(2);
            constraintType = reader.GetString(3);
          }
          columns.Add(table.TableColumns[reader.GetString(4)]);
          lastColumnPosition = columnPosition;
        }
        if (columns.Count > 0)
          CreateIndexBasedConstraint(table, constraintName, constraintType, columns);
      }
    }

    //--- ExtractCheckConstaints
    //  0   -   constraint_schema,
    //  1   -   constraint_name,
    //  2   -   table_schema,
    //  3   -   table_name,
    //  4   -   constraint_type
    private void ExtractCheckConstaints()
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

    private SqlValueType CreateValueType(IDataRecord row,
      int typeNameIndex, int precisionIndex, int scaleIndex, int charLengthIndex)
    {
      string typeName = row.GetString(typeNameIndex).ToUpperInvariant();
      string columnName = row.GetString(6).ToUpperInvariant();

      int precision = row.IsDBNull(precisionIndex) ? DefaultPrecision : ReadInt(row, precisionIndex);
      int scale = row.IsDBNull(scaleIndex) ? DefaultScale : ReadInt(row, scaleIndex);

      if (typeName=="NUMBER" || typeName=="NUMERIC" || typeName=="DOUBLE" || typeName=="REAL" || typeName=="DECIMAL") {
        return new SqlValueType(SqlType.Decimal, precision, scale);
      }

      if (columnName=="TINYINT(1)") {
        return new SqlValueType(SqlType.Boolean);
      }


      if (typeName.StartsWith("TINYINT")) {
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Int8);
      }

      if (typeName.StartsWith("SMALLINT")) {
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Int16);
      }

      if (typeName.StartsWith("MEDIUMINT")) //There is not 34bit Int in SqlType
      {
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Int32);
      }

      if (typeName.StartsWith("INT")) {
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Int32);
      }

      if (typeName.StartsWith("BIGINT")) {
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Int64);
      }

      if (typeName.StartsWith("TIME")) {
        // "timestamp precision" is saved as "scale", ignoring too
        return new SqlValueType(SqlType.DateTime);
      }
      if (typeName.StartsWith("YEAR")) {
        // "timestamp precision" is saved as "scale", ignoring too
        return new SqlValueType(SqlType.Decimal, 4, 0);
      }

      if (typeName=="LONGTEXT") {
        return new SqlValueType(SqlType.VarCharMax);
      }

      if (typeName.Contains("TEXT")) {
        int length = ReadInt(row, charLengthIndex);
        return new SqlValueType(SqlType.VarCharMax, length);
      }

      if (typeName.Contains("BLOB")) {
        return new SqlValueType(SqlType.VarBinaryMax);
      }

      if (typeName=="VARBINARY") {
        int length = ReadInt(row, charLengthIndex);
        return new SqlValueType(SqlType.VarBinary, length);
      }

      if (typeName=="VARCHAR" || typeName=="CHAR") {
        int length = Convert.ToInt32(row[charLengthIndex]);
        SqlType sqlType = typeName.Length==4 ? SqlType.Char : SqlType.VarChar;
        return new SqlValueType(sqlType, length);
      }
      DataTypeInfo typeInfo = Driver.ServerInfo.DataTypes[typeName];
      return typeInfo!=null
        ? new SqlValueType(typeInfo.Type)
        : new SqlValueType(typeName);
    }

    private static void CreateIndexBasedConstraint(
      Table table, string constraintName, string constraintType, List<TableColumn> columns)
    {
      switch (constraintType) {
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
      string value = row.GetString(index);
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

    private static bool IsFullTextIndex(IDataRecord row, int index)
    {
      string value = ReadStringOrNull(row, index);
      if (!string.IsNullOrEmpty(value)) {
        value = value.ToUpperInvariant();
        return value=="FULLTEXT";
      }
      return false;
    }

    private static bool ReadAutoIncrement(IDataRecord row, int index)
    {
      string value = ReadStringOrNull(row, index);
      if (!string.IsNullOrEmpty(value)) {
        value = value.ToUpperInvariant();
        return value=="AUTO_INCREMENT";
      }
      return false;
    }

    private static long ReadLong(IDataRecord row, int index)
    {
      decimal value = row.GetDecimal(index);
      return value > long.MaxValue ? long.MaxValue : (long) value;
    }

    private static int ReadInt(IDataRecord row, int index)
    {
      decimal value = row.GetDecimal(index);
      return value > int.MaxValue ? int.MaxValue : (int) value;
    }

    private static string ReadStringOrNull(IDataRecord row, int index)
    {
      return row.IsDBNull(index) ? null : row.GetString(index);
    }

    private static void ReadCascadeAction(ForeignKey foreignKey, IDataRecord row, int deleteRuleIndex)
    {
      string deleteRule = row.GetString(deleteRuleIndex);
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

    protected virtual void RegisterReplacements(Dictionary<string, string> replacements)
    {
      string schemaFilter = targetSchema!=null
        ? "= " + SqlHelper.QuoteString(targetSchema)
        : "NOT IN ('INFORMATION_SCHEMA', 'MYSQL', 'PERFORMANCE_SCHEMA')";

      replacements[SchemaFilterPlaceholder] = schemaFilter;
      replacements[IndexesFilterPlaceholder] = "1 > 0";
      replacements[TableFilterPlaceholder] = "IS NOT NULL";
    }

    private string PerformReplacements(string query)
    {
      foreach (var registry in replacementsRegistry)
        query = query.Replace(registry.Key, registry.Value);
      return query;
    }

    protected override DbDataReader ExecuteReader(string commandText)
    {
      return base.ExecuteReader(PerformReplacements(commandText));
    }

    protected override DbDataReader ExecuteReader(ISqlCompileUnit statement)
    {
      string commandText = Connection.Driver.Compile(statement).GetCommandText();
      return base.ExecuteReader(PerformReplacements(commandText));
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}