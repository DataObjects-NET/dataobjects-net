// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

namespace Xtensive.Sql.Drivers.SqlServer.v10
{
  internal class Extractor : v09.Extractor
  {
    protected override string GetIndexQuery()
    {
      string query = @"
  SELECT
    t.schema_id,
    t.object_id,
    t.type,
    i.index_id,
    i.name,
    i.type,
    i.is_primary_key,
    i.is_unique,
    i.is_unique_constraint,
    i.fill_factor,
    ic.column_id,
    0,
    ic.key_ordinal,
    ic.is_descending_key,
    ic.is_included_column,
    i.has_filter,
    i.filter_definition
  FROM {CATALOG}.sys.indexes i 
  INNER JOIN (
    SELECT 
      schema_id,
      object_id,
      0 AS type
    FROM {CATALOG}.sys.tables
    WHERE {SYSTABLE_FILTER}
    UNION
    SELECT
      schema_id,
      object_id,
      1 AS type
    FROM {CATALOG}.sys.views
    ) AS t 
      ON i.object_id = t.object_id 
  INNER JOIN {CATALOG}.sys.index_columns ic
    ON i.object_id = ic.object_id
      AND i.index_id = ic.index_id
  WHERE i.type <> 3
    AND schema_id{SCHEMA_FILTER}
  ORDER BY
    t.schema_id,
    t.object_id,
    i.index_id,
    ic.is_included_column,
    ic.key_ordinal";
      query = PerformReplacements(query);
      return query;
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}