// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.05

using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Xtensive.Sql.Drivers.SqlServer.v11
{
  internal class Extractor : v10.Extractor
  {
    private readonly Dictionary<int, Func<DbDataReader, int, long>> valueReaders;

    protected override void ExtractCatalogContents()
    {
      base.ExtractCatalogContents();
      ExtractSequences();
    }

    private void ExtractSequences()
    {
      string query = @"
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
      query = PerformReplacements(query);

      //var currentSchemaId = schemaId;
      //var currentSchema = schema;

      using (var cmd = Connection.CreateCommand(query))
      using (var reader = cmd.ExecuteReader())
        while (reader.Read()) {
          var currentSchema = GetSchema(reader.GetInt32(0));
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
    }

    protected override void RegisterReplacements(Dictionary<string, string> replacements)
    {
      base.RegisterReplacements(replacements);
      replacements[SysTablesFilterPlaceholder] = "is_filetable = 0";
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