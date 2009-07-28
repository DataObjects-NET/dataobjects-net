// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Collections.Generic;
using System.Data;
using Xtensive.Core.Threading;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.Oracle.Resources;

namespace Xtensive.Sql.Oracle.v09
{
  internal class Extractor : Model.Extractor
  {
    private static SqlRow systemUsers = SqlDml.Row("SYS", "SYSTEM", "SYSMAN", "DBSNMP");
    private static ThreadSafeCached<Schema> dataDictionaryCached = ThreadSafeCached<Schema>.Create(new object());
    private Schema dataDictionary;

    private Catalog currentCatalog;
    private string currentCatalogName;

    private string schemaFilter;
    
    protected override void Initialize()
    {
      dataDictionary = dataDictionaryCached.GetValue(BuildDataDictionary);

      currentCatalogName = Connection.Url.GetDatabase();
      currentCatalog = new Catalog(currentCatalogName);
    }

    public override Catalog ExtractAllSchemas()
    {
      schemaFilter = null;
      ExtractSchemas();
      ExtractCatalogContents();
      return currentCatalog;
    }

    public override Catalog ExtractDefaultSchema()
    {
      schemaFilter = GetDefaultSchemaName();
      currentCatalog.CreateSchema(schemaFilter);
      ExtractCatalogContents();
      return currentCatalog;
    }

    private void ExtractCatalogContents()
    {
      ExtractTables();
      ExtractViews();
      ExtractColumns();
      ExtractIndexes();
      ExtractForeignKeys();
      ExtractCheckConstaints();
      ExtractForeignKeys();
      ExtractUniqueAndPrimaryKeyConstraints();
      ExtractCheckConstaints();
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
      using (var command = CreateCommand(select))
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          currentCatalog.CreateSchema(reader.GetString(0));
        }
      }
      // choosing the default schema
      var defaultSchema = currentCatalog.Schemas[GetDefaultSchemaName()];
      currentCatalog.DefaultSchema = defaultSchema;
    }

    private void ExtractTables()
    {
      var allTables = SqlDml.TableRef(dataDictionary.Views["ALL_TABLES"]);
      var select = SqlDml.Select(allTables);
      select.Columns.Add(allTables["OWNER"]);
      select.Columns.Add(allTables["TABLE_NAME"]);
      select.Columns.Add(allTables["TEMPORARY"]);
      select.Columns.Add(allTables["DURATION"]);
      select.Where = allTables["NESTED"]=="NO";
      AddSchemaFilter(select);

      using (var command = CreateCommand(select))
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          var schema = currentCatalog.Schemas[reader.GetString(0)];
          string tableName = reader.GetString(1);
          bool isTemporary = ReadBooleanString(reader, 2);
          if (isTemporary) {
            var table = schema.CreateTemporaryTable(tableName);
            string duration = reader.GetString(3);
            table.PreserveRows = duration=="SYS$SESSION";
            table.IsGlobal = true;
          }
          else {
            schema.CreateTable(tableName);
          }
        }
      }
    }

    private void ExtractColumns()
    {
    }

    private void ExtractViews()
    {
    }

    private void ExtractIndexes()
    {
    }

    private void ExtractForeignKeys()
    {
    }

    private void ExtractUniqueAndPrimaryKeyConstraints()
    {
      var allConstraints = SqlDml.TableRef(dataDictionary.Views["ALL_CONSTRAINTS"]);
      var allIndColumns = SqlDml.TableRef(dataDictionary.Views["ALL_IND_COLUMNS"]);
      var join = SqlDml.Join(SqlJoinType.InnerJoin, allConstraints, allIndColumns,
        allConstraints["INDEX_NAME"]==allIndColumns["INDEX_NAME"] &
        allConstraints["INDEX_OWNER"]==allIndColumns["INDEX_OWNER"]);
      var select = SqlDml.Select(join);
      select.Columns.Add(allConstraints["OWNER"]);
      select.Columns.Add(allConstraints["TABLE_NAME"]);
      select.Columns.Add(allConstraints["CONSTRAINT_NAME"]);
      select.Columns.Add(allConstraints["CONSTRAINT_TYPE"]);
      select.Columns.Add(allIndColumns["COLUMN_NAME"]);
      select.Columns.Add(allIndColumns["COLUMN_POSITION"]);
      select.Where = SqlDml.In(allConstraints["CONSTRAINT_TYPE"], SqlDml.Row("P", "U"));
      AddSchemaFilter(select);
      select.OrderBy.Add(allConstraints["OWNER"]);
      select.OrderBy.Add(allConstraints["TABLE_NAME"]);
      select.OrderBy.Add(allConstraints["CONSTRAINT_NAME"]);
      select.OrderBy.Add(allIndColumns["COLUMN_POSITION"]);

      using (var command = CreateCommand(select))
      using (var reader = command.ExecuteReader()) {
        Table table = null;
        string constraintName = null;
        string constraintType = null;
        var columns = new List<TableColumn>();
        int lastColumnPosition = -1;
        while (reader.Read()) {
          int columnPosition = reader.GetInt32(5);
          if (columnPosition < lastColumnPosition) {
            CreateIndexBasedConstraint(table, constraintName, constraintType, columns);
            columns.Clear();
          }
          if (columns.Count==0) {
            var schema = currentCatalog.Schemas[reader.GetString(0)];
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
      select.Where = allConstraints["CONSTRAINT_TYPE"]=="C";
      AddSchemaFilter(select);

      using (var command = CreateCommand(select))
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          var schema = currentCatalog.Schemas[reader.GetString(0)];
          var table = schema.Tables[reader.GetString(1)];
          table.CreateCheckConstraint(reader.GetString(2), SqlDml.Native(reader.GetString(3)));
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
      AddSchemaFilter(select, allSequences["SEQUENCE_OWNER"]);

      using (var command = CreateCommand(select))
      using (var reader = command.ExecuteReader()) {
        while (reader.Read()) {
          var schema = currentCatalog.Schemas[reader.GetString(0)];
          var sequence = schema.CreateSequence(reader.GetString(1));
          // sequence.DataType = ???
          var descriptor = sequence.SequenceDescriptor;
          descriptor.MinValue = reader.GetInt64(2);
          descriptor.MaxValue = reader.GetInt64(3);
          descriptor.Increment = reader.GetInt64(4);
          descriptor.IsCyclic = ReadBooleanString(reader, 5);
        }
      }
    }

    private static Schema BuildDataDictionary()
    {
      // not all columns have been added to this views
      // if you miss something check oracle documentation first

      var catalog = new Catalog(string.Empty);
      var schema = catalog.CreateSchema("SYS");
      var voidDefintion = SqlDml.Native(string.Empty);

      var allViews = schema.CreateView("ALL_VIEWS", voidDefintion);
      allViews.CreateColumn("OWNER");
      allViews.CreateColumn("VIEW_NAME");
      allViews.CreateColumn("TEXT");

      var allTables = schema.CreateView("ALL_TABLES", voidDefintion);
      allTables.CreateColumn("OWNER");
      allTables.CreateColumn("TABLE_NAME");
      allTables.CreateColumn("TEMPORARY");
      allTables.CreateColumn("DURATION");
      allTables.CreateColumn("NESTED");

      var allIndexes = schema.CreateView("ALL_INDEXES", voidDefintion);
      allIndexes.CreateColumn("OWNER");
      allIndexes.CreateColumn("INDEX_NAME");
      allIndexes.CreateColumn("TABLE_OWNER");
      allIndexes.CreateColumn("TABLE_NAME");
      allIndexes.CreateColumn("UNIQUENESS");

      var allTabColumns = schema.CreateView("ALL_TAB_COLUMNS", voidDefintion);
      allTabColumns.CreateColumn("OWNER");
      allTabColumns.CreateColumn("TABLE_NAME");
      allTabColumns.CreateColumn("COLUMN_NAME");
      allTabColumns.CreateColumn("DATA_TYPE");
      allTabColumns.CreateColumn("DATA_TYPE_MOD");
      allTabColumns.CreateColumn("DATA_TYPE_OWNER");
      allTabColumns.CreateColumn("DATA_LENGTH");
      allTabColumns.CreateColumn("DATA_PRECISION");
      allTabColumns.CreateColumn("DATA_DEFAULT");
      allTabColumns.CreateColumn("NULLABLE");
      allTabColumns.CreateColumn("COLUMN_ID");

      var allIndColumns = schema.CreateView("ALL_IND_COLUMNS", voidDefintion);
      allIndColumns.CreateColumn("INDEX_OWNER");
      allIndColumns.CreateColumn("INDEX_NAME");
      allIndColumns.CreateColumn("TABLE_OWNER");
      allIndColumns.CreateColumn("TABLE_NAME");
      allIndColumns.CreateColumn("COLUMN_NAME");
      allIndColumns.CreateColumn("COLUMN_POSITION");
      allIndColumns.CreateColumn("CHAR_LENGTH");
      allIndColumns.CreateColumn("DESCEND");

      var allUsers = schema.CreateView("ALL_USERS", voidDefintion);
      allUsers.CreateColumn("USERNAME");
      allUsers.CreateColumn("USER_ID");
      allUsers.CreateColumn("CREATED");

      var allSequences = schema.CreateView("ALL_SEQUENCES", voidDefintion);
      allSequences.CreateColumn("SEQUENCE_OWNER");
      allSequences.CreateColumn("SEQUENCE_NAME");
      allSequences.CreateColumn("MIN_VALUE");
      allSequences.CreateColumn("MAX_VALUE");
      allSequences.CreateColumn("INCREMENT_BY");
      allSequences.CreateColumn("CYCLE_FLAG");
      allSequences.CreateColumn("ORDER_FLAG");
      allSequences.CreateColumn("CACHE_SIZE");
      allSequences.CreateColumn("LAST_NUMBER");

      var allConstraints = schema.CreateView("ALL_CONSTRAINTS", voidDefintion);
      allConstraints.CreateColumn("OWNER");
      allConstraints.CreateColumn("CONSTRAINT_NAME");
      allConstraints.CreateColumn("CONSTRAINT_TYPE");
      allConstraints.CreateColumn("SEARCH_CONDITION");
      allConstraints.CreateColumn("TABLE_NAME");
      allConstraints.CreateColumn("DELETE_RULE");
      allConstraints.CreateColumn("STATUS");
      allConstraints.CreateColumn("DEFFERABLE");
      allConstraints.CreateColumn("DEFFERED");
      allConstraints.CreateColumn("INDEX_NAME");
      allConstraints.CreateColumn("INDEX_OWNER");

      return schema;
    }

    private void AddSchemaFilter(SqlSelect select)
    {
      var filteredColumn = select.From["OWNER"];
      AddSchemaFilter(select, filteredColumn);
    }

    private void AddSchemaFilter(SqlSelect select, SqlExpression filteredColumn)
    {
      if (schemaFilter!=null)
        select.Where &= filteredColumn==schemaFilter;
      else
        select.Where &= SqlDml.NotIn(filteredColumn, systemUsers);
    }

    private string GetDefaultSchemaName()
    {
      return Connection.Url.GetSchema(Connection.Url.User).ToUpperInvariant();
    }

    private static bool ReadBooleanString(IDataRecord reader, int index)
    {
      var value = reader.GetString(index);
      switch (value) {
      case "Y":
      case "YES":
      case "ENABLED":
      case "UNIQUE":
      case "DEFFERABLE":
      case "VALIDATED":
      case "DEFFERED":
        return true;
      case "N":
      case "NO":
      case "DISABLED":
      case "NONUNIQUE":
      case "NOT DEFFERABLE":
      case "NOT VALIDATED":
      case "IMMEDIATE":
        return false;
      default:
        throw new ArgumentOutOfRangeException(string.Format(Strings.ExInvalidBooleanStringX, value));
      }
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

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}