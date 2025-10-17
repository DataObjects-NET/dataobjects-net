// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.PostgreSql.v12_0
{
  internal class Extractor : v10_0.Extractor
  {
    /// <inheritdoc/>
    protected override void BuildPgCatalogSchema(Schema schema)
    {
      base.BuildPgCatalogSchema(schema);
      var defaultValuesTable = schema.Tables["pg_attrdef"];
      _ = defaultValuesTable.CreateColumn("adbin", new SqlValueType(SqlType.Binary));

      var indexesTable = schema.Tables["pg_index"];
      CreateInt2Column(indexesTable, "indnkeyatts");
    }

    /// <inheritdoc/>
    protected override ISqlCompileUnit BuildExtractTableAndViewColumnsQuery(ExtractionContext context)
    {
      var tableMap = context.TableMap;
      var viewMap = context.ViewMap;
      var columnsTable = PgAttribute;
      var defaultValuesTable = PgAttrDef;
      var typesTable = PgType;

      var select = SqlDml.Select(columnsTable
        .LeftOuterJoin(defaultValuesTable,
          columnsTable["attrelid"] == defaultValuesTable["adrelid"]
          && columnsTable["attnum"] == defaultValuesTable["adnum"])
        .InnerJoin(typesTable, typesTable["oid"] == columnsTable["atttypid"]));

      select.Where = columnsTable["attisdropped"] == false &&
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
      select.Columns.Add(SqlDml.ColumnRef(SqlDml.Column(
          SqlDml.FunctionCall("pg_get_expr", defaultValuesTable["adbin"], defaultValuesTable["adrelid"])), "adsrc"));
      select.OrderBy.Add(columnsTable["attrelid"]);
      select.OrderBy.Add(columnsTable["attnum"]);
      return select;
    }

    /// <inheritdoc/>
    protected override ISqlCompileUnit BuildExtractTableAndDomainConstraintsQuery(ExtractionContext context)
    {
      var tableMap = context.TableMap;
      var domainMap = context.DomainMap;
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
        SqlDml.ColumnRef(
          SqlDml.Column(SqlDml.FunctionCall("pg_get_constraintdef", constraintTable["oid"])), "consrc"),
        constraintTable["confrelid"],
        constraintTable["confkey"],
        constraintTable["confupdtype"],
        constraintTable["confdeltype"],
        constraintTable["confmatchtype"]);
      return select;
    }

    /// <inheritdoc />
    protected override ISqlCompileUnit BuildExtractTableIndexesQuery(ExtractionContext context)
    {
      var tableMap = context.TableMap;
      var tableSpacesTable = PgTablespace;
      var relationsTable = PgClass;
      var indexTable = PgIndex;
      var dependencyTable = PgDepend;

      //subselect that index was not created automatically
      var subSelect = SqlDml.Select(dependencyTable);
      subSelect.Where = dependencyTable["classid"] == PgClassOid &&
        dependencyTable["objid"] == indexTable["indexrelid"] &&
        dependencyTable["deptype"] == 'i';
      subSelect.Columns.Add(dependencyTable[0]);

      //not automatically created indexes of our tables
      var select = SqlDml.Select(indexTable
        .InnerJoin(relationsTable, relationsTable["oid"] == indexTable["indexrelid"])
        .LeftOuterJoin(tableSpacesTable, tableSpacesTable["oid"] == relationsTable["reltablespace"]));
      select.Where = SqlDml.In(indexTable["indrelid"], CreateOidRow(tableMap.Keys)) && !SqlDml.Exists(subSelect);
      select.Columns.Add(indexTable["indrelid"]);
      select.Columns.Add(indexTable["indexrelid"]);
      select.Columns.Add(relationsTable["relname"]);
      select.Columns.Add(indexTable["indisunique"]);
      select.Columns.Add(indexTable["indisclustered"]);
      select.Columns.Add(indexTable["indkey"]);
      select.Columns.Add(tableSpacesTable["spcname"]);
      select.Columns.Add(indexTable["indnatts"]);
      select.Columns.Add(indexTable["indnkeyatts"]);
      select.Columns.Add(SqlDml.FunctionCall("pg_get_expr", indexTable["indexprs"], indexTable["indrelid"], true),
        "indexprstext");
      select.Columns.Add(SqlDml.FunctionCall("pg_get_expr", indexTable["indpred"], indexTable["indrelid"], true),
        "indpredtext");
      select.Columns.Add(SqlDml.FunctionCall("pg_get_indexdef", indexTable["indexrelid"]), "inddef");
      AddSpecialIndexQueryColumns(select, tableSpacesTable, relationsTable, indexTable, dependencyTable);
      return select;
    }

    /// <inheritdoc />
    protected override int ReadTableIndexData(DbDataReader dataReader, ExtractionContext context)
    {
      var tableMap = context.TableMap;
      var tableColumns = context.TableColumnMap;

      var maxColumnNumber = 0;
      var tableIdentifier = Convert.ToInt64(dataReader["indrelid"]);
      var indexIdentifier = Convert.ToInt64(dataReader["indexrelid"]);
      var indexName = dataReader["relname"].ToString();
      var isUnique = dataReader.GetBoolean(dataReader.GetOrdinal("indisunique"));
      var indexKey = (short[]) dataReader["indkey"];

      var tablespaceName = (dataReader["spcname"] != DBNull.Value) ? dataReader["spcname"].ToString() : null;
      var filterExpression = (dataReader["indpredtext"] != DBNull.Value)
        ? dataReader["indpredtext"].ToString()
        : string.Empty;

      var table = tableMap[tableIdentifier];

      var fullTextRegex =
        @"(?<=CREATE INDEX \S+ ON \S+ USING (?:gist|gin)(?:\s|\S)*)to_tsvector\('(\w+)'::regconfig, \(*(?:(?:\s|\)|\(|\|)*(?:\(""(\S+)""\)|'\s')::text)+\)";
      var indexScript = dataReader["inddef"].ToString();
      var matches = Regex.Matches(indexScript, fullTextRegex, RegexOptions.Compiled);
      if (matches.Count > 0) {
        // Fulltext index
        var fullTextIndex = table.CreateFullTextIndex(indexName);
        foreach (Match match in matches) {
          var columnConfigurationName = match.Groups[1].Value;
          foreach (Capture capture in match.Groups[2].Captures) {
            var columnName = capture.Value;
            var fullTextColumn = fullTextIndex.Columns[columnName]
              ?? fullTextIndex.CreateIndexColumn(table.Columns.Single(column => column.Name == columnName));
            if (fullTextColumn.Languages[columnConfigurationName] == null)
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
        var some = dataReader["indexprstext"];
        if (some != DBNull.Value) {
          context.ExpressionIndexMap[indexIdentifier] = new ExpressionIndexInfo(index, indexKey);
          maxColumnNumber = dataReader.GetInt16(dataReader.GetOrdinal("indnatts"));
        }
        else {
          var keyColumnNumber = dataReader.GetInt16(dataReader.GetOrdinal("indnkeyatts"));
          for (int j = 0; j < indexKey.Length; j++) {
            if (j < keyColumnNumber) {
              int colIndex = indexKey[j];
              if (colIndex > 0) {
                _ = index.CreateIndexColumn(tableColumns[tableIdentifier][colIndex], true);
              }
              else {
                //column index is 0
                //this means that this index column is an expression
                //which is not possible with SqlDom tables
              }
            }
            else {
              int colIndex = indexKey[j];
              index.NonkeyColumns.Add(tableColumns[tableIdentifier][colIndex]);
            }
          }
        }

        ReadSpecialIndexProperties(dataReader, index);
      }

      return maxColumnNumber;
    }


    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}