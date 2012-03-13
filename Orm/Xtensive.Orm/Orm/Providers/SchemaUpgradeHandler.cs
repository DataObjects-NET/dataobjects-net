// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.06

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Diagnostics;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Building;
using Xtensive.Orm.Providers.Sql;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  [Serializable]
  public class SchemaUpgradeHandler : HandlerBase
  {
    private ProviderInfo providerInfo;
    private SchemaResolver schemaResolver;

    private SessionHandler SessionHandler { get { return BuildingContext.Demand().SystemSessionHandler; } }

    /// <summary>
    /// Gets the target schema.
    /// </summary>
    /// <returns>The target schema.</returns>
    public StorageModel GetTargetSchema()
    {
      var configuration = Handlers.Domain.Configuration;

      var converter = new DomainModelConverter(Handlers, GetEmptyStorageModel()) {
        BuildForeignKeys = configuration.Supports(ForeignKeyMode.Reference),
        BuildHierarchyForeignKeys = configuration.Supports(ForeignKeyMode.Hierarchy),
      };

      return converter.Run();
    }

    /// <summary>
    /// Gets the extracted schema.
    /// This method caches the schema inside <see cref="UpgradeContext"/>.
    /// </summary>
    /// <returns>The extracted schema.</returns>
    public StorageModel GetExtractedSchema()
    {
      var upgradeContext = UpgradeContext.Current;
      if (upgradeContext!=null && upgradeContext.ExtractedModelCache!=null)
        return upgradeContext.ExtractedModelCache;

      var result = ExtractSchema();

      if (upgradeContext!=null)
        upgradeContext.ExtractedModelCache = result;

      return result;
    }

    /// <summary>
    /// Gets the native extracted schema.
    /// This method caches the schema inside <see cref="UpgradeContext"/>.
    /// </summary>
    /// <returns>The native extracted schema.</returns>
    public SqlExtractionResult GetExtractedSqlSchema()
    {
      var upgradeContext = UpgradeContext.Current;
      if (upgradeContext!=null && upgradeContext.ExtractedSqlModelCache!=null)
        return upgradeContext.ExtractedSqlModelCache;

      var schema = ExtractSqlSchema();
      if (upgradeContext!=null)
        upgradeContext.ExtractedSqlModelCache = schema;
      return schema;
    }

    /// <summary>
    /// Clears the extracted schema cache.
    /// </summary>
    public void ClearExtractedSchemaCache()
    {
      var upgradeContext = UpgradeContext.Current;
      if (upgradeContext==null)
        return;
      upgradeContext.ExtractedModelCache = null;
      upgradeContext.ExtractedSqlModelCache = null;
    }

    private StorageModel ExtractSchema()
    {
      var sqlModel = GetExtractedSqlSchema(); // Must rely on this method to avoid multiple extractions
      var converter = new SqlModelConverter(Handlers, sqlModel, GetEmptyStorageModel());
      return converter.Run();
    }

    private SqlExtractionResult ExtractSqlSchema()
    {
      var connection = ((Sql.SessionHandler) SessionHandler).Connection;
      var tasks = schemaResolver.GetExtractionTasks(Handlers.ProviderInfo);
      var sqlModel = Handlers.StorageDriver.Extract(connection, tasks);
      FixSqlSchema(sqlModel);
      return sqlModel;
    }

    private void FixSqlSchema(SqlExtractionResult model)
    {
      bool isSqlServerFamily = providerInfo.ProviderName
        .In(WellKnown.Provider.SqlServer, WellKnown.Provider.SqlServerCe);

      if (!isSqlServerFamily)
        return;

      // This code works for Microsoft SQL Server and Microsoft SQL Server CE

      foreach (var schema in model.Catalogs.SelectMany(c => c.Schemas)) {
        var tables = schema.Tables;
        var sysdiagrams = tables["sysdiagrams"];
        if (sysdiagrams!=null)
          tables.Remove(sysdiagrams);
      }
   }


    /// <summary>
    /// Upgrades the storage.
    /// </summary>
    /// <param name="upgradeActions">The upgrade actions.</param>
    /// <param name="sourceModel">The source schema.</param>
    /// <param name="targetModel">The target schema.</param>
    public void UpgradeSchema(ActionSequence upgradeActions, StorageModel sourceModel, StorageModel targetModel)
    {
      var sqlExecutor = SessionHandler.GetService<ISqlExecutor>();

      var enforceChangedColumns = UpgradeContext.Demand().Hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToList();

      var upgradeContext = UpgradeContext.Current;
      var skipConstraints = upgradeContext!=null && upgradeContext.Stage==UpgradeStage.Upgrading;

      var translator = new SqlActionTranslator(
        Handlers, sqlExecutor,
        upgradeActions, GetExtractedSqlSchema(), sourceModel, targetModel,
        enforceChangedColumns, !skipConstraints);

      var result = translator.Translate();

      LogTranslatedStatements(result);

      if (upgradeContext!=null)
        foreach (var pair in upgradeContext.UpgradeHandlers)
          pair.Value.OnBeforeExecuteActions(result);

      var regularProcessor =
        Handlers.ProviderInfo.Supports(ProviderFeatures.TransactionalDdl)
          ? (Action<IEnumerable<string>>) ExecuteTransactionally
          : ExecuteNonTransactionally;

      result.ProcessWith(regularProcessor, ExecuteNonTransactionally);
    }

    private void ExecuteNonTransactionally(IEnumerable<string> batch)
    {
      var context = UpgradeContext.Demand();

      context.TransactionScope.Complete();
      context.TransactionScope.Dispose();

      SessionHandler.GetService<ISqlExecutor>().ExecuteDdl(batch);

      context.TransactionScope = SessionHandler.Session.OpenTransaction();
    }

    protected virtual void ExecuteTransactionally(IEnumerable<string> batch)
    {
      SessionHandler.GetService<ISqlExecutor>().ExecuteDdl(batch);
    }

    private void LogTranslatedStatements(UpgradeActionSequence sequence)
    {
      if (!Log.IsLogged(LogEventTypes.Info))
        return;

      var commands = sequence.ToArray();
      var session = SessionHandler!=null ? SessionHandler.Session : null;

      Log.Info(Strings.LogSessionXSchemaUpgradeScriptY,
        session.ToStringSafely(),
        Handlers.StorageDriver.BuildBatch(commands).Trim());
    }

    public StorageModel GetEmptyStorageModel()
    {
      var result = new StorageModel();
      foreach (var schema in schemaResolver.GetAllSchemas())
        new SchemaInfo(result, schema);
      return result;
    }

    // Initialization

    public override void Initialize()
    {
      providerInfo = Handlers.ProviderInfo;
      schemaResolver = Handlers.SchemaResolver;
    }
  }
}