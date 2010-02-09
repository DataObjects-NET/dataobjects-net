// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Npgsql;
using Xtensive.Core.Collections;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Dml;
using Xtensive.Core.Threading;
using System.Linq;

namespace Xtensive.Sql.PostgreSql.v8_0
{
  internal class Extractor : Model.Extractor
  {
    private static ThreadSafeDictionary<Type, Schema> pgCatalogs = ThreadSafeDictionary<Type, Schema>.Create(new object());

    // The identifier of the current user
    private int mUserSysId = -1;
    private readonly Dictionary<int, string> mUserLookup = new Dictionary<int, string>();

    protected Catalog catalog;
    protected Schema schema;

    protected int PgClassOid { get; private set; }
    protected Schema PgCatalogSchema { get; private set; }

    private class ExpressionIndexInfo
    {
      public Index Index { get; set; }
      public string Columns { get; set; }

      public ExpressionIndexInfo(Index index, string columns)
      {
        Index = index;
        Columns = columns;
      }
    }

    protected override void Initialize()
    {
      catalog = new Catalog(Driver.CoreServerInfo.DatabaseName);

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
          int oid = Convert.ToInt32(dr[0]);
          string name = dr.GetString(1);
          if (name=="pg_class")
            PgClassOid = oid;
        }
    }

    public override Catalog ExtractCatalog()
    {
      ExtractUsers();
      ExtractSchemas(catalog);
      return catalog;
    }

    public override Schema ExtractSchema(string schemaName)
    {
      schema = catalog.CreateSchema(schemaName);
      ExtractUsers();
      ExtractSchemas(catalog);
      return schema;
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

    #region Column creation methods

    protected void CreateOidColumn(Table t)
    {
      t.CreateColumn("oid", new SqlValueType(SqlType.Int32));
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

    #region Table reference creator properties

    protected SqlTableRef PgUser
    {
      get { return SqlDml.TableRef(PgCatalogSchema.Tables["pg_user"]); }
    }

    protected SqlTableRef PgTablespace
    {
      get { return SqlDml.TableRef(PgCatalogSchema.Tables["pg_tablespace"]); }
    }

    protected SqlTableRef PgNamespace
    {
      get { return SqlDml.TableRef(PgCatalogSchema.Tables["pg_namespace"]); }
    }

    protected SqlTableRef PgClass
    {
      get { return SqlDml.TableRef(PgCatalogSchema.Tables["pg_class"]); }
    }

    protected SqlTableRef PgIndex
    {
      get { return SqlDml.TableRef(PgCatalogSchema.Tables["pg_index"]); }
    }

    protected SqlTableRef PgAttribute
    {
      get { return SqlDml.TableRef(PgCatalogSchema.Tables["pg_attribute"]); }
    }

    protected SqlTableRef PgAttrDef
    {
      get { return SqlDml.TableRef(PgCatalogSchema.Tables["pg_attrdef"]); }
    }

    protected SqlTableRef PgViews
    {
      get { return SqlDml.TableRef(PgCatalogSchema.Tables["pg_views"]); }
    }

    protected SqlTableRef PgConstraint
    {
      get { return SqlDml.TableRef(PgCatalogSchema.Tables["pg_constraint"]); }
    }

    protected SqlTableRef PgType
    {
      get { return SqlDml.TableRef(PgCatalogSchema.Tables["pg_type"]); }
    }

    protected SqlTableRef PgDepend
    {
      get { return SqlDml.TableRef(PgCatalogSchema.Tables["pg_depend"]); }
    }

    #endregion

    protected void ExtractUsers()
    {
      mUserLookup.Clear();
      string me;
      using (var command = Connection.CreateCommand("SELECT user"))
        me = (string) command.ExecuteScalar();
      using (DbCommand cmd = Connection.CreateCommand("SELECT usename, usesysid FROM pg_user")) {
        using (DbDataReader dr = cmd.ExecuteReader()) {
          while (dr.Read()) {
            string name = dr[0].ToString();
            int sysid = Convert.ToInt32(dr[1]);
            mUserLookup.Add(sysid, name);
            if (name==me)
              mUserSysId = sysid;
          }
        }
      }
    }

    /// <summary>
    /// Extracts the current user's schemas in the specified catalog.
    /// </summary>
    /// <param name="catalog"></param>
    protected void ExtractSchemas(Catalog catalog)
    {
      Func<IEnumerable<int>, SqlRow> oidRowCreator = oids => {
        SqlRow row = SqlDml.Row();
        foreach (int oid in oids) {
          row.Add(oid);
        }
        //make sure it is not empty, so that "IN" expression always works
        //add an invalid OID value 
        if (row.Count==0)
          row.Add(-1000);
        return row;
      };

      int me = GetMyUserSysId();

      //schemas

      #region Schemas

      var schemas = new Dictionary<int, Schema>();
      {
        SqlTableRef nsp1 = PgNamespace;
        SqlTableRef nsp2 = PgNamespace;
        SqlSelect q1 = SqlDml.Select(nsp1);
        q1.Where = nsp1["nspname"]=="public"
          && nsp1["nspowner"]!=me;
        q1.Columns.Add(nsp1["nspname"]);
        q1.Columns.Add(nsp1["oid"]);
        q1.Columns.Add(nsp1["nspowner"]);
        if (schema!=null)
          q1.Where &= nsp1["nspname"]==schema.Name;
        SqlSelect q2 = SqlDml.Select(nsp2);
        q2.Where = nsp2["nspowner"]==me;
        q2.Columns.Add(nsp2["nspname"]);
        q2.Columns.Add(nsp2["oid"]);
        q2.Columns.Add(nsp2["nspowner"]);
        if (schema!=null)
          q2.Where &= nsp2["nspname"]==schema.Name;
        ISqlCompileUnit q = q1.UnionAll(q2);
        using (var cmd = Connection.CreateCommand(q))
        using (DbDataReader dr = cmd.ExecuteReader()) {
          while (dr.Read()) {
            int oid = Convert.ToInt32(dr["oid"]);
            string name = dr["nspname"].ToString();
            int owner = Convert.ToInt32(dr["nspowner"]);
            Schema sch;
            if (catalog.Schemas[name]==null) {
              sch = catalog.CreateSchema(name);
              if (name=="public")
                catalog.DefaultSchema = sch;
            }
            else
              sch = catalog.Schemas[name];
            sch.Owner = mUserLookup[owner];
            schemas[oid] = sch;
          }
        }
      }

      #endregion

      //tables,views,sequences

      #region Tables, views, sequences

      var tables = new Dictionary<int, Table>();
      var views = new Dictionary<int, View>();
      var sequences = new Dictionary<int, Sequence>();
      var expressionIndexes = new Dictionary<int, ExpressionIndexInfo>();

      if (schemas.Count > 0) {
        SqlTableRef rel = PgClass;
        SqlTableRef spc = PgTablespace;
        SqlSelect q = SqlDml.Select(rel.LeftOuterJoin(spc, spc["oid"]==rel["reltablespace"]));
        q.Where = rel["relowner"]==me && SqlDml.In(rel["relkind"], SqlDml.Row('r', 'v', 'S')) && SqlDml.In(rel["relnamespace"], oidRowCreator(schemas.Keys));
        q.Columns.Add(rel["oid"], "reloid");
        q.Columns.Add(rel["relname"]);
        q.Columns.Add(rel["relkind"]);
        q.Columns.Add(rel["relnamespace"]);
        q.Columns.Add(spc["spcname"]);
        q.Columns.Add(new Func<SqlCase>(() => {
          SqlCase defCase = SqlDml.Case(rel["relkind"]);
          defCase.Add('v', SqlDml.FunctionCall("pg_get_viewdef", rel["oid"]));
          return defCase;
        })(), "definition");

        using (var cmd = Connection.CreateCommand(q))
        using (DbDataReader dr = cmd.ExecuteReader()) {
          while (dr.Read()) {
            int reloid = Convert.ToInt32(dr["reloid"]);
            string relkind = dr["relkind"].ToString();
            string relname = dr["relname"].ToString();
            int relnamespace = Convert.ToInt32(dr["relnamespace"]);
            Schema sch = schemas[relnamespace];
            Debug.Assert(sch!=null);
            if (relkind=="r") {
              Table t = sch.CreateTable(relname);
              object spcname = dr["spcname"];
              if (spcname!=DBNull.Value && spcname!=null)
                t.Filegroup = Convert.ToString(spcname);
              tables.Add(reloid, t);
            }
            else if (relkind=="v") {
              string def = dr["definition"].ToString();
              View v = sch.CreateView(relname, SqlDml.Native(def), CheckOptions.None);
              views.Add(reloid, v);
            }
            else if (relkind=="S") {
              Sequence s = sch.CreateSequence(relname);
              s.DataType = new SqlValueType(SqlType.Int64);
              sequences.Add(reloid, s);
            }
          }
        }
      }

      #endregion

      //table, view columns

      #region Table and view columns

      var tableColumns = new Dictionary<int, Dictionary<int, TableColumn>>();
      if (tables.Count > 0 || views.Count > 0) {
        SqlTableRef att = PgAttribute;
        SqlTableRef ad = PgAttrDef;
        SqlTableRef typ = PgType;
        SqlSelect q = SqlDml.Select(att
          .LeftOuterJoin(ad, att["attrelid"]==ad["adrelid"] && att["attnum"]==ad["adnum"])
          .InnerJoin(typ, typ["oid"]==att["atttypid"]));
        q.Where = att["attisdropped"]==false && att["attnum"] > 0 && (SqlDml.In(att["attrelid"], oidRowCreator(tables.Keys)) || SqlDml.In(att["attrelid"], oidRowCreator(views.Keys)));
        q.Columns.Add(att["attrelid"]);
        q.Columns.Add(att["attnum"]);
        q.Columns.Add(att["attname"]);
        q.Columns.Add(typ["typname"]);
        q.Columns.Add(att["atttypmod"]);
        q.Columns.Add(att["attnotnull"]);
        q.Columns.Add(att["atthasdef"]);
        q.Columns.Add(ad["adsrc"]);
        q.OrderBy.Add(att["attrelid"]);
        q.OrderBy.Add(att["attnum"]);

        using (var cmd = Connection.CreateCommand(q))
        using (DbDataReader dr = cmd.ExecuteReader()) {
          while (dr.Read()) {
            int attrelid = Convert.ToInt32(dr["attrelid"]);
            int attnum = Convert.ToInt32(dr["attnum"]);
            string attname = dr["attname"].ToString();
            if (tables.ContainsKey(attrelid)) {
              Table t = tables[attrelid];
              Debug.Assert(t!=null);
              TableColumn col = t.CreateColumn(attname);
              if (!tableColumns.ContainsKey(attrelid)) {
                tableColumns.Add(attrelid, new Dictionary<int, TableColumn>());
              }
              tableColumns[attrelid].Add(attnum, col);

              string typname = dr["typname"].ToString();
              int atttypmod = Convert.ToInt32(dr["atttypmod"]);
              bool attnotnull = dr.GetBoolean(dr.GetOrdinal("attnotnull"));
              bool atthasdef = dr.GetBoolean(dr.GetOrdinal("atthasdef"));
              if (atthasdef) {
                string def = dr["adsrc"].ToString();
                col.DefaultValue = SqlDml.Native(def);
              }
              col.IsNullable = !attnotnull;

              col.DataType = GetSqlValueType(typname, atttypmod);
            }
            else {
              View v = views[attrelid];
              Debug.Assert(v!=null);
              v.CreateColumn(attname);
            }
          }
        }
      }

      #endregion

      //table indexes

      #region Table indexes

      if (tables.Count > 0) {
        SqlTableRef spc = PgTablespace;
        SqlTableRef rel = PgClass;
        SqlTableRef ind = PgIndex;
        SqlTableRef depend = PgDepend;

        //subselect that index was not created automatically
        SqlSelect subSelect = SqlDml.Select(depend);
        subSelect.Where = depend["classid"]==PgClassOid && depend["objid"]==ind["indexrelid"] && depend["deptype"]=='i';
        subSelect.Columns.Add(depend[0]);

        //not automatically created indexes of our tables
        SqlSelect q = SqlDml.Select(ind
          .InnerJoin(rel, rel["oid"]==ind["indexrelid"])
          .LeftOuterJoin(spc, spc["oid"]==rel["reltablespace"]));
        q.Where = SqlDml.In(ind["indrelid"], oidRowCreator(tables.Keys)) && !SqlDml.Exists(subSelect);
        q.Columns.Add(ind["indrelid"]);
        q.Columns.Add(ind["indexrelid"]);
        q.Columns.Add(rel["relname"]);
        q.Columns.Add(ind["indisunique"]);
        q.Columns.Add(ind["indisclustered"]);
        q.Columns.Add(ind["indkey"]);
        q.Columns.Add(spc["spcname"]);
        q.Columns.Add(ind["indnatts"]);
        q.Columns.Add(ind["indexprs"]);
        q.Columns.Add(SqlDml.FunctionCall("pg_get_expr", ind["indpred"], ind["indrelid"], true), "indpredtext");
        q.Columns.Add(SqlDml.FunctionCall("pg_get_indexdef", ind["indexrelid"]), "inddef");
        AddSpecialIndexQueryColumns(q, spc, rel, ind, depend);

        int maxColumnNumber = 0;
        using (var cmd = Connection.CreateCommand(q))
        using (DbDataReader dr = cmd.ExecuteReader()) {
          while (dr.Read()) {
            int tableOid = Convert.ToInt32(dr["indrelid"]);
            int indexOid = Convert.ToInt32(dr["indexrelid"]);
            string indexName = dr["relname"].ToString();
            bool isUnique = dr.GetBoolean(dr.GetOrdinal("indisunique"));
            bool isClustered = dr.GetBoolean(dr.GetOrdinal("indisclustered"));
            string indKey = dr["indkey"].ToString();
            string tablespaceName = null;
            if (dr["spcname"]!=DBNull.Value)
              tablespaceName = dr["spcname"].ToString();
            string filterExpression = string.Empty;
            if (dr["indpredtext"]!=DBNull.Value)
              filterExpression = dr["indpredtext"].ToString();

            Table t = tables[tableOid];

            string fullTextRegex = @"(?<=CREATE INDEX \S+ ON \S+ USING (?:gist|gin)(?:\s|\S)*)to_tsvector\('(\w+)'::regconfig, \(*(?:(?:\s|\)|\(|\|)*(?:\(""(\w+)""\)|'\s')::text)+\)";
            string indexScript = dr["inddef"].ToString();
            var matches = Regex.Matches(indexScript, fullTextRegex, RegexOptions.Compiled);
            if (matches.Count > 0) {
              // Fulltext index
              var fullTextIndex = t.CreateFullTextIndex(indexName);
              foreach (Match match in matches) {
                var columnConfigurationName = match.Groups[1].Value;
                foreach (Capture capture in match.Groups[2].Captures) {
                  var columnName = capture.Value;
                  IndexColumn fullTextColumn = fullTextIndex.Columns[columnName] ?? fullTextIndex.CreateIndexColumn(t.Columns.Single(column => column.Name==columnName));
                  if (fullTextColumn.Languages[columnConfigurationName]==null) 
                    fullTextColumn.Languages.Add(new Language(columnConfigurationName));
                }
              }
            }
            else {
              //Regular index
              Index i = t.CreateIndex(indexName);
              i.IsBitmap = false;
              i.IsClustered = isClustered;
              i.IsUnique = isUnique;
              i.Filegroup = tablespaceName;
              if (!string.IsNullOrEmpty(filterExpression))
                i.Where = SqlDml.Native(filterExpression);

              // Expression-based index
              if (dr["indexprs"]!=DBNull.Value) {
                expressionIndexes[indexOid] = new ExpressionIndexInfo(i, indKey);
                int columnNumber = dr.GetInt16(dr.GetOrdinal("indnatts"));
                if (columnNumber > maxColumnNumber)
                  maxColumnNumber = columnNumber;
              }
              else {
                //index columns
                string[] indKeyArray = indKey.Split(' ');
                for (int j = 0; j < indKeyArray.Length; j++) {
                  int colIndex = Int32.Parse(indKeyArray[j]);
                  if (colIndex > 0) {
                    i.CreateIndexColumn(tableColumns[tableOid][colIndex], true);
                  }
                  else {
                    int z = 7;
                    //column index is 0
                    //this means that this index column is an expression
                    //which is not possible with SqlDom tables
                  }
                }
              }
              ReadSpecialIndexProperties(dr, i);
            }
          }
        }

        if (expressionIndexes.Count > 0) {
          q = SqlDml.Select(ind);
          q.Columns.Add(ind["indrelid"]);
          q.Columns.Add(ind["indexrelid"]);
          for (int i = 1; i <= maxColumnNumber; i++)
            q.Columns.Add(SqlDml.FunctionCall("pg_get_indexdef", ind["indexrelid"], i, true), i.ToString());
          q.Where = SqlDml.In(ind["indexrelid"], SqlDml.Array(expressionIndexes.Keys.ToArray()));
          using (var cmd = Connection.CreateCommand(q))
          using (DbDataReader dr = cmd.ExecuteReader()) {
            while (dr.Read()) {
              var exprIndexInfo = expressionIndexes[Convert.ToInt32(dr.GetInt64(1))];
              string[] indKeyArray = exprIndexInfo.Columns.Split(' ');
              for (int j = 0; j < indKeyArray.Length; j++) {
                int colIndex = Int32.Parse(indKeyArray[j]);
                if (colIndex > 0) {
                  exprIndexInfo.Index.CreateIndexColumn(tableColumns[Convert.ToInt32(dr.GetInt64(0))][colIndex], true);
                }
                else {
                  exprIndexInfo.Index.CreateIndexColumn(SqlDml.Native(dr[(j + 1).ToString()].ToString()));
                }
              }
            }
          }
        }
      }

      #endregion

      //domains

      #region Domains

      var domains = new Dictionary<int, Domain>();
      if (schemas.Count > 0) {
        SqlTableRef typ = PgType;
        SqlTableRef basetyp = PgType;
        SqlSelect q = SqlDml.Select(typ.InnerJoin(basetyp, basetyp["oid"]==typ["typbasetype"]));
        q.Where = typ["typisdefined"]==true && typ["typtype"]=='d'
          && SqlDml.In(typ["typnamespace"], oidRowCreator(schemas.Keys))
            && typ["typowner"]==me;
        q.Columns.Add(typ["oid"]);
        q.Columns.Add(typ["typname"], "typname");
        q.Columns.Add(typ["typnamespace"], "typnamespace");
        q.Columns.Add(typ["typtypmod"], "typmod");
        q.Columns.Add(typ["typdefault"], "default");
        q.Columns.Add(basetyp["typname"], "basetypname");

        using (var cmd = Connection.CreateCommand(q))
        using (DbDataReader dr = cmd.ExecuteReader()) {
          while (dr.Read()) {
            int oid = Convert.ToInt32(dr["oid"]);
            int typnamespace = Convert.ToInt32(dr["typnamespace"]);
            string typname = dr["typname"].ToString();
            string basetypname = dr["basetypname"].ToString();
            int typmod = Convert.ToInt32(dr["typmod"]);
            string defaultValue = null;
            if (dr["default"]!=DBNull.Value)
              defaultValue = dr["default"].ToString();

            Schema sch = schemas[typnamespace];
            Domain d = sch.CreateDomain(typname, GetSqlValueType(basetypname, typmod));
            if (defaultValue==null)
              d.DefaultValue = SqlDml.Null;
            else
              d.DefaultValue = SqlDml.Native(defaultValue);
            domains.Add(oid, d);
          }
        }
      }

      #endregion

      //table and domain constraints

      #region Table and domain constraints (check, unique, primary, foreign key)

      if (tables.Count > 0 || domains.Count > 0) {
        SqlTableRef con = PgConstraint;
        SqlSelect q = SqlDml.Select(con);
        q.Where = SqlDml.In(con["conrelid"], oidRowCreator(tables.Keys))
          || SqlDml.In(con["contypid"], oidRowCreator(domains.Keys));
        q.Columns.AddRange(con["conname"], con["contype"], con["condeferrable"],
          con["condeferred"], con["conrelid"], con["contypid"], con["conkey"], con["consrc"],
          con["confrelid"], con["confkey"], con["confupdtype"],
          con["confdeltype"], con["confmatchtype"]);

        using (var cmd = Connection.CreateCommand(q))
        using (DbDataReader dr = cmd.ExecuteReader()) {
          while (dr.Read()) {
            char contype = dr["contype"].ToString()[0];
            string conname = dr["conname"].ToString();
            bool condeferrable = dr.GetBoolean(dr.GetOrdinal("condeferrable"));
            bool condeferred = dr.GetBoolean(dr.GetOrdinal("condeferred"));
            int conrelid = Convert.ToInt32(dr["conrelid"]);
            int contypid = Convert.ToInt32(dr["contypid"]);
            object conkey = dr["conkey"];

            if (conrelid!=0) //table constraint
            {
              Table t = tables[conrelid];
              if (contype=='c') //check
              {
                string consrc = dr["consrc"].ToString();
                CheckConstraint c = t.CreateCheckConstraint(conname, SqlDml.Native(consrc));
                c.IsDeferrable = condeferrable;
                c.IsInitiallyDeferred = condeferred;
              }
              else {
                Dictionary<int, TableColumn> keyColumns = tableColumns[conrelid];
                if (contype=='u' || contype=='p') //unique / primary key
                {
                  UniqueConstraint c;
                  if (contype=='u')
                    c = t.CreateUniqueConstraint(conname);
                  else
                    c = t.CreatePrimaryKey(conname);

                  c.IsDeferrable = condeferrable;
                  c.IsInitiallyDeferred = condeferred;
                  int[] colIndexes = ReadIntArray(conkey);
                  for (int i = 0; i < colIndexes.Length; i++) {
                    c.Columns.Add(keyColumns[colIndexes[i]]);
                  }
                }
                else if (contype=='f') //foreign key
                {
                  object confkey = dr["confkey"];
                  int confrelid = Convert.ToInt32(dr["confrelid"]);
                  char confupdtype = dr["confupdtype"].ToString()[0];
                  char confdeltype = dr["confdeltype"].ToString()[0];
                  char confmatchtype = dr["confmatchtype"].ToString()[0];

                  ForeignKey fk = t.CreateForeignKey(conname);
                  fk.IsDeferrable = condeferrable;
                  fk.IsInitiallyDeferred = condeferred;
                  fk.OnDelete = GetReferentialAction(confdeltype);
                  fk.OnUpdate = GetReferentialAction(confupdtype);
                  fk.MatchType = GetMatchType(confmatchtype);
                  fk.ReferencedTable = tables[confrelid];

                  Dictionary<int, TableColumn> fkeyColumns = tableColumns[confrelid];

                  int[] colIndexes = ReadIntArray(conkey);
                  for (int i = 0; i < colIndexes.Length; i++) {
                    fk.Columns.Add(keyColumns[colIndexes[i]]);
                  }
                  colIndexes = ReadIntArray(confkey);
                  for (int i = 0; i < colIndexes.Length; i++) {
                    fk.ReferencedColumns.Add(fkeyColumns[colIndexes[i]]);
                  }
                }
              }
            }
            else if (contypid!=0) //domain constraint
            {
              if (contype=='c') //check
              {
                string consrc = dr["consrc"].ToString();
                Domain d = domains[contypid];
                DomainConstraint c = d.CreateConstraint(conname, SqlDml.Native(consrc));
                c.IsDeferrable = condeferrable;
                c.IsInitiallyDeferred = condeferred;
              }
            }
          }
        }
      }

      #endregion

      //sequence infos

      #region Sequence info

      if (sequences.Count > 0) {
        //Have to do it traditional string concat because cannot select from 
        //a sequence with Sql.Dom
        var query = new StringBuilder();
        {
          Sequence[] seqArray = new Sequence[sequences.Count];
          sequences.Values.CopyTo(seqArray, 0);
          Sequence seq = seqArray[0];
          query.AppendFormat("SELECT * FROM (\nSELECT {0} as id, * FROM {1}", 0, Driver.Translator.Translate(seq));
          for (int i = 1; i < sequences.Count; i++) {
            seq = seqArray[i];
            query.AppendFormat("\nUNION ALL\nSELECT {0} as id, * FROM {1}", i, Driver.Translator.Translate(seq));
          }
          query.Append("\n) all_sequences\nORDER BY id");
        }

        using (DbCommand cmd = Connection.UnderlyingConnection.CreateCommand()) {
          cmd.CommandText = query.ToString();
          using (DbDataReader dr = cmd.ExecuteReader()) {
            foreach (Sequence seq in sequences.Values) {
              dr.Read();
              ReadSequenceDescriptor(dr, seq.SequenceDescriptor);
            }
          }
        }
      }

      #endregion
    }

    protected void ExtractStoredProcedures(Schema schema)
    {
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
      // descriptor.IsCyclic = reader.GetBoolean(reader.GetOrdinal("is_cycled"));
      descriptor.IsCyclic = Convert.ToBoolean(reader["is_cycled"]);
      descriptor.MinValue = Convert.ToInt64(reader["min_value"]);
      descriptor.MaxValue = Convert.ToInt64(reader["max_value"]);
      descriptor.StartValue = Convert.ToInt64(reader["min_value"]);
    }

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
    protected int GetMyUserSysId()
    {
      if (mUserSysId < 0)
        using (var cmd = Connection.CreateCommand("SELECT usesysid FROM pg_user WHERE usename = user"))
          mUserSysId = Convert.ToInt32(cmd.ExecuteScalar());
      return mUserSysId;
    }

    protected virtual string QuoteIdentifier(params string[] names)
    {
      string[] names2 = new string[names.Length];
      for (int i = 0; i < names.Length; i++) {
        names2[i] = names[i].Replace("\"", "\"\"");
      }
      return ("\"" + string.Join("\".\"", names2) + "\"");
    }

    // Constructor

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}