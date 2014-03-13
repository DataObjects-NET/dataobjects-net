// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  internal sealed class MultischemaMappingResolver : MappingResolver
  {
    private readonly string defaultSchema;
    private readonly List<SqlExtractionTask> extractionTasks;
    private readonly SqlExtractionTask metadataTask;
    private readonly NameMappingCollection schemaMapping;

    public override string GetNodeName(string mappingDatabase, string mappingSchema, string mappingName)
    {
      if (string.IsNullOrEmpty(mappingSchema))
        mappingSchema = defaultSchema;
      return FormatNodeName(schemaMapping.Apply(mappingSchema), schemaMapping.Apply(mappingName));
    }

    public override string GetNodeName(SchemaNode node)
    {
      return FormatNodeName(node.Schema.Name, node.Name);
    }

    public override MappingResolveResult Resolve(SchemaExtractionResult model, string nodeName)
    {
      var names = nodeName.Split(NameElementSeparator);
      var schema = model.Catalogs.Single().Schemas[names[0]];
      var name = names[1];
      if (schema==null)
        throw new InvalidOperationException(string.Format(Strings.ExUnableToResolveSchemaForNodeXPleaseVerifyThatThisSchemaExists, nodeName));
      return new MappingResolveResult(schema, name);
    }

    public override IEnumerable<SqlExtractionTask> GetSchemaTasks()
    {
      return extractionTasks;
    }

    public override IEnumerable<SqlExtractionTask> GetMetadataTasks()
    {
      return Enumerable.Repeat(metadataTask, 1);
    }

    private string FormatNodeName(string mappingSchema, string mappingName)
    {
      return mappingSchema + NameElementSeparator + mappingName;
    }


    // Constructors

    public MultischemaMappingResolver(DomainConfiguration configuration, NodeConfiguration nodeConfiguration, ProviderInfo providerInfo)
    {
      schemaMapping = nodeConfiguration.SchemaMapping;

      defaultSchema = configuration.DefaultSchema;

      extractionTasks = configuration.MappingRules
        .Select(r => r.Schema)
        .Where(s => !string.IsNullOrEmpty(s))
        .Concat(Enumerable.Repeat(configuration.DefaultSchema, 1))
        .Distinct()
        .Select(s => new SqlExtractionTask(providerInfo.DefaultDatabase, s))
        .ToList();

      metadataTask = new SqlExtractionTask(providerInfo.DefaultDatabase, defaultSchema);
    }
  }
}