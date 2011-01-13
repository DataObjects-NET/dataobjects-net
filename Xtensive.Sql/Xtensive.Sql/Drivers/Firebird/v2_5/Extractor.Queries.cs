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
            return @"
select     cast(null as varchar(30)) as schema
          ,ri.rdb$relation_name as table_name
          ,ri.rdb$index_name as index_name
          ,coalesce(ri.rdb$unique_flag,0) as unique_flag
          ,ri.rdb$index_id as index_seq
          ,coalesce(ri.rdb$index_type,0) as descend
          ,ris.rdb$field_name as column_name
          ,ris.rdb$field_position as column_position
from       rdb$indices ri join rdb$index_segments ris on ris.rdb$index_name = ri.rdb$index_name
where      ri.rdb$system_flag = 0
       and not exists
              (select   1
               from     rdb$relation_constraints rc
               where    rc.rdb$constraint_type in ('PRIMARY KEY', 'FOREIGN KEY')
                    and rc.rdb$relation_name = ri.rdb$relation_name
                    and rc.rdb$index_name = ri.rdb$index_name)
order by   ri.rdb$relation_name, ri.rdb$index_id, ris.rdb$field_position";
        }

        protected string GetExtractForeignKeysQuery()
        {
            return @"
select     cast(null as varchar(30)) as schema
          ,co.rdb$relation_name as table_name
          ,co.rdb$constraint_name as constraint_name
          ,co.rdb$deferrable as is_deferrable
          ,co.rdb$initially_deferred as deferred
          ,ref.rdb$delete_rule as delete_rule
          ,coidxseg.rdb$field_name as column_name
          ,coidxseg.rdb$field_position as column_position
          ,cast(null as varchar(30)) as referenced_schema
          ,refidx.rdb$relation_name as referenced_table_name
          ,refidxseg.rdb$field_name as referenced_column_name
          ,ref.rdb$match_option as match_option
          ,ref.rdb$update_rule as update_rule
from       rdb$relation_constraints co join rdb$ref_constraints ref on co.rdb$constraint_name = ref.rdb$constraint_name
           join rdb$indices tempidx
              on co.rdb$index_name = tempidx.rdb$index_name
           join rdb$index_segments coidxseg
              on co.rdb$index_name = coidxseg.rdb$index_name
           join rdb$relation_constraints unqc
              on ref.rdb$const_name_uq = unqc.rdb$constraint_name and unqc.rdb$constraint_type = 'UNIQUE'
           join rdb$indices refidx
              on refidx.rdb$index_name = unqc.rdb$index_name and refidx.rdb$relation_name not starts with 'RDB$'
           join rdb$index_segments refidxseg
              on refidx.rdb$index_name = refidxseg.rdb$index_name
             and coidxseg.rdb$field_position = refidxseg.rdb$field_position
where      co.rdb$constraint_type = 'FOREIGN KEY'
order by   co.rdb$relation_name as table_name, co.rdb$constraint_name, coidxseg.rdb$field_position";
        }

        protected string GetExtractUniqueAndPrimaryKeyConstraintsQuery()
        {
            return @"
select     cast(null as varchar(30)) as schema
          ,rel.rdb$relation_name as table_name
          ,rel.rdb$constraint_name as constraint_name
          ,rel.rdb$constraint_type constraint_type
          ,seg.rdb$field_name as column_name
          ,seg.rdb$field_position as column_position
from       rdb$relation_constraints rel left join rdb$indices idx on rel.rdb$index_name = idx.rdb$index_name
           left join rdb$index_segments seg
              on idx.rdb$index_name = seg.rdb$index_name
where      rel.rdb$constraint_type in ('PRIMARY KEY', 'UNIQUE')
order by   rel.rdb$relation_name, rel.rdb$constraint_name, seg.rdb$field_position";
        }

        protected string GetExtractCheckConstraintsQuery()
        {
            return @"
select     cast(null as varchar(30)) as schema
          ,chktb.rdb$relation_name as table_name
          ,chktb.rdb$constraint_name as constraint_name
          ,trig.rdb$trigger_source as check_clausule
from       rdb$relation_constraints chktb inner join rdb$check_constraints chk
              on (chktb.rdb$constraint_name = chk.rdb$constraint_name and chktb.rdb$constraint_type = 'CHECK')
           inner join rdb$triggers trig
              on chk.rdb$trigger_name = trig.rdb$trigger_name and trig.rdb$trigger_type = 1
order by   chktb.rdb$relation_name, chktb.rdb$constraint_name";
        }

        protected virtual string GetExtractSequencesQuery()
        {
            return @"
select   cast(null as varchar(30)) as schema, rdb$generator_name as sequence_name
from     rdb$generators
where    rdb$system_flag = 0";
        }
    }
}
