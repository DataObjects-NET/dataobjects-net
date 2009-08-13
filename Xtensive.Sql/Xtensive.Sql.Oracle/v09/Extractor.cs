// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xtensive.Core.Threading;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.Oracle.Resources;
using Constraint=Xtensive.Sql.Model.Constraint;

namespace Xtensive.Sql.Oracle.v09
{
  internal class Extractor : Model.Extractor
  {
    private const int DefaultPrecision = 38;
    private const int DefaultScale = 0;
    
    private static SqlRow systemUsers = SqlDml.Row(
      AnsiString("SYS"), AnsiString("SYSTEM"), AnsiString("SYSMAN"), AnsiString("DBSNMP"));

    private static ThreadSafeCached<Schema> dataDictionaryCached = ThreadSafeCached<Schema>.Create(new object());

    private Schema dataDictionary;
    private Catalog theCatalog;
    private SqlExpression schemaFilter;

    protected override void Initialize()
    {
      dataDictionary = dataDictionaryCached.GetValue(BuildDataDictionary);
      theCatalog = new Catalog(Connection.Url.GetDatabase());
    }

    public override Catalog ExtractCatalog()
    {
      schemaFilter = null;
      ExtractSchemas();
      ExtractCatalogContents();
      return theCatalog;
    }

    protected override Schema ExtractSchema()
    {
      var schemaName = GetDefaultSchemaName();
      theCatalog.CreateSchema(schemaName);
      schemaFilter = AnsiString(schemaName);
      ExtractCatalogContents();
      return theCatalog.Schemas[schemaName];
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
      var allUsers = SqlDml.TableRef(dataDictionary.Views["ALL_USERS"]);
      var select = SqlDml.Select(allUsers);
      select.Columns.Add(allUsers["USERNAME"]);
      select.Where = SqlDml.NotIn(allUsers["USERNAME"], systemUsers);
      using (var reader = ExecuteReader(select)) {
        while (reader.Read()) {
          theCatalog.CreateSchema(reader.GetString(0));
        }
      }
      // choosing the default schema
      var defaultSchema = theCatalog.Schemas[GetDefaultSchemaName()];
      theCatalog.DefaultSchema = defaultSchema;
    }

    private void ExtractTables()
    {
      var allTables = SqlDml.TableRef(dataDictionary.Views["ALL_TABLES"]);
      var select = SqlDml.Select(allTables);
      select.Columns.Add(allTables["OWNER"]);
      select.Columns.Add(allTables["TABLE_NAME"]);
      select.Columns.Add(allTables["TEMPORARY"]);
      select.Columns.Add(allTables["DURATION"]);
      select.Where = allTables["NESTED"]==AnsiString("NO");
      ApplySchemaFilter(select);

      using (var reader = ExecuteReader(select)) {
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
      var allTabColumns = SqlDml.TableRef(dataDictionary.Views["ALL_TAB_COLUMNS"]);
      var allTables = SqlDml.TableRef(dataDictionary.Views["ALL_TABLES"]);
      var select = SqlDml.Select(SqlDml.Join(SqlJoinType.InnerJoin, allTabColumns, allTables,
        allTabColumns["TABLE_NAME"]==allTables["TABLE_NAME"] &
        allTabColumns["OWNER"]==allTables["OWNER"]));

      select.Columns.Add(allTabColumns["OWNER"]);
      select.Columns.Add(allTabColumns["TABLE_NAME"]);
      select.Columns.Add(allTabColumns["COLUMN_NAME"]);
      select.Columns.Add(allTabColumns["DATA_TYPE"]);
      select.Columns.Add(allTabColumns["DATA_PRECISION"]);
      select.Columns.Add(allTabColumns["DATA_SCALE"]);
      select.Columns.Add(allTabColumns["CHAR_LENGTH"]);
      select.Columns.Add(allTabColumns["NULLABLE"]);
      select.Columns.Add(allTabColumns["DATA_DEFAULT"]);
      select.Columns.Add(allTabColumns["COLUMN_ID"]);

      select.OrderBy.Add(allTabColumns["OWNER"]);
      select.OrderBy.Add(allTabColumns["TABLE_NAME"]);
      select.OrderBy.Add(allTabColumns["COLUMN_ID"]);

      ApplySchemaFilter(select, allTabColumns["OWNER"]);

      using (var reader = ExecuteReader(select)) {
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
      var allViews = SqlDml.TableRef(dataDictionary.Views["ALL_VIEWS"]);
      var select = SqlDml.Select(allViews);
      select.Columns.Add(allViews["OWNER"]);
      select.Columns.Add(allViews["VIEW_NAME"]);
      select.Columns.Add(allViews["TEXT"]);
      ApplySchemaFilter(select);

      using (var reader = ExecuteReader(select)) {
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
      var allTabColumns = SqlDml.TableRef(dataDictionary.Views["ALL_TAB_COLUMNS"]);
      var allViews = SqlDml.TableRef(dataDictionary.Views["ALL_VIEWS"]);
      var select = SqlDml.Select(SqlDml.Join(SqlJoinType.InnerJoin, allTabColumns, allViews,
        allTabColumns["TABLE_NAME"]==allViews["VIEW_NAME"] &
        allTabColumns["OWNER"]==allViews["OWNER"]));

      select.Columns.Add(allTabColumns["OWNER"]);
      select.Columns.Add(allTabColumns["TABLE_NAME"]);
      select.Columns.Add(allTabColumns["COLUMN_NAME"]);
      select.Columns.Add(allTabColumns["COLUMN_ID"]);

      select.OrderBy.Add(allTabColumns["OWNER"]);
      select.OrderBy.Add(allTabColumns["TABLE_NAME"]);
      select.OrderBy.Add(allTabColumns["COLUMN_ID"]);

      ApplySchemaFilter(select, allTabColumns["OWNER"]);

      using (var reader = ExecuteReader(select)) {
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
      var allIndexes = SqlDml.TableRef(dataDictionary.Views["ALL_INDEXES"]);
      var allIndColumns = SqlDml.TableRef(dataDictionary.Views["ALL_IND_COLUMNS"]);
      var select = SqlDml.Select(SqlDml.Join(SqlJoinType.InnerJoin, allIndColumns, allIndexes,
        allIndexes["INDEX_NAME"]==allIndColumns["INDEX_NAME"] &
        allIndexes["OWNER"]==allIndColumns["INDEX_OWNER"]));

      select.Columns.Add(allIndexes["TABLE_OWNER"]);
      select.Columns.Add(allIndexes["TABLE_NAME"]);
      select.Columns.Add(allIndexes["INDEX_NAME"]);
      select.Columns.Add(allIndexes["UNIQUENESS"]);
      select.Columns.Add(allIndexes["INDEX_TYPE"]);
      select.Columns.Add(allIndexes["PCT_FREE"]);
      select.Columns.Add(allIndColumns["COLUMN_POSITION"]);
      select.Columns.Add(allIndColumns["COLUMN_NAME"]);
      select.Columns.Add(allIndColumns["DESCEND"]);

      select.OrderBy.Add(allIndexes["TABLE_OWNER"]);
      select.OrderBy.Add(allIndexes["TABLE_NAME"]);
      select.OrderBy.Add(allIndexes["INDEX_NAME"]);
      select.OrderBy.Add(allIndColumns["COLUMN_POSITION"]);

      var allConstraints = SqlDml.TableRef(dataDictionary.Views["ALL_CONSTRAINTS"]);
      var ignoredIndexes = SqlDml.Select(allConstraints);
      ignoredIndexes.Columns.Add(allConstraints["INDEX_OWNER"]);
      ignoredIndexes.Columns.Add(allConstraints["INDEX_NAME"]);
      ignoredIndexes.Where =
        SqlDml.In(allConstraints["CONSTRAINT_TYPE"], SqlDml.Row(AnsiString("P"), AnsiString("U")));
      ApplySchemaFilter(ignoredIndexes);
      ApplyTableFilter(ignoredIndexes, allConstraints["OWNER"], allConstraints["TABLE_NAME"]);

      select.Where =
        SqlDml.In(allIndexes["INDEX_TYPE"], SqlDml.Row(AnsiString("NORMAL"), AnsiString("BITMAP"))) &
        SqlDml.NotIn(SqlDml.Row(allIndexes["OWNER"], allIndexes["INDEX_NAME"]), ignoredIndexes) &
        allIndexes["DROPPED"]==AnsiString("NO");
      ApplySchemaFilter(select, allIndexes["TABLE_OWNER"]);

      using (var reader = ExecuteReader(select)) {
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
            index.IsBitmap = reader.GetString(4)=="BITMAP";
            if (!reader.IsDBNull(5)) {
              int pctFree = ReadInt(reader, 5);
              index.FillFactor = (byte) (100 - pctFree);
            }
          }
          var column = table.TableColumns[reader.GetString(7)];
          bool isAscending = reader.GetString(8)=="ASC";
          index.CreateIndexColumn(column, isAscending);
          lastColumnPosition = columnPosition;
        }
      }
    }

    private void ExtractForeignKeys()
    {
      var allConstraints = SqlDml.TableRef(dataDictionary.Views["ALL_CONSTRAINTS"]);
      var referencingColumns = SqlDml.TableRef(dataDictionary.Views["ALL_CONS_COLUMNS"]);
      var referencedColumns = SqlDml.TableRef(dataDictionary.Views["ALL_CONS_COLUMNS"]);

      var select = SqlDml.Select(
        SqlDml.Join(SqlJoinType.InnerJoin,
          SqlDml.Join(SqlJoinType.InnerJoin, allConstraints, referencingColumns,
            allConstraints["CONSTRAINT_NAME"]==referencingColumns["CONSTRAINT_NAME"] &
            allConstraints["OWNER"]==referencingColumns["OWNER"]),
          referencedColumns,
          allConstraints["R_CONSTRAINT_NAME"]==referencedColumns["CONSTRAINT_NAME"] &
          allConstraints["R_OWNER"]==referencedColumns["OWNER"] & 
          referencingColumns["POSITION"]==referencedColumns["POSITION"]));
      
      select.Columns.Add(allConstraints["OWNER"]);
      select.Columns.Add(allConstraints["TABLE_NAME"]);
      select.Columns.Add(allConstraints["CONSTRAINT_NAME"]);
      select.Columns.Add(allConstraints["DEFERRABLE"]);
      select.Columns.Add(allConstraints["DEFERRED"]);
      select.Columns.Add(allConstraints["DELETE_RULE"]);
      select.Columns.Add(referencingColumns["COLUMN_NAME"]);
      select.Columns.Add(referencingColumns["POSITION"]);
      select.Columns.Add(referencedColumns["OWNER"]);
      select.Columns.Add(referencedColumns["TABLE_NAME"]);
      select.Columns.Add(referencedColumns["COLUMN_NAME"]);
      
      select.OrderBy.Add(allConstraints["OWNER"]);
      select.OrderBy.Add(allConstraints["TABLE_NAME"]);
      select.OrderBy.Add(allConstraints["CONSTRAINT_NAME"]);
      select.OrderBy.Add(referencedColumns["POSITION"]);
      
      select.Where = allConstraints["CONSTRAINT_TYPE"]==AnsiString("R");
      ApplySchemaFilter(select, allConstraints["OWNER"]);
      ApplyTableFilter(select, allConstraints["OWNER"], allConstraints["TABLE_NAME"]);

      using (var reader = ExecuteReader(select)) {
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
      var allConstraints = SqlDml.TableRef(dataDictionary.Views["ALL_CONSTRAINTS"]);
      var allConsColumns = SqlDml.TableRef(dataDictionary.Views["ALL_CONS_COLUMNS"]);

      var select = SqlDml.Select(
        SqlDml.Join(SqlJoinType.InnerJoin, allConstraints, allConsColumns,
          allConstraints["CONSTRAINT_NAME"]==allConsColumns["CONSTRAINT_NAME"] &
          allConstraints["OWNER"]==allConsColumns["OWNER"]));

      select.Columns.Add(allConstraints["OWNER"]);
      select.Columns.Add(allConstraints["TABLE_NAME"]);
      select.Columns.Add(allConstraints["CONSTRAINT_NAME"]);
      select.Columns.Add(allConstraints["CONSTRAINT_TYPE"]);
      select.Columns.Add(allConsColumns["COLUMN_NAME"]);
      select.Columns.Add(allConsColumns["POSITION"]);

      select.OrderBy.Add(allConstraints["OWNER"]);
      select.OrderBy.Add(allConstraints["TABLE_NAME"]);
      select.OrderBy.Add(allConstraints["CONSTRAINT_NAME"]);
      select.OrderBy.Add(allConsColumns["POSITION"]);

      select.Where =
        SqlDml.In(allConstraints["CONSTRAINT_TYPE"], SqlDml.Row(AnsiString("P"), AnsiString("U")));
      ApplySchemaFilter(select, allConstraints["OWNER"]);
      ApplyTableFilter(select, allConstraints["OWNER"], allConstraints["TABLE_NAME"]);

      using (var reader = ExecuteReader(select)) {
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
      var allConstraints = SqlDml.TableRef(dataDictionary.Views["ALL_CONSTRAINTS"]);
      var select = SqlDml.Select(allConstraints);
      select.Columns.Add(allConstraints["OWNER"]);
      select.Columns.Add(allConstraints["TABLE_NAME"]);
      select.Columns.Add(allConstraints["CONSTRAINT_NAME"]);
      select.Columns.Add(allConstraints["SEARCH_CONDITION"]);
      select.Columns.Add(allConstraints["DEFERRABLE"]);
      select.Columns.Add(allConstraints["DEFERRED"]);
      select.Where = allConstraints["CONSTRAINT_TYPE"]==AnsiString("C") &
        allConstraints["GENERATED"]==AnsiString("USER NAME");
      ApplySchemaFilter(select);
      ApplyTableFilter(select, allConstraints["OWNER"], allConstraints["TABLE_NAME"]);

      using (var reader = ExecuteReader(select)) {
        while (reader.Read()) {
          var schema = theCatalog.Schemas[reader.GetString(0)];
          var table = schema.Tables[reader.GetString(1)];
          var constraint = table.CreateCheckConstraint(
            reader.GetString(2), SqlDml.Native(reader.GetString(3)));
          ReadConstraintProperties(constraint, reader, 4, 5);
        }
      }
    }

    private void ExtractSequences()
    {
      var allSequences = SqlDml.TableRef(dataDictionary.Views["ALL_SEQUENCES"]);
      var select = SqlDml.Select(allSequences);
      select.Columns.Add(allSequences["SEQUENCE_OWNER"]);
      select.Columns.Add(allSequences["SEQUENCE_NAME"]);
      select.Columns.Add(allSequences["MIN_VALUE"]);
      select.Columns.Add(allSequences["MAX_VALUE"]);
      select.Columns.Add(allSequences["INCREMENT_BY"]);
      select.Columns.Add(allSequences["CYCLE_FLAG"]);
      ApplySchemaFilter(select, allSequences["SEQUENCE_OWNER"]);

      using (var reader = ExecuteReader(select)) {
        while (reader.Read()) {
          var schema = theCatalog.Schemas[reader.GetString(0)];
          var sequence = schema.CreateSequence(reader.GetString(1));
          sequence.DataType = new SqlValueType(SqlType.Decimal, DefaultPrecision, DefaultScale);
          var descriptor = sequence.SequenceDescriptor;
          descriptor.MinValue = ReadLong(reader, 2);
          descriptor.MaxValue = ReadLong(reader, 3);
          descriptor.Increment = ReadLong(reader, 4);
          descriptor.IsCyclic = ReadBool(reader, 5);
        }
      }
    }

    private void ApplySchemaFilter(SqlSelect select)
    {
      var filteredColumn = select.From["OWNER"];
      ApplySchemaFilter(select, filteredColumn);
    }

    private void ApplySchemaFilter(SqlSelect select, SqlExpression filteredColumn)
    {
      if (!schemaFilter.IsNullReference())
        select.Where &= filteredColumn==schemaFilter;
      else
        select.Where &= SqlDml.NotIn(filteredColumn, systemUsers);
    }

    private void ApplyTableFilter(SqlSelect select, SqlExpression tableOwner, SqlExpression tableName)
    {
      var allTables = SqlDml.TableRef(dataDictionary.Views["ALL_TABLES"]);
      var originalSource = select.From;
      select.From = SqlDml.Join(SqlJoinType.InnerJoin, originalSource, allTables,
        tableOwner==allTables["OWNER"] & tableName==allTables["TABLE_NAME"]);
    }

    private string GetDefaultSchemaName()
    {
      return Connection.Url.GetSchema(Connection.Url.User).ToUpperInvariant();
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

    private static SqlExpression AnsiString(string value)
    {
      return SqlDml.Native("'" + value + "'");
    }

    private static Schema BuildDataDictionary()
    {
      // not all columns have been added to this views
      // if you miss something check oracle documentation first

      var catalog = new Catalog(string.Empty);
      var schema = catalog.CreateSchema("SYS");

      var allViews = schema.CreateView("ALL_VIEWS");
      allViews.CreateColumn("OWNER");
      allViews.CreateColumn("VIEW_NAME");
      allViews.CreateColumn("TEXT");

      var allTables = schema.CreateView("ALL_TABLES");
      allTables.CreateColumn("OWNER");
      allTables.CreateColumn("TABLE_NAME");
      allTables.CreateColumn("TEMPORARY");
      allTables.CreateColumn("DURATION");
      allTables.CreateColumn("NESTED");

      var allIndexes = schema.CreateView("ALL_INDEXES");
      allIndexes.CreateColumn("OWNER");
      allIndexes.CreateColumn("INDEX_NAME");
      allIndexes.CreateColumn("INDEX_TYPE");
      allIndexes.CreateColumn("TABLE_OWNER");
      allIndexes.CreateColumn("TABLE_NAME");
      allIndexes.CreateColumn("UNIQUENESS");
      allIndexes.CreateColumn("PCT_FREE");
      allIndexes.CreateColumn("DROPPED");
      allIndexes.CreateColumn("GENERATED");

      var allTabColumns = schema.CreateView("ALL_TAB_COLUMNS");
      allTabColumns.CreateColumn("OWNER");
      allTabColumns.CreateColumn("TABLE_NAME");
      allTabColumns.CreateColumn("COLUMN_NAME");
      allTabColumns.CreateColumn("DATA_TYPE");
      allTabColumns.CreateColumn("DATA_TYPE_MOD");
      allTabColumns.CreateColumn("DATA_TYPE_OWNER");
      allTabColumns.CreateColumn("DATA_LENGTH");
      allTabColumns.CreateColumn("DATA_PRECISION");
      allTabColumns.CreateColumn("DATA_SCALE");
      allTabColumns.CreateColumn("NULLABLE");
      allTabColumns.CreateColumn("COLUMN_ID");
      allTabColumns.CreateColumn("DATA_DEFAULT");
      allTabColumns.CreateColumn("CHAR_LENGTH");
      
      var allIndColumns = schema.CreateView("ALL_IND_COLUMNS");
      allIndColumns.CreateColumn("INDEX_OWNER");
      allIndColumns.CreateColumn("INDEX_NAME");
      allIndColumns.CreateColumn("TABLE_OWNER");
      allIndColumns.CreateColumn("TABLE_NAME");
      allIndColumns.CreateColumn("COLUMN_NAME");
      allIndColumns.CreateColumn("COLUMN_POSITION");
      allIndColumns.CreateColumn("CHAR_LENGTH");
      allIndColumns.CreateColumn("DESCEND");

      var allConsColumns = schema.CreateView("ALL_CONS_COLUMNS");
      allConsColumns.CreateColumn("OWNER");
      allConsColumns.CreateColumn("CONSTRAINT_NAME");
      allConsColumns.CreateColumn("TABLE_NAME");
      allConsColumns.CreateColumn("COLUMN_NAME");
      allConsColumns.CreateColumn("POSITION");

      var allUsers = schema.CreateView("ALL_USERS");
      allUsers.CreateColumn("USERNAME");
      allUsers.CreateColumn("USER_ID");
      allUsers.CreateColumn("CREATED");

      var allSequences = schema.CreateView("ALL_SEQUENCES");
      allSequences.CreateColumn("SEQUENCE_OWNER");
      allSequences.CreateColumn("SEQUENCE_NAME");
      allSequences.CreateColumn("MIN_VALUE");
      allSequences.CreateColumn("MAX_VALUE");
      allSequences.CreateColumn("INCREMENT_BY");
      allSequences.CreateColumn("CYCLE_FLAG");
      allSequences.CreateColumn("ORDER_FLAG");
      allSequences.CreateColumn("CACHE_SIZE");
      allSequences.CreateColumn("LAST_NUMBER");

      var allConstraints = schema.CreateView("ALL_CONSTRAINTS");
      allConstraints.CreateColumn("OWNER");
      allConstraints.CreateColumn("CONSTRAINT_NAME");
      allConstraints.CreateColumn("CONSTRAINT_TYPE");
      allConstraints.CreateColumn("TABLE_NAME");
      allConstraints.CreateColumn("SEARCH_CONDITION");
      allConstraints.CreateColumn("R_OWNER");
      allConstraints.CreateColumn("R_CONSTRAINT_NAME");
      allConstraints.CreateColumn("DELETE_RULE");
      allConstraints.CreateColumn("STATUS");
      allConstraints.CreateColumn("DEFERRABLE");
      allConstraints.CreateColumn("DEFERRED");
      allConstraints.CreateColumn("GENERATED");
      allConstraints.CreateColumn("INDEX_NAME");
      allConstraints.CreateColumn("INDEX_OWNER");

      return schema;
    }
    
    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}