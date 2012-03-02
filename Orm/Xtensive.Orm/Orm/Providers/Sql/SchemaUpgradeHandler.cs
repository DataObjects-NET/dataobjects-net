// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Diagnostics;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Orm;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Orm.Building;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  public class SchemaUpgradeHandler : Providers.SchemaUpgradeHandler
  {
    private PartialIndexFilterNormalizer indexFilterNormalizer;

    protected Providers.DomainHandler DomainHandler { get { return Handlers.DomainHandler; } }
    protected SessionHandler SessionHandler { get { return (SessionHandler) BuildingContext.Demand().SystemSessionHandler; } }
    protected StorageDriver Driver { get { return Handlers.StorageDriver; } }

    /// <inheritdoc/>
    public override void UpgradeSchema(ActionSequence upgradeActions, StorageModel sourceSchema, StorageModel targetSchema)
    {
      var sqlExecutor = SessionHandler.GetService<ISqlExecutor>();

      var cachingKeyGeneratorService = (CachingKeyGeneratorService)DomainHandler.Domain.Services.Get<ICachingKeyGeneratorService>();
      var enforceChangedColumns = UpgradeContext.Demand().Hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToList();

      var upgradeContext = UpgradeContext.Current;
      var skipConstraints = upgradeContext!=null && upgradeContext.Stage==UpgradeStage.Upgrading;

      var translator = new SqlActionTranslator(
        upgradeActions,
        (Schema) GetNativeExtractedSchema(),
        sourceSchema, targetSchema, Handlers.ProviderInfo, Driver,
        Handlers.NameBuilder.TypeIdColumnName,
        enforceChangedColumns,
        sqlExecutor,
        cachingKeyGeneratorService.GetCurrentValueImplementation,
        !skipConstraints);

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

    /// <inheritdoc/>
    protected override StorageModel ExtractSchema()
    {
      var schema = (Schema) GetNativeExtractedSchema(); // Must rely on this method to avoid multiple extractions
      var converter = new SqlModelConverter(schema, Handlers.ProviderInfo, indexFilterNormalizer);
      return converter.GetConversionResult();
    }

    /// <inheritdoc/>
    protected override object ExtractNativeSchema()
    {
      var schema = Handlers.StorageDriver.ExtractSchema(SessionHandler.Connection);
      bool isSqlServerFamily = DomainHandler.Domain.Configuration.ConnectionInfo.Provider
        .In(WellKnown.Provider.SqlServer, WellKnown.Provider.SqlServerCe);
      if (isSqlServerFamily) {
        // This code works for Microsoft SQL Server and Microsoft SQL Server CE
        var tables = schema.Tables;
        var sysdiagrams = tables["sysdiagrams"];
        if (sysdiagrams!=null)
          tables.Remove(sysdiagrams);
      }
      return schema;
    }

    /// <inheritdoc/>
    protected override Orm.Upgrade.StorageModelBuilder GetStorageModelBuilder()
    {
      return new StorageModelBuilder(Handlers, indexFilterNormalizer);
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      indexFilterNormalizer = Handlers.Factory.CreateHandler<PartialIndexFilterNormalizer>();
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
        session.ToStringSafely(), Driver.BuildBatch(commands).Trim());
    }
  }
}