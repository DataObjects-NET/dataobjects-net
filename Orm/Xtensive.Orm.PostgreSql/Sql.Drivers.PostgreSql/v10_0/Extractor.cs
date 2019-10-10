// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.09.25

using System;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.PostgreSql.v10_0
{
  internal class Extractor : v9_1.Extractor
  {
    /// <summary>
    /// <see cref="SqlTableRef">Reference</see> to system table pg_sequence.
    /// </summary>
    protected SqlTableRef PgSequence => SqlDml.TableRef(PgCatalogSchema.Tables["pg_sequence"]);

    protected override void BuildPgCatalogSchema(Schema schema)
    {
      base.BuildPgCatalogSchema(schema);
      var t = schema.CreateTable("pg_sequence");
      CreateOidColumn(t, "seqrelid");
      CreateOidColumn(t, "seqtypid");
      CreateInt8Column(t, "seqstart");
      CreateInt8Column(t, "seqincrement");
      CreateInt8Column(t, "seqmax");
      CreateInt8Column(t, "seqmin");
      CreateInt8Column(t, "seqcache");
      CreateBoolColumn(t, "seqcycle");
    }

    protected override void ExtractSequenses(ExtractionContext context)
    {
      var sequenceMap = context.SequenceMap;

      if (sequenceMap.Count > 0) {
        // select all the sequences registered in map
        var tableRef = PgSequence;
        var select = SqlDml.Select(tableRef);
        select.Where = SqlDml.In(tableRef["seqrelid"], SqlDml.Array(sequenceMap.Keys.ToArray()));

        using (DbCommand cmd = Connection.CreateCommand(select))
        using (DbDataReader dr = cmd.ExecuteReader()) {
          while (dr.Read()) {
            var seqId = Convert.ToInt64(dr["seqrelid"]);
            var sequence = context.SequenceMap[seqId];
            ReadSequenceDescriptor(dr, sequence.SequenceDescriptor);
          }
        }
      }
    }

    protected override void ReadSequenceDescriptor(DbDataReader reader, SequenceDescriptor descriptor)
    {
      descriptor.Increment = Convert.ToInt64(reader["seqincrement"]);
      descriptor.IsCyclic = Convert.ToBoolean(reader["seqcycle"]);
      descriptor.MinValue = Convert.ToInt64(reader["seqmin"]);
      descriptor.MaxValue = Convert.ToInt64(reader["seqmax"]);
      descriptor.StartValue = Convert.ToInt64(reader["seqstart"]);
    }

    protected void CreateOidColumn(Table table, string name)
    {
      table.CreateColumn(name, new SqlValueType(SqlType.Int64));
    }

    protected void CreateInt8Column(Table table, string name)
    {
      table.CreateColumn(name, new SqlValueType(SqlType.Int64));
    }

    // Consructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}