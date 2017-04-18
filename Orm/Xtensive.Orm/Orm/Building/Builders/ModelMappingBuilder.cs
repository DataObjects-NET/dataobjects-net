// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.06

using System;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Building.Builders
{
  internal static class ModelMappingBuilder
  {
    public static ModelMapping Build(HandlerAccessor handlers, SchemaExtractionResult sqlModel, MappingResolver resolver, NodeConfiguration nodeConfiguration, bool isLegacy)
    {
      var domainModel = handlers.Domain.Model;
      var configuration = handlers.Domain.Configuration;
      var providerInfo = handlers.ProviderInfo;

      var mapping = new ModelMapping();

      // Register persistent types

      var typesToProcess = domainModel.Types.AsEnumerable();

      if (isLegacy || configuration.IsMultidatabase)
        typesToProcess = typesToProcess.Where(t => !t.IsSystem);

      foreach (var type in typesToProcess) {
        var primaryIndex = type.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
        if (primaryIndex==null || primaryIndex.IsAbstract)
          continue;
        var reflectedType = primaryIndex.ReflectedType;
        var nodeName = resolver.GetNodeName(reflectedType);
        var storageTable = resolver.Resolve(sqlModel, nodeName).GetTable();
        if (storageTable==null)
          throw new DomainBuilderException(String.Format(Strings.ExTableXIsNotFound, nodeName));
        mapping.Register(reflectedType, storageTable);
      }

      // Register key generators

      var keyGenerator = domainModel.Hierarchies
        .Select(h => h.Key.Sequence)
        .Where(s => s!=null)
        .Distinct();

      Func<MappingResolveResult, SchemaNode> nodeResolver;
      
      if (providerInfo.Supports(ProviderFeatures.Sequences))
        nodeResolver = r => r.GetSequence();
      else
        nodeResolver = r => r.GetTable();

      foreach (var sequence in keyGenerator) {
        var nodeResult = resolver.Resolve(sqlModel, resolver.GetNodeName(sequence));
        var node = nodeResolver.Invoke(nodeResult);
        mapping.Register(sequence, node);
      }

      // Fill information for TemporaryTableManager

      var defaultSchema = resolver.ResolveSchema(sqlModel, configuration.DefaultDatabase, configuration.DefaultSchema);
      if (configuration.ShareStorageSchemaOverNodes && handlers.ProviderInfo.Supports(ProviderFeatures.Multischema)) {
        mapping.TemporaryTableDatabase = nodeConfiguration.GetActualNameFor(defaultSchema.Catalog);
        mapping.TemporaryTableSchema = nodeConfiguration.GetActualNameFor(defaultSchema);
      }
      else {
        mapping.TemporaryTableDatabase = defaultSchema.Catalog.GetNameInternal();
        mapping.TemporaryTableSchema = defaultSchema.GetNameInternal();
      }

      if (providerInfo.Supports(ProviderFeatures.Collations))
        if (!string.IsNullOrEmpty(configuration.Collation)) {
          // If user explicitly specified collation use that
          mapping.TemporaryTableCollation = configuration.Collation;
        }
        else {
          // Otherwise use first available collation
          var collation = defaultSchema.Collations.FirstOrDefault();
          mapping.TemporaryTableCollation = collation!=null ? collation.Name : null;
        }

      mapping.Lock();
      return mapping;
    }
  }
}