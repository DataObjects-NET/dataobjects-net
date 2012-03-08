// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.06

using System;
using System.Linq;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  internal static class ModelMappingBuilder
  {
    public static ModelMapping Build(HandlerAccessor handlers, SqlExtractionResult sqlModel)
    {
      var domainModel = handlers.Domain.Model;
      var configuration = handlers.Domain.Configuration;
      var providerInfo = handlers.ProviderInfo;
      var schemaResolver = handlers.SchemaResolver;

      var mapping = new ModelMapping();

      // Register persistent types

      var typesToProcess = domainModel.Types.AsEnumerable();

      if (configuration.UpgradeMode.IsLegacy())
        typesToProcess = typesToProcess.Where(t => !t.IsSystem);

      foreach (var type in typesToProcess) {
        var primaryIndex = type.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
        if (primaryIndex==null || primaryIndex.IsAbstract)
          continue;
        var reflectedType = primaryIndex.ReflectedType;
        var schema = schemaResolver.ResolveSchema(sqlModel, schemaResolver.GetSchemaName(reflectedType));
        string mappingName = reflectedType.MappingName;
        var storageTable = schema.Tables[mappingName];
        if (storageTable==null)
          throw new DomainBuilderException(String.Format(Strings.ExTableXIsNotFound, mappingName));
        mapping.Register(reflectedType, storageTable);
      }

      // Register key generators

      var keyGenerator = domainModel.Hierarchies
        .Select(h => h.Key.Sequence)
        .Where(s => s!=null)
        .Distinct();

      var nodeResolver = providerInfo.Supports(ProviderFeatures.Sequences)
        ? (Func<ModelMapping, SequenceInfo, Schema, SchemaNode>) GetGeneratorSequence
        : GetGeneratorTable;

      foreach (var sequence in keyGenerator) {
        var schema = schemaResolver.ResolveSchema(sqlModel, schemaResolver.GetSchemaName(sequence));
        var node = nodeResolver.Invoke(mapping, sequence, schema);
        mapping.Register(sequence, node);
      }

      // Fill information for TemporaryTableManager

      var defaultSchemaName = schemaResolver
        .GetSchemaName(configuration.DefaultDatabase, configuration.DefaultSchema);
      var defaultSchema = schemaResolver.ResolveSchema(sqlModel, defaultSchemaName);

      mapping.TemporaryTableDatabase = defaultSchema.Catalog.Name;
      mapping.TemporaryTableSchema = defaultSchema.Name;
      var collation = defaultSchema.Collations.FirstOrDefault();
      mapping.TemporaryTableCollation = collation!=null ? collation.Name : null;

      mapping.Lock();
      return mapping;
    }

    private static SchemaNode GetGeneratorSequence(ModelMapping mapping, SequenceInfo sequenceInfo, Schema schema)
    {
      return schema.Sequences[sequenceInfo.Name];
    }

    private static SchemaNode GetGeneratorTable(ModelMapping mapping, SequenceInfo sequenceInfo, Schema schema)
    {
      return schema.Tables[sequenceInfo.MappingName];
    }
  }
}