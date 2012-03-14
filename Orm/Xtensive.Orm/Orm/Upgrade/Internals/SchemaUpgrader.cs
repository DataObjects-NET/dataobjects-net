// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Sql;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  internal sealed class SchemaUpgrader : IDisposable
  {
    private readonly Session session;
    private readonly UpgradeContext context;
    private readonly ProviderInfo providerInfo;
    private readonly SchemaResolver schemaResolver;
    private readonly StorageDriver driver;
    private readonly UpgradeServiceAccessor services;

    private TransactionScope transactionScope;

    /// <summary>
    /// Gets the extracted schema.
    /// This method caches the schema inside <see cref="UpgradeContext"/>.
    /// </summary>
    /// <returns>The extracted schema.</returns>
    public StorageModel GetExtractedSchema()
    {
      if (context.ExtractedModelCache!=null)
        return context.ExtractedModelCache;

      var result = ExtractSchema();
      context.ExtractedModelCache = result;
      return result;
    }

    /// <summary>
    /// Gets the native extracted schema.
    /// This method caches the schema inside <see cref="UpgradeContext"/>.
    /// </summary>
    /// <returns>The native extracted schema.</returns>
    public SqlExtractionResult GetExtractedSqlSchema()
    {
      if (context.ExtractedSqlModelCache!=null)
        return context.ExtractedSqlModelCache;

      var schema = ExtractSqlSchema();
      context.ExtractedSqlModelCache = schema;
      return schema;
    }

    /// <summary>
    /// Clears the extracted schema cache.
    /// </summary>
    public void ClearExtractedSchemaCache()
    {
      context.ExtractedModelCache = null;
      context.ExtractedSqlModelCache = null;
    }

    private StorageModel ExtractSchema()
    {
      var schema = GetExtractedSqlSchema(); // Must rely on this method to avoid multiple extractions
      return new SqlModelConverter(services, schema).Run();
    }

    private SqlExtractionResult ExtractSqlSchema()
    {
      var tasks = schemaResolver.GetExtractionTasks(providerInfo);
      return session.Handler.GetService<ISqlExecutor>().Extract(tasks);
    }

    public void UpgradeSchema(ActionSequence upgradeActions, StorageModel sourceModel, StorageModel targetModel)
    {
      var executor = session.Handler.GetService<ISqlExecutor>();

      var enforceChangedColumns = context.Hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToList();

      var skipConstraints = context.Stage==UpgradeStage.Upgrading;

      var translator = new SqlActionTranslator(
        session.Handlers, executor,
        upgradeActions, GetExtractedSqlSchema(), sourceModel, targetModel,
        enforceChangedColumns, !skipConstraints);

      var result = translator.Translate();

      if (Providers.Log.IsLogged(LogEventTypes.Info))
        LogStatements(result);

      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnBeforeExecuteActions(result);

      var regularProcessor =
        providerInfo.Supports(ProviderFeatures.TransactionalDdl)
          ? (Action<IEnumerable<string>>) ExecuteTransactionally
          : ExecuteNonTransactionally;

      result.ProcessWith(regularProcessor, ExecuteNonTransactionally);
    }

    private void ExecuteNonTransactionally(IEnumerable<string> batch)
    {
      CommitTransaction();

      session.Handler.GetService<ISqlExecutor>().ExecuteDdl(batch);

      BeginTransaction();
    }

    private void ExecuteTransactionally(IEnumerable<string> batch)
    {
      session.Handler.GetService<ISqlExecutor>().ExecuteDdl(batch);
    }

    private void LogStatements(IEnumerable<string> statements)
    {
      Providers.Log.Info(
        Strings.LogSessionXSchemaUpgradeScriptY,
        session.ToStringSafely(),
        driver.BuildBatch(statements.ToArray()).Trim());
    }

    public void Dispose()
    {
      CommitTransaction();
    }

    private void BeginTransaction()
    {
      transactionScope = session.OpenTransaction();
    }

    private void CommitTransaction()
    {
      if (transactionScope==null)
        return;
      try {
        transactionScope.Complete();
      }
      finally {
        transactionScope.Dispose();
        transactionScope = null;
      }
    }

    // Constructors

    public SchemaUpgrader(UpgradeContext context, Session session)
    {
      this.context = context;
      this.session = session;

      services = context.Services;
      driver = services.Driver;
      schemaResolver = services.SchemaResolver;
      providerInfo = services.ProviderInfo;

      BeginTransaction();
    }

  }
}