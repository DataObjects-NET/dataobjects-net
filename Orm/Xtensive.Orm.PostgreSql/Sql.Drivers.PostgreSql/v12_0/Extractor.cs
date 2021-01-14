// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
      defaultValuesTable.CreateColumn("adbin", new SqlValueType(SqlType.Binary));
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

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}