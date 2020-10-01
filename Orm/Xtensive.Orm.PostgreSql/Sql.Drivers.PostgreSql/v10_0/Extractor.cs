// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.25

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override ISqlCompileUnit BuildExtractSequencesQuery(ExtractionContext context)
    {
      var sequenceMap = context.SequenceMap;
      // select all the sequences registered in map
      var tableRef = PgSequence;
      var select = SqlDml.Select(tableRef);
      select.Where = SqlDml.In(tableRef["seqrelid"], SqlDml.Array(sequenceMap.Keys.ToArray()));
      return select;
    }

    /// <inheritdoc/>
    protected override void ReadSequenceDescriptor(DbDataReader dataReader, ExtractionContext context)
    {
      var seqId = Convert.ToInt64(dataReader["seqrelid"]);
      var descriptor = context.SequenceMap[seqId].SequenceDescriptor;
      descriptor.Increment = Convert.ToInt64(dataReader["seqincrement"]);
      descriptor.IsCyclic = Convert.ToBoolean(dataReader["seqcycle"]);
      descriptor.MinValue = Convert.ToInt64(dataReader["seqmin"]);
      descriptor.MaxValue = Convert.ToInt64(dataReader["seqmax"]);
      descriptor.StartValue = Convert.ToInt64(dataReader["seqstart"]);
    }

    protected void CreateOidColumn(Table table, string name) =>
      table.CreateColumn(name, new SqlValueType(SqlType.Int64));

    protected void CreateInt8Column(Table table, string name) =>
      table.CreateColumn(name, new SqlValueType(SqlType.Int64));

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}