
namespace Xtensive.Sql.Drivers.MySql.v5
{
    partial class Extractor
    {
        protected const string SchemaFilterPlaceholder = "{SCHEMA_FILTER}";
        protected const string TableFilterPlaceholder = "{TABLE_FILTER}";
        protected const string IndexesFilterPlaceholder = "{INDEXES_FILTER}";

        protected string GetExtractSchemasQuery()
        {
            return
              @"select 
                    u.user 
                from mysql.user u 
                where user <> ''";
        }

        protected string GetExtractTablesQuery()
        {
            return
              @"select 
                    t.table_schema, 
                    t.table_name, 
                    t.table_type
                from information_schema.tables t
                where t.table_schema {SCHEMA_FILTER})";
        }

        protected string GetExtractTableColumnsQuery()
        {
            return
                @"select c.table_schema,
                    c.table_name,
                    c.ordinal_position,
                    c.column_name,
                    c.data_type,
                    c.column_type,
                    c.character_maximum_length,
                    c.numeric_precision,
                    c.numeric_scale,
                    c.collation_name,
                    c.Extra
                from information_schema.columns c
                where c.table_schema {SCHEMA_FILTER}
                and c.table_name {TABLE_FILTER}
                order by 
                    c.table_schema, 
                    c.table_name, 
                    c.ordinal_position";
        }

        protected string GetExtractViewsQuery()
        {
            return
              @"select
                    v.table_schema,
                    v.table_name,
                    v.view_definition
                from information_schema.views v
                where
                    v.table_schema {SCHEMA_FILTER}";
        }

        protected string GetExtractViewColumnsQuery()
        {
            return
              @"select
                    v.table_schema,
                    v.table_name,
                    c.column_name,
                    c.ordinal_position,
                    v.view_definition
                from information_schema.views v
                inner join information_schema.columns c
                on ((c.table_schema = v.table_schema)
                and (c.table_name = v.table_name))
                where
                    c.table_schema {SCHEMA_FILTER}
                order by
                    c.table_schema,
                    v.table_name,
                    c.ordinal_position";
        }

        protected string GetExtractIndexesQuery()
        {
            return
                @"";
        }

        protected string GetExtractForeignKeysQuery()
        {
            return
              @"select 
                 r.constraint_schema,
                 r.table_name,
                 r.constraint_name,
                 r.delete_rule,
                 c.column_name,
                 c.ordinal_position,
                 c.referenced_table_schema,
                 c.referenced_table_name,
                 c.referenced_column_name
            from 
                information_schema.referential_constraints r join
                information_schema.key_column_usage c
                on ((r.table_name = c.table_name) and (r.constraint_schema = c.table_schema))
            WHERE
                 (r.constraint_schema {SCHEMA_FILTER})
            ORDER BY
                r.constraint_schema, r.table_name, r.constraint_name, c.ordinal_position";
        }

        protected string GetExtractCheckConstraintsQuery()
        {
            return
            @"";
        }

        protected string GetExtractUniqueAndPrimaryKeyConstraintsQuery()
        {
            return
              @"select 
                    t.constraint_schema,
                    t.table_name,
                    t.constraint_name,
                    t.constraint_type,
                    c.column_name,
                    c.ordinal_position
                from
                information_schema.table_constraints t join
                information_schema.key_column_usage c
                on ((t.table_name = c.table_name) and (t.constraint_schema = c.table_schema))
                WHERE
                    AND (t.constraint_schema {SCHEMA_FILTER})
                    AND (t.table_name {TABLE_FILTER})
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
