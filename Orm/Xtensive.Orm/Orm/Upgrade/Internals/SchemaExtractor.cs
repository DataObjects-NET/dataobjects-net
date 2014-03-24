// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.16

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Sql;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class SchemaExtractor
  {
    private readonly UpgradeContext context;
    private readonly UpgradeServiceAccessor services;
    private readonly ISqlExecutor executor;

    public StorageModel GetSchema()
    {
      if (context.ExtractedModelCache!=null)
        return context.ExtractedModelCache;

      var converter = new SqlModelConverter(services, GetSqlSchema(), GetPartialIndexes());
      var result =  converter.Run();
      context.ExtractedModelCache = result;
      return result;
    }

    public SchemaExtractionResult GetSqlSchema()
    {
      if (context.ExtractedSqlModelCache!=null)
        return context.ExtractedSqlModelCache;

      var schema = new SchemaExtractionResult(executor.Extract(services.MappingResolver.GetSchemaTasks()));
      var handledSchema = new IgnoreRulesHandler(schema, services.Configuration, services.MappingResolver).Handle();
      context.ExtractedSqlModelCache = handledSchema;
      return handledSchema;
    }

    public void ClearCache()
    {
      context.ExtractedModelCache = null;
      context.ExtractedSqlModelCache = null;
    }

    private IEnumerable<StoredPartialIndexFilterInfo> GetPartialIndexes()
    {
      if (!services.ProviderInfo.Supports(ProviderFeatures.PartialIndexes))
        return Enumerable.Empty<StoredPartialIndexFilterInfo>();
      var metadata = context.Metadata;
      if (metadata==null)
        return Enumerable.Empty<StoredPartialIndexFilterInfo>();
      var extensions = metadata.Extensions.Where(e => e.Name==WellKnown.PartialIndexDefinitionsExtensionName);
      var result = new List<StoredPartialIndexFilterInfo>();
      foreach (var extension in extensions) {
        var items = StoredPartialIndexFilterInfoCollection.Deserialize(extension.Value).Items;
        if (items!=null)
          result.AddRange(items);
      }
      return result;
    }

    // Constructors

    public SchemaExtractor(UpgradeContext context, Session session)
    {
      this.context = context;

      services = context.Services;
      executor = session.Services.Demand<ISqlExecutor>();
    }
  }
}