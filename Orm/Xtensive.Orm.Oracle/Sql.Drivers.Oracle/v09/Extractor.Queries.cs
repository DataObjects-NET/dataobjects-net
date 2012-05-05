// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.16

namespace Xtensive.Sql.Drivers.Oracle.v09
{
  partial class Extractor 
  {
    protected const string SchemaFilterPlaceholder = "{SCHEMA_FILTER}";
    protected const string TableFilterPlaceholder = "{TABLE_FILTER}";
    protected const string IndexesFilterPlaceholder = "{INDEXES_FILTER}";

    protected string GetExtractSchemasQuery()
    {
      return
@"SELECT
    USERNAME
FROM
    SYS.ALL_USERS";
    }

    protected string GetExtractTablesQuery()
    {
      return
@"SELECT
    OWNER,
    TABLE_NAME,
    TEMPORARY,
    DURATION
FROM
    SYS.ALL_TABLES
WHERE
    (NESTED = 'NO')
    AND (OWNER {SCHEMA_FILTER})
    AND (TABLE_NAME {TABLE_FILTER})";
    }

    protected string GetExtractTableColumnsQuery()
    {
      return
@"SELECT
    columns.OWNER,
    columns.TABLE_NAME,
    columns.COLUMN_NAME,
    columns.DATA_TYPE,
    columns.DATA_PRECISION,
    columns.DATA_SCALE,
    columns.CHAR_LENGTH,
    columns.NULLABLE,
    columns.DATA_DEFAULT,
    columns.COLUMN_ID
FROM
    SYS.ALL_TAB_COLUMNS columns 
    JOIN SYS.ALL_TABLES tables
        ON ((columns.TABLE_NAME = tables.TABLE_NAME)
        AND (columns.OWNER = tables.OWNER)) 
WHERE
    columns.OWNER {SCHEMA_FILTER}
    AND columns.TABLE_NAME {TABLE_FILTER}
ORDER BY
    columns.OWNER,
    columns.TABLE_NAME,
    columns.COLUMN_ID";
    }

    protected string GetExtractViewsQuery()
    {
      return
@"SELECT
    OWNER,
    VIEW_NAME,
    TEXT
FROM
    SYS.ALL_VIEWS views
WHERE
    OWNER {SCHEMA_FILTER}";
    }

    protected string GetExtractViewColumnsQuery()
    {
      return
@"SELECT
    columns.OWNER,
    columns.TABLE_NAME,
    columns.COLUMN_NAME,
    columns.COLUMN_ID
FROM
    SYS.ALL_TAB_COLUMNS columns
    JOIN SYS.ALL_VIEWS views
        ON ((columns.TABLE_NAME = views.VIEW_NAME)
        AND (columns.OWNER = views.OWNER))
WHERE
    columns.OWNER {SCHEMA_FILTER}
ORDER BY
    columns.OWNER,
    columns.TABLE_NAME,
    columns.COLUMN_ID";
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
    columns.DESCEND,
    expressions.COLUMN_EXPRESSION
FROM
    SYS.ALL_IND_COLUMNS columns
    JOIN SYS.ALL_INDEXES indexes
        ON ((indexes.INDEX_NAME = columns.INDEX_NAME)
        AND (indexes.OWNER = columns.INDEX_OWNER))
    LEFT JOIN SYS.ALL_IND_EXPRESSIONS expressions
        ON ((columns.INDEX_NAME = expressions.INDEX_NAME)
        AND (columns.INDEX_OWNER = expressions.INDEX_OWNER)
        AND (columns.COLUMN_POSITION = expressions.COLUMN_POSITION))
WHERE
    indexes.INDEX_TYPE IN ('NORMAL', 'BITMAP', 'FUNCTION-BASED NORMAL', 'FUNCTION-BASED BITMAP')
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