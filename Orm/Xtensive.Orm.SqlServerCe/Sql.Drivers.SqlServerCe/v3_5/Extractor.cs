// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.11

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Xtensive.Threading;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using UniqueConstraint=Xtensive.Sql.Model.UniqueConstraint;

namespace Xtensive.Sql.Drivers.SqlServerCe.v3_5
{
  internal class Extractor : Model.Extractor
  {
    protected Catalog catalog;
    protected Schema schema;

    protected override void Initialize()
    {
      catalog = new Catalog(Driver.CoreServerInfo.DatabaseName);
    }

    public override Catalog ExtractCatalog(string catalogName)
    {
      ExtractCatalogContents();
      return catalog;
    }

    public override Schema ExtractSchema(string catalogName, string schemaName)
    {
      schema = catalog.CreateSchema(schemaName);
      ExtractCatalogContents();
      return schema;
    }

    private void ExtractCatalogContents()
    {
      ExtractTables();
      ExtractColumns();
      ExtractIndexes();
      ExtractForeignKeys();
    }

    private void ExtractTables()
    {
      using (var cmd = Connection.CreateCommand("select TABLE_NAME from [INFORMATION_SCHEMA].[TABLES]"))
      using (IDataReader reader = cmd.ExecuteReader())
        while (reader.Read())
          schema.CreateTable(reader.GetString(0));
    }

    private void ExtractColumns()
    {
      string select = "select TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, CHARACTER_SET_NAME, COLLATION_NAME, COLUMN_DEFAULT, DOMAIN_SCHEMA, DOMAIN_NAME, AUTOINC_SEED, AUTOINC_INCREMENT from INFORMATION_SCHEMA.COLUMNS order by ORDINAL_POSITION";

      using (var cmd = Connection.CreateCommand(select))
      using (IDataReader reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          string tableName = (string) reader["TABLE_NAME"];

          SqlValueType sqlDataType = ReadDataType(reader);

          // Create new column
            Table table = schema.Tables[tableName];
            TableColumn column = table.CreateColumn((string) reader["COLUMN_NAME"], sqlDataType);
            string isNullableText = reader["IS_NULLABLE"].ToString().ToLower();
            column.IsNullable = isNullableText=="yes" || isNullableText=="true" || isNullableText=="1";

            // Collation
            if (reader["COLLATION_NAME"]!=DBNull.Value) {
              string name = (string) reader["COLLATION_NAME"];
              column.Collation = schema.Collations[name] ?? schema.CreateCollation(name);
            }

            // Default value
            if (!Convert.IsDBNull(reader["COLUMN_DEFAULT"])) {
              var defaultValue = (string) reader["COLUMN_DEFAULT"];
              if (string.IsNullOrEmpty(defaultValue) || defaultValue == "''")
                continue;
              column.DefaultValue = SqlDml.Native(defaultValue.Trim('\'', '\''));
            }

          // Autoincrement
            if (reader["AUTOINC_SEED"]!=DBNull.Value)
            column.SequenceDescriptor =
              new SequenceDescriptor(column, Convert.ToInt32(reader["AUTOINC_SEED"]),
                Convert.ToInt32(reader["AUTOINC_INCREMENT"]));

            // Domain
            if (!Convert.IsDBNull(reader["DOMAIN_SCHEMA"]) && !Convert.IsDBNull(reader["DOMAIN_NAME"])) {
              string domainSchema = (string) reader["DOMAIN_SCHEMA"];
              string domainName = (string) reader["DOMAIN_NAME"];
              column.Domain = catalog.Schemas[domainSchema].Domains[domainName];
          }
        }
      }
    }

    private void ExtractIndexes()
    {
      string select = "SELECT TABLE_NAME, INDEX_NAME, COLUMN_NAME, PRIMARY_KEY, [UNIQUE], [CLUSTERED], ORDINAL_POSITION FROM INFORMATION_SCHEMA.INDEXES ORDER BY TABLE_NAME, INDEX_NAME, ORDINAL_POSITION";

      using (var cmd = Connection.CreateCommand(select))
      using (IDataReader reader = cmd.ExecuteReader()) {
        Index index = null;
        PrimaryKey primaryKey = null;

        while (reader.Read()) {

          var table = schema.Tables[(string) reader["TABLE_NAME"]];
          var column = table.TableColumns[(string)reader["COLUMN_NAME"]];
          int position = (int) reader["ORDINAL_POSITION"];
          var indexName = (string) reader["INDEX_NAME"];

          // New index
          if (position == 1) {
            index = null;
            primaryKey = null;

            if ((bool) reader["PRIMARY_KEY"])
              primaryKey = table.CreatePrimaryKey(indexName);
            else {
              index = table.CreateIndex(indexName);
              index.IsUnique = (bool) reader["UNIQUE"];
            }
          }

          if (primaryKey != null) 
            primaryKey.Columns.Add(column);
          if (index != null)
            index.CreateIndexColumn(column);
        }
      }
    }

    private void ExtractForeignKeys()
    {
      string select = "SELECT CU.TABLE_NAME, CU.CONSTRAINT_NAME, CU.COLUMN_NAME, CU.ORDINAL_POSITION, RC.UNIQUE_CONSTRAINT_TABLE_NAME, RC.UNIQUE_CONSTRAINT_NAME,  RC.UPDATE_RULE, RC.DELETE_RULE FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS CU INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C ON CU.TABLE_NAME = C.TABLE_NAME AND CU.CONSTRAINT_NAME = C.CONSTRAINT_NAME INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC ON CU.TABLE_NAME = RC.CONSTRAINT_TABLE_NAME AND CU.CONSTRAINT_NAME = RC.CONSTRAINT_NAME ORDER BY CU.TABLE_NAME, CU.CONSTRAINT_NAME, CU.ORDINAL_POSITION";

      using (var cmd = Connection.CreateCommand(select))
      using (IDataReader reader = cmd.ExecuteReader()) {

        ForeignKey foreignKey = null;
        while (reader.Read()) {
          // get table
          Table referencingTable = schema.Tables[(string) reader["TABLE_NAME"]];

          // create foreign key
          if ((int)reader["ORDINAL_POSITION"] == 1) {
            foreignKey = referencingTable.CreateForeignKey((string) reader["CONSTRAINT_NAME"]);
            foreignKey.OnDelete = GetReferentialAction((string) reader["DELETE_RULE"]);
            foreignKey.OnUpdate = GetReferentialAction((string) reader["UPDATE_RULE"]);

            // referenced table
            Table referencedTable = schema.Tables[(string) reader["UNIQUE_CONSTRAINT_TABLE_NAME"]];
            foreignKey.ReferencedTable = referencedTable;
            var referencedConstraint = (UniqueConstraint)referencedTable.TableConstraints[(string) reader["UNIQUE_CONSTRAINT_NAME"]];
            foreignKey.ReferencedColumns.AddRange(referencedConstraint.Columns);
          }

          // referencing column
          TableColumn referencingColumn = referencingTable.TableColumns[(string) reader["COLUMN_NAME"]];
          foreignKey.Columns.Add(referencingColumn);
        }
      }
    }

    private ReferentialAction GetReferentialAction(string actionName)
    {
      if (actionName.ToUpper() == "SET NULL")
        return ReferentialAction.SetNull;
      if (actionName.ToUpper() == "SET DEFAULT")
        return ReferentialAction.SetDefault;
      if (actionName.StartsWith("CASCADE"))
        return ReferentialAction.Cascade;
      return ReferentialAction.NoAction;
    }

    private SqlValueType ReadDataType(IDataRecord reader)
    {
      var typeName = (string) reader["DATA_TYPE"];
      var typeInfo = Connection.Driver.ServerInfo.DataTypes[typeName];
      SqlType type;
      if (typeInfo!=null) {
        type = typeInfo.Type;
        typeName = null;
      }
      else
        type = SqlType.Unknown;

      int? precision = ReadNullableInt(reader, "NUMERIC_PRECISION");
      int? scale = ReadNullableInt(reader, "NUMERIC_SCALE");
      
      if (typeInfo!=null && typeInfo.MaxPrecision==null) {
        // resetting precision & scale for types that do not require specifying them
        precision = null;
        scale = null;
      }

      int? size = ReadNullableInt(reader, "CHARACTER_MAXIMUM_LENGTH");
      if (size<=0) {
        size = null;
        switch (type) {
        case SqlType.VarChar:
          type = SqlType.VarCharMax;
          break;
        case SqlType.VarBinary:
          type = SqlType.VarBinaryMax;
          break;
        }
      }
      if (typeInfo!=null && typeInfo.MaxLength==null) {
        // resetting length for types that do not require specifying it
        size = null;
      }
      return new SqlValueType(type, typeName, size, precision, scale);
    }

    private static int? ReadNullableInt(IDataRecord reader, string column)
    {
      return Convert.IsDBNull(reader[column]) ? null : (int?) Convert.ToInt32(reader[column]);
    }

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}