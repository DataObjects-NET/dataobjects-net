// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.10
using System;

namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
    partial class Extractor
    {
        protected const string TableFilterPlaceholder = "{TABLE_FILTER}";
        protected const string IndexesFilterPlaceholder = "{INDEXES_FILTER}";

        protected string GetExtractSchemasQuery()
        {
            return @"SELECT " + Constants.DefaultSchemaName + " FROM RDB$DATABASE";
        }

        protected string GetExtractTablesQuery()
        {
            return @"
select cast(null as varchar(30)) as schema
      ,rdb$relation_name as table_name
      ,rdb$relation_type as table_type
from rdb$relations 
where rdb$relation_type in (0, 5, 4)";
        }

        protected string GetExtractTableColumnsQuery()
        {
            return @"
select   schema
        ,table_name
        ,ordinal_position
        ,column_name
        ,case
            when field_type = 7 then 'smallint'
            when field_type = 8 and column_subtype = 0 then 'integer'
            when field_type = 8 and column_subtype = 2 then 'decimal'
            when field_type = 10 then 'float'
            when field_type = 12 then 'date'
            when field_type = 13 then 'time'
            when field_type = 14 then 'char'
            when field_type = 16 and column_subtype = 0 then 'bigint'
            when field_type = 16 and column_subtype = 1 then 'numeric'
            when field_type = 27 then 'double precision'
            when field_type = 35 then 'timestamp'
            when field_type = 37 then 'varchar'
            when field_type = 261 and column_subtype = 0 then 'blob sub type 1'
            when field_type = 261 and column_subtype = 1 then 'blob sub type 0'
         end
            data_type
        ,column_size
        ,numeric_precision
        ,-numeric_scale as numeric_scale
        ,column_nullable
        ,column_default
from     (select   cast(null as varchar(30)) as schema
                  ,rfr.rdb$relation_name as table_name
                  ,rfr.rdb$field_name as column_name
                  ,fld.rdb$field_sub_type as column_subtype
                  ,cast(fld.rdb$field_length as integer) as column_size
                  ,cast(fld.rdb$field_precision as integer) as numeric_precision
                  ,cast(fld.rdb$field_scale as integer) as numeric_scale
                  ,cast(fld.rdb$character_length as integer) as character_max_length
                  ,cast(fld.rdb$field_length as integer) as character_octet_length
                  ,rfr.rdb$field_position as ordinal_position
                  ,rfr.rdb$field_source as domain_name
                  ,rfr.rdb$default_source as column_default
                  ,fld.rdb$computed_source as computed_source
                  ,fld.rdb$dimensions as column_array
                  ,coalesce(fld.rdb$null_flag, rfr.rdb$null_flag) as column_nullable
                  ,0 as is_readonly
                  ,fld.rdb$field_type as field_type
                  ,cs.rdb$character_set_name as character_set_name
                  ,coll.rdb$collation_name as collation_name
                  ,rfr.rdb$description as description
          from     rdb$relation_fields rfr left join rdb$fields fld on rfr.rdb$field_source = fld.rdb$field_name
                   left join rdb$character_sets cs
                      on cs.rdb$character_set_id = fld.rdb$character_set_id
                   left join rdb$collations coll
                      on (coll.rdb$collation_id = fld.rdb$collation_id
                      and coll.rdb$character_set_id = fld.rdb$character_set_id)
          where    1 = 1
          order by table_name, ordinal_position)";
        }

        protected string GetExtractViewsQuery()
        {
            return @"
select cast(null as varchar(30)) as schema
      ,rdb$relation_name as table_name
      ,cast(rdb$view_source as varchar(30000)) as view_source
from rdb$relations
where rdb$relation_type = 1";
        }

        protected string GetExtractViewColumnsQuery()
        {
            return @"
select     cast(null as varchar(30)) as schema
          ,rfr.rdb$relation_name as table_name
          ,rfr.rdb$field_name as column_name
          ,rfr.rdb$field_position as ordinal_position
from       rdb$relations rr join rdb$relation_fields rfr on rfr.rdb$relation_name = rr.rdb$relation_name
where      rr.rdb$relation_type = 1
order by   rfr.rdb$relation_name, rfr.rdb$field_position";
        }

        protected string GetExtractIndexesQuery()
        {
            return
      @"SELECT
    indexes.TABLE_OWNER,
    indexes.TABLE_NAME,
    indexes.INDEX_NAME,
    indexes.UNIQUENESS,
    indexes.INDEX_TYPE,
    indexes.PCT_FREE,
    columns.COLUMN_POSITION,
    columns.COLUMN_NAME,
    columns.DESCEND
FROM
    SYS.ALL_IND_COLUMNS columns
    JOIN SYS.ALL_INDEXES indexes
        ON ((indexes.INDEX_NAME = columns.INDEX_NAME)
        AND (indexes.OWNER = columns.INDEX_OWNER))
WHERE
    indexes.INDEX_TYPE IN ('NORMAL', 'BITMAP')
    AND (indexes.TABLE_NAME {TABLE_FILTER})
    AND (indexes.TABLE_OWNER {SCHEMA_FILTER})
    AND ({INDEXES_FILTER})
    AND ((indexes.OWNER, indexes.INDEX_NAME) NOT IN (
        SELECT
            constraints.INDEX_OWNER,
            constraints.INDEX_NAME
        FROM
            SYS.ALL_CONSTRAINTS constraints
            JOIN SYS.ALL_TABLES tables
                ON ((constraints.OWNER = tables.OWNER)
                AND (constraints.TABLE_NAME = tables.TABLE_NAME))
        WHERE
            (constraints.CONSTRAINT_TYPE IN ('P', 'U')
            AND (constraints.OWNER {SCHEMA_FILTER}))))
ORDER BY
    indexes.TABLE_OWNER,
    indexes.TABLE_NAME,
    indexes.INDEX_NAME,
    columns.COLUMN_POSITION";
        }

        protected string GetExtractForeignKeysQuery()
        {
            return
      @"SELECT
    constraints.OWNER,
    constraints.TABLE_NAME,
    constraints.CONSTRAINT_NAME,
    constraints.""DEFERRABLE"",
    constraints.""DEFERRED"",
    constraints.DELETE_RULE,
    columns.COLUMN_NAME,
    columns.POSITION,
    rel_columns.OWNER,
    rel_columns.TABLE_NAME,
    rel_columns.COLUMN_NAME
FROM
    SYS.ALL_CONSTRAINTS constraints
    JOIN SYS.ALL_CONS_COLUMNS columns
        ON ((constraints.CONSTRAINT_NAME = columns.CONSTRAINT_NAME)
        AND (constraints.OWNER = columns.OWNER))
    JOIN SYS.ALL_CONS_COLUMNS rel_columns
        ON ((constraints.R_CONSTRAINT_NAME = rel_columns.CONSTRAINT_NAME)
        AND (constraints.R_OWNER = rel_columns.OWNER)
        AND (columns.POSITION = rel_columns.POSITION))
    JOIN SYS.ALL_TABLES tables
        ON ((constraints.OWNER = tables.OWNER)
        AND (constraints.TABLE_NAME = tables.TABLE_NAME))
WHERE
    (constraints.CONSTRAINT_TYPE = 'R')
    AND (constraints.OWNER {SCHEMA_FILTER})
ORDER BY
    constraints.OWNER, constraints.TABLE_NAME, constraints.CONSTRAINT_NAME, rel_columns.POSITION";
        }

        protected string GetExtractCheckConstraintsQuery()
        {
            return
      @"SELECT
    constraints.OWNER,
    constraints.TABLE_NAME,
    constraints.CONSTRAINT_NAME,
    constraints.SEARCH_CONDITION,
    constraints.""DEFERRABLE"",
    constraints.""DEFERRED""
FROM
    SYS.ALL_CONSTRAINTS constraints
    JOIN SYS.ALL_TABLES tables
        ON ((constraints.OWNER = tables.OWNER)
        AND (constraints.TABLE_NAME = tables.TABLE_NAME))
WHERE
    (constraints.CONSTRAINT_TYPE = 'C')
    AND (constraints.GENERATED = 'USER NAME')
    AND (constraints.OWNER {SCHEMA_FILTER})";
        }

        protected string GetExtractUniqueAndPrimaryKeyConstraintsQuery()
        {
            return
      @"SELECT
    constraints.OWNER,
    constraints.TABLE_NAME,
    constraints.CONSTRAINT_NAME,
    constraints.CONSTRAINT_TYPE,
    columns.COLUMN_NAME,
    columns.POSITION
FROM
    SYS.ALL_CONSTRAINTS constraints
    JOIN SYS.ALL_CONS_COLUMNS columns
        ON ((constraints.CONSTRAINT_NAME = columns.CONSTRAINT_NAME)
        AND (constraints.OWNER = columns.OWNER))
    JOIN SYS.ALL_TABLES tables
        ON ((constraints.OWNER = tables.OWNER)
        AND (constraints.TABLE_NAME = tables.TABLE_NAME))
WHERE
    (constraints.CONSTRAINT_TYPE IN ('P', 'U'))
    AND (constraints.OWNER {SCHEMA_FILTER})
    AND (constraints.TABLE_NAME {TABLE_FILTER})
ORDER BY
    constraints.OWNER,
    constraints.TABLE_NAME,
    constraints.CONSTRAINT_NAME,
    columns.POSITION";
        }

        protected virtual string GetExtractSequencesQuery()
        {
            return
      @"SELECT
    SEQUENCE_OWNER,
    SEQUENCE_NAME,
    MIN_VALUE,
    MAX_VALUE,
    INCREMENT_BY,
    CYCLE_FLAG
FROM
    SYS.ALL_SEQUENCES
WHERE
    SEQUENCE_OWNER {SCHEMA_FILTER}";
        }
    }
}
