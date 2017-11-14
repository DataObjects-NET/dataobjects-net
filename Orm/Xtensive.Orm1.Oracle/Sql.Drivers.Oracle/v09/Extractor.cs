// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Xtensive.Core;
using Xtensive.Sql.Model;
using Xtensive.Sql.Drivers.Oracle.Resources;
using Constraint=Xtensive.Sql.Model.Constraint;

namespace Xtensive.Sql.Drivers.Oracle.v09
{
  internal partial class Extractor : Model.Extractor
  {
    private const int DefaultPrecision = 38;
    private const int DefaultScale = 0;

    private readonly HashSet<string> targetSchemes = new HashSet<string>();
    private readonly Dictionary<string, string> replacementsRegistry = new Dictionary<string, string>();

    private Catalog theCatalog;

    private string nonSystemSchemasFilter;

    public override Catalog ExtractCatalog(string catalogName)
    {
      theCatalog = new Catalog(catalogName);
      targetSchemes.Clear();

      RegisterReplacements(replacementsRegistry);
      ExtractSchemas();
      ExtractCatalogContents();
      return theCatalog;
    }

    public override Schema ExtractSchema(string catalogName, string schemaName)
    {
      targetSchemes.Clear();
      theCatalog = new Catalog(catalogName);
      var targetSchema = schemaName.ToUpperInvariant();
      targetSchemes.Add(targetSchema);

      RegisterReplacements(replacementsRegistry);
      ExtractSchemas();
      EnsureShemasExists(theCatalog, new []{schemaName});
      ExtractCatalogContents();
      return theCatalog.Schemas[targetSchema];
    }

    public override Catalog ExtractSchemes(string catalogName, string[] schemaNames)
    {
      theCatalog = new Catalog(catalogName);
      targetSchemes.Clear();
      foreach (var schemaName in schemaNames) {
        var targetSchema = schemaName.ToUpperInvariant();
        targetSchemes.Add(targetSchema);
      }

      RegisterReplacements(replacementsRegistry);
      ExtractSchemas();
      EnsureShemasExists(theCatalog, schemaNames);
      ExtractCatalogContents();
      return theCatalog;
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
      // oracle does not clearly distinct users and schemas.
      // so we extract just users.
      using (var reader = ExecuteReader(GetExtractSchemasQuery()))
        while (reader.Read())
          theCatalog.CreateSchema(reader.GetString(0));
      // choosing the default schema
      var defaultSchemaName = Driver.CoreServerInfo.DefaultSchemaName.ToUpperInvariant();
      var defaultSchema = theCatalog.Schemas[defaultSchemaName];
      theCatalog.DefaultSchema = defaultSchema;
    }

    private void ExtractTables()
    {
      using (var reader = ExecuteReader(GetExtractTablesQuery())) {
        while (reader.Read()) {
          var schema = theCatalog.Schemas[reader.GetString(0)];
          string tableName = reader.GetString(1);
          bool isTemporary = ReadBool(reader, 2);
          if (isTemporary) {
            var table = schema.CreateTemporaryTable(tableName);
            table.PreserveRows = reader.GetString(3)=="SYS$SESSION";
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
      using (var reader = ExecuteReader(GetExtractTableColumnsQuery())) {
        Table table = null;
        int lastColumnId = int.MaxValue;
        while (reader.Read()) {
          int columnId = ReadInt(reader, 9);
          if (columnId <= lastColumnId) {
            var schema = theCatalog.Schemas[reader.GetString(0)];
            table = schema.Tables[reader.GetString(1)];
          }
          var column = table.CreateColumn(reader.GetString(2));
          column.DataType = CreateValueType(reader, 3, 4, 5, 6);
          column.IsNullable = ReadBool(reader, 7);
          var defaultValue = ReadStringOrNull(reader, 8);
          if (!string.IsNullOrEmpty(defaultValue))
            column.DefaultValue = SqlDml.Native(defaultValue);
          lastColumnId = columnId;
        }
      }
    }

    private void ExtractViews()
    {
      using (var reader = ExecuteReader(GetExtractViewsQuery())) {
        while (reader.Read()) {
          var schema = theCatalog.Schemas[reader.GetString(0)];
          var view = reader.GetString(1);
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
      using (var reader = ExecuteReader(GetExtractViewColumnsQuery())) {
        int lastColumnId = int.MaxValue;
        View view = null;
        while (reader.Read()) {
          int columnId = ReadInt(reader, 3);
          if (columnId <= lastColumnId) {
            var schema = theCatalog.Schemas[reader.GetString(0)];
            view = schema.Views[reader.GetString(1)];
          }
          view.CreateColumn(reader.GetString(2));
          lastColumnId = columnId;
        }
      }
    }

    private void ExtractIndexes()
    {
      // it's possible to have table and index in different schemas in oracle.
      // we silently ignore this, indexes are always belong to the same schema as its table.
      using (var reader = (OracleDataReader) ExecuteReader(GetExtractIndexesQuery())) {
        int lastColumnPosition = int.MaxValue;
        Table table = null;
        Index index = null;
        while (reader.Read()) {
          int columnPosition = ReadInt(reader, 6);
          if (columnPosition <= lastColumnPosition) {
            var schema = theCatalog.Schemas[reader.GetString(0)];
            table = schema.Tables[reader.GetString(1)];
            index = table.CreateIndex(reader.GetString(2));
            index.IsUnique = ReadBool(reader, 3);
            index.IsBitmap = reader.GetString(4).EndsWith("BITMAP");
            if (!reader.IsDBNull(5)) {
              int pctFree = ReadInt(reader, 5);
              index.FillFactor = (byte) (100 - pctFree);
            }
          }
          var columnName = reader.IsDBNull(9) ? reader.GetString(7) : reader.GetOracleString(9).Value;
          columnName = columnName.Trim('"');
          var column = table.TableColumns[columnName];
          bool isAscending = reader.GetString(8)=="ASC";
          index.CreateIndexColumn(column, isAscending);
          lastColumnPosition = columnPosition;
        }
      }
    }

    private void ExtractForeignKeys()
    {
      using (var reader = ExecuteReader(GetExtractForeignKeysQuery())) {
        int lastColumnPosition = int.MaxValue;
        ForeignKey constraint = null;
        Table referencingTable = null;
        Table referencedTable = null;
        while (reader.Read()) {
          int columnPosition = ReadInt(reader, 7);
          if (columnPosition <= lastColumnPosition) {
            var referencingSchema = theCatalog.Schemas[reader.GetString(0)];
            referencingTable = referencingSchema.Tables[reader.GetString(1)];
            constraint = referencingTable.CreateForeignKey(reader.GetString(2));
            ReadConstraintProperties(constraint, reader, 3, 4);
            ReadCascadeAction(constraint, reader, 5);
            var referencedSchema = theCatalog.Schemas[reader.GetString(8)];
            referencedTable = referencedSchema.Tables[reader.GetString(9)];
            constraint.ReferencedTable = referencedTable;
          }
          var referencingColumn = referencingTable.TableColumns[reader.GetString(6)];
          var referencedColumn = referencedTable.TableColumns[reader.GetString(10)];
          constraint.Columns.Add(referencingColumn);
          constraint.ReferencedColumns.Add(referencedColumn);
          lastColumnPosition = columnPosition;
        }
      }
    }

    private void ExtractUniqueAndPrimaryKeyConstraints()
    {
      using (var reader = ExecuteReader(GetExtractUniqueAndPrimaryKeyConstraintsQuery())) {
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
            var schema = theCatalog.Schemas[reader.GetString(0)];
            table = schema.Tables[reader.GetString(1)];
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

    private void ExtractCheckConstaints()
    {
      using (var reader = ExecuteReader(GetExtractCheckConstraintsQuery())) {
        while (reader.Read()) {
          var schema = theCatalog.Schemas[reader.GetString(0)];
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
      }
    }

    private void ExtractSequences()
    {
      using (var reader = ExecuteReader(GetExtractSequencesQuery())) {
        while (reader.Read()) {
          var schema = theCatalog.Schemas[reader.GetString(0)];
          var sequence = schema.CreateSequence(reader.GetString(1));
          var descriptor = sequence.SequenceDescriptor;
          descriptor.MinValue = ReadLong(reader, 2);
          descriptor.MaxValue = ReadLong(reader, 3);
          descriptor.Increment = ReadLong(reader, 4);
          descriptor.IsCyclic = ReadBool(reader, 5);
        }
      }
    }

    private SqlValueType CreateValueType(IDataRecord row,
      int typeNameIndex, int precisionIndex, int scaleIndex, int charLengthIndex)
    {
      string typeName = row.GetString(typeNameIndex);
      if (typeName=="NUMBER") {
        int precision = row.IsDBNull(precisionIndex) ? DefaultPrecision : ReadInt(row, precisionIndex);
        int scale = row.IsDBNull(scaleIndex) ? DefaultScale : ReadInt(row, scaleIndex);
        return new SqlValueType(SqlType.Decimal, precision, scale);
      }
      if (typeName.StartsWith("INTERVAL DAY")) {
        // ignoring "day precision" and "second precision"
        // although they can be read as "scale" and "precision"
        return new SqlValueType(SqlType.Interval);
      }
      if (typeName.StartsWith("TIMESTAMP")) {
        // "timestamp precision" is saved as "scale", ignoring too
        if (typeName.Contains("WITH TIME ZONE"))
          return new SqlValueType(SqlType.DateTimeOffset);
        return new SqlValueType(SqlType.DateTime);
      }
      if (typeName=="NVARCHAR2" || typeName=="NCHAR") {
        int length = ReadInt(row, charLengthIndex);
        var sqlType = typeName.Length==5 ? SqlType.Char : SqlType.VarChar;
        return new SqlValueType(sqlType, length);
      }
      var typeInfo = Driver.ServerInfo.DataTypes[typeName];
      return typeInfo!=null
        ? new SqlValueType(typeInfo.Type)
        : new SqlValueType(typeName);
    }

    private static void CreateIndexBasedConstraint(
      Table table, string constraintName, string constraintType, List<TableColumn> columns)
    {
      switch (constraintType) {
      case "P":
        table.CreatePrimaryKey(constraintName, columns.ToArray());
        return;
      case "U":
        table.CreateUniqueConstraint(constraintName, columns.ToArray());
        return;
      default:
        throw new ArgumentOutOfRangeException("constraintType");
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
      decimal value = row.GetDecimal(index);

      if (value > long.MaxValue)
        return long.MaxValue;

      if (value < long.MinValue)
        return long.MinValue;

      return (long)value;
    }

    private static int ReadInt(IDataRecord row, int index)
    {
      decimal value = row.GetDecimal(index);

      if (value > int.MaxValue)
        return int.MaxValue;

      if (value < int.MinValue)
        return int.MinValue;

      return (int)value;
    }

    private static string ReadStringOrNull(IDataRecord row, int index)
    {
      return row.IsDBNull(index) ? null : row.GetString(index);
    }

    private static void ReadConstraintProperties(Constraint constraint,
      IDataRecord row, int isDeferrableIndex,  int isInitiallyDeferredIndex)
    {
      constraint.IsDeferrable = row.GetString(isDeferrableIndex)=="DEFERRABLE";
      constraint.IsInitiallyDeferred = row.GetString(isInitiallyDeferredIndex)=="DEFERRED";
    }

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
    
    protected virtual void RegisterReplacements(Dictionary<string, string> replacements)
    {
      var schemaFilter = targetSchemes!=null && targetSchemes.Count!=0
        ? MakeSchemaFilter()
        //? "= " + SqlHelper.QuoteString(targetSchema)
        : GetNonSystemSchemasFilter();
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

    private string MakeSchemaFilter()
    {
      var schemaStrings = targetSchemes.Select(SqlHelper.QuoteString);
      var schemaList = string.Join(",", schemaStrings);
      return string.Format("IN ({0})", schemaList);
    }

    protected override DbDataReader ExecuteReader(string commandText)
    {
      var realCommandText = PerformReplacements(commandText);
      using (var command = (OracleCommand) Connection.CreateCommand(realCommandText)) {
        // This option is required to access LONG columns
        command.InitialLONGFetchSize = -1;
        return command.ExecuteReader();
      }
    }

    protected override DbDataReader ExecuteReader(ISqlCompileUnit statement)
    {
      var commandText = Connection.Driver.Compile(statement).GetCommandText();
      return base.ExecuteReader(PerformReplacements(commandText));
    }

    protected virtual IEnumerable<string> GetSystemSchemas()
    {
      return new[] {
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
    }

    private string GetNonSystemSchemasFilter()
    {
      if (nonSystemSchemasFilter==null) {
        var schemaStrings = GetSystemSchemas().Select(SqlHelper.QuoteString).ToArray();
        var schemaList = string.Join(",", schemaStrings);
        nonSystemSchemasFilter = string.Format("NOT IN ({0})", schemaList);
      }
      return nonSystemSchemasFilter;
    }

    private void EnsureShemasExists(Catalog catalog, string[] schemaNames)
    {
      foreach (var schemaName in schemaNames) {
        var realSchemaName = schemaName.ToUpperInvariant();
        var schema = catalog.Schemas[realSchemaName];
        if (schema==null)
          catalog.CreateSchema(realSchemaName);
      }
    }


    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}