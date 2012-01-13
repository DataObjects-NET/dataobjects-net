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
using Xtensive.Orm.Providers.Sql.Resources;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Orm.Building;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Upgrade;
using ModelTypeInfo = Xtensive.Orm.Upgrade.Model.TypeInfo;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  public class SchemaUpgradeHandler : Providers.SchemaUpgradeHandler
  {
    private PartialIndexFilterNormalizer indexFilterNormalizer;

    protected DomainHandler DomainHandler { get { return (DomainHandler) Handlers.DomainHandler; } }
    protected SessionHandler SessionHandler { get { return (SessionHandler) BuildingContext.Demand().SystemSessionHandler; } }
    protected Driver Driver { get { return DomainHandler.Driver; } }

    /// <inheritdoc/>
    public override void UpgradeSchema(ActionSequence upgradeActions, StorageInfo sourceSchema, StorageInfo targetSchema)
    {
      var queryExecutor = SessionHandler.GetService<IQueryExecutor>(true);

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
        sourceSchema, targetSchema, DomainHandler.ProviderInfo, Driver,
        Handlers.NameBuilder.TypeIdColumnName,
        enforceChangedColumns,
        queryExecutor.ExecuteScalar,
        queryExecutor.ExecuteNonQuery,
        cachingKeyGeneratorService.GetCurrentValueImplementation,
        !skipConstraints);

      var result = translator.Translate();

      LogTranslatedStatements(result);

      if (upgradeContext!=null)
        foreach (var pair in upgradeContext.UpgradeHandlers)
          pair.Value.OnBeforeExecuteActions(result);

      result.ProcessWith(Execute, ExecuteNonTransactionally);
    }

    /// <inheritdoc/>
    protected override StorageInfo ExtractSchema()
    {
      var schema = (Schema) GetNativeExtractedSchema(); // Must rely on this method to avoid multiple extractions
      var converter = new SqlModelConverter(schema, DomainHandler.ProviderInfo, indexFilterNormalizer);
      return converter.GetConversionResult();
    }

    /// <inheritdoc/>
    protected override object ExtractNativeSchema()
    {
      return DomainHandler.Driver.ExtractSchema(SessionHandler.Connection);
    }

    /// <inheritdoc/>
    protected override Orm.Upgrade.StorageModelBuilder GetStorageModelBuilder()
    {
      return new StorageModelBuilder(DomainHandler, indexFilterNormalizer);
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      indexFilterNormalizer = Handlers.HandlerFactory.CreateHandler<PartialIndexFilterNormalizer>();
    }

    private void ExecuteNonTransactionally(IEnumerable<string> batch)
    {
      var context = UpgradeContext.Demand();
      context.TransactionScope.Complete();
      context.TransactionScope.Dispose();
      Execute(batch);
      context.TransactionScope = SessionHandler.Session.OpenTransaction();
    }

    protected virtual void Execute(IEnumerable<string> batch)
    {
      if (DomainHandler.ProviderInfo.Supports(ProviderFeatures.DdlBatches)) {
        IEnumerable<IEnumerable<string>> subbatches = SplitToSubbatches(batch);
        foreach (var subbatch in subbatches) {
          var commandText = Driver.BuildBatch(subbatch.ToArray());
          if (string.IsNullOrEmpty(commandText))
            return;
          var command = SessionHandler.Connection.CreateCommand(commandText);
          using (command) {
            Driver.ExecuteNonQuery(null, command);
          }
        }
      }
      else {
        foreach (var commandText in batch) {
          if (string.IsNullOrEmpty(commandText))
            continue;
          var command = SessionHandler.Connection.CreateCommand(commandText);
          using (command) {
            Driver.ExecuteNonQuery(null, command);
          }
        }
      }
    }

    private static IEnumerable<IEnumerable<string>> SplitToSubbatches(IEnumerable<string> batch)
    {
      var subbatch = new List<string>();
      foreach (string item in batch) {
        if (item.IsNullOrEmpty()) {
          if (subbatch.Count==0)
            continue;
          yield return subbatch;
          subbatch = new List<string>();
        }
        else {
          subbatch.Add(item);
        }
      }
      if (subbatch.Count!=0)
        yield return subbatch;
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