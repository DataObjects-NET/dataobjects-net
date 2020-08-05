// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.04.05

using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Xtensive.Sql.Drivers.SqlServer.v11
{
  internal class Extractor : v10.Extractor
  {
    protected override void ExtractCatalogContents()
    {
      base.ExtractCatalogContents();
      ExtractSequences();
    }

    protected override async Task ExtractCatalogContentsAsync(CancellationToken token)
    {
      await base.ExtractCatalogContentsAsync(token).ConfigureAwait(false);
      await ExtractSequencesAsync(token).ConfigureAwait(false);
    }

    private void ExtractSequences()
    {
      var query = BuildExtractSequencesQuery();

      using var cmd = Connection.CreateCommand(query);
      using var reader = cmd.ExecuteReader();
      while (reader.Read()) {
        ReadSequenceData(reader);
      }
    }

    private async Task ExtractSequencesAsync(CancellationToken token)
    {
      var query = BuildExtractSequencesQuery();

      var cmd = Connection.CreateCommand(query);
      await using (cmd.ConfigureAwait(false)) {
        var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
        await using (reader.ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
            ReadSequenceData(reader);
          }
        }
      }
    }

    private string BuildExtractSequencesQuery()
    {
      var query = @"
  SELECT
    schema_id,
    name,
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
      query = PerformReplacements(query);
      return query;
    }

    private void ReadSequenceData(DbDataReader reader)
    {
      var currentSchema = GetSchema(reader.GetInt32(0));
      var sequence = currentSchema.CreateSequence(reader.GetString(1));
      var descriptor = sequence.SequenceDescriptor;
      descriptor.StartValue = reader.GetInt64(2);
      descriptor.Increment = reader.GetInt64(3);
      descriptor.MinValue = reader.GetInt64(4);
      descriptor.MaxValue = reader.GetInt64(5);
      descriptor.IsCyclic = reader.GetBoolean(6);
      descriptor.LastValue = reader.GetInt64(7);
    }


    protected override void RegisterReplacements(Dictionary<string, string> replacements)
    {
      base.RegisterReplacements(replacements);
      replacements[SysTablesFilterPlaceholder] = "is_filetable = 0";
    }

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}