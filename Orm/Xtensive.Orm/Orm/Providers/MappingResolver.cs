// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.06

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  internal abstract class MappingResolver
  {
    protected const char NameElementSeparator = ':'; // This char is forbidden by name validator

    public string GetNodeName(SchemaMappedNode node)
    {
      return GetNodeName(node.MappingDatabase, node.MappingSchema, node.MappingName);
    }

    public abstract string GetNodeName(string mappingDatabase, string mappingSchema, string mappingName);

    public abstract string GetNodeName(SchemaNode node);

    public abstract MappingResolveResult Resolve(SchemaExtractionResult model, string nodeName);

    public Schema ResolveSchema(SchemaExtractionResult model, string mappingDatabase, string mappingSchema)
    {
      var sampleNameNode = GetNodeName(mappingDatabase, mappingSchema, "sample");
      return Resolve(model, sampleNameNode).Schema;
    }

    public abstract IEnumerable<SqlExtractionTask> GetSchemaTasks();

    public abstract IEnumerable<SqlExtractionTask> GetMetadataTasks();

    public static MappingResolver Create(DomainConfiguration configuration, NodeConfiguration nodeConfiguration, ProviderInfo providerInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      ArgumentValidator.EnsureArgumentNotNull(nodeConfiguration, "nodeConfiguration");
      ArgumentValidator.EnsureArgumentNotNull(providerInfo, "providerInfo");

      if (configuration.IsMultidatabase)
        return new MultidatabaseMappingResolver(configuration, nodeConfiguration);
      if (configuration.IsMultischema)
        return new MultischemaMappingResolver(configuration, nodeConfiguration, providerInfo);
      return new SimpleMappingResolver(providerInfo);
    }
  }
}