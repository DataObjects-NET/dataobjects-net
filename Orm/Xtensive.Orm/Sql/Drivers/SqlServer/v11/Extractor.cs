// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.05

using System;
using System.Data.Common;

namespace Xtensive.Sql.Drivers.SqlServer.v11
{
  internal class Extractor : v10.Extractor
  {
    protected override void ExtractCatalogContents()
    {
      base.ExtractCatalogContents();
      ExtractSequences();
    }

    private void ExtractSequences()
    {
      string query =
        " select" +
        "   schema_id, name, start_value, increment," +
        "   minimum_value, maximum_value, is_cycling, current_value" +
        " from" +
        "   sys.sequences";

      if (schema!=null)
        query += " where schema_id = " + schemaId;
      query += " order by schema_id, object_id";
      query = AddCatalog(query);

      var currentSchemaId = schemaId;
      var currentSchema = schema;

      using (var cmd = Connection.CreateCommand(query))
      using (var reader = cmd.ExecuteReader())
        while (reader.Read()) {
          GetSchema(reader.GetInt32(0), ref currentSchemaId, ref currentSchema);
          var sequence = currentSchema.CreateSequence(reader.GetString(1));
          var descriptor = sequence.SequenceDescriptor;
          descriptor.StartValue = reader.GetInt64(2);
          descriptor.Increment = reader.GetInt64(3);
          descriptor.MinValue = reader.GetInt64(4);
          descriptor.MaxValue = reader.GetInt64(5);
          descriptor.IsCyclic = reader.GetBoolean(6);
          descriptor.LastValue = reader.GetInt64(7);
        }
    }

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}