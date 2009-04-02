using System;
using System.Collections.Generic;
using System.Data;
using Xtensive.Sql.Common;
using Xtensive.Sql.Common.Mssql;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Extractor;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Mssql.v2005
{
  public class MssqlExtractor: SqlExtractor
  {
    private static bool initialized = false;
    private static Model model;

    /// <summary>
    /// Extracts the users.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="server">The server.</param>
    public override void ExtractUsers(SqlExtractorContext context, Server server)
    {
      View tUsers = model.DefaultServer.Catalogs["master"].Schemas["sys"].Views["sysusers"];
      SqlTableRef tUsersRef = Sql.TableRef(tUsers);
      SqlSelect select = Sql.Select(tUsersRef);
      select.Columns.Add(tUsersRef["name"]);
      select.Where = tUsersRef["issqlrole"] == 0 && tUsersRef["hasdbaccess"] == 1;

      using (SqlCommand cmd = new SqlCommand(context.Connection)) {
        cmd.Transaction = context.Transaction;
        cmd.Statement = select;
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            server.CreateUser(reader.GetString(0));
          }
        }
      }
    }

    /// <summary>
    /// Extracts the schemes.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="catalog">The catalog.</param>
    public override void ExtractSchemas(SqlExtractorContext context, Catalog catalog)
    {
      View tSchema = model.DefaultServer.Catalogs["master"].Schemas["INFORMATION_SCHEMA"].Views["SCHEMATA"];
      SqlTableRef tSchemaRef = Sql.TableRef(tSchema);
      SqlSelect select = Sql.Select(tSchemaRef);
      select.Columns.AddRange(tSchemaRef["CATALOG_NAME"], tSchemaRef["SCHEMA_NAME"], tSchemaRef["SCHEMA_OWNER"]);
      if (catalog != null)
        select.Where = tSchemaRef["CATALOG_NAME"] == catalog.Name;

      using (SqlCommand cmd = new SqlCommand(context.Connection)) {
        cmd.Transaction = context.Transaction;
        cmd.Statement = select;
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            Catalog schemaCatalog = catalog ?? context.Model.DefaultServer.Catalogs[(string)reader["CATALOG_NAME"]];

            Schema schema = schemaCatalog.CreateSchema((string)reader["SCHEMA_NAME"]);
            schema.Owner = schemaCatalog.Server.Users[(string)reader["SCHEMA_OWNER"]];
          }
        }
      }
    }

    /// <summary>
    /// Extracts the views.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public override void ExtractViews(SqlExtractorContext context, Schema schema)
    {
      View tViews = model.DefaultServer.Catalogs["master"].Schemas["INFORMATION_SCHEMA"].Views["VIEWS"];
      SqlTableRef tableRefViews = Sql.TableRef(tViews);
      SqlSelect select = Sql.Select(tableRefViews);
      select.Columns.Add(tableRefViews["TABLE_NAME"], "VIEW_NAME");
      select.Columns.Add(tableRefViews["VIEW_DEFINITION"]);
      if (schema != null)
        select.Where = tableRefViews["TABLE_SCHEMA"] == schema.Name;

      using (SqlCommand cmd = new SqlCommand(context.Connection)) {
        cmd.Transaction = context.Transaction;
        cmd.Statement = select;
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            schema.CreateView((string)reader["VIEW_NAME"], Sql.Native((string)reader["VIEW_DEFINITION"]));
          }
        }
      }
    }

    /// <summary>
    /// Extracts the tables.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public override void ExtractTables(SqlExtractorContext context, Schema schema)
    {
      View tTables = model.DefaultServer.Catalogs["master"].Schemas["sys"].Views["tables"];
      View tSchemas = model.DefaultServer.Catalogs["master"].Schemas["sys"].Views["schemas"];
      SqlTableRef tTablesRef = Sql.TableRef(tTables);
      SqlTableRef tSchemasRef = Sql.TableRef(tSchemas);
      SqlSelect select =
        Sql.Select(tTablesRef.InnerJoin(tSchemasRef, tSchemasRef["schema_id"] == tTablesRef["schema_id"]));
      select.Columns.Add(tTablesRef["name"], "TABLE_NAME");
      select.Columns.Add(tSchemasRef["name"], "SCHEMA_NAME");
      if (schema != null)
        select.Where = tSchemasRef["name"] == schema.Name;

      using (SqlCommand cmd = new SqlCommand(context.Connection)) {
        cmd.Transaction = context.Transaction;
        cmd.Statement = select;
        using (IDataReader reader = cmd.ExecuteReader())
          while (reader.Read()) {
            Schema tableSchema = schema ?? context.Model.DefaultServer.DefaultCatalog.Schemas[(string)reader["SCHEMA_NAME"]];
            tableSchema.CreateTable((string)reader["TABLE_NAME"]);
          }
      }
    }

    /// <summary>
    /// Extracts the columns.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public override void ExtractColumns(SqlExtractorContext context, Schema schema)
    {
      Dictionary<string, TableColumn> identityColumns = new Dictionary<string, TableColumn>();
      View tColumns = model.DefaultServer.Catalogs["master"].Schemas["INFORMATION_SCHEMA"].Views["COLUMNS"];
      View tTables = model.DefaultServer.Catalogs["master"].Schemas["INFORMATION_SCHEMA"].Views["TABLES"];
      SqlTableRef tColumnsRef = Sql.TableRef(tColumns, "C");
      SqlTableRef tTablesRef = Sql.TableRef(tTables, "T");
      SqlSelect select =
        Sql.Select(
          tColumnsRef.InnerJoin(tTablesRef,
                                tTablesRef["TABLE_NAME"] == tColumnsRef["TABLE_NAME"] &&
                                Sql.In(tTablesRef["TABLE_TYPE"], Sql.Array("BASE TABLE", "VIEW"))));
      select.Columns.AddRange(tColumnsRef["COLUMN_NAME"], tColumnsRef["DATA_TYPE"], tColumnsRef["IS_NULLABLE"],
                              tColumnsRef["CHARACTER_MAXIMUM_LENGTH"], tColumnsRef["NUMERIC_PRECISION"],
                              tColumnsRef["NUMERIC_SCALE"], tColumnsRef["CHARACTER_SET_NAME"]);
      select.Columns.AddRange(tColumnsRef["COLLATION_NAME"], tColumnsRef["TABLE_NAME"], tTablesRef["TABLE_NAME"],
                              tTablesRef["TABLE_TYPE"], tColumnsRef["COLUMN_DEFAULT"]);
      select.Columns.AddRange(tColumnsRef["DOMAIN_SCHEMA"], tColumnsRef["DOMAIN_NAME"]);
      select.Columns.Add(
        Sql.FunctionCall("columnproperty",
                         Sql.FunctionCall("object_id", tColumnsRef["TABLE_SCHEMA"] + '.' + tColumnsRef["TABLE_NAME"]),
                         tColumnsRef["COLUMN_NAME"], "IsIdentity"), "IS_IDENTITY");
      select.Where = tTablesRef["TABLE_SCHEMA"] == schema.Name;
      select.OrderBy.Add(tColumnsRef["ORDINAL_POSITION"]);

      using (SqlCommand cmd = new SqlCommand(context.Connection)) {
        cmd.Transaction = context.Transaction;
        cmd.Statement = select;
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            string tableOrViewName = (string)reader["TABLE_NAME"];

            SqlValueType sqlDataType = ReadDataType(context, reader);

            // Create new column
            switch ((string)reader["TABLE_TYPE"]) {
              case "BASE TABLE":
                Table table = schema.Tables[tableOrViewName];
                TableColumn column = table.CreateColumn((string)reader["COLUMN_NAME"], sqlDataType);
                string isNullableText = reader["IS_NULLABLE"].ToString().ToLower();
                column.IsNullable = isNullableText == "yes" || isNullableText == "true" || isNullableText == "1";

                // Collation
                if (reader["COLLATION_NAME"] != DBNull.Value) {
                  string name = (string)reader["COLLATION_NAME"];
                  column.Collation = schema.Collations[name] ?? schema.CreateCollation(name);
                }

                // Default value
                if (!Convert.IsDBNull(reader["COLUMN_DEFAULT"]))
                  column.DefaultValue = Sql.Native(((string)reader["COLUMN_DEFAULT"]).Trim(')', '('));

                // Autoincrement
                if (reader["IS_IDENTITY"] != DBNull.Value && Convert.ToBoolean(reader["IS_IDENTITY"]))
                  identityColumns[tableOrViewName] = column;

                // Domain
                if (!Convert.IsDBNull(reader["DOMAIN_SCHEMA"]) && !Convert.IsDBNull(reader["DOMAIN_NAME"])) {
                  string domainSchema = (string) reader["DOMAIN_SCHEMA"];
                  string domainName = (string) reader["DOMAIN_NAME"];
                  column.Domain = context.Model.DefaultServer.DefaultCatalog.Schemas[domainSchema].Domains[domainName];
                }

                break;

              case "VIEW":
                View view = schema.Views[tableOrViewName];
                view.CreateColumn((string)reader["COLUMN_NAME"]);
                break;
            }
          }
        }
      }

      SqlTableRef tTablesRefN = Sql.TableRef(tTables);
      SqlSelect subSelect = Sql.Select(tTablesRefN);
      subSelect.Columns.Add(Sql.FunctionCall("IDENT_SEED", tTablesRefN["TABLE_NAME"]), "IDENT_SEED");
      subSelect.Columns.Add(Sql.FunctionCall("IDENT_INCR", tTablesRefN["TABLE_NAME"]), "IDENT_INCR");
      subSelect.Columns.Add(tTablesRefN["TABLE_NAME"], "TABLE_NAME");
      subSelect.Where = tTablesRefN["TABLE_SCHEMA"] == schema.Name;
      SqlQueryRef tSelect = Sql.QueryRef(subSelect);
      SqlSelect selectN = Sql.Select(tSelect);
      selectN.Columns.Add(tSelect["IDENT_SEED"]);
      selectN.Columns.Add(tSelect["IDENT_INCR"]);
      selectN.Columns.Add(tSelect["TABLE_NAME"]);
      selectN.Where = Sql.IsNotNull(tSelect["IDENT_SEED"]);

      using (SqlCommand cmd = new SqlCommand(context.Connection)) {
        cmd.Transaction = context.Transaction;
        cmd.Statement = selectN;
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            TableColumn identityColumn = identityColumns[(string)reader["TABLE_NAME"]];
            identityColumn.SequenceDescriptor =
              new SequenceDescriptor(identityColumn, Convert.ToInt32(reader["IDENT_SEED"]),
                                     Convert.ToInt32(reader["IDENT_INCR"]));
          }
        }
      }
    }

    /// <summary>
    /// Extracts the indexes.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public override void ExtractIndexes(SqlExtractorContext context, Schema schema)
    {
      int columnsToLoad = 15;
      View tConstr = model.DefaultServer.Catalogs["master"].Schemas["INFORMATION_SCHEMA"].Views["TABLE_CONSTRAINTS"];
      View tIndexes = model.DefaultServer.Catalogs["master"].Schemas["sys"].Views["sysindexes"];
      View tSysobjects = model.DefaultServer.Catalogs["master"].Schemas["sys"].Views["sysobjects"];
      View tSysusers = model.DefaultServer.Catalogs["master"].Schemas["sys"].Views["sysusers"];
      SqlTableRef tConstrRef = Sql.TableRef(tConstr, "TABLE_CONSTRAINTS");
      SqlTableRef tIndexesRef = Sql.TableRef(tIndexes, "IDX");
      SqlTableRef tSysobjRef = Sql.TableRef(tSysobjects, "TBL");
      SqlTableRef tSysusersRef = Sql.TableRef(tSysusers, "USR");
      SqlSelect select = Sql.Select(tIndexesRef);
      select.From = select.From.InnerJoin(tSysobjRef, tIndexesRef["id"] == tSysobjRef["id"] && tSysobjRef["type"] == "U");
      select.From = select.From.InnerJoin(tSysusersRef, tSysobjRef["uid"] == tSysusersRef["uid"]);
      select.From = select.From.LeftOuterJoin(tConstrRef,
                                tConstrRef["CONSTRAINT_NAME"] == tIndexesRef["name"] &&
                                tConstrRef["TABLE_NAME"] == tSysobjRef["name"]);
      select.Where = tIndexesRef["impid"] >= 0;
      select.Where &= tIndexesRef["indid"] > 0;
      select.Where &= Sql.FunctionCall("indexproperty", tSysobjRef["id"], tIndexesRef["name"], "IsStatistics") == 0;
      if (schema != null)
        select.Where &= tSysusersRef["name"] == schema.Name;
      select.Columns.Add(tSysobjRef["name"], "TABLE_NAME");
      select.Columns.Add(tIndexesRef["name"], "INDEX_NAME");

      for (int i = 1; i <= columnsToLoad; i++)
        select.Columns.Add(
          Sql.FunctionCall("index_col", tSysusersRef["name"] + "." + tSysobjRef["name"], tIndexesRef["indid"], i),
          "COL" + i);

      for (int i = 1; i <= columnsToLoad; i++)
        select.Columns.Add(
          Sql.FunctionCall("INDEXKEY_PROPERTY", tSysobjRef["id"], tIndexesRef["indid"], i, "IsDescending"),
          "COL_DESC" + i);

      select.Columns.Add(Sql.FunctionCall("indexproperty", tSysobjRef["id"], tIndexesRef["name"], "IndexFillFactor"),
                         "INDEX_FILL_FACTOR");
      select.Columns.Add(Sql.FunctionCall("indexproperty", tSysobjRef["id"], tIndexesRef["name"], "IsUnique"),
                         "IS_UNIQUE");
      select.Columns.Add(Sql.FunctionCall("indexproperty", tSysobjRef["id"], tIndexesRef["name"], "IsClustered"),
                         "IS_CLUSTERED");
      SqlCase c = Sql.Case();
      c[tConstrRef["CONSTRAINT_TYPE"] == "PRIMARY KEY"] = 1;
      c.Else = 0;
      select.Columns.Add(c, "IS_PRIMARY_KEY");
      select.OrderBy.Add(tIndexesRef["name"]);

      using (SqlCommand cmd = new SqlCommand(context.Connection)) {
        cmd.Transaction = context.Transaction;
        cmd.Statement = select;
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            Table table = schema.Tables[(string)reader["TABLE_NAME"]];
            if ((int)reader["IS_PRIMARY_KEY"] == 1) {
              PrimaryKey pk = table.CreatePrimaryKey((string)reader["INDEX_NAME"]);
              for (int i = 1; i <= columnsToLoad; i++) {
                object column = reader["COL" + i];
                if (column == DBNull.Value)
                  break;
                pk.Columns.Add(table.TableColumns[(string)column]);
              }
            } 
            else {
              Index index = table.CreateIndex((string)reader["INDEX_NAME"]);
              for (int i = 1; i <= columnsToLoad; i++) {
                object column = reader["COL" + i];
                if (column == DBNull.Value)
                  break;
                index.CreateIndexColumn(table.TableColumns[(string)column], (int)reader["COL_DESC" + i] == 0);
              }
              index.FillFactor = Convert.ToByte(reader["INDEX_FILL_FACTOR"]);
              index.IsUnique = (int)reader["IS_UNIQUE"] != 0;
              index.IsClustered = (int)reader["IS_CLUSTERED"] != 0;
            }
          }
        }
      }
    }

    /// <summary>
    /// Extracts the foreign keys.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="catalog">The catalog.</param>
    public string GenerateExtractForeignKeysSQL(SqlExtractorContext context, Catalog catalog, Schema schema)
    {
      View tKeyCol = model.DefaultServer.Catalogs["master"].Schemas["INFORMATION_SCHEMA"].Views["KEY_COLUMN_USAGE"];
      View tConstr = model.DefaultServer.Catalogs["master"].Schemas["INFORMATION_SCHEMA"].Views["TABLE_CONSTRAINTS"];
      View tRefConstr =
        model.DefaultServer.Catalogs["master"].Schemas["INFORMATION_SCHEMA"].Views["REFERENTIAL_CONSTRAINTS"];
      SqlTableRef tKeyColRef = Sql.TableRef(tKeyCol);
      SqlTableRef tRefConstrRef = Sql.TableRef(tRefConstr);
      SqlTableRef tConstrRef = Sql.TableRef(tConstr);
      SqlTableRef tUniqueRef = Sql.TableRef(tKeyCol);
      SqlSelect select = Sql.Select(tKeyColRef);
      select.From =
        select.From.InnerJoin(tRefConstrRef, tRefConstrRef["CONSTRAINT_NAME"] == tKeyColRef["CONSTRAINT_NAME"]);
      select.From = select.From.InnerJoin(tConstrRef, tConstrRef["CONSTRAINT_NAME"] == tRefConstrRef["UNIQUE_CONSTRAINT_NAME"]);
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

      if (schema != null) {
        select.Where = tKeyColRef["CONSTRAINT_SCHEMA"] == schema.Name;
        select.Where &= tRefConstrRef["CONSTRAINT_SCHEMA"] == schema.Name;
        select.Where &= tConstrRef["CONSTRAINT_SCHEMA"] == schema.Name;
        select.Where &= tUniqueRef["CONSTRAINT_SCHEMA"] == schema.Name;
      }

      if (catalog != null) {
        select.Where = tKeyColRef["CONSTRAINT_CATALOG"] == catalog.Name;
        select.Where &= tRefConstrRef["CONSTRAINT_CATALOG"] == catalog.Name;
        select.Where &= tConstrRef["CONSTRAINT_CATALOG"] == catalog.Name;
        select.Where &= tUniqueRef["CONSTRAINT_CATALOG"] == catalog.Name;
      }

      select.OrderBy.Add(tKeyColRef["TABLE_NAME"]);
      select.OrderBy.Add(tKeyColRef["CONSTRAINT_NAME"]);
      select.OrderBy.Add(tKeyColRef["ORDINAL_POSITION"]);

      // Render.
      MssqlDriver mssqlDriver = new MssqlDriver(new MssqlVersionInfo(new Version()));
      return mssqlDriver.Compile(select).CommandText;
    }

    public override void ExtractForeignKeys(SqlExtractorContext context, Schema schema)
    {
      GenerateExtractForeignKeysSQL(context, null, schema);
      base.ExtractForeignKeys(context, schema);
    }

    public override void ExtractForeignKeys(SqlExtractorContext context, Catalog catalog)
    {
      GenerateExtractForeignKeysSQL(context, catalog, null);
      ExtractForeignKeys(context, catalog, null);
    }

    private void ExtractForeignKeys(SqlExtractorContext context, Catalog catalog, Schema schema)
    {
      string sqlQuery = GenerateExtractForeignKeysSQL(context, catalog, schema);
      using (IDbCommand cmd = context.Connection.RealConnection.CreateCommand()) {
        cmd.Transaction = context.Transaction;
        cmd.CommandText = sqlQuery;
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            // get table
            string tableName = (string)reader["CONSTRAINT_TABLE"];
            Schema referencingSchema = schema ?? catalog.Schemas[(string)reader["CONSTRAINT_TABLE_SCHEMA"]];
            Table referencingTable = referencingSchema.Tables[tableName];
            string constraintName = (string)reader["CONSTRAINT_NAME"];

            // referenced table
            Schema referencedSchema = schema ?? catalog.Schemas[(string)reader["REFERENCE_TABLE_SCHEMA"]];
            Table referencedTable = referencedSchema.Tables[(string)reader["REFERENCE_TABLE"]];

            // get foreign key
            ForeignKey foreignKey = (ForeignKey)referencingTable.TableConstraints[constraintName];
            if (foreignKey == null) {
              foreignKey = referencingTable.CreateForeignKey(constraintName);
              foreignKey.ReferencedTable = referencedTable;
            }

            foreignKey.OnDelete = GetReferentialAction((string)reader["DELETE_RULE"]);
            foreignKey.OnUpdate = GetReferentialAction((string)reader["UPDATE_RULE"]);

            // referencing columns
            TableColumn referencingColumn = referencingTable.TableColumns[(string)reader["CONSTRAINT_COLUMN"]];
            TableColumn referencedColumn = referencedTable.TableColumns[(string)reader["REFERENCE_COLUMN"]];

            foreignKey.Columns.Add(referencingColumn);
            foreignKey.ReferencedColumns.Add(referencedColumn);
          }
        }
      }
    }

    protected virtual ReferentialAction GetReferentialAction(string actionName)
    {
      if (actionName.ToUpper() == "SET NULL")
        return ReferentialAction.SetNull;
      if (actionName.ToUpper() == "SET DEFAULT")
        return ReferentialAction.SetDefault;
      if (actionName.StartsWith("CASCADE"))
        return ReferentialAction.Cascade;
      return ReferentialAction.NoAction;
    }

    /// <summary>
    /// Extracts the unique constraints.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="schema">The schema.</param>
    public override void ExtractUniqueConstraints(SqlExtractorContext context, Schema schema)
    {
      View tColumnUsage =
        model.DefaultServer.Catalogs["master"].Schemas["INFORMATION_SCHEMA"].Views["CONSTRAINT_COLUMN_USAGE"];
      View tConstraints =
        model.DefaultServer.Catalogs["master"].Schemas["INFORMATION_SCHEMA"].Views["TABLE_CONSTRAINTS"];
      SqlTableRef tColUseRef = Sql.TableRef(tColumnUsage, "constraintColumns");
      SqlTableRef tConstrRef = Sql.TableRef(tConstraints, "constraints");
      SqlSelect select =
        Sql.Select(
          tColUseRef.FullOuterJoin(tConstrRef, tConstrRef["CONSTRAINT_CATALOG"] == tColUseRef["CONSTRAINT_CATALOG"]));
      select.Columns.AddRange(tColUseRef["TABLE_NAME"], tColUseRef["COLUMN_NAME"], tConstrRef["CONSTRAINT_NAME"]);
      select.Where = tConstrRef["CONSTRAINT_NAME"] == tColUseRef["CONSTRAINT_NAME"] &&
                     tConstrRef["CONSTRAINT_TYPE"] == "UNIQUE" && tConstrRef["TABLE_SCHEMA"] == schema.Name;

      using (SqlCommand cmd = new SqlCommand(context.Connection)) {
        cmd.Transaction = context.Transaction;
        cmd.Statement = select;
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            Table table = schema.Tables[(string)reader["TABLE_NAME"]];
            string constraintName = (string)reader["CONSTRAINT_NAME"];

            Database.UniqueConstraint uniqueConstraint =
              (Database.UniqueConstraint)table.TableConstraints[constraintName];
            if (uniqueConstraint == null)
              uniqueConstraint = table.CreateUniqueConstraint(constraintName);

            uniqueConstraint.Columns.Add(table.TableColumns[(string)reader["COLUMN_NAME"]]);
          }
        }
      }
    }

    /// <inheritdoc/>
    public override void ExtractDomains(SqlExtractorContext context, Schema schema)
    {
      var domains = Sql.TableRef(model.DefaultServer.Catalogs["master"].Schemas["INFORMATION_SCHEMA"].Views["DOMAINS"]);
      var select = Sql.Select(domains);
      select.Columns.AddRange(
        domains["DOMAIN_NAME"],
        domains["DOMAIN_DEFAULT"],
        domains["DATA_TYPE"],
        domains["CHARACTER_MAXIMUM_LENGTH"],
        domains["NUMERIC_PRECISION"],
        domains["NUMERIC_SCALE"]
      );
      select.Where = domains["DOMAIN_CATALOG"]==schema.Catalog.Name && domains["DOMAIN_SCHEMA"]==schema.Name;
      using (var command = new SqlCommand(context.Connection)) {
        command.Transaction = context.Transaction;
        command.Statement = select;
        using (var reader = command.ExecuteReader()) {
          while (reader.Read()) {
            string domainName = (string)reader[0];
            object domainDefault = reader[1];
            var dataType = ReadDataType(context, reader);
            schema.CreateDomain(domainName, dataType,
              Convert.IsDBNull(domainDefault) ? null : Sql.Native((string)domainDefault));
          }
        }
      }
    }

    /// <inheritdoc/>
    public override void Initialize(SqlExtractorContext context)
    {
      if (initialized)
        return;
      model = new Model();
      model.CreateServer(context.Connection.ConnectionInfo.Host);
      model.DefaultServer.CreateCatalog("master");
      Catalog catalog = model.DefaultServer.DefaultCatalog;

      // select schema
      using (IDbCommand cmd = context.Connection.RealConnection.CreateCommand()) {
        cmd.Transaction = context.Transaction;
        cmd.CommandText = @" Select Distinct " + "\n [CATALOG_NAME], " + "\n [SCHEMA_NAME], " + "\n [SCHEMA_OWNER] " +
                          "\n From [Master].[INFORMATION_SCHEMA].[Schemata]";

        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read())
            catalog.CreateSchema((string)reader["SCHEMA_NAME"]);
        }
      }

      // select views
      foreach (Schema cSchema in model.DefaultServer.DefaultCatalog.Schemas) {
        string selectViewsSQL = @" Select [all_views].[name] as [VIEW_NAME], [schemas].[name] as [SCHEMA_NAME] " +
                                "\n From [sys].[all_views] " +
                                "\n Left Join [sys].[schemas] on [schemas].[schema_id]=[all_views].[schema_id] " +
                                "\n Where [schemas].[name]='" + cSchema.Name + "'";

        using (IDbCommand cmd = context.Connection.RealConnection.CreateCommand()) {
          cmd.Transaction = context.Transaction;
          cmd.CommandText = selectViewsSQL;
          using (IDataReader reader = cmd.ExecuteReader()) {
            while (reader.Read())
              cSchema.CreateView((string)reader["VIEW_NAME"], Sql.Native("view def"));
          }
        }

        // select columns
        string selectColumnsSql =
          @" Select [all_views].[name] as [VIEW_NAME], [all_columns].[name] as [COLUMN_NAME], " +
          "\n [schemas].[name] as [SCHEMA_NAME] " + "\n From [sys].[all_views] " +
          "\n Left Join [sys].[all_columns] on [all_columns].[object_id]=[all_views].[object_id] " +
          "\n Left Join [sys].[schemas] on [schemas].[schema_id]=[all_views].[schema_id] " +
          "\n Where [schemas].[name] = '" + cSchema.Name + "'";

        using (IDbCommand cmd = context.Connection.RealConnection.CreateCommand()) {
          cmd.Transaction = context.Transaction;
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
      initialized = true;
    }

    private SqlValueType ReadDataType(SqlExtractorContext context, IDataReader reader)
    {
      byte precision = 0;
      if (!Convert.IsDBNull(reader["NUMERIC_PRECISION"]))
        precision = Convert.ToByte(reader["NUMERIC_PRECISION"]);

      byte scale = 0;
      if (!Convert.IsDBNull(reader["NUMERIC_SCALE"]))
        scale = Convert.ToByte(reader["NUMERIC_SCALE"]);

      DataTypeInfo dataTypeInfo = context.Connection.Driver.ServerInfo.DataTypes[(string)reader["DATA_TYPE"]];
      SqlDataType dataType = dataTypeInfo != null ? dataTypeInfo.SqlType : SqlDataType.Unknown;

      // dataType and size
      int size = 0;
      if ((dataType == SqlDataType.Char) || (dataType == SqlDataType.AnsiChar) ||
        (dataType == SqlDataType.VarChar) || (dataType == SqlDataType.AnsiVarChar) ||
          (dataType == SqlDataType.Binary) || (dataType == SqlDataType.VarBinary))
      {
        size = Convert.ToInt32(reader["CHARACTER_MAXIMUM_LENGTH"]);
        if (size == -1)
        {
          size = 0;
          switch (dataType)
          {
            case SqlDataType.VarChar:
              dataType = SqlDataType.VarCharMax;
              break;
            case SqlDataType.AnsiVarChar:
              dataType = SqlDataType.AnsiVarCharMax;
              break;
            case SqlDataType.VarBinary:
              dataType = SqlDataType.VarBinaryMax;
              break;
          }
        }
      }

      return size == 0
        ? new SqlValueType(dataType, precision, scale)
        : new SqlValueType(dataType, size);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MssqlExtractor"/> class.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected internal MssqlExtractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}