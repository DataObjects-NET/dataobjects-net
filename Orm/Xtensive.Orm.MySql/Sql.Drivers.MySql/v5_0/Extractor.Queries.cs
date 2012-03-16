// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

namespace Xtensive.Sql.Drivers.MySql.v5_0
{
  internal partial class Extractor
  {
    protected const string SchemaFilterPlaceholder = "{SCHEMA_FILTER}";
    protected const string TableFilterPlaceholder = "{TABLE_FILTER}";
    protected const string IndexesFilterPlaceholder = "{INDEXES_FILTER}";

    protected string GetExtractSchemasQuery()
    {
      return
        @"SELECT 
                    u.user 
                FROM mysql.user u 
                WHERE user <> ''";
    }

    protected string GetExtractTablesQuery()
    {
      return
        @"SELECT 
                    t.table_schema, 
                    t.table_name, 
                    t.table_type
                FROM information_schema.tables t
                WHERE t.table_schema {SCHEMA_FILTER}";
    }

    protected string GetExtractTableColumnsQuery()
    {
      return
        @"SELECT 
                    c.table_schema,
                    c.table_name,
                    c.ordinal_position,
                    c.column_name,
                    c.data_type,
                    c.is_nullable,
                    c.column_type,
                    c.character_maximum_length,
                    c.numeric_precision,
                    c.numeric_scale,
                    c.collation_name,
                    c.column_key,
                    c.column_default,
                    c.Extra,
                    t.auto_increment
                FROM information_schema.columns c LEFT OUTER JOIN information_schema.tables t  ON (c.table_name = t.table_name)
                WHERE c.table_schema {SCHEMA_FILTER}
                AND t.table_schema {SCHEMA_FILTER}
                AND c.table_name {TABLE_FILTER}
                ORDER BY 
                    c.table_schema, 
                    c.table_name, 
                    c.ordinal_position";
    }

    protected string GetExtractViewsQuery()
    {
      return
        @"SELECT
                    v.table_schema,
                    v.table_name,
                    v.view_definition
                FROM information_schema.views v
                WHERE
                    v.table_schema {SCHEMA_FILTER}";
    }

    protected string GetExtractViewColumnsQuery()
    {
      return
        @"SELECT
                    v.table_schema,
                    v.table_name,
                    c.column_name,
                    c.ordinal_position,
                    v.view_definition
                FROM information_schema.views v
                INNER JOIN information_schema.columns c
                ON ((c.table_schema = v.table_schema)
                AND (c.table_name = v.table_name))
                WHERE
                    c.table_schema {SCHEMA_FILTER}
                ORDER BY
                    c.table_schema,
                    v.table_name,
                    c.ordinal_position";
    }

    protected string GetExtractIndexesQuery()
    {
      return
        @"SELECT 
                       s.table_schema,
                       s.table_name,
                       s.index_name,
                       s.non_unique,
                       s.index_type,
                       s.seq_in_index,
                       s.column_name,
                       s.cardinality,
                       s.sub_part,
                       s.nullable
                FROM 
                       information_schema.statistics s
                WHERE
                    (s.table_schema {SCHEMA_FILTER}
                AND s.index_name <> 'PRIMARY')
                ORDER BY
                      s.table_schema,
                      s.table_name,
                      s.index_name,
                      s.seq_in_index";
    }

    protected string GetExtractForeignKeysQuery()
    {
      return
        @"SELECT 
                     r.constraint_schema,
                     r.table_name,
                     r.constraint_name,
                     r.delete_rule,
                     c.column_name,
                     c.ordinal_position,
                     c.referenced_table_schema,
                     c.referenced_table_name,
                     c.referenced_column_name
                FROM 
                    information_schema.referential_constraints r join
                    information_schema.key_column_usage c
                    on ((r.table_name = c.table_name) and (r.constraint_schema = c.table_schema) and (r.constraint_name = c.constraint_name))
                WHERE
                     (r.constraint_schema {SCHEMA_FILTER})
                AND c.constraint_name <> 'PRIMARY'
                ORDER BY
                    r.constraint_schema, r.table_name, r.constraint_name, c.ordinal_position";
    }

    protected string GetExtractCheckConstraintsQuery()
    {
      return
        @"SELECT 
                  t.constraint_schema,
                  t.constraint_name,
                  t.table_schema,
                  t.table_name,
                  t.constraint_type
              FROM
                  information_schema.table_constraints t
              WHERE
                  (t.constraint_schema {SCHEMA_FILTER})
              AND t.constraint_type = 'CHECK'";
    }

    protected string GetExtractUniqueAndPrimaryKeyConstraintsQuery()
    {
      return
        @"SELECT 
                t.constraint_schema,
                t.table_name,
                t.constraint_name,
                t.constraint_type,
                c.column_name,
                c.ordinal_position
            FROM
                information_schema.table_constraints t join
                information_schema.key_column_usage c
            ON ((t.table_name = c.table_name) and (t.constraint_schema = c.table_schema) AND (t.constraint_name = c.constraint_name))
            WHERE
                (t.constraint_schema {SCHEMA_FILTER})
                AND (t.table_name {TABLE_FILTER})
                AND t.constraint_type in('PRIMARY KEY', 'UNIQUE')
            ORDER BY
                t.constraint_schema,
                t.table_name,
                t.constraint_name,
                c.ordinal_position";
    }

    protected virtual string GetExtractSequencesQuery()
    {
      return
        @"";
    }
  }
}