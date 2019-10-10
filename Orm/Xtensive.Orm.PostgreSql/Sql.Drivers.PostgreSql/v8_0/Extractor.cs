// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Dml;
using System.Linq;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class Extractor : Model.Extractor
  {
    protected sealed class ExtractionContext
    {
      /// <summary>
      /// Specific schemas to extract
      /// </summary>
      public readonly Dictionary<string, Schema> TargetSchemes = new Dictionary<string, Schema>();

      /// <summary>
      /// Extracted users.
      /// </summary>
      public readonly Dictionary<long, string> UserLookup = new Dictionary<long, string>();

      /// <summary>
      /// Catalog to extract information.
      /// </summary>
      public readonly Catalog Catalog;

      /// <summary>
      /// Extracted schemas.
      /// </summary>
      public readonly Dictionary<long, Schema> SchemaMap = new Dictionary<long, Schema>();

      /// <summary>
      /// Extracted schemas identifiers.
      /// </summary>
      public readonly Dictionary<Schema, long> ReversedSchemaMap = new Dictionary<Schema, long>();

      /// <summary>
      /// Extracted tables.
      /// </summary>
      public readonly Dictionary<long, Table> TableMap = new Dictionary<long, Table>();

      /// <summary>
      /// Extracted views.
      /// </summary>
      public readonly Dictionary<long, View> ViewMap = new Dictionary<long, View>();

      /// <summary>
      /// Extracted sequences.
      /// </summary>
      public readonly Dictionary<long, Sequence> SequenceMap = new Dictionary<long, Sequence>();

      /// <summary>
      /// Extracted index expressions.
      /// </summary>
      public readonly Dictionary<long, ExpressionIndexInfo> ExpressionIndexMap = new Dictionary<long, ExpressionIndexInfo>();

      /// <summary>
      /// Extracted domains.
      /// </summary>
      public readonly Dictionary<long, Domain> DomainMap = new Dictionary<long, Domain>();

      /// <summary>
      /// Extracted columns connected grouped by owner (table or view)
      /// </summary>
      public readonly Dictionary<long, Dictionary<long, TableColumn>> TableColumnMap = new Dictionary<long, Dictionary<long, TableColumn>>();

      public long CurrentUserSysId { get; set; } = -1;
      public long? CurrentUserIdentifier { get; set; }

      public ExtractionContext(Catalog catalog)
      {
        Catalog = catalog;
      }
    }

    protected class ExpressionIndexInfo
    {
      public Index Index { get; set; }
      public short[] Columns { get; set; }

      public ExpressionIndexInfo(Index index, short[] columns)
      {
        Index = index;
        Columns = columns;
      }
    }

    private static ThreadSafeDictionary<Type, Schema> pgCatalogs = ThreadSafeDictionary<Type, Schema>.Create(new object());

    protected long PgClassOid { get; private set; }
    protected Schema PgCatalogSchema { get; private set; }

    #region Table reference creator properties

    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_user.
    /// </summary>
    protected SqlTableRef PgUser => SqlDml.TableRef(PgCatalogSchema.Tables["pg_user"]);

    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_tablespace.
    /// </summary>
    protected SqlTableRef PgTablespace => SqlDml.TableRef(PgCatalogSchema.Tables["pg_tablespace"]);

    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_namespace.
    /// </summary>
    protected SqlTableRef PgNamespace => SqlDml.TableRef(PgCatalogSchema.Tables["pg_namespace"]);

    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_class.
    /// </summary>
    protected SqlTableRef PgClass => SqlDml.TableRef(PgCatalogSchema.Tables["pg_class"]);

    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_index.
    /// </summary>
    protected SqlTableRef PgIndex => SqlDml.TableRef(PgCatalogSchema.Tables["pg_index"]);

    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_attribute.
    /// </summary>
    protected SqlTableRef PgAttribute => SqlDml.TableRef(PgCatalogSchema.Tables["pg_attribute"]);

    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_attrdef.
    /// </summary>
    protected SqlTableRef PgAttrDef => SqlDml.TableRef(PgCatalogSchema.Tables["pg_attrdef"]);

    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_views.
    /// </summary>
    protected SqlTableRef PgViews => SqlDml.TableRef(PgCatalogSchema.Tables["pg_views"]);

    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_constraint.
    /// </summary>
    protected SqlTableRef PgConstraint => SqlDml.TableRef(PgCatalogSchema.Tables["pg_constraint"]);

    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_type.
    /// </summary>
    protected SqlTableRef PgType => SqlDml.TableRef(PgCatalogSchema.Tables["pg_type"]);

    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_depend.
    /// </summary>
    protected SqlTableRef PgDepend => SqlDml.TableRef(PgCatalogSchema.Tables["pg_depend"]);

    #endregion

    #region Initialization

    protected override void Initialize()
    {
      PgCatalogSchema = pgCatalogs.GetValue(GetType(), CreatePgCatalogSchema);

      // Query OID of some system catalog tables for using them in pg_depend lookups

      SqlTableRef rel = PgClass;
      SqlSelect q = SqlDml.Select(rel);
      q.Where = SqlDml.In(rel["relname"], SqlDml.Row("pg_class"));
      q.Columns.Add(rel["oid"]);
      q.Columns.Add(rel["relname"]);

      using (var cmd = Connection.CreateCommand(q))
      using (var dr = cmd.ExecuteReader())
        while (dr.Read()) {
          long oid = Convert.ToInt64(dr[0]);
          string name = dr.GetString(1);
          if (name=="pg_class")
            PgClassOid = oid;
        }
    }

    private Schema CreatePgCatalogSchema(Type dummy)
    {
      var pgCatalog = new Catalog("info_catalog");
      var pgSchema = pgCatalog.CreateSchema("pg_catalog");
      BuildPgCatalogSchema(pgSchema);
      return pgSchema;
    }

    protected virtual void BuildPgCatalogSchema(Schema schema)
    {
      Table t;
      t = schema.CreateTable("pg_user");
      CreateTextColumn(t, "usename");
      CreateInt4Column(t, "usesysid");

      t = schema.CreateTable("pg_tablespace");
      CreateOidColumn(t);
      CreateTextColumn(t, "spcname");
      CreateInt4Column(t, "spcowner");

      t = schema.CreateTable("pg_namespace");
      CreateOidColumn(t);
      CreateTextColumn(t, "nspname");
      CreateInt4Column(t, "nspowner");

      t = schema.CreateTable("pg_class");
      CreateOidColumn(t);
      CreateTextColumn(t, "relname");
      CreateInt4Column(t, "relnamespace");
      CreateInt4Column(t, "relowner");
      CreateInt4Column(t, "reltablespace");
      CreateChar1Column(t, "relkind");

      t = schema.CreateTable("pg_index");
      CreateOidColumn(t);
      CreateInt4Column(t, "indexrelid");
      CreateInt4Column(t, "indrelid");
      CreateBoolColumn(t, "indisunique");
      CreateBoolColumn(t, "indisprimary");
      CreateBoolColumn(t, "indisclustered");
      CreateTextColumn(t, "indkey");
      CreateTextColumn(t, "indoption");
      CreateTextColumn(t, "indexprs");
      CreateTextColumn(t, "indpred");
      CreateInt2Column(t, "indnatts");

      t = schema.CreateTable("pg_attribute");
      CreateOidColumn(t);
      CreateInt4Column(t, "attrelid");
      CreateTextColumn(t, "attname");
      CreateInt4Column(t, "atttypid");
      CreateInt2Column(t, "attlen");
      CreateInt2Column(t, "attnum");
      CreateInt4Column(t, "atttypmod");
      CreateBoolColumn(t, "attnotnull");
      CreateBoolColumn(t, "atthasdef");
      CreateBoolColumn(t, "attisdropped");

      t = schema.CreateTable("pg_attrdef");
      CreateOidColumn(t);
      CreateInt4Column(t, "adrelid");
      CreateInt2Column(t, "adnum");
      CreateTextColumn(t, "adsrc");

      t = schema.CreateTable("pg_constraint");
      CreateOidColumn(t);
      CreateTextColumn(t, "conname");
      CreateInt4Column(t, "connamespace");
      CreateChar1Column(t, "contype");
      CreateBoolColumn(t, "condeferrable");
      CreateBoolColumn(t, "condeferred");
      CreateInt4Column(t, "conrelid");
      CreateInt4Column(t, "contypid");
      CreateInt4Column(t, "confrelid");
      CreateChar1Column(t, "confupdtype");
      CreateChar1Column(t, "confdeltype");
      CreateChar1Column(t, "confmatchtype");
      CreateTextColumn(t, "conkey");
      CreateTextColumn(t, "confkey");
      CreateTextColumn(t, "consrc");

      t = schema.CreateTable("pg_type");
      CreateOidColumn(t);
      CreateTextColumn(t, "typname");
      CreateInt4Column(t, "typbasetype");
      CreateInt4Column(t, "typnamespace");
      CreateInt4Column(t, "typowner");
      CreateChar1Column(t, "typtype");
      CreateBoolColumn(t, "typnotnull");
      CreateInt2Column(t, "typlen");
      CreateInt4Column(t, "typtypmod");
      CreateTextColumn(t, "typdefault");
      CreateBoolColumn(t, "typisdefined");

      t = schema.CreateTable("pg_depend");
      CreateInt4Column(t, "classid");
      CreateInt4Column(t, "objid");
      CreateInt4Column(t, "objsubid");
      CreateInt4Column(t, "refclassid");
      CreateInt4Column(t, "refobjid");
      CreateInt4Column(t, "refobjsubid");
      CreateChar1Column(t, "deptype");
    }

    #endregion

    public override Catalog ExtractCatalog(string catalogName)
    {
      var catalog = new Catalog(catalogName);
      var context = new ExtractionContext(catalog);
      ExtractUsers(context);
      ExtractSchemas(context);
      return catalog;
    }

    public override Schema ExtractSchema(string catalogName, string schemaName)
    {
      var catalog = new Catalog(catalogName);
      var context = new ExtractionContext(catalog);
      context.TargetSchemes.Add(schemaName, catalog.CreateSchema(schemaName));
      ExtractUsers(context);
      ExtractSchemas(context);
      var result = context.TargetSchemes[schemaName];
      return result;
    }

    public override Catalog ExtractSchemes(string catalogName, string[] schemaNames)
    {
      var catalog = new Catalog(catalogName);
      var context = new ExtractionContext(catalog);
      foreach (var schemaName in schemaNames)
        context.TargetSchemes.Add(schemaName, catalog.CreateSchema(schemaName));
      ExtractUsers(context);
      ExtractSchemas(context);
      return catalog;
    }

    /// <summary>
    /// Extracts users and puts them to <see cref="ExtractionContext.UserLookup"/> and sets current user identifier
    /// </summary>
    /// <param name="context"></param>
    protected void ExtractUsers(ExtractionContext context)
    {
      context.UserLookup.Clear();
      string me;
      using (var command = Connection.CreateCommand("SELECT user"))
        me = (string) command.ExecuteScalar();

      using (DbCommand cmd = Connection.CreateCommand("SELECT usename, usesysid FROM pg_user"))
      using (DbDataReader dr = cmd.ExecuteReader()) {
        while (dr.Read()) {
          string name = dr[0].ToString();
          long sysid = Convert.ToInt64(dr[1]);
          context.UserLookup.Add(sysid, name);
          if (name==me)
            context.CurrentUserSysId = sysid;
        }
      }
    }

    /// <summary>
    /// Extracts complete information about schemas.
    /// </summary>
    /// <param name="context">The context.</param>
    protected void ExtractSchemas(ExtractionContext context)
    {
      context.CurrentUserIdentifier = GetMyUserSysId(context.CurrentUserSysId);

      //Extraction of public schemas and schemas which is owned by current user
      ExtractSchemasInfo(context);

      //Extraction of tables, views and sequences
      ExtractSchemaContents(context);

      //Extraction of columns of table and view
      ExtractTableAndViewColumns(context);

      //Extraction of table indexes
      ExtractTableIndexes(context);

      //Extraction of domains
      ExtractDomains(context);

      //Extraction of table and domain constraints
      ExtractTableAndDomainConstraints(context);

      //sequence infos
      ExtractSequenses(context);
    }

    /// <summary>
    /// Extracts general information about existing schemas, nothing else.
    /// </summary>
    /// <param name="context">The context.</param>
    protected virtual void ExtractSchemasInfo(ExtractionContext context)
    {
      var namespaceTable1 = PgNamespace;
      var namespaceTable2 = PgNamespace;
      var selectPublic = SqlDml.Select(namespaceTable1);
      selectPublic.Where = namespaceTable1["nspname"]=="public"
                           && namespaceTable1["nspowner"] != context.CurrentUserIdentifier;
      selectPublic.Columns.Add(namespaceTable1["nspname"]);
      selectPublic.Columns.Add(namespaceTable1["oid"]);
      selectPublic.Columns.Add(namespaceTable1["nspowner"]);

      var selectMine = SqlDml.Select(namespaceTable2);
      selectMine.Where = namespaceTable2["nspowner"]==context.CurrentUserIdentifier;
      selectMine.Columns.Add(namespaceTable2["nspname"]);
      selectMine.Columns.Add(namespaceTable2["oid"]);
      selectMine.Columns.Add(namespaceTable2["nspowner"]);

      var union = selectPublic.UnionAll(selectMine);
      var catalog = context.Catalog;

      using (var command = Connection.CreateCommand(union))
      using (var dataReader = command.ExecuteReader()) {
        while (dataReader.Read()) {
          var oid = Convert.ToInt64(dataReader["oid"]);
          var name = dataReader["nspname"].ToString();
          var owner = Convert.ToInt64(dataReader["nspowner"]);

          var schema = catalog.Schemas[name] ?? catalog.CreateSchema(name);
          if (name=="public")
            catalog.DefaultSchema = schema;
          schema.Owner = context.UserLookup[owner];
          context.SchemaMap[oid] = schema;
          context.ReversedSchemaMap[schema] = oid;
        }
      }
    }

    /// <summary>
    /// Extracts general information schemas' content like tables, view, sequences and puts it to the context.
    /// </summary>
    /// <param name="context">The context.</param>
    protected virtual void ExtractSchemaContents(ExtractionContext context)
    {
      var catalog = context.Catalog;
      var targetSchemes = context.TargetSchemes;

      if (context.SchemaMap.Count > 0) {
        var relationsTable = PgClass;
        var tablespacesTable = PgTablespace;

        var join = relationsTable.LeftOuterJoin(tablespacesTable, tablespacesTable["oid"]==relationsTable["reltablespace"]);
        var select = SqlDml.Select(join);
        select.Where = relationsTable["relowner"]==context.CurrentUserIdentifier
          && SqlDml.In(relationsTable["relkind"], SqlDml.Row('r', 'v', 'S'));

        if (targetSchemes!=null && targetSchemes.Count > 0) {
          var schemesIndexes = catalog.Schemas.Where(sch => targetSchemes.ContainsKey(sch.Name)).Select(sch => context.ReversedSchemaMap[sch]);
          select.Where &= SqlDml.In(relationsTable["relnamespace"], CreateOidRow(schemesIndexes));
        }
        select.Columns.Add(relationsTable["oid"], "reloid");
        select.Columns.Add(relationsTable["relname"]);
        select.Columns.Add(relationsTable["relkind"]);
        select.Columns.Add(relationsTable["relnamespace"]);
        select.Columns.Add(tablespacesTable["spcname"]);
        select.Columns.Add(new Func<SqlCase>(() => {
          SqlCase defCase = SqlDml.Case(relationsTable["relkind"]);
          defCase.Add('v', SqlDml.FunctionCall("pg_get_viewdef", relationsTable["oid"]));
          return defCase;
        })(), "definition");

        using (var command = Connection.CreateCommand(select))
        using (var dataReader = command.ExecuteReader()) {
          while (dataReader.Read()) {
            var relationOid = Convert.ToInt64(dataReader["reloid"]);
            var relationKind = dataReader["relkind"].ToString();
            var relationName = dataReader["relname"].ToString();
            var relationNamespace = Convert.ToInt64(dataReader["relnamespace"]);

            Schema schema;
            if (!context.SchemaMap.TryGetValue(relationNamespace, out schema))
              continue;
            Debug.Assert(schema!=null);
            if (relationKind=="r") {
              var table = schema.CreateTable(relationName);
              var tableSpaceName = dataReader["spcname"];
              if (tableSpaceName != DBNull.Value && tableSpaceName != null)
                table.Filegroup = Convert.ToString(tableSpaceName);
              context.TableMap.Add(relationOid, table);
            }
            else if (relationKind=="v") {
              var definition = dataReader["definition"].ToString();
              var view = schema.CreateView(relationName, SqlDml.Native(definition), CheckOptions.None);
              context.ViewMap.Add(relationOid, view);
            }
            else if (relationKind=="S") {
              var sequence = schema.CreateSequence(relationName);
              context.SequenceMap.Add(relationOid, sequence);
            }
          }
        }
      }
    }

    /// <summary>
    /// Extracts information about columns for items listed in <see cref="ExtractionContext.TableMap"/> and <see cref="ExtractionContext.ViewMap"/> and fills <see cref="ExtractionContext.TableColumnMap"/>.
    /// </summary>
    /// <param name="context">The context</param>
    protected virtual void ExtractTableAndViewColumns(ExtractionContext context)
    {
      var tableMap = context.TableMap;
      var viewMap = context.ViewMap;
      var tableColumns = context.TableColumnMap;

      if (tableMap.Count > 0 || viewMap.Count > 0) {
        var columnsTable = PgAttribute;
        var dafaultValuesTable = PgAttrDef;
        var typesTable = PgType;

        var select = SqlDml.Select(columnsTable
          .LeftOuterJoin(dafaultValuesTable, columnsTable["attrelid"]==dafaultValuesTable["adrelid"] && columnsTable["attnum"]==dafaultValuesTable["adnum"])
          .InnerJoin(typesTable, typesTable["oid"]==columnsTable["atttypid"]));

        select.Where = columnsTable["attisdropped"]==false &&
                       columnsTable["attnum"] > 0 &&
                       (SqlDml.In(columnsTable["attrelid"], CreateOidRow(tableMap.Keys)) ||
                        SqlDml.In(columnsTable["attrelid"], CreateOidRow(viewMap.Keys)));

        select.Columns.Add(columnsTable["attrelid"]);
        select.Columns.Add(columnsTable["attnum"]);
        select.Columns.Add(columnsTable["attname"]);
        select.Columns.Add(typesTable["typname"]);
        select.Columns.Add(columnsTable["atttypmod"]);
        select.Columns.Add(columnsTable["attnotnull"]);
        select.Columns.Add(columnsTable["atthasdef"]);
        select.Columns.Add(dafaultValuesTable["adsrc"]);
        select.OrderBy.Add(columnsTable["attrelid"]);
        select.OrderBy.Add(columnsTable["attnum"]);

        using (var command = Connection.CreateCommand(select))
        using (DbDataReader dataReader = command.ExecuteReader()) {
          while (dataReader.Read()) {
            var columnOwnerId = Convert.ToInt64(dataReader["attrelid"]);
            var columnId = Convert.ToInt64(dataReader["attnum"]);
            var columnName = dataReader["attname"].ToString();
            if (tableMap.ContainsKey(columnOwnerId)) {
              var table = tableMap[columnOwnerId];
              Debug.Assert(table != null);
              TableColumn col = table.CreateColumn(columnName);
              if (!tableColumns.ContainsKey(columnOwnerId))
                tableColumns.Add(columnOwnerId, new Dictionary<long, TableColumn>());
              tableColumns[columnOwnerId].Add(columnId, col);

              var columnTypeName = dataReader["typname"].ToString();
              var columnTypeSpecificData = Convert.ToInt32(dataReader["atttypmod"]);
              var notNullFlag = dataReader.GetBoolean(dataReader.GetOrdinal("attnotnull"));
              var defaultValueFlag = dataReader.GetBoolean(dataReader.GetOrdinal("atthasdef"));
              if (defaultValueFlag) {
                var defaultValue = dataReader["adsrc"].ToString();
                col.DefaultValue = SqlDml.Native(defaultValue);
              }
              col.IsNullable = !notNullFlag;
              col.DataType = GetSqlValueType(columnTypeName, columnTypeSpecificData);
            }
            else {
              var view = viewMap[columnOwnerId];
              Debug.Assert(view != null);
              view.CreateColumn(columnName);
            }
          }
        }
      }
    }

    /// <summary>
    /// Extracts information about indexes for the tables listed in <see cref="ExtractionContext.TableMap"/>.
    /// </summary>
    /// <param name="context">The context</param>
    protected virtual void ExtractTableIndexes(ExtractionContext context)
    {
      var tableMap = context.TableMap;
      var tableColumns = context.TableColumnMap;

      if (tableMap.Count > 0) {
        var tableSpacesTable = PgTablespace;
        var relationsTable = PgClass;
        var indexTable = PgIndex;
        var dependencyTable = PgDepend;

        //subselect that index was not created automatically
        var subSelect = SqlDml.Select(dependencyTable);
        subSelect.Where = dependencyTable["classid"]==PgClassOid &&
                          dependencyTable["objid"]==indexTable["indexrelid"] && 
                          dependencyTable["deptype"]=='i';
        subSelect.Columns.Add(dependencyTable[0]);

        //not automatically created indexes of our tables
        var select = SqlDml.Select(indexTable
          .InnerJoin(relationsTable, relationsTable["oid"]==indexTable["indexrelid"])
          .LeftOuterJoin(tableSpacesTable, tableSpacesTable["oid"]==relationsTable["reltablespace"]));
        select.Where = SqlDml.In(indexTable["indrelid"], CreateOidRow(tableMap.Keys)) && !SqlDml.Exists(subSelect);
        select.Columns.Add(indexTable["indrelid"]);
        select.Columns.Add(indexTable["indexrelid"]);
        select.Columns.Add(relationsTable["relname"]);
        select.Columns.Add(indexTable["indisunique"]);
        select.Columns.Add(indexTable["indisclustered"]);
        select.Columns.Add(indexTable["indkey"]);
        select.Columns.Add(tableSpacesTable["spcname"]);
        select.Columns.Add(indexTable["indnatts"]);
        select.Columns.Add(indexTable["indexprs"]);
        select.Columns.Add(indexTable["indpred"]);
        select.Columns.Add(SqlDml.FunctionCall("pg_get_indexdef", indexTable["indexrelid"]), "inddef");
        AddSpecialIndexQueryColumns(select, tableSpacesTable, relationsTable, indexTable, dependencyTable);

        int maxColumnNumber = 0;
        using (var command = Connection.CreateCommand(select))
        using (var dataReader = command.ExecuteReader()) {
          while (dataReader.Read()) {
            var tableIdentifier = Convert.ToInt64(dataReader["indrelid"]);
            var indexIdentifier = Convert.ToInt64(dataReader["indexrelid"]);
            var indexName = dataReader["relname"].ToString();
            var isUnique = dataReader.GetBoolean(dataReader.GetOrdinal("indisunique"));
            var isClustered = dataReader.GetBoolean(dataReader.GetOrdinal("indisclustered"));
            var indexKey = (short[])dataReader["indkey"];

            var tablespaceName = (dataReader["spcname"]!=DBNull.Value) ? dataReader["spcname"].ToString() : (string)null;
            var filterExpression = (dataReader["indpred"]!=DBNull.Value) ? dataReader["indpred"].ToString() : string.Empty;

            var table = tableMap[tableIdentifier];

            var fullTextRegex = @"(?<=CREATE INDEX \S+ ON \S+ USING (?:gist|gin)(?:\s|\S)*)to_tsvector\('(\w+)'::regconfig, \(*(?:(?:\s|\)|\(|\|)*(?:\(""(\S+)""\)|'\s')::text)+\)";
            var indexScript = dataReader["inddef"].ToString();
            var matches = Regex.Matches(indexScript, fullTextRegex, RegexOptions.Compiled);
            if (matches.Count > 0) {
              // Fulltext index
              var fullTextIndex = table.CreateFullTextIndex(indexName);
              foreach (Match match in matches) {
                var columnConfigurationName = match.Groups[1].Value;
                foreach (Capture capture in match.Groups[2].Captures) {
                  var columnName = capture.Value;
                  var fullTextColumn = fullTextIndex.Columns[columnName] ?? fullTextIndex.CreateIndexColumn(table.Columns.Single(column => column.Name == columnName));
                  if (fullTextColumn.Languages[columnConfigurationName]==null)
                    fullTextColumn.Languages.Add(new Language(columnConfigurationName));
                }
              }
            }
            else {
              //Regular index
              var index = table.CreateIndex(indexName);
              index.IsBitmap = false;
              index.IsUnique = isUnique;
              index.Filegroup = tablespaceName;
              if (!string.IsNullOrEmpty(filterExpression))
                index.Where = SqlDml.Native(filterExpression);

              // Expression-based index
              var some = dataReader["indexprs"];
              if (some!=DBNull.Value) {
                context.ExpressionIndexMap[indexIdentifier] = new ExpressionIndexInfo(index, indexKey);
                int columnNumber = dataReader.GetInt16(dataReader.GetOrdinal("indnatts"));
                if (columnNumber > maxColumnNumber)
                  maxColumnNumber = columnNumber;
              }
              else {
                for (int j = 0; j < indexKey.Length; j++) {
                  int colIndex = indexKey[j];
                  if (colIndex > 0)
                    index.CreateIndexColumn(tableColumns[tableIdentifier][colIndex], true);
                  else{
                    int z = 7;
                    //column index is 0
                    //this means that this index column is an expression
                    //which is not possible with SqlDom tables
                  }
                }
              }
              ReadSpecialIndexProperties(dataReader, index);
            }
          }
        }

        var expressionIndexMap = context.ExpressionIndexMap;

        if (expressionIndexMap.Count > 0) {
          select = SqlDml.Select(indexTable);
          select.Columns.Add(indexTable["indrelid"]);
          select.Columns.Add(indexTable["indexrelid"]);

          for (int i = 1; i <= maxColumnNumber; i++)
            select.Columns.Add(SqlDml.FunctionCall("pg_get_indexdef", indexTable["indexrelid"], i, true), i.ToString());
          select.Where = SqlDml.In(indexTable["indexrelid"], SqlDml.Array(expressionIndexMap.Keys.ToArray()));

          using (var command = Connection.CreateCommand(select))
          using (var dataReader = command.ExecuteReader()) {
            while (dataReader.Read()) {
              var exprIndexInfo = expressionIndexMap[Convert.ToInt64(dataReader[1])];
              for (int j = 0; j < exprIndexInfo.Columns.Length; j++) {
                int colIndex = exprIndexInfo.Columns[j];
                if (colIndex > 0)
                  exprIndexInfo.Index.CreateIndexColumn(tableColumns[Convert.ToInt64(dataReader[0])][colIndex], true);
                else
                  exprIndexInfo.Index.CreateIndexColumn(SqlDml.Native(dataReader[(j + 1).ToString()].ToString()));
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Extracts information about domains for the schemas listed in <see cref="ExtractionContext.SchemaMap"/> and fills <see cref="ExtractionContext.DomainMap"/>
    /// </summary>
    /// <param name="context">The context.</param>
    protected virtual void ExtractDomains(ExtractionContext context)
    {
      var schemaIndex = context.SchemaMap;
      var domains = context.DomainMap;

      if (schemaIndex.Count > 0) {
        var typeTable = PgType;
        var baseTypeTable = PgType;
        var select = SqlDml.Select(typeTable.InnerJoin(baseTypeTable, baseTypeTable["oid"]==typeTable["typbasetype"]));
        select.Where = typeTable["typisdefined"]==true &&
                       typeTable["typtype"]=='d' &&
                       SqlDml.In(typeTable["typnamespace"], CreateOidRow(schemaIndex.Keys)) &&
                       typeTable["typowner"]==context.CurrentUserIdentifier;
        select.Columns.Add(typeTable["oid"]);
        select.Columns.Add(typeTable["typname"], "typname");
        select.Columns.Add(typeTable["typnamespace"], "typnamespace");
        select.Columns.Add(typeTable["typtypmod"], "typmod");
        select.Columns.Add(typeTable["typdefault"], "default");
        select.Columns.Add(baseTypeTable["typname"], "basetypname");

        using (var command = Connection.CreateCommand(select))
        using (var dataReader = command.ExecuteReader()) {
          while (dataReader.Read()) {
            var typeId = Convert.ToInt64(dataReader["oid"]);
            var typeNamespace = Convert.ToInt64(dataReader["typnamespace"]);
            var typeName = dataReader["typname"].ToString();
            var baseTypeName = dataReader["basetypname"].ToString();
            int typmod = Convert.ToInt32(dataReader["typmod"]);
            var defaultValue = (dataReader["default"]!=DBNull.Value) ? dataReader["default"].ToString() : (string)null;

            Schema schema;
            if (!schemaIndex.TryGetValue(typeNamespace, out schema))
              continue;
            var domain = schema.CreateDomain(typeName, GetSqlValueType(baseTypeName, typmod));
            domain.DefaultValue = (defaultValue==null) ? (SqlExpression)SqlDml.Null : (SqlExpression)SqlDml.Native(defaultValue);
            domains.Add(typeId, domain);
          }
        }
      }
    }

    /// <summary>
    /// Extracts table and domain constraints for items listed in <see cref="ExtractionContext.TableMap"/> and <see cref="ExtractionContext.DomainMap"/>.
    /// </summary>
    /// <param name="context">The context.</param>
    protected virtual void ExtractTableAndDomainConstraints(ExtractionContext context)
    {
      var tableMap = context.TableMap;
      var domainMap = context.DomainMap;
      var tableColumns = context.TableColumnMap;

      if (tableMap.Count > 0 || domainMap.Count > 0) {
        var constraintTable = PgConstraint;
        var select = SqlDml.Select(constraintTable);
        select.Where = SqlDml.In(constraintTable["conrelid"], CreateOidRow(tableMap.Keys)) ||
                       SqlDml.In(constraintTable["contypid"], CreateOidRow(domainMap.Keys));

        select.Columns.AddRange(constraintTable["conname"],
          constraintTable["contype"],
          constraintTable["condeferrable"],
          constraintTable["condeferred"],
          constraintTable["conrelid"],
          constraintTable["contypid"],
          constraintTable["conkey"],
          constraintTable["consrc"],
          constraintTable["confrelid"],
          constraintTable["confkey"],
          constraintTable["confupdtype"],
          constraintTable["confdeltype"],
          constraintTable["confmatchtype"]);

        using (var command = Connection.CreateCommand(select))
        using (var dataReader = command.ExecuteReader()) {
          while (dataReader.Read()) {
            var constraintType = dataReader["contype"].ToString()[0];
            var constraintName = dataReader["conname"].ToString();
            var isDeferrable = dataReader.GetBoolean(dataReader.GetOrdinal("condeferrable"));
            var isDeferred = dataReader.GetBoolean(dataReader.GetOrdinal("condeferred"));
            var tableId = Convert.ToInt64(dataReader["conrelid"]);
            var domainId = Convert.ToInt64(dataReader["contypid"]);
            object constraintKeyColumns = dataReader["conkey"];

            if (tableId!=0) {
              //table constraint
              var table = tableMap[tableId];
              if (constraintType=='c') {
                //[c]heck
                var consrc = dataReader["consrc"].ToString();
                var constraint = table.CreateCheckConstraint(constraintName, SqlDml.Native(consrc));
                constraint.IsDeferrable = isDeferrable;
                constraint.IsInitiallyDeferred = isDeferred;
              }
              else {
                var columnsOfTable = tableColumns[tableId];
                if (constraintType=='u' || constraintType=='p') {
                  //[u]nique or [p]rimary key
                  UniqueConstraint constraint = (constraintType=='u')
                    ? table.CreateUniqueConstraint(constraintName)
                    : table.CreatePrimaryKey(constraintName);

                  constraint.IsDeferrable = isDeferrable;
                  constraint.IsInitiallyDeferred = isDeferred;
                  int[] colIndexes = ReadIntArray(constraintKeyColumns);
                  for (int i = 0; i < colIndexes.Length; i++)
                    constraint.Columns.Add(columnsOfTable[colIndexes[i]]);
                }
                else if (constraintType=='f') {
                  //[f]oreign key
                  object confkey = dataReader["confkey"];
                  var referencedTableId = Convert.ToInt64(dataReader["confrelid"]);
                  var updateAction = dataReader["confupdtype"].ToString()[0];
                  var deleteAction = dataReader["confdeltype"].ToString()[0];
                  var matchType = dataReader["confmatchtype"].ToString()[0];

                  var foreignKey = table.CreateForeignKey(constraintName);
                  foreignKey.IsDeferrable = isDeferrable;
                  foreignKey.IsInitiallyDeferred = isDeferred;
                  foreignKey.OnDelete = GetReferentialAction(deleteAction);
                  foreignKey.OnUpdate = GetReferentialAction(updateAction);
                  foreignKey.MatchType = GetMatchType(matchType);
                  foreignKey.ReferencedTable = tableMap[referencedTableId];

                  var fkeyColumns = tableColumns[referencedTableId];

                  int[] colIndexes = ReadIntArray(constraintKeyColumns);
                  for (int i = 0; i < colIndexes.Length; i++)
                    foreignKey.Columns.Add(columnsOfTable[colIndexes[i]]);

                  colIndexes = ReadIntArray(confkey);
                  for (int i = 0; i < colIndexes.Length; i++)
                    foreignKey.ReferencedColumns.Add(fkeyColumns[colIndexes[i]]);
                }
              }
            }
            else if (domainId!=0) {
              //domain constraint
              if (constraintType=='c') {
                //check
                string consrc = dataReader["consrc"].ToString();
                var domain = domainMap[domainId];
                var constraint = domain.CreateConstraint(constraintName, SqlDml.Native(consrc));
                constraint.IsDeferrable = isDeferrable;
                constraint.IsInitiallyDeferred = isDeferred;
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Extracts sequences listed in <see cref="ExtractionContext.SequenceMap"/>.
    /// </summary>
    /// <param name="context">The context</param>
    protected virtual void ExtractSequenses(ExtractionContext context)
    {
      var sequenceMap = context.SequenceMap;

      if (sequenceMap.Count > 0) {
        //Have to do it traditional string concat because cannot select from 
        //a sequence with Sql.Dom
        var query = new StringBuilder(); {
          Sequence[] seqArray = new Sequence[sequenceMap.Count];
          sequenceMap.Values.CopyTo(seqArray, 0);
          Sequence seq = seqArray[0];
          query.AppendFormat("SELECT * FROM (\nSELECT {0} as id, * FROM {1}", 0,
            Driver.Translator.Translate(null, seq)); // context is not used in PostrgreSQL translator
          for (int i = 1; i < sequenceMap.Count; i++)
          {
            seq = seqArray[i];
            query.AppendFormat("\nUNION ALL\nSELECT {0} as id, * FROM {1}", i,
              Driver.Translator.Translate(null, seq)); // context is not used in PostgreSQL translator
          }
          query.Append("\n) all_sequences\nORDER BY id");
        }

        using (DbCommand cmd = Connection.UnderlyingConnection.CreateCommand()) {
          cmd.CommandText = query.ToString();
          using (DbDataReader dr = cmd.ExecuteReader()) {
            foreach (Sequence seq in sequenceMap.Values) {
              dr.Read();
              ReadSequenceDescriptor(dr, seq.SequenceDescriptor);
            }
          }
        }
      }
    }

    protected virtual void AddSpecialIndexQueryColumns(SqlSelect query, SqlTableRef spc, SqlTableRef rel, SqlTableRef ind, SqlTableRef depend)
    {
    }

    protected virtual void ReadSpecialIndexProperties(DbDataReader dr, Index i)
    {
    }

    protected virtual void ReadSequenceDescriptor(DbDataReader reader, SequenceDescriptor descriptor)
    {
      descriptor.Increment = Convert.ToInt64(reader["increment_by"]);
      descriptor.IsCyclic = Convert.ToBoolean(reader["is_cycled"]);
      descriptor.MinValue = Convert.ToInt64(reader["min_value"]);
      descriptor.MaxValue = Convert.ToInt64(reader["max_value"]);
      descriptor.StartValue = Convert.ToInt64(reader["min_value"]);
    }

    #region Column creation methods

    protected void CreateOidColumn(Table t)
    {
      t.CreateColumn("oid", new SqlValueType(SqlType.Int64));
    }

    protected void CreateInt2Column(Table t, string name)
    {
      t.CreateColumn(name, new SqlValueType(SqlType.Int16));
    }

    protected void CreateInt4Column(Table t, string name)
    {
      t.CreateColumn(name, new SqlValueType(SqlType.Int32));
    }

    protected void CreateChar1Column(Table t, string name)
    {
      t.CreateColumn(name, new SqlValueType(SqlType.Char, 1));
    }

    protected void CreateTextColumn(Table t, string name)
    {
      t.CreateColumn(name, new SqlValueType(SqlType.VarChar));
    }

    protected void CreateBoolColumn(Table t, string name)
    {
      t.CreateColumn(name, new SqlValueType(SqlType.Boolean));
    }

    #endregion

    protected SqlValueType GetSqlValueType(string typname, int typmod)
    {
      DataTypeInfo typeInfo = Driver.ServerInfo.DataTypes[typname];
      // Unlike MS SQL extractor we do not set precision/scale/length for unknown type,
      // 'cause we don't know how to treat typmod
      if (typeInfo==null)
        return new SqlValueType(typname);

      if (typeInfo.Type==SqlType.Decimal) {
        if (typmod==-1)
          // in this case we cannot determine the actual precision and scale
          // it should be avoided
          return new SqlValueType(typeInfo.Type);
        int precision;
        int scale;
        GetPrecisionAndScale(typmod, out precision, out scale);
        return new SqlValueType(typeInfo.Type, precision, scale);
      }
      /*
      if (typeInfo.Type==SqlType.DateTimeOffset)
        return new SqlValueType(typeInfo.Type);
        */
      return typmod!=-1 
        ? new SqlValueType(typeInfo.Type, typmod - 4)
        : new SqlValueType(typeInfo.Type);
    }

    protected void GetPrecisionAndScale(int typmod, out int precision, out int scale)
    {
      //high word
      precision = ((typmod - 4) >> 16);
      //low word
      scale = ((typmod - 4) & 0xFFFF);
    }

    protected ReferentialAction GetReferentialAction(char c)
    {
      switch (c) {
      case 'c':
        return ReferentialAction.Cascade;
      case 'n':
        return ReferentialAction.SetNull;
      case 'd':
        return ReferentialAction.SetDefault;
      case 'r':
        return ReferentialAction.Restrict;
      default:
        return ReferentialAction.NoAction; //a
      }
    }

    protected SqlMatchType GetMatchType(char c)
    {
      switch (c) {
      case 'f':
        return SqlMatchType.Full; //f
      default:
        return SqlMatchType.None; //u
      }
    }

    protected SqlRow CreateOidRow(IEnumerable<long> oids)
    {
      var result = SqlDml.Row();
      foreach (var oid in oids)
        result.Add(oid);
      // make sure it is not empty, so that "IN" expression always works
      // add an invalid OID value 
      if (result.Count==0)
        result.Add(-1000);
      return result;
    }

    private int[] ReadIntArray(object value)
    {
      var shortArray = value as short[];
      if (shortArray!=null) {
        var result = new int[shortArray.Length];
        for (int i = 0; i < shortArray.Length; i++)
          result[i] = shortArray[i];
        return result;
      }
      var intArray = value as int[];
      if (intArray!=null)
        return intArray;
      throw new InvalidOperationException();
    }

    /// <summary>
    /// Gets and caches the inner identifier of the current database user.
    /// </summary>
    private long GetMyUserSysId(long mUserSysId)
    {
      if (mUserSysId < 0)
        using (var cmd = Connection.CreateCommand("SELECT usesysid FROM pg_user WHERE usename = user"))
          return Convert.ToInt64(cmd.ExecuteScalar());
      return mUserSysId;
    }

    // Constructor

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}