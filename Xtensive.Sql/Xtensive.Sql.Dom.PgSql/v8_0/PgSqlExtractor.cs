using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using Npgsql;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Extractor;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.PgSql.v8_0
{
  public class PgSqlExtractor : SqlExtractor
  {
    public PgSqlExtractor(PgSqlDriver driver)
      : base(driver)
    {
    }


    public virtual string QuoteIdentifier(params string[] names)
    {
      string[] names2 = new string[names.Length];
      for (int i = 0; i < names.Length; i++) {
        names2[i] = names[i].Replace("\"", "\"\"");
      }
      return ("\"" + string.Join("\".\"", names2) + "\"");
    }

    public override void Initialize(SqlExtractorContext context)
    {
      CreateCatalogModel();

      //Query OID of some system catalog tables for using them in pg_depend lookups
      {
        SqlTableRef rel = PgClass;
        SqlSelect q = Sql.Select(rel);
        q.Where = Sql.In(rel["relname"], Sql.Row("pg_class"));
        q.Columns.Add(rel["oid"]);
        q.Columns.Add(rel["relname"]);

        using (SqlCommand cmd = new SqlCommand(context.Connection)) {
          cmd.Transaction = context.Transaction;
          cmd.Statement = q;
          using (DbDataReader dr = cmd.ExecuteReader()) {
            while (dr.Read()) {
              int oid = Convert.ToInt32(dr[0]);
              string name = dr.GetString(1);
              if (name=="pg_class") {
                mPgClassOid = oid;
              }
            }
          }
        }
      }
    }

    protected void CreateCatalogModel()
    {
      mPgCatalogModel = new Model("catalog");
      PgCatalogModel.CreateServer("default");
      PgCatalogModel.DefaultServer.CreateCatalog("default");
      Schema sch = PgCatalogModel.DefaultServer.DefaultCatalog.CreateSchema("pg_catalog");
      mPgCatalogSchema = sch;
      BuildPgCatalogSchema(sch);
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
      t.CreateColumn("oid", new SqlValueType(SqlDataType.Int32));
    }

    protected void CreateInt2Column(Table t, string name)
    {
      t.CreateColumn(name, new SqlValueType(SqlDataType.Int16));
    }

    protected void CreateInt4Column(Table t, string name)
    {
      t.CreateColumn(name, new SqlValueType(SqlDataType.Int32));
    }

    protected void CreateChar1Column(Table t, string name)
    {
      t.CreateColumn(name, new SqlValueType(SqlDataType.Char, 1));
    }

    protected void CreateTextColumn(Table t, string name)
    {
      t.CreateColumn(name, new SqlValueType(SqlDataType.Text));
    }

    protected void CreateBoolColumn(Table t, string name)
    {
      t.CreateColumn(name, new SqlValueType(SqlDataType.Boolean));
    }

    #endregion

    #region Table reference creator properties

    protected SqlTableRef PgUser
    {
      get { return Sql.TableRef(PgCatalogSchema.Tables["pg_user"]); }
    }

    protected SqlTableRef PgTablespace
    {
      get { return Sql.TableRef(PgCatalogSchema.Tables["pg_tablespace"]); }
    }

    protected SqlTableRef PgNamespace
    {
      get { return Sql.TableRef(PgCatalogSchema.Tables["pg_namespace"]); }
    }

    protected SqlTableRef PgClass
    {
      get { return Sql.TableRef(PgCatalogSchema.Tables["pg_class"]); }
    }

    protected SqlTableRef PgIndex
    {
      get { return Sql.TableRef(PgCatalogSchema.Tables["pg_index"]); }
    }

    protected SqlTableRef PgAttribute
    {
      get { return Sql.TableRef(PgCatalogSchema.Tables["pg_attribute"]); }
    }

    protected SqlTableRef PgAttrDef
    {
      get { return Sql.TableRef(PgCatalogSchema.Tables["pg_attrdef"]); }
    }

    protected SqlTableRef PgViews
    {
      get { return Sql.TableRef(PgCatalogSchema.Tables["pg_views"]); }
    }

    protected SqlTableRef PgConstraint
    {
      get { return Sql.TableRef(PgCatalogSchema.Tables["pg_constraint"]); }
    }

    protected SqlTableRef PgType
    {
      get { return Sql.TableRef(PgCatalogSchema.Tables["pg_type"]); }
    }

    protected SqlTableRef PgDepend
    {
      get { return Sql.TableRef(PgCatalogSchema.Tables["pg_depend"]); }
    }

    #endregion

    #region PgClassOid property

    private int mPgClassOid;

    protected int PgClassOid
    {
      get { return mPgClassOid; }
    }

    #endregion

    public override void ExtractUsers(SqlExtractorContext context, Server server)
    {
      mUserLookup.Clear();
      string me = context.Connection.ConnectionInfo.User;
      using (DbCommand cmd = context.Connection.RealConnection.CreateCommand()) {
        cmd.CommandText = "SELECT usename, usesysid FROM pg_user";
        using (DbDataReader dr = cmd.ExecuteReader()) {
          while (dr.Read()) {
            string name = dr[0].ToString();
            int sysid = Convert.ToInt32(dr[1]);
            User u = server.CreateUser(name);
            mUserLookup.Add(sysid, u);
            if (name==me)
              mUserSysId = sysid;
          }
        }
      }
    }

    private Dictionary<int, User> mUserLookup = new Dictionary<int, User>();

    /// <summary>
    /// Extracts the current user's schemas in the specified catalog.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="catalog"></param>
    public override void ExtractSchemas(SqlExtractorContext context, Catalog catalog)
    {
      Common.PgSql.Func<IEnumerable<int>, SqlRow> oidRowCreator = delegate(IEnumerable<int> oids) {
        SqlRow row = Sql.Row();
        foreach (int oid in oids) {
          row.Add(oid);
        }
        //make sure it is not empty, so that "IN" expression always works
        //add an invalid OID value 
        if (row.Count==0)
          row.Add(-1000);
        return row;
      };

      int me = GetMyUserSysId(context);

      //schemas

      #region Schemas

      Dictionary<int, Schema> schemas = new Dictionary<int, Schema>();
      {
        SqlTableRef nsp1 = PgNamespace;
        SqlTableRef nsp2 = PgNamespace;
        SqlSelect q1 = Sql.Select(nsp1);
        q1.Where = nsp1["nspname"]=="public"
          && nsp1["nspowner"]!=me;
        q1.Columns.Add(nsp1["nspname"]);
        q1.Columns.Add(nsp1["oid"]);
        q1.Columns.Add(nsp1["nspowner"]);
        SqlSelect q2 = Sql.Select(nsp2);
        q2.Where = nsp2["nspowner"]==me;
        q2.Columns.Add(nsp2["nspname"]);
        q2.Columns.Add(nsp2["oid"]);
        q2.Columns.Add(nsp2["nspowner"]);
        ISqlCompileUnit q = q1.UnionAll(q2);
        using (SqlCommand cmd = new SqlCommand(context.Connection)) {
          cmd.Statement = q;

          using (DbDataReader dr = cmd.ExecuteReader()) {
            while (dr.Read()) {
              int oid = Convert.ToInt32(dr["oid"]);
              string name = dr["nspname"].ToString();
              int owner = Convert.ToInt32(dr["nspowner"]);
              Schema sch = catalog.CreateSchema(name);
              schemas.Add(oid, sch);
              sch.Owner = mUserLookup[owner];
            }
          }
        }
      }

      #endregion

      //tables,views,sequences

      #region Tables, views, sequences

      Dictionary<int, Table> tables = new Dictionary<int, Table>();
      Dictionary<int, View> views = new Dictionary<int, View>();
      Dictionary<int, Sequence> sequences = new Dictionary<int, Sequence>();
      if (schemas.Count > 0) {
        SqlTableRef rel = PgClass;
        SqlTableRef spc = PgTablespace;
        SqlSelect q = Sql.Select(rel.LeftOuterJoin(spc, spc["oid"]==rel["reltablespace"]));
        q.Where = rel["relowner"]==me && Sql.In(rel["relkind"], Sql.Row('r', 'v', 'S')) && Sql.In(rel["relnamespace"], oidRowCreator(schemas.Keys));
        q.Columns.Add(rel["oid"], "reloid");
        q.Columns.Add(rel["relname"]);
        q.Columns.Add(rel["relkind"]);
        q.Columns.Add(rel["relnamespace"]);
        q.Columns.Add(spc["spcname"]);
        q.Columns.Add(new Common.PgSql.Func<SqlCase>(delegate {
          SqlCase defCase = Sql.Case(rel["relkind"]);
          defCase.Add('v', Sql.FunctionCall("pg_get_viewdef", rel["oid"]));
          return defCase;
        })(), "definition");

        using (SqlCommand cmd = new SqlCommand(context.Connection)) {
          cmd.Statement = q;
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
                View v = sch.CreateView(relname, Sql.Constant(def), CheckOptions.None);
                views.Add(reloid, v);
              }
              else if (relkind=="S") {
                Sequence s = sch.CreateSequence(relname);
                s.DataType = new SqlValueType(SqlDataType.Int64);
                sequences.Add(reloid, s);
              }
            }
          }
        }
      }

      #endregion

      //table, view columns

      #region Table and view columns

      Dictionary<int, Dictionary<int, TableColumn>> tableColumns = new Dictionary<int, Dictionary<int, TableColumn>>();
      if (tables.Count > 0 || views.Count > 0) {
        SqlTableRef att = PgAttribute;
        SqlTableRef ad = PgAttrDef;
        SqlTableRef typ = PgType;
        SqlSelect q = Sql.Select(att
          .LeftOuterJoin(ad, att["attrelid"]==ad["adrelid"] && att["attnum"]==ad["adnum"])
          .InnerJoin(typ, typ["oid"]==att["atttypid"]));
        q.Where = att["attisdropped"]==false && att["attnum"] > 0 && (Sql.In(att["attrelid"], oidRowCreator(tables.Keys)) || Sql.In(att["attrelid"], oidRowCreator(views.Keys)));
        q.Columns.Add(att["attrelid"]);
        q.Columns.Add(att["attnum"]);
        q.Columns.Add(att["attname"]);
        q.Columns.Add(typ["typname"]);
        q.Columns.Add(att["atttypmod"]);
        q.Columns.Add(att["attnotnull"]);
        q.Columns.Add(att["atthasdef"]);
        q.Columns.Add(ad["adsrc"]);

        using (SqlCommand cmd = new SqlCommand(context.Connection)) {
          cmd.Statement = q;
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
                  col.DefaultValue = Sql.Constant(def);
                }
                col.IsNullable = !attnotnull;

                col.DataType = GetSqlValueType(context, typname, atttypmod);
              }
              else {
                View v = views[attrelid];
                Debug.Assert(v!=null);
                v.CreateColumn(attname);
              }
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
        SqlSelect subSelect = Sql.Select(depend);
        subSelect.Where = depend["classid"]==PgClassOid && depend["objid"]==ind["indexrelid"] && depend["deptype"]=='i';
        subSelect.Columns.Add(depend[0]);

        //not automatically created indexes of our tables
        SqlSelect q = Sql.Select(ind
          .InnerJoin(rel, rel["oid"]==ind["indexrelid"])
          .LeftOuterJoin(spc, spc["oid"]==rel["reltablespace"]));
        q.Where = Sql.In(ind["indrelid"], oidRowCreator(tables.Keys)) && !Sql.Exists(subSelect);
        q.Columns.Add(ind["indrelid"]);
        q.Columns.Add(ind["indexrelid"]);
        q.Columns.Add(rel["relname"]);
        q.Columns.Add(ind["indisunique"]);
        q.Columns.Add(ind["indisclustered"]);
        q.Columns.Add(ind["indkey"]);
        q.Columns.Add(spc["spcname"]);
        AddSpecialIndexQueryColumns(q, spc, rel, ind, depend);

        using (SqlCommand cmd = new SqlCommand(context.Connection)) {
          cmd.Statement = q;
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

              Table t = tables[tableOid];
              Index i = t.CreateIndex(indexName);
              i.IsBitmap = false;
              i.IsClustered = isClustered;
              i.IsUnique = isUnique;
              i.Filegroup = tablespaceName;

              ReadSpecialIndexProperties(dr, i);

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
          }
        }
      }

      #endregion

      //domains

      #region Domains

      Dictionary<int, Domain> domains = new Dictionary<int, Domain>();
      if (schemas.Count > 0) {
        SqlTableRef typ = PgType;
        SqlTableRef basetyp = PgType;
        SqlSelect q = Sql.Select(typ.InnerJoin(basetyp, basetyp["oid"]==typ["typbasetype"]));
        q.Where = typ["typisdefined"]==true && typ["typtype"]=='d'
          && Sql.In(typ["typnamespace"], oidRowCreator(schemas.Keys))
            && typ["typowner"]==me;
        q.Columns.Add(typ["oid"]);
        q.Columns.Add(typ["typname"], "typname");
        q.Columns.Add(typ["typnamespace"], "typnamespace");
        q.Columns.Add(typ["typtypmod"], "typmod");
        q.Columns.Add(typ["typdefault"], "default");
        q.Columns.Add(basetyp["typname"], "basetypname");
        using (SqlCommand cmd = new SqlCommand(context.Connection)) {
          cmd.Statement = q;
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
              Domain d = sch.CreateDomain(typname, GetSqlValueType(context, typname, typmod));
              if (defaultValue==null)
                d.DefaultValue = Sql.Null;
              else
                d.DefaultValue = Sql.Constant(defaultValue);
              domains.Add(oid, d);
            }
          }
        }
      }

      #endregion

      //table and domain constraints

      #region Table and domain constraints (check, unique, primary, foreign key)

      if (tables.Count > 0 || domains.Count > 0) {
        SqlTableRef con = PgConstraint;
        SqlSelect q = Sql.Select(con);
        q.Where = Sql.In(con["conrelid"], oidRowCreator(tables.Keys))
          || Sql.In(con["contypid"], oidRowCreator(domains.Keys));
        q.Columns.AddRange(con["conname"], con["contype"], con["condeferrable"],
          con["condeferred"], con["conrelid"], con["contypid"], con["conkey"], con["consrc"],
          con["confrelid"], con["confkey"], con["confupdtype"],
          con["confdeltype"], con["confmatchtype"]);

        using (SqlCommand cmd = new SqlCommand(context.Connection)) {
          cmd.Statement = q;
          using (DbDataReader dr = cmd.ExecuteReader()) {
            while (dr.Read()) {
              char contype = dr["contype"].ToString()[0];
              string conname = dr["conname"].ToString();
              bool condeferrable = dr.GetBoolean(dr.GetOrdinal("condeferrable"));
              bool condeferred = dr.GetBoolean(dr.GetOrdinal("condeferred"));
              int conrelid = Convert.ToInt32(dr["conrelid"]);
              int contypid = Convert.ToInt32(dr["contypid"]);
              string conkey = dr["conkey"].ToString();

              if (conrelid!=0) //table constraint
              {
                Table t = tables[conrelid];
                if (contype=='c') //check
                {
                  string consrc = dr["consrc"].ToString();
                  CheckConstraint c = t.CreateCheckConstraint(conname, Sql.Constant(consrc));
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
                    int[] colIndexes = ArrayLiteral2IntArray(conkey);
                    for (int i = 0; i < colIndexes.Length; i++) {
                      c.Columns.Add(keyColumns[colIndexes[i]]);
                    }
                  }
                  else if (contype=='f') //foreign key
                  {
                    string confkey = dr["confkey"].ToString();
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

                    int[] colIndexes = ArrayLiteral2IntArray(conkey);
                    for (int i = 0; i < colIndexes.Length; i++) {
                      fk.Columns.Add(keyColumns[colIndexes[i]]);
                    }
                    colIndexes = ArrayLiteral2IntArray(confkey);
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
                  DomainConstraint c = d.CreateConstraint(conname, Sql.Constant(consrc));
                  c.IsDeferrable = condeferrable;
                  c.IsInitiallyDeferred = condeferred;
                }
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
        StringBuilder sb = new StringBuilder();
        {
          Sequence[] seqArray = new Sequence[sequences.Count];
          sequences.Values.CopyTo(seqArray, 0);
          Sequence seq = seqArray[0];
          sb.AppendFormat("SELECT * FROM {0}", Driver.Translator.Translate(seq));
          for (int i = 1; i < sequences.Count; i++) {
            seq = seqArray[i];
            sb.AppendFormat("\nUNION ALL SELECT * FROM {0}"
              , Driver.Translator.Translate(seq));
          }
        }

        using (DbCommand cmd = context.Connection.RealConnection.CreateCommand()) {
          cmd.CommandText = sb.ToString();
          using (DbDataReader dr = cmd.ExecuteReader()) {
            foreach (Sequence seq in sequences.Values) {
              dr.Read();
              seq.SequenceDescriptor.Increment = Convert.ToInt64(dr["increment_by"]);
              seq.SequenceDescriptor.IsCyclic = dr.GetBoolean(dr.GetOrdinal("is_cycled"));
              seq.SequenceDescriptor.MinValue = Convert.ToInt64(dr["min_value"]);
              seq.SequenceDescriptor.MaxValue = Convert.ToInt64(dr["max_value"]);
              seq.SequenceDescriptor.StartValue = Convert.ToInt64(dr["min_value"]);
            }
          }
        }
      }

      #endregion
    }

    public override void ExtractStoredProcedures(SqlExtractorContext context, Schema schema)
    {
      //TODO
      base.ExtractStoredProcedures(context, schema);
    }

    protected virtual void AddSpecialIndexQueryColumns(SqlSelect query, SqlTableRef spc, SqlTableRef rel, SqlTableRef ind, SqlTableRef depend)
    {
    }

    protected virtual void ReadSpecialIndexProperties(DbDataReader dr, Index i)
    {
    }

    protected SqlValueType GetSqlValueType(SqlExtractorContext context, string typname, int typmod)
    {
      SqlValueType result;
      DataTypeCollection dataTypes = context.Connection.Driver.ServerInfo.DataTypes;
      DataTypeInfo dti = dataTypes[typname];
      SqlDataType sqlDataType = (dti==null ? SqlDataType.Unknown : dti.SqlType);

      if (sqlDataType==SqlDataType.Decimal) {
        if (typmod!=-1) {
          short precision = 0;
          byte scale = 0;
          GetPrecisionAndScale(typmod, out precision, out scale);
          result = new SqlValueType(sqlDataType, precision, scale);
        }
        else {
          //in this case we cannot determine the actual precision and scale
          //it should be avoided
          result = new SqlValueType(sqlDataType);
        }
      }
      else if (typmod!=-1) {
        result = new SqlValueType(sqlDataType, typmod - 4);
      }
      else {
        result = new SqlValueType(sqlDataType);
      }
      return result;
    }

    protected void GetPrecisionAndScale(int typmod, out short precision, out byte scale)
    {
      //high word
      precision = (short) ((typmod - 4) >> 16);
      //low word
      scale = (byte) ((typmod - 4) & 0xffff);
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

    /// <summary>
    /// Converts array literal to integer array: "{2, 1, 10}" -> {2, 1, 10}
    /// </summary>
    /// <param name="arrayLiteral"></param>
    /// <returns></returns>
    private int[] ArrayLiteral2IntArray(string arrayLiteral)
    {
      arrayLiteral = arrayLiteral.Trim();
      arrayLiteral = arrayLiteral.Substring(1, arrayLiteral.Length - 2);
      string[] elements = arrayLiteral.Split(','); //{"2","1","10"}
      int[] indexes = new int[elements.Length];
      for (int i = 0; i < elements.Length; i++) {
        indexes[i] = Int32.Parse(elements[i]);
      }
      return indexes;
    }

    /// <summary>
    /// Converts an int[] to a value list in SQL
    /// </summary>
    /// <param name="ints"></param>
    /// <returns></returns>
    private string IntArray2ValueList(int[] ints)
    {
      StringBuilder sb = new StringBuilder("(");
      string separator = ",";
      foreach (int i in ints) {
        sb.Append(i + separator);
      }
      if (ints.Length > 0)
        sb.Length -= separator.Length;
      sb.Append(")");
      return sb.ToString();
    }

    /// <summary>
    /// Converts array literal to SQL value list: "{2, 1, 10}" -> "(2, 1, 10)"
    /// </summary>
    /// <param name="arrayLiteral">Array literal</param>
    /// <returns>Value list</returns>
    private string ArrayLiteral2ValueList(string arrayLiteral)
    {
      arrayLiteral = arrayLiteral.Trim();
      arrayLiteral = arrayLiteral.Substring(1, arrayLiteral.Length - 2);
      return "(" + arrayLiteral + ")"; //"(2,1,10)"
    }

    /// <summary>
    /// Gets and caches the inner identifier of the current database user.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected int GetMyUserSysId(SqlExtractorContext context)
    {
      if (mUserSysId < 0) {
        string user = context.Connection.ConnectionInfo.User;
        using (NpgsqlCommand cmd = context.Connection.RealConnection.CreateCommand() as NpgsqlCommand) {
          cmd.CommandText = @"SELECT usesysid FROM pg_user WHERE usename = @name";
          cmd.Parameters.Add("@name", user);
          mUserSysId = Convert.ToInt32(cmd.ExecuteScalar());
        }
      }
      return mUserSysId;
    }

    //The identifier of the current user
    private int mUserSysId = -1;

    private Model mPgCatalogModel;

    protected Model PgCatalogModel
    {
      get { return mPgCatalogModel; }
    }

    private Schema mPgCatalogSchema;

    protected Schema PgCatalogSchema
    {
      get { return mPgCatalogSchema; }
    }
  }
}