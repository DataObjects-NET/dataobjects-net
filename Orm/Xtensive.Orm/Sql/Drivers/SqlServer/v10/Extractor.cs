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
      string query = "select t.schema_id, t.object_id, t.type, i.index_id, i.name, i.type, i.is_primary_key, i.is_unique, i.is_unique_constraint, i.fill_factor, ic.column_id, 0, ic.key_ordinal, ic.is_descending_key, ic.is_included_column, i.has_filter, i.filter_definition from sys.indexes i inner join (select schema_id, object_id, 0 as type from sys.tables union select schema_id, object_id, 1 as type from sys.views) as t on i.object_id = t.object_id inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id where i.type <> 3";
      if (schema!=null)
        query += " and schema_id = " + schemaId;
      query += " order by t.schema_id, t.object_id, i.index_id, ic.is_included_column, ic.key_ordinal";
      query = AddCatalog(query);
      return query;
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}