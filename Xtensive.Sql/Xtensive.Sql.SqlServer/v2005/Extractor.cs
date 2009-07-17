using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Dml;
using Xtensive.Core.Threading;

namespace Xtensive.Sql.SqlServer.v2005
{
  internal class Extractor : Model.Extractor
  {
    private static ThreadSafeCached<Catalog> infoCatalogCached = ThreadSafeCached<Catalog>.Create(new object());

    private Catalog infoCatalog;
    private Catalog catalog;

    protected override void Initialize()
    {
      infoCatalog = infoCatalogCached.GetValue(BuildInfoCatalog, Connection.UnderlyingConnection, Transaction);
    }

    public override Catalog Extract()
    {
      catalog = new Catalog(Connection.Url.Resource);
      ExtractSchemas();
      foreach (Schema schema in catalog.Schemas) {
        ExtractDomains(schema);
        ExtractTables(schema);
        ExtractViews(schema);
        ExtractColumns(schema);
        foreach (var table in schema.Tables)
          ExtractDefaultConstraints(schema, table);
        ExtractUniqueConstraints(schema);
        ExtractIndexes(schema);
      }
      ExtractForeignKeys();
      return catalog;
    }

    private void ExtractSchemas()
    {
      View tSchema = infoCatalog.Schemas["INFORMATION_SCHEMA"].Views["SCHEMATA"];
      SqlTableRef tSchemaRef = SqlDml.TableRef(tSchema);
      SqlSelect select = SqlDml.Select(tSchemaRef);
      select.Columns.AddRange(tSchemaRef["CATALOG_NAME"], tSchemaRef["SCHEMA_NAME"], tSchemaRef["SCHEMA_OWNER"]);
      select.Where = tSchemaRef["CATALOG_NAME"] == catalog.Name;

      using (var cmd = CreateCommand(select))
      using (IDataReader reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          Schema schema = catalog.CreateSchema((string) reader["SCHEMA_NAME"]);
          schema.Owner = (string) reader["SCHEMA_OWNER"];
        }
      }
    }

    private void ExtractViews(Schema schema)
    {
      View tViews = infoCatalog.Schemas["INFORMATION_SCHEMA"].Views["VIEWS"];
      SqlTableRef tableRefViews = SqlDml.TableRef(tViews);
      SqlSelect select = SqlDml.Select(tableRefViews);
      select.Columns.Add(tableRefViews["TABLE_NAME"], "VIEW_NAME");
      select.Columns.Add(tableRefViews["VIEW_DEFINITION"]);
      select.Where = tableRefViews["TABLE_SCHEMA"]==schema.Name;

      using (var cmd = CreateCommand(select))
      using (IDataReader reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          schema.CreateView((string) reader["VIEW_NAME"], SqlDml.Native((string) reader["VIEW_DEFINITION"]));
        }
      }
    }

    private void ExtractTables(Schema schema)
    {
      View tTables = infoCatalog.Schemas["sys"].Views["tables"];
      View tSchemas = infoCatalog.Schemas["sys"].Views["schemas"];
      SqlTableRef tTablesRef = SqlDml.TableRef(tTables);
      SqlTableRef tSchemasRef = SqlDml.TableRef(tSchemas);
      SqlSelect select =
        SqlDml.Select(tTablesRef.InnerJoin(tSchemasRef, tSchemasRef["schema_id"]==tTablesRef["schema_id"]));
      select.Columns.Add(tTablesRef["name"], "TABLE_NAME");
      select.Columns.Add(tSchemasRef["name"], "SCHEMA_NAME");
      select.Where = tSchemasRef["name"]==schema.Name;

      using (var cmd = CreateCommand(select)) {
        using (IDataReader reader = cmd.ExecuteReader())
          while (reader.Read()) {
            schema.CreateTable((string) reader["TABLE_NAME"]);
          }
      }
    }

    private void ExtractColumns(Schema schema)
    {
      var identityColumns = new Dictionary<string, TableColumn>();
      View tColumns = infoCatalog.Schemas["INFORMATION_SCHEMA"].Views["COLUMNS"];
      View tTables = infoCatalog.Schemas["INFORMATION_SCHEMA"].Views["TABLES"];
      SqlTableRef tColumnsRef = SqlDml.TableRef(tColumns, "C");
      SqlTableRef tTablesRef = SqlDml.TableRef(tTables, "T");
      SqlSelect select =
        SqlDml.Select(
          tColumnsRef.InnerJoin(tTablesRef,
            tTablesRef["TABLE_NAME"] == tColumnsRef["TABLE_NAME"] &&
              SqlDml.In(tTablesRef["TABLE_TYPE"], SqlDml.Array("BASE TABLE", "VIEW"))));
      select.Columns.AddRange(tColumnsRef["COLUMN_NAME"], tColumnsRef["DATA_TYPE"], tColumnsRef["IS_NULLABLE"],
        tColumnsRef["CHARACTER_MAXIMUM_LENGTH"], tColumnsRef["NUMERIC_PRECISION"],
        tColumnsRef["NUMERIC_SCALE"], tColumnsRef["CHARACTER_SET_NAME"]);
      select.Columns.AddRange(tColumnsRef["COLLATION_NAME"], tColumnsRef["TABLE_NAME"], tTablesRef["TABLE_NAME"],
        tTablesRef["TABLE_TYPE"], tColumnsRef["COLUMN_DEFAULT"]);
      select.Columns.AddRange(tColumnsRef["DOMAIN_SCHEMA"], tColumnsRef["DOMAIN_NAME"]);
      select.Columns.Add(
        SqlDml.FunctionCall("columnproperty",
          SqlDml.FunctionCall("object_id", tColumnsRef["TABLE_SCHEMA"] + '.' + tColumnsRef["TABLE_NAME"]),
          tColumnsRef["COLUMN_NAME"], "IsIdentity"), "IS_IDENTITY");
      select.Where = tTablesRef["TABLE_SCHEMA"] == schema.Name;
      select.OrderBy.Add(tColumnsRef["ORDINAL_POSITION"]);

      using (var cmd = CreateCommand(select))
      using (IDataReader reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          string tableOrViewName = (string) reader["TABLE_NAME"];

          SqlValueType sqlDataType = ReadDataType(reader);

          // Create new column
          switch ((string) reader["TABLE_TYPE"]) {
          case "BASE TABLE":
            Table table = schema.Tables[tableOrViewName];
            TableColumn column = table.CreateColumn((string) reader["COLUMN_NAME"], sqlDataType);
            string isNullableText = reader["IS_NULLABLE"].ToString().ToLower();
            column.IsNullable = isNullableText=="yes" || isNullableText=="true" || isNullableText=="1";

            // Collation
            if (reader["COLLATION_NAME"]!=DBNull.Value) {
              string name = (string) reader["COLLATION_NAME"];
              column.Collation = schema.Collations[name] ?? schema.CreateCollation(name);
            }

            // Default value
            if (!Convert.IsDBNull(reader["COLUMN_DEFAULT"]))
              column.DefaultValue = SqlDml.Native(((string) reader["COLUMN_DEFAULT"]).Trim(')', '('));

            // Autoincrement
            if (reader["IS_IDENTITY"]!=DBNull.Value && Convert.ToBoolean(reader["IS_IDENTITY"]))
              identityColumns[tableOrViewName] = column;

            // Domain
            if (!Convert.IsDBNull(reader["DOMAIN_SCHEMA"]) && !Convert.IsDBNull(reader["DOMAIN_NAME"])) {
              string domainSchema = (string) reader["DOMAIN_SCHEMA"];
              string domainName = (string) reader["DOMAIN_NAME"];
              column.Domain = catalog.Schemas[domainSchema].Domains[domainName];
            }

            break;

          case "VIEW":
            View view = schema.Views[tableOrViewName];
            view.CreateColumn((string) reader["COLUMN_NAME"]);
            break;
          }
        }
      }

      SqlTableRef tTablesRefN = SqlDml.TableRef(tTables);
      SqlSelect subSelect = SqlDml.Select(tTablesRefN);
      subSelect.Columns.Add(SqlDml.FunctionCall("IDENT_SEED", tTablesRefN["TABLE_NAME"]), "IDENT_SEED");
      subSelect.Columns.Add(SqlDml.FunctionCall("IDENT_INCR", tTablesRefN["TABLE_NAME"]), "IDENT_INCR");
      subSelect.Columns.Add(tTablesRefN["TABLE_NAME"], "TABLE_NAME");
      subSelect.Where = tTablesRefN["TABLE_SCHEMA"] == schema.Name;
      SqlQueryRef tSelect = SqlDml.QueryRef(subSelect);
      SqlSelect selectN = SqlDml.Select(tSelect);
      selectN.Columns.Add(tSelect["IDENT_SEED"]);
      selectN.Columns.Add(tSelect["IDENT_INCR"]);
      selectN.Columns.Add(tSelect["TABLE_NAME"]);
      selectN.Where = SqlDml.IsNotNull(tSelect["IDENT_SEED"]);

      using (var cmd = CreateCommand(selectN))
      using (IDataReader reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          TableColumn identityColumn = identityColumns[(string) reader["TABLE_NAME"]];
          identityColumn.SequenceDescriptor =
            new SequenceDescriptor(identityColumn,
              Convert.ToInt32(reader["IDENT_SEED"]),
              Convert.ToInt32(reader["IDENT_INCR"]));
        }
      }
    }

    private void ExtractIndexes(Schema schema)
    {
      int columnsToLoad = 15;
      View tConstr = infoCatalog.Schemas["INFORMATION_SCHEMA"].Views["TABLE_CONSTRAINTS"];
      View tIndexes = infoCatalog.Schemas["sys"].Views["sysindexes"];
      View tSysobjects = infoCatalog.Schemas["sys"].Views["sysobjects"];
      View tSysusers = infoCatalog.Schemas["sys"].Views["sysusers"];
      SqlTableRef tConstrRef = SqlDml.TableRef(tConstr, "TABLE_CONSTRAINTS");
      SqlTableRef tIndexesRef = SqlDml.TableRef(tIndexes, "IDX");
      SqlTableRef tSysobjRef = SqlDml.TableRef(tSysobjects, "TBL");
      SqlTableRef tSysusersRef = SqlDml.TableRef(tSysusers, "USR");
      SqlSelect select = SqlDml.Select(tIndexesRef);
      select.From = select.From.InnerJoin(tSysobjRef, tIndexesRef["id"] == tSysobjRef["id"] && tSysobjRef["type"] == "U");
      select.From = select.From.InnerJoin(tSysusersRef, tSysobjRef["uid"] == tSysusersRef["uid"]);
      select.From = select.From.LeftOuterJoin(tConstrRef,
        tConstrRef["CONSTRAINT_NAME"] == tIndexesRef["name"] &&
          tConstrRef["TABLE_NAME"] == tSysobjRef["name"]);
      select.Where = tIndexesRef["impid"] >= 0;
      select.Where &= tIndexesRef["indid"] > 0;
      select.Where &= SqlDml.FunctionCall("indexproperty", tSysobjRef["id"], tIndexesRef["name"], "IsStatistics") == 0;
      select.Where &= tSysusersRef["name"] == schema.Name;
      select.Columns.Add(tSysobjRef["name"], "TABLE_NAME");
      select.Columns.Add(tIndexesRef["name"], "INDEX_NAME");
      
      for (int i = 1; i <= columnsToLoad; i++)
        select.Columns.Add(
          SqlDml.FunctionCall("index_col", "[" + tSysusersRef["name"] + "].[" + tSysobjRef["name"] + "]", tIndexesRef["indid"], i),
          "COL" + i);

      for (int i = 1; i <= columnsToLoad; i++)
        select.Columns.Add(
          SqlDml.FunctionCall("INDEXKEY_PROPERTY", tSysobjRef["id"], tIndexesRef["indid"], i, "IsDescending"),
          "COL_DESC" + i);
      
      select.Columns.Add(SqlDml.FunctionCall("indexproperty", tSysobjRef["id"], tIndexesRef["name"], "IndexFillFactor"),
        "INDEX_FILL_FACTOR");
      select.Columns.Add(SqlDml.FunctionCall("indexproperty", tSysobjRef["id"], tIndexesRef["name"], "IsUnique"),
        "IS_UNIQUE");
      select.Columns.Add(SqlDml.FunctionCall("indexproperty", tSysobjRef["id"], tIndexesRef["name"], "IsClustered"),
        "IS_CLUSTERED");
      SqlCase c = SqlDml.Case();
      c[tConstrRef["CONSTRAINT_TYPE"] == "PRIMARY KEY"] = 1;
      c.Else = 0;
      select.Columns.Add(c, "IS_PRIMARY_KEY");
      select.OrderBy.Add(tIndexesRef["name"]);

      using (var cmd = CreateCommand(select))
      using (IDataReader reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          Table table = schema.Tables[(string) reader["TABLE_NAME"]];
          if ((int) reader["IS_PRIMARY_KEY"]==1) {
            PrimaryKey pk = table.CreatePrimaryKey((string) reader["INDEX_NAME"]);
            for (int i = 1; i <= columnsToLoad; i++) {
              object column = reader["COL" + i];
              if (column==DBNull.Value)
                break;
              pk.Columns.Add(table.TableColumns[(string) column]);
            }
          }
          else {
            Index index = table.CreateIndex((string) reader["INDEX_NAME"]);
            ExtractSecondaryIndexColumns(schema, index);
            index.FillFactor = Convert.ToByte(reader["INDEX_FILL_FACTOR"]);
            index.IsUnique = (int) reader["IS_UNIQUE"]!=0;
            index.IsClustered = (int) reader["IS_CLUSTERED"]!=0;
          }
        }
      }
    }

    private void ExtractSecondaryIndexColumns(Schema schema, Index index)
    {
      var tSysobjects = infoCatalog.Schemas["sys"].Views["objects"];
      var tIndexes = infoCatalog.Schemas["sys"].Views["indexes"];
      var tIndexColumns = infoCatalog.Schemas["sys"].Views["index_columns"];
      var tColumns = infoCatalog.Schemas["sys"].Views["columns"];
      var tSysobjRef = SqlDml.TableRef(tSysobjects, "TBL");
      var tIndexesRef = SqlDml.TableRef(tIndexes, "IDX");
      var tIndexColumnsRef = SqlDml.TableRef(tIndexColumns, "IDX_COL");
      var tColumnsRef = SqlDml.TableRef(tColumns, "COL");

      var select = SqlDml.Select(tSysobjRef);
      select.From = select.From.InnerJoin(tIndexesRef,
        tIndexesRef["object_id"]==tSysobjRef["object_id"]);
      select.From = select.From.InnerJoin(tIndexColumnsRef,
        tIndexColumnsRef["object_id"]==tIndexesRef["object_id"]
          && tIndexColumnsRef["index_id"]==tIndexesRef["index_id"]);
      select.From = select.From.InnerJoin(tColumnsRef,
        tColumnsRef["object_id"]==tIndexColumnsRef["object_id"]
          && tColumnsRef["column_id"]==tIndexColumnsRef["column_id"]);
      select.Where = tSysobjRef["name"]==index.DataTable.DbName;
      select.Where &= tIndexesRef["name"]==index.DbName;

      select.Columns.Add(tColumnsRef["name"], "NAME");
      select.Columns.Add(tIndexColumnsRef["is_descending_key"], "IS_DESCENDING");
      select.Columns.Add(tIndexColumnsRef["is_included_column"], "IS_INCLUDED");

      select.OrderBy.Add(tIndexColumnsRef["is_included_column"]);
      select.OrderBy.Add(tIndexColumnsRef["key_ordinal"]);

      using (var cmd = CreateCommand(select)) {
        var table = schema.Tables[index.DataTable.Name];
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            if ((bool) reader["IS_INCLUDED"])
              index.NonkeyColumns.Add(table.TableColumns[(string) reader["NAME"]]);
            else
              index.CreateIndexColumn(table.TableColumns[(string) reader["NAME"]],
                !(bool) reader["IS_DESCENDING"]);
          }
        }
      }
    }

    private void ExtractDefaultConstraints(Schema schema, Table table)
    {
      var tDefaultConstraints = infoCatalog.Schemas["sys"].Views["default_constraints"];
      var tTables = infoCatalog.Schemas["sys"].Views["objects"];
      var tColumns = infoCatalog.Schemas["sys"].Views["columns"];
      var tSchemas = infoCatalog.Schemas["sys"].Views["schemas"];

      var tDefaultConstraintsRef = SqlDml.TableRef(tDefaultConstraints, "dc");
      var tTablesRef = SqlDml.TableRef(tTables, "t");
      var tColumnsRef = SqlDml.TableRef(tColumns, "c");
      var tSchemasRef = SqlDml.TableRef(tSchemas);

      var select = SqlDml.Select(tDefaultConstraintsRef
        .InnerJoin(tTablesRef, tDefaultConstraintsRef["parent_object_id"]==tTablesRef["object_id"])
        .InnerJoin(tColumnsRef, tTablesRef["object_id"]==tColumnsRef["object_id"]
          & tDefaultConstraintsRef["parent_column_id"]==tColumnsRef["column_id"])
        .InnerJoin(tSchemasRef, tDefaultConstraintsRef["schema_id"]==tSchemasRef["schema_id"]));
      
      select.Columns.Add(tDefaultConstraintsRef["name"], "CONSTRAINT_NAME");
      select.Columns.Add(tSchemasRef["name"], "SCHEMA_NAME");
      select.Columns.Add(tTablesRef["name"], "TABLE_NAME");
      select.Columns.Add(tColumnsRef["name"], "COLUMN_NAME");
      select.Where &= tDefaultConstraintsRef["type"]=="D";
      select.Where &= tTablesRef["name"]==table.Name;
      select.Where &= tSchemasRef["name"]==schema.Name;

      using (var cmd = CreateCommand(select))
      using (IDataReader reader = cmd.ExecuteReader())
        while (reader.Read()) {
          var columnName = reader["COLUMN_NAME"].ToString();
          var column = table.TableColumns[columnName];
          table.CreateDefaultConstraint(reader["CONSTRAINT_NAME"].ToString(), column);
        }
    }

    private SqlSelect GenerateExtractForeignKeysSQL()
    {
      View tKeyCol = infoCatalog.Schemas["INFORMATION_SCHEMA"].Views["KEY_COLUMN_USAGE"];
      View tConstr = infoCatalog.Schemas["INFORMATION_SCHEMA"].Views["TABLE_CONSTRAINTS"];
      View tRefConstr = infoCatalog.Schemas["INFORMATION_SCHEMA"].Views["REFERENTIAL_CONSTRAINTS"];
      SqlTableRef tKeyColRef = SqlDml.TableRef(tKeyCol);
      SqlTableRef tRefConstrRef = SqlDml.TableRef(tRefConstr);
      SqlTableRef tConstrRef = SqlDml.TableRef(tConstr);
      SqlTableRef tUniqueRef = SqlDml.TableRef(tKeyCol);
      SqlSelect select = SqlDml.Select(tKeyColRef);
      select.From =
        select.From.InnerJoin(tRefConstrRef, tRefConstrRef["CONSTRAINT_NAME"]==tKeyColRef["CONSTRAINT_NAME"]);
      select.From = select.From.InnerJoin(tConstrRef, tConstrRef["CONSTRAINT_NAME"]==tRefConstrRef["UNIQUE_CONSTRAINT_NAME"]);
      select.From =
        select.From.InnerJoin(tUniqueRef,
          tUniqueRef["CONSTRAINT_NAME"] == tRefConstrRef["UNIQUE_CONSTRAINT_NAME"] &&
            tUniqueRef["ORDINAL_POSITION"] == tKeyColRef["ORDINAL_POSITION"]);
      select.Columns.Add(tKeyColRef["TABLE_NAME"], "CONSTRAINT_TABLE");
      select.Columns.Add(tKeyColRef["TABLE_SCHEMA"], "CONSTRAINT_TABLE_SCHEMA");
      select.Columns.Add(tKeyColRef["COLUMN_NAME"], "CONSTRAINT_COLUMN");
      select.Columns.Add(tConstrRef["TABLE_NAME"], "REFERENCE_TABLE");
      select.Columns.Add(tConstrRef["TABLE_SCHEMA"], "REFERENCE_TABLE_SCHEMA");
      select.Columns.Add(tUniqueRef["COLUMN_NAME"], "REFERENCE_COLUMN");
      select.Columns.Add(tUniqueRef["ORDINAL_POSITION"]);
      select.Columns.Add(tRefConstrRef["DELETE_RULE"], "DELETE_RULE");
      select.Columns.Add(tRefConstrRef["UPDATE_RULE"], "UPDATE_RULE");
      select.Columns.Add(tRefConstrRef["CONSTRAINT_NAME"], "CONSTRAINT_NAME");

      select.Where = tKeyColRef["CONSTRAINT_CATALOG"]==catalog.Name;
      select.Where &= tRefConstrRef["CONSTRAINT_CATALOG"]==catalog.Name;
      select.Where &= tConstrRef["CONSTRAINT_CATALOG"]==catalog.Name;
      select.Where &= tUniqueRef["CONSTRAINT_CATALOG"]==catalog.Name;

      select.OrderBy.Add(tKeyColRef["TABLE_NAME"]);
      select.OrderBy.Add(tKeyColRef["CONSTRAINT_NAME"]);
      select.OrderBy.Add(tKeyColRef["ORDINAL_POSITION"]);

      return select;
    }

    private void ExtractForeignKeys()
    {
      using (var cmd = CreateCommand(GenerateExtractForeignKeysSQL()))
      using (IDataReader reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          // get table
          string tableName = (string) reader["CONSTRAINT_TABLE"];
          Schema referencingSchema = catalog.Schemas[(string) reader["CONSTRAINT_TABLE_SCHEMA"]];
          Table referencingTable = referencingSchema.Tables[tableName];
          string constraintName = (string) reader["CONSTRAINT_NAME"];

          // referenced table
          Schema referencedSchema = catalog.Schemas[(string) reader["REFERENCE_TABLE_SCHEMA"]];
          Table referencedTable = referencedSchema.Tables[(string) reader["REFERENCE_TABLE"]];

          // get foreign key
          var foreignKey = (ForeignKey) referencingTable.TableConstraints[constraintName];
          if (foreignKey==null) {
            foreignKey = referencingTable.CreateForeignKey(constraintName);
            foreignKey.ReferencedTable = referencedTable;
          }

          foreignKey.OnDelete = GetReferentialAction((string) reader["DELETE_RULE"]);
          foreignKey.OnUpdate = GetReferentialAction((string) reader["UPDATE_RULE"]);

          // referencing columns
          TableColumn referencingColumn = referencingTable.TableColumns[(string) reader["CONSTRAINT_COLUMN"]];
          TableColumn referencedColumn = referencedTable.TableColumns[(string) reader["REFERENCE_COLUMN"]];

          foreignKey.Columns.Add(referencingColumn);
          foreignKey.ReferencedColumns.Add(referencedColumn);
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

    public void ExtractUniqueConstraints(Schema schema)
    {
      View tColumnUsage =
        infoCatalog.Schemas["INFORMATION_SCHEMA"].Views["CONSTRAINT_COLUMN_USAGE"];
      View tConstraints =
        infoCatalog.Schemas["INFORMATION_SCHEMA"].Views["TABLE_CONSTRAINTS"];
      SqlTableRef tColUseRef = SqlDml.TableRef(tColumnUsage, "constraintColumns");
      SqlTableRef tConstrRef = SqlDml.TableRef(tConstraints, "constraints");
      SqlSelect select =
        SqlDml.Select(
          tColUseRef.FullOuterJoin(tConstrRef, tConstrRef["CONSTRAINT_CATALOG"] == tColUseRef["CONSTRAINT_CATALOG"]));
      select.Columns.AddRange(tColUseRef["TABLE_NAME"], tColUseRef["COLUMN_NAME"], tConstrRef["CONSTRAINT_NAME"]);
      select.Where = tConstrRef["CONSTRAINT_NAME"] == tColUseRef["CONSTRAINT_NAME"] &&
        tConstrRef["CONSTRAINT_TYPE"] == "UNIQUE" && tConstrRef["TABLE_SCHEMA"] == schema.Name;

      using (var cmd = CreateCommand(select))
      using (IDataReader reader = cmd.ExecuteReader()) {
        while (reader.Read()) {
          Table table = schema.Tables[(string) reader["TABLE_NAME"]];
          string constraintName = (string) reader["CONSTRAINT_NAME"];

          var uniqueConstraint = (Model.UniqueConstraint) table.TableConstraints[constraintName];
          if (uniqueConstraint==null)
            uniqueConstraint = table.CreateUniqueConstraint(constraintName);

          uniqueConstraint.Columns.Add(table.TableColumns[(string) reader["COLUMN_NAME"]]);
        }
      }
    }

    private void ExtractDomains(Schema schema)
    {
      var domains = SqlDml.TableRef(infoCatalog.Schemas["INFORMATION_SCHEMA"].Views["DOMAINS"]);
      var select = SqlDml.Select(domains);
      select.Columns.AddRange(
        domains["DOMAIN_NAME"],
        domains["DOMAIN_DEFAULT"],
        domains["DATA_TYPE"],
        domains["CHARACTER_MAXIMUM_LENGTH"],
        domains["NUMERIC_PRECISION"],
        domains["NUMERIC_SCALE"]
        );
      select.Where = domains["DOMAIN_CATALOG"]==schema.Catalog.Name && domains["DOMAIN_SCHEMA"]==schema.Name;
      using (var command = CreateCommand(select)) {
        using (var reader = command.ExecuteReader()) {
          while (reader.Read()) {
            string domainName = (string)reader[0];
            object domainDefault = reader[1];
            var dataType = ReadDataType(reader);
            schema.CreateDomain(domainName, dataType,
              Convert.IsDBNull(domainDefault) ? null : SqlDml.Native((string)domainDefault));
          }
        }
      }
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

    private static Catalog BuildInfoCatalog(DbConnection connection, DbTransaction transaction)
    {
      var catalog = new Catalog("info_catalog");

      // select schema
      using (var cmd = connection.CreateCommand()) {
        cmd.Transaction = transaction;
        cmd.CommandText = @" Select Distinct " + "\n [CATALOG_NAME], " + "\n [SCHEMA_NAME], " + "\n [SCHEMA_OWNER] " +
          "\n From [master].[INFORMATION_SCHEMA].[SCHEMATA]";

        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read())
            catalog.CreateSchema((string) reader["SCHEMA_NAME"]);
        }
      }

      // select views
      foreach (Schema cSchema in catalog.Schemas) {
        string selectViewsSQL = @" Select [all_views].[name] as [VIEW_NAME], [schemas].[name] as [SCHEMA_NAME] " +
          "\n From [sys].[all_views] " +
            "\n Left Join [sys].[schemas] on [schemas].[schema_id]=[all_views].[schema_id] " +
              "\n Where [schemas].[name]='" + cSchema.Name + "'";

        using (IDbCommand cmd = connection.CreateCommand()) {
          cmd.Transaction = transaction;
          cmd.CommandText = selectViewsSQL;
          using (IDataReader reader = cmd.ExecuteReader()) {
            while (reader.Read())
              cSchema.CreateView((string)reader["VIEW_NAME"], SqlDml.Native("view def"));
          }
        }

        // select columns
        string selectColumnsSql =
          @" Select [all_views].[name] as [VIEW_NAME], [all_columns].[name] as [COLUMN_NAME], " +
            "\n [schemas].[name] as [SCHEMA_NAME] " + "\n From [sys].[all_views] " +
              "\n Left Join [sys].[all_columns] on [all_columns].[object_id]=[all_views].[object_id] " +
                "\n Left Join [sys].[schemas] on [schemas].[schema_id]=[all_views].[schema_id] " +
                  "\n Where [schemas].[name] = '" + cSchema.Name + "'";

        using (IDbCommand cmd = connection.CreateCommand()) {
          cmd.Transaction = transaction;
          cmd.CommandText = selectColumnsSql;
          using (IDataReader reader = cmd.ExecuteReader())
            while (reader.Read()) {
              string tableOrViewName = (string)reader["VIEW_NAME"];
              // Create new column

              View view = cSchema.Views[tableOrViewName];
              view.CreateColumn((string)reader["COLUMN_NAME"]);
            }
        }
      }
      return catalog;
    }

    private DbCommand CreateCommand(ISqlCompileUnit statement)
    {
      var command = Connection.CreateCommand(statement);
      command.Transaction = Transaction;
      return command;
    }

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}