// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Sql.Drivers.Firebird.v4_0
{
  internal partial class Extractor
  {
    protected override string GetExtractTableColumnsQuery()
    {
      return @"
select   schema
        ,table_name
        ,ordinal_position
        ,column_name
        ,field_type
        ,column_subtype
        ,column_size
        ,numeric_precision
        ,-numeric_scale as numeric_scale
        ,character_max_length
        ,(1 - coalesce(column_nullable,0)) as column_nullable
        ,column_default
        ,relation_type
from     (select   cast(null as varchar(30)) as schema
                  ,trim(rfr.rdb$relation_name) as table_name
                  ,trim(rfr.rdb$field_name) as column_name
                  ,fld.rdb$field_sub_type as column_subtype
                  ,cast(fld.rdb$field_length as integer) as column_size
                  ,cast(fld.rdb$field_precision as integer) as numeric_precision
                  ,cast(fld.rdb$field_scale as integer) as numeric_scale
                  ,cast(fld.rdb$character_length as integer) as character_max_length
                  ,cast(fld.rdb$field_length as integer) as character_octet_length
                  ,rfr.rdb$field_position as ordinal_position
                  ,trim(rfr.rdb$field_source) as domain_name
                  ,trim(rfr.rdb$default_source) as column_default
                  ,trim(fld.rdb$computed_source) as computed_source
                  ,fld.rdb$dimensions as column_array
                  ,coalesce(fld.rdb$null_flag, rfr.rdb$null_flag) as column_nullable
                  ,0 as is_readonly
                  ,fld.rdb$field_type as field_type
                  ,trim(cs.rdb$character_set_name) as character_set_name
                  ,trim(coll.rdb$collation_name) as collation_name
                  ,trim(rfr.rdb$description) as description
                  ,cast(rr.rdb$relation_type as integer) as relation_type
          from     rdb$relations rr join rdb$relation_fields rfr on rfr.rdb$relation_name = rr.rdb$relation_name
                   left join rdb$fields fld on rfr.rdb$field_source = fld.rdb$field_name
                   left join rdb$character_sets cs
                      on cs.rdb$character_set_id = fld.rdb$character_set_id
                   left join rdb$collations coll
                      on (coll.rdb$collation_id = fld.rdb$collation_id
                      and coll.rdb$character_set_id = fld.rdb$character_set_id)
          where    rr.rdb$relation_type in (0, 4, 5) and rr.rdb$relation_name not starts with 'RDB$' and rr.rdb$relation_name not starts with 'MON$'
          order by table_name, ordinal_position)";
    }

    protected override string GetExtractUniqueAndPrimaryKeyConstraintsQuery()
    {
      return @"
select     cast(null as varchar(30)) as schema
          ,trim(rel.rdb$relation_name) as table_name
          ,trim(rel.rdb$constraint_name) as constraint_name
          ,trim(rel.rdb$constraint_type) constraint_type
          ,trim(seg.rdb$field_name) as column_name
          ,seg.rdb$field_position as column_position
from       rdb$relation_constraints rel
left join rdb$indices idx on rel.rdb$index_name = idx.rdb$index_name
left join rdb$index_segments seg on idx.rdb$index_name = seg.rdb$index_name
where      rel.rdb$constraint_type in ('PRIMARY KEY', 'UNIQUE')
           and rel.rdb$relation_name not starts with 'RDB$'
           and rel.rdb$relation_name not starts with 'MON$'
order by   rel.rdb$relation_name, rel.rdb$constraint_name, seg.rdb$field_position";
    }
  }
}
