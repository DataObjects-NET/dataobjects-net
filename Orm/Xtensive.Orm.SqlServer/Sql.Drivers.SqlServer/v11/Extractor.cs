// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.04.05

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;

namespace Xtensive.Sql.Drivers.SqlServer.v11
{
  internal class Extractor : v10.Extractor
  {
    private readonly Dictionary<int, Func<DbDataReader, int, long>> valueReaders;

    protected override void ExtractCatalogContents(ExtractionContext context)
    {
      base.ExtractCatalogContents(context);
      ExtractSequences(context);
    }

    protected override async Task ExtractCatalogContentsAsync(ExtractionContext context, CancellationToken token)
    {
      await base.ExtractCatalogContentsAsync(context, token).ConfigureAwaitFalse();
      await ExtractSequencesAsync(context, token).ConfigureAwaitFalse();
    }

    private void ExtractSequences(ExtractionContext context)
    {
      var query = BuildExtractSequencesQuery(context);

      using var cmd = Connection.CreateCommand(query);
      using var reader = cmd.ExecuteReader();
      while (reader.Read()) {
        ReadSequenceData(reader, context);
      }
    }

    private async Task ExtractSequencesAsync(ExtractionContext context, CancellationToken token)
    {
      var query = BuildExtractSequencesQuery(context);

      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwaitFalse()) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwaitFalse();
        await using (reader.ConfigureAwaitFalse()) {
          while (await reader.ReadAsync(token).ConfigureAwaitFalse()) {
            ReadSequenceData(reader, context);
          }
        }
      }
    }

    private string BuildExtractSequencesQuery(ExtractionContext context)
    {
      var query = @"
  SELECT
    schema_id,
    name,
    user_type_id,
    start_value,
    increment,
    minimum_value,
    maximum_value,
    is_cycling,
    current_value
  FROM {CATALOG}.sys.sequences
  WHERE schema_id {SCHEMA_FILTER}
  ORDER BY
    schema_id,
    object_id";
      query = context.PerformReplacements(query);
      return query;
    }

    private void ReadSequenceData(DbDataReader reader, ExtractionContext context)
    {

      var currentSchema = context.SchemaIndex[reader.GetInt32(0)];
      var sequenceName = reader.GetString(1);
      var sequence = currentSchema.CreateSequence(sequenceName);
      var descriptor = sequence.SequenceDescriptor;

      if (!valueReaders.TryGetValue(reader.GetInt32(2), out var valueReader)) {
        throw new ArgumentOutOfRangeException($"Type of sequence '{sequenceName}' is not supported.");
      }

      descriptor.StartValue = valueReader(reader, 3);
      descriptor.Increment = valueReader(reader, 4);
      descriptor.MinValue = valueReader(reader, 5);
      descriptor.MaxValue = valueReader(reader, 6);
      descriptor.IsCyclic = reader.GetBoolean(7);
      descriptor.LastValue = valueReader(reader, 8);
    }


    protected override void RegisterReplacements(ExtractionContext context)
    {
      base.RegisterReplacements(context);
      context.RegisterReplacement(SysTablesFilterPlaceholder, "is_filetable = 0");
    }

    public Extractor(SqlDriver driver)
      : base(driver)
    {
      valueReaders = new Dictionary<int, Func<DbDataReader, int, long>>(4);
      valueReaders[48] = (DbDataReader reader, int index) => reader.GetByte(index);
      valueReaders[52] = (DbDataReader reader, int index) => reader.GetInt16(index);
      valueReaders[56] = (DbDataReader reader, int index) => reader.GetInt32(index);
      valueReaders[127] = (DbDataReader reader, int index) => reader.GetInt64(index);
    }
  }
}
