// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.PostgreSql.v12_0
{
  internal class Extractor : v10_0.Extractor
  {
    /// <inheritdoc/>
    protected override void BuildPgCatalogSchema(Schema schema)
    {
      base.BuildPgCatalogSchema(schema);
      var dafaultValuesTable = schema.Tables["pg_attrdef"];
      dafaultValuesTable.CreateColumn("adbin", new SqlValueType(SqlType.Binary));
    }

    /// <inheritdoc/>
    protected override void ExtractTableAndViewColumns(ExtractionContext context)
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
        select.Columns.Add(SqlDml.ColumnRef(SqlDml.Column(
          SqlDml.FunctionCall("pg_get_expr", dafaultValuesTable["adbin"], dafaultValuesTable["adrelid"])),
          "adsrc"));
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
              view.CreateColumn(columnName);
            }
          }
        }
      }
    }

    /// <inheritdoc/>
    protected override void ExtractTableAndDomainConstraints(ExtractionContext context)
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
          SqlDml.ColumnRef(
            SqlDml.Column(SqlDml.FunctionCall("pg_get_constraintdef", constraintTable["oid"])), "consrc"),
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

    // Consructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}