// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.16

using Xtensive.Core;
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

      var result = ExtractSchema();
      context.ExtractedModelCache = result;
      return result;
    }

    public SqlExtractionResult GetSqlSchema()
    {
      if (context.ExtractedSqlModelCache!=null)
        return context.ExtractedSqlModelCache;

      var schema = ExtractSqlSchema();
      context.ExtractedSqlModelCache = schema;
      return schema;
    }

    public void ClearCache()
    {
      context.ExtractedModelCache = null;
      context.ExtractedSqlModelCache = null;
    }

    #region Private / internal methods

    private StorageModel ExtractSchema()
    {
      var schema = GetSqlSchema(); // Must rely on this method to avoid multiple extractions
      return new SqlModelConverter(services, schema).Run();
    }

    private SqlExtractionResult ExtractSqlSchema()
    {
      return executor.Extract(services.Resolver.GetExtractionTasks(services.ProviderInfo));
    }

    #endregion

    // Constructors

    public SchemaExtractor(UpgradeContext context, Session session)
    {
      this.context = context;
      services = context.Services;
      executor = session.Services.Demand<ISqlExecutor>();
    }
  }
}