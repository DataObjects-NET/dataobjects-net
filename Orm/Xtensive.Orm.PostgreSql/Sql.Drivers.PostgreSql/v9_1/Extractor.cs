// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.10.10

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xtensive.Arithmetic;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.PostgreSql.v9_1
{
  internal class Extractor : v9_0.Extractor
  {
    // Consructors

    protected override void ExtractTableIndexes(ExtractionContext context)
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
        select.Columns.Add(SqlDml.FunctionCall("pg_get_expr", indexTable["indexprs"], indexTable["indrelid"], true), "indexprstext");
        select.Columns.Add(SqlDml.FunctionCall("pg_get_expr", indexTable["indpred"], indexTable["indrelid"], true), "indpredtext");
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
            var indexKey = (short[]) dataReader["indkey"];

            var tablespaceName = (dataReader["spcname"]!=DBNull.Value) ? dataReader["spcname"].ToString() : (string) null;
            var filterExpression = (dataReader["indpredtext"]!=DBNull.Value) ? dataReader["indpredtext"].ToString() : string.Empty;

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
                  else {
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

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}