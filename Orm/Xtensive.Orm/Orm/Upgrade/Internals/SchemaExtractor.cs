// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.03.16

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    public async Task<StorageModel> GetSchemaAsync(CancellationToken token)
    {
      if (context.ExtractedModelCache!=null)
        return context.ExtractedModelCache;

      var schemaExtractionResult = await GetSqlSchemaAsync(token).ConfigureAwaitFalse();
      var converter = new SqlModelConverter(services, schemaExtractionResult, GetPartialIndexes());
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

    public async Task<SchemaExtractionResult> GetSqlSchemaAsync(CancellationToken token)
    {
      if (context.ExtractedSqlModelCache!=null) {
        return context.ExtractedSqlModelCache;
      }

      var sqlExtractionTasks = services.MappingResolver.GetSchemaTasks();
      var sqlExtractionResult = await executor.ExtractAsync(sqlExtractionTasks, token).ConfigureAwaitFalse();
      var schema = new SchemaExtractionResult(sqlExtractionResult);
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