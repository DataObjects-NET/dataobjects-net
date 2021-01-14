// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.10.10

namespace Xtensive.Sql.Drivers.PostgreSql.v9_1
{
  internal class Extractor : v9_0.Extractor
  {
    /// <inheritdoc/>
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
      select.Columns.Add(SqlDml.FunctionCall("pg_get_expr", indexTable["indexprs"], indexTable["indrelid"], true),
        "indexprstext");
      select.Columns.Add(SqlDml.FunctionCall("pg_get_expr", indexTable["indpred"], indexTable["indrelid"], true),
        "indpredtext");
      select.Columns.Add(SqlDml.FunctionCall("pg_get_indexdef", indexTable["indexrelid"]), "inddef");
      AddSpecialIndexQueryColumns(select, tableSpacesTable, relationsTable, indexTable, dependencyTable);
      return select;
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
