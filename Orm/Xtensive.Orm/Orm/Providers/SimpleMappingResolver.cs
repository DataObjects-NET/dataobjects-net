// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  internal sealed class SimpleMappingResolver : MappingResolver
  {
    private readonly SqlExtractionTask extractionTask;

    public override string GetNodeName(string mappingDatabase, string mappingSchema, string mappingName)
    {
      return mappingName;
    }

    public override string GetNodeName(SchemaNode node)
    {
      return node.Name;
    }

    public override MappingResolveResult Resolve(SchemaExtractionResult model, string nodeName)
    {
      var schema = model.Catalogs[extractionTask.Catalog].Schemas[extractionTask.Schema];
      return new MappingResolveResult(schema, nodeName);
    }

    public override IEnumerable<SqlExtractionTask> GetSchemaTasks()
    {
      return Enumerable.Repeat(extractionTask, 1);
    }

    public override IEnumerable<SqlExtractionTask> GetMetadataTasks()
    {
      return Enumerable.Repeat(extractionTask, 1);
    }

    // Constructors

    public SimpleMappingResolver(DefaultSchemaInfo defaultSchemaInfo)
    {
      extractionTask = new SqlExtractionTask(defaultSchemaInfo.Database, defaultSchemaInfo.Schema);
    }
  }
}